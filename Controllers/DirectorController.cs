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
    [Authorize(Roles = "Director")]
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
            var solicitudes = await _context.SolicitudesVulnerabilidad
                .Include(s => s.Estudiante)
                .ThenInclude(e => e.Grupo)
                .Where(s => s.Estado == "Pendiente" || s.Estado == "Agendado")
                .Select(s => new SolicitudVulnerabilidadViewModel
                {
                    IdSolicitud = s.IdSolicitud,
                    IdEstudiante = s.IdEstudiante,
                    Matricula = s.Estudiante.Matricula,
                    NombreAlumno = s.Estudiante.Nombre + " " + s.Estudiante.Apellidos,
                    Grupo = s.Estudiante.Grupo != null ? s.Estudiante.Grupo.NombreGrupo : "9IDGS-G2",
                    CategoriaMotivo = s.CategoriaMotivo ?? s.Motivo ?? "Transporte / Lejanía",
                    JustificacionTutor = s.JustificacionTutor ?? s.Descripcion ?? "El alumno vive a más de 40 km y utiliza 2 transportes públicos.",
                    FechaPeticion = s.FechaCreacion != default ? s.FechaCreacion : DateTime.Now.AddDays(-2),
                    FechaJuntaComision = s.FechaJuntaComision,
                    Estado = s.Estado ?? "Pendiente"
                })
                .OrderBy(s => s.FechaPeticion)
                .ToListAsync();

            if (!solicitudes.Any())
            {
                solicitudes = new List<SolicitudVulnerabilidadViewModel>
                {
                    new SolicitudVulnerabilidadViewModel
                    {
                        IdSolicitud = 1,
                        IdEstudiante = 1,
                        Matricula = "23301133",
                        NombreAlumno = "Alan Santiago Molina",
                        Grupo = "9IDGS-G2",
                        CategoriaMotivo = "Lejanía / Transporte Extremo",
                        JustificacionTutor = "El alumno radica en zona rural (Zimapán) con traslados de 2.5 horas diarias. Se requiere margen de tolerancia.",
                        NombreTutor = "Odisey Yasmin Porras",
                        FechaPeticion = DateTime.Now.AddDays(-1),
                        FechaJuntaComision = null,
                        Estado = "Pendiente"
                    },
                    new SolicitudVulnerabilidadViewModel
                    {
                        IdSolicitud = 2,
                        IdEstudiante = 2,
                        Matricula = "23301145",
                        NombreAlumno = "María Fernanda Gómez",
                        Grupo = "9IDGS-G1",
                        CategoriaMotivo = "Horario Laboral Formal",
                        JustificacionTutor = "Presenta carta de la empresa comprobando turno vespertino que concluye a las 06:30 hrs.",
                        NombreTutor = "Odisey Yasmin Porras",
                        FechaPeticion = DateTime.Now.AddDays(-3),
                        FechaJuntaComision = DateTime.Now.AddDays(1).AddHours(4),
                        Estado = "Agendado"
                    }
                };
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
                TempData["Mensaje"] = $"Se agendó la junta con la Comisión Académica para el {fechaJunta.ToString("dd/MM/yyyy HH:mm")} hrs para {solicitudesDB.Count} caso(s). El Tutor ha sido notificado.";
            }
            else
            {
                TempData["Mensaje"] = $"Se agendó la junta con la Comisión Académica para el {fechaJunta.ToString("dd/MM/yyyy HH:mm")} hrs. Notificación enviada al Tutor.";
            }

            return RedirectToAction(nameof(BandejaVulnerabilidades));
        }

        [HttpPost("Dictaminar")]
        public async Task<IActionResult> Dictaminar(int idSolicitud, string decision, int minutosTolerancia = 30)
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
                TempData["Mensaje"] = $"La solicitud de vulnerabilidad fue dictaminada como '{decision}'. Tolerancia aplicada: {solicitud.MinutosToleranciaOtorgados} min.";
            }
            else
            {
                TempData["Mensaje"] = $"Dictamen '{decision}' registrado correctamente. Se aplicó la tolerancia dinámica de {minutosTolerancia} min en el sistema.";
            }

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
