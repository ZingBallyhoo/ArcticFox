using System;
using System.Linq;
using PolyType;
using PolyType.Abstractions;

namespace ArcticFox.RPC.Parameters
{
    public class RpcParameterVisitor<TCallContext> : TypeShapeVisitor
    {
        private readonly IRpcParameterBinder<TCallContext> m_parameterBinder;
        
        public RpcParameterVisitor(IRpcParameterBinder<TCallContext> parameterBinder)
        {
            m_parameterBinder = parameterBinder;
        }

        public override object? VisitMethod<TDeclaringType, TArgumentState, TResult>(IMethodShape<TDeclaringType, TArgumentState, TResult> methodShape, object? state = null)
        {
            return methodShape.Parameters.Select(x => x.Accept(this, state)).Cast<RpcParameter<TCallContext, TArgumentState>>().ToArray();
        }

        public override object? VisitProperty<TDeclaringType, TPropertyType>(IPropertyShape<TDeclaringType, TPropertyType> propertyShape, object? state = null)
        {
            if (propertyShape.PropertyType.Kind != TypeShapeKind.Function)
            {
                return null;
            }
            return propertyShape.PropertyType.Accept(this, state);
        }

        public override object? VisitFunction<TFunction, TArgumentState, TResult>(IFunctionTypeShape<TFunction, TArgumentState, TResult> functionShape, object? state = null)
        {
            return functionShape.Parameters.Select(x => x.Accept(this, state)).Cast<RpcParameter<TCallContext, TArgumentState>>().ToArray();
        }

        public override object? VisitParameter<TArgumentState, TParameterType>(IParameterShape<TArgumentState, TParameterType> parameterShape, object? state = null)
        {
            var implicitParameter = m_parameterBinder.VisitImplicitParameter(parameterShape, state);
            if (implicitParameter != null)
            {
                return implicitParameter;
            }
            
            var explicitParameter = m_parameterBinder.VisitExplicitParameter(parameterShape, state);
            if (explicitParameter != null)
            {
                return explicitParameter;
            }

            throw new ArgumentException($"don't know how to bind parameter: {parameterShape}");
        }
    }
}