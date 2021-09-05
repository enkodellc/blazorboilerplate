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
    public class ShopClient : BaseApiClient, IShopClient
    {
        /// <summary>
        /// this client uses breeze.sharp for entity management therefore
        /// inherits from baseapiclient if you dont like use the breeze.sharp
        /// you may not use baseapiclient so just put the http request
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="logger"></param>
        public ShopClient(HttpClient httpClient, ILogger<ShopClient> logger) 
            : base(httpClient, logger, "api/shop/")
        { }

        #region categories
        public async Task<Breeze.Sharp.QueryResult<Categories>> LoadCategories(int? take = null, int? skip = null)
        {
            return await GetItems<Categories>(from: "Categories"
                                            , orderByDescending: null
                                            , take: take
                                            , skip: skip
                                            , parameters: null);
        }
        #endregion

        #region products
        public async Task<Breeze.Sharp.QueryResult<Product>> LoadProducts(int? take = null, int? skip = null)
        {
            return await GetItems<Product>(from: "Products"
                                            , orderByDescending: null
                                            , take: take
                                            , skip: skip
                                            , parameters: null);
        }
        #endregion


    }
}
