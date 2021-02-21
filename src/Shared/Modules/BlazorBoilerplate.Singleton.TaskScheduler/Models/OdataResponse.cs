using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BlazorBoilerplate.Singleton.TaskScheduler.Models
{
    public partial class Wiki
    {
        [JsonProperty("batchcomplete")]
        public string Batchcomplete { get; set; }

        [JsonProperty("query")]
        public Query Query { get; set; }
    }

    public partial class Query
    {
        [JsonProperty("pages")]
        public Dictionary<string, WikiPage> Pages { get; set; }
    }

}