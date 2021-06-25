using System;
using CommunityToolkit.HighPerformance.Buffers;

namespace ArcticFox.Net.Event
{
    public class NetEvent : RefCounted
    {
        private MemoryOwner<byte> m_memoryOwner;

        protected NetEvent(ReadOnlySpan<byte> data)
        {
            m_memoryOwner = MemoryOwner<byte>.Allocate(data.Length);
            data.CopyTo(m_memoryOwner.Span);
        }

        public ReadOnlyMemory<byte> GetMemory()
        {
            return m_memoryOwner.Memory;
        }

        public static NetEvent Create(ReadOnlySpan<byte> data)
        {
            // todo: i used to pool these, is it even a good idea?
            return new NetEvent(data);
        }

        private void ReleaseBuffer()
        {
            m_memoryOwner.Dispose();
            m_memoryOwner = MemoryOwner<byte>.Empty;
        }

        protected override void Cleanup()
        {
            ReleaseBuffer();
            GC.SuppressFinalize(this);
        }

        ~NetEvent()
        {
            ReleaseBuffer();
        }
    }
}