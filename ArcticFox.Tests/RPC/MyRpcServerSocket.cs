using System;
using System.IO;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Codec.Binary;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;

 namespace ArcticFox.Tests.RPC
{
    public class MyRpcServerSocket : HighLevelSocket, ISpanConsumer<byte>
    {
        public MyRpcServerSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain().AddCodec(this);
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            var reader = new BitReader(input);
            var methodID = reader.ReadUInt32LittleEndian();
            var requestID = reader.ReadUInt32LittleEndian();
            
            ITestRpcMessage request = methodID switch
            {
                1 => new Request1(),
                2 => new Request2(),
                3 => new Request3(),
                4 => new Request4(),
                _ => throw new InvalidDataException($"unknown message {methodID}")
            };

            m_taskQueue.Enqueue(async () =>
            {
                var resolvedServiceImpl = new MyService_Server();
                object? response;
                try
                {
                    response = await resolvedServiceImpl.InvokeMethodHandler(this, request.GetType().FullName!, ReadOnlySpan<byte>.Empty, request);
                } catch
                {
                    if (requestID != 0) await SendResponse(requestID, new ErrorResponse());
                    return;
                }
                
                if (response == null || requestID == 0) return;
                await SendResponse(requestID, (ITestRpcMessage)response);
            });
        }

        private ValueTask SendResponse(uint requestID, ITestRpcMessage response)
        {
            var writer = new BitWriter(new byte[8]);
            writer.WriteUInt32LittleEndian(response.GetID());
            writer.WriteUInt32LittleEndian(requestID);
            return TempBroadcasterExtensions.BroadcastBytes(this, writer.m_output);
        }

        public void Abort()
        {
            Close();
        }
    }
}