using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaContableCSG.Data;
using SistemaContableCSG.Models;
using SistemaContableCSG.ViewModels;

namespace SistemaContableCSG.Controllers
{
    public class AsientoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AsientoController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }


        // GET: AsientoController
        [Authorize(Permissions.Permissions.GestionarAsientos.View)]
        public ActionResult Index()
        {
            var periodos = _context.Periodo.Where(p => p.Iniciado == true).ToList().Select(p => new
            {
                Id = p.Id,
                Texto = $"{p.FechaInicial.ToShortDateString()} al {p.FechaFinal.ToShortDateString()}"
            }).ToList();

            AsientoIndex asientoIndex = new AsientoIndex();

            asientoIndex.Periodos = new SelectList(periodos, "Id", "Texto");

            return View(asientoIndex);
        }

        // POST: AsientoController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarAsientos.Create)]
        public IActionResult Create(AsientoIndex model, string[] Cuentas, decimal[] Debe, decimal[] Haber)
        {
            var periodo = _context.Periodo.FirstOrDefault(p => p.Id == model.PeriodoId);

            if(periodo == null)
            {
                return Content("Error");

            }

            model.Asiento.Periodo = periodo;

            Asiento asiento = new Asiento();
            asiento.Glosa = model.Asiento.Glosa;
            asiento.Fecha = model.Asiento.Fecha;
            asiento.Periodo = model.Asiento.Periodo;

            _context.Asiento.Add(asiento);

            for (int i=0; i < Debe.Length; i++)
            {
                var cuenta = _context.Cuenta.FirstOrDefault(c => c.Codigo == Cuentas[i == 0 ? i:i+i]);
                Transaccion transaccion = new Transaccion();
                transaccion.Asiento = asiento;
                transaccion.Cuenta = cuenta;
                transaccion.Debe = Debe[i];
                _context.Transaccion.Add(transaccion);
            }

            for (int i = 0; i < Haber.Length; i++)
            {
                var cuenta = _context.Cuenta.FirstOrDefault(c => c.Codigo == Cuentas[i == 0 ? i+1:i+i+1]); 
                Transaccion transaccion = new Transaccion();
                transaccion.Asiento = asiento;
                transaccion.Cuenta = cuenta;
                transaccion.Haber = Haber[i];
                _context.Transaccion.Add(transaccion);
            }

            try
            {
                _context.SaveChanges();
                TempData["status"] = "success";
                TempData["msj"] = "El asiento a sido guardado con exito";
            }
            catch (Exception)
            {
                TempData["status"] = "error";
                TempData["msj"] = "No se pudo guardar el asiento";
            }

            return RedirectToAction("Index");
        }
    }
}
