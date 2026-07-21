using System;
using System.ComponentModel.DataAnnotations;

namespace Aura.Models
{
    public class SolicitudVulnerabilidad
    {
        [Key] // <--- Esta etiqueta le dice a SQL que esta es la llave primaria
        public int IdSolicitud { get; set; }

        public int IdEstudiante { get; set; }
        public Estudiante Estudiante { get; set; }

        public string CategoriaMotivo { get; set; }
        public string JustificacionTutor { get; set; }
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaResolucion { get; set; }
        public string Dictamen { get; set; }
        public int MinutosToleranciaOtorgados { get; set; }

        public int? IdTutor { get; set; }
        public string Motivo { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public DateTime? FechaJuntaComision { get; set; }
        public int? IdDirectorResolucion { get; set; }
    }
}