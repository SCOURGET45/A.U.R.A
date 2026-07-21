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
        public int IdTutor { get; set; }
        public string Motivo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;
        public DateTime? FechaJuntaComision { get; set; }
        public string Dictamen { get; set; } = "Pendiente";
        public int? IdDirectorResolucion { get; set; }
        public int MinutosToleranciaOtorgados { get; set; } = 0;
    }
}
