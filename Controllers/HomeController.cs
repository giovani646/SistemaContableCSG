using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaContableCSG.Data;
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
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, RolesController rolesController, ApplicationDbContext context)
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            _rolesController = rolesController;
            _context = context;
        }

        [Authorize(Permissions.Permissions.Bitacora.View)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult bitacoraData()
        {
            //Representa el número de veces que se ha realizado una petición
            int NroPeticion = Convert.ToInt32(Request.Form["draw"].FirstOrDefault() ?? "0");

            //cuantos registros va a devolver
            int CantidadRegistros = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");

            //cuantos registros va a omitir
            int OmitirRegistros = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");

            //el texto de busqueda
            string ValorBuscado = Request.Form["search[value]"].FirstOrDefault() ?? "";

            //direccion y columna de ordenamiento (ascendente o descendente)
            string? sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][data]"].FirstOrDefault();
            string sortColumnNormalized = Char.ToUpper(sortColumn[0]).ToString() + sortColumn.Substring(1);
            string sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";

            //========================= PARA OBTENER DATOS =============================//
            List<Bitacora> lista = new List<Bitacora>();


            IQueryable<Bitacora> queryBitacora = _context.Bitacora.Include(x =>x.User);//select * from bitacora

            // Total de registros antes de filtrar.
            int TotalRegistros = queryBitacora.Count();

            queryBitacora = queryBitacora.Where(e => string.Concat(e.Accion, e.User.Nombre, e.User.Email).Contains(ValorBuscado));

            // Total de registros ya filtrados.
            int TotalRegistrosFiltrados = queryBitacora.Count();

            if (sortColumnDirection.Equals("asc"))
            {
                lista = queryBitacora.Skip(OmitirRegistros).Take(CantidadRegistros).OrderBy(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }
            else
            {
                lista = queryBitacora.Skip(OmitirRegistros).Take(CantidadRegistros).OrderByDescending(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }

            return Json(new
            {
                draw = NroPeticion,
                recordsTotal = TotalRegistros,
                recordsFiltered = TotalRegistrosFiltrados,
                data = lista,
            });
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