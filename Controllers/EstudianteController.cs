using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Authorize(Roles = "Estudiante")]
    [Route("Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly AuraDbContext _context;

        public EstudianteController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var estudiante = await _context.Estudiantes
                .Include(e => e.Usuario)
                .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);

            if (estudiante == null)
            {
                return NotFound();
            }

            int justificantesUsados = await _context.Justificantes
                .CountAsync(j => j.IdEstudiante == estudiante.IdEstudiante);

            var viewModel = new DashboardEstudianteViewModel
            {
                Nombre = estudiante.Usuario?.NombreCompleto ?? "Estudiante",
                Matricula = estudiante.Matricula,
                AsistenciaGlobal = 88.5,
                JustificantesEmitidos = justificantesUsados,
                TieneToleranciaActiva = estudiante.TieneToleranciaActiva,
                Materias = new List<SemaforoMateriaViewModel>
                {
                    new SemaforoMateriaViewModel
                    {
                        NombreMateria = "Arquitectura de Software",
                        FaltasAcumuladas = 1,
                        LimiteFaltas = 6,
                        PorcentajeAsistencia = 95.0,
                        ColorSemaforo = "success"
                    },
                    new SemaforoMateriaViewModel
                    {
                        NombreMateria = "Base de Datos Avanzadas",
                        FaltasAcumuladas = 4,
                        LimiteFaltas = 5,
                        PorcentajeAsistencia = 82.0,
                        ColorSemaforo = "warning"
                    },
                    new SemaforoMateriaViewModel
                    {
                        NombreMateria = "Inglés Técnico",
                        FaltasAcumuladas = 6,
                        LimiteFaltas = 6,
                        PorcentajeAsistencia = 75.0,
                        ColorSemaforo = "danger"
                    }
                }
            };

            return View(viewModel);
        }
    }
}
