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
        [SlashCommand("play", "Toca uma música")]
        public async Task PlayCommand([Remainder] string song)
            => await RespondAsync(embed: await AudioManager.PlayAsync(Context.User as SocketGuildUser, Context.Guild, song, Context));

        [SlashCommand("pause", "Pausa a música atual, caso esteja tocando uma.")]
        public async Task PauseCommand()
            => await RespondAsync(await AudioManager.PauseAsync(Context.Guild));

        [SlashCommand("leave", "Faz o bot sair do canal de voz")]
        public async Task LeaveCommand()
            => await RespondAsync(await AudioManager.LeaveAsync(Context.Guild));

        [SlashCommand("resume", "Volta a tocar uma música que estava pausada, caso haja uma.")]
        public async Task ResumeCommand()
            => await RespondAsync(await AudioManager.ResumeAsync(Context.Guild));

        [SlashCommand("skip", "Pula a música atual")]
        public async Task SkipCommand()
            => await RespondAsync(await AudioManager.SkipTrackAsync(Context.Guild));

        [SlashCommand("stop", "Para a música e limpa a playlist.")]
        public async Task StopCommand()
            => await RespondAsync(await AudioManager.StopAsync(Context.Guild));

        [SlashCommand("queue", "Lista as músicas da playlist atual caso haja uma.")]
        public async Task ListCommand(uint page)
            => await RespondAsync(embed: AudioManager.ListQueue(Context.Guild, page));

        [SlashCommand("search", "Pesquisa uma música por uma palavra-chave.")]
        public async Task SearchCommand([Remainder] string search) {
            dynamic resp = await AudioManager.SearchAsync(search);

            if(resp is MessageComponent _component)
            {
                await RespondAsync(components: _component);
                return;
            } else if(resp is Embed _embed)
            {

                await RespondAsync(embed: _embed);
            }
        }

        [SlashCommand("remove", "Remove uma música da playlist de acordo com a posição passada.")]
        public async Task RemoveCommand([Remainder] int position)
            => await RespondAsync(embed: AudioManager.Remove(Context.Guild, position));

        [Alias("vol")]
        [SlashCommand("volume", "Altera o volume da música em reprodução.")]
        public async Task VolumeCommand([Remainder] ushort volume)
            => await RespondAsync(embed: await AudioManager.SetVolumeAsync(Context.Guild, volume));

        [SlashCommand("skipto", "Pula as músicas até uma posição específicada na playlist.")]
        public async Task SkipToCommand([Remainder] int skipCount)
            => await RespondAsync(embed: await AudioManager.SkipToPositionAsync(Context.Guild, Context.User, skipCount));

        [SlashCommand("bump", "Move uma música da playlist para o topo da posição.")]
        public async Task BumpCommand([Remainder] int position)
            => await RespondAsync(embed: AudioManager.BumpTrack(Context.Guild, Context.User, position));

        [SlashCommand("shuffle", "Embaralha as músicas da playlist atual.")]
        public async Task ShuffleCommand()
            => await RespondAsync(embed: AudioManager.ShuffleTracks(Context.Guild, Context.User));
    }

}
