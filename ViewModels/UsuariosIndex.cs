using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.ViewModels
{
    public class UsuariosIndex
    {
        [ValidateNever]
        public ICollection<IdentityRole> Roles { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [EmailAddress(ErrorMessage = "El {0} ingresado no es un correo electronico valido")]
        [Display(Name = "Correo electronico")]
        public string Email { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [StringLength(100, ErrorMessage = "La {0} de tener por lo menos {2} y como maximo {1} caracteres.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El campo {0} es requerido")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmacion de contraseña")]
        [Compare("Password", ErrorMessage = "La contraseña y el campo de {0} no son iguales.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.PhoneNumber, ErrorMessage = "El {0} no es valido.")]
        [RegularExpression(@"^([0-9]{8})$", ErrorMessage = "El {0} no es valido.")]
        [Display(Name = "Numero de telefono")]
        public string? PhoneNumber { get; set; }
    }
}
