using System;

namespace Cascade
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.Title = "Loading Cascade...";

            global::Cascade.Cascade.Initialize();

            Console.ReadKey();
        }
    }
}
