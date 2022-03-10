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


        [Alias("p")]
        [Command("play")]
        [Summary("Chama o bot para tocar uma música no canal de voz.")]
        public async Task PlayCommand([Remainder] string search)
            => await Context.Channel.SendMessageAsync(embed: await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, Context));


        [Alias("quit", "dc")]
        [Command("leave")]
        [Summary("Faz o bot sair do canal de voz.")]
        public async Task LeaveCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.LeaveAsync(Context.Guild));

        [Alias("sk", "fs")]
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

        [Command("stop")]
        [Summary("Para a música e limpa a playlist.")]
        public async Task StopCommand()
            => await Context.Channel.SendMessageAsync(await AudioManager.StopAsync(Context.Guild));

        [Alias("q", "pl")]
        [Command("queue")]
        [Summary("Lista as músicas da playlist atual caso haja uma.")]
        public async Task List()
            => await Context.Channel.SendMessageAsync(embed: await AudioManager.ListAsync(Context));
        
        [Alias("vol")]
        [Command("volume")]
        public async Task VolumeCommand([Remainder] ushort volume)
            => await Context.Channel.SendMessageAsync(embed: await AudioManager.SetVolumeAsync(Context.Guild, volume));

    }
}
