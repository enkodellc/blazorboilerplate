using BlazorBoilerplate.Server.Data;
using BlazorBoilerplate.Server.Data.Core;
using BlazorBoilerplate.Server.Middleware.Wrappers;
using BlazorBoilerplate.Server.Models;
using BlazorBoilerplate.Server.Services;
using BlazorBoilerplate.Shared.AuthorizationDefinitions;
using BlazorBoilerplate.Shared.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazorBoilerplate.Server.Controllers
{
    /// <summary>
    /// This controller is the entry API for the platform administration.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {

        #region Variables

        private static readonly UserInfoDto LoggedOutUser = new UserInfoDto { IsAuthenticated = false, Roles = new List<string>() };

        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        #endregion

        #region Constructors

        public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext db,
            SignInManager<ApplicationUser> signInManager, ILogger<AccountController> logger,
            RoleManager<IdentityRole<Guid>> roleManager, IEmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
            _emailService = emailService;
            _configuration = configuration;
            _db = db;
        }

        #endregion

        #region GetUsers

        [HttpGet("Users")]
        [Authorize]
        public async Task<ApiResponse> GetUsers([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 0)
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

        #endregion

        #region GetPermissions

        [HttpGet("Permissions")]
        [Authorize]
        public ApiResponse GetPermissions()
        {
            var permissions = ApplicationPermissions.GetAllPermissionValues();
            return new ApiResponse(200, "Permissions list fetched", permissions);
        }

        #endregion

        #region GetRoles

        [HttpGet("Roles")]
        [Authorize]
        public async Task<ApiResponse> GetRoles([FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 0)
        {
            var roleDtoList = new List<RoleDto>();
            List<IdentityRole<Guid>> listResponse;

            // get paginated list of users
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
                    var permissions = claims.Where(x => x.Type == "permission").Select(x => x.Value).ToList();

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

        #endregion

        #region CRUD : GetRoleAsync

        [HttpGet("Role/{roleName}")]
        [Authorize]
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
                var permissions = claims.Where(x => x.Type == ClaimConstants.Permission).Select(x => x.Value).ToList();

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

        #endregion

        #region CRUD : CreateRoleAsync 

        [HttpPost("Role")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> CreateRoleAsync([FromBody] RoleDto newRole)
        {
            try
            {

                // first make sure the role doesn't already exist
                if (_roleManager.Roles.Any(r => r.Name == newRole.Name))
                    return new ApiResponse(400, "role already exists");

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
                    var resultAddClaim = await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByValue(claim)));

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

        #endregion

        #region CRUD : UpdateRoleAsync

        [HttpPut("Role")]
        [Authorize(Policy = Policies.IsAdmin)]
        public async Task<ApiResponse> UpdateRoleAsync([FromBody] RoleDto newRole)
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
                    await _roleManager.RemoveClaimAsync(identityRole, new Claim(ClaimConstants.Permission, permission));

                foreach (string claim in newRole.Permissions)
                {
                    var result = await _roleManager.AddClaimAsync(identityRole, new Claim(ClaimConstants.Permission, ApplicationPermissions.GetPermissionByValue(claim)));

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

        #endregion

        #region CRUD : DeleteRoleAsync

        // DELETE: api/Admin/Role/5
        [HttpDelete("Role/{name}")]
        [Authorize(Policy = Policies.IsAdmin)]
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

        #endregion

    }
}
