using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;
using Victoria.Responses.Search;

namespace Giyu.Core.Managers
{
    public static class EventManager
    {
        private readonly static LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        private readonly static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        private readonly static CommandService _commandService = ServiceManager.GetService<CommandService>();
        private static InteractionService _interactionService = ServiceManager.Provider.GetRequiredService<InteractionService>();

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


            _lavaNode.OnTrackException += OnTrackException;
            _lavaNode.OnTrackStuck += OnTrackStuck;
            _lavaNode.OnWebSocketClosed += OnWebSocketClosed;
            _lavaNode.OnTrackEnded += AudioManager.TrackEnded;

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
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(_client, message);

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
                {
                    LogManager.Log("SLASH", $"{command.Name} carregado com sucesso.");
                }

                _client.InteractionCreated += OnInteractionCreated;

                _interactionService.SlashCommandExecuted += SlashCommandExecuted;

                await _lavaNode.ConnectAsync();
            } catch (Exception ex)
            {
                throw ex;
            }

            LogManager.Log("READY", "Bot está online.");

            await _client.SetStatusAsync(Discord.UserStatus.Online);
            await _client.SetGameAsync($"Prefix: {ConfigManager.Config.Prefix}", null, Discord.ActivityType.Listening);
        }

     /* private static async Task SlashCommandHandler(SocketSlashCommand command) // Controlar comandos futuramente;
        {
            await command.RespondAsync($"Executado {command.Data.Name}");
        } */

        private static async Task OnTrackException(TrackExceptionEventArgs arg)
        {
            LogManager.Log("TrackException", $"{arg.Track.Title} lançou um erro. => Console do Lavalink.");
            arg.Player.Queue.Enqueue(arg.Track);
            await arg.Player.TextChannel?.SendMessageAsync(
                $"{arg.Track.Title} has been re-added to queue after throwing an exception.");
        }

        private static async Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            LogManager.Log("TrackStuck", $"{arg.Track.Title} ficou presa por {arg.Threshold}ms. => Console do Lavalink.");

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
