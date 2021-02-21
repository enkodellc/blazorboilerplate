using BlazorBoilerplate.Shared.DataInterfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using BlazorBoilerplate.Storage;
using BlazorBoilerplate.Infrastructure.Storage.DataModels;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Shared.ApiContentService
{
    public class ApiContentProvider : IApiContentProvider
    {
        protected readonly ILogger Logger;
        private readonly ApiContentDbContext _apiContentDbContext;
        public ApiContentProvider(
            ApiContentDbContext apiContentDbContext,
            ILogger<ApiContentProvider> logger)
        {
            _apiContentDbContext = apiContentDbContext;
            Logger = logger;
        }
        public async Task<bool> Upsert(WikiPage wikiPage)
        {
            var existing = _apiContentDbContext.WikiPages.Where(x => x.pageid == wikiPage.pageid).FirstOrDefault();
            if(existing == null)
            {
                _apiContentDbContext.Add(wikiPage);
                return true;
            }
            else
            {
                //update
                //not doing anything with update now so returning false
                return false;
            }
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _apiContentDbContext.SaveChangesAsync();
        }
    }
}
