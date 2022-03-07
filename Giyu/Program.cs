using Giyu.Core;
using System;

namespace Giyu
{
    internal class Program
    {
        static void Main(string[] args)
            => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}
