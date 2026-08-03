using System;
using System.Collections.Generic;

namespace Aura.Models
{
    public class DocenteMiDiaViewModel
    {
        public string NombreDocente { get; set; } = string.Empty;
        public DateTime FechaActual { get; set; } = DateTime.Now;
        public List<ClaseHoy> Clases { get; set; } = new List<ClaseHoy>();
    }

    public class ClaseHoy
    {
        public int IdSesion { get; set; }
        public string Grupo { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public string EstadoFase { get; set; } = "Pendiente"; // "En Curso", "Completada", "Pendiente"
        public bool AlertasVulnerabilidad { get; set; }
    }

    public class DocenteMisGruposViewModel
    {
        public string NombreDocente { get; set; } = string.Empty;
        public List<DocenteGrupoCardViewModel> Grupos { get; set; } = new List<DocenteGrupoCardViewModel>();
    }

    public class DocenteGrupoCardViewModel
    {
        public int IdGrupo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string Carrera { get; set; } = string.Empty;
        public string Cuatrimestre { get; set; } = string.Empty;
        public string Materia { get; set; } = string.Empty;
        public int TotalAlumnos { get; set; }
        public double PromedioAsistencia { get; set; }
        public int AlumnosEnRiesgoCount { get; set; }
        public int RetardosConvertidosFaltasCount { get; set; }
        public int AlumnosWithToleranciaCount { get; set; }
    }
}
