using System.Collections.Generic;

namespace Aura.Models.DTOs
{
    public class ResultadoCargaDto
    {
        public int RegistrosExitosos { get; set; }
        public List<LogErrorCarga> Errores { get; set; } = new List<LogErrorCarga>();
    }
}
