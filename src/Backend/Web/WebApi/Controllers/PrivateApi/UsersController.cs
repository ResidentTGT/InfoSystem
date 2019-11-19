using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NLog;
using Company.Common.Models.Identity;
using WebApi.Extensions;

namespace WebApi.Controllers.PrivateApi
{
    [Area("api")]
    [Produces("application/json")]
    [Route("[area]/v1/[controller]")]
    public class UsersController : Controller
    {
        private readonly IdentityContext _context;
        private readonly UserManager<User> _userManager;
        private readonly Logger _logger;

        public UsersController(IdentityContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
            _logger = LogManager.GetCurrentClassLogger();
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetUserInfo([FromQuery] int userId, [FromQuery] Module? module)
        {
            _logger.Debug($"Start getting user info by userId={userId} method...");
            _logger.Trace($"Getting user by userId={userId}...");
            var user = await _context.Users
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role.ResourcePermissions)
                            .Include(u => u.UserRoles)
                            .ThenInclude(ur => ur.Role.SectionPermissions)
                            .SingleAsync(u => u.Id == userId);

            _logger.Trace($"User by userId={userId} successfully received");
            if (module != null)
            {
                _logger.Trace($"Filtering user roles by module={module}...");
                user.UserRoles = user.UserRoles.Where(ur => ur.Role.Module == module).ToList();
            }
            _logger.Debug($"Getting user info by userId={userId} method is finished");
            return Ok(user);
        }


        [HttpGet]
        public async Task<IActionResult> GetUsersInDeparment([FromQuery] int departmentId)
        {
            _logger.Debug($"Start getting deparment users by departmentId={departmentId} method...");
            _logger.Trace($"Loading deparments...");
            _context.Departments.Load();
            _logger.Trace($"Getting deparment  by departmentId={departmentId}...");
            var department = _context.Departments.FirstOrDefault(d => d.Id == departmentId);
            _logger.Trace($"Department successfully received");

            _logger.Trace($"Recursive loading sub deparments...");
            PreloadDepartmentUsers(department);
            _logger.Trace($"Deparments successfully loaded");
            if (department == null)
            {
                _logger.Error($"Deparment with id={departmentId} not found");
                return NotFound("Данный департамент не существует");
            }
            _logger.Trace($"Getting users from deparment tree");
            var users = GetUsersFromDepartmentsTree(department);
            _logger.Debug($"Getting deparment users by departmentId={departmentId} method is finished");
            return Ok(users);
        }

        [HttpGet("byDepartmentsIds")]
        public async Task<IActionResult> GetUserCmpyDeparmentsIds()
        {
            var departmentsIds = Request.GetIdsFromQuery("departmentsIds");
            _logger.Debug($"Start getting deparment users by departmentsIds={string.Join(",", departmentsIds)} method...");

            _logger.Trace($"Getting deparments  by departmentsIds={departmentsIds}...");
            var departments = await _context.Departments.Where(d => departmentsIds.Contains(d.Id)).ToListAsync();

            if (!departments.Any())
            {
                _logger.Error($"Deparments with ids={string.Join(",", departmentsIds)} not found");
                return NotFound($"Департаменты по выбранным ids ({string.Join(",", departmentsIds)}) не найдены");
            }
            _logger.Trace($"Recursive loading sub deparments...");
            departments.ForEach(PreloadDepartmentUsers);
            _logger.Trace($"Deparments successfully loaded");
            var users = new HashSet<User>();
            _logger.Trace($"Getting users from deparments trees");
            departments.ForEach(d =>
            {
                foreach (var user in GetUsersFromDepartmentsTree(d))
                {
                    users.Add(user);
                }

            });
            _logger.Debug($"Getting deparment users by departmentsIds={string.Join(",", departmentsIds)} method is finished");
            return Ok(users);


        }


        private void PreloadDepartmentUsers(Department department)
        {
            _context.Entry(department).Collection(d => d.Users).Load();

            foreach (var subDep in department.SubDepartments)
            {
                PreloadDepartmentUsers(subDep);
            }
        }

        private List<User> GetUsersFromDepartmentsTree(Department department)
        {
            var users = department.Users.ToList();

            department.SubDepartments.ToList().ForEach(sd => users.AddRange(GetUsersFromDepartmentsTree(sd)));

            return users;
        }

    }
}