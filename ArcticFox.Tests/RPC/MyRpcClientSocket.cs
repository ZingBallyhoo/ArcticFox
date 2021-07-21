using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Codec.Binary;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;
using ArcticFox.RPC;

namespace ArcticFox.Tests.RPC
{
    public class MyRpcClientSocket  : HighLevelSocket, IRpcSocket, ISpanConsumer<byte>
    {
        private readonly AsyncLockedAccess<Dictionary<uint, RpcCallback>> m_callbacks;
        private readonly IDFactory m_callbackIDFactory;
        
        public MyRpcClientSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain().AddCodec(this);
            
            m_callbacks = new AsyncLockedAccess<Dictionary<uint, RpcCallback>>(new Dictionary<uint, RpcCallback>());
            m_callbackIDFactory = new IDFactory();
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            var reader = new BitReader(input);
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
            
            m_taskQueue.Enqueue(async () =>
            {
                RpcCallback callback;
                using (var callbacks = await m_callbacks.Get())
                {
                    callback = callbacks.m_value[requestID];
                    callbacks.m_value.Remove(requestID);
                }

                if (response is ErrorResponse)
                {
                    callback.SetException(new Exception());
                } else
                {
                    await callback.Process(ReadOnlySpan<byte>.Empty, response);
                }
            });
        }

        public async ValueTask CallRemoteAsync(RpcMethod method, object request, RpcCallback? callback)
        {
            var requestTyped = (ITestRpcMessage) request;
            
            uint callbackID = 0;
            if (callback != null)
            {
                var callbackIDLong = m_callbackIDFactory.Next();
                Debug.Assert(callbackIDLong <= uint.MaxValue);
                callbackID = (uint) callbackIDLong;
                using var callbacks = await m_callbacks.Get();
                callbacks.m_value[callbackID] = callback;
            }
            await SendRequest(callbackID, requestTyped);
        }
        
        private ValueTask SendRequest(uint requestID, ITestRpcMessage request)
        {
            var writer = new BitWriter(new byte[8]);
            writer.WriteUInt32LittleEndian(request.GetID());
            writer.WriteUInt32LittleEndian(requestID);
            return TempBroadcasterExtensions.BroadcastBytes(this, writer.m_output);
        }

        public void Abort()
        {
            Close();
        }
        
        public override async ValueTask DisposeAsync()
        {
            using (var callbacks = m_callbacks.GetSync())
            {
                foreach (var callbackPair in callbacks.m_value)
                {
                    callbackPair.Value.Cancel();
                }
            }
            await base.DisposeAsync();
        }
    }
}