using System;
using System.IO;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Codec.Binary;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using PolyType.Abstractions;

namespace ArcticFox.Tests.RPC.New
{
    public class MyRpcServerSocket2 : HighLevelSocket, ISpanConsumer<byte>
    {
        public MyRpcServerSocket2(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain<byte>(this);
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            var reader = new BitReader(input);
            while (reader.m_dataOffset < reader.m_dataLength)
            {
                var methodID = reader.ReadUInt32LittleEndian();
                var requestID = reader.ReadUInt32LittleEndian();
            
                Console.Out.WriteLine($"recv: {methodID} {requestID}");
            
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
                    var resolver = new MyRpcResolver();
                    resolver.BindService(TypeShapeResolver.Resolve<IMyService2<MyRpcServerSocket2>, MyService2_Server>(), _ => new MyService2_Server());

                    var callContext = new MyRpcCallContext
                    {
                        Socket = this,
                        Payload = request,
                    };
                    
                    object? response;
                    try
                    {
                        response = await resolver.InvokeAsync(callContext);
                    } catch
                    {
                        if (requestID != 0) await SendResponse(requestID, new ErrorResponse());
                        return;
                    }
                
                    if (response == null || requestID == 0) return;
                    await SendResponse(requestID, (ITestRpcMessage)response);
                });
            }
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