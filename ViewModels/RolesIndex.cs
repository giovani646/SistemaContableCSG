using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.ViewModels
{
    public class RolesIndex
    {
        [Required(ErrorMessage = "El {0} es requerido")]
        [Display(Name = "Nombre")]
        public string Name { get; set; }

        [ValidateNever]
        public List<string> Permisos { get; set; }
    }
}
