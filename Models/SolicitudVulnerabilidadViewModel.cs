using System;

namespace Aura.Models
{
    public class SolicitudVulnerabilidadViewModel
    {
        public int IdSolicitud { get; set; }
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreAlumno { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public string CategoriaMotivo { get; set; } = string.Empty;
        public string JustificacionTutor { get; set; } = string.Empty;
        public string NombreTutor { get; set; } = string.Empty;
        public DateTime FechaPeticion { get; set; }
        public DateTime? FechaJuntaComision { get; set; }
        public int MinutosToleranciaOtorgados { get; set; } = 30;
        public string Estado { get; set; } = "Pendiente";
        public bool Seleccionado { get; set; }
    }

    public class DirectorMonitorViewModel
    {
        public int TotalEstudiantes { get; set; }
        public int AlumnosEnRiesgo { get; set; }
        public int CasosVulnerablesActivos { get; set; }
        public double PromedioAsistenciaDivisional { get; set; }

        public List<EstadisticaCarreraViewModel> Carreras { get; set; } = new List<EstadisticaCarreraViewModel>();
    }

    public class EstadisticaCarreraViewModel
    {
        public string NombreCarrera { get; set; } = string.Empty;
        public int TotalGrupos { get; set; }
        public double PromedioAsistencia { get; set; }
        public int AlumnosEnRiesgo { get; set; }
    }
}
