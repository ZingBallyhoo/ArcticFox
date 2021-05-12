using Xunit;

namespace ArcticFox.Tests
{
    public class OutputCodecTests
    {
         [Fact]
         public void TestAddNull()
         {
             using var receiver = new TestEncodeCodecChain();
             receiver.DataInput("Hello");
             receiver.DataInput("World");
             receiver.AssertOutput("Hello\0", "World\0");
         }
         
         [Fact]
         public void TestWithNull()
         {
             using var receiver = new TestEncodeCodecChain();
             receiver.DataInput("Hello\0");
             receiver.DataInput("World\0");
             receiver.AssertOutput("Hello\0", "World\0");
         }
         
         [Fact]
         public void TestAbortTooManyNull()
         {
             using var receiver = new TestEncodeCodecChain();
             receiver.DataInput("Hello\0\0");
             receiver.AssertAborted();
         }
         
         [Fact]
         public void TestAbortEmbeddedNull()
         {
             using var receiver = new TestEncodeCodecChain();
             receiver.DataInput("He\0llo");
             receiver.AssertAborted();
         }
    }
}