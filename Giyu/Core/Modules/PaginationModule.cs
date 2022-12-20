using Discord;
using Giyu.Core.Managers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;

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

        public async Task<Embed> Paginate(LavaPlayer player, int page = 1)
        {
            EmbedBuilder embed = new EmbedBuilder();

            DefaultQueue<LavaTrack> queue = player.Queue;

            List<LavaTrack> playlist = new List<LavaTrack>();

            int endPage = page * 10;

            int startPage = endPage <= 10 ? 1 : endPage - 10;

            foreach(LavaTrack track in queue)
            {
                playlist.Add(track);
            }

            var tracks = playlist.ToArray();

            LavaTrack[] data = new LavaTrack[0];

            try
            {
                if(tracks.Length > startPage)
                    data = tracks[startPage..endPage];
            }
            catch(ArgumentOutOfRangeException outExcept)
            {
                data = tracks[startPage..endPage];
            }
            catch(Exception ex)
            {
                LogManager.LogError("PaginationModule", ex.Message);
            }

            foreach(LavaTrack track in data)
            {
                embed.AddField(track.Title, track.Author);
            }

            return embed.Build();

        }
        
    }
}
