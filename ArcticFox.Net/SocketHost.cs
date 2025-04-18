using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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

        public bool IsRunning() => !m_cancellationTokenSource.IsCancellationRequested;
        public CancellationToken GetCancellationToken() => m_cancellationTokenSource.Token;

        private readonly AsyncLockedAccess<List<HighLevelSocket>> m_sockets = new AsyncLockedAccess<List<HighLevelSocket>>(new List<HighLevelSocket>());
        
        [Obsolete] public bool m_batchMessages;
        [Obsolete] public int m_maxBatchSize = 1460 - 14; // - ws overhead

        public int m_recvBufferSize = 1024;
        
        public virtual Task StartAsync(CancellationToken cancellationToken=default)
        {
            Task.Factory.StartNew(async () =>
            {
                await UpdateThread(GetCancellationToken());
            }, TaskCreationOptions.LongRunning);
            return Task.CompletedTask;
        }
        
        public virtual Task StopAsync(CancellationToken cancellationToken=default)
        {
            m_cancellationTokenSource.Cancel();
            // Defer completion promise, until our application has reported it is done.
            return m_taskCompletionSource.Task;
        }

        protected virtual ISendContext CreateSendContext(Memory<byte>? existingMemory=null)
        {
            if (m_batchMessages)
            {
                return new BatchedSendContext(existingMemory ?? new Memory<byte>(new byte[m_maxBatchSize]));
            } else
            {
                return new NormalSendContext();
            }
        }
        
        public async Task UpdateThread(CancellationToken cancellationToken)
        {
            var ctx = CreateSendContext();
            
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
                            if (socket.IsClosed()) continue;
                            count += await socket.HandlePendingSendEvents(ctx);
                        }
                    }
                    
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
                    socket.Close();
                }
            }

            var shutdownStart = DateTime.UtcNow;
            while (true)
            {
                using var sockets = await m_sockets.Get();
                if (sockets.m_value.Count == 0) break;

                if (DateTime.UtcNow - shutdownStart > TimeSpan.FromSeconds(10))
                {
                    m_taskCompletionSource.SetException(new Exception($"SocketHost ({GetType()}) shutdown timeout. {sockets.m_value.Count} sockets remain"));
                    return;
                }
                
                await Task.Delay(2);
            }
            m_taskCompletionSource.SetResult();
        }

        public abstract HighLevelSocket CreateHighLevelSocket(SocketInterface socket);

        public async ValueTask<int> GetSocketCount()
        {
            using var sockets = await m_sockets.Get();
            return sockets.m_value.Count;
        }

        public async ValueTask<IReadOnlyList<HighLevelSocket>> GetSockets()
        {
            using var sockets = await m_sockets.Get();
            return sockets.m_value.ToArray();
        }

        public async ValueTask AddSocket(HighLevelSocket socket)
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
                    await hl.m_taskQueue.ConsumeAll();
                    await hl.HandlePendingSendEvents(ctx);
                }
            } catch (Exception e)
            {
                // todo: exception handling
                await Console.Out.WriteLineAsync(e.ToString());
            } finally
            {
                hl.Close();
                hl.m_taskQueue.Complete();
                await DestroySocket(hl);

                using var sockets = await m_sockets.Get();
                sockets.m_value.Remove(hl);
            }
        }

        private async ValueTask DestroySocket(HighLevelSocket socket)
        {
            try
            {
                await socket.CleanupAsync();
            } catch (Exception e)
            {
                // todo: log
            }
            try
            {
                await socket.m_socket.TryCloseSocket();
            } catch (Exception e)
            {
                // todo: log
            }
            try
            {
                socket.m_socket.Dispose();
            } catch (Exception e)
            {
                // todo: log
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