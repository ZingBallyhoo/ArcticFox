using System.Threading;
using ArcticFox.Net.Util;
using Xunit;

namespace ArcticFox.Tests
{
    public class IDTests
    {
        [Fact]
        public void FirstIDIs1()
        {
            var factory = new IDFactory();
            var first = factory.Next();
            Assert.Equal(1u, first);
        }
        
        [Fact]
        public void CheckOverflowBehavior()
        {
            // not that we can ever really hit this limit
            
            var val = long.MaxValue;
            Interlocked.Increment(ref val);

            var valUlong = (ulong) val;
            Assert.Equal((ulong)long.MaxValue + 1, valUlong);
        }
    }
}