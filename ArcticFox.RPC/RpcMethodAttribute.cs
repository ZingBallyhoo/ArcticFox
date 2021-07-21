using System;

namespace ArcticFox.RPC
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class RpcMethodAttribute : Attribute
    {
        public readonly Type m_type;
        public readonly string m_name;
        
        public RpcMethodAttribute(Type type, string name)
        {
            m_type = type;
            m_name = name;
        }
    }
}