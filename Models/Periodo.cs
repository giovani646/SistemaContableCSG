﻿using System.ComponentModel.DataAnnotations;

namespace SistemaContableCSG.Models
{
    public class Periodo
    {
        public Periodo()
        {
            Asientos = new HashSet<Asiento>();
        }

        public int Id { get; set; }
        [Required]
        public DateTime FechaInicial { get; set; }
        [Required]
        public DateTime FechaFinal { get; set; }
        [Required]
        public bool Iniciado { get; set; }

        public ICollection<Asiento> Asientos { get; set; }
    }
}
