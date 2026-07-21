using System.Collections.Generic;
using Aura.Models;

namespace Aura.Services
{
    public interface IAsistenciaService
    {
        ResultadoAsistencia CalcularAsistenciaUnidad(List<RegistroAsistencia> historial, int totalClasesUnidad);
    }
}
