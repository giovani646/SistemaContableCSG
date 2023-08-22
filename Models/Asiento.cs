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
        public string Glosa { get; set; }
        [Required]
        public DateTime Fecha { get; set; }

        public virtual Periodo? Periodo { get; set; }
        public ICollection<Transaccion> Transacciones { get; set; }
    }
}
