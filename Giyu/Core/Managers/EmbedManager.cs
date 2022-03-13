using Discord;
using System;

namespace Giyu.Core.Managers
{
    public static class EmbedManager
    {

        private static readonly Color[] colors = new Color[] { 
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
        public static Embed ReplySimple(string title, string description)
        {

            EmbedBuilder embed = new EmbedBuilder();

            Random rnd = new Random();

            embed
            .WithColor(colors[rnd.Next(0, colors.Length)])
            .WithAuthor(x => x.IconUrl = "https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150")
            .WithCurrentTimestamp()
            .WithTitle(title)
            .WithDescription(description);

            return embed.Build();
        }

        public static Embed ReplyError(string description)
        {

            EmbedBuilder embed = new EmbedBuilder();

            embed
            .WithColor(Color.Red)
            .WithCurrentTimestamp()
            .WithTitle("Erro")
            .WithDescription(description);

            return embed.Build();
        }
    }
}
