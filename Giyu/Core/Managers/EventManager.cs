using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace Giyu.Core.Managers
{
    public static class EventManager
    {
        private readonly static LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();

        private readonly static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        private readonly static CommandService _commandService = ServiceManager.GetService<CommandService>();
        private static InteractionService _interactionService = ServiceManager.Provider.GetRequiredService<InteractionService>();
        public static IReadOnlyList<SlashCommandInfo> AllSlashCommands;
        public static Task LoadCommands()
        {
            _client.Log += message =>
            {
                LogManager.Log($"{message.Source}", $"{message.Message}");
                return Task.CompletedTask;
            };

            _commandService.Log += message =>
            {
                LogManager.Log($"{message.Source}", $"{message.Message}");
                return Task.CompletedTask;
            };

            _client.Ready += OnReady;

            _client.MessageReceived += OnMessageReceived;

            _lavaNode.OnLog += message =>
            {
                LogManager.LogDebug("LAVALINK", message.Message);

                return Task.CompletedTask;
            };

            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
            
            _lavaNode.OnTrackEnded += AudioManager.TrackEnded;

            //WSocketManager socket = new WSocketManager("localhost:80");

            return Task.CompletedTask;
        }

        private static async Task OnInteractionCreated(SocketInteraction interaction)
        {
            IServiceScope scope = ServiceManager.Provider.CreateScope();
            SocketInteractionContext context = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, scope.ServiceProvider);
        }

        private static async Task OnMessageReceived(SocketMessage arg) 
        {
            SocketUserMessage message = arg as SocketUserMessage;
            SocketCommandContext context = new SocketCommandContext(_client, message);

            if(message.Author.IsBot || message.Channel is IDMChannel) return;

            int argPos = 0;

            if (!(message.HasStringPrefix(ConfigManager.Config.Prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;

            var result = await _commandService.ExecuteAsync(context, argPos, ServiceManager.Provider);

            if(!result.IsSuccess)
            {
                if (result.Error == CommandError.UnknownCommand) return;
            }



        }

        private static async Task SlashCommandExecuted(SlashCommandInfo slashInfo, Discord.IInteractionContext slashCtx, Discord.Interactions.IResult slashResult)
        {
            if (!slashResult.IsSuccess)
            {
                switch (slashResult.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        await slashCtx.Interaction.RespondAsync($"Pré condição não atendida: {slashResult.ErrorReason}");
                        break;
                    case InteractionCommandError.UnknownCommand:
                        await slashCtx.Interaction.RespondAsync("Comando desconhecido.");
                        break;
                    case InteractionCommandError.Exception:
                        await slashCtx.Interaction.RespondAsync($"Erro no comando: {slashResult.ErrorReason}");
                        break;
                    case InteractionCommandError.Unsuccessful:
                        await slashCtx.Interaction.RespondAsync("Comando não pode ser executado");
                        break;
                    case InteractionCommandError.BadArgs:
                        await slashCtx.Interaction.RespondAsync("Número ou argumentos inválidos");
                        break;

                    default:
                        break;
                }
            }
        }

        private static async Task OnReady()
        {
            try
            {
                _interactionService = new InteractionService(_client.Rest);
                await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceManager.Provider);
                await _interactionService.RegisterCommandsGloballyAsync();

                foreach(var command in _interactionService.SlashCommands)
                    LogManager.Log("SLASH", $"{command.Name} carregado com sucesso.");

                AllSlashCommands = _interactionService.SlashCommands;

                _client.InteractionCreated += OnInteractionCreated;

                _interactionService.SlashCommandExecuted += SlashCommandExecuted;

                _client.SelectMenuExecuted += SelectMenuExecuted;

                await _lavaNode.ConnectAsync();
            } catch (Exception ex)
            {
                LogManager.LogError(ex.Source, ex.Message);
            }

            LogManager.Log("READY", "Bot está online.");

            await _client.SetStatusAsync(Discord.UserStatus.Online);
            await _client.SetGameAsync($"Prefix: {ConfigManager.Config.Prefix}", null, Discord.ActivityType.Listening);
        }

        private static async Task SelectMenuExecuted(SocketMessageComponent interaction)
        {
            try
            {
                SocketInteractionContext context = new SocketInteractionContext(_client, interaction);

                switch (interaction.Data.CustomId)
                {
                    case "select-song":

                        string songId = string.Join(", ", interaction.Data.Values);

                        SocketUserMessage message = interaction.Message;

                        SocketGuildUser user = context.User as SocketGuildUser;

                        await _lavaNode.JoinAsync(user.VoiceChannel, context.Channel as ITextChannel);

                        LogManager.Log("SELECT", $"[{interaction.Data.CustomId}] => [{songId}]");

                        Embed embed = await AudioManager.PlayAsync(user, context.Guild, $"https://youtube.com/watch?v={songId}", context);

                        await interaction.RespondAsync(embed: embed);

                        await interaction.DeleteOriginalResponseAsync();
                        break;
                    case "next_page":
                    case "last_page":
                        IGuild guild = context.Guild;

                        string page = string.Join(", ", interaction.Data.Values);

                        LavaTrack[] tracks = AudioManager.GetPageOfQueue(guild, int.Parse(page));

                        EmbedBuilder eBuilder = new EmbedBuilder();

                        foreach(LavaTrack track in tracks)
                        {
                            eBuilder.AddField(track.Title, track.Author);
                        }

                        Embed replyQueueEmbed = eBuilder.Build();

                        await interaction.UpdateAsync(msg =>
                        {
                            msg.Embed = replyQueueEmbed;
                        });

                        break;
                    default:
                        LogManager.Log("SELECT", "ID Não encontrado: {interaction.Data.CustomId}");
                        break;
                }

            } 
            catch(Exception ex) 
            {
                LogManager.Log("Exception", ex.Message);
            }
        }

        private static async Task OnTrackException(TrackExceptionEventArgs arg)
        {
            LogManager.Log("TrackException", $"{arg.Track.Title}");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel?.SendMessageAsync(
                $"{arg.Track.Title} foi adicionada novamente a playlist após um erro.");
        }

        private static async Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            LogManager.LogDebug("TrackStuck", $"{arg.Track.Title} ficou presa por {arg.Threshold}ms. => Console do Lavalink.");

            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel?.SendMessageAsync(
                $"{arg.Track.Title} foi adicionada novamente a playlist após ficar travada.");
        }

        private static Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
        {
            LogManager.Log("WebSocketClosed", $"Conexão a Discord WebSocket fechada pelo motivo: {arg.Reason}");
            return Task.CompletedTask;
        }
    }
}
