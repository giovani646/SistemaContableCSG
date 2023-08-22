using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaContableCSG.Models;
using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.ViewModels
{
    public class CuentaIndex
    {
        [Required]
        public Cuenta Cuenta { get; set; }
        public SelectList? Cuentas { get; set; }
        public string? CuentaPadre { get; set; }
    }
}
