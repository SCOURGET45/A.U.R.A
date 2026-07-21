using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using Aura.Data;
using Aura.Models;
using System.Linq;

namespace Aura.Controllers
{
    [Authorize(Roles = "Secretaria")]
    [Route("api/[controller]")]
    [ApiController]
    public class SecretariaController : ControllerBase
    {
        private readonly AuraDbContext _context;

        public SecretariaController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpPost("CargaMasiva")]
        public async Task<IActionResult> SubirPlantillaCSV(IFormFile archivoCsv)
        {
            if (archivoCsv == null || archivoCsv.Length == 0)
                return BadRequest("Por favor, selecciona un archivo CSV válido.");

            if (!archivoCsv.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("El formato del archivo debe ser .csv");

            var logErrores = new List<string>();
            int filasProcesadas = 0;

            using (var stream = new StreamReader(archivoCsv.OpenReadStream()))
            {
                string encabezado = await stream.ReadLineAsync();

                while (!stream.EndOfStream)
                {
                    var linea = await stream.ReadLineAsync();
                    var valores = linea.Split(',');

                    if (valores.Length < 4)
                    {
                        logErrores.Add($"Fila incompleta ignorada: {linea}");
                        continue;
                    }

                    try
                    {
                        string matricula = valores[0].Trim();
                        string nombre = valores[1].Trim();
                        string correo = valores[2].Trim();
                        string nombreGrupo = valores[3].Trim();

                        bool existe = _context.Estudiantes.Any(e => e.Matricula == matricula);
                        if (existe)
                        {
                            logErrores.Add($"El alumno con matrícula {matricula} ya está registrado.");
                            continue;
                        }

                        var grupo = _context.Grupos.FirstOrDefault(g => g.NombreGrupo == nombreGrupo);
                        if (grupo == null)
                        {
                            logErrores.Add($"Grupo '{nombreGrupo}' no encontrado para el alumno {matricula}.");
                            continue;
                        }

                        var nuevoUsuario = new Usuario
                        {
                            NombreCompleto = nombre,
                            CorreoElectronico = correo,
                            ContrasenaHash = matricula,
                            IdRol = 1,
                            Activo = true
                        };

                        _context.Usuarios.Add(nuevoUsuario);
                        await _context.SaveChangesAsync();

                        var nuevoEstudiante = new Estudiante
                        {
                            IdUsuario = nuevoUsuario.IdUsuario,
                            IdGrupo = grupo.IdGrupo,
                            Matricula = matricula
                        };

                        _context.Estudiantes.Add(nuevoEstudiante);
                        filasProcesadas++;
                    }
                    catch (Exception ex)
                    {
                        logErrores.Add($"Error al procesar la fila '{linea}': {ex.Message}");
                    }
                }

                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                Mensaje = $"Carga finalizada. {filasProcesadas} registros insertados con éxito.",
                Errores = logErrores
            });
        }

        [HttpPost("ConfigurarUnidad")]
        public async Task<IActionResult> ConfigurarUnidad([FromBody] UnidadAcademica modelo)
        {
            if (modelo.FechaFin <= modelo.FechaInicio)
            {
                return BadRequest("La fecha de fin debe ser mayor a la fecha de inicio.");
            }

            if (modelo.TotalClasesProgramadas <= 0)
            {
                return BadRequest("El total de clases programadas debe ser mayor a cero para calcular los porcentajes.");
            }

            _context.UnidadesAcademicas.Add(new UnidadAcademica
            {
                IdMateria = modelo.IdMateria,
                NumeroUnidad = modelo.NumeroUnidad,
                FechaInicio = modelo.FechaInicio,
                FechaFin = modelo.FechaFin,
                TotalClasesProgramadas = modelo.TotalClasesProgramadas
            });

            await _context.SaveChangesAsync();

            return Ok($"Unidad {modelo.NumeroUnidad} configurada correctamente. El sistema ahora puede calcular el 80% de margen.");
        }
    }
}
