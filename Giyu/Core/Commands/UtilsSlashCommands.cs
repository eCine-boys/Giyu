using Discord;
using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Giyu.Core.Commands
{
    
    public class UtilsSlashCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("youtube", "Cria uma interação com o youtube together.")]
        [RequireUserPermission(GuildPermission.CreateInstantInvite)]
        public async Task YoutubeTogether()
        {
            IVoiceState VoiceState = Context.User as IVoiceState;

            IInviteMetadata Invite = await VoiceState.VoiceChannel.CreateInviteToApplicationAsync(DefaultApplications.Youtube, maxAge: 86400, maxUses: 0, isTemporary: false);

            Color YTColor = new Color(0xFF0000);

            EmbedBuilder embed = new EmbedBuilder()
                .WithAuthor(name: "YouTube Together", iconUrl: "https://cdn.discordapp.com/emojis/749289646097432667.png?v=1")
                .WithColor(YTColor)
                .WithDescription($"__**[Clique aqui para começar.](https://discord.com/invite/{Invite.Code})**__\n" +
                " **Aviso:** Funciona apenas para Desktop.");

            await ReplyAsync(embed: embed.Build());
        }
    }
}
