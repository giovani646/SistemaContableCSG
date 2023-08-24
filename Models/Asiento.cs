using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.Models
{
    public class Asiento
    {
        public Asiento()
        {
            Transacciones = new HashSet<Transaccion>();
        }

        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public string Glosa { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")]
        public DateTime Fecha { get; set; }

        public virtual Periodo? Periodo { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
    }
}
