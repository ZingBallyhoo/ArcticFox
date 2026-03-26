using PolyType.Abstractions;

namespace ArcticFox.RPC.Methods
{
    public class RpcCanonNameVisitor<TMethod> : TypeShapeVisitor where TMethod : IRpcMethodWithName
    {
        public override object? VisitMethod<TDeclaringType, TArgumentState, TResult>(IMethodShape<TDeclaringType, TArgumentState, TResult> methodShape, object? state = null)
        {
            var method = (TMethod)state!;
            method.MethodName = methodShape.Name;

            return method;
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            var method = (TMethod)state!;
            method.MethodName = propertyShape.Name;

            return method;
        }
    }
}