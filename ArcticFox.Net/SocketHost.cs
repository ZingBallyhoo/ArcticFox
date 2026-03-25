using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Net.Sockets;
using ArcticFox.Net.Util;

namespace ArcticFox.Net
{
    public abstract class SocketHost : IAsyncDisposable
    {
        private readonly CancellationTokenSource m_cancellationTokenSource = new CancellationTokenSource();

        public bool IsRunning() => !m_cancellationTokenSource.IsCancellationRequested;
        public CancellationToken GetCancellationToken() => m_cancellationTokenSource.Token;

        private readonly AsyncLockedAccess<List<HighLevelSocket>> m_sockets = new AsyncLockedAccess<List<HighLevelSocket>>(new List<HighLevelSocket>());
        
        public int m_recvBufferSize = 1024;
        
        public virtual Task StartAsync(CancellationToken cancellationToken=default)
        {
            return Task.CompletedTask;
        }
        
        public virtual async Task StopAsync(CancellationToken cancellationToken=default)
        {
            await m_cancellationTokenSource.CancelAsync();

            var shutdownStart = DateTime.UtcNow;
            while (true)
            {
                using var sockets = await m_sockets.Get();
                if (sockets.m_value.Count == 0) break;
                
                foreach (var socket in sockets.m_value)
                {
                    socket.Close();
                }

                if (DateTime.UtcNow - shutdownStart > TimeSpan.FromSeconds(10) || cancellationToken.IsCancellationRequested)
                {
                    throw new Exception($"SocketHost ({GetType()}) shutdown timeout. {sockets.m_value.Count} sockets remain");
                }
                
                // ReSharper disable once MethodSupportsCancellation
                await Task.Delay(5);
            }
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
            _ = Task.Run(() => SocketListenTask(socket.m_socket, socket));
            _ = Task.Run(() => SocketSendTask(socket.m_socket, socket));
        }

        private async Task SocketListenTask(SocketInterface socket, HighLevelSocket hl)
        {
            try
            {
                var receiveBuffer = new byte[m_recvBufferSize];
                var receiveMemory = new Memory<byte>(receiveBuffer);
                
                while (!socket.IsClosed())
                {
                    var count = await socket.ReceiveBuffer(receiveMemory);
                    if (count == 0) break;
                    hl.NetworkInput(new ReadOnlySpan<byte>(receiveBuffer, 0, count));
                    await hl.m_taskQueue.ConsumeAll(socket.m_cancellationTokenSource.Token);
                }
            } catch (Exception e)
            {
                hl.HandleException(e);
            } finally
            {
                hl.Close();
                hl.m_taskQueue.Complete();
                await DestroySocket(hl);

                using var sockets = await m_sockets.Get();
                sockets.m_value.Remove(hl);
            }
        }

        private async Task SocketSendTask(SocketInterface socket, HighLevelSocket hl)
        {
            try
            {
                await hl.m_netEventQueue.FlushWorker(socket);
            } catch (Exception e)
            {
                hl.HandleException(e);
            } finally
            {
                hl.Close();
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