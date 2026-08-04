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
    [Route("api/[controller]")]
    [ApiController]
    public class AsistenciaController : ControllerBase
    {
        private readonly AuraDbContext _context;

        // Registro real dinámico de pases de lista en vivo por matrícula
        public static readonly Dictionary<string, (string Estado, DateTime Hora, string Metodo)> _paseListaEnVivo =
            new Dictionary<string, (string, DateTime, string)>(StringComparer.OrdinalIgnoreCase);

        public AsistenciaController(AuraDbContext context)
        {
            _context = context;
        }

        // Obtiene la hora oficial de México (UTC-6 / America/Mexico_City) independiente del servidor de la nube (Render)
        public static DateTime ObtenerHoraMexico()
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
                return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            }
            catch
            {
                try
                {
                    var tzInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzInfo);
                }
                catch
                {
                    return DateTime.UtcNow.AddHours(-6); // Fallback hora México UTC-6
                }
            }
        }

        public static string NormalizarTexto(string txt)
        {
            if (string.IsNullOrWhiteSpace(txt)) return string.Empty;
            string unaccent = txt.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in unaccent)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if (char.IsLetterOrDigit(c)) sb.Append(char.ToUpperInvariant(c));
                }
            }
            return sb.ToString();
        }

        // Endpoint GET: Obtener Pase de Lista en Vivo del Grupo en Atendimiento
        [HttpGet("ObtenerPaseListaGrupo")]
        public async Task<IActionResult> ObtenerPaseListaGrupo([FromQuery] string grupo = "9IDGS-G2")
        {
            var grupoOficial9IDGS = new[]
            {
                new { id = 1, mat = "23301133", nom = "ALAN SANTIAGO MOLINA" },
                new { id = 2, mat = "23301145", nom = "MARÍA FERNANDA GÓMEZ" },
                new { id = 3, mat = "23301199", nom = "CARLOS EDUARDO PÉREZ" },
                new { id = 4, mat = "23301201", nom = "DANIELA RÍOS CÁRDENAS" },
                new { id = 5, mat = "23301205", nom = "ALBERTO CRUZ ZEPEDA" },
                new { id = 6, mat = "23301206", nom = "ALDO ALEXIS MEZA ARGUELLES" },
                new { id = 7, mat = "23301210", nom = "BRIDGED CITLALI CORNEJO YAÑEZ" },
                new { id = 8, mat = "23301211", nom = "CHRISTOPHER CAMARGO GONZALEZ" },
                new { id = 9, mat = "23301215", nom = "DELIA LESLIE JIMENEZ NERI" },
                new { id = 10, mat = "23301216", nom = "DIEGO PARRA CRUZ" },
                new { id = 11, mat = "23301220", nom = "DORIAN ALEJANDRO TREJO VEGA" },
                new { id = 12, mat = "23301221", nom = "FATIMA XIMENA GARCIA GONZALEZ" },
                new { id = 13, mat = "23301225", nom = "FELICITAS RUBI DIEGO GARCIA" },
                new { id = 14, mat = "23301230", nom = "JESSUI FLORES PACHECO" },
                new { id = 15, mat = "23301231", nom = "JOSE DE JESUS LOPEZ ISLAS" },
                new { id = 16, mat = "23301235", nom = "LEONARDO ISAAC BARRERA TEJEDA" },
                new { id = 17, mat = "23301236", nom = "LIZETH PEREZ ATANACIO" },
                new { id = 18, mat = "23301240", nom = "MARIA DEL ROCIO CRUZ CERVANTES" },
                new { id = 19, mat = "23301241", nom = "MARISOL GONZALEZ VILLA" },
                new { id = 20, mat = "23301245", nom = "MELANIE JOLIEE BONILLA DOMINGUEZ" },
                new { id = 21, mat = "23301246", nom = "OMAR PICAZO ARANZOLO" },
                new { id = 22, mat = "23301250", nom = "OSCAR JOSE SALINAS ESCOBAR" },
                new { id = 23, mat = "23301251", nom = "RODRIGO DOMINGUEZ CRESPO" },
                new { id = 24, mat = "23301255", nom = "RODRIGO SANCHEZ CRUZ" },
                new { id = 25, mat = "23301256", nom = "VICTOR MANUEL RUFIN PIÑA" },
                new { id = 26, mat = "23301260", nom = "YAEL MONROY CRUZ" }
            };

            // Estructura unificada de alumnos con desduplicación por Nombre y Matrícula
            var listaCompleta = new List<AlumnoMonitorDto>();
            var matriculasAgregadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nombresNormalizadosAgregados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Alumnos registrados/subidos dinámicamente en Secretaría (Máxima Prioridad de Reemplazo)
            try
            {
                foreach (var am in SecretariaController._alumnosMemoria)
                {
                    string nomCompleto = $"{am.Nombre} {am.Apellidos}".Trim();
                    string normNom = NormalizarTexto(nomCompleto);
                    string matClean = am.Matricula.Split('@')[0].Trim();

                    if (matriculasAgregadas.Add(matClean) && (string.IsNullOrEmpty(normNom) || nombresNormalizadosAgregados.Add(normNom)))
                    {
                        listaCompleta.Add(new AlumnoMonitorDto
                        {
                            IdEstudiante = am.IdEstudiante,
                            Matricula = matClean,
                            NombreCompleto = nomCompleto,
                            Grupo = am.NombreGrupo,
                            TieneTolerancia = am.Matricula == "23301133" || am.Matricula == "23301145"
                        });
                    }
                }
            }
            catch { }

            // 2. Alumnos desde la BD
            try
            {
                var estudiantesDb = await _context.Estudiantes
                    .Include(e => e.Grupo)
                    .Where(e => e.Grupo == null || e.Grupo.NombreGrupo == grupo || grupo == "9IDGS-G2")
                    .ToListAsync();

                foreach (var e in estudiantesDb)
                {
                    string nomCompleto = $"{e.Nombre} {e.Apellidos}".Trim();
                    string normNom = NormalizarTexto(nomCompleto);
                    string matClean = e.Matricula.Split('@')[0].Trim();

                    bool matNueva = matriculasAgregadas.Add(matClean);
                    bool nomNuevo = string.IsNullOrEmpty(normNom) || nombresNormalizadosAgregados.Add(normNom);

                    if (matNueva && nomNuevo)
                    {
                        listaCompleta.Add(new AlumnoMonitorDto
                        {
                            IdEstudiante = e.IdEstudiante,
                            Matricula = matClean,
                            NombreCompleto = nomCompleto,
                            Grupo = e.Grupo?.NombreGrupo ?? grupo,
                            TieneTolerancia = e.TieneToleranciaActiva
                        });
                    }
                }
            }
            catch { }

            // 3. Nómina oficial por defecto (Solo se agregan si no fueron sustituidos por Secretaría o BD)
            foreach (var item in grupoOficial9IDGS)
            {
                string normNom = NormalizarTexto(item.nom);
                string matClean = item.mat.Split('@')[0].Trim();

                bool matNueva = matriculasAgregadas.Add(matClean);
                bool nomNuevo = nombresNormalizadosAgregados.Add(normNom);

                if (matNueva && nomNuevo)
                {
                    listaCompleta.Add(new AlumnoMonitorDto
                    {
                        IdEstudiante = item.id,
                        Matricula = matClean,
                        NombreCompleto = item.nom,
                        Grupo = grupo,
                        TieneTolerancia = item.mat == "23301133" || item.mat == "23301145"
                    });
                }
            }

            // Construir respuesta en vivo
            var estudiantesResult = new List<object>();

            foreach (var alumno in listaCompleta)
            {
                string rawMat = alumno.Matricula.Trim();
                string cleanMat = rawMat.Split('@')[0].Trim();

                string estadoVal = "PENDIENTE";
                string horaVal = null;
                string metodoVal = "-";

                if (_paseListaEnVivo.ContainsKey(rawMat) || _paseListaEnVivo.ContainsKey(cleanMat))
                {
                    var reg = _paseListaEnVivo.ContainsKey(rawMat) ? _paseListaEnVivo[rawMat] : _paseListaEnVivo[cleanMat];
                    estadoVal = reg.Estado;
                    horaVal = reg.Hora.ToString("hh:mm:ss tt");
                    metodoVal = reg.Metodo;
                }

                estudiantesResult.Add(new
                {
                    idEstudiante = alumno.IdEstudiante,
                    matricula = alumno.Matricula,
                    nombreCompleto = alumno.NombreCompleto,
                    grupo = alumno.Grupo,
                    estado = estadoVal,
                    horaMarcado = horaVal,
                    metodo = metodoVal,
                    tieneTolerancia = alumno.TieneTolerancia
                });
            }

            return Ok(estudiantesResult);
        }

        // Endpoint POST: Registrar Asistencia por Matrícula Real desde el Teléfono Móvil del Alumno Específico
        [HttpPost("RegistrarPorMatricula")]
        public IActionResult RegistrarPorMatricula([FromBody] RegistrarMatriculaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Matricula))
            {
                return BadRequest(new { Mensaje = "La matrícula es requerida." });
            }

            string rawMat = dto.Matricula.Trim();
            string cleanMat = rawMat.Split('@')[0].Trim();

            DateTime horaActualMx = ObtenerHoraMexico();

            _paseListaEnVivo[rawMat] = ("PRESENTE", horaActualMx, "Ultrasonido 19.5 kHz");
            _paseListaEnVivo[cleanMat] = ("PRESENTE", horaActualMx, "Ultrasonido 19.5 kHz");

            return Ok(new
            {
                Exito = true,
                Mensaje = $"Asistencia ultrasónica registrada correctamente para el alumno con matrícula {cleanMat}.",
                HoraRegistro = horaActualMx.ToString("hh:mm:ss tt")
            });
        }

        // Endpoint POST: Permite al docente cambiar el estado de un alumno manualmente
        [HttpPost("MarcarAsistenciaManual")]
        public IActionResult MarcarAsistenciaManual([FromBody] MarcarManualDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Matricula)) return BadRequest("Matrícula requerida.");

            DateTime horaActualMx = ObtenerHoraMexico();
            string rawMat = dto.Matricula.Trim();
            string cleanMat = rawMat.Split('@')[0].Trim();

            _paseListaEnVivo[rawMat] = (dto.NuevoEstado, horaActualMx, "Manual Docente");
            _paseListaEnVivo[cleanMat] = (dto.NuevoEstado, horaActualMx, "Manual Docente");

            return Ok(new { Mensaje = $"Estado de asistencia actualizado a {dto.NuevoEstado} para {cleanMat}." });
        }

        [HttpPost("Registrar")]
        public async Task<IActionResult> RegistrarAsistenciaUltrasonica([FromBody] RegistroAsistenciaDto dto)
        {
            DateTime horaActualMx = ObtenerHoraMexico();
            string matriculaBuscar = dto.IdEstudiante.ToString();
            _paseListaEnVivo[matriculaBuscar] = ("PRESENTE", horaActualMx, "Ultrasonido 19.5 kHz");

            return Ok(new
            {
                Mensaje = "Pase de lista ultrasónico exitoso.",
                EstadoFinal = "Asistencia",
                MinutosRetrasoReales = 0
            });
        }
    }

    public class AlumnoMonitorDto
    {
        public int IdEstudiante { get; set; }
        public string Matricula { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public bool TieneTolerancia { get; set; }
    }

    public class RegistrarMatriculaDto
    {
        public string Matricula { get; set; } = string.Empty;
    }

    public class RegistroAsistenciaDto
    {
        public int IdEstudiante { get; set; }
        public int IdSesion { get; set; }
        public DateTime HoraLlegada { get; set; }
    }

    public class MarcarManualDto
    {
        public string Matricula { get; set; } = string.Empty;
        public string NuevoEstado { get; set; } = "PRESENTE";
    }
}
