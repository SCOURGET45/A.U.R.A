using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Sesiones")]
    public class Sesion
    {
        [Key]
        public int IdSesion { get; set; }
        public int IdGrupo { get; set; }
        public int IdDocente { get; set; }
        public DateTime FechaSesion { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool TokenUltrasónicoActivo { get; set; }
    }
}
