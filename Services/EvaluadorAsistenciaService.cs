using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Aura.Data;

namespace Aura.Services
{
    public class EvaluadorAsistenciaService
    {
        private readonly AuraDbContext _context;

        public EvaluadorAsistenciaService(AuraDbContext context)
        {
            _context = context;
        }

        public async Task<string> DeterminarEstadoLlegada(int idEstudiante, DateTime horaInicioClase, DateTime horaLlegadaAlumno)
        {
            double minutosRetraso = (horaLlegadaAlumno - horaInicioClase).TotalMinutes;

            if (minutosRetraso <= 0) return "Asistencia";

            var toleranciaActiva = await _context.SolicitudesVulnerabilidad
                .Where(s => s.IdEstudiante == idEstudiante && s.Dictamen == "APROBADO")
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefaultAsync();

            int margenGraciaGeneral = 10;
            int limiteRetardoGeneral = 20;

            if (toleranciaActiva != null && toleranciaActiva.MinutosToleranciaOtorgados > 0)
            {
                margenGraciaGeneral += toleranciaActiva.MinutosToleranciaOtorgados;
                limiteRetardoGeneral += toleranciaActiva.MinutosToleranciaOtorgados;
            }

            if (minutosRetraso <= margenGraciaGeneral)
            {
                return "Asistencia";
            }
            else if (minutosRetraso <= limiteRetardoGeneral)
            {
                return "Retardo";
            }
            else
            {
                return "Falta";
            }
        }
    }
}
