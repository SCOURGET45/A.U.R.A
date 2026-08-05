using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Aura.Data;
using Aura.Models;

namespace Aura.Controllers
{
    [Authorize(Roles = "Docente")]
    [Route("Docente")]
    public class DocenteController : Controller
    {
        private readonly AuraDbContext _context;

        // Almacén estático compartido para guardar el historial diario del reporte F-DC-02/R5
        public static readonly Dictionary<string, string> _historialAsistenciasFDC02 =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public DocenteController(AuraDbContext context)
        {
            _context = context;
        }

        // Helper para obtener alumnos unificados del grupo sin duplicados
        public List<AlumnoMonitorDto> ObtenerAlumnosUnificados(string grupo = "9IDGS-G2")
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

            var listaCompleta = new List<AlumnoMonitorDto>();
            var matriculasAgregadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var nombresAgregados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Alumnos dinámicos de Secretaría
            try
            {
                foreach (var am in SecretariaController._alumnosMemoria)
                {
                    string nomCompleto = $"{am.Nombre} {am.Apellidos}".Trim();
                    string normNom = AsistenciaController.NormalizarTexto(nomCompleto);
                    string matClean = am.Matricula.Split('@')[0].Trim();

                    if (matriculasAgregadas.Add(matClean) && (string.IsNullOrEmpty(normNom) || nombresAgregados.Add(normNom)))
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

            // 2. Alumnos BD
            try
            {
                var estudiantesDb = _context.Estudiantes
                    .Include(e => e.Grupo)
                    .Where(e => e.Grupo == null || e.Grupo.NombreGrupo == grupo || grupo == "9IDGS-G2")
                    .ToList();

                foreach (var e in estudiantesDb)
                {
                    string nomCompleto = $"{e.Nombre} {e.Apellidos}".Trim();
                    string normNom = AsistenciaController.NormalizarTexto(nomCompleto);
                    string matClean = e.Matricula.Split('@')[0].Trim();

                    if (matriculasAgregadas.Add(matClean) && (string.IsNullOrEmpty(normNom) || nombresAgregados.Add(normNom)))
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

            // 3. Roster oficial
            foreach (var item in grupoOficial9IDGS)
            {
                string normNom = AsistenciaController.NormalizarTexto(item.nom);
                string matClean = item.mat.Split('@')[0].Trim();

                if (matriculasAgregadas.Add(matClean) && nombresAgregados.Add(normNom))
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

            return listaCompleta;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(MiDia));
        }

        [HttpGet("MiDia")]
        public async Task<IActionResult> MiDia()
        {
            var nombreDocente = User.Identity?.Name ?? "Odisey Yasmin Porras Beltrán";
            var clasesViewModel = new List<ClaseHoy>();

            try
            {
                var dbClases = await _context.Sesiones.ToListAsync();
                if (dbClases.Any())
                {
                    int idSesion = 1;
                    foreach (var s in dbClases)
                    {
                        clasesViewModel.Add(new ClaseHoy
                        {
                            IdSesion = idSesion++,
                            Grupo = "9IDGS-G2",
                            Materia = "Administración de Proyectos de TI",
                            HoraInicio = s.HoraInicio,
                            HoraFin = s.HoraFin,
                            EstadoFase = "En Curso",
                            AlertasVulnerabilidad = true
                        });
                    }
                }
            }
            catch { }

            if (!clasesViewModel.Any())
            {
                clasesViewModel = new List<ClaseHoy>
                {
                    new ClaseHoy
                    {
                        IdSesion = 1,
                        Grupo = "9IDGS-G2",
                        Materia = "Administración de Proyectos de TI",
                        HoraInicio = new TimeSpan(8, 0, 0),
                        HoraFin = new TimeSpan(10, 0, 0),
                        EstadoFase = "En Curso",
                        AlertasVulnerabilidad = true
                    },
                    new ClaseHoy
                    {
                        IdSesion = 2,
                        Grupo = "9IDGS-G1",
                        Materia = "Desarrollo Web Profesional",
                        HoraInicio = new TimeSpan(10, 30, 0),
                        HoraFin = new TimeSpan(12, 30, 0),
                        EstadoFase = "En Curso",
                        AlertasVulnerabilidad = false
                    }
                };
            }

            var model = new DocenteMiDiaViewModel
            {
                NombreDocente = nombreDocente,
                FechaActual = AsistenciaController.ObtenerHoraMexico(),
                Clases = clasesViewModel
            };

            return View(model);
        }

        [HttpGet("MisGrupos")]
        public async Task<IActionResult> MisGrupos()
        {
            var nombreDocente = User.Identity?.Name ?? "Odisey Yasmin Porras Beltrán";

            var model = new DocenteMisGruposViewModel
            {
                NombreDocente = nombreDocente,
                Grupos = new List<DocenteGrupoCardViewModel>
                {
                    new DocenteGrupoCardViewModel
                    {
                        IdGrupo = 1,
                        NombreGrupo = "9IDGS-G2",
                        Carrera = "Desarrollo de Software Multiplataforma",
                        Cuatrimestre = "Noveno Cuatrimestre",
                        Materia = "Administración de Proyectos de TI",
                        TotalAlumnos = 28,
                        PromedioAsistencia = 89.2,
                        AlumnosEnRiesgoCount = 2,
                        RetardosConvertidosFaltasCount = 5,
                        AlumnosWithToleranciaCount = 3
                    },
                    new DocenteGrupoCardViewModel
                    {
                        IdGrupo = 2,
                        NombreGrupo = "9IDGS-G1",
                        Carrera = "Desarrollo de Software Multiplataforma",
                        Cuatrimestre = "Noveno Cuatrimestre",
                        Materia = "Desarrollo Web Profesional",
                        TotalAlumnos = 30,
                        PromedioAsistencia = 92.5,
                        AlumnosEnRiesgoCount = 1,
                        RetardosConvertidosFaltasCount = 3,
                        AlumnosWithToleranciaCount = 2
                    },
                    new DocenteGrupoCardViewModel
                    {
                        IdGrupo = 3,
                        NombreGrupo = "7MEC-G1",
                        Carrera = "Mecatrónica y Sistemas Automatizados",
                        Cuatrimestre = "Séptimo Cuatrimestre",
                        Materia = "Sistemas Embebidos",
                        TotalAlumnos = 25,
                        PromedioAsistencia = 84.0,
                        AlumnosEnRiesgoCount = 4,
                        RetardosConvertidosFaltasCount = 8,
                        AlumnosWithToleranciaCount = 1
                    }
                }
            };

            return View(model);
        }

        // POST: Finalizar Pase de Lista y Asentar Automáticamente en Reporte F-DC-02/R5 según el día y escaneos
        [HttpPost("FinalizarPaseLista")]
        public IActionResult FinalizarPaseLista(int semana, int diaSemana, string grupo = "9IDGS-G2")
        {
            var alumnos = ObtenerAlumnosUnificados(grupo);

            int presentes = 0;
            int faltas = 0;
            int retardos = 0;
            int justificados = 0;

            foreach (var alumno in alumnos)
            {
                string rawMat = alumno.Matricula.Trim();
                string cleanMat = rawMat.Split('@')[0].Trim();
                string key = $"{cleanMat}_S{semana}_D{diaSemana}";

                if (AsistenciaController._paseListaEnVivo.ContainsKey(rawMat) || AsistenciaController._paseListaEnVivo.ContainsKey(cleanMat))
                {
                    var reg = AsistenciaController._paseListaEnVivo.ContainsKey(cleanMat) ?
                        AsistenciaController._paseListaEnVivo[cleanMat] : AsistenciaController._paseListaEnVivo[rawMat];

                    if (reg.Estado == "PRESENTE")
                    {
                        _historialAsistenciasFDC02[key] = ".";
                        presentes++;
                    }
                    else if (reg.Estado == "RETARDO")
                    {
                        _historialAsistenciasFDC02[key] = "X";
                        retardos++;
                    }
                    else if (reg.Estado == "TOLERANCIA_ACTIVA" || reg.Estado == "JUSTIFICADO")
                    {
                        _historialAsistenciasFDC02[key] = "+=";
                        justificados++;
                    }
                    else
                    {
                        _historialAsistenciasFDC02[key] = "/";
                        faltas++;
                    }
                }
                else
                {
                    // Alumno no escaneó -> Falta '/'
                    _historialAsistenciasFDC02[key] = "/";
                    faltas++;
                }
            }

            TempData["Exito"] = $"Pase de lista asentado exitosamente en Reporte F-DC-02/R5 (Semana {semana} - Día {diaSemana}): {presentes} Asistencias (.), {faltas} Faltas (/), {retardos} Retardos (X), {justificados} Justificados (+=).";
            return RedirectToAction(nameof(ReporteSemanal), new { grupo = grupo });
        }

        // Vista Imprimible / PDF del Reporte Oficial UTTT F-DC-02/R5
        [HttpGet("ReporteSemanal")]
        public IActionResult ReporteSemanal(string grupo = "9IDGS-G2", string asignatura = "Administración de Proyectos de TI")
        {
            var alumnos = ObtenerAlumnosUnificados(grupo);

            ViewBag.Grupo = grupo;
            ViewBag.Asignatura = asignatura;
            ViewBag.Alumnos = alumnos;
            ViewBag.Historial = _historialAsistenciasFDC02;
            return View();
        }

        // Descarga de Excel F-DC-02/R5 de Control de Asistencias y Evaluaciones
        [HttpGet("DescargarReporteExcel")]
        public IActionResult DescargarReporteExcel(string grupo = "9IDGS-G2", string asignatura = "Administración de Proyectos de TI")
        {
            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("F-DC-02 R5 Control Asistencia");

            // Cabecera Institucional
            hoja.Cell("A1").Value = "UNIVERSIDAD TECNOLÓGICA DE TULA-TEPEJI";
            hoja.Cell("A1").Style.Font.SetBold().Font.SetFontSize(14).Font.SetFontColor(XLColor.DarkGreen);

            hoja.Cell("E1").Value = "CONTROL DE ASISTENCIAS Y EVALUACIONES";
            hoja.Cell("E1").Style.Font.SetBold().Font.SetFontSize(12);

            hoja.Cell("U1").Value = "Universidad Tecnológica de Tula-Tepeji\nOrganismo Público Descentralizado\ndel Gobierno del Estado de Hidalgo";
            hoja.Cell("U1").Style.Font.SetFontSize(8).Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

            // Bloque Metadatos
            hoja.Cell("A3").Value = "Programa Educativo : TECNOLOGÍAS DE LA INFORMACIÓN, INGENIERÍA EN DESARROLLO Y GESTIÓN DE SOFTWARE";
            hoja.Cell("A3").Style.Font.SetBold();
            hoja.Cell("A4").Value = $"Asignatura : {asignatura.ToUpper()}";
            hoja.Cell("U4").Value = $"Grupo : {grupo}";
            hoja.Cell("A5").Value = "Cuatrimestre : NOVENO CUATRIMESTRE";
            hoja.Cell("A6").Value = $"Docente : {User.Identity?.Name ?? "Odisey Yasmin Porras Beltrán"}";

            // Encabezados Matriz de Semanas (Fila 8-10)
            hoja.Cell("A8").Value = "#";
            hoja.Cell("B8").Value = "Nombre del Alumno";

            string[] semanas = { "Semana 1", "Semana 2", "Semana 3", "Semana 4", "Semana 5" };
            int col = 3;
            foreach (var sem in semanas)
            {
                var rngSem = hoja.Range(8, col, 8, col + 3);
                rngSem.Merge().Value = sem;
                rngSem.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                rngSem.Style.Font.SetBold();
                rngSem.Style.Fill.SetBackgroundColor(XLColor.LightGray);

                hoja.Cell(9, col).Value = "Lu";
                hoja.Cell(9, col + 1).Value = "Ma";
                hoja.Cell(9, col + 2).Value = "Mi";
                hoja.Cell(9, col + 3).Value = "Ju";
                col += 4;
            }

            hoja.Cell("W8").Value = "TC";
            hoja.Cell("X8").Value = "TA";
            hoja.Cell("Y8").Value = "TF";
            hoja.Cell("Z8").Value = "% As";

            var hdrRange = hoja.Range(8, 1, 9, 26);
            hdrRange.Style.Font.SetBold();
            hdrRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
            hdrRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

            var alumnos = ObtenerAlumnosUnificados(grupo);

            int row = 10;
            int idx = 1;
            foreach (var item in alumnos)
            {
                hoja.Cell(row, 1).Value = idx;
                hoja.Cell(row, 2).Value = item.NombreCompleto;

                int tc = 0;
                int ta = 0;
                int tf = 0;

                int colIdx = 3;
                for (int sem = 1; sem <= 5; sem++)
                {
                    for (int dia = 1; dia <= 4; dia++)
                    {
                        string cleanMat = item.Matricula.Split('@')[0].Trim();
                        string key = $"{cleanMat}_S{sem}_D{dia}";
                        string marca = ".";

                        if (_historialAsistenciasFDC02.ContainsKey(key))
                        {
                            marca = _historialAsistenciasFDC02[key];
                        }
                        else
                        {
                            // Marcas por defecto simuladas si no se ha asentado ese día
                            if (colIdx == 7 && idx % 3 == 0) marca = "X";
                            else if (colIdx == 11 && (idx == 4 || idx == 17)) marca = "/";
                            else if (colIdx == 14 && (idx == 1 || idx == 13)) marca = "+=";
                            else if (colIdx == 19 && (idx == 4 || idx == 17)) marca = "/";
                        }

                        hoja.Cell(row, colIdx).Value = marca;
                        hoja.Cell(row, colIdx).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        colIdx++;

                        tc++;
                        if (marca == "." || marca == "+=") ta++;
                        else if (marca == "/") tf++;
                        else if (marca == "X") ta++;
                    }
                }

                double pct = tc > 0 ? Math.Round(((double)ta / tc) * 100, 1) : 100.0;

                hoja.Cell(row, 23).Value = tc;
                hoja.Cell(row, 24).Value = ta;
                hoja.Cell(row, 25).Value = tf;
                hoja.Cell(row, 26).Value = $"{pct}%";

                if (pct < 80)
                {
                    hoja.Range(row, 1, row, 26).Style.Fill.SetBackgroundColor(XLColor.LightPink);
                }

                row++;
                idx++;
            }

            var gridRange = hoja.Range(8, 1, row - 1, 26);
            gridRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Medium);
            gridRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

            int symRow = row + 1;
            hoja.Cell(symRow, 1).Value = "Simbología:";
            hoja.Cell(symRow, 1).Style.Font.SetBold();
            hoja.Cell(symRow + 1, 1).Value = ". = Asistencia   / = falta   += = Falta justificada   X = Retardo";
            hoja.Cell(symRow + 2, 1).Value = "TC = Total de clases   TA = Total asistencia   TF = Total faltas";
            hoja.Cell(symRow + 2, 23).Value = "F-DC-02/R5 - Septiembre 02, 2015";
            hoja.Cell(symRow + 2, 23).Style.Font.SetBold().Font.SetFontSize(8);

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Reporte_Asistencia_UTTT_F-DC-02R5_{grupo}.xlsx");
        }

        [HttpGet("DescargarLista/{idGrupo}")]
        public IActionResult DescargarLista(int idGrupo)
        {
            var alumnos = ObtenerAlumnosUnificados("9IDGS-G2");
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("No.,Matricula,NombreCompleto,Grupo");
            int i = 1;
            foreach (var a in alumnos)
            {
                sb.AppendLine($"{i++},{a.Matricula},{a.NombreCompleto},{a.Grupo}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"Lista_Oficial_Grupo_9IDGS-G2.csv");
        }
    }
}
