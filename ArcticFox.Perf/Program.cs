using System;
using BenchmarkDotNet.Running;

namespace ArcticFox.Perf
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            BenchmarkRunner.Run<BroadcastFilter>();
            //BenchmarkRunner.Run<SemaBench>();
        }
    }
}
