using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ArcticFox.RPC.Methods;
using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{    
    public class RpcImplementationVisitor<TCallContext> : TypeShapeVisitor
    {
        public override object? VisitMethod<TDeclaringType, TArgumentState, TResult>(IMethodShape<TDeclaringType, TArgumentState, TResult> methodShape, object? state = null)
        {
            var rpcMethod = (IRpcMethod)state!;
            var rpcParameters = (RpcParameter<TCallContext, TArgumentState>[])rpcMethod.Parameters;
            
            var argumentStateCtor = methodShape.GetArgumentStateConstructor();
            var invoker = methodShape.GetMethodInvoker();
            
            var boundMethod = (IRpcMethodInvokable<TCallContext>)state!;
            boundMethod.Invoke = async (target, callContext) =>
            {
                var argumentState = argumentStateCtor();
                foreach (var parameter in rpcParameters)
                {
                    parameter.FromContext(ref argumentState, ref callContext);
                }

                if (!argumentState.AreRequiredArgumentsSet)
                {
                    ThrowMissingRequiredArguments(ref argumentState, methodShape.Parameters);
                }

                var typedTarget = new StrongBox<TDeclaringType>((TDeclaringType)target!);
                var typedResult = await invoker(ref typedTarget.Value!, ref argumentState).ConfigureAwait(false);

                return typedResult;
            };
            
            return boundMethod;
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var funcInvoker = (Func<object, TCallContext, Task<object?>>)propertyShape.PropertyType.Accept(this, state)!;
            var propertyGetter = propertyShape.GetGetter();
            
            var boundMethod = (IRpcMethodInvokable<TCallContext>)state!;
            boundMethod.Invoke = (target, context) =>
            {
                var typedTarget = new StrongBox<TDeclaringType>((TDeclaringType)target!);
                var func = propertyGetter.Invoke(ref typedTarget.Value!)!;

                return funcInvoker(func, context);
            };
            
            return boundMethod;
        }

        public override object? VisitFunction<TFunction, TArgumentState, TResult>(IFunctionTypeShape<TFunction, TArgumentState, TResult> functionShape, object? state = null)
        {
            var rpcMethod = (IRpcMethod)state!;
            var rpcParameters = (RpcParameter<TCallContext, TArgumentState>[])rpcMethod.Parameters;
            
            var argumentStateCtor = functionShape.GetArgumentStateConstructor();
            var invoker = functionShape.GetFunctionInvoker();

            return (Func<object, TCallContext, Task<object?>>)(async (functionRaw, callContext) =>
            {
                var argumentState = argumentStateCtor();
                foreach (var parameter in rpcParameters)
                {
                    parameter.FromContext(ref argumentState, ref callContext);
                }

                if (!argumentState.AreRequiredArgumentsSet)
                {
                    ThrowMissingRequiredArguments(ref argumentState, functionShape.Parameters);
                }

                var typedFunction = (TFunction)functionRaw;
                var typedResult = await invoker(ref typedFunction, ref argumentState).ConfigureAwait(false);

                return typedResult;
            });
        }
        
        private static void ThrowMissingRequiredArguments<TArgumentState>(ref TArgumentState argumentState, IReadOnlyList<IParameterShape> parameters) where TArgumentState : IArgumentState
        {
            Debug.Assert(!argumentState.AreRequiredArgumentsSet);
            
            var missingRequiredParams = new List<string>();
            foreach (var parameter in parameters)
            {
                if (parameter.IsRequired && !argumentState.IsArgumentSet(parameter.Position))
                {
                    missingRequiredParams.Add($"'{parameter.Name}'");
                }
            }

            throw new KeyNotFoundException($"Missing required parameters: {string.Join(", ", missingRequiredParams)}");
        }
    }
}