using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;

namespace Giyu.Core.Managers
{
    public static class EventManager
    {
        private static LavaNode _lavaNode = ServiceManager.Provider.GetRequiredService<LavaNode>();
        private static DiscordSocketClient _client = ServiceManager.GetService<DiscordSocketClient>();
        private static CommandService _commandService = ServiceManager.GetService<CommandService>();

        public static Task LoadCommands()
        {
            _client.Log += message =>
            {
                Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t{message.Message}");
                return Task.CompletedTask;
            };

            _commandService.Log += message =>
            {
                Console.WriteLine($"[{DateTime.Now}]\t({message.Source})\t{message.Message}");
                return Task.CompletedTask;
            };

            _client.Ready += OnReady;

            _client.MessageReceived += OnMessageReceived;

            return Task.CompletedTask;
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
                Console.WriteLine(result.ToString());
            }
        }

        private static async Task OnReady()
        {
            try
            {
                await _lavaNode.ConnectAsync();
            } catch (Exception ex)
            {
                throw ex;
            }

            Console.WriteLine($"[{DateTime.Now}]\t(READY)\tBot está online.");

            await _client.SetStatusAsync(Discord.UserStatus.Online);
            await _client.SetGameAsync($"Prefix: {ConfigManager.Config.Prefix}", null, Discord.ActivityType.Listening);
        }
    }
}
