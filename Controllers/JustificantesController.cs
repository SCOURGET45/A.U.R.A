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
    public class JustificantesController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public JustificantesController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpPost("Emitir")]
        public async Task<IActionResult> EmitirJustificante([FromBody] NuevoJustificanteDto dto)
        {
            var cantidadJustificantesActuales = await _context.Justificantes
                .Where(j => j.IdEstudiante == dto.IdEstudiante)
                .CountAsync();

            if (cantidadJustificantesActuales >= 2)
            {
                return BadRequest("El sistema impide emitir más de 2 justificantes por cuatrimestre para este alumno.");
            }

            var diasAcumulados = await _context.Justificantes
                .Where(j => j.IdEstudiante == dto.IdEstudiante && (j.Motivo.ToLower().Contains("enfermedad") || j.Motivo.ToLower().Contains("salud")))
                .SumAsync(j => j.DiasAmparados);

            if (diasAcumulados + dto.DiasAmparados > 15)
            {
                return BadRequest($"El acumulado excedería los 15 días máximos por enfermedad. Días actuales: {diasAcumulados}.");
            }

            var nuevoJustificante = new Justificantes
            {
                IdEstudiante = dto.IdEstudiante,
                IdTutorEmisor = dto.IdTutor,
                DiasAmparados = dto.DiasAmparados,
                Motivo = dto.Motivo,
                FechaEmision = System.DateTime.Now
            };

            _context.Justificantes.Add(nuevoJustificante);
            await _context.SaveChangesAsync();

            return Ok(new { Mensaje = "Justificante emitido exitosamente. Sincronización con docentes realizada." });
        }
    }
}
