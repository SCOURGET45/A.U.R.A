using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Aura.Models.DTOs;
using Aura.Data;
using Aura.Models;
using Microsoft.EntityFrameworkCore;

namespace Aura.Controllers
{
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
        public async Task<IActionResult> ProcesarCargaCSV(IFormFile archivo)
        {
            var resultado = new ResultadoCargaDto();

            if (archivo == null || archivo.Length == 0)
                return BadRequest("No se proporcionó ningún archivo.");

            if (Path.GetExtension(archivo.FileName).ToLower() != ".csv")
                return BadRequest("El formato del archivo debe ser CSV.");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using (var stream = new StreamReader(archivo.OpenReadStream()))
            using (var csv = new CsvReader(stream, config))
            {
                var registros = csv.GetRecords<CargaMasivaRowDto>().ToList();
                int numeroFila = 1;

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    foreach (var fila in registros)
                    {
                        numeroFila++;

                        if (string.IsNullOrWhiteSpace(fila.Matricula) || string.IsNullOrWhiteSpace(fila.NombreGrupo))
                        {
                            resultado.Errores.Add(new LogErrorCarga
                            {
                                Fila = numeroFila,
                                Descripcion = "Datos requeridos incompletos (Matrícula o Grupo)",
                                DatoProblematico = fila.Matricula ?? "Vacio"
                            });
                            continue;
                        }

                        var tutor = await _context.Usuarios.FirstOrDefaultAsync(u => u.CorreoElectronico == fila.CorreoTutor);
                        if (tutor == null)
                        {
                            tutor = new Usuario { NombreCompleto = fila.NombreTutor, CorreoElectronico = fila.CorreoTutor, IdRol = 3 };
                            _context.Usuarios.Add(tutor);
                            await _context.SaveChangesAsync();
                        }

                        var grupo = await _context.Grupos.FirstOrDefaultAsync(g => g.NombreGrupo == fila.NombreGrupo);
                        if (grupo == null)
                        {
                            grupo = new Grupo { NombreGrupo = fila.NombreGrupo, IdTutor = tutor.IdUsuario, IdCuatrimestre = 1 };
                            _context.Grupos.Add(grupo);
                            await _context.SaveChangesAsync();
                        }

                        var alumnoExistente = await _context.Estudiantes.FirstOrDefaultAsync(e => e.Matricula == fila.Matricula);
                        if (alumnoExistente != null)
                        {
                            resultado.Errores.Add(new LogErrorCarga
                            {
                                Fila = numeroFila,
                                Descripcion = "La matrícula ya se encuentra registrada en el sistema.",
                                DatoProblematico = fila.Matricula
                            });
                            continue;
                        }

                        var usuarioAlumno = new Usuario { NombreCompleto = fila.NombreAlumno, CorreoElectronico = fila.CorreoAlumno, IdRol = 1 };
                        _context.Usuarios.Add(usuarioAlumno);
                        await _context.SaveChangesAsync();

                        var nuevoEstudiante = new Estudiante
                        {
                            IdUsuario = usuarioAlumno.IdUsuario,
                            Matricula = fila.Matricula,
                            IdGrupo = grupo.IdGrupo
                        };
                        _context.Estudiantes.Add(nuevoEstudiante);

                        resultado.RegistrosExitosos++;
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (System.Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Error crítico procesando el archivo: {ex.Message}");
                }
            }

            if (resultado.Errores.Any())
            {
                return Ok(new { Mensaje = "Carga completada con advertencias.", Detalle = resultado });
            }

            return Ok(new { Mensaje = "Carga masiva ejecutada con éxito total.", RegistrosInsertados = resultado.RegistrosExitosos });
        }
    }
}
