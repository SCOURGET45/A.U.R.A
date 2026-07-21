using Microsoft.AspNetCore.Mvc;
using Aura.Services;

namespace Aura.Controllers
{
    public class EstudianteController : Controller
    {
        private readonly IAsistenciaService _asistenciaService;

        public EstudianteController(IAsistenciaService asistenciaService)
        {
            _asistenciaService = asistenciaService;
        }

        public IActionResult DashboardUnidad(int idUnidad)
        {
            // Aquí iría la consulta real con Entity Framework
            // var historial = _dbContext.Asistencias.Where(a => a.IdUnidad == idUnidad).ToList();
            // var totalClases = ...

            // var resultado = _asistenciaService.CalcularAsistenciaUnidad(historial, totalClases);

            // return View(resultado);

            return View();
        }
    }
}
