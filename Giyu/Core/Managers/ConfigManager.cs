﻿using Newtonsoft.Json;
using System.IO;

namespace Giyu.Core.Managers
{
    public static class ConfigManager
    {
        private static readonly string ConfigFolder = "Resources";
        private static readonly string ConfigFile = "config.json";
        private static readonly string ConfigPath = ConfigFolder + "/" + ConfigFile;
        public static BotConfig Config { get; private set; }

        static ConfigManager()
        {
            if(!Directory.Exists(ConfigFolder))
                Directory.CreateDirectory(ConfigFolder);

            if(!File.Exists(ConfigPath))
            {
                Config = new BotConfig();
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);

                File.WriteAllText(ConfigPath, json);
            }
            else
            {
                string json = File.ReadAllText(ConfigPath);
                Config = JsonConvert.DeserializeObject<BotConfig>(json);
            }
        }
    }

    public struct BotConfig
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("autoplay")]
        public bool Autoplay { get; set; }

        [JsonProperty("bot_provider_url")]
        public string BotProviderUri { get; set; }

        [JsonProperty("authorization")]
        public string LavaAuthorization { get; private set; }

        [JsonProperty("hostname")]
        public string LavaHostname { get; private set; }
    }
}
