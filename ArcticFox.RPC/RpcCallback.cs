using System;
using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public class RpcCallback
    {
        private readonly IResponseDecoder m_decoder;
        private readonly TaskCompletionSource<object>? m_tcs;
        private readonly Func<object, ValueTask>? m_func;
        
        public RpcCallback(IResponseDecoder decoder, Func<object, ValueTask>? func)
        {
            m_decoder = decoder;
            m_func = func;
        }
        
        public RpcCallback(IResponseDecoder decoder, TaskCompletionSource<object>? tcs=null)
        {
            m_decoder = decoder;
            m_tcs = tcs;
        }

        public ValueTask Process(ReadOnlySpan<byte> data, object? token)
        {
            var message = m_decoder.DecodeResponse(data, token);
            
            if (m_func != null) return m_func(message);
            
            if (m_tcs != null) m_tcs.TrySetResult(message);
            
            return ValueTask.CompletedTask;
        }

        public void SetException(Exception exception)
        {
            m_tcs?.TrySetException(exception);
        }

        public void Cancel()
        {
            m_tcs?.TrySetCanceled();
        }
    }
}