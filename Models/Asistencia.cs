using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Aura.Models
{
    [Table("Asistencias")]
    public class Asistencia
    {
        [Key]
        public int IdAsistencia { get; set; }
        public int IdSesion { get; set; }
        public int IdEstudiante { get; set; }
        public DateTime FechaHoraRegistro { get; set; }
        public string Estado { get; set; }
        public bool ValidacionUltrasonica { get; set; }

        [ForeignKey("IdSesion")]
        public Sesion Sesion { get; set; }
    }
}
