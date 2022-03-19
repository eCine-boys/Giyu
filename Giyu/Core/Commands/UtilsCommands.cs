
using Discord;
using Discord.Commands;
using Giyu.Core.Managers;
using System.Threading.Tasks;


namespace Giyu.Core.Commands
{
    public class UtilsCommands : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeChatCommand([Remainder] uint amount)
            => await Context.Channel.SendMessageAsync(await UtilsManager.PurgeChatAsync(Context.Channel as ITextChannel, amount));

        [Command("youtube")]
        [RequireUserPermission(GuildPermission.CreateInstantInvite)]
        public async Task YoutubeTogether()
            => await Context.Channel.SendMessageAsync(embed: await UtilsManager.YoutubeTogetherAsync(Context.User as IVoiceState));

    }
}
