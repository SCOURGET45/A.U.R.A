using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("UnidadesAcademicas")]
    public class UnidadAcademica
    {
        [Key]
        public int IdUnidadAcademica { get; set; }
        public int IdMateria { get; set; }
        public int NumeroUnidad { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalClasesProgramadas { get; set; }
    }
}
