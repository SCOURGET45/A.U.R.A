using System.Collections.Generic;

namespace Aura.Models
{
    public class EstudianteDashboardViewModel
    {
        public string NombreAlumno { get; set; }
        public string Matricula { get; set; }
        public double AsistenciaGlobal { get; set; }
        public int JustificantesUsados { get; set; }
        public int DiasAmparados { get; set; }
        public bool TieneToleranciaActiva { get; set; }

        public List<UnidadDashboard> MateriasActivas { get; set; }
    }

    public class UnidadDashboard
    {
        public string NombreMateria { get; set; }
        public string UnidadActual { get; set; }
        public string Semaforo { get; set; }
        public int FaltasPermitidasRestantes { get; set; }
        public int RetardosAcumulados { get; set; }
    }
}
