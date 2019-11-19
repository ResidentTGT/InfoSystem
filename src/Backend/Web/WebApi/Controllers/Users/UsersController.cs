using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NLog;
using Company.Common.Models.Identity;
using WebApi.Attributes;
using WebApi.Dto.Users;

namespace WebApi.Controllers.Users
{
    [Produces("application/json")]
    [Route("v1/users")]
    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;

        private readonly IdentityContext _dbContext;
        private readonly Logger _logger;

        public UsersController(IdentityContext context, IConfiguration configuration)
        {
            _dbContext = context;
            _configuration = configuration;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("info")]
        [Authorize]
        public async Task<IActionResult> GetUserInfo([FromQuery] Module? module)
        {
            _logger.Debug("Start getting user info method...");
            var user = HttpContext.Items["User"] as User;

            var userDto = new UserDto()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                DisplayName = user.DisplayName,
                AuthorizationType = user.AuthorizationType,
                LastName = user.LastName,
                IsLead = user.IsLead,
                DepartmentId = user.DepartmentId,
                ProviderData = user.ProviderData
            };
            _logger.Trace("Getting user role groups...");
            var roleGroups = module == null
                ? user.UserRoles.Select(ur => ur.Role).GroupBy(r => r.Module).ToList()
                : user.UserRoles.Where(ur => ur.Role.Module == module).Select(ur => ur.Role).GroupBy(r => r.Module).ToList();
            _logger.Trace("User role groups successfully got");
            _logger.Trace("Getting user permissions and adding to userDto...");
            foreach (var roleGroup in roleGroups)
            {
                var sectionPermissions = roleGroup.Select(r => r).SelectMany(r => r.SectionPermissions).ToList().Select(s => new Company.Common.Models.Users.SectionPermission()
                {
                    Id = s.Id,
                    Name = s.Name,
                    RoleId = s.RoleId
                });

                var resourcePermissions = roleGroup.Select(r => r).SelectMany(r => r.ResourcePermissions)
                    .GroupBy(rp => rp.Name)
                    .ToList()
                    .Select(g => new ResourcePermissionDto()
                    {
                        Name = g.Key,
                        Value = g.Select(rp => rp.Value)
                                            .ToList()
                                            .Aggregate((current, next) => current | next)
                    });

                userDto.ModulePermissions.Add(new ModulePermissoinDto()
                {
                    Module = roleGroup.Key,
                    SectionPermissions = sectionPermissions.ToList(),
                    ResourcePermissions = resourcePermissions.ToList()
                });
            }
            _logger.Trace("Adding user roles to userDto...");
            userDto.Roles.AddRange(roleGroups.SelectMany(r => r.Select(role => new RoleDto(role))));
            _logger.Debug("Getting user info method is finished");
            return Ok(userDto);
        }


        [HttpGet("partner-departments")]
        [Authorize]
        public async Task<IActionResult> GetPartnerDepartments()
        {
            _logger.Debug("Start getting partner departments method...");
            _logger.Trace("Getting PartnerDepartmentId from configuration...");
            var partnerDepartmentId = Convert.ToInt32(_configuration.GetSection("PartnerDepartmentId").Value);
            _logger.Trace("Loading departments...");
            _dbContext.Departments.Load();
            _logger.Trace($"Checking is department with id={partnerDepartmentId} exist...");
            var department = await _dbContext.Departments.FirstOrDefaultAsync(d => d.Id == partnerDepartmentId);
            if (department == null)
            {
                _logger.Error($"Department with id={partnerDepartmentId} not found");
                return NotFound("Данный департамент не существует");
            }
            _logger.Trace($"Recursive loading department users");
            PreloadDepartmentUsers(department);
            _logger.Trace($"Department users successfully loaded");

            var departments = new List<Department>();
            _logger.Trace($"Converting departments tree to list of departments");
            FillDepartments(departments, department);
            _logger.Trace($"Department tree successfully converted to list of departments");

            _logger.Trace($"Converting departments to DepartmentDto");
            var departmentsDto = departments.Select(d => new DepartmentDto(d));
            _logger.Trace($"Departments  successfully converted to DepartmentDto");

            _logger.Debug("Getting partner departments method is finished");
            return Ok(departmentsDto);
        }

        private void PreloadDepartmentUsers(Department department)
        {
           
            _dbContext.Entry(department).Collection(d => d.Users).Load();

            foreach (var subDep in department.SubDepartments)
            {
                PreloadDepartmentUsers(subDep);
            }
        }

        private void FillDepartments(List<Department> departments, Department department)
        {
            departments.Add(department);

            foreach (var subDep in department.SubDepartments)
                FillDepartments(departments, subDep);
        }
    }
}