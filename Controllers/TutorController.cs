using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Authorize(Roles = "Tutor")]
    [Route("api/[controller]")]
    [ApiController]
    public class TutorController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public TutorController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpPost("EmitirJustificante")]
        public async Task<IActionResult> EmitirJustificante([FromBody] Justificantes model)
        {
            var cantidadEmitidos = await _context.Justificantes
                .CountAsync(j => j.IdEstudiante == model.IdEstudiante);

            if (cantidadEmitidos >= 2)
            {
                return BadRequest("Bloqueo del sistema: El estudiante ya ha alcanzado el límite de 2 justificantes por cuatrimestre.");
            }

            var diasAcumulados = await _context.Justificantes
                .Where(j => j.IdEstudiante == model.IdEstudiante)
                .SumAsync(j => j.DiasAmparados);

            if ((diasAcumulados + model.DiasAmparados) > 15)
            {
                return BadRequest($"Límite excedido: El alumno tiene {diasAcumulados} días amparados. Un justificante de {model.DiasAmparados} días superaría el máximo de 15 días permitidos.");
            }

            model.FechaEmision = DateTime.Now;
            _context.Justificantes.Add(model);
            await _context.SaveChangesAsync();

            return Ok("Justificante emitido exitosamente. La vista del docente se ha actualizado.");
        }

        [HttpPost("IniciarVulnerabilidad")]
        public async Task<IActionResult> IniciarTramiteVulnerabilidad([FromBody] SolicitudVulnerabilidad solicitud)
        {
            solicitud.FechaSolicitud = DateTime.Now;
            solicitud.Dictamen = "Pendiente";

            _context.SolicitudesVulnerabilidad.Add(solicitud);
            await _context.SaveChangesAsync();

            return Ok("Solicitud enviada directamente a la bandeja del Director.");
        }
    }
}
