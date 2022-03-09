using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Giyu.Core.Managers
{
    public static class EmbedManager
    {

        private static Color[] colors = new Color[] { 
            Color.Blue,
            Color.DarkMagenta,
            Color.Magenta,
            Color.Purple,
            Color.Teal,
            Color.Orange,
            Color.LightOrange,
            Color.Green,
            Color.Gold,
            Color.DarkBlue,
            Color.DarkRed,
        };
        public static Color GetRandomColor ()
        {
            Random rnd = new Random();

            return colors[rnd.Next(0, colors.Length)];
        }
        public static Embed ReplySimple(string title, string description, Color color, string author)
        {

            EmbedBuilder embed = new EmbedBuilder();

            Random rnd = new Random();

            embed
            .WithColor(colors[rnd.Next(0, colors.Length)])
            .WithCurrentTimestamp()
            .WithTitle(title)
            .WithDescription(description);

            return embed.Build();
        }
    }
}
