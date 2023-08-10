using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        public string Nombre { get; set; }
    }
}
