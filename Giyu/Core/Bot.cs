using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Giyu.Core.Managers;
using Giyu.Core.Modules;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Victoria;

namespace Giyu.Core
{
    public class Bot
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commandService;
        private readonly InteractionService _interactionService;
        public Bot()
        {
            try
            {
                _client = new DiscordSocketClient(new DiscordSocketConfig()
                {
                    LogLevel = LogSeverity.Debug,
                });

                _commandService = new CommandService(new CommandServiceConfig()
                {
                    LogLevel = LogSeverity.Debug,
                    CaseSensitiveCommands = true,
                    DefaultRunMode = Discord.Commands.RunMode.Async,
                    IgnoreExtraArgs = true,
                });

                _interactionService = new InteractionService(_client.Rest, new InteractionServiceConfig()
                {
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = Discord.Interactions.RunMode.Async,
                });

                ServiceCollection collection = new ServiceCollection();

                collection.AddSingleton(_client);
                collection.AddSingleton(_interactionService);
                collection.AddSingleton(_commandService);

                if(string.IsNullOrEmpty(ConfigManager.Config.LavaAuthorization) || string.IsNullOrEmpty(ConfigManager.Config.LavaHostname))
                {
                    throw new Exception("Autorização/Hostname Lavalink vazios");
                }

                collection.AddLavaNode(x =>
                {
                    x.SelfDeaf = true;
                    x.Authorization = ConfigManager.Config.LavaAuthorization;
                    x.Hostname = ConfigManager.Config.LavaHostname;
                    x.IsSsl = false;
                    x.Port = 2333;
                });

                ServiceManager.SetProvider(collection);
            } catch(Exception err)
            {
                LogManager.LogError("BOT", err.Message);
            }
        }

        public async Task MainAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ConfigManager.Config.Token))
                {
                    throw new ArgumentNullException("Bot Token está vazio ou não existe.");
                };

                ProcessStartInfo p_info = new ProcessStartInfo();
                    p_info.UseShellExecute = true;
                    p_info.CreateNoWindow = false;
                    p_info.WindowStyle = ProcessWindowStyle.Normal;
                    p_info.FileName = @"C:\Lavalink\start.bat";

                Process.Start(p_info);

                await CommandManager.LoadCommandsAsync();
                await EventManager.LoadCommands();
                await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
                await _client.StartAsync();

                await Task.Delay(-1);
            }
            catch (Exception ex)
            {
                LogManager.LogError("GLOBAL", ex.Message);
            }
        }
    }
}
