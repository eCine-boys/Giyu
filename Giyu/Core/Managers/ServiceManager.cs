using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Giyu.Core.Managers
{
    public static class ServiceManager
    {
        public static IServiceProvider Provider { get; private set; }

        public static void SetProvider(ServiceCollection collection)
            => Provider = collection.BuildServiceProvider();

        public static T GetService<T>() where T : new ()
            => Provider.GetRequiredService<T>();

    }
}
