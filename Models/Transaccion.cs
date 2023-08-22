using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaContableCSG.Models
{
    public class Transaccion
    {
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Debe { get; set; }
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Haber { get; set; }

        public virtual Asiento? Asiento { get; set; }
        public virtual Cuenta? Cuenta { get; set; }
    }
}
