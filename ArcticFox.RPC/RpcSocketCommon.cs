using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;

namespace ArcticFox.RPC
{
    public abstract class RpcSocketCommon<T> : HighLevelSocket, IRpcSocket where T : notnull
    {
        protected readonly ConcurrentDictionary<T, RpcCallback> m_callbacks;

        protected RpcSocketCommon(SocketInterface socket) : base(socket)
        {
            m_callbacks = new ConcurrentDictionary<T, RpcCallback>();
        }

        protected bool TryRemoveCallback(T callbackID, [NotNullWhen(true)] out RpcCallback? callback)
        {
            return m_callbacks.TryRemove(callbackID, out callback);
        }
        
        protected void RegisterCallback(RpcCallback callback, T callbackID, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
        
            if (IsClosed())
            {
                callback.Cancel();
                return;
            }
            
            if (!m_callbacks.TryAdd(callbackID, callback))
            {
                callback.Cancel();
                throw new Exception("callback id should be unique");
            }

            if (IsClosed() || cancellationToken.IsCancellationRequested)
            {
                // how could you :(
                // need to be 100% sure this is cleaned up
                TryCancelCallback(callbackID);
                cancellationToken.ThrowIfCancellationRequested();
                return;
            }
            // else, dispose will clean this up

            LinkCallbackCancellation(callbackID, cancellationToken);
        }
        
        private void LinkCallbackCancellation(T callbackID, CancellationToken cancellationToken)
        {
            if (cancellationToken == default) return; // nothing to do, avoid closure box
            cancellationToken.Register(() =>
            {
                TryCancelCallback(callbackID);
            });
        }
        
        private void TryCancelCallback(T callbackID)
        {
            if (!m_callbacks.TryRemove(callbackID, out var callback)) return;
            callback.Cancel();
        }

        protected void ProcessCallback(T callbackID, ReadOnlySpan<byte> input, object? token)
        {
            if (!TryRemoveCallback(callbackID, out var callback))
            {
                return;
            }
            ProcessCallback(callback, input, token);
        }

        protected void ProcessCallback(RpcCallback callback, ReadOnlySpan<byte> input, object? token)
        {
            var processTask = callback.Process(input, token);
            if (processTask.IsCompleted) return;
            
            m_taskQueue.Enqueue(async () =>
            {
                await processTask;
            });
        }

        public override ValueTask CleanupAsync()
        {
            foreach (var callback in m_callbacks)
            {
                callback.Value.Cancel();
            }
            // IsClosed is set, no more callbacks can be added :tm:
            m_callbacks.Clear();
            
            return base.CleanupAsync();
        }

        public abstract ValueTask CallRemoteAsync<TRequest>(RpcMethod method, TRequest request, RpcCallback? callback, CancellationToken cancellationToken = default) where TRequest : class;
    }
}