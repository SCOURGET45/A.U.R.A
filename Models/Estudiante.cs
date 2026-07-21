using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Estudiantes")]
    public class Estudiante
    {
        [Key]
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public bool TieneToleranciaActiva { get; set; }

        public int IdUsuario { get; set; }
        public Usuario Usuario { get; set; }

        public int IdGrupo { get; set; }
        public Grupo Grupo { get; set; }
    }
}
