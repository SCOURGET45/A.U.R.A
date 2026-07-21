using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Unidades")]
    public class Unidad
    {
        [Key]
        public int IdUnidad { get; set; }
        public int IdMateria { get; set; }
        public string NombreUnidad { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalClasesProgramadas { get; set; }
    }
}
