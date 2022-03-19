
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
    }
}
