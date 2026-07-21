using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciaController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public AsistenciaController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarAsistenciaUltrasonica([FromBody] RegistroAsistenciaDto dto)
        {
            var sesion = await _context.Sesiones.FindAsync(dto.IdSesion);
            if (sesion == null) return NotFound("Sesión no encontrada.");

            bool yaRegistrado = await _context.Asistencias
                .AnyAsync(a => a.IdSesion == dto.IdSesion && a.IdEstudiante == dto.IdEstudiante);

            if (yaRegistrado) return BadRequest("El alumno ya tiene un registro de asistencia para esta sesión.");

            int margenAsistencia = 10;
            int limiteRetardo = 20;

            var vulnerabilidad = await _context.SolicitudesVulnerabilidad
                .Where(v => v.IdEstudiante == dto.IdEstudiante && v.Dictamen == "Aprobado")
                .OrderByDescending(v => v.FechaCreacion)
                .FirstOrDefaultAsync();

            if (vulnerabilidad != null)
            {
                margenAsistencia += vulnerabilidad.MinutosToleranciaOtorgados;
                limiteRetardo += vulnerabilidad.MinutosToleranciaOtorgados;
            }

            TimeSpan horaLlegada = dto.HoraLlegada.TimeOfDay;
            double minutosRetraso = (horaLlegada - sesion.HoraInicio).TotalMinutes;

            string estadoFinal = "Asistencia";

            if (minutosRetraso > limiteRetardo)
            {
                estadoFinal = "Falta";
            }
            else if (minutosRetraso > margenAsistencia)
            {
                estadoFinal = "Retardo";
            }

            if (estadoFinal == "Retardo")
            {
                int retardosAcumulados = await _context.Asistencias
                    .Include(a => a.Sesion)
                    .CountAsync(a => a.IdEstudiante == dto.IdEstudiante
                                  && a.Sesion.IdGrupo == sesion.IdGrupo
                                  && a.Estado == "Retardo");

                if ((retardosAcumulados + 1) % 3 == 0)
                {
                    estadoFinal = "Falta";
                }
            }

            var nuevaAsistencia = new Asistencia
            {
                IdSesion = dto.IdSesion,
                IdEstudiante = dto.IdEstudiante,
                FechaHoraRegistro = dto.HoraLlegada,
                Estado = estadoFinal,
                ValidacionUltrasonica = true
            };

            _context.Asistencias.Add(nuevaAsistencia);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Mensaje = "Pase de lista ultrasónico exitoso.",
                EstadoFinal = estadoFinal,
                MinutosRetrasoReales = Math.Max(0, Math.Round(minutosRetraso, 1))
            });
        }
    }

    public class RegistroAsistenciaDto
    {
        public int IdEstudiante { get; set; }
        public int IdSesion { get; set; }
        public DateTime HoraLlegada { get; set; }
    }
}
