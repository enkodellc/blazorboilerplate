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
using System.Threading;

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
        /// <summary>
        /// Asynchronously fetches <see cref="DbLog">log</see> entries from the database in a paginated manner.
        /// Supports filtering via the predicate parameter.
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="page"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        public async Task<ApiResponse> GetAsync(int pageSize = 25, int page = 0, Expression<Func<DbLog, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var r = await _dbContext.Logs.AsNoTracking().Where(predicate).OrderByDescending(x=>x.TimeStamp).Skip(pageSize * page).Take(pageSize).ToArrayAsync(cancellationToken).ConfigureAwait(false);
                var count = await _dbContext.Logs.CountAsync(predicate, cancellationToken).ConfigureAwait(false);
                var syncPoint = await _dbContext.Logs.AsNoTracking().Where(predicate).OrderBy(x=>x.TimeStamp).Select(x=>x.Id).LastAsync(cancellationToken).ConfigureAwait(false);
                if (r.Length == 0)
                {
                    return new ApiResponse(Status204NoContent, $"No results for request; pageSize ={ pageSize}; page ={ page}");
                }
                else
                {
                    foreach (var item in r)
                    {
                        var propertyDict = RegexUtilities.DirtyXmlPropertyParser(item.Properties);
                        item.LogProperties = propertyDict;

                    }
                    //return new ApiResponse(Status200OK, "", formattedMessageList);
                    return new ApiResponse(
                        statusCode: Status200OK,
                        message:$"Retrieved Type=DbLog;pageSize={pageSize};page={page}",
                        result: r,
                        paginationDetails: new PaginationDetails()
                        {
                            CollectionSize= count,
                            PageIndex = page,
                            PageSize = 25,
                            SyncPointReference = syncPoint,
                          
                        });


                }

            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "Error while retrieving Db Logs");
                return new ApiResponse(Status500InternalServerError, "Error while retrieving Db Logs", null);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Operation Error while retrieving Db Logs");
                return new ApiResponse(Status500InternalServerError, "Operation Error while retrieving Db Logs", null);

            }
        }
        protected static System.IO.MemoryStream GenerateStreamFromString(string value)
        {
            return new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(value ?? ""));
        }
        protected async Task<System.IO.Stream> GetStreamFromStringAsync(string inputString)
        {
            await using var stream = new System.IO.MemoryStream();
            await using var writer = new System.IO.StreamWriter(stream);
            await writer.WriteAsync(inputString).ConfigureAwait(true);
            await writer.FlushAsync();
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Unused Currently.
        /// Extracts xml formatted properties from serilog mssql logs
        /// </summary>
        /// <param name="log"></param>
        protected string ExtractPropertiesFromLog(DbLog log)
        {

            var settings = new XmlReaderSettings
            {
                Async = true
            };

            var propertyDict = new Dictionary<string, string>();
            try
            {

                propertyDict = RegexUtilities.DirtyXmlPropertyParser(log.Properties);
                if (propertyDict.Count == 0)
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
                if (propertyDict.ContainsKey(match.Value))
                    s.Replace($"{{{match.Value}}}", propertyDict[match.Value]);
                match = match.NextMatch();

            }
            return s.ToString();
        }

        /// <summary>
        /// Returns meta information (currently number of new entries) about the change in a collection as compared to a provided reference point
        /// </summary>
        /// <param name="deltaIndex"></param>
        /// <param name="predicate"></param>
        /// <param name="cancellationToken"></param>
        public async Task<ApiResponse> GetDeltaMetaAsync(int deltaIndex, Expression<Func<DbLog, bool>> predicate = null, CancellationToken cancellationToken = default)
        {
            int newEntryCount;
            if (predicate != null)
                newEntryCount = await _dbContext.Logs.AsNoTracking().CountAsync(x=>(x.Id > deltaIndex), cancellationToken).ConfigureAwait(false);
            else
                newEntryCount = await _dbContext.Logs.AsNoTracking().CountAsync(x => x.Id > deltaIndex, cancellationToken).ConfigureAwait(false);

            var syncPoint = await _dbContext.Logs.AsNoTracking().OrderBy(x=>x.TimeStamp).Select(x=>x.Id).LastAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogDebug($"There are {newEntryCount} newer entries than reference of {deltaIndex}");
            
            return new ApiResponse(
                        statusCode: Status204NoContent,
                        message: $"There are {newEntryCount} newer entries than reference of {deltaIndex}",
                        paginationDetails: new PaginationDetails()
                        {
                           DeltaRequestReference = deltaIndex,
                           CollectionSizeDelta = newEntryCount,
                           SyncPointReference = syncPoint,
                           
                        });

        }
    }
}
