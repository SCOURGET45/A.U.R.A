using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using Aura.Data;
using Aura.Models.DTOs;

namespace Aura.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionAcademicaController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public ConfiguracionAcademicaController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpPost("DefinirUnidades")]
        public async Task<IActionResult> DefinirUnidadesMaterias([FromBody] List<UnidadConfiguracionDto> unidades)
        {
            if (unidades == null || unidades.Count == 0)
                return BadRequest("Debe enviar al menos una unidad para configurar.");

            // Aquí iría la inserción masiva en una tabla de Unidades.
            // Ejemplo:
            // foreach (var u in unidades)
            // {
            //     _context.Unidades.Add(new Unidad { ... });
            // }
            // await _context.SaveChangesAsync();

            return Ok(new { Mensaje = $"Se han configurado {unidades.Count} unidades temáticas correctamente." });
        }
    }
}
