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

        public IActionResult Index()
        {
            return RedirectToAction(nameof(MiDia));
        }

        [HttpGet("MiDia")]
        public async Task<IActionResult> MiDia()
        {
            var nombreDocente = User.Identity?.Name ?? "Odisey Yasmin Porras Beltrán";

            var clasesViewModel = new List<ClaseHoy>();

            // Cargar clases asignadas desde Secretaría o fallback demo
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
            catch
            {
                // Usar clases asignadas por Secretaría
            }

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
                FechaActual = DateTime.Now,
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

        // POST: Finalizar Pase de Lista y Asentar Automáticamente en Reporte F-DC-02/R5
        [HttpPost("FinalizarPaseLista")]
        public IActionResult FinalizarPaseLista(int semana, int diaSemana, string grupo = "9IDGS-G2")
        {
            var listaMatriculas = new[] {
                "23301133", "23301145", "23301199", "23301201", "23301205", "23301206",
                "23301210", "23301211", "23301215", "23301216", "23301220", "23301221",
                "23301225", "23301230", "23301231", "23301235", "23301236", "23301240",
                "23301241", "23301245", "23301246", "23301250", "23301251", "23301255",
                "23301256", "23301260"
            };

            int presentes = 0;
            int faltas = 0;

            foreach (var mat in listaMatriculas)
            {
                string key = $"{mat}_S{semana}_D{diaSemana}";
                if (AsistenciaController._paseListaEnVivo.ContainsKey(mat))
                {
                    var est = AsistenciaController._paseListaEnVivo[mat].Estado;
                    if (est == "PRESENTE")
                    {
                        _historialAsistenciasFDC02[key] = ".";
                        presentes++;
                    }
                    else if (est == "RETARDO")
                    {
                        _historialAsistenciasFDC02[key] = "X";
                        presentes++;
                    }
                    else if (est == "TOLERANCIA_ACTIVA")
                    {
                        _historialAsistenciasFDC02[key] = "+=";
                        presentes++;
                    }
                    else
                    {
                        _historialAsistenciasFDC02[key] = "/";
                        faltas++;
                    }
                }
                else
                {
                    _historialAsistenciasFDC02[key] = "/"; // Sin escanear = Falta
                    faltas++;
                }
            }

            TempData["Exito"] = $"Pase de lista de la sesión concluido exitosamente ({presentes} Asistencias, {faltas} Faltas asentadas en Semana {semana} - Día {diaSemana}). Se actualizó el Reporte Oficial F-DC-02/R5.";
            return RedirectToAction(nameof(ReporteSemanal), new { grupo = grupo });
        }

        // Vista Imprimible / PDF del Reporte Oficial UTTT F-DC-02/R5
        [HttpGet("ReporteSemanal")]
        public IActionResult ReporteSemanal(string grupo = "9IDGS-G2", string asignatura = "Administración de Proyectos de TI")
        {
            ViewBag.Grupo = grupo;
            ViewBag.Asignatura = asignatura;
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

            // Alumnos del grupo
            var listaAlumnos = new[] {
                new { Mat = "23301133", Nom = "ALAN SANTIAGO MOLINA" },
                new { Mat = "23301145", Nom = "MARÍA FERNANDA GÓMEZ" },
                new { Mat = "23301199", Nom = "CARLOS EDUARDO PÉREZ" },
                new { Mat = "23301201", Nom = "DANIELA RÍOS CÁRDENAS" },
                new { Mat = "23301205", Nom = "ALBERTO CRUZ ZEPEDA" },
                new { Mat = "23301206", Nom = "ALDO ALEXIS MEZA ARGUELLES" },
                new { Mat = "23301210", Nom = "BRIDGED CITLALI CORNEJO YAÑEZ" },
                new { Mat = "23301211", Nom = "CHRISTOPHER CAMARGO GONZALEZ" },
                new { Mat = "23301215", Nom = "DELIA LESLIE JIMENEZ NERI" },
                new { Mat = "23301216", Nom = "DIEGO PARRA CRUZ" },
                new { Mat = "23301220", Nom = "DORIAN ALEJANDRO TREJO VEGA" },
                new { Mat = "23301221", Nom = "FATIMA XIMENA GARCIA GONZALEZ" },
                new { Mat = "23301225", Nom = "FELICITAS RUBI DIEGO GARCIA" },
                new { Mat = "23301230", Nom = "JESSUI FLORES PACHECO" },
                new { Mat = "23301231", Nom = "JOSE DE JESUS LOPEZ ISLAS" },
                new { Mat = "23301235", Nom = "LEONARDO ISAAC BARRERA TEJEDA" },
                new { Mat = "23301236", Nom = "LIZETH PEREZ ATANACIO" },
                new { Mat = "23301240", Nom = "MARIA DEL ROCIO CRUZ CERVANTES" },
                new { Mat = "23301241", Nom = "MARISOL GONZALEZ VILLA" },
                new { Mat = "23301245", Nom = "MELANIE JOLIEE BONILLA DOMINGUEZ" },
                new { Mat = "23301246", Nom = "OMAR PICAZO ARANZOLO" },
                new { Mat = "23301250", Nom = "OSCAR JOSE SALINAS ESCOBAR" },
                new { Mat = "23301251", Nom = "RODRIGO DOMINGUEZ CRESPO" },
                new { Mat = "23301255", Nom = "RODRIGO SANCHEZ CRUZ" },
                new { Mat = "23301256", Nom = "VICTOR MANUEL RUFIN PIÑA" },
                new { Mat = "23301260", Nom = "YAEL MONROY CRUZ" }
            };

            int row = 10;
            int idx = 1;
            foreach (var item in listaAlumnos)
            {
                hoja.Cell(row, 1).Value = idx;
                hoja.Cell(row, 2).Value = item.Nom;

                bool esRiesgo = idx == 4 || idx == 17;
                bool tieneRet = idx % 3 == 0;
                bool tieneJust = idx == 1 || idx == 13;

                int colIdx = 3;
                for (int sem = 1; sem <= 5; sem++)
                {
                    for (int dia = 1; dia <= 4; dia++)
                    {
                        string key = $"{item.Mat}_S{sem}_D{dia}";
                        string marca = ".";

                        if (_historialAsistenciasFDC02.ContainsKey(key))
                        {
                            marca = _historialAsistenciasFDC02[key];
                        }
                        else
                        {
                            if (colIdx == 7 && tieneRet) marca = "X";
                            else if (colIdx == 11 && esRiesgo) marca = "/";
                            else if (colIdx == 14 && tieneJust) marca = "+=";
                            else if (colIdx == 19 && esRiesgo) marca = "/";
                        }

                        hoja.Cell(row, colIdx).Value = marca;
                        hoja.Cell(row, colIdx).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        colIdx++;
                    }
                }

                int tc = 20;
                int ta = esRiesgo ? 14 : (tieneRet ? 18 : 19);
                int tf = tc - ta;
                double pct = Math.Round(((double)ta / tc) * 100, 1);

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
            return RedirectToAction(nameof(DescargarReporteExcel), new { grupo = idGrupo == 1 ? "9IDGS-G2" : "9IDGS-G1" });
        }
    }
}
