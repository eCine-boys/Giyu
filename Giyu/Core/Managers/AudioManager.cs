using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace Giyu.Core.Managers
{
    public static class AudioManager
    {
        private static readonly LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        public static async Task<string> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
        {
            if (_lavaNode.HasPlayer(guild)) return "Já estou conectado a um canal de voz.";

            if (voiceState.VoiceChannel is null) return "Você precisa estar em um canal de voz para isso.";

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);

                return $"Conectado em {voiceState.VoiceChannel.Name}";
            } catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<Embed> PlayAsync(IGuild guild, string id)
        {
            try
            {

                LavaPlayer player = _lavaNode.GetPlayer(guild);

                if (player is null)
                    return EmbedManager.ReplyError("Player não conectado.");

                LavaTrack track;

                SearchResponse data = await _lavaNode.SearchYouTubeAsync(id);

                if (data.Status == SearchStatus.NoMatches)
                    return EmbedManager.ReplySimple("Aviso", $"Não foram encontrados resultados para: {id}");

                track = data.Tracks.FirstOrDefault();

                var thumb_image = await track.FetchArtworkAsync();

                if (string.IsNullOrEmpty(thumb_image))
                {
                    thumb_image = $"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg";
                }

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
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
                            x.Text = guild.Name;
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
                        x.Text = guild.Name;
                    });

                return embed.Build();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<Embed> PlayAsync(SocketGuildUser user, SocketGuild guild, string query, dynamic context)
        {
            if (user.VoiceChannel is null)
                return EmbedManager.ReplySimple("Aviso", "Você precisa estar em um canal de voz para isso.");

            if (!_lavaNode.HasPlayer(guild))
            {
                try
                {
                    if(context.Channel is ITextChannel channel)
                        await _lavaNode.JoinAsync(user.VoiceChannel, channel);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);

                LavaTrack track;

                SearchResponse search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchYouTubeAsync(query)
                    : await _lavaNode.SearchAsync(SearchType.YouTube, query);

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
                    player.Queue.Enqueue(track);
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
                LavaPlayer player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                LogManager.Log("AUDIO", $"Bot saiu do canal de voz.");
                return $"Saindo do canal de voz {player.VoiceChannel.Name}";
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
                LavaPlayer player = _lavaNode.GetPlayer(guild);

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
                var player = _lavaNode.GetPlayer(guild);

                if (player is null)
                    return $"Não há música ativa no momento.";

                player.Queue.Clear();

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
                LavaPlayer player = _lavaNode.GetPlayer(guild);

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

                if(volume > 150 || volume < 0)
                {
                    return EmbedManager.ReplyError("Digite um valor entre 0 e 150 para o volume.");
                }

                LavaPlayer player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState != PlayerState.None)
                {
                    await player.UpdateVolumeAsync(volume);

                    return EmbedManager.ReplySimple("Volume", $"Volume atualizado para {volume}%");
                } else
                {
                    return EmbedManager.ReplySimple("Volume", "O Bot precisa estar conectado a um canal de voz para isso.\n use **join**.");
                }

            } catch(Exception ex)
            {
                return EmbedManager.ReplySimple("Volume", $"Erro ao atualizar volume: {ex.Message}");
            }
        }

        public static async Task<string> SkipTrackAsync(IGuild guild)
        {

            if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer player))
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

        public static Embed ListAsync(IGuild guild)
        {
            try
            {
                StringBuilder ListBuilder = new StringBuilder();

                LavaPlayer player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return EmbedManager.ReplySimple("Queue", "Não foi possível obter o player.");

                if(player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    if(player.Queue.Count < 1 && player.Track != null)
                    {
                        return EmbedManager.ReplySimple("Tocando agora", $"{player.Track.Author} - {player.Track.Title}");
                    }
                    else
                    {
                        int trackPosNum = 2;
                        foreach(LavaTrack track in player.Queue)
                        {
                            ListBuilder.Append($"{trackPosNum}: [{track.Title}]({track.Url})\n");
                            trackPosNum++;
                        }

                        return EmbedManager.ReplySimple("Queue", $"Tocando agora: [{player.Track.Title}]({player.Track.Url}) \n{ListBuilder}");
                    }
                }
                else
                {
                    return EmbedManager.ReplySimple("Erro", "O Bot deve estar parado ou pausado para isso.");
                }

            }
            catch(Exception ex)
            {
                return EmbedManager.ReplySimple("Error", $"{ex.Message}");
            }
        }

        public static async Task<dynamic> SearchAsync(SocketInteractionContext context, string query)
        {
            try
            {
                StringBuilder ListBuilder = new StringBuilder();

                var selectBuilder = new SelectMenuBuilder()
                    .WithCustomId("select-song")
                    .WithPlaceholder("Selecione uma música")
                    .WithMinValues(1)
                    .WithMaxValues(1);

                var searchResponse = await _lavaNode.SearchYouTubeAsync(query);

                if (searchResponse.Status is SearchStatus.NoMatches)
                    return EmbedManager.ReplySimple("Erro", $"Sem resultados para {query}");

                foreach(var track in searchResponse.Tracks)
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

            if(_lavaNode.HasPlayer(guild))
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
            } else
            {
                return EmbedManager.ReplyError("Player não conectado");
            }
        }

        public static async Task TryAutoPlayNext(TrackEndedEventArgs args)
        {
            string search = $"https://www.youtube.com/watch?v={args.Track.Id}&list=RD{args.Track.Id}";

            SearchResponse response = await _lavaNode.SearchAsync(SearchType.YouTubeMusic, search);

            if (response.Status == SearchStatus.NoMatches)
                return;

            LavaTrack _track = response.Tracks.ToArray()[1];

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

        public static async Task TrackEnded(TrackEndedEventArgs args)
        {
            LogManager.Log("DEBUG", args.Reason.ToString());

            if (args.Reason != TrackEndReason.Finished)
            {
                return;
            }

            var player = args.Player;

            if (!player.Queue.TryDequeue(out var lavaTrack))
            {
                //await args.Player.TextChannel.SendMessageAsync("Fim da playlist.");
                return;
            }

            if(lavaTrack is null)
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
