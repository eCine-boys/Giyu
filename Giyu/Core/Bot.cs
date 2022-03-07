using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Giyu.Core.Managers;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Victoria;

namespace Giyu.Core
{
    public class Bot
    {
        private DiscordSocketClient _client;
        private CommandService _commandService;


        public Bot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Debug,
            });

            _commandService = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Debug,
                CaseSensitiveCommands = true,
                DefaultRunMode = RunMode.Async,
                IgnoreExtraArgs = true,
            });

            ServiceCollection collection = new ServiceCollection();
            
            collection.AddSingleton(_client);
            collection.AddSingleton(_commandService);
            collection.AddLavaNode(x =>
            {
                x.SelfDeaf = false;
            });

            ServiceManager.SetProvider(collection);

        }

        public async Task MainAsync()
        {
            if (string.IsNullOrWhiteSpace(ConfigManager.Config.Token)) return;

            await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
