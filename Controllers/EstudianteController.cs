using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;
using Aura.Services;

namespace Aura.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstudianteController : ControllerBase
    {
        private readonly AuraDbContext _context;
        private readonly IAsistenciaService _asistenciaService;

        public EstudianteController(AuraDbContext context, IAsistenciaService asistenciaService)
        {
            _context = context;
            _asistenciaService = asistenciaService;
        }

        [HttpGet("Dashboard/{idEstudiante}/Unidad/{idUnidad}")]
        public async Task<IActionResult> ObtenerDashboardUnidad(int idEstudiante, int idUnidad)
        {
            var unidad = await _context.Unidades.FindAsync(idUnidad);
            if (unidad == null)
                return NotFound("Unidad no encontrada.");

            var historialAsistencias = await _context.Asistencias
                .Where(a => a.IdEstudiante == idEstudiante)
                .ToListAsync();

            var registrosParaServicio = historialAsistencias.Select(a => new RegistroAsistencia
            {
                Estado = a.Estado.ToLower() == "retardo" ? TipoAsistencia.Retardo :
                         a.Estado.ToLower() == "falta" ? TipoAsistencia.Falta :
                         TipoAsistencia.Asistencia
            }).ToList();

            var resultadoEvaluacion = _asistenciaService.CalcularAsistenciaUnidad(
                registrosParaServicio,
                unidad.TotalClasesProgramadas
            );

            var dashboardResponse = new
            {
                UnidadActual = unidad.NombreUnidad,
                TotalClasesProgramadas = unidad.TotalClasesProgramadas,
                IndicadoresVisuales = new
                {
                    TermometroGlobal = $"{resultadoEvaluacion.PorcentajeActual}%",
                    SemaforoDeUnidad = resultadoEvaluacion.Semaforo.ToString(),
                    FaltasPermitidasRestantes = resultadoEvaluacion.FaltasPermitidasRestantes
                },
                DesgloseMicro = new
                {
                    Asistencias = registrosParaServicio.Count(x => x.Estado == TipoAsistencia.Asistencia),
                    RetardosAcumulados = registrosParaServicio.Count(x => x.Estado == TipoAsistencia.Retardo),
                    FaltasDirectas = registrosParaServicio.Count(x => x.Estado == TipoAsistencia.Falta)
                }
            };

            return Ok(dashboardResponse);
        }
    }
}
