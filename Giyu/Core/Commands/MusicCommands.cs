using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Giyu.Core.Managers;
using System.Threading.Tasks;

namespace Giyu.Core.Commands
{
    [Name("Music")]
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {
        [Command("join")]
        [Summary("Faz o bot entrar no canal de voz.")]
        public async Task JoinCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));

        [Command("play")]
        [Summary("Chama o bot para tocar uma música no canal de voz.")]
        public async Task PlayCommand([Remainder] string search)
            => await Context.Channel.SendMessageAsync(await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));

        [Command("leave")]
        [Summary("Faz o bot sair do canal de voz.")]
        public async Task LeaveCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.LeaveAsync(Context.Guild));

        [Command("skip")]
        [Summary("Pula a música atual.")]
        public async Task SkipCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.SkipTrackAsync(Context.Guild));

        [Command("pause")]
        [Summary("Pausa a música atual.")]
        public async Task PauseCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.PauseAsync(Context.Guild));

        [Command("resume")]
        [Summary("Pula a música atual.")]
        public async Task ResumeCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.ResumeAsync(Context.Guild));
    }
}
