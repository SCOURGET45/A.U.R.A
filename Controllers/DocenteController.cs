using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using System.IO;

namespace Aura.Controllers
{
    [Authorize(Roles = "Docente")]
    public class DocenteController : Controller
    {
        public IActionResult Index()
        {
            return View(); 
        }

        // Este es el método que descarga el Excel
        [HttpGet]
        public IActionResult DescargarLista(int idGrupo)
        {
            // 1. Buscamos dónde guardaste la plantilla
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates", "FormatoUTTT.xlsx");
            
            // 2. Abrimos el Excel en la memoria RAM
            using var workbook = new XLWorkbook(templatePath);
            var hoja = workbook.Worksheet("Lista");

            // 3. Llenamos los encabezados (Por ahora con datos fijos para probar)
            hoja.Cell("E9").Value = "ADMINISTRACIÓN DE PROYECTOS DE TI"; 
            hoja.Cell("E10").Value = "NOVENO CUATRIMESTRE";
            hoja.Cell("E11").Value = "Odisey Yasmin Porras Beltrán"; // Aquí luego pondremos el nombre del usuario logueado

            // 4. Simulamos insertar al primer alumno en la fila 17
            hoja.Cell(17, 1).Value = "23301133";
            hoja.Cell(17, 2).Value = "23301133@uttt.edu.mx";
            hoja.Cell(17, 3).Value = "ALAN SANTIAGO MOLINA";
            
            // Simular que Alan asistió el primer día
            hoja.Cell(17, 4).Value = "."; 

            // 5. Preparamos el archivo para enviarlo al navegador
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            var content = stream.ToArray();

            // 6. Retornamos el Excel para que se descargue
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Lista_Grupo_{idGrupo}.xlsx");
        }
    }
}
