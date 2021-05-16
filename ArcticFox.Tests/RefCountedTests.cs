using System;
using System.Threading.Tasks;
using ArcticFox.Net.Event;
using Xunit;

namespace ArcticFox.Tests
{
    public class TestRefCounter : RefCounted
    {
        public bool m_cleanedUp;
        
        protected override void Cleanup()
        {
            m_cleanedUp = true;
        }

        public void AssertCleanedUp()
        {
            Assert.True(m_cleanedUp);
        }
        
        public void AssertNotCleanedUp()
        {
            Assert.False(m_cleanedUp);
        }
    }
    
    public class RefCountedTests
    {
        [Fact]
        public void CleansUpNoRef()
        {
            var c = new TestRefCounter();
            c.AssertNotCleanedUp();
            c.ReleaseCreationRef();
            c.AssertCleanedUp();
        }
        
        [Fact]
        public void ReleaseCreationRefTwice()
        {
            var c = new TestRefCounter();
            
            c.GetRef();
            
            c.AssertNotCleanedUp();
            c.ReleaseCreationRef();
            c.ReleaseCreationRef();
            c.AssertNotCleanedUp();
            
            c.ReleaseRef();
            c.AssertCleanedUp();
        }
        
        [Fact]
        public void CleansUp100Ref()
        {
            var c = new TestRefCounter();

            const int count = 100;
            for (var i = 0; i < count; i++)
            {
                c.AssertNotCleanedUp();
                c.GetRef();
            }
            for (var i = 0; i < count; i++)
            {
                c.ReleaseRef();
                c.AssertNotCleanedUp();
            }
            
            c.ReleaseCreationRef();
            c.AssertCleanedUp();
        }
        
        [Fact]
        public void CleansUp100Ref2()
        {
            var c = new TestRefCounter();

            const int count = 100;
            for (var i = 0; i < count; i++)
            {
                c.AssertNotCleanedUp();
                c.GetRef();
            }
            
            c.ReleaseCreationRef();
            
            for (var i = 0; i < count; i++)
            {
                c.ReleaseRef();
                if (i == count - 1)
                {
                    // the last iter, no refs remain
                    c.AssertCleanedUp();
                } else
                {
                    c.AssertNotCleanedUp();
                }
            }
            
            // sane
            c.AssertCleanedUp();
        }

        [Fact]
        public void Tasks()
        {
            var c = new TestRefCounter();

            const int count = 100;
            var getRefTasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                getRefTasks[i] = Task.Run(() =>
                {
                    c.GetRef();
                });
            }

            Task.WaitAll(getRefTasks);
            
            c.AssertNotCleanedUp();
            c.ReleaseCreationRef();
            c.AssertNotCleanedUp();
            
            Assert.Equal(count, c.RefCount());
            
            var releaseRefTasks = new Task[count];
            for (var i = 0; i < count; i++)
            {
                releaseRefTasks[i] = Task.Run(() =>
                {
                    c.ReleaseRef();
                });
            }
            Task.WaitAll(releaseRefTasks);
            
            c.AssertCleanedUp();
        }
        
        [Fact]
        public void DontRevive()
        {
            var c = new TestRefCounter();
            
            c.ReleaseCreationRef();
            c.AssertCleanedUp();

            Assert.Throws<Exception>(() => c.GetRef());
        }
    }
}