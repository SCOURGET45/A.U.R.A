using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Aura.Data;

namespace Aura.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocenteController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public DocenteController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("ExportarLista/{idGrupo}")]
        public async Task<IActionResult> DescargarListaAsistencia(int idGrupo)
        {
            var estudiantes = await _context.Estudiantes
                .Where(e => e.IdGrupo == idGrupo)
                .ToListAsync();

            if (!estudiantes.Any())
                return NotFound("No se encontraron estudiantes en este grupo.");

            var builder = new StringBuilder();
            builder.AppendLine("Matricula,Nombre del Alumno,Asistencias,Faltas,Retardos,Porcentaje");

            foreach (var est in estudiantes)
            {
                int asistencias = 10;
                int faltas = 1;
                int retardos = 2;
                double porcentaje = 92.5;

                builder.AppendLine($"{est.Matricula},Alumno_{est.IdUsuario},{asistencias},{faltas},{retardos},{porcentaje}%");
            }

            var fileBytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(fileBytes, "text/csv", $"ListaAsistencia_Grupo_{idGrupo}_{System.DateTime.Now:yyyyMMdd}.csv");
        }
    }
}
