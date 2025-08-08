using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Giyu.Core.Managers;
using Giyu.Core.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
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


                #if DEBUG
                    IConfigurationBuilder builder = new ConfigurationBuilder()
                        .AddJsonFile($"appsettings.Development.json", true, true);
                #else
                    var builder = new ConfigurationBuilder()
                            .AddJsonFile($"appsettings.json", true, true);
                #endif

                IConfiguration configuration = builder.AddEnvironmentVariables().Build();

                ServiceCollection collection = new ServiceCollection();

                collection.AddSingleton(configuration);

                collection.AddSingleton(_client);
                collection.AddSingleton(_interactionService);
                collection.AddSingleton(_commandService);
                collection.AddSingleton<PlaybackService>();
                collection.AddSingleton<QueueService>();
                collection.AddSingleton<LyricsService>();
                collection.AddSingleton<AudioManager>();

                if(string.IsNullOrEmpty(ConfigManager.Config.LavaAuthorization) || string.IsNullOrEmpty(ConfigManager.Config.LavaHostname))
                {
                    throw new Exception("Autorização/Hostname Lavalink vazios");
                }

                collection.AddLavaNode(x =>
                {
                    x.SelfDeaf = true;
                    x.Hostname = configuration.GetSection("lava_host").Value;
                    x.Authorization = configuration.GetSection("lava_pass").Value;
                    x.IsSsl = false;
                    x.Port = ushort.Parse(configuration.GetSection("lava_port").Value);
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
                IConfiguration configuration = ServiceManager.Provider.GetService<IConfiguration>();

                string token = configuration.GetSection("token").Value;

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentNullException("Bot Token está vazio ou não existe.");
                };

                string workingDirectory = Environment.CurrentDirectory;
                string projectDirectory = Directory.GetParent(workingDirectory).Parent.Parent.FullName;

                ProcessStartInfo p_info = new ProcessStartInfo();
                    p_info.UseShellExecute = true;
                    p_info.CreateNoWindow = false;
                    p_info.WindowStyle = ProcessWindowStyle.Normal;
                    p_info.FileName = $"{projectDirectory}\\Lavalink\\start.bat";

                Process.Start(p_info);

                await CommandManager.LoadCommandsAsync();
                await EventManager.LoadCommands();
                await _client.LoginAsync(TokenType.Bot, token);
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
