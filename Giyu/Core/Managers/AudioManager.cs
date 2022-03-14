using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;
using Victoria.Filters;
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
                return $"ERROR\n{ex.Message}";
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
                    if(context is SocketCommandContext CommandCtx)
                    {
                        if (!!(CommandCtx.Channel is ITextChannel channel))
                        {
                            await _lavaNode.JoinAsync(user.VoiceChannel, channel);
                        }
                    } else if(context is SocketInteractionContext InteractionCtx)
                    {
                        if (!!(InteractionCtx.Channel is ITextChannel channel))
                        {
                            await _lavaNode.JoinAsync(user.VoiceChannel, channel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    return EmbedManager.ReplySimple("Erro", $"{ex.Message}");
                }
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);

                LavaTrack track;

                SearchResponse search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchYouTubeAsync(query)
                    : await _lavaNode.SearchAsync(SearchType.YouTubeMusic, query);

                if (search.Status == SearchStatus.NoMatches)
                    return EmbedManager.ReplySimple("Aviso", $"Não foram encontrados resultados para: {query}");

                track = search.Tracks.FirstOrDefault();

                var thumb_image = await track.FetchArtworkAsync();

                if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    LogManager.Log("AUDIO", "Música adicionada a playlist.");
                    EmbedBuilder embed_add = new EmbedBuilder();

                    if (string.IsNullOrEmpty(thumb_image))
                    {
                        thumb_image = $"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg";
                    }

                    embed_add
                        .WithTitle(track.Title)
                        .WithUrl(track.Url)
                        .AddField("Autor", track.Author, true)
                        .AddField("Duração", track.Duration, true)
                        //.AddField("Link", track.Url, true)
                        .WithThumbnailUrl(thumb_image)
                        .WithImageUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150")
                        //.WithImageUrl(await track.FetchArtworkAsync())
                        .WithCurrentTimestamp()
                        .WithColor(EmbedManager.GetRandomColor())
                        .WithFooter(x =>
                        {
                            x.IconUrl = guild.IconUrl;
                            x.Text = "Adicionada na playlist";
                        });

                    return embed_add.Build();
                }

                await player.PlayAsync(track);
                LogManager.Log("AUDIO", $"Tocando agora: {track.Title}.");

                EmbedBuilder embed = new EmbedBuilder();

                if (string.IsNullOrEmpty(thumb_image))
                {
                    thumb_image = $"https://i.ytimg.com/vi/{track.Id}/hqdefault.jpg";
                }

                embed
                    .WithTitle(track.Title)
                    .WithUrl(track.Url)
                    .AddField("Autor", track.Author, true)
                    .AddField("Duração", track.Duration, true)
                        //.AddField("Link", track.Url)
                    .WithThumbnailUrl(thumb_image)
                        //.WithImageUrl(await track.FetchArtworkAsync())
                    .WithImageUrl("https://i0.wp.com/minecraftmodpacks.net/wp-content/uploads/2017/11/a47764f58bdb6731fd0a903697af9d98.png?resize=150%2C150")
                    .WithCurrentTimestamp()
                    .WithColor(EmbedManager.GetRandomColor())
                    .WithFooter(x =>
                    {
                        x.IconUrl = guild.IconUrl;
                        x.Text = "Tocando agora";
                    });

                return embed.Build();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /*
        public static async Task<Embed> PlayAsync(SocketGuildUser user, IGuild guild, string query, SocketCommandContext context)
        {
            if (user.VoiceChannel is null) 
                return EmbedManager.ReplySimple("Aviso", "Você precisa estar em um canal de voz para isso.");

            if (!_lavaNode.HasPlayer(guild))
            {
                try
                {
                    if (!!(context.Channel is ITextChannel channel))
                    {
                        await _lavaNode.JoinAsync(user.VoiceChannel, channel);

                        Emoji likeEmote = new Emoji("👍");

                        _ = context.Message.AddReactionAsync(likeEmote);
                    }
                }
                catch (Exception ex)
                {
                    return EmbedManager.ReplySimple("Erro", $"{ex.Message}");
                }
            }

            try
            {
                LavaPlayer player = _lavaNode.GetPlayer(guild);

                LavaTrack track;

                SearchResponse search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(SearchType.YouTubeMusic, query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                if (search.Status == SearchStatus.NoMatches)
                    return EmbedManager.ReplySimple("Aviso", $"Não foram encontrados resultados para: {query}");

                track = search.Tracks.FirstOrDefault();

                if(player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    LogManager.Log("AUDIO", "Música adicionada a playlist.");
                    EmbedBuilder embed_add = new EmbedBuilder();

                    embed_add
                        .WithTitle(track.Title)
                        .AddField("Autor", track.Author)
                        .AddField("Duração", track.Duration)
                        .AddField("Link", track.Url)
                        .WithThumbnailUrl(await track.FetchArtworkAsync())
                        //.WithImageUrl(await track.FetchArtworkAsync())
                        .WithCurrentTimestamp()
                        .WithColor(EmbedManager.GetRandomColor())
                        .WithFooter(x =>
                        {
                            x.IconUrl = guild.IconUrl;
                            x.Text = "Adicionada na playlist";
                        });

                    return embed_add.Build();
                }

                await player.PlayAsync(track);
                LogManager.Log("AUDIO", $"Tocando agora: {track.Title}.");

                EmbedBuilder embed = new EmbedBuilder();

                embed
                    .WithTitle(track.Title)
                    .AddField("Autor", track.Author)
                    .AddField("Duração", track.Duration)
                    .AddField("Link", track.Url)
                    .WithThumbnailUrl(await track.FetchArtworkAsync())
                    //.WithImageUrl(await track.FetchArtworkAsync())
                    .WithCurrentTimestamp()
                    .WithColor(EmbedManager.GetRandomColor())
                    .WithFooter(x =>
                    {
                        x.IconUrl = guild.IconUrl;
                        x.Text = "Tocando agora";
                    });

                return embed.Build();
            } 
            catch (Exception ex)
            {
                throw ex;
            }

        }

        */

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
                return $"Error: {ex.Message}";
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

                await player.StopAsync();

                return $"Audio parado, playlist removida. 👍";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
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
            catch (InvalidOperationException)
            {
                return "O Bot não está conectado a um canal de voz para isso.";
            }
        }

        public static async Task<Embed> SetVolumeAsync(IGuild guild, ushort volume)
        {
            try
            {
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
            try
            {
                var player = _lavaNode.GetPlayer(guild);

                if (player == null)
                    return $"Não foi possível obter o player.";

                if (player.Queue.Count < 1)
                {
                    return $"Não não há próximas músicas para pular.";
                }
                else
                {
                    try
                    {
                        var currentTrack = player.Track;
                        await player.SkipAsync();
                        return $"{currentTrack.Title} foi pulada.";
                    }
                    catch (Exception ex)
                    {
                        return $"Skip {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Skip {ex.Message}";
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
                            ListBuilder.Append($"{trackPosNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
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

        public static async Task TryAutoPlayNext(TrackEndedEventArgs args)
        {
            string search = $"https://www.youtube.com/watch?v={args.Track.Id}&list=RD{args.Track.Id}";

            SearchResponse response = await _lavaNode.SearchAsync(SearchType.YouTubeMusic, search);

            if (response.Status == SearchStatus.NoMatches)
                return;

            LavaTrack _track = response.Tracks.ToArray()[1];

            await args.Player.PlayAsync(_track);

            EmbedBuilder embed = new EmbedBuilder();

            embed
                .WithTitle(_track.Title)
                .AddField("Autor", _track.Author)
                .AddField("Duração", _track.Duration)
                .AddField("Link", _track.Url)
                .WithThumbnailUrl(await _track.FetchArtworkAsync())
                //.WithImageUrl(await track.FetchArtworkAsync())
                .WithCurrentTimestamp()
                .WithColor(EmbedManager.GetRandomColor())
                .WithFooter(x =>
                {
                    x.IconUrl = args.Player.TextChannel.Guild.IconUrl;
                    x.Text = "Tocando agora";
                });

            await args.Player.TextChannel.SendMessageAsync(embed: embed.Build());
        }

        public static async Task TrackEnded(TrackEndedEventArgs args)
        {
            LogManager.Log("DEBUG", args.Reason.ToString());

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                try
                {
                    // await TryAutoPlayNext(args);

                    return;
                } catch(Exception ex)
                {
                    LogManager.Log("ERROR", ex.Message);
                }
                
            }

            if (queueable is LavaTrack track)
            {
                await args.Player.PlayAsync(queueable);

                await args.Player.TextChannel.SendMessageAsync($"Tocando agora: {track.Title}");

                return;
            } else
            {
                // await args.Player.TextChannel.SendMessageAsync("Próximo item na playlist não é uma música.");
                return;
            }

        }

    }
}
