using System.IO;
using ArcticFox.Tests.Impls;
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
             Assert.Throws<InvalidDataException>(() =>
             {
                 receiver.DataInput("Hello\0\0");
             });
         }
         
         [Fact]
         public void TestAbortEmbeddedNull()
         {
             using var receiver = new TestEncodeCodecChain();
             Assert.Throws<InvalidDataException>(() =>
             {
                 receiver.DataInput("He\0llo");
             });
         }
    }
}