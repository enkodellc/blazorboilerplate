using BlazorBoilerplate.Shared.Dto.BBShop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.Interfaces.BBShop
{
    public interface ICategoryClient
    {
        Task<Breeze.Sharp.QueryResult<Categories>> LoadCategories();
    }
}
