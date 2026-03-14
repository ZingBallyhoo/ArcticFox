namespace ArcticFox.RPC.Methods
{
    public interface IRpcMethodWithName
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
    }
}