using System.ComponentModel.DataAnnotations;

namespace Aura.Models
{
    public class NuevoUsuarioViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo institucional es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo institucional válido.")]
        public string CorreoElectronico { get; set; } = string.Empty;

        [Required(ErrorMessage = "La matrícula o número de empleado es obligatorio.")]
        public string MatriculaEmpleado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El rol es obligatorio.")]
        public string NombreRol { get; set; } = "Docente"; // Estudiante, Docente, Tutor, Secretaria, Director

        public string NombreGrupo { get; set; } = "9IDGS-G2";

        public string ContrasenaInicial { get; set; } = "123456";
    }

    public class CambiarPasswordViewModel
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa tu contraseña actual.")]
        public string ContrasenaActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ingresa la nueva contraseña.")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma la nueva contraseña.")]
        [Compare("NuevaContrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}
