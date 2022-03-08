using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using Victoria.Enums;
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

        public static async Task<string> PlayAsync(SocketGuildUser user, IGuild guild, string query)
        {
            Console.WriteLine($"{user.Nickname} - {query}");
            if (user.VoiceChannel is null) return "Você precisa estar em um canal de voz para isso.";

            if(!_lavaNode.HasPlayer(guild))
            {
                try
                {
                    var _channels = await guild.GetTextChannelsAsync();

                    var _channel = _channels.First();

                    if(!(_channel is null))
                    {
                        await _lavaNode.JoinAsync(user.VoiceChannel, _channel);

                        //return $"Conectado em {user.VoiceChannel.Name}";
                    }
                }
                catch (Exception ex)
                {
                    return $"ERROR\n{ex.Message}";
                }
            }

            try
            {
                var player = _lavaNode.GetPlayer(guild);

                LavaTrack track;

                var search = Uri.IsWellFormedUriString(query, UriKind.Absolute) ?
                    await _lavaNode.SearchAsync(SearchType.YouTubeMusic, query)
                    : await _lavaNode.SearchYouTubeAsync(query);

                if (search.Status == SearchStatus.NoMatches) return $"Não foram encontrados resultados para: {query}";

                track = search.Tracks.FirstOrDefault();

                if(player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
                {
                    player.Queue.Enqueue(track);
                    Console.WriteLine($"[{DateTime.Now}]\t[AUDIO]\tMúsica adicionada a playlist.");
                    return $"{track.Title} foi adicionada a playlist.";
                }

                await player.PlayAsync(track);
                Console.WriteLine($"Tocando agora: {track.Title}");
                return $"Tocando agora: {track.Title}";
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
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Playing) await player.StopAsync();

                await _lavaNode.LeaveAsync(player.VoiceChannel);

                Console.WriteLine($"[{DateTime.Now}]\t(AUDIO)\tBot saiu do canal de voz.");
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
                var player = _lavaNode.GetPlayer(guild);
                if (!(player.PlayerState is PlayerState.Playing))
                {
                    await player.PauseAsync();
                    return $"Não tem música ativa para pausar.";
                }

                await player.PauseAsync();
                return $"**Pausado:** {player.Track.Title}.";
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
                var player = _lavaNode.GetPlayer(guild);

                if (player.PlayerState is PlayerState.Paused)
                {
                    await player.ResumeAsync();
                }

                return $"**Resumed:** {player.Track.Title}";
            }
            catch (InvalidOperationException ex)
            {
                return ex.Message;
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

        public static async Task TrackEnded(TrackEndedEventArgs args)
        {

            if (!args.Player.Queue.TryDequeue(out var queueable))
            {
                await args.Player.TextChannel.SendMessageAsync("Sem mais músicas para tocar.");
                return;
            }


            if (!(queueable is LavaTrack track))
            {
                await args.Player.TextChannel.SendMessageAsync("Próximo item na playlist não é uma música.");
                return;
            }

            await args.Player.PlayAsync(track);

            await args.Player.TextChannel.SendMessageAsync($"Tocando agora: {track.Title}");
        }

    }
}
