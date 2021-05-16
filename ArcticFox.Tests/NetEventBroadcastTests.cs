using System;
using System.Collections.Generic;
using System.Text;
using ArcticFox.Net;
using ArcticFox.Net.Event;
using Xunit;

namespace ArcticFox.Tests
{
    public class DummyBroadcaster : IBroadcaster
    {
        public void BroadcastEvent(NetEvent ev)
        {
        }
    }
    
    public class TestBroadcaster : IBroadcaster
    {
        public List<string> m_eventsCreatedAsString = new List<string>();
        
        public void FakeSendEvent(string text)
        {
            TempBroadcasterExtensions.Broadcast(this, text);
        }
        
        public void BroadcastEvent(NetEvent ev)
        {
            m_eventsCreatedAsString.Add(Encoding.ASCII.GetString(ev.GetMemory().Span));
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

        public readonly Action<NetEvent, string> m_kidsExcludeFilterAction;
        public readonly FilterBroadcaster2< string> m_kidsExcludeFilter;

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

            m_kidsExcludeFilterAction = (ne, kidToExclude) =>
            {
                foreach (var kid in m_kids)
                {
                    if (kid.Key == kidToExclude) continue;
                    kid.Value.BroadcastEvent(ne);
                }
            };
            m_kidsExcludeFilter = new FilterBroadcaster2<string>(m_kidsExcludeFilterAction);
        }

        public void BroadcastEvent(NetEvent ev)
        {
            foreach (var kid in m_kids)
            {
                kid.Value.BroadcastEvent(ev);
            }
        }
    }
    
    public class NetEventBroadcastTests
    {
        [Fact]
        public void TestAddNull()
        {
            var broadcaster = new TestBroadcaster();
            broadcaster.FakeSendEvent("Hello");
            broadcaster.AssertCreated("Hello\0");
        }
        
        [Fact]
        public void TestFilter()
        {
            var filterOwner = new FilterOwner<TestBroadcaster>();
            
            var filter = new FilterBroadcaster<string>(filterOwner.m_kidsExcludeFilterAction, "A");
            filter.Broadcast("GGs");
            
            filterOwner.m_kids["A"].AssertCreated();
            filterOwner.m_kids["B"].AssertCreated("GGs");
        }
        
        [Fact]
        public void TestFilter2()
        {
            var filterOwner = new FilterOwner<TestBroadcaster>();

            var filter = filterOwner.m_kidsExcludeFilter;
            using (filter.Enter("A"))
            {
                filter.Broadcast("GGs");
            }
            
            filterOwner.m_kids["A"].AssertCreated();
            filterOwner.m_kids["B"].AssertCreated("GGs");
        }
    }
}