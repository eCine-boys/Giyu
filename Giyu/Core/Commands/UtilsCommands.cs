
using Discord;
using Discord.Commands;
using System.Threading.Tasks;


namespace Giyu.Core.Commands
{
    public class UtilsCommands : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeChat(uint amount)
        {
            var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).FlattenAsync();

            foreach(var message in messages)
            {
                await Task.Delay(500);
                await Context.Channel.DeleteMessageAsync(message.Id);
            }

            await ReplyAsync($"{amount} apagadas.");
        }

        [Command("youtube")]
        [RequireUserPermission(GuildPermission.CreateInstantInvite)]
        public async Task YoutubeTogether()
        {
            IVoiceState VoiceState = Context.User as IVoiceState;

            IInviteMetadata Invite = await VoiceState.VoiceChannel.CreateInviteToApplicationAsync(DefaultApplications.Youtube, maxAge: 86400, maxUses: 0, isTemporary: false);

            Color YTColor = new Color(0xFF0000);

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(name: "YouTube Together", url: "https://cdn.discordapp.com/emojis/749289646097432667.png?v=1")
                .WithColor(YTColor)
                .WithDescription("Clique em *Join YouTube Together* para começar.\n" +
                $"__**[Join YouTube Together](https://discord.com/invite/{Invite.Code})**__\n" +
                " **Aviso:** Funciona apenas para Desktop.");

            await ReplyAsync(embed: embed.Build());
        }

    }
}
