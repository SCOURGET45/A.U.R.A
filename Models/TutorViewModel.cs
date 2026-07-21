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
        public string NivelRiesgo { get; set; } = string.Empty;
        public bool TieneSolicitudEnProceso { get; set; }
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
}
