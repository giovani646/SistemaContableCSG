using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaContableCSG.Data;
using SistemaContableCSG.Models;
using SistemaContableCSG.ViewModels;
using System.Linq;

namespace SistemaContableCSG.Controllers
{
    public class CuentaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CuentaController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        // GET: CuentaController
        [Authorize(Permissions.Permissions.GestionarCuentas.View)]
        public ActionResult Index()
        {
            var cuentas = _context.Cuenta.ToList();
            CuentaIndex cuentaIndex = new CuentaIndex();
            cuentaIndex.Cuentas = new SelectList(cuentas, "Codigo", "Nombre");

            return View(cuentaIndex);
        }

        [HttpPost]
        [Authorize(Permissions.Permissions.GestionarCuentas.View)]
        public JsonResult cuentasData()
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
            List<Cuenta> lista = new();


            IQueryable<Cuenta> queryCuenta = _context.Cuenta;//select * from cuenta

            // Total de registros antes de filtrar.
            int TotalRegistros = queryCuenta.Count();

            queryCuenta = queryCuenta.Where(e => string.Concat(e.Codigo, e.Nombre).Contains(ValorBuscado));

            // Total de registros ya filtrados.
            int TotalRegistrosFiltrados = queryCuenta.Count();

            if (sortColumnDirection.Equals("asc"))
            {
                lista = queryCuenta.Skip(OmitirRegistros).Take(CantidadRegistros).OrderBy(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }
            else
            {
                lista = queryCuenta.Skip(OmitirRegistros).Take(CantidadRegistros).OrderByDescending(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }

            return Json(new
            {
                draw = NroPeticion,
                recordsTotal = TotalRegistros,
                recordsFiltered = TotalRegistrosFiltrados,
                data = lista,
            });
        }

        // POST: CuentaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarCuentas.Create)]
        public JsonResult Create(CuentaIndex model)
        {
            string msj, status;

            if (!ModelState.IsValid)
            {
                msj = "Error. Los datos no son validos";
                status = "error";
                return Json(new { msj, status });
            }

            int cuentaExist = _context.Cuenta.Where(c => c.Codigo == model.Cuenta.Codigo).Count();

            if (cuentaExist > 0)
            {
                msj = "Error. La cuenta con el codigo: " + model.Cuenta.Codigo + " ya existe";
                status = "error";
                return Json(new { msj, status });
            }

            if (!string.IsNullOrEmpty(model.CuentaPadre))
            {
                var cuentaPadre = _context.Cuenta.FirstOrDefault(c => c.Codigo == model.CuentaPadre);//buscar el objeto cuenta basada en el codigo obtenido en la propiedad model.CuentaPadre (cuenta padre seleccionada en la vista) 
                model.Cuenta.Cuentas = cuentaPadre;
            }

            _context.Cuenta.Add(model.Cuenta); //guardar los datos de la cuenta y el objeto de la cuenta padre en la propiedad model.Cuenta.Cuentas (autereferencia del modelo), asp.net core detecta que el solo que debe guardar unicamente la pk en el campo CuentaCodigo
            _context.SaveChanges();
            msj = "La cuenta a sido creada correctamente";
            status = "success";
            return Json(new { msj, status });
        }

        // GET: CuentaController/Edit/5
        [HttpGet]
        [Authorize(Permissions.Permissions.GestionarCuentas.Edit)]
        public IActionResult editarData(string IdCuenta)
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))//comprobar si la solicitud no es ajax
            {
                return NotFound();
            }

            string msj, status;
            var cuenta = _context.Cuenta.Include(c => c.Cuentas).FirstOrDefault(c => c.Codigo == IdCuenta);

            if (cuenta == null)
            {
                msj = "Error. la cuenta no a sido encontrada";
                status = "error";
                return Json(new { msj, status });
            }

            return Json(new { cuenta });
        }

        // POST: CuentaController/Edit/5
        [HttpPut]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarCuentas.Edit)]
        public JsonResult Edit(CuentaIndex model,string id)
        {
            string msj, status;
            
            ModelState.Remove("Cuenta.Codigo");

            if (!ModelState.IsValid)
            {
                msj = "Error. los datos proporcionados no son validos";
                status = "error";
                return Json(new { msj,status });
            }

            var cuenta = _context.Cuenta.Include(c => c.Cuentas).FirstOrDefault(c => c.Codigo == id);

            if (cuenta == null)
            {
                msj = "Error. la cuenta no a sido encontrada";
                status = "error";
                return Json(new { msj, status });
            }

            //cuenta.Codigo = model.Cuenta.Codigo;
            cuenta.Nombre = model.Cuenta.Nombre;
            cuenta.Clasificacion = model.Cuenta.Clasificacion;

            if (!string.IsNullOrEmpty(model.CuentaPadre))
            {
                var cuentaPadre = _context.Cuenta.FirstOrDefault(c => c.Codigo == model.CuentaPadre);//buscar el objeto cuenta basada en el codigo obtenido en la propiedad model.CuentaPadre (cuenta padre seleccionada en la vista) 
                cuenta.Cuentas = cuentaPadre;
            }
            
            cuenta.Tipo = model.Cuenta.Tipo;
            cuenta.TipoSaldo = model.Cuenta.TipoSaldo;
            _context.Cuenta.Update(cuenta);
            _context.SaveChanges();
            msj = "La cuenta a sido actualizada correctamente";
            status = "success";

            return Json(new { msj, status });
        }

        // GET: CuentaController/Delete/5
        [HttpDelete]
        [Authorize(Permissions.Permissions.GestionarCuentas.Delete)]
        public JsonResult Delete(string id)
        {
            string msj, status;

            var cuenta = _context.Cuenta.FirstOrDefault(c => c.Codigo == id);

            if(cuenta == null)
            {
                msj = "Error. la cuenta no a sido encontrada";
                status = "error";
                return Json(new { msj, status });
            }

            _context.Cuenta.Remove(cuenta);
            _context.SaveChanges();
            msj = "La cuenta a sido eliminada correctamente";
            status = "success";
            return Json(new { msj, status });
        }

        public IActionResult Catalogo()
        {
            return View();
        }

        [HttpGet]
        public IActionResult catalogoData()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))//comprobar si la solicitud no es ajax
            {
                return NotFound();
            }

            var cuentas = _context.Cuenta.Select(c => new {c.Codigo,c.Nombre}).ToList();

            return Json(new { data = cuentas });
        }

        [HttpGet]
        public IActionResult asientoCuentasData()
        {
            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))//comprobar si la solicitud no es ajax
            {
                return NotFound();
            }

            var cuentas = _context.Cuenta.Where(c => c.Clasificacion == 2).Select(c => new { c.Codigo, c.Nombre }).ToList();

            return Json(new { data = cuentas });
        }
    }
}
