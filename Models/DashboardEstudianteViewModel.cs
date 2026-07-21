using System.Collections.Generic;

namespace Aura.Models
{
    public class DashboardEstudianteViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Matricula { get; set; } = string.Empty;
        public double AsistenciaGlobal { get; set; }
        public int JustificantesEmitidos { get; set; }
        public bool TieneToleranciaActiva { get; set; }

        public List<SemaforoMateriaViewModel> Materias { get; set; } = new List<SemaforoMateriaViewModel>();
    }

    public class SemaforoMateriaViewModel
    {
        public string NombreMateria { get; set; } = string.Empty;
        public int FaltasAcumuladas { get; set; }
        public int LimiteFaltas { get; set; }
        public double PorcentajeAsistencia { get; set; }
        public string ColorSemaforo { get; set; } = string.Empty;
    }
}
