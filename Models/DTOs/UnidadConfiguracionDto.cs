using System;

namespace Aura.Models.DTOs
{
    public class UnidadConfiguracionDto
    {
        public int IdGrupo { get; set; }
        public string NombreUnidad { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int TotalClasesProgramadas { get; set; }
    }
}
