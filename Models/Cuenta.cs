using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.Models
{
    public class Cuenta
    {
        public Cuenta()
        {
            Transacciones = new HashSet<Transaccion>();
        }

        [Key]
        public string Codigo { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public int Clasificacion { get; set; }
        [Required]
        public int Tipo { get; set; }
        [Required]
        public int TipoSaldo { get; set; }

        public virtual Cuenta? Cuentas { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
    }
}
