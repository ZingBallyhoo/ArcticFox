using ArcticFox.RPC.Parameters;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Methods
{
    public class RpcMethodVisitor<TMethod> : TypeShapeVisitor where TMethod : IRpcMethod, new()
    {
        public override object? VisitMethod<TDeclaringType, TArgumentState, TResult>(IMethodShape<TDeclaringType, TArgumentState, TResult> methodShape, object? state = null)
        {
            return new TMethod
            {
                FromShape = methodShape,
                Parameters = (IRpcParameter[])state!
            };
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            return new TMethod
            {
                FromShape = propertyShape,
                Parameters = (IRpcParameter[])state!
            };
        }
    }
}