using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SistemaContableCSG.Models;
using System.Diagnostics;

namespace SistemaContableCSG.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RolesController _rolesController;

        public HomeController(ILogger<HomeController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, RolesController rolesController)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _rolesController = rolesController;
        }

        public IActionResult Index()
        {
            return View();
        }
        //[Authorize(Roles="Admin,Miembro")]
        //[Authorize(Permissions.Permissions.GestionarUsuarios.View)]
        [Route("Privacidad")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("DefaultRoles")]
        public async Task<IActionResult> Roles(string email)
        {
            string[] roleNames = {_rolesController.AdminRole};

            foreach (var roleName in roleNames)
            {
                var roleExist = await _roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            if(email != null)
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    await _userManager.AddToRoleAsync(user, _rolesController.AdminRole);
                    return Content("Hecho. el usuario con email: " + email + " a obtenido rol de administrador.");
                }
                else
                {
                    return Content("Error. el usuario con email: " + email + " no a sido encontrado.");
                }
            }
            
            return Content("Error. debe proporcionar el email por url, por ejemplo: https://localhost:7047/DefaultRoles?email=correo@gmail.com");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}