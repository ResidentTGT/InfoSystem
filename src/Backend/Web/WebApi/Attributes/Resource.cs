using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Company.Common.Models.Identity;
using ResourcePermission = Company.Common.Models.Users.ResourcePermission;
using ResourcePermissionsValues = Company.Common.Models.Users.ResourcePermissionsValues;

namespace WebApi.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ResourceAttribute : ActionFilterAttribute
    {
        private readonly string _resourceName;
        private readonly ResourcePermissionsValues _methods;

        public ResourceAttribute(string resourceName, ResourcePermissionsValues methods)
        {
            _resourceName = resourceName;
            _methods = methods;
        }

        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            var user = actionContext.HttpContext.Items["User"] as User;
            if (user == null)
            {
                actionContext.Result = new UnauthorizedResult();
                return;
            }
            var resourcePermissions = new List<ResourcePermission>();
            foreach (var role in user.UserRoles)
            {
                resourcePermissions.AddRange(role.Role.ResourcePermissions);
            }
            var userHasAccess = resourcePermissions.Where(rp => rp.Name == _resourceName)
                .Any(rp => rp.Value.HasFlag(_methods));

            if (userHasAccess)
                return;

            actionContext.Result = new ForbidResult();
        }
    }
}