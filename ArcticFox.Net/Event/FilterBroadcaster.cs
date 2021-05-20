using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.HighPerformance;

namespace ArcticFox.Net.Event
{
    // todo: decide which is better
    public struct FilterBroadcaster<TState> : IBroadcaster
    {
        private readonly Func<NetEvent, TState, ValueTask> m_filter;
        private TState m_current;

        public FilterBroadcaster(Func<NetEvent, TState, ValueTask> filter, TState filterParam)
        {
            m_filter = filter;
            m_current = filterParam;
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            return m_filter(ev, m_current);
        }
    }
    
    public class FilterBroadcaster2<TState> : IBroadcaster
    {
        private readonly Func<NetEvent, TState, ValueTask> m_filter;
        private SpinLock m_lock;
        private TState? m_current;

        public FilterBroadcaster2(Func<NetEvent, TState, ValueTask> filter)
        {
            m_filter = filter;
            m_lock = new SpinLock();
        }

        public SpinLockExtensions.Lock Enter(TState state)
        {
            m_current = state;
            return m_lock.Enter();
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            return m_filter(ev, m_current!);
        }
    }
}