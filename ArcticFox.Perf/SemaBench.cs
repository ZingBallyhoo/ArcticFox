using System;
using System.Threading;
using System.Threading.Tasks;
using ArcticFox.Net.Util;
using BenchmarkDotNet.Attributes;

namespace ArcticFox.Perf
{
    [MemoryDiagnoser]
    public class SemaBench
    {
        [Benchmark]
        public async Task<int> CountAsync()
        {
            var index = 0;
            await s_semaphore.WaitAsync().ConfigureAwait(false);
            var val = s_counters[index]++;
            s_semaphore.Release();
            return val;
        }

        [Benchmark]
        public async ValueTask<int> CountAsyncValueTask()
        {
            var index = 0;
            await s_semaphore.WaitAsync().ConfigureAwait(false);
            var val =  s_counters[index]++;
            s_semaphore.Release();
            return val;
        }

        [Benchmark]
        public ValueTask<int> CountAsyncValueTaskLocalFunction()
        {
            var index = 0;
            var task = s_semaphore.WaitAsync();
            return task.IsCompleted ? new ValueTask<int>(Remainder()) : new ValueTask<int>(Awaited());

            async Task<int> Awaited()
            {
                await task.ConfigureAwait(false);
                return Remainder();
            }

            int Remainder()
            {
                var val = s_counters[index]++;
                s_semaphore.Release();
                return val;
            }
        }

        [Benchmark]
        public ValueTask<int> CountAsyncValueTaskLocalFunctionNoCapture()
        {
            var index = 0;
            var task = s_semaphore.WaitAsync();
            return task.IsCompleted ? new ValueTask<int>(Remainder(index)) : new ValueTask<int>(Awaited(task, index));

            static async Task<int> Awaited(Task task_, int index_)
            {
                await task_.ConfigureAwait(false);
                return Remainder(index_);
            }

            static int Remainder(int index_)
            {
                var val = s_counters[index_]++;
                s_semaphore.Release();
                return val;
            }
        }

        [Benchmark]
        public ValueTask<int> CountAsyncValueTaskMultipleFunctions()
        {
            var index = 0;
            var task = s_semaphore.WaitAsync();
            return task.IsCompleted
                ? new ValueTask<int>(CountAsyncValueTaskMultipleFunctionsRemainder(index))
                : new ValueTask<int>(CountAsyncValueTaskMultipleFunctionsAwaited(task, index));
        }

        private static async Task<int> CountAsyncValueTaskMultipleFunctionsAwaited(Task task, int index)
        {
            await task.ConfigureAwait(false);
            return CountAsyncValueTaskMultipleFunctionsRemainder(index);
        }

        private static int CountAsyncValueTaskMultipleFunctionsRemainder(int index)
        {
            var val = s_counters[index]++;
            s_semaphore.Release();
            return val;
        }
        
        [Benchmark]
        public ValueTask<int> CountAsyncValueTaskMultipleFunctions0Timeout()
        {
            var index = 0;
            var got = s_semaphore.Wait(0);
            return got
                ? new ValueTask<int>(CountAsyncValueTaskMultipleFunctionsRemainder(index))
                : new ValueTask<int>(CountAsyncValueTaskMultipleFunctionsAwaited(s_semaphore.WaitAsync(), index));
        }

        [Benchmark]
        public ValueTask<int> AsyncLockedAccess()
        {
            var index = 0;

            var getTask = s_asyncLock.Get();
            if (!getTask.IsCompleted) Throw();
            
            using var token = getTask.Result;
            var val = token.m_value[index]++;

            return new ValueTask<int>(val);
        }

        private static void Throw() => throw new Exception();

        private static readonly int[] s_counters = new int[20];
        private static readonly AsyncLockedAccess<int[]> s_asyncLock = new AsyncLockedAccess<int[]>(s_counters);
        private static readonly SemaphoreSlim s_semaphore = new SemaphoreSlim(1, 1);
    }
}