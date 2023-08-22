using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.Models
{
    public class Bitacora
    {
        public int Id { get; set; }
        [Required]
        public string Accion { get; set; }

        public virtual ApplicationUser? User { get; set; }
    }
}
