using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SistemaContableCSG.Controllers;
using SistemaContableCSG.Models;

namespace SistemaContableCSG.Permissions
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly RolesController _rolesController;

        public PermissionAuthorizationHandler(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, RolesController rolesController)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _rolesController = rolesController;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User == null)
            {
                return;
            }

            // Get all the roles the user belongs to and check if any of the roles has the permission required
            // for the authorization to succeed.
            var user = await _userManager.GetUserAsync(context.User);
            if (user == null)
            {
                return;
            }
            var userRoleNames = await _userManager.GetRolesAsync(user);
            var userRoles = _roleManager.Roles.Where(x => userRoleNames.Contains(x.Name)).ToList();

            foreach (var role in userRoles)
            {
                var roleClaims = await _roleManager.GetClaimsAsync(role);

                var permissions = roleClaims.Where(x => x.Type == CustomClaimTypes.Permission &&
                                                        x.Value == requirement.Permission &&
                                                        x.Issuer == "LOCAL AUTHORITY")
                                            .Select(x => x.Value);

                if (permissions.Any() || context.User.IsInRole(_rolesController.AdminRole))
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
