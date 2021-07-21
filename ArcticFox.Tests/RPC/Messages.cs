namespace ArcticFox.Tests.RPC
{
    public interface ITestRpcMessage
    {
        public uint GetID();
    }
    
    public class Request1 : ITestRpcMessage
    {
        public uint GetID() => 1;
    }

    public class Request2 : ITestRpcMessage
    {
        public uint GetID() => 2;
    }
    
    public class Request3 : ITestRpcMessage
    {
        public uint GetID() => 3;
    }
    
    public class Request4 : ITestRpcMessage
    {
        public uint GetID() => 4;
    }

    public class Response1 : ITestRpcMessage
    {
        public uint GetID() => 10001;
    }
    
    public class Response2 : ITestRpcMessage
    {
        public uint GetID() => 10002;
    }
    
    public class Response4 : ITestRpcMessage
    {
        public uint GetID() => 10004;
    }
    
    public class ErrorResponse : ITestRpcMessage
    {
        public uint GetID() => 20000;
    }
}