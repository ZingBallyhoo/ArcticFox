using System.Collections.Generic;
using PolyType;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Binding
{
    public class RpcCollectMethodsVisitor<TMethod> : TypeShapeVisitor
    {
        private readonly TypeShapeVisitor[] m_visitors;
        
        public RpcCollectMethodsVisitor(params TypeShapeVisitor[] visitors)
        {
            m_visitors = visitors;
        }
        
        public override object? VisitObject<T>(IObjectTypeShape<T> objectShape, object? state = null)
        {
            var methods = new List<TMethod>();
            foreach (var method in objectShape.Methods)
            {
                var methodFromMethod = (TMethod?)method.Accept(this, state);
                if (methodFromMethod != null)
                {
                    methods.Add(methodFromMethod);
                }
            }
            foreach (var property in objectShape.Properties)
            {
                var methodFromProperty = (TMethod?)property.Accept(this, state);
                if (methodFromProperty != null)
                {
                    methods.Add(methodFromProperty);
                }
            }
            return methods.ToArray();
        }

        public override object? VisitMethod<TDeclaringType, TArgumentState, TResult>(IMethodShape<TDeclaringType, TArgumentState, TResult> methodShape, object? state = null)
        {
            foreach (var visitor in m_visitors)
            {
                state = methodShape.Accept(visitor, state);
                if (state == null) return null;
            }

            return state;
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            if (propertyShape.PropertyType.Kind != TypeShapeKind.Function)
            {
                return null;
            }
            
            foreach (var visitor in m_visitors)
            {
                state = propertyShape.Accept(visitor, state);
                if (state == null) return null;
            }

            return state;
        }
    }
}