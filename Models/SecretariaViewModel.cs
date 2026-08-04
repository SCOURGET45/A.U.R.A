using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aura.Models
{
    public class SecretariaDashboardViewModel
    {
        public int TotalAlumnosInscritos { get; set; }
        public int TotalGruposActivos { get; set; }
        public int TotalAlumnosVulnerables { get; set; }
        public int TotalMateriasConfiguradas { get; set; }

        public List<AlumnoVulnerableViewModel> AlumnosVulnerables { get; set; } = new List<AlumnoVulnerableViewModel>();
        public List<AlumnoEditViewModel> AlumnosRegistrados { get; set; } = new List<AlumnoEditViewModel>();
        public List<AsignacionClaseDocenteViewModel> AsignacionesClases { get; set; } = new List<AsignacionClaseDocenteViewModel>();
        public List<AsignacionGrupoTutorViewModel> AsignacionesTutores { get; set; } = new List<AsignacionGrupoTutorViewModel>();
    }

    public class AlumnoVulnerableViewModel
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreGrupo { get; set; } = string.Empty;
        public string NombreTutor { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public int MinutosTolerancia { get; set; }
        public DateTime FechaAprobacion { get; set; }
    }

    public class AlumnoEditViewModel
    {
        public int IdEstudiante { get; set; }

        [Required(ErrorMessage = "La matrícula es obligatoria.")]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string Apellidos { get; set; } = string.Empty;

        [Required(ErrorMessage = "El grupo es obligatorio.")]
        public string NombreGrupo { get; set; } = string.Empty;
    }

    public class AsignacionClaseDocenteViewModel
    {
        public int IdAsignacion { get; set; }

        [Required(ErrorMessage = "El nombre del docente es obligatorio.")]
        public string NombreDocente { get; set; } = string.Empty;

        [Required(ErrorMessage = "La asignatura es obligatoria.")]
        public string NombreMateria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El grupo es obligatorio.")]
        public string NombreGrupo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El horario es obligatorio.")]
        public string Horario { get; set; } = string.Empty;
    }

    public class AsignacionGrupoTutorViewModel
    {
        public int IdAsignacion { get; set; }

        [Required(ErrorMessage = "El nombre del tutor es obligatorio.")]
        public string NombreTutor { get; set; } = string.Empty;

        [Required(ErrorMessage = "El grupo es obligatorio.")]
        public string NombreGrupo { get; set; } = string.Empty;

        public string Carrera { get; set; } = "Desarrollo de Software Multiplataforma";
        public int TotalAlumnos { get; set; } = 28;
    }

    public class ConfigurarUnidadViewModel
    {
        public int IdMateria { get; set; }
        public string NombreMateria { get; set; } = string.Empty;

        [Required(ErrorMessage = "El número de unidades es obligatorio.")]
        [Range(1, 10, ErrorMessage = "El número de unidades debe estar entre 1 y 10.")]
        public int NumeroUnidades { get; set; } = 3;

        [Required(ErrorMessage = "El total de clases planificadas es obligatorio.")]
        [Range(5, 100, ErrorMessage = "El total de clases debe estar entre 5 y 100.")]
        public int TotalClasesCuatrimestre { get; set; } = 60;
    }
}
