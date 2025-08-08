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
        private readonly PlaybackService _playback = ServiceManager.GetService<PlaybackService>();
        private readonly QueueService _queue = ServiceManager.GetService<QueueService>();

        [Command("join")]
        [Summary("Faz o bot entrar no canal de voz.")]
        public async Task JoinCommand()
            => await Context.Channel.SendMessageAsync(await _playback.JoinAsync(Context.Guild, Context.User as IVoiceState, Context.Channel as ITextChannel));


        [Alias("p")]
        [Command("play")]
        [Summary("Chama o bot para tocar uma música no canal de voz.")]
        public async Task PlayCommand([Remainder] string search)
            => await Context.Channel.SendMessageAsync(embed: await _playback.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, Context));


        [Alias("quit", "dc")]
        [Command("leave")]
        [Summary("Faz o bot sair do canal de voz.")]
        public async Task LeaveCommand()
            => await Context.Channel.SendMessageAsync(await _playback.LeaveAsync(Context.Guild));

        [Alias("sk", "fs")]
        [Command("skip")]
        [Summary("Pula a música atual.")]
        public async Task SkipCommand()
            => await Context.Channel.SendMessageAsync(await _playback.SkipTrackAsync(Context.Guild));

        [Command("pause")]
        [Summary("Pausa a música atual.")]
        public async Task PauseCommand()
            => await Context.Channel.SendMessageAsync(await _playback.PauseAsync(Context.Guild));

        [Command("resume")]
        [Summary("Pula a música atual.")]
        public async Task ResumeCommand()
            => await Context.Channel.SendMessageAsync(await _playback.ResumeAsync(Context.Guild));

        [Command("stop")]
        [Summary("Para a música e limpa a playlist.")]
        public async Task StopCommand()
            => await Context.Channel.SendMessageAsync(await _playback.StopAsync(Context.Guild));

        [Alias("q", "pl")]
        [Command("queue")]
        [Summary("Lista as músicas da playlist atual caso haja uma.")]
        public async Task ListCommand()
            => await Context.Channel.SendMessageAsync(embed: _queue.ListQueue(Context.Guild));
        
        [Alias("vol")]
        [Command("volume")]
        public async Task VolumeCommand([Remainder] ushort volume)
            => await Context.Channel.SendMessageAsync(embed: await _playback.SetVolumeAsync(Context.Guild, volume));

        [Alias("bmp")]
        [Command("bump")]
        [Summary("Move uma música da playlist para o topo da posição.")]
        public async Task BumpCommand([Remainder] int position)
            => await Context.Channel.SendMessageAsync(embed: _queue.BumpTrack(Context.Guild, Context.User, position));

        [Alias("sh")]
        [Command("shuffle")]
        [Summary("Embaralha as músicas da playlist atual.")]
        public async Task ShuffleCommand()
        {
            _queue.ShuffleTracks(Context.Guild, Context.User);

            await Context.Message.AddReactionAsync(Emote.Parse("👍"));
        }
    }
}
