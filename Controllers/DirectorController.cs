using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Authorize]
    [Route("Director")]
    public class DirectorController : Controller
    {
        private readonly AuraDbContext _context;

        public DirectorController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("BandejaVulnerabilidades")]
        public async Task<IActionResult> BandejaVulnerabilidades()
        {
            var solicitudes = new List<SolicitudVulnerabilidadViewModel>();

            // 1. Cargar desde la base de datos SQL en Render
            try
            {
                var dbList = await _context.SolicitudesVulnerabilidad
                    .Include(s => s.Estudiante)
                    .ThenInclude(e => e.Grupo)
                    .OrderByDescending(s => s.FechaCreacion)
                    .ToListAsync();

                foreach (var s in dbList)
                {
                    solicitudes.Add(new SolicitudVulnerabilidadViewModel
                    {
                        IdSolicitud = s.IdSolicitud,
                        IdEstudiante = s.IdEstudiante,
                        Matricula = s.Estudiante != null ? s.Estudiante.Matricula : "23301133",
                        NombreAlumno = s.Estudiante != null ? $"{s.Estudiante.Nombre} {s.Estudiante.Apellidos}" : "Alumno Registrado",
                        Grupo = (s.Estudiante != null && s.Estudiante.Grupo != null) ? s.Estudiante.Grupo.NombreGrupo : "9IDGS-G2",
                        CategoriaMotivo = s.CategoriaMotivo ?? s.Motivo ?? "Transporte / Lejanía",
                        JustificacionTutor = s.JustificacionTutor ?? s.Descripcion ?? "Solicitud registrada en sistema.",
                        FechaPeticion = s.FechaCreacion != default ? s.FechaCreacion : DateTime.Now,
                        FechaJuntaComision = s.FechaJuntaComision,
                        Estado = s.Estado ?? "Pendiente"
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Advertencia DB BandejaVulnerabilidades: " + ex.Message);
            }

            // 2. Incluir solicitudes creadas por el Tutor desde memoria
            try
            {
                foreach (var vMem in TutorController._vulnerabilidadesMemoria)
                {
                    var existente = solicitudes.FirstOrDefault(s => s.IdSolicitud == vMem.IdSolicitud);
                    if (existente == null)
                    {
                        var alumnoMem = SecretariaController._alumnosMemoria.FirstOrDefault(a => a.IdEstudiante == vMem.IdEstudiante);
                        solicitudes.Add(new SolicitudVulnerabilidadViewModel
                        {
                            IdSolicitud = vMem.IdSolicitud,
                            IdEstudiante = vMem.IdEstudiante,
                            Matricula = alumnoMem?.Matricula ?? "23301133",
                            NombreAlumno = alumnoMem != null ? $"{alumnoMem.Nombre} {alumnoMem.Apellidos}" : "Alumno UTTT",
                            Grupo = alumnoMem?.NombreGrupo ?? "9IDGS-G2",
                            CategoriaMotivo = vMem.CategoriaMotivo ?? "Transporte / Lejanía",
                            JustificacionTutor = vMem.JustificacionTutor ?? "Solicitud enviada por Tutoría",
                            NombreTutor = "Odisey Yasmin Porras",
                            FechaPeticion = vMem.FechaCreacion,
                            FechaJuntaComision = vMem.FechaJuntaComision,
                            Estado = vMem.Estado ?? "Pendiente"
                        });
                    }
                    else
                    {
                        // Sincronizar estado si ya existía
                        existente.Estado = vMem.Estado ?? existente.Estado;
                        existente.FechaJuntaComision = vMem.FechaJuntaComision ?? existente.FechaJuntaComision;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Advertencia Memoria BandejaVulnerabilidades: " + ex.Message);
            }

            return View(solicitudes);
        }

        [HttpPost("AgendarJunta")]
        public async Task<IActionResult> AgendarJunta([FromForm] int[] idsSolicitudes, [FromForm] DateTime fechaJunta)
        {
            if (idsSolicitudes == null || idsSolicitudes.Length == 0)
            {
                TempData["Error"] = "Por favor selecciona al menos una solicitud para agendar junta.";
                return RedirectToAction(nameof(BandejaVulnerabilidades));
            }

            try
            {
                var solicitudesDB = await _context.SolicitudesVulnerabilidad
                    .Where(s => idsSolicitudes.Contains(s.IdSolicitud))
                    .ToListAsync();

                foreach (var sol in solicitudesDB)
                {
                    sol.FechaJuntaComision = fechaJunta;
                    sol.Estado = "Agendado";
                }

                if (solicitudesDB.Any())
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch { }

            foreach (var vMem in TutorController._vulnerabilidadesMemoria)
            {
                if (idsSolicitudes.Contains(vMem.IdSolicitud))
                {
                    vMem.FechaJuntaComision = fechaJunta;
                    vMem.Estado = "Agendado";
                }
            }

            TempData["Mensaje"] = $"Se agendó la junta con la Comisión Académica para el {fechaJunta.ToString("dd/MM/yyyy HH:mm")} hrs. El Tutor ha sido notificado.";
            return RedirectToAction(nameof(BandejaVulnerabilidades));
        }

        [HttpPost("Dictaminar")]
        public async Task<IActionResult> Dictaminar(int idSolicitud, string decision, int minutosTolerancia = 30)
        {
            try
            {
                var solicitud = await _context.SolicitudesVulnerabilidad
                    .Include(s => s.Estudiante)
                    .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

                if (solicitud != null)
                {
                    solicitud.Estado = decision;
                    solicitud.Dictamen = decision;
                    solicitud.FechaResolucion = DateTime.Now;
                    solicitud.MinutosToleranciaOtorgados = decision == "Aprobado" ? (minutosTolerancia > 0 ? minutosTolerancia : 30) : 0;

                    if (decision == "Aprobado" && solicitud.Estudiante != null)
                    {
                        solicitud.Estudiante.TieneToleranciaActiva = true;
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch { }

            // Actualizar memoria estática de Tutoría
            var vMem = TutorController._vulnerabilidadesMemoria.FirstOrDefault(v => v.IdSolicitud == idSolicitud);
            if (vMem != null)
            {
                vMem.Estado = decision;
                vMem.Dictamen = decision;
                vMem.FechaResolucion = DateTime.Now;
                vMem.MinutosToleranciaOtorgados = decision == "Aprobado" ? (minutosTolerancia > 0 ? minutosTolerancia : 30) : 0;
            }

            TempData["Mensaje"] = $"La solicitud #{idSolicitud} fue dictaminada exitosamente como '{decision}' con {minutosTolerancia} minutos de tolerancia otorgados.";
            return RedirectToAction(nameof(BandejaVulnerabilidades));
        }

        [HttpGet("MonitorGeneral")]
        public IActionResult MonitorGeneral()
        {
            var model = new DirectorMonitorViewModel
            {
                TotalEstudiantes = 420,
                AlumnosEnRiesgo = 28,
                CasosVulnerablesActivos = 14,
                PromedioAsistenciaDivisional = 89.4,
                Carreras = new List<EstadisticaCarreraViewModel>
                {
                    new EstadisticaCarreraViewModel
                    {
                        NombreCarrera = "Desarrollo de Software Multiplataforma",
                        TotalGrupos = 6,
                        PromedioAsistencia = 91.2,
                        AlumnosEnRiesgo = 5
                    },
                    new EstadisticaCarreraViewModel
                    {
                        NombreCarrera = "Redes Inteligentes y Ciberseguridad",
                        TotalGrupos = 4,
                        PromedioAsistencia = 87.8,
                        AlumnosEnRiesgo = 12
                    },
                    new EstadisticaCarreraViewModel
                    {
                        NombreCarrera = "Mecatrónica y Robótica",
                        TotalGrupos = 5,
                        PromedioAsistencia = 89.0,
                        AlumnosEnRiesgo = 11
                    }
                }
            };

            return View(model);
        }
    }
}
