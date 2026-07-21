using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string NombreCompleto { get; set; }
        public string CorreoElectronico { get; set; }
        public string ContrasenaHash { get; set; } = "PendienteDeConfigurar";
        public bool Activo { get; set; } = true;
        public Rol Rol { get; set; }
    }
}
