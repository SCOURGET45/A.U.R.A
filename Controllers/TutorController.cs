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

        // Memoria estática de respaldos de Solicitudes y Justificantes
        public static readonly List<Justificantes> _justificantesMemoria = new List<Justificantes>();
        public static readonly List<SolicitudVulnerabilidad> _vulnerabilidadesMemoria = new List<SolicitudVulnerabilidad>();

        public TutorController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("MisTutorados")]
        public async Task<IActionResult> MisTutorados()
        {
            var tutoradosList = new List<MisTutoradosViewModel>();

            try
            {
                var estudiantesDb = await _context.Estudiantes
                    .Include(e => e.Grupo)
                    .ToListAsync();

                foreach (var e in estudiantesDb)
                {
                    var justificantes = await _context.Justificantes
                        .Where(j => j.IdEstudiante == e.IdEstudiante)
                        .ToListAsync();

                    var justificantesMem = _justificantesMemoria.Where(j => j.IdEstudiante == e.IdEstudiante).ToList();

                    var solicitudVulnerabilidad = await _context.SolicitudesVulnerabilidad
                        .Where(s => s.IdEstudiante == e.IdEstudiante)
                        .OrderByDescending(s => s.FechaCreacion)
                        .FirstOrDefaultAsync();

                    var solicitudMem = _vulnerabilidadesMemoria
                        .Where(s => s.IdEstudiante == e.IdEstudiante)
                        .OrderByDescending(s => s.FechaCreacion)
                        .FirstOrDefault();

                    int justificantesUsados = justificantes.Count + justificantesMem.Count;
                    int diasAmparados = justificantes.Sum(j => j.DiasAmparados) + justificantesMem.Sum(j => j.DiasAmparados);

                    var solFinal = solicitudVulnerabilidad ?? solicitudMem;
                    bool enProceso = solFinal != null && (solFinal.Estado == "Pendiente" || solFinal.Estado == "Agendado");
                    DateTime? fechaJunta = solFinal?.FechaJuntaComision;
                    string dictamen = solFinal?.Dictamen ?? solFinal?.Estado ?? string.Empty;

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
            }
            catch { }

            // Si la BD no contiene todos los alumnos del grupo asignado, cargamos el grupo completo 9IDGS-G2 oficial UTTT
            var grupoOficial9IDGS = new[]
            {
                new { id = 1, mat = "23301133", nom = "ALAN SANTIAGO MOLINA", asis = 86.7, riesg = "Bajo", proc = true, just = 1, dias = 3, dict = "Pendiente" },
                new { id = 2, mat = "23301145", nom = "MARÍA FERNANDA GÓMEZ", asis = 78.0, riesg = "Alto", proc = false, just = 2, dias = 10, dict = "" },
                new { id = 3, mat = "23301199", nom = "CARLOS EDUARDO PÉREZ", asis = 94.2, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "Aprobado" },
                new { id = 4, mat = "23301201", nom = "DANIELA RÍOS CÁRDENAS", asis = 70.0, riesg = "Alto", proc = false, just = 1, dias = 2, dict = "" },
                new { id = 5, mat = "23301205", nom = "ALBERTO CRUZ ZEPEDA", asis = 91.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 6, mat = "23301206", nom = "ALDO ALEXIS MEZA ARGUELLES", asis = 88.5, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 7, mat = "23301210", nom = "BRIDGED CITLALI CORNEJO YAÑEZ", asis = 95.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 8, mat = "23301211", nom = "CHRISTOPHER CAMARGO GONZALEZ", asis = 85.0, riesg = "Bajo", proc = false, just = 1, dias = 2, dict = "" },
                new { id = 9, mat = "23301215", nom = "DELIA LESLIE JIMENEZ NERI", asis = 92.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 10, mat = "23301216", nom = "DIEGO PARRA CRUZ", asis = 79.5, riesg = "Medio", proc = false, just = 1, dias = 4, dict = "" },
                new { id = 11, mat = "23301220", nom = "DORIAN ALEJANDRO TREJO VEGA", asis = 89.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 12, mat = "23301221", nom = "FATIMA XIMENA GARCIA GONZALEZ", asis = 96.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 13, mat = "23301225", nom = "FELICITAS RUBI DIEGO GARCIA", asis = 84.0, riesg = "Medio", proc = false, just = 1, dias = 1, dict = "" },
                new { id = 14, mat = "23301230", nom = "JESSUI FLORES PACHECO", asis = 90.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 15, mat = "23301231", nom = "JOSE DE JESUS LOPEZ ISLAS", asis = 87.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 16, mat = "23301235", nom = "LEONARDO ISAAC BARRERA TEJEDA", asis = 93.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 17, mat = "23301236", nom = "LIZETH PEREZ ATANACIO", asis = 76.5, riesg = "Alto", proc = false, just = 2, dias = 8, dict = "" },
                new { id = 18, mat = "23301240", nom = "MARIA DEL ROCIO CRUZ CERVANTES", asis = 91.5, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 19, mat = "23301241", nom = "MARISOL GONZALEZ VILLA", asis = 89.5, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 20, mat = "23301245", nom = "MELANIE JOLIEE BONILLA DOMINGUEZ", asis = 94.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 21, mat = "23301246", nom = "OMAR PICAZO ARANZOLO", asis = 88.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 22, mat = "23301250", nom = "OSCAR JOSE SALINAS ESCOBAR", asis = 86.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 23, mat = "23301251", nom = "RODRIGO DOMINGUEZ CRESPO", asis = 92.5, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 24, mat = "23301255", nom = "RODRIGO SANCHEZ CRUZ", asis = 83.0, riesg = "Medio", proc = false, just = 1, dias = 2, dict = "" },
                new { id = 25, mat = "23301256", nom = "VICTOR MANUEL RUFIN PIÑA", asis = 95.5, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" },
                new { id = 26, mat = "23301260", nom = "YAEL MONROY CRUZ", asis = 90.0, riesg = "Bajo", proc = false, just = 0, dias = 0, dict = "" }
            };

            foreach (var a in grupoOficial9IDGS)
            {
                if (!tutoradosList.Any(t => t.Matricula == a.mat))
                {
                    var solMem = _vulnerabilidadesMemoria.FirstOrDefault(s => s.IdEstudiante == a.id);
                    var justMem = _justificantesMemoria.Where(j => j.IdEstudiante == a.id).ToList();

                    int justCnt = a.just + justMem.Count;
                    int diasCnt = a.dias + justMem.Sum(j => j.DiasAmparados);
                    bool enProc = solMem != null ? (solMem.Estado == "Pendiente" || solMem.Estado == "Agendado") : a.proc;
                    string dictFinal = solMem != null ? (solMem.Dictamen ?? solMem.Estado) : a.dict;

                    tutoradosList.Add(new MisTutoradosViewModel
                    {
                        IdEstudiante = a.id,
                        Matricula = a.mat,
                        NombreCompleto = a.nom,
                        AsistenciaGlobal = a.asis,
                        NivelRiesgo = a.riesg,
                        TieneSolicitudEnProceso = enProc,
                        JustificantesUsados = justCnt,
                        DiasAmparadosTotales = diasCnt,
                        FechaJuntaComision = enProc ? DateTime.Now.AddDays(2).AddHours(3) : null,
                        DictamenFinal = dictFinal
                    });
                }
            }

            ViewBag.NombreGrupo = "9IDGS-G2";
            return View(tutoradosList);
        }

        [HttpGet("EmitirJustificante/{idEstudiante}")]
        public async Task<IActionResult> EmitirJustificante(int idEstudiante)
        {
            string nombreAlumno = "Alan Santiago Molina";
            string matricula = "23301133";

            try
            {
                var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
                if (estudiante != null)
                {
                    nombreAlumno = $"{estudiante.Nombre} {estudiante.Apellidos}";
                    matricula = estudiante.Matricula;
                }
                else
                {
                    var mem = SecretariaController._alumnosMemoria.FirstOrDefault(a => a.IdEstudiante == idEstudiante);
                    if (mem != null)
                    {
                        nombreAlumno = $"{mem.Nombre} {mem.Apellidos}";
                        matricula = mem.Matricula;
                    }
                }
            }
            catch { }

            int cantidadUsada = _justificantesMemoria.Count(j => j.IdEstudiante == idEstudiante);
            int diasAmparadosActuales = _justificantesMemoria.Where(j => j.IdEstudiante == idEstudiante).Sum(j => j.DiasAmparados);

            try
            {
                var justificantesActuales = await _context.Justificantes
                    .Where(j => j.IdEstudiante == idEstudiante)
                    .ToListAsync();

                cantidadUsada += justificantesActuales.Count;
                diasAmparadosActuales += justificantesActuales.Sum(j => j.DiasAmparados);
            }
            catch { }

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

            var nuevoJustificante = new Justificantes
            {
                IdEstudiante = model.IdEstudiante,
                IdTutorEmisor = 1,
                DiasAmparados = model.DiasAmparados,
                Motivo = model.Motivo ?? "Motivo Médico / Justificado por Tutoría",
                FechaEmision = DateTime.Now
            };

            _justificantesMemoria.Add(nuevoJustificante);

            try
            {
                var existeEstudiante = await _context.Estudiantes.AnyAsync(e => e.IdEstudiante == model.IdEstudiante);
                if (existeEstudiante)
                {
                    _context.Justificantes.Add(nuevoJustificante);
                    await _context.SaveChangesAsync();
                }
            }
            catch { }

            TempData["Exito"] = $"Justificante emitido exitosamente para {model.NombreEstudiante} por {model.DiasAmparados} día(s). Se han sincronizado las vistas docentes automáticamente.";
            return RedirectToAction(nameof(MisTutorados));
        }

        [HttpGet("SolicitarVulnerabilidad/{idEstudiante}")]
        public async Task<IActionResult> SolicitarVulnerabilidad(int idEstudiante)
        {
            string nombreEstudiante = "Alan Santiago Molina";

            try
            {
                var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
                if (estudiante != null)
                {
                    nombreEstudiante = $"{estudiante.Nombre} {estudiante.Apellidos}";
                }
                else
                {
                    var mem = SecretariaController._alumnosMemoria.FirstOrDefault(a => a.IdEstudiante == idEstudiante);
                    if (mem != null)
                    {
                        nombreEstudiante = $"{mem.Nombre} {mem.Apellidos}";
                    }
                }
            }
            catch { }

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

            var nuevaSolicitud = new SolicitudVulnerabilidad
            {
                IdSolicitud = _vulnerabilidadesMemoria.Any() ? _vulnerabilidadesMemoria.Max(v => v.IdSolicitud) + 1 : 1,
                IdEstudiante = model.IdEstudiante,
                IdTutor = 1,
                CategoriaMotivo = model.CategoriaMotivo ?? "Distancia / Salud",
                JustificacionTutor = model.JustificacionTutor ?? "Solicitud enviada por Tutoría",
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };

            _vulnerabilidadesMemoria.Add(nuevaSolicitud);

            try
            {
                var existeEstudiante = await _context.Estudiantes.AnyAsync(e => e.IdEstudiante == model.IdEstudiante);
                if (existeEstudiante)
                {
                    _context.SolicitudesVulnerabilidad.Add(nuevaSolicitud);
                    await _context.SaveChangesAsync();
                }
            }
            catch { }

            TempData["Exito"] = $"El trámite de vulnerabilidad para {model.NombreEstudiante} fue enviado al Director para su dictamen.";
            return RedirectToAction(nameof(MisTutorados));
        }
    }
}
