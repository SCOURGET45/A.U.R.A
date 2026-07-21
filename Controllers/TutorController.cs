using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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
            if (string.IsNullOrEmpty(idUsuarioStr))
            {
                return RedirectToAction("Login", "Auth");
            }

            int idTutor = int.Parse(idUsuarioStr);

            var tutorados = await _context.Estudiantes
                .Where(e => e.Grupo.IdTutor == idTutor)
                .Select(e => new MisTutoradosViewModel
                {
                    IdEstudiante = e.IdEstudiante,
                    Matricula = e.Matricula,
                    NombreCompleto = e.Nombre + " " + e.Apellidos,
                    AsistenciaGlobal = 85.5,
                    NivelRiesgo = "Medio",
                    TieneSolicitudEnProceso = _context.SolicitudesVulnerabilidad.Any(s => s.IdEstudiante == e.IdEstudiante && s.Estado == "Pendiente")
                })
                .ToListAsync();

            return View(tutorados);
        }

        [HttpGet("SolicitarVulnerabilidad/{idEstudiante}")]
        public async Task<IActionResult> SolicitarVulnerabilidad(int idEstudiante)
        {
            var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
            if (estudiante == null)
            {
                return NotFound();
            }

            var model = new CrearSolicitudViewModel
            {
                IdEstudiante = estudiante.IdEstudiante,
                NombreEstudiante = estudiante.Nombre + " " + estudiante.Apellidos
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
                IdEstudiante = model.IdEstudiante,
                CategoriaMotivo = model.CategoriaMotivo,
                JustificacionTutor = model.JustificacionTutor,
                Estado = "Pendiente",
                FechaCreacion = DateTime.Now
            };

            _context.SolicitudesVulnerabilidad.Add(nuevaSolicitud);
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"El caso de {model.NombreEstudiante} fue enviado al Director para su dictamen.";
            return RedirectToAction(nameof(MisTutorados));
        }
    }
}
