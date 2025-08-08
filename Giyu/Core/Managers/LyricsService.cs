using Discord;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Giyu.Core.Managers
{
    public class LyricsService
    {
        private readonly LavaNode _lavaNode;

        public LyricsService()
        {
            _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        }

        private bool UserConnectedVoiceChannel(IUser user)
            => !((user as IVoiceState).VoiceChannel is null);

        public async Task<Embed> GetLyrics(string song, IGuild guild, IUser user)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer player))
            {
                return EmbedManager.ReplyError("Não foi possível obter o player. \n Use o comando **join** ou toque uma música **play**");
            }

            string lyrics_genius = await player.Track.FetchLyricsFromGeniusAsync();

            string lyrics_ovh = await player.Track.FetchLyricsFromOvhAsync();

            if(string.IsNullOrEmpty(lyrics_genius) && string.IsNullOrEmpty(lyrics_ovh))
            {
                return EmbedManager.ReplyError("Letra de música não encontrada.");
            }

            return EmbedManager.ReplySimple("Lyrics", string.IsNullOrEmpty(lyrics_genius) ? lyrics_ovh : lyrics_genius);
        }
    }
}
