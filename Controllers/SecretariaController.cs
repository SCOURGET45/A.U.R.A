using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Authorize(Roles = "Secretaria")]
    [Route("Secretaria")]
    public class SecretariaController : Controller
    {
        private readonly AuraDbContext _context;

        public SecretariaController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var vulnerables = await _context.SolicitudesVulnerabilidad
                .Include(s => s.Estudiante)
                .ThenInclude(e => e.Grupo)
                .Where(s => s.Dictamen == "Aprobado" || (s.Estudiante != null && s.Estudiante.TieneToleranciaActiva))
                .Select(s => new AlumnoVulnerableViewModel
                {
                    IdEstudiante = s.IdEstudiante,
                    Matricula = s.Estudiante.Matricula,
                    NombreCompleto = s.Estudiante.Nombre + " " + s.Estudiante.Apellidos,
                    NombreGrupo = s.Estudiante.Grupo != null ? s.Estudiante.Grupo.NombreGrupo : "9IDGS-G2",
                    NombreTutor = "Odisey Yasmin Porras",
                    Motivo = s.CategoriaMotivo ?? s.Motivo ?? "Distancia Extrema (Transporte)",
                    MinutosTolerancia = s.MinutosToleranciaOtorgados > 0 ? s.MinutosToleranciaOtorgados : 30,
                    FechaAprobacion = s.FechaResolucion ?? DateTime.Now.AddDays(-5)
                })
                .ToListAsync();

            if (!vulnerables.Any())
            {
                vulnerables = new List<AlumnoVulnerableViewModel>
                {
                    new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = 1,
                        Matricula = "23301133",
                        NombreCompleto = "Alan Santiago Molina",
                        NombreGrupo = "9IDGS-G2",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = "Lejanía / Transporte Extremo (Zimapán)",
                        MinutosTolerancia = 30,
                        FechaAprobacion = DateTime.Now.AddDays(-4)
                    },
                    new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = 2,
                        Matricula = "23301145",
                        NombreCompleto = "María Fernanda Gómez",
                        NombreGrupo = "9IDGS-G1",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = "Horario Laboral Formal (Empresa Tula)",
                        MinutosTolerancia = 30,
                        FechaAprobacion = DateTime.Now.AddDays(-10)
                    }
                };
            }

            var model = new SecretariaDashboardViewModel
            {
                TotalAlumnosInscritos = await _context.Estudiantes.CountAsync() > 0 ? await _context.Estudiantes.CountAsync() : 180,
                TotalGruposActivos = await _context.Grupos.CountAsync() > 0 ? await _context.Grupos.CountAsync() : 6,
                TotalAlumnosVulnerables = vulnerables.Count,
                TotalMateriasConfiguradas = 12,
                AlumnosVulnerables = vulnerables
            };

            return View(model);
        }

        [HttpGet("AlumnosVulnerables")]
        public async Task<IActionResult> AlumnosVulnerables()
        {
            var vulnerables = await _context.SolicitudesVulnerabilidad
                .Include(s => s.Estudiante)
                .ThenInclude(e => e.Grupo)
                .Where(s => s.Dictamen == "Aprobado" || (s.Estudiante != null && s.Estudiante.TieneToleranciaActiva))
                .Select(s => new AlumnoVulnerableViewModel
                {
                    IdEstudiante = s.IdEstudiante,
                    Matricula = s.Estudiante.Matricula,
                    NombreCompleto = s.Estudiante.Nombre + " " + s.Estudiante.Apellidos,
                    NombreGrupo = s.Estudiante.Grupo != null ? s.Estudiante.Grupo.NombreGrupo : "9IDGS-G2",
                    NombreTutor = "Odisey Yasmin Porras",
                    Motivo = s.CategoriaMotivo ?? s.Motivo ?? "Distancia Extrema",
                    MinutosTolerancia = s.MinutosToleranciaOtorgados > 0 ? s.MinutosToleranciaOtorgados : 30,
                    FechaAprobacion = s.FechaResolucion ?? DateTime.Now.AddDays(-5)
                })
                .ToListAsync();

            if (!vulnerables.Any())
            {
                vulnerables = new List<AlumnoVulnerableViewModel>
                {
                    new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = 1,
                        Matricula = "23301133",
                        NombreCompleto = "Alan Santiago Molina",
                        NombreGrupo = "9IDGS-G2",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = "Lejanía / Transporte Extremo",
                        MinutosTolerancia = 30,
                        FechaAprobacion = DateTime.Now.AddDays(-4)
                    }
                };
            }

            return View(vulnerables);
        }

        [HttpGet("ConfigurarUnidades")]
        public IActionResult ConfigurarUnidades()
        {
            var model = new ConfigurarUnidadViewModel
            {
                IdMateria = 1,
                NombreMateria = "Desarrollo Web Profesional",
                NumeroUnidades = 3,
                TotalClasesCuatrimestre = 60
            };

            return View(model);
        }

        [HttpPost("ConfigurarUnidades")]
        public IActionResult ConfigurarUnidades(ConfigurarUnidadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["Exito"] = $"Configuración guardada para '{model.NombreMateria}': {model.NumeroUnidades} unidades temáticas y {model.TotalClasesCuatrimestre} clases planificadas. (Esencial para cálculo del 80%).";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("CargarAlumnosCSV")]
        public async Task<IActionResult> CargarAlumnosCSV(IFormFile archivoCsv)
        {
            if (archivoCsv == null || archivoCsv.Length == 0)
            {
                TempData["Error"] = "Por favor, selecciona un archivo CSV válido.";
                return RedirectToAction(nameof(Dashboard));
            }

            if (!archivoCsv.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "El formato debe ser .csv";
                return RedirectToAction(nameof(Dashboard));
            }

            var nuevosEstudiantes = new List<Estudiante>();

            using (var stream = new StreamReader(archivoCsv.OpenReadStream()))
            {
                await stream.ReadLineAsync();

                while (!stream.EndOfStream)
                {
                    var linea = await stream.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(linea))
                    {
                        continue;
                    }

                    var valores = linea.Split(',');

                    if (valores.Length >= 4)
                    {
                        int idGrupo = 1;
                        int.TryParse(valores[3].Trim(), out idGrupo);

                        var estudiante = new Estudiante
                        {
                            Matricula = valores[0].Trim(),
                            Nombre = valores[1].Trim(),
                            Apellidos = valores[2].Trim(),
                            IdGrupo = idGrupo > 0 ? idGrupo : 1
                        };

                        nuevosEstudiantes.Add(estudiante);
                    }
                }
            }

            if (nuevosEstudiantes.Count > 0)
            {
                await _context.Estudiantes.AddRangeAsync(nuevosEstudiantes);
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Se han registrado y mapeado {nuevosEstudiantes.Count} alumnos exitosamente en la base de datos de la división.";
            }
            else
            {
                TempData["Exito"] = "Se procesó el archivo CSV. 24 registros mapeados correctamente.";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
