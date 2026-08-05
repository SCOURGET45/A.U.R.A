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

            // Fallback por usuario de sesión en memoria dinámica
            if (string.IsNullOrEmpty(viewModel.NombreAlumno))
            {
                if (AuthController._usuariosDinamicos.ContainsKey(userEmail))
                {
                    viewModel.NombreAlumno = AuthController._usuariosDinamicos[userEmail].Nombre;
                    viewModel.Matricula = matriculaClean;
                }
                else
                {
                    viewModel.NombreAlumno = !string.IsNullOrEmpty(userEmail) && userEmail.Contains("@") ? userEmail.Split('@')[0].ToUpper() : "ALAN SANTIAGO MOLINA";
                    viewModel.Matricula = !string.IsNullOrEmpty(matriculaClean) ? matriculaClean : "23301133";
                }

                viewModel.NombreGrupo = "9IDGS-G2";
                viewModel.TieneToleranciaActiva = viewModel.Matricula == "23301133" || viewModel.Matricula == "23301145";
                viewModel.MinutosTolerancia = viewModel.TieneToleranciaActiva ? 30 : 0;
                viewModel.MotivoTolerancia = viewModel.TieneToleranciaActiva ? "Tolerancia Institucional (+30m)" : string.Empty;
                viewModel.JustificantesUsados = 0;
                viewModel.DiasAmparados = 0;
            }

            // Cálculo 100% REAL de asistencias acumuladas del estudiante
            string cleanMatBusqueda = viewModel.Matricula.Split('@')[0].Trim();

            int totalClasesRegistradas = 0;
            int asistenciasReales = 0;
            int faltasReales = 0;
            int retardosReales = 0;

            foreach (var kvp in DocenteController._historialAsistenciasFDC02)
            {
                if (kvp.Key.StartsWith($"{cleanMatBusqueda}_", StringComparison.OrdinalIgnoreCase))
                {
                    totalClasesRegistradas++;
                    string marca = kvp.Value;
                    if (marca == "." || marca == "+=") asistenciasReales++;
                    else if (marca == "X") { asistenciasReales++; retardosReales++; }
                    else if (marca == "/") faltasReales++;
                }
            }

            double pctAsistenciaReal = totalClasesRegistradas > 0 ?
                Math.Round(((double)asistenciasReales / totalClasesRegistradas) * 100, 1) : 100.0;

            string semaforoReal = pctAsistenciaReal >= 90 ? "Verde" : (pctAsistenciaReal >= 80 ? "Amarillo" : "Rojo");

            viewModel.MateriasActivas = new List<UnidadDashboardViewModel>
            {
                new UnidadDashboardViewModel
                {
                    IdMateria = 1,
                    NombreMateria = "Administración de Proyectos de TI",
                    UnidadActual = "Unidad II: Metodologías Ágiles",
                    PorcentajeAsistencia = pctAsistenciaReal,
                    Semaforo = semaforoReal,
                    FaltasAcumuladas = faltasReales,
                    FaltasPermitidasRestantes = Math.Max(0, 4 - faltasReales),
                    LimiteFaltasTotal = 4,
                    RetardosAcumulados = retardosReales,
                    TotalClasesUnidad = totalClasesRegistradas,
                    ClasesAsistidas = asistenciasReales
                }
            };

            viewModel.AsistenciaGlobal = pctAsistenciaReal;
            viewModel.AlertasRiesgoCount = pctAsistenciaReal < 80 ? 1 : 0;

            return View(viewModel);
        }
    }
}
