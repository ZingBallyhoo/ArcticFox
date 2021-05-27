using System.Threading.Tasks;
using ArcticFox.Net.Util;
using Xunit;

namespace ArcticFox.Tests
{
    public class TaskQueueing
    {
        [Fact]
        public async Task RunsInOrder()
        {
            var queue = new TaskQueue();

            var number = 0;

            for (var i = 0; i < 10; i++)
            {
                var iCopy = i; // todo: hmm
                await queue.Enqueue(async () =>
                {
                    await Task.Delay(10);
                    Assert.Equal(number, iCopy);
                    number++;
                });
            }
            await queue.ConsumeAll();
        }
    }
}