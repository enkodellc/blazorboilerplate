using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataModels;
using BlazorBoilerplate.Storage;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using static Microsoft.AspNetCore.Http.StatusCodes;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System.Xml;
using System.Text;
using BlazorBoilerplate.Server.Helpers;

namespace BlazorBoilerplate.Server.Managers
{
    public class DbLogManager : IDbLogManager
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<DbLogManager> _logger;

        public DbLogManager(ApplicationDbContext context, ILogger<DbLogManager> logger)
        {
            _dbContext = context;
            _logger = logger;
        }

        public async Task<ApiResponse> Get(int pageSize = 25, int page = 0, Expression<Func<DbLog, bool>> predicate = null)
        {

            try
            {
                var r = await _dbContext.Logs.Where(predicate).Skip(pageSize * page).Take(pageSize).ToArrayAsync().ConfigureAwait(false);
                if (r.Length == 0)
                {
                    return new ApiResponse(Status204NoContent, $"No results for request; pageSize ={ pageSize}; page ={ page}");
                }
                else
                {
                    foreach (var item in r)
                    {
                        var PropertyDict = RegexUtilities.DirtyXMLParser(item.Properties);
                        item.LogProperties = PropertyDict;

                    }
                    //return new ApiResponse(Status200OK, "", formattedMessageList);
                    return new ApiResponse(Status200OK, $"Retrieved Type=DbLog;pageSize={pageSize};page={page}", r);


                }

            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error while retrieving Db Logs");
                return new ApiResponse(Status500InternalServerError, "Error while retrieving Db Logs", null);
            }
        }
        protected static System.IO.MemoryStream GenerateStreamFromString(string value)
        {
            return new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(value ?? ""));
        }
        protected async Task<System.IO.Stream> GetStreamFromStringAsync(string inputString)
        {
            using var stream = new System.IO.MemoryStream();
            using var writer = new System.IO.StreamWriter(stream);
            await writer.WriteAsync(inputString).ConfigureAwait(true);
            await writer.FlushAsync();
            stream.Position = 0;
            return stream;
        }
        protected string ExtractPropertiesFromLog(DbLog log)
        {

            XmlReaderSettings settings = new XmlReaderSettings
            {
                Async = true
            };

            var PropertyDict = new Dictionary<string, string>();
            try
            {

                PropertyDict = RegexUtilities.DirtyXMLParser(log.Properties);
                if (PropertyDict.Count == 0)
                    return log.MessageTemplate;


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            var s = new StringBuilder(log.MessageTemplate);
            var match = Helpers.RegexUtilities.StringInterpolationHelper.Match(log.MessageTemplate);
            while (match.Success)
            {
                if (PropertyDict.ContainsKey(match.Value))
                    s.Replace($"{{{match.Value}}}", PropertyDict[match.Value]);
                match = match.NextMatch();

            }
            return s.ToString();
        }


        public Task Log(DbLog LogItem)
        {
            throw new NotImplementedException();
        }
    }
}
