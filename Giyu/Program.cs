using Giyu.Core;

namespace Giyu
{
    internal class Program
    {
        static void Main(string[] args)
            => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}
