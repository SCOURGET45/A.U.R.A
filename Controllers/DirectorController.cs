using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Authorize(Roles = "Director")]
    [Route("api/[controller]")]
    [ApiController]
    public class DirectorController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public DirectorController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("BandejaVulnerabilidad")]
        public async Task<IActionResult> VerSolicitudesPendientes()
        {
            var pendientes = await _context.SolicitudesVulnerabilidad
                .Where(s => s.Dictamen == "Pendiente")
                .ToListAsync();

            return Ok(pendientes);
        }

        [HttpPost("Dictaminar/{idSolicitud}")]
        public async Task<IActionResult> EmitirDictamen(int idSolicitud, [FromBody] DictamenRequest request)
        {
            var solicitud = await _context.SolicitudesVulnerabilidad.FindAsync(idSolicitud);

            if (solicitud == null)
                return NotFound("Solicitud no encontrada.");

            var idDirector = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            solicitud.Dictamen = request.Estado;
            solicitud.IdDirectorResolucion = idDirector;

            if (request.Estado == "Aprobado")
            {
                solicitud.MinutosToleranciaOtorgados = request.MinutosTolerancia;
            }

            _context.SolicitudesVulnerabilidad.Update(solicitud);
            await _context.SaveChangesAsync();

            return Ok($"La solicitud ha sido {request.Estado}. El panel de la Secretaría ha sido actualizado.");
        }
    }

    public class DictamenRequest
    {
        public string Estado { get; set; }
        public int MinutosTolerancia { get; set; }
    }
}
