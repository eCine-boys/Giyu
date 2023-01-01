using Giyu.Core.Managers;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Giyu.Core.Modules
{
    public class MusicModule
    {
        private static HttpClient client;

        public MusicModule(string apiUrl)
        {
            client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10),
                BaseAddress = new Uri(apiUrl),
            };

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private class IGetNextSongsBySongIdPayload
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("guildId")]
            public ulong GuildId { get; set; }
        }
        public async Task<IRelatedVideos> GetNextSongsBySongId(ulong _guildId, string _id)
        {
            
            try
            {
                IGetNextSongsBySongIdPayload payload = new IGetNextSongsBySongIdPayload()
                {
                    Id = _id,
                    GuildId = _guildId,
                };

                string json = JsonConvert.SerializeObject(payload);

                StringContent httpContent = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage res = await client.PostAsync("/related", httpContent);

                if (res.IsSuccessStatusCode)
                {
                    var res_json = await res.Content.ReadAsStringAsync();
                    var jsonModel = JsonConvert.DeserializeObject<IRelatedVideos>(res_json);

                    LogManager.LogDebug("AUTOPLAY", $"{jsonModel.Author} - {jsonModel.Title}");

                    return jsonModel;
                }
                else
                {
                    throw new Exception(res.ReasonPhrase);
                }
            }
            catch (Exception ex)
            {
                LogManager.LogError($"MUSICMODULE", ex.Message);
                return null;
            }
        }
    }
}
