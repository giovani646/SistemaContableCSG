using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaContableCSG.Data;
using SistemaContableCSG.Permissions;
using SistemaContableCSG.ViewModels;
using System.Security.Claims;
using System.Reflection;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace SistemaContableCSG.Controllers
{
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        public string AdminRole { get; } = "Admin";
        
        public RolesController(ApplicationDbContext context, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // GET: RolesController
        [HttpGet]
        [Authorize(Permissions.Permissions.GestionarRoles.View)]
        public IActionResult Index()
        {
            var permissionsList = new List<string>();

            // Obtener todas las subclases de Permissions
            var subClasses = typeof(Permissions.Permissions).GetNestedTypes(BindingFlags.Public | BindingFlags.Static);

            foreach (var subClass in subClasses)
            {
                // Obtener todas las propiedades de la subclase
                var properties = subClass.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var property in properties)
                {
                    // Obtener el valor de la propiedad y agregarlo a la lista de permisos
                    var value = property.GetValue(null);
                    permissionsList.Add(value.ToString());
                }
            }

            RolesIndex rolesIndex = new RolesIndex();
            rolesIndex.Permisos = permissionsList;

            return View(rolesIndex);
        }

        [HttpPost]
        [Authorize(Permissions.Permissions.GestionarRoles.View)]
        public JsonResult rolesData()
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
            List<IdentityRole> lista = new List<IdentityRole>();


            IQueryable<IdentityRole> queryRole = _context.Roles;//select * from roles

            // Total de registros antes de filtrar.
            int TotalRegistros = queryRole.Where(x => x.Name != AdminRole).Count(); //se quita el rol de admin, ya que dicho rol no se deberia gestionar

            queryRole = queryRole.Where(e => e.Name != AdminRole)
                .Where(e => e.Name.Contains(ValorBuscado));

            // Total de registros ya filtrados.
            int TotalRegistrosFiltrados = queryRole.Count();

            if (sortColumnDirection.Equals("asc"))
            {
                lista = queryRole.Skip(OmitirRegistros).Take(CantidadRegistros).OrderBy(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }
            else
            {
                lista = queryRole.Skip(OmitirRegistros).Take(CantidadRegistros).OrderByDescending(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }

            return Json(new
            {
                draw = NroPeticion,
                recordsTotal = TotalRegistros,
                recordsFiltered = TotalRegistrosFiltrados,
                data = lista,
            });
        }

        // POST: RolesController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarRoles.Create)]
        public async Task<JsonResult> Create(RolesIndex rolesIndex, string[] permisos)
        {
            string msj, status;

            if (!ModelState.IsValid)
            {
                msj = "Error. los datos no son validos";
                status = "error";

                return Json(new { msj, status });
            }

            IdentityRole role = new IdentityRole();
            role.Name = rolesIndex.Name;

            IdentityResult result = await _roleManager.CreateAsync(role);

            foreach (var item in permisos)
            {
                await _roleManager.AddClaimAsync(role, new Claim(CustomClaimTypes.Permission, item));
            }

            if (!result.Succeeded)
            {
                msj = "Error. el rol no se creo correcamente";
                status = "error";

                return Json(new { msj, status });
            }

            msj = "El rol se creo correcamente";
            status = "success";
            
            return Json(new { msj, status });
        }

        // POST: RolesController/Edit/5
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarRoles.Edit)]
        public async Task<JsonResult> Edit(RolesIndex rolesIndex, string id, string[] permisos)
        {
            string msj, status;

            if (!ModelState.IsValid)
            {
                msj = "Error. los datos no son validos";
                status = "error";

                return Json(new { msj, status });
            }

            var rol = await _roleManager.FindByIdAsync(id);

            if (rol == null)
            {
                msj = "Error. el rol no a sido encontrado, posiblemete ya a sido eliminado por otro usuario";
                status = "error";
                return Json(new { msj, status });
            }

            rol.Name = rolesIndex.Name;
            IdentityResult result = await _roleManager.UpdateAsync(rol);

            if (!result.Succeeded)
            {
                msj = "Ocurrio un error. el rol no fue actualizado correctamente";
                status = "error";

                return Json(new { msj, status });
            }

            var permissions = await _roleManager.GetClaimsAsync(rol);
            List<string> currentPermissions = new();

            foreach (var item in permissions)
            {
                currentPermissions.Add(item.Value);
            }

            var rolesAssign = permisos.Except(currentPermissions); //De la lista de permisos seleccionados por el usuario, se excluyen los permisos actuales del rol
            var rolesUnssign = currentPermissions.Except(permisos); //De la lista de permisos actuales del usuario se excluyen los permisos seleccionados

            foreach (var item in rolesUnssign)
            {
                await _roleManager.RemoveClaimAsync(rol, new Claim(CustomClaimTypes.Permission, item));
            }

            foreach (var item in rolesAssign)
            {
                await _roleManager.AddClaimAsync(rol, new Claim(CustomClaimTypes.Permission, item));
            }

            msj = "El rol a sido actualizado correctamente";
            status = "success";

            return Json(new { msj, status });
        }

        [HttpGet]
        [Authorize(Permissions.Permissions.GestionarRoles.Edit)]
        public async Task<IActionResult> editarData(string IdRol)
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))//comprobar si la solicitud no es ajax
            {
                return NotFound();
            }

            string msj, status;
            var rol = await _roleManager.FindByIdAsync(IdRol);

            if(rol == null)
            {
                msj = "Error. no se pudo encontrar el rol solicitado, posiblemente ya a sido eliminado por otro usuario,se actualizara la lista de roles de la tabla";
                status = "error";

                return Json(new { msj, status });
            }

            var permisos = await _roleManager.GetClaimsAsync(rol);
            List<string> permissionsList = new();

            foreach (var item in permisos)
            {
                permissionsList.Add(item.Value);
            }
            
            return Json(new { rol, permisos = permissionsList });
        }

        [HttpDelete]
        [Authorize(Permissions.Permissions.GestionarRoles.Delete)]
        public async Task<JsonResult> Delete(string id)
        {
            string msj, status;
            var rol = await _roleManager.FindByIdAsync(id);

            if(rol == null)
            {
                msj = "Error. No se pudo eliminar el rol correctamente, posiblemente ya a sido eliminado por otro usuario";
                status = "error";
                return Json(new { msj, status });
            }

            IdentityResult result = await _roleManager.DeleteAsync(rol);

            if (result.Succeeded)
            {
                msj = "Se elimino el rol correctamente";
                status = "success";
            }
            else
            {
                msj = "Ocurrio un error. No se pudo eliminar el rol correctamente";
                status = "error";
            }

            return Json(new { msj, status });
        }
    }
}
