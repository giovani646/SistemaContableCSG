using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaContableCSG.Data;
using SistemaContableCSG.Models;

namespace SistemaContableCSG.Controllers
{
    public class PeriodoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PeriodoController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
        }

        // GET: PeriodoController
        [Authorize(Permissions.Permissions.GestionarPeriodos.View)]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Permissions.Permissions.GestionarPeriodos.View)]
        public JsonResult periodosData()
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
            List<Periodo> lista = new();


            IQueryable<Periodo> queryPeriodo = _context.Periodo;//select * from periodo

            // Total de registros antes de filtrar.
            int TotalRegistros = queryPeriodo.Count();

            queryPeriodo = queryPeriodo.Where(e => string.Concat(e.FechaInicial.ToString(), e.FechaFinal.ToString(), e.Iniciado.ToString()).Contains(ValorBuscado));

            // Total de registros ya filtrados.
            int TotalRegistrosFiltrados = queryPeriodo.Count();

            if (sortColumnDirection.Equals("asc"))
            {
                lista = queryPeriodo.Skip(OmitirRegistros).Take(CantidadRegistros).OrderBy(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }
            else
            {
                lista = queryPeriodo.Skip(OmitirRegistros).Take(CantidadRegistros).OrderByDescending(p => EF.Property<object>(p, sortColumnNormalized)).ToList();
            }

            return Json(new
            {
                draw = NroPeticion,
                recordsTotal = TotalRegistros,
                recordsFiltered = TotalRegistrosFiltrados,
                data = lista,
            });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Permissions.Permissions.GestionarPeriodos.Create)]
        public JsonResult Create(Periodo periodo)
        {
            string msj, status;

            if (!ModelState.IsValid)
            {
                msj = "Error. Los datos proporcionados no son validos";
                status = "error";
                return Json(new { msj, status });
            }

            if(periodo.FechaFinal <= periodo.FechaInicial)
            {
                msj = "Error. La fecha final debe ser mayor que la fecha inicial";
                status = "error";
                return Json(new { msj, status });
            }

            periodo.Iniciado = false;
            _context.Periodo.Add(periodo);
            _context.SaveChanges();

            msj = "El periodo a sido agregado con exito";
            status = "success";

            return Json(new { msj, status });
        }

        [HttpPost]
        [Authorize(Permissions.Permissions.GestionarPeriodos.ActivarDesactivar)]
        public JsonResult ActivarDesactivar(int id, int operacion)
        {
            string msj, status;
            var periodosCount = _context.Periodo.Where(x => x.Iniciado == true).Count();

            if(periodosCount > 0 && operacion == 1)
            {
                msj = "Error. No es posible activar el periodo debido a que ya se encuentra un periodo actualmente activo";
                status = "error";
                return Json(new { msj, status });
            }

            var periodo = _context.Periodo.FirstOrDefault(x => x.Id == id);

            if (periodo == null)
            {
                msj = "Error. No a sido posible encontrar el periodo";
                status = "error";
                return Json(new { msj, status });
            }

            if (operacion == 1)
            {
                periodo.Iniciado = true;
                _context.SaveChanges();
                msj = "Periodo activado correctamente";
                status = "success";
                return Json(new { msj, status });

            }
            else
            {
                periodo.Iniciado = false;
                _context.SaveChanges();
                msj = "Periodo desactivado correctamente";
                status = "success";
                return Json(new { msj, status });
            }
        }
    }
}
