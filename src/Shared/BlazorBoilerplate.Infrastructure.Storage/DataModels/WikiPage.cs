using BlazorBoilerplate.Infrastructure.Storage.Permissions;
using BlazorBoilerplate.Shared.DataInterfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorBoilerplate.Infrastructure.Storage.DataModels
{
    [Permissions(Actions.CRUD)]
    public partial class WikiPage : IWikiPage
    {
        [Key]
        public int Id { get; set; }
        [JsonProperty("pageid")]
        public int pageid { get; set; }

        [JsonProperty("ns")]
        public string ns { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("extract")]
        public string Extract { get; set; }
        
    }
}
