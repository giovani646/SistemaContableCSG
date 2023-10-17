using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System.Linq.Expressions;

namespace SistemaContableCSG.Controllers
{
    public class ReporteController : Controller
    {

        private readonly IPeriodoHelper _periodoHelper;

        public ReporteController(IPeriodoHelper PeriodoHelper) {
            _periodoHelper = PeriodoHelper;
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
                    return RedirectToAction("LibroDiario");

                case "Transacciones por tipo de saldo":
                    return RedirectToAction("TransactTipoSaldo");

                default:
                    return Content("Error, el reporte seleccionado no es valido");
            }
        }

    }
}
