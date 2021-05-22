using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using ArcticFox.Net.Batching;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;
using Serilog;

namespace ArcticFox.Net
{
    public abstract class SocketHost : IAsyncDisposable
    {
        private readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();
        private readonly TaskCompletionSource m_taskCompletionSource = new TaskCompletionSource();
        private readonly Channel<HighLevelSocket> m_disposeQueue = Channel.CreateUnbounded<HighLevelSocket>();

        public bool IsRunning() => !m_cancellationTokenSource.IsCancellationRequested;
        public CancellationToken GetCancellationToken() => m_cancellationTokenSource.Token;

        private readonly AsyncLockedAccess<List<HighLevelSocket>> m_sockets = new AsyncLockedAccess<List<HighLevelSocket>>(new List<HighLevelSocket>());
        
        public bool m_batchMessages;
        public int m_maxBatchSize = 1460 - 14; // - ws overhead

        public int m_recvBufferSize = 1024;
        
        public virtual Task StartAsync(CancellationToken cancellationToken=default)
        {
            Task.Factory.StartNew(async () =>
            {
                await UpdateThread(GetCancellationToken());
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(async () =>
            {
                await DisposeThread();
            }, TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken=default)
        {
            m_cancellationTokenSource.Cancel();
            // Defer completion promise, until our application has reported it is done.
            return m_taskCompletionSource.Task;
        }

        private ISendContext CreateSendContext(Memory<byte>? existingMemory=null)
        {
            if (m_batchMessages)
            {
                return new BatchedSendContext(existingMemory ?? new Memory<byte>(new byte[m_maxBatchSize]));
            } else
            {
                return new NormalSendContext();
            }
        }

        private async Task DisposeThread()
        {
            while (true)
            {
                HighLevelSocket a;
                try
                {
                    a = await m_disposeQueue.Reader.ReadAsync();
                } catch (ChannelClosedException)
                {
                    break;
                }
                try
                {
                    await a.m_socket.TryCloseSocket();
                    await a.DisposeAsync();
                } catch (Exception e)
                {
                    // todo: log
                }
            }
        }
        
        public async Task UpdateThread(CancellationToken cancellationToken)
        {
            var ctx = CreateSendContext();

            var toRemove = new List<HighLevelSocket>();
            var sw = new Stopwatch();

            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = 0;
                try
                {
                    sw.Start();
                    var count = 0;

                    using (var sockets = await m_sockets.Get())
                    {
                        foreach (var socket in sockets.m_value)
                        {
                            if (socket.IsClosed())
                            {
                                toRemove.Add(socket);
                                continue;
                            }
                            count += await socket.HandlePendingSendEvents(ctx);
                        }
                        foreach (var impl in toRemove)
                        {
                            sockets.m_value.Remove(impl);
                            await m_disposeQueue.Writer.WriteAsync(impl);
                        }
                    }
                    toRemove.Clear();
                    
                    sw.Stop();
                    if (count > 0) Log.Error("Send Count: {Count} {Time}", count, sw.Elapsed.TotalMilliseconds);
                    elapsed = sw.Elapsed.Milliseconds;
                } catch (Exception e)
                {
                    Log.Fatal(e, "Error on SocketHost update");
                } finally
                {
                    sw.Reset();
                }
                await Task.Delay(16-Math.Min(16, elapsed)); // todo: loop accumulation
            }

            using (var sockets = await m_sockets.Get())
            {
                foreach (var socket in sockets.m_value)
                {
                    await m_disposeQueue.Writer.WriteAsync(socket);
                }
                sockets.m_value.Clear();
            }
            m_disposeQueue.Writer.TryComplete();
            await m_disposeQueue.Reader.Completion;
            m_taskCompletionSource.SetResult();
        }

        public abstract HighLevelSocket CreateHighLevelSocket(SocketInterface socket);

        public async Task<int> GetSocketCount()
        {
            using (var sockets = await m_sockets.Get())
            {
                return sockets.m_value.Count;
            }
        }

        public async Task<IReadOnlyList<HighLevelSocket>> GetSockets()
        {
            using (var sockets = await m_sockets.Get())
            {
                return sockets.m_value.ToArray();
            }
        }
        
        public async Task AddSocket(HighLevelSocket socket)
        {
            using (var sockets = await m_sockets.Get())
            {
                sockets.m_value.Add(socket);
            }
            // run in background
            Task.Run(() => SocketListenTask(socket.m_socket, socket));
        }

        private async Task SocketListenTask(SocketInterface socket, HighLevelSocket hl)
        {
            try
            {
                var receiveBuffer = new byte[m_recvBufferSize];
                var receiveMemory = new Memory<byte>(receiveBuffer);
                var ctx = CreateSendContext(receiveMemory);
                
                while (!socket.IsClosed())
                {
                    var count = await socket.ReceiveBuffer(receiveMemory);
                    if (count == 0) break;
                    hl.NetworkInput(new ReadOnlySpan<byte>(receiveBuffer, 0, count));
                    await hl.HandlePendingSendEvents(ctx);
                }
            } catch (Exception e)
            {
                // todo: exception handling
                await Console.Out.WriteLineAsync(e.ToString());
            } finally
            {
                socket.Close();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!m_cancellationTokenSource.IsCancellationRequested)
            {
                await StopAsync();
            }
        }
    }
}