using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Codec.Binary;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;
using ArcticFox.RPC;

namespace ArcticFox.Tests.RPC
{
    public class MyRpcClientSocket : RpcSocketCommon<uint>, ISpanConsumer<byte>
    {
        private readonly IDFactory m_callbackIDFactory;
        
        public MyRpcClientSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain().AddCodec(this);
            m_callbackIDFactory = new IDFactory();
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            var reader = new BitReader(input);

            while (reader.m_dataOffset < reader.m_dataLength)
            {
                var methodID = reader.ReadUInt32LittleEndian();
                var requestID = reader.ReadUInt32LittleEndian();

                ITestRpcMessage response = methodID switch
                {
                    10001 => new Response1(),
                    10002 => new Response2(),
                    10004 => new Response4(),
                    20000 => new ErrorResponse(),
                    _ => throw new InvalidDataException($"unknown message {methodID}")
                };
                
                if (response is ErrorResponse)
                {
                    if (TryRemoveCallback(requestID, out var callback))
                    {
                        callback.SetException(new Exception());
                    }
                    return;
                }
                
                ProcessCallback(requestID, ReadOnlySpan<byte>.Empty, response);
            }
        }

        public override async ValueTask CallRemoteAsync<T>(RpcMethod method, T request, RpcCallback? callback, CancellationToken cancellationToken = default) where T : class
        {
            var requestTyped = (ITestRpcMessage)request;
            
            uint callbackID = 0;
            if (callback != null)
            {
                callbackID = m_callbackIDFactory.Next32();
                RegisterCallback(callback, callbackID, cancellationToken);
            }
            await SendRequest(callbackID, requestTyped);
        }
        
        private ValueTask SendRequest(uint requestID, ITestRpcMessage request)
        {
            Console.Out.WriteLine($"send: {request.GetID()} {requestID}");
            
            var writer = new BitWriter(new byte[8]);
            writer.WriteUInt32LittleEndian(request.GetID());
            writer.WriteUInt32LittleEndian(requestID);
            return this.BroadcastBytes(writer.m_output);
        }

        public void Abort()
        {
            Close();
        }
    }
}