using System;
using System.Threading;
using CommunityToolkit.HighPerformance;

namespace ArcticFox.Net.Event
{
    // todo: decide which is better
    public struct FilterBroadcaster<TState> : IBroadcaster
    {
        private readonly Action<NetEvent, TState> m_filter;
        private TState m_current;

        public FilterBroadcaster(Action<NetEvent, TState> filter, TState filterParam)
        {
            m_filter = filter;
            m_current = filterParam;
        }
        
        public void BroadcastEvent(NetEvent ev)
        {
            m_filter(ev, m_current);
        }
    }
    
    public class FilterBroadcaster2<TState> : IBroadcaster
    {
        private readonly Action<NetEvent, TState> m_filter;
        private SpinLock m_lock;
        private TState? m_current;

        public FilterBroadcaster2(Action<NetEvent, TState> filter)
        {
            m_filter = filter;
            m_lock = new SpinLock();
        }

        public SpinLockExtensions.Lock Enter(TState state)
        {
            m_current = state;
            return m_lock.Enter();
        }
        
        public void BroadcastEvent(NetEvent ev)
        {
            m_filter(ev, m_current!);
        }
    }
}