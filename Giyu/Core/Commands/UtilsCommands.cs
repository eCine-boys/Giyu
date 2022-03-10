
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Giyu.Core.Commands
{
    public class UtilsCommands : ModuleBase<SocketCommandContext>
    {
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task PurgeChat(uint amount)
        {
            //var messages = await Context.Channel.GetMessagesAsync((int)amount + 1).Flatten();

            //await Context.Channel.DeleteMessageAsync(messages);
            //await ReplyAsync($"{amount} apagadas.");
        }
    }
}
