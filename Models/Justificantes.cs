using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Justificantes")]
    public class Justificantes
    {
        [Key]
        public int IdJustificante { get; set; }
        public int IdEstudiante { get; set; }
        public int IdTutorEmisor { get; set; }
        public int DiasAmparados { get; set; }
        public string Motivo { get; set; }
        public DateTime FechaEmision { get; set; } = DateTime.Now;
    }
}
