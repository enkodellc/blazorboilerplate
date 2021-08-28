using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Shared.Dto.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces.BBShop
{
    public interface ICategoryClient
    {
        Task<Breeze.Sharp.QueryResult<Categories>> LoadCategories(int? take = null, int? skip = null);
    }
}
