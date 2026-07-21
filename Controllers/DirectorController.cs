using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
                .Where(s => s.Estado == "Pendiente")
                .Select(s => new SolicitudVulnerabilidadViewModel
                {
                    IdSolicitud = s.IdSolicitud,
                    Matricula = s.Estudiante.Matricula,
                    NombreAlumno = s.Estudiante.Nombre + " " + s.Estudiante.Apellidos,
                    Grupo = s.Estudiante.Grupo.NombreGrupo,
                    CategoriaMotivo = s.CategoriaMotivo,
                    JustificacionTutor = s.JustificacionTutor,
                    FechaPeticion = s.FechaCreacion
                })
                .OrderBy(s => s.FechaPeticion)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpPost("Dictaminar")]
        public async Task<IActionResult> Dictaminar(int idSolicitud, string decision)
        {
            var solicitud = await _context.SolicitudesVulnerabilidad
                .Include(s => s.Estudiante)
                .FirstOrDefaultAsync(s => s.IdSolicitud == idSolicitud);

            if (solicitud != null)
            {
                solicitud.Estado = decision;
                solicitud.FechaResolucion = DateTime.Now;

                if (decision == "Aprobado")
                {
                    solicitud.Estudiante.TieneToleranciaActiva = true;
                }

                await _context.SaveChangesAsync();
                TempData["Mensaje"] = $"La solicitud del alumno {solicitud.Estudiante.Matricula} fue {decision.ToLower()} exitosamente.";
            }

            return RedirectToAction(nameof(BandejaVulnerabilidades));
        }
    }
}
