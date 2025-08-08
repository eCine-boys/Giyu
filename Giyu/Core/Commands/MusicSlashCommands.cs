using Discord;
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
        private readonly PlaybackService _playback = ServiceManager.GetService<PlaybackService>();
        private readonly QueueService _queue = ServiceManager.GetService<QueueService>();
        private readonly LyricsService _lyrics = ServiceManager.GetService<LyricsService>();

        [SlashCommand("play", "Toca uma música")]
        public async Task PlayCommand([Remainder] string song)
            => await RespondAsync(embed: await _playback.PlayAsync(Context.User as SocketGuildUser, Context.Guild, song, Context));

        [SlashCommand("pause", "Pausa a música atual, caso esteja tocando uma.")]
        public async Task PauseCommand()
            => await RespondAsync(await _playback.PauseAsync(Context.Guild));

        [SlashCommand("leave", "Faz o bot sair do canal de voz")]
        public async Task LeaveCommand()
            => await RespondAsync(await _playback.LeaveAsync(Context.Guild));

        [SlashCommand("resume", "Volta a tocar uma música que estava pausada, caso haja uma.")]
        public async Task ResumeCommand()
            => await RespondAsync(await _playback.ResumeAsync(Context.Guild));

        [SlashCommand("skip", "Pula a música atual")]
        public async Task SkipCommand()
            => await RespondAsync(await _playback.SkipTrackAsync(Context.Guild));

        [SlashCommand("stop", "Para a música e limpa a playlist.")]
        public async Task StopCommand()
            => await RespondAsync(await _playback.StopAsync(Context.Guild));

        [SlashCommand("queue", "Lista as músicas da playlist atual caso haja uma.")]
        public async Task ListCommand()
            => await RespondAsync(embed: _queue.ListQueue(Context.Guild));

        [SlashCommand("search", "Pesquisa uma música por uma palavra-chave.")]
        public async Task SearchCommand([Remainder] string search) {
            dynamic resp = await _playback.SearchAsync(search);

            if(resp is MessageComponent _component)
            {
                await RespondAsync(components: _component);
            } else if(resp is Embed _embed)
            {
                await RespondAsync(embed: _embed);
            }
        }

        [SlashCommand("remove", "Remove uma música da playlist de acordo com a posição passada.")]
        public async Task RemoveCommand([Remainder] int position)
            => await RespondAsync(embed: _queue.Remove(Context.Guild, position));

        [Alias("vol")]
        [SlashCommand("volume", "Altera o volume da música em reprodução.")]
        public async Task VolumeCommand([Remainder] ushort volume)
            => await RespondAsync(embed: await _playback.SetVolumeAsync(Context.Guild, volume));

        [SlashCommand("skipto", "Pula as músicas até uma posição específicada na playlist.")]
        public async Task SkipToCommand([Remainder] int skipCount)
            => await RespondAsync(embed: await _queue.SkipToPositionAsync(Context.Guild, Context.User, skipCount));

        [SlashCommand("bump", "Move uma música da playlist para o topo da posição.")]
        public async Task BumpCommand([Remainder] int position)
            => await RespondAsync(embed: _queue.BumpTrack(Context.Guild, Context.User, position));

        [SlashCommand("shuffle", "Embaralha as músicas da playlist atual.")]
        public async Task ShuffleCommand()
            => await RespondAsync(embed: _queue.ShuffleTracks(Context.Guild, Context.User));

        [SlashCommand("lyrics", "Procura pela letra de uma música.")]
        public async Task LyricsCommand([Remainder] string song)
            => await RespondAsync(embed: await _lyrics.GetLyrics(song, Context.Guild, Context.User));
    }

}
