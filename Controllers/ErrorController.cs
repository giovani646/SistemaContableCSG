using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace SistemaContableCSG.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult AuthError()
        {
            var referrer = HttpContext.Request.Headers["Referer"].ToString();
            //verificar que la funcion se ejcute solo si a sido referida/redireccionada, en este caso desde program.cs
            if (!string.IsNullOrEmpty(referrer))
            {
                ViewBag.msj = new HtmlString("Es posible que haya presionado el botón Atrás, actualizado durante el inicio de sesión, abierto demasiados diálogos de inicio de sesión o que haya algún problema con las cookies, ya que no pudimos encontrar su sesión. Intente <a class='alert-link' href='/Login'>iniciar sesión</a> de nuevo desde la aplicación y, si el problema persiste, póngase en contacto con el administrador.");
                return View();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
