using System;
using System.IO;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Shared.DataInterfaces;
using BlazorBoilerplate.Shared.Dto.Tenant;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace BlazorBoilerplate.Server.Managers
{
    public class TenantManager : ITenantManager
    {
        private readonly ITenantStore _tenantStore;

        public TenantManager(ITenantStore tenantStore)
        {
            _tenantStore = tenantStore;
        }

        public async Task<ApiResponse> Get()
        {
            try
            {
                var tenants = _tenantStore.GetAll();
                return new ApiResponse(Status200OK, "Retrieved Tenants", tenants);
            }
            catch (Exception ex)
            {
                return new ApiResponse(Status400BadRequest, ex.Message);
            }
        }

        public async Task<ApiResponse> Get(long id)
        {
            try
            {
                var tenant = _tenantStore.GetById(id);
                return new ApiResponse(Status200OK, "Retrieved Tenant", tenant);
            }
            catch
            {
                return new ApiResponse(Status400BadRequest, "Failed to Retrieve Tenant");
            }
        }

        public async Task<ApiResponse> Create(TenantDto tenantDto)
        {
            var tenant = await _tenantStore.Create(tenantDto);
            return new ApiResponse(Status200OK, "Created Tenant", tenant);
        }

        public async Task<ApiResponse> Update(TenantDto tenantDto)
        {
            try
            {
                var tenant = await _tenantStore.Update(tenantDto);
                return new ApiResponse(Status200OK, "Updated Tenant", tenant);
            }
            catch (InvalidDataException dataException)
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Tenant");
            }
        }

        public async Task<ApiResponse> Delete(long id)
        {
            try
            {
                await _tenantStore.DeleteById(id);
                return new ApiResponse(Status200OK, "Soft Delete Tenant");
            }
            catch (InvalidDataException dataException)
            {
                return new ApiResponse(Status400BadRequest, "Failed to update Tenant");
            }
        }

        public Task<ApiResponse> Get(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
