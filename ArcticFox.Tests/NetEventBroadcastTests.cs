using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using Xunit;

namespace ArcticFox.Tests
{
    public class DummyBroadcaster : IBroadcaster
    {
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            return ValueTask.CompletedTask;
        }
    }
    
    public class TestBroadcaster : IBroadcaster
    {
        public List<string> m_eventsCreatedAsString = new List<string>();
        
        public ValueTask FakeSendEvent(string text)
        {
            return TempBroadcasterExtensions.Broadcast(this, text.AsSpan());
        }
        
        public ValueTask BroadcastEvent(NetEvent ev)
        {
            m_eventsCreatedAsString.Add(Encoding.ASCII.GetString(ev.GetMemory().Span));
            return ValueTask.CompletedTask;
        }
        
        public void AssertCreated(params string[] expected)
        {
            Assert.Equal(expected, m_eventsCreatedAsString);
            m_eventsCreatedAsString.Clear();
        }
    }

    public class FilterOwner<T> : IBroadcaster where T : IBroadcaster, new()
    {
        public readonly Dictionary<string, T> m_kids;

        public readonly Func<NetEvent, string, ValueTask> m_kidsExcludeFilterAction;

        public FilterOwner()
        {
            m_kids = new Dictionary<string, T>();
            m_kids["A"] = new T();
            m_kids["B"] = new T();
            m_kids["D"] = new T();
            m_kids["E"] = new T();
            m_kids["F"] = new T();
            m_kids["G"] = new T();

            for (var i = 0; i < 1000; i++)
            {
                m_kids.Add(i.ToString(), new T());
            }

            m_kidsExcludeFilterAction = KidsExcludeFilterAction;
        }

        private async ValueTask KidsExcludeFilterAction(NetEvent ne, string kidToExclude)
        {
            foreach (var kid in m_kids)
            {
                if (kid.Key == kidToExclude) continue;
                await kid.Value.BroadcastEvent(ne);
            }
        }

        public async ValueTask BroadcastEvent(NetEvent ev)
        {
            foreach (var kid in m_kids)
            {
                await kid.Value.BroadcastEvent(ev);
            }
        }
    }
    
    public class NetEventBroadcastTests
    {
        [Fact]
        public async Task TestAddNull()
        {
            var broadcaster = new TestBroadcaster();
            await broadcaster.FakeSendEvent("Hello");
            broadcaster.AssertCreated("Hello\0");
        }
        
        [Fact]
        public async Task TestFilter()
        {
            var filterOwner = new FilterOwner<TestBroadcaster>();
            
            var filter = new FilterBroadcaster<string>(filterOwner.m_kidsExcludeFilterAction, "A");
            await filter.Broadcast("GGs");
            
            filterOwner.m_kids["A"].AssertCreated();
            filterOwner.m_kids["B"].AssertCreated("GGs");
        }
        
        //[Fact]
        //public void TestFilter2()
        //{
        //    var filterOwner = new FilterOwner<TestBroadcaster>();
        //    var filter = filterOwner.m_kidsExcludeFilter;
        //    using (filter.Enter("A"))
        //    {
        //        filter.Broadcast("GGs");
        //    }
        //    
        //    filterOwner.m_kids["A"].AssertCreated();
        //    filterOwner.m_kids["B"].AssertCreated("GGs");
        //}
    }
}