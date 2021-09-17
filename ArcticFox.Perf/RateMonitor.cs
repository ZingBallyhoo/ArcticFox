using System;
using System.Threading;

namespace ArcticFox.Perf
{
    public class RateMonitor
    {
        private readonly TimeSpan m_period;

        private DateTime m_previousTime;
        private int m_currentTicks;
        
        public int TPS { get; private set; }

        public RateMonitor(TimeSpan period)
        {
            m_period = period;
        }

        public RateMonitor()
        {
            m_period = TimeSpan.FromSeconds(1);
        }

        public bool Tick(int count=1)
        {
            var frameCount = Interlocked.Add(ref m_currentTicks, count);
            var now = DateTime.Now;
            if (now - m_previousTime >= m_period)
            {
                if (Interlocked.CompareExchange(ref m_currentTicks, 0, frameCount) != frameCount)
                {
                    return false;
                }
                TPS = frameCount;
                m_previousTime = now;
                return true;
            }
            return false;
        }
    }
}