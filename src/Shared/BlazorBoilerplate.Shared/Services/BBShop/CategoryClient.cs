using BlazorBoilerplate.Shared.Interfaces.BBShop;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Services.BBShop
{
    public class CategoryClient : BaseApiClient, ICategoryClient
    {
        /// <summary>
        /// this client uses breeze.sharp for entity management therefore
        /// inherits from baseapiclient if you dont like use the breeze.sharp
        /// you may not use baseapiclient so just put the http request
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="logger"></param>
        public CategoryClient(HttpClient httpClient, ILogger<CategoryClient> logger) 
            : base(httpClient, logger, "api/shop/")
        { }

        
        public async Task<Breeze.Sharp.QueryResult<Categories>> LoadCategories(int? take = null, int? skip = null)
        {
            return await GetItems<Categories>(from: "Categories"
                , orderByDescending: null
                , take: take
                , skip: skip
                , parameters: null);
        }
    }
}
