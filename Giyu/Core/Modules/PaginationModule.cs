using System;
using System.Collections.Generic;
using System.Text;

namespace Giyu.Core.Modules
{
    public class PaginationModule
    {
        public static ulong guildId;
        public static int page = 0;
        public static int total;

        public PaginationModule(ulong _guildId, int _total)
        {
            guildId = _guildId;
            total = _total;
        }

        
    }
}
