using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Shared.Dto.Dataset;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Infrastructure.Server
{
    public interface IDatasetManager
    {
        Task<ApiResponse> GetDataset(GetDatasetRequestDto dataset);
        Task<ApiResponse> GetDatasets();
        Task<ApiResponse> GetTabularDatasetColumnNames(GetTabularDatasetColumnNamesRequestDto dataset);
        Task<ApiResponse> Upload(FileUploadRequestDto file);
    }
}
