using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaContableCSG.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.ViewModels
{
    public class AsientoIndex
    {
        public Asiento Asiento { get; set; }
        public SelectList Periodos { get; set; }
        public int PeriodoId { get; set; }
    }
}
