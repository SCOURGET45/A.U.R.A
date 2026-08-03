using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Aura.Models
{
    public class MisTutoradosViewModel
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public double AsistenciaGlobal { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty; // "Bajo", "Medio", "Alto"
        public bool TieneSolicitudEnProceso { get; set; }
        public int JustificantesUsados { get; set; }
        public int JustificantesMaximos { get; set; } = 2;
        public int DiasAmparadosTotales { get; set; }
        public int DiasAmparadosMaximos { get; set; } = 15;
        public DateTime? FechaJuntaComision { get; set; }
        public string DictamenFinal { get; set; } = string.Empty;
    }

    public class CrearSolicitudViewModel
    {
        public int IdEstudiante { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debes seleccionar una categoría.")]
        public string CategoriaMotivo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La justificación es obligatoria.")]
        [StringLength(500, MinimumLength = 20, ErrorMessage = "Debes detallar el caso (mínimo 20 caracteres).")]
        public string JustificacionTutor { get; set; } = string.Empty;
    }

    public class EmitirJustificanteViewModel
    {
        public int IdEstudiante { get; set; }
        public string NombreEstudiante { get; set; } = string.Empty;
        public string MatriculaEstudiante { get; set; } = string.Empty;

        public int JustificantesPreviosCount { get; set; }
        public int DiasAmparadosPreviosCount { get; set; }

        [Required(ErrorMessage = "Indica el número de días a amparar.")]
        [Range(1, 15, ErrorMessage = "El número de días debe estar entre 1 y 15.")]
        public int DiasAmparados { get; set; } = 1;

        [Required(ErrorMessage = "El motivo o diagnóstico médico es obligatorio.")]
        [StringLength(300, MinimumLength = 10, ErrorMessage = "El motivo debe contener al menos 10 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
