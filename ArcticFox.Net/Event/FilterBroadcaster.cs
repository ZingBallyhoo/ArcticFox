using System;
using System.Threading.Tasks;

namespace ArcticFox.Net.Event
{
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
}