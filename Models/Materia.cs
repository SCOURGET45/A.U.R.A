using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Materias")]
    public class Materia
    {
        [Key]
        public int IdMateria { get; set; }
        public int IdCuatrimestre { get; set; }
        public string NombreMateria { get; set; }
    }
}
