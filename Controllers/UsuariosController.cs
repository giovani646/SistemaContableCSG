using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using SistemaContableCSG.Data;
using SistemaContableCSG.Models;
using System.Text.Encodings.Web;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using SistemaContableCSG.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace SistemaContableCSG.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;

        public UsuariosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        // GET: UsuariosController
        [HttpGet]
        [Authorize(Permissions.Permissions.GestionarUsuarios.View)]
        public IActionResult Index()
        {
            List<IdentityRole> roles = _context.Roles.OrderBy(x=>x.Name).ToList();

            UsuariosIndex usersIndex = new UsuariosIndex();

            usersIndex.Roles = roles;

            return View(usersIndex);
        }

        [HttpPost]
        [Authorize(Permissions.Permissions.GestionarUsuarios.View)]
        public JsonResult usuariosData()
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
            List<ApplicationUser> lista = new List<ApplicationUser>();


            IQueryable<ApplicationUser> queryUser = _context.Users;//select * from users

            // Total de registros antes de filtrar.
            int TotalRegistros = queryUser.Count() - 1; //se resta el usuario actual
            
            queryUser = queryUser.Where(u => u.Id != _userManager.GetUserAsync(HttpContext.User).Result.Id)
                .Where(e => string.Concat(e.Nombre, e.Email, e.PhoneNumber).Contains(ValorBuscado));

            // Total de registros ya filtrados.
            int TotalRegistrosFiltrados = queryUser.Count();

            if (sortColumnDirection.Equals("asc")){
                lista = queryUser.Skip(OmitirRegistros).Take(CantidadRegistros).OrderBy(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }
            else
            {
                lista = queryUser.Skip(OmitirRegistros).Take(CantidadRegistros).OrderByDescending(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }

            return Json(new { 
                draw = NroPeticion,
                recordsTotal = TotalRegistros,
                recordsFiltered = TotalRegistrosFiltrados,
                data = lista,
            });
        }

        // POST: UsuariosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarUsuarios.Create)]
        public async Task<JsonResult> Create(UsuariosIndex usuariosIndex, string[] roles)
        {
            string? msj = null, status;

            if (!ModelState.IsValid)
            {
                msj = "Ocurrio un error. Los datos no han sido validados correctamente, intente mas tarde";
                status = "error";

                return Json(new{msj,status});
            }

            var user = new ApplicationUser();
            user.Nombre = usuariosIndex.Nombre;
            user.UserName = usuariosIndex.Email;
            user.Email = usuariosIndex.Email;
            user.PhoneNumber = usuariosIndex.PhoneNumber;

            var result = await _userManager.CreateAsync(user, usuariosIndex.Password);

            if (result.Succeeded)
            {
                string statusE = await SendEmailConfirmation(user, usuariosIndex.Email);

                if (roles.Count() > 0)
                {
                    await _userManager.AddToRolesAsync(user, roles);
                }

                msj = "Se creo el usuario correctamente, se a enviado un correo para confirmar la direccion de correo electronico";
                status = "success";
                return Json(new { msj,status });
            }
            else
            {
                foreach (var item in result.Errors)
                {
                    if (item.Code.Equals("DuplicateUserName")) //El correo ya se encuentra registrado
                    {
                        msj = "Error. El correo electronico: " + usuariosIndex.Email + " ya se encuentra registrado!";
                    }
                    if (item.Code.Equals("PasswordRequiresNonAlphanumeric") || item.Code.Equals("PasswordRequiresDigit") || item.Code.Equals("PasswordRequiresUpper"))
                    {
                        msj = "Error en la contraseña";
                    }
                }
                //Ocurre cuando ocurre cualquier otro error que no sea por el email, se generaliza el msj de error
                msj = msj ?? "Ocurrio un error. No se creo el usuario correctamente.";
                status = "error";
            }

            return Json(new{msj,status});
        }

        [HttpGet]
        [Authorize(Permissions.Permissions.GestionarUsuarios.Edit)]
        public async Task<IActionResult> editarData(string IdUsuario)
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))//comprobar si la solicitud no es ajax
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(IdUsuario);
            IList<string>? user_roles;

            if (user != null)
            {
               user_roles = await _userManager.GetRolesAsync(user);
            }
            else
            {
                user_roles = null;
                user = null;
            }

            return Json(new
            {
                usuario = user,
                roles = user_roles
            });
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarUsuarios.Edit)]
        public async Task<JsonResult> Edit(UsuariosIndex usuarioIndex, string id, string[] roles)
        {
            string? status, msj;

            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");

            if (!ModelState.IsValid)
            {
                msj = "Ocurrio un error. Los datos no han sido validados correctamente, intente mas tarde";
                status = "error";

                return Json(new{msj,status});
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user != null)
            {
                var oldemail = user.Email;

                user.Nombre = usuarioIndex.Nombre;
                user.Email = usuarioIndex.Email;
                user.PhoneNumber = usuarioIndex.PhoneNumber;
                user.UserName = usuarioIndex.Email;

                if (oldemail.Equals(usuarioIndex.Email) == false)
                {
                    user.EmailConfirmed = false;
                }

                var currentUserRoles = await _userManager.GetRolesAsync(user);
                var rolesAssign = roles.Except(currentUserRoles); //De la lista de roles seleccionados por el usuario, se excluyen los roles actuales del usuario
                var rolesUnssign = currentUserRoles.Except(roles); //De la lista de roles actuales del usuario se excluyen los roles seleccionados

                IdentityResult resultRemove = await _userManager.RemoveFromRolesAsync(user, rolesUnssign);
                IdentityResult resultAsign = await _userManager.AddToRolesAsync(user, rolesAssign);
                IdentityResult result = await _userManager.UpdateAsync(user);

                if (resultRemove.Succeeded & resultAsign.Succeeded & result.Succeeded)
                {
                    if (oldemail.Equals(usuarioIndex.Email) == false)
                    {
                        string statusE = await SendEmailConfirmation(user, usuarioIndex.Email);
                        msj = "El usuario a sido actualizado correctamente, se a enviado un correo para confirmar la direccion de correo electronico";
                    }
                    else
                    {
                        msj = "El usuario a sido actualizado correctamente";
                    }
                    status = "success";
                }
                else
                {
                    msj = "Ocurrio un error. No se pudo actualizar el usuario, intente mas tarde";
                    status = "error";
                }
            }
            else
            {
                msj = "Error. No se pudo actualizar el usuario. Posiblemente el usuario ya a sido eliminado";
                status = "error";
            }

            return Json(new{msj,status});
        }

        [HttpDelete]
        [Authorize(Permissions.Permissions.GestionarUsuarios.Delete)]
        public async Task<JsonResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            string? msj, status;

            if(user != null)
            {
                IdentityResult result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    msj = "Se elimino al usuario correctamente";
                    status = "success";
                }
                else
                {
                    msj = "Ocurrio un error, intente mas tarde";
                    status = "error";
                }
            }
            else
            {
                msj = "Error. No se elimino al usuario, posiblemente ya a sido eliminado por otro usuario";
                status = "error";
            }
            return Json(new { msj, status });
        }

        [HttpPost]
        [Authorize(Permissions.Permissions.GestionarUsuarios.BlockAndUnclock)]
        public async Task<JsonResult> Bloquear(string id, int operacion)
        {
            var user = await _userManager.FindByIdAsync(id);
            string? msj=null, status=null;

            if(user != null)
            {
                switch (operacion)
                {
                    case 1:
                        user.LockoutEnd = DateTime.MaxValue.Date;
                        /*actualizar securitystamp para que se invalide la cookie y se verifique en la proxima request
                        al haber baneado al usuario se cierra su sesion aunque este logeado*/
                        user.SecurityStamp = String.Concat(Array.ConvertAll(Guid.NewGuid().ToByteArray(), b => b.ToString("X2")));
                        break;
                    case 2:
                        user.LockoutEnd = null;
                        break;
                }

                IdentityResult result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    switch (operacion)
                    {
                        case 1:
                            msj = "El usuario a sido bloqueado correctamente";
                            status = "success";
                            break;
                        case 2:
                            msj = "El usuario a sido desbloqueado correctamente";
                            status = "success";
                            break;
                    }
                }
                else
                {
                    msj = "El usuario no a sido bloqueado correctamente";
                    status = "error";
                }
            }
            else
            {
                msj = "Error. El usuario no a sido bloqueado correctamente, posiblemente ya a sido eliminado por otro usuario";
                status = "error";
            }

            return Json(new { msj, status });
        }

        public async Task<string> SendEmailConfirmation(ApplicationUser user, string email)
        {
            string? returnUrl = null;
            returnUrl ??= Url.Content("~/");

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme);

            try
            {
                await _emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");
                return "Succeeded";
            }
            catch (Exception)
            {
                return "Error";
            }
        }
    }
}
