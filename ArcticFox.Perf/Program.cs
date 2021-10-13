using System;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using BenchmarkDotNet.Running;

namespace ArcticFox.Perf
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            //BenchmarkRunner.Run<BroadcastFilter>();
            //BenchmarkRunner.Run<SemaBench>();
            //BenchmarkRunner.Run<BinaryPerf>();

            var binPerf = new BinaryPerf();
            binPerf.m_offset = 1;
            binPerf.GlobalSetup();

            for (var i = 0; i < 100000; i++)
            {
                binPerf.ReadAsSingles();
            }

            // await SendPerfTest();
        }

        private static async Task SendPerfTest()
        {
            var monitor = new RateMonitor();

            await using var host = new NullSocketHost();
            host.m_batchMessages = true;
            await host.StartAsync();
            
            var sockets = new HighLevelSocket[450];
            for (var i = 0; i < sockets.Length; i++)
            {
                sockets[i] = host.CreateHighLevelSocket(new NullSocketInterface());
                await host.AddSocket(sockets[i]);
            }

            var socketList = new SocketList(sockets);
            var toSend = new byte[]
            {
                //0, 1, 2, 3, 4
            };

            //for (var i = 0; i < 100; i++)
            //{
            //Task.Run(async () =>
            //{
            while (true)
            {
                await socketList.BroadcastBytes(toSend);
                if (monitor.Tick(sockets.Length))
                {
                    //using var got = sockets[0].m_netEventQueue.m_eventQueue.TryGet();
                    //await Console.Out.WriteLineAsync($"{monitor.TPS} {got?.m_value.Count}");
                            
                    await Console.Out.WriteLineAsync($"{monitor.TPS}");
                }
                        
                //for (var j = 0; j < sockets.Length; j++)
                //{
                //    //await sockets[j].BroadcastBytes(toSend);
                //}
            }
            //});
            //}
            // await Task.Delay(-1);
        }

        private class SocketList : IBroadcaster
        {
            public readonly HighLevelSocket[] m_sockets;

            public SocketList(HighLevelSocket[] sockets)
            {
                m_sockets = sockets;
            }

            public async ValueTask BroadcastEvent(NetEvent ev)
            {
                foreach (var socket in m_sockets)
                {
                    await socket.BroadcastEvent(ev);
                }
            }
        }
    }
}
