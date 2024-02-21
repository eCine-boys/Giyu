using Giyu.Core;
using Giyu.Core.Managers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Giyu
{
    internal class Program
    {
        static void Main(string[] args) => new Bot().MainAsync().GetAwaiter().GetResult();
    }
}
