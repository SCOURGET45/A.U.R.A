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
    [Authorize(Roles = "Tutor")]
    [Route("Tutor")]
    public class TutorController : Controller
    {
        private readonly AuraDbContext _context;

        public TutorController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("MisTutorados")]
        public async Task<IActionResult> MisTutorados()
        {
            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int idTutor = 0;
            if (!string.IsNullOrEmpty(idUsuarioStr))
            {
                int.TryParse(idUsuarioStr, out idTutor);
            }

            var estudiantesQuery = _context.Estudiantes
                .Include(e => e.Grupo)
                .AsQueryable();

            if (idTutor > 0)
            {
                estudiantesQuery = estudiantesQuery.Where(e => e.Grupo.IdTutor == idTutor);
            }

            var estudiantes = await estudiantesQuery.ToListAsync();
            var tutoradosList = new List<MisTutoradosViewModel>();

            foreach (var e in estudiantes)
            {
                var justificantes = await _context.Justificantes
                    .Where(j => j.IdEstudiante == e.IdEstudiante)
                    .ToListAsync();

                var solicitudVulnerabilidad = await _context.SolicitudesVulnerabilidad
                    .Where(s => s.IdEstudiante == e.IdEstudiante)
                    .OrderByDescending(s => s.FechaCreacion)
                    .FirstOrDefaultAsync();

                int justificantesUsados = justificantes.Count;
                int diasAmparados = justificantes.Sum(j => j.DiasAmparados);

                bool enProceso = solicitudVulnerabilidad != null && solicitudVulnerabilidad.Estado == "Pendiente";
                DateTime? fechaJunta = solicitudVulnerabilidad?.FechaJuntaComision;
                string dictamen = solicitudVulnerabilidad?.Dictamen ?? solicitudVulnerabilidad?.Estado ?? string.Empty;

                tutoradosList.Add(new MisTutoradosViewModel
                {
                    IdEstudiante = e.IdEstudiante,
                    Matricula = e.Matricula,
                    NombreCompleto = $"{e.Nombre} {e.Apellidos}",
                    AsistenciaGlobal = 87.5,
                    NivelRiesgo = "Bajo",
                    TieneSolicitudEnProceso = enProceso,
                    JustificantesUsados = justificantesUsados,
                    DiasAmparadosTotales = diasAmparados,
                    FechaJuntaComision = fechaJunta,
                    DictamenFinal = dictamen
                });
            }

            // Si no hay tutorados en la base de datos para este usuario, cargamos un grupo representativo de demostración
            if (!tutoradosList.Any())
            {
                tutoradosList = new List<MisTutoradosViewModel>
                {
                    new MisTutoradosViewModel
                    {
                        IdEstudiante = 1,
                        Matricula = "23301133",
                        NombreCompleto = "Alan Santiago Molina",
                        AsistenciaGlobal = 86.7,
                        NivelRiesgo = "Bajo",
                        TieneSolicitudEnProceso = true,
                        JustificantesUsados = 1,
                        DiasAmparadosTotales = 3,
                        FechaJuntaComision = DateTime.Now.AddDays(2).AddHours(3),
                        DictamenFinal = "Pendiente"
                    },
                    new MisTutoradosViewModel
                    {
                        IdEstudiante = 2,
                        Matricula = "23301145",
                        NombreCompleto = "María Fernanda Gómez",
                        AsistenciaGlobal = 78.0,
                        NivelRiesgo = "Alto",
                        TieneSolicitudEnProceso = false,
                        JustificantesUsados = 2,
                        DiasAmparadosTotales = 10,
                        FechaJuntaComision = null,
                        DictamenFinal = string.Empty
                    },
                    new MisTutoradosViewModel
                    {
                        IdEstudiante = 3,
                        Matricula = "23301199",
                        NombreCompleto = "Carlos Eduardo Pérez",
                        AsistenciaGlobal = 94.2,
                        NivelRiesgo = "Bajo",
                        TieneSolicitudEnProceso = false,
                        JustificantesUsados = 0,
                        DiasAmparadosTotales = 0,
                        FechaJuntaComision = null,
                        DictamenFinal = "Aprobado"
                    }
                };
            }

            return View(tutoradosList);
        }

        [HttpGet("EmitirJustificante/{idEstudiante}")]
        public async Task<IActionResult> EmitirJustificante(int idEstudiante)
        {
            var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
            string nombreAlumno = estudiante != null ? $"{estudiante.Nombre} {estudiante.Apellidos}" : "Alumno Demostración";
            string matricula = estudiante != null ? estudiante.Matricula : "23301133";

            var justificantesActuales = await _context.Justificantes
                .Where(j => j.IdEstudiante == idEstudiante)
                .ToListAsync();

            int cantidadUsada = justificantesActuales.Count;
            int diasAmparadosActuales = justificantesActuales.Sum(j => j.DiasAmparados);

            // Regla dura 1: Bloqueo por límite institucional de 2 justificantes
            if (cantidadUsada >= 2)
            {
                TempData["Error"] = $"Regla de Negocio Bloqueada: El alumno {nombreAlumno} ya alcanzó el límite máximo de 2 justificantes por cuatrimestre.";
                return RedirectToAction(nameof(MisTutorados));
            }

            var model = new EmitirJustificanteViewModel
            {
                IdEstudiante = idEstudiante,
                NombreEstudiante = nombreAlumno,
                MatriculaEstudiante = matricula,
                JustificantesPreviosCount = cantidadUsada,
                DiasAmparadosPreviosCount = diasAmparadosActuales,
                DiasAmparados = 1
            };

            return View(model);
        }

        [HttpPost("EmitirJustificante/{idEstudiante}")]
        public async Task<IActionResult> EmitirJustificante(EmitirJustificanteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var justificantesActuales = await _context.Justificantes
                .Where(j => j.IdEstudiante == model.IdEstudiante)
                .ToListAsync();

            // Regla dura 1: Límite de 2 justificantes por cuatrimestre
            if (justificantesActuales.Count >= 2)
            {
                ModelState.AddModelError(string.Empty, "Regla Dura Violada: El sistema impide emitir más de 2 justificantes por cuatrimestre por alumno.");
                return View(model);
            }

            // Regla dura 2: Límite acumulado de 15 días por enfermedad
            int diasAcumulados = justificantesActuales.Sum(j => j.DiasAmparados);
            if (diasAcumulados + model.DiasAmparados > 15)
            {
                ModelState.AddModelError(string.Empty, $"Regla Dura Violada: El acumulado excedería el límite de 15 días por enfermedad. Días actuales: {diasAcumulados}, intentando agregar: {model.DiasAmparados}.");
                return View(model);
            }

            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int idTutorEmisor = 1;
            if (!string.IsNullOrEmpty(idUsuarioStr))
            {
                int.TryParse(idUsuarioStr, out idTutorEmisor);
            }

            var nuevoJustificante = new Justificantes
            {
                IdEstudiante = model.IdEstudiante,
                IdTutorEmisor = idTutorEmisor,
                DiasAmparados = model.DiasAmparados,
                Motivo = model.Motivo,
                FechaEmision = DateTime.Now
            };

            _context.Justificantes.Add(nuevoJustificante);
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"Justificante emitido exitosamente para {model.NombreEstudiante} por {model.DiasAmparados} día(s). Se han sincronizado las vistas docentes automáticamente.";
            return RedirectToAction(nameof(MisTutorados));
        }

        [HttpGet("SolicitarVulnerabilidad/{idEstudiante}")]
        public async Task<IActionResult> SolicitarVulnerabilidad(int idEstudiante)
        {
            var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
            string nombreEstudiante = estudiante != null ? $"{estudiante.Nombre} {estudiante.Apellidos}" : "Alan Santiago Molina";

            var model = new CrearSolicitudViewModel
            {
                IdEstudiante = idEstudiante,
                NombreEstudiante = nombreEstudiante
            };

            return View(model);
        }

        [HttpPost("SolicitarVulnerabilidad/{idEstudiante}")]
        public async Task<IActionResult> SolicitarVulnerabilidad(CrearSolicitudViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var idUsuarioStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int idTutor = 1;
            if (!string.IsNullOrEmpty(idUsuarioStr))
            {
                int.TryParse(idUsuarioStr, out idTutor);
            }

            var nuevaSolicitud = new SolicitudVulnerabilidad
            {
                IdEstudiante = model.IdEstudiante,
                IdTutor = idTutor,
                CategoriaMotivo = model.CategoriaMotivo,
                JustificacionTutor = model.JustificacionTutor,
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };

            _context.SolicitudesVulnerabilidad.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"El trámite de vulnerabilidad para {model.NombreEstudiante} fue enviado al Director para su dictamen.";
            return RedirectToAction(nameof(MisTutorados));
        }
    }
}
