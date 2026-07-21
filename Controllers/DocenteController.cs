using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Data;

namespace Aura.Controllers
{
    [Authorize(Roles = "Docente")]
    [Route("api/[controller]")]
    [ApiController]
    public class DocenteController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public DocenteController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("DescargarLista/{idGrupo}")]
        public async Task<IActionResult> DescargarListaAsistencia(int idGrupo)
        {
            var grupo = await _context.Grupos.FindAsync(idGrupo);
            if (grupo == null)
            {
                return NotFound("Grupo no encontrado.");
            }

            var estudiantes = await _context.Estudiantes
                .Include(e => e.Usuario)
                .Where(e => e.IdGrupo == idGrupo)
                .OrderBy(e => e.Usuario.NombreCompleto)
                .ToListAsync();

            if (!estudiantes.Any())
            {
                return BadRequest("No hay estudiantes inscritos en este grupo.");
            }

            var builder = new StringBuilder();
            builder.AppendLine("Matricula,Nombre Completo,Grupo");

            foreach (var est in estudiantes)
            {
                builder.AppendLine($"{est.Matricula},{est.Usuario.NombreCompleto},{grupo.NombreGrupo}");
            }

            var bytesFormatoInstitucional = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytesFormatoInstitucional, "text/csv", $"ListaAsistencia_{grupo.NombreGrupo}.csv");
        }
    }
}
