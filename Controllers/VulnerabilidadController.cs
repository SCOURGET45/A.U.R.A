using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;
using Aura.Models.DTOs;

namespace Aura.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VulnerabilidadController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public VulnerabilidadController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpPost("Solicitar")]
        public async Task<IActionResult> CrearSolicitud([FromBody] NuevaSolicitudDto dto)
        {
            var nuevaSolicitud = new SolicitudVulnerabilidad
            {
                IdEstudiante = dto.IdEstudiante,
                IdTutor = dto.IdTutor,
                Motivo = dto.Motivo,
                Descripcion = dto.Descripcion
            };

            _context.SolicitudesVulnerabilidad.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Solicitud enviada al Director con éxito.", IdSolicitud = nuevaSolicitud.IdSolicitud });
        }

        [HttpPut("Agendar/{idSolicitud}")]
        public async Task<IActionResult> AgendarJunta(int idSolicitud, [FromBody] AgendarJuntaDto dto)
        {
            var solicitud = await _context.SolicitudesVulnerabilidad.FindAsync(idSolicitud);
            if (solicitud == null) return NotFound("Solicitud no encontrada.");

            solicitud.FechaJuntaComision = dto.FechaJunta;
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = $"Junta agendada para el {dto.FechaJunta.ToString("dd/MM/yyyy HH:mm")}." });
        }

        [HttpPut("Dictaminar/{idSolicitud}")]
        public async Task<IActionResult> EmitirDictamen(int idSolicitud, [FromBody] DictamenDto dto)
        {
            var solicitud = await _context.SolicitudesVulnerabilidad.FindAsync(idSolicitud);
            if (solicitud == null) return NotFound("Solicitud no encontrada.");

            solicitud.Dictamen = dto.EstadoDictamen;
            solicitud.IdDirectorResolucion = dto.IdDirector;

            if (dto.EstadoDictamen.ToUpper() == "APROBADO")
            {
                solicitud.MinutosToleranciaOtorgados = dto.MinutosTolerancia;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = $"Dictamen guardado: {solicitud.Dictamen}. Tolerancia otorgada: {solicitud.MinutosToleranciaOtorgados} min." });
        }
    }
}
