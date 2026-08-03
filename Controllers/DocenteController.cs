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
    public class DocenteController : Controller
    {
        private readonly AuraDbContext _context;

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

            var model = new DocenteMiDiaViewModel
            {
                NombreDocente = nombreDocente,
                FechaActual = DateTime.Now,
                Clases = new List<ClaseHoy>
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
                        EstadoFase = "Pendiente",
                        AlertasVulnerabilidad = false
                    }
                }
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

        // Método para descargar la lista de asistencia en formato Excel institucional
        [HttpGet("DescargarLista/{idGrupo}")]
        public IActionResult DescargarLista(int idGrupo)
        {
            using var workbook = new XLWorkbook();
            var hoja = workbook.Worksheets.Add("Lista de Asistencia");

            // Encabezados institucionales
            hoja.Cell("B2").Value = "UNIVERSIDAD TECNOLÓGICA DE TULA-TEPEJI";
            hoja.Cell("B2").Style.Font.SetBold().Font.SetFontSize(14);
            hoja.Cell("B3").Value = "SISTEMA A.U.R.A. - LISTA DE ASISTENCIA Y EVALUACIÓN";
            hoja.Cell("B3").Style.Font.SetBold();

            hoja.Cell("B5").Value = "Carrera: Desarrollo de Software Multiplataforma";
            hoja.Cell("B6").Value = "Cuatrimestre: Noveno Cuatrimestre";
            hoja.Cell("B7").Value = $"Grupo ID: {idGrupo}";
            hoja.Cell("B8").Value = $"Docente: {User.Identity?.Name ?? "Odisey Yasmin Porras Beltrán"}";
            hoja.Cell("B9").Value = $"Fecha de Emisión: {DateTime.Now:dd/MM/yyyy HH:mm}";

            // Tabla de Alumnos
            int filaInicial = 11;
            hoja.Cell(filaInicial, 1).Value = "#";
            hoja.Cell(filaInicial, 2).Value = "Matrícula";
            hoja.Cell(filaInicial, 3).Value = "Nombre Completo";
            hoja.Cell(filaInicial, 4).Value = "Asistencias";
            hoja.Cell(filaInicial, 5).Value = "Retardos";
            hoja.Cell(filaInicial, 6).Value = "Faltas Totales (3R=1F)";
            hoja.Cell(filaInicial, 7).Value = "% Asistencia";
            hoja.Cell(filaInicial, 8).Value = "Estatus Derecho";

            var rangoEncabezados = hoja.Range(filaInicial, 1, filaInicial, 8);
            rangoEncabezados.Style.Font.SetBold();
            rangoEncabezados.Style.Fill.SetBackgroundColor(XLColor.DarkGray);
            rangoEncabezados.Style.Font.FontColor = XLColor.White;

            // Datos de demostración estructurados
            var alumnos = new[]
            {
                new { Mat = "23301133", Nom = "ALAN SANTIAGO MOLINA", Asis = 18, Ret = 2, Faltas = 1, Pct = 90.0, Est = "CON DERECHO" },
                new { Mat = "23301145", Nom = "MARÍA FERNANDA GÓMEZ", Asis = 15, Ret = 3, Faltas = 3, Pct = 83.3, Est = "CON DERECHO" },
                new { Mat = "23301199", Nom = "CARLOS EDUARDO PÉREZ", Asis = 19, Ret = 1, Faltas = 0, Pct = 95.0, Est = "CON DERECHO" },
                new { Mat = "23301201", Nom = "DANIELA RÍOS CÁRDENAS", Asis = 14, Ret = 4, Faltas = 5, Pct = 70.0, Est = "SIN DERECHO (REPROBADO)" }
            };

            int fila = filaInicial + 1;
            int count = 1;
            foreach (var a in alumnos)
            {
                hoja.Cell(fila, 1).Value = count++;
                hoja.Cell(fila, 2).Value = a.Mat;
                hoja.Cell(fila, 3).Value = a.Nom;
                hoja.Cell(fila, 4).Value = a.Asis;
                hoja.Cell(fila, 5).Value = a.Ret;
                hoja.Cell(fila, 6).Value = a.Faltas;
                hoja.Cell(fila, 7).Value = $"{a.Pct}%";
                hoja.Cell(fila, 8).Value = a.Est;

                if (a.Est.Contains("SIN DERECHO"))
                {
                    hoja.Range(fila, 1, fila, 8).Style.Fill.SetBackgroundColor(XLColor.LightCoral);
                }

                fila++;
            }

            hoja.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Lista_Asistencia_Grupo_{idGrupo}.xlsx");
        }
    }
}
