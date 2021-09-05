using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces.BBShop
{
    public interface IShopClient
    {
        Task<Breeze.Sharp.QueryResult<Categories>> LoadCategories(int? take = null, int? skip = null);

        Task<Breeze.Sharp.QueryResult<Product>> LoadProducts(int? take = null, int? skip = null);
    }
}
