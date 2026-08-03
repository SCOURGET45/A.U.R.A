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
    public class EstudianteController : Controller
    {
        private readonly AuraDbContext _context;

        public EstudianteController(AuraDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int idUsuario = 0;
            if (!string.IsNullOrEmpty(idUsuarioStr))
            {
                int.TryParse(idUsuarioStr, out idUsuario);
            }

            Estudiante? estudiante = null;
            if (idUsuario > 0)
            {
                estudiante = await _context.Estudiantes
                    .Include(e => e.Grupo)
                    .FirstOrDefaultAsync(e => e.IdUsuario == idUsuario);
            }

            if (estudiante == null)
            {
                estudiante = await _context.Estudiantes
                    .Include(e => e.Grupo)
                    .FirstOrDefaultAsync();
            }

            var viewModel = new EstudianteDashboardViewModel();

            if (estudiante != null)
            {
                viewModel.NombreAlumno = $"{estudiante.Nombre} {estudiante.Apellidos}";
                viewModel.Matricula = estudiante.Matricula;
                viewModel.NombreGrupo = estudiante.Grupo?.NombreGrupo ?? "9IDGS-G2";

                // Consultar vulnerabilidad aprobada
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

                // Justificantes
                var justificantes = await _context.Justificantes
                    .Where(j => j.IdEstudiante == estudiante.IdEstudiante)
                    .ToListAsync();

                viewModel.JustificantesUsados = justificantes.Count;
                viewModel.DiasAmparados = justificantes.Sum(j => j.DiasAmparados);

                // Materias y asistencias
                var asistencias = await _context.Asistencias
                    .Include(a => a.Sesion)
                    .Where(a => a.IdEstudiante == estudiante.IdEstudiante)
                    .ToListAsync();

                if (asistencias.Any())
                {
                    var gruposSesiones = asistencias.GroupBy(a => a.Sesion.IdGrupo);
                    var materiasList = new List<UnidadDashboardViewModel>();

                    foreach (var grupo in gruposSesiones)
                    {
                        int totalClases = Math.Max(grupo.Count(), 20); // Mínimo 20 clases simuladas para la unidad
                        int retardos = grupo.Count(a => a.Estado == "Retardo");
                        int faltasDirectas = grupo.Count(a => a.Estado == "Falta");
                        int faltasPorRetardo = retardos / 3;
                        int faltasTotales = faltasDirectas + faltasPorRetardo;
                        int clasesAsistidas = totalClases - faltasTotales;

                        double porcentaje = Math.Round(((double)clasesAsistidas / totalClases) * 100, 1);
                        int limiteFaltas = (int)Math.Floor(totalClases * 0.20);
                        int faltasRestantes = Math.Max(0, limiteFaltas - faltasTotales);

                        string semaforo = "Verde";
                        if (porcentaje < 80) semaforo = "Rojo";
                        else if (porcentaje < 90) semaforo = "Amarillo";

                        materiasList.Add(new UnidadDashboardViewModel
                        {
                            NombreMateria = "Materia General",
                            UnidadActual = "Unidad II: Evaluaciones",
                            PorcentajeAsistencia = porcentaje,
                            Semaforo = semaforo,
                            FaltasAcumuladas = faltasTotales,
                            FaltasPermitidasRestantes = faltasRestantes,
                            LimiteFaltasTotal = limiteFaltas,
                            RetardosAcumulados = retardos,
                            TotalClasesUnidad = totalClases,
                            ClasesAsistidas = clasesAsistidas
                        });
                    }

                    viewModel.MateriasActivas = materiasList;
                    viewModel.AsistenciaGlobal = Math.Round(materiasList.Average(m => m.PorcentajeAsistencia), 1);
                    viewModel.AlertasRiesgoCount = materiasList.Count(m => m.PorcentajeAsistencia < 80);
                }
            }

            // Si no hay materias activas en DB o datos insuficientes, inicializamos un conjunto representativo de demostración
            if (!viewModel.MateriasActivas.Any())
            {
                if (string.IsNullOrEmpty(viewModel.NombreAlumno))
                {
                    viewModel.NombreAlumno = "Alan Santiago Molina";
                    viewModel.Matricula = "23301133";
                    viewModel.NombreGrupo = "9IDGS-G2";
                    viewModel.TieneToleranciaActiva = true;
                    viewModel.MinutosTolerancia = 30;
                    viewModel.MotivoTolerancia = "Distancia Extrema (Transporte)";
                    viewModel.JustificantesUsados = 1;
                    viewModel.DiasAmparados = 3;
                }

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
