using System;
using System.IO;
using System.Runtime.CompilerServices;
using ArcticFox.RPC.Methods;
using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{
    public class RpcFakeImplementationVisitor<TMethod, TCallContext> : TypeShapeVisitor
    {
        private readonly IRpcFakeImplementationBinder<TMethod, TCallContext> m_binder;
        
        public RpcFakeImplementationVisitor(IRpcFakeImplementationBinder<TMethod, TCallContext> binder)
        {
            m_binder = binder;
        }
        
        public override object? VisitObject<T>(IObjectTypeShape<T> objectShape, object? state = null)
        {
            var defaultConstructor = objectShape.GetDefaultConstructor();
            if (defaultConstructor == null)
            {
                throw new InvalidDataException($"unable to find default constructor for {typeof(T)}");
            }

            var instance = defaultConstructor()!;
            
            var rpcMethods = (IRpcMethod[])state!;
            foreach (var method in rpcMethods)
            {
                if (method.FromShape is IPropertyShape propertyShape)
                {
                    var propSetter = (Action<object>)propertyShape.Accept(this, method)!;
                    propSetter(instance);
                }
            }

            return instance;
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var func = propertyShape.PropertyType.Accept(this, state);

            var setter = propertyShape.GetSetter();
            return (object o) =>
            {
                var typedInstance = new StrongBox<TDeclaringType>((TDeclaringType)o);
                setter.Invoke(ref typedInstance.Value!, (TPropertyType)func!);
            };
        }

        public override object? VisitFunction<TFunction, TArgumentState, TResult>(IFunctionTypeShape<TFunction, TArgumentState, TResult> functionShape, object? state = null)
        {
            var rpcMethod = (IRpcMethod)state!;
            return m_binder.BindFunction(functionShape, (TMethod)state!, (RpcParameter<TCallContext, TArgumentState>[])rpcMethod.Parameters);
        }
    }
}