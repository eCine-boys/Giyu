using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Giyu.Core.Managers;
using System.Threading.Tasks;

namespace Giyu.Core.Commands
{
    [Name("MusicSlash")]
    public class MusicSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("play", "Toca uma música")]
        public async Task PlayCommand([Remainder] string song)
            => await RespondAsync(embed: await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, song, Context));
    }
}
