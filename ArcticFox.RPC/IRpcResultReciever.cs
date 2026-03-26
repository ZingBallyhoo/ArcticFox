using System;
using System.Threading.Tasks;

namespace ArcticFox.RPC
{
    public interface IRpcResultReceiver
    {
        ValueTask ProcessResult(object? result);
        ValueTask ProcessError(Exception exception);
        void Cancel();
    }

    public class RpcResultReceiver_Task : IRpcResultReceiver
    {
        public readonly TaskCompletionSource<object?> TaskCompletionSource = new TaskCompletionSource<object?>();
        
        public ValueTask ProcessResult(object? result)
        {
            TaskCompletionSource.TrySetResult(result);
            return ValueTask.CompletedTask;
        }

        public ValueTask ProcessError(Exception exception)
        {
            TaskCompletionSource.TrySetException(exception);
            return ValueTask.CompletedTask;
        }

        public void Cancel()
        {
            TaskCompletionSource.TrySetCanceled();
        }
    }
}