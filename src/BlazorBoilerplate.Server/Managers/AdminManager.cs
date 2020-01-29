using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Shared.Dto;
using BlazorBoilerplate.Storage.Core;
using Microsoft.AspNetCore.Identity;

namespace BlazorBoilerplate.Server.Managers
{
    public class AdminManager : IAdminManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public AdminManager(UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }
        
        public async Task<ApiResponse> GetUsers(int pageSize = 10, int pageNumber = 0)
        {
            var userDtoList = new List<UserInfoDto>();
            List<ApplicationUser> listResponse;

            // get paginated list of users
            try
            {
                var userList = _userManager.Users.AsQueryable();
                listResponse = userList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            // create the dto object with mapped properties and fetch roles associated with each user
            try
            {
                foreach (var applicationUser in listResponse)
                {
                    userDtoList.Add(new UserInfoDto
                    {
                        FirstName = applicationUser.FirstName,
                        LastName = applicationUser.LastName,
                        UserName = applicationUser.UserName,
                        Email = applicationUser.Email,
                        UserId = applicationUser.Id,
                        Roles = (List<string>)(await _userManager.GetRolesAsync(applicationUser).ConfigureAwait(true))
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            return new ApiResponse(200, "User list fetched", userDtoList);
        }

        public ApiResponse GetPermissions()
        {
            var permissions = ApplicationPermissions.GetAllPermissionNames();
            return new ApiResponse(200, "Permissions list fetched", permissions);
        }

        public async Task<ApiResponse> GetRoles(int pageSize = 10, int pageNumber = 0)
        {
            var roleDtoList = new List<RoleDto>();
            List<IdentityRole<Guid>> listResponse;

            // get paginated list of roles
            try
            {
                var roleList = _roleManager.Roles.AsQueryable();
                listResponse = roleList.OrderBy(x => x.Id).Skip(pageNumber * pageSize).Take(pageSize).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            try
            {
                foreach (var role in listResponse)
                {
                    var claims = await _roleManager.GetClaimsAsync(role);
                    var permissions = claims.Where(x => x.Type == "permission").Select(x => ApplicationPermissions.GetPermissionByValue(x.Value).Name).ToList();

                    roleDtoList.Add(new RoleDto
                    {
                        Name = role.Name,
                        Permissions = permissions
                    }); ;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            return new ApiResponse(200, "Roles list fetched", roleDtoList);
        }

        public async Task<ApiResponse> GetRoleAsync(string roleName)
        {
            RoleDto roleDto;
            IdentityRole<Guid> identityRole;

            // get paginated list of users
            try
            {
                identityRole = await _roleManager.FindByNameAsync(roleName);
            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            try
            {
                var claims = await _roleManager.GetClaimsAsync(identityRole);
                var permissions = claims.Where(x => x.Type == "permission").Select(x => ApplicationPermissions.GetPermissionByValue(x.Value).Name).ToList();

                roleDto = new RoleDto
                {
                    Name = roleName,
                    Permissions = permissions
                };

            }
            catch (Exception ex)
            {
                throw new Exception(null, ex);
            }

            return new ApiResponse(200, "Role fetched", roleDto);
        }

        public async Task<ApiResponse> CreateRoleAsync(RoleDto newRole)
        {
            try
            {
                // first make sure the role doesn't already exist
                if (_roleManager.Roles.Any(r => r.Name == newRole.Name))
                    return new ApiResponse(400, "Role already exists");

                // Create the role
                var result = await _roleManager.CreateAsync(new IdentityRole<Guid>(newRole.Name));

                if (!result.Succeeded)
                {
                    string errorMessage = result.Errors.Select(x => x.Description).Aggregate((i, j) => i + " - " + j);
                    return new ApiResponse(500, errorMessage);
                }

                // Re-create the permissions
                IdentityRole<Guid> role = await _roleManager.FindByNameAsync(newRole.Name);

                foreach (string claim in newRole.Permissions)
                {
                    var resultAddClaim = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));

                    if (!resultAddClaim.Succeeded)
                        await _roleManager.DeleteAsync(role);
                }

            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }

            return new ApiResponse(200);
        }

        public async Task<ApiResponse> UpdateRoleAsync(RoleDto newRole)
        {
            try
            {
                // first make sure the role already exist
                if (!_roleManager.Roles.Any(r => r.Name == newRole.Name))
                    return new ApiResponse(400, "This role doesn't exists");

                // Create the permissions
                IdentityRole<Guid> identityRole = await _roleManager.FindByNameAsync(newRole.Name);

                var claims = await _roleManager.GetClaimsAsync(identityRole);
                var permissions = claims.Where(x => x.Type == ClaimConstants.Permission).Select(x => x.Value).ToList();

                foreach (var permission in permissions)
                {
                    await _roleManager.RemoveClaimAsync(identityRole, new Claim(ClaimConstants.Permission, permission));
                }

                foreach (string claim in newRole.Permissions)
                {
                    var result = await _roleManager.AddClaimAsync(identityRole, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByName(claim)));

                    if (!result.Succeeded)
                        await _roleManager.DeleteAsync(identityRole);
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse(500, ex.Message);
            }
            return new ApiResponse(200);
        }

        public async Task<ApiResponse> DeleteRoleAsync(string name)
        {
            try
            {
                // Check if the role is used by a user
                var users = await _userManager.GetUsersInRoleAsync(name);
                if (users.Any())
                    return new ApiResponse(404, "This role is still used by a user, you cannot delete it");

                // Delete the role
                var role = await _roleManager.FindByNameAsync(name);
                await _roleManager.DeleteAsync(role);

                return new ApiResponse(200, "Role Deletion Successful");
            }
            catch
            {
                return new ApiResponse(400, "Role Deletion Failed");
            }
        }
    }
}
