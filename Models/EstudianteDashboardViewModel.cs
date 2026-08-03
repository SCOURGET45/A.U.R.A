using System.Collections.Generic;

namespace Aura.Models
{
    public class EstudianteDashboardViewModel
    {
        public string NombreAlumno { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public double AsistenciaGlobal { get; set; }
        public int JustificantesUsados { get; set; }
        public int JustificantesMaximos { get; set; } = 2;
        public int DiasAmparados { get; set; }
        public int DiasAmparadosMaximos { get; set; } = 15;
        public bool TieneToleranciaActiva { get; set; }
        public int MinutosTolerancia { get; set; } = 0;
        public string MotivoTolerancia { get; set; } = string.Empty;
        public int AlertasRiesgoCount { get; set; }

        public List<UnidadDashboardViewModel> MateriasActivas { get; set; } = new List<UnidadDashboardViewModel>();
    }

    public class UnidadDashboardViewModel
    {
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; } = string.Empty;
        public string UnidadActual { get; set; } = string.Empty;
        public double PorcentajeAsistencia { get; set; }
        public string Semaforo { get; set; } = "Verde"; // "Verde", "Amarillo", "Rojo"
        public int FaltasAcumuladas { get; set; }
        public int FaltasPermitidasRestantes { get; set; }
        public int LimiteFaltasTotal { get; set; }
        public int RetardosAcumulados { get; set; }
        public int TotalClasesUnidad { get; set; }
        public int ClasesAsistidas { get; set; }
    }
}
