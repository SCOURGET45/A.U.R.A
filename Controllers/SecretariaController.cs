using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
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
        public IActionResult Dashboard()
        {
            return View();
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
                        var estudiante = new Estudiante
                        {
                            Matricula = valores[0].Trim(),
                            Nombre = valores[1].Trim(),
                            Apellidos = valores[2].Trim(),
                            IdGrupo = int.Parse(valores[3].Trim())
                        };

                        nuevosEstudiantes.Add(estudiante);
                    }
                }
            }

            if (nuevosEstudiantes.Count > 0)
            {
                await _context.Estudiantes.AddRangeAsync(nuevosEstudiantes);
                await _context.SaveChangesAsync();
                TempData["Exito"] = $"Se han registrado {nuevosEstudiantes.Count} alumnos exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se encontraron registros válidos en el archivo.";
            }

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
