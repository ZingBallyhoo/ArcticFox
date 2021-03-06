using ArcticFox.Tests.Impls;
using Xunit;

namespace ArcticFox.Tests
{
    public class InputCodecTests
    {
        [Fact]
        public void Test1()
        {
            using var receiver = new TestDecodeCodecChain();
            receiver.DataInput("Hello\0");
            receiver.AssertReceived("Hello");
        }
        
        [Fact]
        public void TestJoined()
        {
            using var receiver = new TestDecodeCodecChain();
            receiver.DataInput("Hello\0World\0");
            receiver.AssertReceived("Hello", "World");
        }
        
        [Fact]
        public void TestSplit()
        {
            using var receiver = new TestDecodeCodecChain();
            receiver.DataInput("H");
            receiver.AssertReceived();
            receiver.DataInput("ello");
            receiver.DataInput("\0World");
            receiver.AssertReceived("Hello");
            receiver.DataInput("\0");
            receiver.AssertReceived("World");
        }
        
        [Fact]
        public void TestMissingDataIssue()
        {
            using var receiver = new TestDecodeCodecChain();
            receiver.DataInput("H");
            receiver.DataInput("ello\0");
            receiver.AssertReceived("Hello");
        }
        
        [Fact]
        public void TestAsciiText()
        {
            using var receiver = new TestDecodeCodecChain();
            receiver.DataInput("\xFF\xFF\xFF\0");
            receiver.AssertReceived("???");
        }
        
        [Fact]
        public void TestAbortedNoText()
        {
            using var receiver = new TestDecodeCodecChain();
            receiver.DataInput("\0\0");
            receiver.AssertAborted();
        }
        
        [Fact]
        public void TestHugeBlob()
        {
            const int size = 1000;
            var str = new string('a', size);
            var inputStr = str + "\0";

            using var receiver = new TestDecodeCodecChain();
            for (var i = 0; i < 10; i++)
            {
                receiver.DataInput("H\0");
                receiver.DataInput(inputStr);
                receiver.AssertReceived("H", str);
            }
        }
    }
}
