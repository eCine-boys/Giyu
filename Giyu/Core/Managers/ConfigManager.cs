using Newtonsoft.Json;
using System;
using System.IO;

namespace Giyu.Core.Managers
{
    public static class ConfigManager
    {
        private static readonly string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        private static readonly string resourcesFolder = Path.Combine(projectDirectory, "Resources");
        private static readonly string resouceConfigFile = resourcesFolder + @"\" + "config.json";

        public static BotConfig Config { get; private set; }

        static ConfigManager()
        {
            if (!Directory.Exists(resourcesFolder))
                Directory.CreateDirectory(resourcesFolder);

            if(!File.Exists(resouceConfigFile))
            {
                Config = new BotConfig()
                {
                    LavaHostname = "localhost",
                    LavaAuthorization = "youshallnotpass"
                };

                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);

                File.WriteAllText(resouceConfigFile, json);
            }
            else
            {
                string json = File.ReadAllText(resouceConfigFile);
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
        public ushort LavaPort { get; set; }
    }
}
