using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using ArcticFox.Tests;
using BenchmarkDotNet.Attributes;

namespace ArcticFox.Perf
{
    [SimpleJob]
    [MemoryDiagnoser]
    public class BroadcastFilter
    {
        private FilterOwner<DummyBroadcaster> m_owner;
        
        private const string c_kidToFilter = "A";
        private const string c_messageToSend = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\0";

        [GlobalSetup]
        public void Setup()
        {
            m_owner = new FilterOwner<DummyBroadcaster>();
        }

        [Benchmark]
        public ValueTask Filtered()
        {
            var filter = new FilterBroadcaster<string>(m_owner.m_kidsExcludeFilterAction, c_kidToFilter);
            return filter.BroadcastZeroTerminatedAscii(c_messageToSend);
        }
        
        [Benchmark]
        public ValueTask Unfiltered()
        {
            return m_owner.BroadcastZeroTerminatedAscii(c_messageToSend);
        }
    }
}