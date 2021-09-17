using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gu.Roslyn.AnalyzerExtensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ArcticFox.RPC.Generator
{
    [Generator]
    public class RpcGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            try
            {
                ExecuteInternal(context);
            } catch (Exception e)
            {
                var descriptor = new DiagnosticDescriptor(nameof(RpcGenerator), "Error", e.ToString(), "Error", DiagnosticSeverity.Error, true);
                var diagnostic = Diagnostic.Create(descriptor, Location.None);
                context.ReportDiagnostic(diagnostic);
            }
        }
        
        private class ClassGenInfo
        {
            public readonly INamedTypeSymbol m_symbol;
            public readonly List<MethodGenInfo> m_methods = new List<MethodGenInfo>();

            public ClassGenInfo(INamedTypeSymbol symbol)
            {
                m_symbol = symbol;
            }
        }

        private class MethodGenInfo
        {
            public readonly INamedTypeSymbol m_type;
            public readonly string m_name;

            public INamedTypeSymbol m_requestType;
            public INamedTypeSymbol? m_responseType;

            public MethodGenInfo(INamedTypeSymbol type, string name)
            {
                m_type = type;
                m_name = name;

                var currentType = type;
                while (currentType != null)
                {
                    if (currentType.Name == "RpcMethod")
                    {
                        m_requestType = (INamedTypeSymbol)currentType.TypeArguments[0];
                        if (currentType.TypeArguments.Length > 1) m_responseType = (INamedTypeSymbol)currentType.TypeArguments[1];
                        break;
                    }

                    currentType = currentType.BaseType;
                }

                if (m_requestType == null)
                {
                    throw new Exception($"unable to find RpcMethod base in {type}");
                }
            }

            public string GetDispatchName()
            {
                if (m_type.ToDisplayString().Contains("TokenPassthrough"))
                {
                    return $"\"{m_requestType.FullName()}\"";
                } else
                {
                    return $"\"{m_name}\"";
                }
            }
        }

        private static void ExecuteInternal(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is SyntaxReceiver receiver))
                return;
            
            var compilation = context.Compilation;
            
            var methodAttributeSymbol = compilation.GetTypeByMetadataName("ArcticFox.RPC.RpcMethodAttribute");

            Dictionary<INamedTypeSymbol, ClassGenInfo> classes = new Dictionary<INamedTypeSymbol, ClassGenInfo>(SymbolEqualityComparer.Default);
            
            foreach (var classDeclaration in receiver.m_candidateClasses)
            {
                var model = compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                var classSymbol = (INamedTypeSymbol)ModelExtensions.GetDeclaredSymbol(model, classDeclaration)!;
                foreach (var attribute in classSymbol.GetAttributes())
                {
                    if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass, methodAttributeSymbol)) continue;

                    var rpcMethodType = (INamedTypeSymbol) attribute.ConstructorArguments[0].Value!;
                    var rpcMethodName = (string) attribute.ConstructorArguments[1].Value!;

                    if (!classes.TryGetValue(classSymbol, out var classGenInfo))
                    {
                        classGenInfo = new ClassGenInfo(classSymbol);
                        classes[classSymbol] = classGenInfo;
                    }
                    
                    classGenInfo.m_methods.Add(new MethodGenInfo(rpcMethodType, rpcMethodName));
                }
            }

            foreach (var info in classes)
            {
                var classSource = ProcessClass(info.Value);
                context.AddSource($"{nameof(RpcGenerator)}_{info.Value.m_symbol.Name}.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }
        
        private static string ProcessClass(ClassGenInfo classGenInfo)
        {
            var writer = new IndentedTextWriter(new StringWriter(), "    ");
            
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Threading.Tasks;");
            writer.WriteLine("using ArcticFox.RPC;");
            writer.WriteLine();
            
            var scope = new NestedScope(classGenInfo.m_symbol);
            scope.Start(writer);
            
            writer.WriteLine(NestedScope.GetClsString(classGenInfo.m_symbol));
            writer.WriteLine("{");
            writer.Indent++;
            foreach (var method in classGenInfo.m_methods)
            {
                writer.WriteLine($"public static readonly {method.m_type} {method.m_name}Method = new (nameof({classGenInfo.m_symbol}), {method.GetDispatchName()});");
            }
            writer.Indent--;
            writer.WriteLine("}");
            
            GenerateGenericClass(classGenInfo, writer);
            GenerateRemoteClass(classGenInfo, writer);

            scope.End(writer);
            
            return writer.InnerWriter.ToString();
        }

        private static void GenerateGenericClass(ClassGenInfo classGenInfo, IndentedTextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine($"public abstract class {classGenInfo.m_symbol.Name}<T> : {classGenInfo.m_symbol.Name}, IService<T>");
            writer.WriteLine("{");
            writer.Indent++;
            GenerateAbstractMethods(classGenInfo, writer);
            GenerateTypelessMethods(classGenInfo, writer);
            GenerateMethodHandler(classGenInfo, writer);
            writer.Indent--;
            writer.WriteLine("}");
        }

        private static void GenerateRemoteClass(ClassGenInfo classGenInfo, IndentedTextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine($"public class {classGenInfo.m_symbol.Name}_Remote : {classGenInfo.m_symbol.Name}<IRpcSocket>");
            writer.WriteLine("{");
            writer.Indent++;
            
            writer.WriteLine($"public static readonly {classGenInfo.m_symbol.Name}_Remote Instance = new();");
            writer.WriteLine();
            
            foreach (var method in classGenInfo.m_methods)
            {
                writer.Write("public override ValueTask");
                if (method.m_responseType != null)
                {
                    writer.Write($"<{method.m_responseType}>");
                }
                writer.Write($" {method.m_name}(IRpcSocket socket, {method.m_requestType} request) => {method.m_name}Method.Call");
                if (method.m_responseType == null) writer.Write("Async");
                writer.WriteLine("(socket, request);");
            }

            writer.Indent--;
            writer.WriteLine("}");
        }

        private static void GenerateAbstractMethods(ClassGenInfo classGenInfo, IndentedTextWriter writer)
        {
            foreach (var method in classGenInfo.m_methods)
            {
                writer.Write("public abstract ValueTask");
                if (method.m_responseType != null)
                {
                    writer.Write($"<{method.m_responseType}>");
                }
                writer.WriteLine($" {method.m_name}(T socket, {method.m_requestType} request);");
            }
        }

        private static void GenerateTypelessMethods(ClassGenInfo classGenInfo, IndentedTextWriter writer)
        {
            writer.WriteLine();
            foreach (var method in classGenInfo.m_methods)
            {
                writer.Write($"private async ValueTask<object?> __{method.m_name}Typeless(T socket, {method.m_requestType} request) ");
                if (method.m_responseType != null)
                {
                    writer.WriteLine($"=> await {method.m_name}(socket, request);");
                } else
                {
                    writer.WriteLine($"{{ await {method.m_name}(socket, request); return null; }}");
                }
            }
        }

        private static void GenerateMethodHandler(ClassGenInfo classGenInfo, IndentedTextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("public ValueTask<object?> InvokeMethodHandler(T socket, string method, ReadOnlySpan<byte> data, object? token)");
            writer.WriteLine("{");
            writer.Indent++;
            writer.WriteLine("switch (method)");
            writer.WriteLine("{");
            writer.Indent++;
            
            foreach (var method in classGenInfo.m_methods)
            {
                writer.WriteLine($"case {method.GetDispatchName()}: return __{method.m_name}Typeless(socket, {method.m_name}Method.DecodeRequest(data, token));");
            }
            writer.WriteLine("default: throw new Exception($\"unknown method {method}\");");
            
            writer.Indent--;
            writer.WriteLine("}");
            writer.Indent--;
            writer.WriteLine("}");
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public readonly List<ClassDeclarationSyntax> m_candidateClasses = new List<ClassDeclarationSyntax>();
            
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
                {
                    m_candidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}