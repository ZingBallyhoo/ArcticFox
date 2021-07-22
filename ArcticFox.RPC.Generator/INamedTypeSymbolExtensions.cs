// see https://github.com/dotnet/roslyn/issues/1891
// see https://github.com/GuOrg/Gu.Roslyn.Extensions/issues/30
// todo: why can't add reference to this...
/*
MIT License

Copyright (c) 2018 Johan Larsson

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/


using System.Text;

namespace Gu.Roslyn.AnalyzerExtensions
{
    using System;
    using Microsoft.CodeAnalysis;

    /// <summary>
    /// Helpers for <see cref="INamedTypeSymbol"/>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class INamedTypeSymbolExtensions
    {
        private static readonly SymbolDisplayFormat Simple = new SymbolDisplayFormat(
            SymbolDisplayGlobalNamespaceStyle.Omitted,
            SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable);

        /// <summary>
        /// Returns what System.Type.FullName returns.
        /// </summary>
        /// <param name="type">The <see cref="INamedTypeSymbol"/>.</param>
        /// <returns>What System.Type.FullName returns.</returns>
        public static string FullName(this INamedTypeSymbol type)
        {
            var builder = new StringBuilder();
            var previous = default(SymbolDisplayPart);
            foreach (var part in type.ToDisplayParts(Simple))
            {
                switch (part.Kind)
                {
                    case SymbolDisplayPartKind.ClassName:
                    case SymbolDisplayPartKind.InterfaceName:
                    case SymbolDisplayPartKind.StructName:
                    case SymbolDisplayPartKind.NamespaceName:
                        builder.Append(part.Symbol.MetadataName);
                        break;
                    case SymbolDisplayPartKind.Punctuation when part.ToString() == ".":
                        builder.Append(previous.Symbol == null || previous.Symbol.Kind == SymbolKind.Namespace ? "." : "+");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (part.Symbol != null)
                {
                    previous = part;
                }
            }

            if (!SymbolEqualityComparer.Default.Equals(type.ConstructedFrom, type))
            {
                builder.Append("[");
                for (var i = 0; i < type.TypeArguments.Length; i++)
                {
                    var argument = type.TypeArguments[i];
                    if (i > 0)
                    {
                        builder.Append(",");
                    }

                    builder.Append("[");
                    if (argument is INamedTypeSymbol argType)
                    {
                        builder.Append(FullName(argType))
                               .Append(", ").Append(argType.ContainingAssembly.Identity.ToString());
                    }

                    builder.Append("]");
                }

                builder.Append("]");
            }

            return builder.ToString();
        }
    }
}