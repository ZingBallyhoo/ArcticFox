using System;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using ArcticFox.Tests;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace ArcticFox.Perf
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            BenchmarkRunner.Run<BroadcastFilter>();
        }
    }
    
    [SimpleJob]
    [MemoryDiagnoser]
    public class BroadcastFilter
    {
        private FilterOwner<DummyBroadcaster> m_owner;
        private FilterBroadcaster2<string> m_filter2;
        
        private const string c_kidToFilter = "A";
        private const string c_messageToSend = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        [GlobalSetup]
        public void Setup()
        {
            m_owner = new FilterOwner<DummyBroadcaster>();
            m_filter2 = m_owner.m_kidsExcludeFilter;
        }

        [Benchmark]
        public async Task Filtered()
        {
            var filter = new FilterBroadcaster<string>(m_owner.m_kidsExcludeFilterAction, c_kidToFilter);
            await filter.Broadcast(c_messageToSend);
        }
        
        //[Benchmark]
        //public void Filtered2()
        //{
        //    using (m_filter2.Enter(c_kidToFilter))
        //    {
        //        m_filter2.Broadcast(c_messageToSend);
        //    }
        //}
        
        [Benchmark]
        public async Task Unfiltered()
        {
            await m_owner.Broadcast(c_messageToSend);
        }
    }
}
