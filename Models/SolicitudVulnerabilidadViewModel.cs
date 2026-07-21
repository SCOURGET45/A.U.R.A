using System;

namespace Aura.Models
{
    public class SolicitudVulnerabilidadViewModel
    {
        public int IdSolicitud { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreAlumno { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public string CategoriaMotivo { get; set; } = string.Empty;
        public string JustificacionTutor { get; set; } = string.Empty;
        public DateTime FechaPeticion { get; set; }
    }
}
