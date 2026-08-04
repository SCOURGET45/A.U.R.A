using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var viewModel = new EstudianteDashboardViewModel();

            string userEmail = User.Identity?.Name ?? string.Empty;
            string matriculaClean = !string.IsNullOrEmpty(userEmail) ? userEmail.Split('@')[0].Trim() : string.Empty;

            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int idUsuario = 0;
            if (!string.IsNullOrEmpty(idUsuarioStr))
            {
                int.TryParse(idUsuarioStr, out idUsuario);
            }

            try
            {
                Estudiante? estudiante = null;

                if (idUsuario > 0)
                {
                    estudiante = await _context.Estudiantes
                        .Include(e => e.Grupo)
                        .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);
                }

                if (estudiante == null && !string.IsNullOrEmpty(matriculaClean))
                {
                    estudiante = await _context.Estudiantes
                        .Include(e => e.Grupo)
                        .FirstOrDefaultAsync(e => e.Matricula == matriculaClean || e.Matricula == userEmail);
                }

                if (estudiante != null)
                {
                    viewModel.NombreAlumno = $"{estudiante.Nombre} {estudiante.Apellidos}";
                    viewModel.Matricula = estudiante.Matricula;
                    viewModel.NombreGrupo = estudiante.Grupo?.NombreGrupo ?? "9IDGS-G2";

                    var vulnerabilidad = await _context.SolicitudesVulnerabilidad
                        .Where(s => s.IdEstudiante == estudiante.IdEstudiante && s.Estado == "Aprobado")
                        .OrderByDescending(s => s.FechaCreacion)
                        .FirstOrDefaultAsync();

                    if (vulnerabilidad != null)
                    {
                        viewModel.TieneToleranciaActiva = true;
                        viewModel.MinutosTolerancia = vulnerabilidad.MinutosToleranciaOtorgados > 0 ? vulnerabilidad.MinutosToleranciaOtorgados : 30;
                        viewModel.MotivoTolerancia = vulnerabilidad.CategoriaMotivo ?? "Distancia / Salud";
                    }
                    else
                    {
                        viewModel.TieneToleranciaActiva = estudiante.TieneToleranciaActiva;
                        viewModel.MinutosTolerancia = estudiante.TieneToleranciaActiva ? 30 : 0;
                        viewModel.MotivoTolerancia = estudiante.TieneToleranciaActiva ? "Tolerancia Institucional" : string.Empty;
                    }

                    var justificantes = await _context.Justificantes
                        .Where(j => j.IdEstudiante == estudiante.IdEstudiante)
                        .ToListAsync();

                    viewModel.JustificantesUsados = justificantes.Count;
                    viewModel.DiasAmparados = justificantes.Sum(j => j.DiasAmparados);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Advertencia BD en EstudianteController: " + ex.Message);
            }

            // Buscar en memoria de Secretaría si no se encontró en la BD
            if (string.IsNullOrEmpty(viewModel.NombreAlumno))
            {
                var alumnoMem = SecretariaController._alumnosMemoria
                    .FirstOrDefault(a => a.Matricula.Equals(matriculaClean, StringComparison.OrdinalIgnoreCase) || a.Matricula.Equals(userEmail, StringComparison.OrdinalIgnoreCase));

                if (alumnoMem != null)
                {
                    viewModel.NombreAlumno = $"{alumnoMem.Nombre} {alumnoMem.Apellidos}";
                    viewModel.Matricula = alumnoMem.Matricula;
                    viewModel.NombreGrupo = alumnoMem.NombreGrupo;
                }
            }

            // Fallback por matrícula específica en caso de ser un usuario de demostración
            if (string.IsNullOrEmpty(viewModel.NombreAlumno))
            {
                switch (matriculaClean)
                {
                    case "23301145":
                        viewModel.NombreAlumno = "MARÍA FERNANDA GÓMEZ";
                        viewModel.Matricula = "23301145";
                        break;
                    case "23301199":
                        viewModel.NombreAlumno = "CARLOS EDUARDO PÉREZ";
                        viewModel.Matricula = "23301199";
                        break;
                    case "23301201":
                        viewModel.NombreAlumno = "DANIELA RÍOS CÁRDENAS";
                        viewModel.Matricula = "23301201";
                        break;
                    default:
                        viewModel.NombreAlumno = !string.IsNullOrEmpty(userEmail) && userEmail.Contains("@") ? userEmail.Split('@')[0].ToUpper() : "ALAN SANTIAGO MOLINA";
                        viewModel.Matricula = !string.IsNullOrEmpty(matriculaClean) ? matriculaClean : "23301133";
                        break;
                }

                viewModel.NombreGrupo = "9IDGS-G2";
                viewModel.TieneToleranciaActiva = viewModel.Matricula == "23301133" || viewModel.Matricula == "23301145";
                viewModel.MinutosTolerancia = viewModel.TieneToleranciaActiva ? 30 : 0;
                viewModel.MotivoTolerancia = viewModel.TieneToleranciaActiva ? "Tolerancia Institucional (+30m)" : string.Empty;
                viewModel.JustificantesUsados = 1;
                viewModel.DiasAmparados = 3;
            }

            // Poblado de materias para el panel del estudiante
            if (!viewModel.MateriasActivas.Any())
            {
                viewModel.MateriasActivas = new List<UnidadDashboardViewModel>
                {
                    new UnidadDashboardViewModel
                    {
                        IdMateria = 1,
                        NombreMateria = "Desarrollo Web Profesional",
                        UnidadActual = "Unidad II: APIs RESTful y Seguridad",
                        PorcentajeAsistencia = 92.5,
                        Semaforo = "Verde",
                        FaltasAcumuladas = 1,
                        FaltasPermitidasRestantes = 3,
                        LimiteFaltasTotal = 4,
                        RetardosAcumulados = 2,
                        TotalClasesUnidad = 20,
                        ClasesAsistidas = 19
                    },
                    new UnidadDashboardViewModel
                    {
                        IdMateria = 2,
                        NombreMateria = "Arquitectura de Software",
                        UnidadActual = "Unidad I: Patrones de Diseño y Microservicios",
                        PorcentajeAsistencia = 83.3,
                        Semaforo = "Amarillo",
                        FaltasAcumuladas = 3,
                        FaltasPermitidasRestantes = 1,
                        LimiteFaltasTotal = 4,
                        RetardosAcumulados = 2,
                        TotalClasesUnidad = 18,
                        ClasesAsistidas = 15
                    },
                    new UnidadDashboardViewModel
                    {
                        IdMateria = 3,
                        NombreMateria = "Administración de Proyectos de TI",
                        UnidadActual = "Unidad II: Metodologías Ágiles y Scrum",
                        PorcentajeAsistencia = 76.0,
                        Semaforo = "Rojo",
                        FaltasAcumuladas = 5,
                        FaltasPermitidasRestantes = 0,
                        LimiteFaltasTotal = 4,
                        RetardosAcumulados = 1,
                        TotalClasesUnidad = 21,
                        ClasesAsistidas = 16
                    },
                    new UnidadDashboardViewModel
                    {
                        IdMateria = 4,
                        NombreMateria = "Inglés IX",
                        UnidadActual = "Unidad III: Technical Presentations",
                        PorcentajeAsistencia = 95.0,
                        Semaforo = "Verde",
                        FaltasAcumuladas = 0,
                        FaltasPermitidasRestantes = 3,
                        LimiteFaltasTotal = 3,
                        RetardosAcumulados = 1,
                        TotalClasesUnidad = 16,
                        ClasesAsistidas = 16
                    }
                };

                viewModel.AsistenciaGlobal = Math.Round(viewModel.MateriasActivas.Average(m => m.PorcentajeAsistencia), 1);
                viewModel.AlertasRiesgoCount = viewModel.MateriasActivas.Count(m => m.PorcentajeAsistencia < 80);
            }

            return View(viewModel);
        }
    }
}
