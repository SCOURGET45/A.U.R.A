using System;
using System.Collections.Generic;

namespace Aura.Models
{
    public class DocenteMiDiaViewModel
    {
        public string NombreDocente { get; set; }
        public DateTime FechaActual { get; set; }
        public List<ClaseHoy> Clases { get; set; }
    }

    public class ClaseHoy
    {
        public int IdSesion { get; set; }
        public string Grupo { get; set; }
        public string Materia { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string EstadoFase { get; set; }
        public bool AlertasVulnerabilidad { get; set; }
    }
}
