using Discord;
using Discord.WebSocket;
using Giyu.Core.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Player.Args;
using Victoria.Player;
using Victoria.Responses.Search;
using Victoria.Node;
using Victoria.Resolvers;
using Victoria.Node.EventArgs;

namespace Giyu.Core.Managers
{
    public static class AudioManager
    {
        private static readonly LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        private static readonly MusicModule musicRest = new MusicModule(ConfigManager.Config.BotProviderUri);
        private static bool UserConnectedVoiceChannel(IUser user)
            => !((user as IVoiceState).VoiceChannel is null);
        public static async Task<string> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild)) return "Já estou conectado a um canal de voz.";

            if (voiceState.VoiceChannel is null) return "Você precisa estar em um canal de voz para isso.";

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);

                return $"Conectado em {voiceState.VoiceChannel.Name}";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static Embed ShuffleTracks(IGuild guild, IUser user)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player))
            {
                return EmbedManager.ReplyError("Não foi possível obter o player. \n Use o comando **join** ou toque uma música **play**");
            }


            if(player.Vueue.Count == 0)
            {
                return EmbedManager.ReplySimple("Shuffle", "Não tem músicas na playlist para embaralhar.");
            }

            if (player.Vueue.Count > 0)
            {
                player.Vueue.Shuffle();

                return EmbedManager.ReplySimple("Shuffle", "Playlist embaralhada.");
            }
            else
            {
                return EmbedManager.ReplySimple("Shuffle", "Não tem músicas na playlist para o shuffle.");
            }
        }

        public static async Task<Embed> GetLyrics(string song, IGuild guild, IUser user)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player))
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

        public static Embed BumpTrack(IGuild guild, IUser user, int trackIndex)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player))
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
                        return EmbedManager.ReplySimple("Bump", $"A música já {((player.PlayerState is PlayerState.Playing) ? "tocando" : "no topo")}.");
                }
                else if (trackIndex <= 0)
                {
                    return EmbedManager.ReplySimple("Bump", "Digite um valor válido para o bump, acima de 2.");
                }
            }

            if (player.Vueue.Count < 2)
            {
                return EmbedManager.ReplySimple("Bump", "Não há músicas para subir na playlist.");
            }

            int song = trackIndex - 2;

            try
            {
                var queue = player.Vueue;
                LavaTrack track = player.Vueue.ElementAt(song);
                LavaTrack firstSongInQueue = player.Vueue.First();

                if (track is null)
                    return EmbedManager.ReplySimple("Bump", $"Nenhuma música encontrada na posição específicada: {trackIndex}");

                if (firstSongInQueue is null)
                    return EmbedManager.ReplySimple("Bump", $"Nenhuma música tocando.");

                player.Vueue.Clear();

                player.Vueue.Enqueue(firstSongInQueue);
                player.Vueue.Enqueue(track);
                player.Vueue.Enqueue(queue.RemoveRange(0, 1));

                return EmbedManager.ReplySimple("Bump", $"{track.Title} foi movida para o topo da playlist.");
            }
            catch (Exception ex)
            {
                return EmbedManager.ReplyError(ex.Message);
            }
        }

        private static void TrySkip(LavaPlayer<LavaTrack> player, int skipCount, out IEnumerable<LavaTrack> queue)
        {
            try
            {
                queue = player.Vueue.Skip(skipCount);
            }
            catch (ArgumentNullException)
            {
                queue = null;
            }
        }

        private static string GetQueuePage(LavaPlayer player, int page)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                IEnumerable<LavaTrack> tracksPage = new LavaTrack[] { };

                if (page == 1)
                    tracksPage = player.Vueue.Skip(0).Take(10);
                else
                    tracksPage = player.Vueue.Skip(page * 10 - 10).Take(10);

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

        public static async Task<Embed> SkipToPositionAsync(IGuild guild, IUser user, int skipCount)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplyError("Você precisa estar conectado a um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player))
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
                return EmbedManager.ReplySimple("Skipto", await SkipTrackAsync(guild));
            }


            try
            {
                TrySkip(player, skipCount, out var queue);

                if (queue is null)
                    return EmbedManager.ReplyError("Não foi possível obter o range especificado para a playlist atual.\n/queue para ver a playlist atual.");

                player.Vueue.Clear();

                player.Vueue.Enqueue(queue);

                _ = player.PlayAsync(player.Vueue.First());

                return EmbedManager.ReplySimple("Skip", $"{skipCount} músicas puladas.");
            }
            catch (ArgumentNullException ex)
            {
                LogManager.Log("ArgNullEx - SkipToPositionAsync", ex.Message);
                return EmbedManager.ReplySimple("Skipto", "Range específicado não existe.");
            }
        }

        public static async Task<Embed> PlayAsync(SocketGuildUser user, SocketGuild guild, string query, dynamic context)
        {
            if (!UserConnectedVoiceChannel(user))
                return EmbedManager.ReplySimple("Aviso", "Você precisa estar em um canal de voz para isso.");

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> pl))
            {
                try
                {
                    if (context.Channel is ITextChannel channel)
                        await _lavaNode.JoinAsync(user.VoiceChannel, channel);

                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            try
            {
                LavaPlayer<LavaTrack> player = pl;

                if(player.PlayerState == PlayerState.None)
                {
                    await _lavaNode.JoinAsync(user.VoiceChannel);
                }

                LavaTrack track;

                SearchResponse search = await _lavaNode.SearchAsync(SearchType.YouTube, query);

                Console.WriteLine(search);

                var s1 = await _lavaNode.SearchAsync(SearchType.YouTube, "https://www.youtube.com/watch?v=diW6jXhLE0E&list=PLuszt_6dXbee5fDOW2i5SD3EtvZXnr5NX");

                Console.WriteLine($"{s1.Status}");

                if (search.Status == SearchStatus.NoMatches)
                    return EmbedManager.ReplySimple("Aviso", $"Não foram encontrados resultados para: {query}");

                track = search.Tracks.FirstOrDefault();


                var thumb_image = await track.FetchArtworkAsync();

                if (string.IsNullOrEmpty(thumb_image))
                {
                    thumb_image = $"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg";
                }

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Vueue.Enqueue(track);
                    LogManager.Log("AUDIO", "Música adicionada a playlist.");
                    EmbedBuilder embed_add = new EmbedBuilder();

                    embed_add
                        .WithAuthor(Author =>
                        {
                            Author.WithIconUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150");
                            Author.WithName("Adicionada na playlist");
                        })
                        .WithDescription($"[{track.Title}]({track.Url})")
                        .AddField("Autor", track.Author, true)
                        .AddField("Duração", track.Duration, true)
                        .WithThumbnailUrl(thumb_image)
                        .WithCurrentTimestamp()
                        .WithColor(EmbedManager.GetRandomColor())
                        .WithFooter(x =>
                        {
                            x.IconUrl = guild.IconUrl;
                            x.Text = user.Username;
                        });

                    return embed_add.Build();
                }

                await player.PlayAsync(track);

                LogManager.Log("AUDIO", $"Tocando agora: {track.Title}.");

                EmbedBuilder embed = new EmbedBuilder();

                embed
                    .WithAuthor(Author =>
                    {
                        Author.WithIconUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150");
                        Author.WithName("Tocando agora");
                    })
                    .WithDescription($"[{track.Title}]({track.Url})")
                    .AddField("Autor", track.Author, true)
                    .AddField("Duração", track.Duration, true)
                    .WithThumbnailUrl(thumb_image)
                    .WithCurrentTimestamp()
                    .WithColor(EmbedManager.GetRandomColor())
                    .WithFooter(x =>
                    {
                        x.IconUrl = guild.IconUrl;
                        x.Text = user.Username;
                    });

                return embed.Build();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> LeaveAsync(IGuild guild)
        {
            try
            {
                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

                if (player.PlayerState is PlayerState.None) return $"Não conectado a nenhum canal de voz.";

                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                LogManager.Log("AUDIO", $"Bot saiu do canal de voz.");
                return $"Saindo do canal de voz {player.VoiceChannel?.Name}";
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public static async Task<string> PauseAsync(IGuild guild)
        {
            try
            {
                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

                if (player.PlayerState == PlayerState.None)
                {
                    return "Player não conectado a um canal de voz.";
                }

                if (player.PlayerState is PlayerState.Playing)
                {
                    await player.PauseAsync();
                }

                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"Não tem música ativa para pausar.";
                }

                await player.PauseAsync();
                return $"**Pausado:** {player.Track.Title}.";
            }
            catch (InvalidOperationException)
            {
                return "O Bot não está conectado a um canal de voz para isso.";
            }
        }

        public static async Task<string> StopAsync(IGuild guild)
        {
            try
            {
                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

                if (player is null)
                    return $"Não há música ativa no momento.";

                player.Vueue.Clear();

                await player.StopAsync();

                return $"Audio parado, playlist removida. 👍";
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public static async Task<string> ResumeAsync(IGuild guild)
        {
            try
            {
                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);


                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                return $"**Tocando novamente:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                throw ex;
            }
        }

        public static async Task<Embed> SetVolumeAsync(IGuild guild, ushort volume)
        {
            try
            {

                if (volume > 150 || volume < 0)
                {
                    return EmbedManager.ReplyError("Digite um valor entre 0 e 150 para o volume.");
                }

                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

                if (player.PlayerState != PlayerState.None)
                {
                    await player.SetVolumeAsync(volume);

                    return EmbedManager.ReplySimple("Volume", $"Volume atualizado para {volume}%");
                }
                else
                {
                    return EmbedManager.ReplySimple("Volume", "O Bot precisa estar conectado a um canal de voz para isso.\n use **join**.");
                }

            }
            catch (Exception ex)
            {
                return EmbedManager.ReplySimple("Volume", $"Erro ao atualizar volume: {ex.Message}");
            }
        }

        public static async Task<string> SkipTrackAsync(IGuild guild)
        {

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player))
            {
                return "Não foi possível obter o player.";
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                return "Não se pode pular quando não há nada tocando.";
            }

            try
            {
                (LavaTrack oldTrack, LavaTrack currentTrack) = await player.SkipAsync();

                return $"{oldTrack.Title} foi pulada.";
            }
            catch (Exception ex)
            {
                return $"{ex.Message}";
            }
        }

        public static Embed ListQueue(IGuild guild)
        {
            try
            {
                StringBuilder ListBuilder = new StringBuilder();


                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

                if (player == null)
                    return EmbedManager.ReplySimple("Queue", "Não foi possível obter o player.");

                if (player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    if (player.Vueue.Count < 1 && player.Track != null)
                    {
                        return EmbedManager.ReplySimple("Tocando agora", $"{player.Track.Author} - {player.Track.Title}");
                    }
                    else
                    {
                        //int trackPosNum = 2;
                        EmbedBuilder embed = new EmbedBuilder();

                        embed
                        .WithAuthor(Author =>
                        {
                            Author.WithIconUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150");
                            Author.WithName("Queue");
                        })
                        .WithDescription($"Tocando agora [{player.Track.Title}]({player.Track.Url})")
                        .WithCurrentTimestamp()
                        .WithColor(EmbedManager.GetRandomColor());

                        foreach (LavaTrack track in player.Vueue)
                        {
                            embed.AddField($"[{track.Title}]({track.Url})", track.Author, true);

                            //ListBuilder.Append($"{trackPosNum}: [{track.Title}]({track.Url})\n");
                            //trackPosNum++;
                        }

                        

                        return embed.Build();

                        ComponentBuilder pagesBuilder = new ComponentBuilder()
                            .WithButton("<", $"last_page:{1}")
                            .WithButton(">", $"next_page:{2}");

                        //return EmbedManager.ReplySimple("Queue", $"Tocando agora: [{player.Track.Title}]({player.Track.Url}) \n{ListBuilder}");
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

        public static Embed GetByPage (IGuild guild, int page)
        {
            var sb = GetPageOfQueue(guild, page);

            return EmbedManager.ReplySimple("Queue", sb.ToString());
        }

        public static LavaTrack[] GetPageOfQueue(IGuild guild, int page)
        {
            _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

            try
            {
                LavaTrack[] arrQueue = player.Vueue.ToArray();

                Index index = 0;

                Index index2 = page;

                var sliced = arrQueue.Take(page*10);

                //LavaTrack[] sliced = arrQueue[index..index2];

                if (sliced is null)
                    return null;

                foreach (var item in sliced)
                    LogManager.LogDebug("SLICE", item.Title);

                StringBuilder sb = new StringBuilder();

                foreach (var item2 in sliced)
                    sb.Append(item2.Title);

                return sliced.ToArray();
            } catch(ArgumentNullException ex)
            {
                LogManager.LogError("GetPageOfQueue", ex.Message);
                throw ex;
            }
        }

        public static async Task<dynamic> SearchAsync(string query)
        {
            try
            {

                if (string.IsNullOrEmpty(query))
                    return EmbedManager.ReplySimple("Search", "Digite um valor válido para a pesquisa.");

                StringBuilder ListBuilder = new StringBuilder();

                SelectMenuBuilder selectBuilder = new SelectMenuBuilder()
                    .WithCustomId("select-song")
                    .WithPlaceholder("Selecione uma música")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                SearchResponse searchResponse = await _lavaNode.SearchAsync(Uri.IsWellFormedUriString(query, UriKind.Absolute) ? SearchType.Direct : SearchType.YouTube, query);

                if (searchResponse.Status is SearchStatus.NoMatches)
                    return EmbedManager.ReplySimple("Search", $"Sem resultados para {query}");

                foreach (var track in searchResponse.Tracks)
                {
                    selectBuilder.AddOption(track.Title, track.Id, track.Author);
                }

                var songList = new ComponentBuilder().WithSelectMenu(selectBuilder);

                return songList.Build();
            }
            catch (Exception ex)
            {
                return EmbedManager.ReplySimple("Error", $"{ex.Message}");
            }
        }

        public static Embed Remove(IGuild guild, int songIndex)
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
                _lavaNode.TryGetPlayer(guild, out LavaPlayer<LavaTrack> player);

                if (player == null)
                    return EmbedManager.ReplySimple("Queue", "Não foi possível obter o player.");

                LavaTrack removedTrack = player.Vueue.ToArray()[song];

                if (removedTrack is null)
                    return EmbedManager.ReplyError("Música não encontrada na posição específicada.");

                LavaTrack track = player.Vueue.RemoveAt(song);

                if (track is null)
                    return EmbedManager.ReplySimple("Queue", "Música não encontrada pelo index especificado");

                return EmbedManager.ReplySimple("Queue", $"{track.Title} removida da playlist.");
            }
            else
            {
                return EmbedManager.ReplyError("Player não conectado");
            }
        }

        public static async Task TryAutoPlayNext(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> args)
        {
            IRelatedVideos relatedVideo = await musicRest.GetNextSongsBySongId(args.Player.TextChannel.GuildId, args.Track.Id);

            if (relatedVideo is null)
                return;

            string search = $"https://www.youtube.com/watch?v={relatedVideo.Id}";

            //string search = $"https://www.youtube.com/watch?v={args.Track.Id}&list=RD{args.Track.Id}";

            SearchResponse response = await _lavaNode.SearchAsync(SearchType.YouTubeMusic, search);

            if (response.Status == SearchStatus.NoMatches)
                return;

            LavaTrack _track = response.Tracks.FirstOrDefault();

            await args.Player.PlayAsync(_track);

            EmbedBuilder embed = new EmbedBuilder();

            var thumb_image = await _track.FetchArtworkAsync();

            if (string.IsNullOrEmpty(thumb_image))
            {
                thumb_image = $"https://i.ytimg.com/vi/{_track.Id}/hqdefault.jpg";
            }


            embed
                    .WithAuthor(Author =>
                    {
                        Author.WithIconUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150");
                        Author.WithName("Tocando agora");
                    })
                    .WithDescription($"[{_track.Title}]({_track.Url})")
                    .AddField("Autor", _track.Author, true)
                    .AddField("Duração", _track.Duration, true)
                    .WithThumbnailUrl(thumb_image)
                    .WithCurrentTimestamp()
                    .WithColor(EmbedManager.GetRandomColor())
                .WithFooter(x =>
                {
                    x.IconUrl = args.Player.TextChannel.Guild.IconUrl;
                    x.Text = $"Tocando em {args.Player.VoiceChannel.Name}";
                });

            await args.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        }

        public static async Task TrackEnded(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> args)
        {
            LogManager.Log("DEBUG", args.Reason.ToString());

            if (args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;

            if (!player.Vueue.TryDequeue(out var lavaTrack))
            {
                //await args.Player.TextChannel.SendMessageAsync("Fim da playlist.");
                await TryAutoPlayNext(args);

                return;
            }

            if (lavaTrack is null)
            {
                // Próximo item na playlist não é uma música;
                return;
            }

            await args.Player.PlayAsync(lavaTrack);

            EmbedBuilder embed = new EmbedBuilder();

            var guild = args.Player.TextChannel.Guild;

            var thumb_image = await lavaTrack.FetchArtworkAsync();

            if (string.IsNullOrEmpty(thumb_image))
            {
                thumb_image = $"https://i.ytimg.com/vi/{lavaTrack.Id}/hqdefault.jpg";
            }

            embed
                .WithAuthor(Author =>
                {
                    Author.WithIconUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150");
                    Author.WithName("Tocando agora");
                })
                .WithDescription($"[{lavaTrack.Title}]({lavaTrack.Url})")
                .AddField("Autor", lavaTrack.Author, true)
                .AddField("Duração", lavaTrack.Duration, true)
                .WithThumbnailUrl(thumb_image)
                .WithCurrentTimestamp()
                .WithColor(EmbedManager.GetRandomColor())
                .WithFooter(x =>
                {
                    x.IconUrl = guild.IconUrl;
                    x.Text = $"Tocando agora em {args.Player.VoiceChannel.Name} 🔊";
                });

            await args.Player.TextChannel.SendMessageAsync(embed: embed.Build());
            return;
        }

    }
}
