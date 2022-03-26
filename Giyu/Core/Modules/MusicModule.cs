using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

                StringContent httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");


                var res = await client.PostAsync($"/related", httpContent);

                if (res.IsSuccessStatusCode)
                {
                    var res_json = await res.Content.ReadAsStringAsync();
                    var jsonModel = Newtonsoft.Json.JsonConvert.DeserializeObject<IRelatedVideos>(res_json);

                    Console.WriteLine(jsonModel.Id);

                    return jsonModel;
                } else
                {
                    throw new Exception(res.ReasonPhrase);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        
    }
}
