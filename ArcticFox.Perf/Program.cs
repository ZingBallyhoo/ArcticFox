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
        
        private const string c_kidToFilter = "A";
        private const string c_messageToSend = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";

        [GlobalSetup]
        public void Setup()
        {
            m_owner = new FilterOwner<DummyBroadcaster>();
        }

        [Benchmark]
        public ValueTask Filtered()
        {
            var filter = new FilterBroadcaster<string>(m_owner.m_kidsExcludeFilterAction, c_kidToFilter);
            return filter.Broadcast(c_messageToSend);
        }
        
        [Benchmark]
        public ValueTask Unfiltered()
        {
            return m_owner.Broadcast(c_messageToSend);
        }
    }
}
