using System;
using System.Buffers;
using System.IO;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Codec
{
    public abstract class DynamicSizeBufferCodec<T> : SpanCodec<T, T>, IDisposable
    {
        private IMemoryOwner<T>? m_memory;
        protected int m_maxMemorySize = -1;

        protected abstract void Reset();

        protected void ExtendFinalMemory(ref ReadOnlySpan<T> input, ReadOnlySpan<T> finalData, ref object? state)
        {
            if (m_memory == null)
            {
                CodecOutput(finalData, ref state);
            } else
            {
                ExtendMemoryInternal(finalData);
                CodecOutput(m_memory.Memory.Span, ref state);
                KillMemory();
            }     
            input = input.Slice(finalData.Length);
            
            Reset();
        }

        private void ExtendMemoryInternal(ReadOnlySpan<T> newData)
        {
            var requiredSize = newData.Length;
            var writeOffset = 0;
            if (m_memory != null)
            {
                writeOffset = m_memory.Memory.Length;
                requiredSize += writeOffset;
            }

            if (m_maxMemorySize != -1 && requiredSize > m_maxMemorySize)
            {
                Abort();
                throw new InvalidDataException($"message too big. {requiredSize} > {m_maxMemorySize}");
            }

            var newMemory = MemoryOwner<T>.Allocate(requiredSize);
            var existingMemory = m_memory;
            if (existingMemory != null)
            {
                existingMemory.Memory.CopyTo(newMemory.Memory);
                existingMemory.Dispose();
                KillMemory();
            }

            newData.CopyTo(newMemory.Memory.Span.Slice(writeOffset));
            m_memory = newMemory;
        }

        protected void ExtendMemory(ref ReadOnlySpan<T> newData)
        {
            ExtendMemoryInternal(newData);
            newData = ReadOnlySpan<T>.Empty;
        }

        private void KillMemory()
        {
            var mem = m_memory;
            if (mem == null) return;
            m_memory = null;
            mem.Dispose();
        }

        public void Dispose()
        {
            KillMemory();
            GC.SuppressFinalize(this);
        }

        ~DynamicSizeBufferCodec()
        {
            Dispose();
        }
    }
}