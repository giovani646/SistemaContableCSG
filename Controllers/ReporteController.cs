using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using SistemaContableCSG.Data;
using System.Linq.Expressions;

namespace SistemaContableCSG.Controllers
{
    public class ReporteController : Controller
    {

        private readonly IPeriodoHelper _periodoHelper;
        private readonly ApplicationDbContext _context;

        public ReporteController(IPeriodoHelper PeriodoHelper, ApplicationDbContext dbContext) {
            _periodoHelper = PeriodoHelper;
            _context = dbContext;
        }

        // GET: ReporteController
        public ActionResult Index(string reporte)
        {
            var periodoIniciado = _periodoHelper.IsPeriodoIniciado();

            if(periodoIniciado)
            {
                return Content("Error, debe cerrar primero el periodo contable");
            }

            var reportes = new List<string>() { "Balance general","Libro diario", "Transacciones por tipo de saldo" };

            if (!reportes.Contains(reporte))
            {
                return BadRequest();
            }

            ViewData["reporte"] = reporte;
            return View();
        }

        public ActionResult BalanceGeneral()
        {
            StringValues header;
            Request.Headers.TryGetValue("Referer", out header);
            if (header.Count == 0) //verificar que se esta accediendo mediante RedirectToAction y no accesando desde url directa
            {
                return BadRequest();
            }

            ViewData["reporte"] = TempData["reporte"];
            ViewData["finicial"] = TempData["finicial"];
            ViewData["ffinal"] = TempData["ffinal"];

            return View();
        }

        public ActionResult Librodiario()
        {
            StringValues header;
            Request.Headers.TryGetValue("Referer", out header);
            if (header.Count == 0) //verificar que se esta accediendo mediante RedirectToAction y no accesando desde url directa
            {
                return BadRequest();
            }

            DateTime fecha = Convert.ToDateTime(TempData["finicial"]);


            ViewData["reporte"] = TempData["reporte"];
            ViewData["finicial"] = fecha.ToShortDateString();
            ViewData["ffinal"] = Convert.ToDateTime(TempData["ffinal"]);


            return View();
        }


        public ActionResult catalogoDiario(String fecha)
        {


            int fechaa = Convert.ToInt32(fecha.Substring(6));
            int fecham = Convert.ToInt32(fecha.Substring(3, 2));
            int fechad = Convert.ToInt32(fecha.Substring(0, 2));
            var date = new DateTime(fechaa, fecham, fechad, 00, 00, 00, 0000000);
            DateTime fecha2 = DateTime.Parse(date.ToString("yyyy-MM-dd"));

            if (!Request.Headers["X-Requested-With"].Equals("XMLHttpRequest"))//comprobar si la solicitud no es ajax
            {
                return NotFound();
            }
            var cuentas = _context.Cuenta.Join(_context.Transaccion, cu => cu.Codigo, tr => tr.Cuenta.Codigo, (cu, tr) => new { cu, tr }).Join(_context.Asiento, tr2 => tr2.tr.Id, asi => asi.Id, (tr2, asi) => new { tr2, asi }).Where(b => b.asi.Fecha == fecha2).Select(r => new { r.tr2.cu.Codigo, r.tr2.cu.Nombre, r.tr2.tr.Debe, r.tr2.tr.Haber }).ToList();

            return Json(new { data = cuentas });


        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Generar(string reporte, DateTime finicial, DateTime ffinal)
        {
            TempData["reporte"] = reporte;
            TempData["finicial"] = finicial.ToShortDateString();
            TempData["ffinal"] = ffinal.ToShortDateString();

            switch (reporte)
            {
                case "Balance general":
                    return RedirectToAction("BalanceGeneral");

                case "Libro diario":
                    return RedirectToAction("Librodiario");

                case "Transacciones por tipo de saldo":
                    return RedirectToAction("TransactTipoSaldo");

                default:
                    return Content("Error, el reporte seleccionado no es valido");
            }
        }

    }
}
