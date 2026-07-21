using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Grupos")]
    public class Grupo
    {
        [Key]
        public int IdGrupo { get; set; }
        public int IdCuatrimestre { get; set; }
        public string NombreGrupo { get; set; }
        public int IdTutor { get; set; }
    }
}
