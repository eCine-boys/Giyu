﻿using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Giyu.Core.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
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



            collection.AddLavaNode(x =>
            {
                x.SelfDeaf = false;
                x.Port = ConfigManager.Config.Port;
                x.IsSsl = false;
                x.Hostname = ConfigManager.Config.Hostname;
                x.Authorization = ConfigManager.Config.Authorization;
            });

            ServiceManager.SetProvider(collection);

        }

        public async Task MainAsync()
        {
            if (string.IsNullOrWhiteSpace(ConfigManager.Config.Token))
            {
                throw new ArgumentNullException("Bot Token está vazio ou não existe.");
            };

            await CommandManager.LoadCommandsAsync();
            await EventManager.LoadCommands();
            await _client.LoginAsync(TokenType.Bot, ConfigManager.Config.Token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }
    }
}
