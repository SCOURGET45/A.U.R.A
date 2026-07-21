using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("SolicitudesVulnerabilidad")]
    public class SolicitudVulnerabilidad
    {
        [Key]
        public int IdSolicitud { get; set; }

        public int IdEstudiante { get; set; }
        public Estudiante Estudiante { get; set; }

        public string CategoriaMotivo { get; set; } = string.Empty;
        public string JustificacionTutor { get; set; } = string.Empty;
        public string Estado { get; set; } = "Pendiente";
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime? FechaResolucion { get; set; }
        public string Dictamen { get; set; } = "Pendiente";
        public int MinutosToleranciaOtorgados { get; set; } = 0;
    }
}
