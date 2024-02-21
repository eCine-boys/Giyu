using Newtonsoft.Json;
using System;
using System.IO;

namespace Giyu.Core.Managers
{
    public static class ConfigManager
    {
        private static readonly string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        private static readonly string ResourcesPath = Path.Combine(projectDirectory, "Resources");
        private static readonly string ConfigFileName = "config.json";
        private static readonly string ConfigFilePath = $@"{ResourcesPath}/{ConfigFileName}";
        public static BotConfig Config { get; private set; }

        static ConfigManager()
        {
            var config = Environment.GetEnvironmentVariable("token");

            Console.WriteLine(config);
            
            if(!Directory.Exists(ResourcesPath))
                Directory.CreateDirectory(ResourcesPath);

            if(!File.Exists(ConfigFilePath))
            {
                Config = new BotConfig()
                {
                    LavaHostname = "localhost",
                    LavaAuthorization = "youshallnotpass"
                };

                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);

                File.WriteAllText(ConfigFilePath, json);
            }
            else
            {
                string json = File.ReadAllText(ConfigFilePath);
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

        [JsonProperty("clientsecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("lava_pass")]
        public string LavaAuthorization { get; set; }

        [JsonProperty("lava_host")]
        public string LavaHostname { get; set; }
        
        [JsonProperty("lava_port")]
        public short LavaPort { get; set; }
    }
}
