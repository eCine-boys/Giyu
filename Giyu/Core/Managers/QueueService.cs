using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;

namespace Giyu.Core.Managers
{
    public class QueueService
    {
        private readonly LavaNode _lavaNode;

        public QueueService()
        {
            _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        }

        private bool UserConnectedVoiceChannel(IUser user)
            => !((user as IVoiceState).VoiceChannel is null);

        public Embed ShuffleTracks(IGuild guild, IUser user)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer player))
            {
                return EmbedManager.ReplyError("Não foi possível obter o player. \n Use o comando **join** ou toque uma música **play**");
            }

            if(player.Queue.Count == 0)
            {
                return EmbedManager.ReplySimple("Shuffle", "Não tem músicas na playlist para embaralhar.");
            }

            if (player.Queue.Count > 0)
            {
                player.Queue.Shuffle();

                return EmbedManager.ReplySimple("Shuffle", "Playlist embaralhada.");
            }
            else
            {
                return EmbedManager.ReplySimple("Shuffle", "Não tem músicas na playlist para o shuffle.");
            }
        }

        public Embed BumpTrack(IGuild guild, IUser user, int trackIndex)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer player))
            {
                return EmbedManager.ReplyError("Não foi possível obter o player. \n Use o comando **join** ou toque uma música **play**");
            }

            if (trackIndex <= 2)
            {
                if (trackIndex == 2)
                {
                    return EmbedManager.ReplySimple("Bump", "A música já está no topo da playlist.");
                }
                else if (trackIndex == 1)
                {
                    if (player.PlayerState is PlayerState.Playing)
                        return EmbedManager.ReplySimple("Bump", $"A música já {(player.PlayerState is PlayerState.Playing ? "tocando" : "no topo")}.");
                }
                else if (trackIndex <= 0)
                {
                    return EmbedManager.ReplySimple("Bump", "Digite um valor válido para o bump, acima de 2.");
                }
            }

            if (player.Queue.Count < 2)
            {
                return EmbedManager.ReplySimple("Bump", "Não há músicas para subir na playlist.");
            }

            int song = trackIndex - 2;

            try
            {
                var queue = player.Queue;
                LavaTrack track = player.Queue.ElementAt(song);
                LavaTrack firstSongInQueue = player.Queue.First();

                if (track is null)
                    return EmbedManager.ReplySimple("Bump", $"Nenhuma música encontrada na posição específicada: {trackIndex}");

                if (firstSongInQueue is null)
                    return EmbedManager.ReplySimple("Bump", $"Nenhuma música tocando.");

                player.Queue.Clear();

                player.Queue.Enqueue(firstSongInQueue);
                player.Queue.Enqueue(track);
                player.Queue.Enqueue(queue.RemoveRange(0, 1));

                return EmbedManager.ReplySimple("Bump", $"{track.Title} foi movida para o topo da playlist.");
            }
            catch (Exception ex)
            {
                return EmbedManager.ReplyError(ex.Message);
            }
        }

        private void TrySkip(LavaPlayer player, int skipCount, out IEnumerable<LavaTrack> queue)
        {
            try
            {
                queue = player.Queue.Skip(skipCount);
            }
            catch (ArgumentNullException)
            {
                queue = null;
            }
        }

        private string GetQueuePage(LavaPlayer player, int page)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                IEnumerable<LavaTrack> tracksPage = new LavaTrack[] { };

                if (page == 1)
                    tracksPage = player.Queue.Skip(0).Take(10);
                else
                    tracksPage = player.Queue.Skip(page * 10 - 10).Take(10);

                foreach(LavaTrack track in tracksPage)
                {
                    sb.Append(track.Title);
                }

                return sb.ToString();
            }
            catch (ArgumentNullException)
            {
                return string.Empty;
            }
        }

        public async Task<Embed> SkipToPositionAsync(IGuild guild, IUser user, int skipCount)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer player))
            {
                return EmbedManager.ReplyError("Não foi possível obter o player. \n Use o comando **join** ou toque uma música **play**");
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return EmbedManager.ReplyError("Não se pode pular quando não há nada tocando.");
            }

            if (skipCount < 0)
            {
                return EmbedManager.ReplyError("Valor inválido para skip.");
            }
            else if (skipCount == 1)
            {
                return EmbedManager.ReplyError("Não se pode pular para a música atual. digite um valor válido **acima de 2**\n/queue para ver a playlist atual.");
            }
            else if (skipCount == 2)
            {
                var playback = ServiceManager.GetService<PlaybackService>();
                return EmbedManager.ReplySimple("Skipto", await playback.SkipTrackAsync(guild));
            }


            try
            {
                TrySkip(player, skipCount, out var queue);

                if (queue is null)
                    return EmbedManager.ReplyError("Não foi possível obter o range especificado para a playlist atual.\n/queue para ver a playlist atual.");

                player.Queue.Clear();

                player.Queue.Enqueue(queue);

                _ = player.PlayAsync(player.Queue.First());

                return EmbedManager.ReplySimple("Skip", $"{skipCount} músicas puladas.");
            }
            catch (ArgumentNullException ex)
            {
                LogManager.Log("ArgNullEx - SkipToPositionAsync", ex.Message);
                return EmbedManager.ReplySimple("Skipto", "Range específicado não existe.");
            }
        }

        public Embed ListQueue(IGuild guild)
        {
            try
            {
                StringBuilder ListBuilder = new StringBuilder();

                LavaPlayer player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return EmbedManager.ReplySimple("Queue", "Não foi possível obter o player.");

                if (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    if (player.Queue.Count < 1 && player.Track != null)
                    {
                        return EmbedManager.ReplySimple("Tocando agora", $"{player.Track.Author} - {player.Track.Title}");
                    }
                    else
                    {
                        EmbedBuilder embed = new EmbedBuilder();

                        embed
                        .WithAuthor(Author =>
                        {
                            Author.WithIconUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150");
                            Author.WithName("Playlist");
                        })
                        .WithDescription("Lista de músicas adicionadas à playlist")
                        .WithColor(EmbedManager.GetRandomColor())
                        .WithCurrentTimestamp();

                        int trackPosNum = 2;

                        foreach (var track in player.Queue)
                        {
                            embed.AddField(trackPosNum.ToString(), track.Title);
                            trackPosNum++;
                        }

                        return embed.Build();
                    }

                }
                else
                {
                    return EmbedManager.ReplySimple("Erro", "O Bot deve estar tocando ou pausado para isso.");
                }

            }
            catch (Exception ex)
            {
                return EmbedManager.ReplySimple("Error", $"{ex.Message}");
            }
        }

        public Embed Remove(IGuild guild, int songIndex)
        {
            if (songIndex < 2)
            {
                if (songIndex == 1) // Removendo música atual;
                {
                    return EmbedManager.ReplySimple("Queue", "Música atual foi removida.");
                }
                else if (songIndex == 0) // Removendo música atual;
                {
                    return EmbedManager.ReplySimple("Queue", "Valor inválido: 0");
                }
                else
                {
                    return EmbedManager.ReplyError("Digite um valor acima de 0 para remover da playlist.");
                }
            }


            int song = songIndex - 2;

            if (_lavaNode.HasPlayer(guild))
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return EmbedManager.ReplySimple("Queue", "Não foi possível obter o player.");

                LavaTrack removedTrack = player.Queue.ToArray()[song];

                if (removedTrack is null)
                    return EmbedManager.ReplyError("Música não encontrada na posição específicada.");

                LavaTrack track = player.Queue.RemoveAt(song);

                if (track is null)
                    return EmbedManager.ReplySimple("Queue", "Música não encontrada pelo index especificado");

                return EmbedManager.ReplySimple("Queue", $"{track.Title} removida da playlist.");
            }
            else
            {
                return EmbedManager.ReplyError("Player não conectado");
            }
        }

        public LavaTrack[] GetPageOfQueue(IGuild guild, int page)
        {
            LavaPlayer player = _lavaNode.GetPlayer(guild);

            try
            {
                LavaTrack[] arrQueue = player.Queue.ToArray();

                Index index = 0;

                Index index2 = page;

                LavaTrack[] sliced = arrQueue[index..index2];

                if (sliced is null)
                    return null;

                foreach (var item in sliced)
                    LogManager.LogDebug("SLICE", item.Title);

                return sliced;
            } catch(ArgumentNullException ex)
            {
                LogManager.LogError("GetPageOfQueue", ex.Message);
                throw ex;
            }
        }
    }
}
