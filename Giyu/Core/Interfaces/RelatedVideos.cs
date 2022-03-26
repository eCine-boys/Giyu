using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Giyu.Core
{
    public class Thumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class Author
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("channel_url")]
        public string ChannelUrl { get; set; }

        [JsonProperty("user_url")]
        public string UserUrl { get; set; }

        [JsonProperty("thumbnails")]
        public List<Thumbnail> Thumbnails { get; set; }

        [JsonProperty("verified")]
        public bool Verified { get; set; }
    }

    public class IRelatedVideos
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("published")]
        public string Published { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("short_view_count_text")]
        public string ShortViewCountText { get; set; }

        [JsonProperty("view_count")]
        public string ViewCount { get; set; }

        [JsonProperty("length_seconds")]
        public int LengthSeconds { get; set; }

        [JsonProperty("thumbnails")]
        public List<Thumbnail> Thumbnails { get; set; }

        [JsonProperty("richThumbnails")]
        public List<object> RichThumbnails { get; set; }

        [JsonProperty("isLive")]
        public bool IsLive { get; set; }
    }
}
