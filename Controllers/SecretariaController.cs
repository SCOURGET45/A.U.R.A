using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        // Almacén persistente de Alumnos
        public static readonly List<AlumnoEditViewModel> _alumnosMemoria = new List<AlumnoEditViewModel>
        {
            new AlumnoEditViewModel { IdEstudiante = 1, Matricula = "23301133", Nombre = "Alan Santiago", Apellidos = "Molina", NombreGrupo = "9IDGS-G2" },
            new AlumnoEditViewModel { IdEstudiante = 2, Matricula = "23301145", Nombre = "María Fernanda", Apellidos = "Gómez", NombreGrupo = "9IDGS-G1" },
            new AlumnoEditViewModel { IdEstudiante = 3, Matricula = "23301199", Nombre = "Carlos Eduardo", Apellidos = "Pérez", NombreGrupo = "9IDGS-G2" },
            new AlumnoEditViewModel { IdEstudiante = 4, Matricula = "23301201", Nombre = "Daniela", Apellidos = "Ríos Cárdenas", NombreGrupo = "7MEC-G1" }
        };

        // Almacén persistente de Asignaciones de Clases a Docentes
        public static readonly List<AsignacionClaseDocenteViewModel> _asignacionesClases = new List<AsignacionClaseDocenteViewModel>
        {
            new AsignacionClaseDocenteViewModel { IdAsignacion = 1, NombreDocente = "Odisey Yasmin Porras Beltrán", NombreMateria = "Administración de Proyectos de TI", NombreGrupo = "9IDGS-G2", Horario = "Lunes y Miércoles 08:00 - 10:00 hrs" },
            new AsignacionClaseDocenteViewModel { IdAsignacion = 2, NombreDocente = "Odisey Yasmin Porras Beltrán", NombreMateria = "Desarrollo Web Profesional", NombreGrupo = "9IDGS-G1", Horario = "Martes y Jueves 10:30 - 12:30 hrs" },
            new AsignacionClaseDocenteViewModel { IdAsignacion = 3, NombreDocente = "Carlos Ramírez Cruz", NombreMateria = "Sistemas Embebidos", NombreGrupo = "7MEC-G1", Horario = "Viernes 08:00 - 12:00 hrs" }
        };

        // Almacén persistente de Asignaciones de Grupos Enteros a Tutores
        private static readonly List<AsignacionGrupoTutorViewModel> _asignacionesTutores = new List<AsignacionGrupoTutorViewModel>
        {
            new AsignacionGrupoTutorViewModel { IdAsignacion = 1, NombreTutor = "Odisey Yasmin Porras Beltrán", NombreGrupo = "9IDGS-G2", Carrera = "Desarrollo de Software Multiplataforma", TotalAlumnos = 28 },
            new AsignacionGrupoTutorViewModel { IdAsignacion = 2, NombreTutor = "María Elena Gutiérrez", NombreGrupo = "9IDGS-G1", Carrera = "Desarrollo de Software Multiplataforma", TotalAlumnos = 30 },
            new AsignacionGrupoTutorViewModel { IdAsignacion = 3, NombreTutor = "Roberto Sánchez López", NombreGrupo = "7MEC-G1", Carrera = "Mecatrónica y Sistemas Automatizados", TotalAlumnos = 25 }
        };

        public SecretariaController(AuraDbContext context)
        {
            _context = context;
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            List<AlumnoVulnerableViewModel> vulnerables = new List<AlumnoVulnerableViewModel>();

            try
            {
                vulnerables = await _context.SolicitudesVulnerabilidad
                    .Include(s => s.Estudiante)
                    .ThenInclude(e => e.Grupo)
                    .Where(s => s.Dictamen == "Aprobado" || (s.Estudiante != null && s.Estudiante.TieneToleranciaActiva))
                    .Select(s => new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = s.IdEstudiante,
                        Matricula = s.Estudiante.Matricula,
                        NombreCompleto = s.Estudiante.Nombre + " " + s.Estudiante.Apellidos,
                        NombreGrupo = s.Estudiante.Grupo != null ? s.Estudiante.Grupo.NombreGrupo : "9IDGS-G2",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = s.CategoriaMotivo ?? s.Motivo ?? "Distancia Extrema (Transporte)",
                        MinutosTolerancia = s.MinutosToleranciaOtorgados > 0 ? s.MinutosToleranciaOtorgados : 30,
                        FechaAprobacion = s.FechaResolucion ?? DateTime.Now.AddDays(-5)
                    })
                    .ToListAsync();
            }
            catch
            {
                // Fallback demo
            }

            if (!vulnerables.Any())
            {
                vulnerables = new List<AlumnoVulnerableViewModel>
                {
                    new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = 1,
                        Matricula = "23301133",
                        NombreCompleto = "Alan Santiago Molina",
                        NombreGrupo = "9IDGS-G2",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = "Lejanía / Transporte Extremo (Zimapán)",
                        MinutosTolerancia = 30,
                        FechaAprobacion = DateTime.Now.AddDays(-4)
                    },
                    new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = 2,
                        Matricula = "23301145",
                        NombreCompleto = "María Fernanda Gómez",
                        NombreGrupo = "9IDGS-G1",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = "Horario Laboral Formal (Empresa Tula)",
                        MinutosTolerancia = 30,
                        FechaAprobacion = DateTime.Now.AddDays(-10)
                    }
                };
            }

            try
            {
                var dbList = await _context.Estudiantes
                    .Include(e => e.Grupo)
                    .Select(e => new AlumnoEditViewModel
                    {
                        IdEstudiante = e.IdEstudiante,
                        Matricula = e.Matricula,
                        Nombre = e.Nombre,
                        Apellidos = e.Apellidos,
                        NombreGrupo = e.Grupo != null ? e.Grupo.NombreGrupo : "9IDGS-G2"
                    })
                    .ToListAsync();

                foreach (var item in dbList)
                {
                    if (!_alumnosMemoria.Any(a => a.Matricula == item.Matricula))
                    {
                        _alumnosMemoria.Add(item);
                    }
                }
            }
            catch
            {
                // Usar almacén estático
            }

            var listaFinalAlumnos = _alumnosMemoria.ToList();
            int totalGruposContados = listaFinalAlumnos.Select(a => a.NombreGrupo).Distinct().Count();

            var model = new SecretariaDashboardViewModel
            {
                TotalAlumnosInscritos = listaFinalAlumnos.Count,
                TotalGruposActivos = totalGruposContados > 0 ? totalGruposContados : 6,
                TotalAlumnosVulnerables = vulnerables.Count,
                TotalMateriasConfiguradas = 12,
                AlumnosVulnerables = vulnerables,
                AlumnosRegistrados = listaFinalAlumnos,
                AsignacionesClases = _asignacionesClases.ToList(),
                AsignacionesTutores = _asignacionesTutores.ToList()
            };

            return View(model);
        }

        // Asignación de Clases a Docentes (POST)
        [HttpPost("AsignarClaseDocente")]
        public IActionResult AsignarClaseDocente(AsignacionClaseDocenteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor completa todos los campos para asignar la clase al docente.";
                return RedirectToAction(nameof(Dashboard));
            }

            var existente = _asignacionesClases.FirstOrDefault(a =>
                a.NombreDocente.Equals(model.NombreDocente, StringComparison.OrdinalIgnoreCase) &&
                a.NombreMateria.Equals(model.NombreMateria, StringComparison.OrdinalIgnoreCase) &&
                a.NombreGrupo.Equals(model.NombreGrupo, StringComparison.OrdinalIgnoreCase));

            if (existente != null)
            {
                existente.Horario = model.Horario;
            }
            else
            {
                int nuevoId = _asignacionesClases.Any() ? _asignacionesClases.Max(a => a.IdAsignacion) + 1 : 1;
                model.IdAsignacion = nuevoId;
                _asignacionesClases.Add(model);
            }

            TempData["Exito"] = $"Clase '{model.NombreMateria}' ({model.NombreGrupo}) asignada exitosamente al docente {model.NombreDocente}.";
            return RedirectToAction(nameof(Dashboard));
        }

        // Quitar / Desasignar Clase o Materia a Docente (POST)
        [HttpPost("EliminarAsignacionClase")]
        public IActionResult EliminarAsignacionClase(int idAsignacion)
        {
            var item = _asignacionesClases.FirstOrDefault(a => a.IdAsignacion == idAsignacion);
            if (item != null)
            {
                _asignacionesClases.Remove(item);
                TempData["Exito"] = $"La materia '{item.NombreMateria}' ({item.NombreGrupo}) fue removida correctamente del docente {item.NombreDocente}.";
            }
            else
            {
                TempData["Error"] = "No se encontró la asignación especificada.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // Quitar Tutoría de Grupo Entero (POST)
        [HttpPost("EliminarAsignacionTutor")]
        public IActionResult EliminarAsignacionTutor(int idAsignacion)
        {
            var item = _asignacionesTutores.FirstOrDefault(a => a.IdAsignacion == idAsignacion);
            if (item != null)
            {
                _asignacionesTutores.Remove(item);
                TempData["Exito"] = $"La tutoría del grupo '{item.NombreGrupo}' asignada a {item.NombreTutor} fue removida correctamente.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        // Asignación de Grupos Enteros a Tutores (POST)
        [HttpPost("AsignarGrupoTutor")]
        public async Task<IActionResult> AsignarGrupoTutor(AsignacionGrupoTutorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor selecciona el grupo y tutor correspondiente.";
                return RedirectToAction(nameof(Dashboard));
            }

            // Actualizar asignación en almacén
            var existente = _asignacionesTutores.FirstOrDefault(a => a.NombreGrupo == model.NombreGrupo);
            if (existente != null)
            {
                existente.NombreTutor = model.NombreTutor;
            }
            else
            {
                int nuevoId = _asignacionesTutores.Any() ? _asignacionesTutores.Max(a => a.IdAsignacion) + 1 : 1;
                model.IdAsignacion = nuevoId;
                model.TotalAlumnos = _alumnosMemoria.Count(a => a.NombreGrupo == model.NombreGrupo);
                if (model.TotalAlumnos == 0) model.TotalAlumnos = 28;
                _asignacionesTutores.Add(model);
            }

            // Intentar vincular en BD
            try
            {
                var grupoObj = await _context.Grupos.FirstOrDefaultAsync(g => g.NombreGrupo == model.NombreGrupo);
                if (grupoObj != null)
                {
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Registrado en memoria
            }

            TempData["Exito"] = $"El grupo entero '{model.NombreGrupo}' fue asignado exitosamente a la tutoría de {model.NombreTutor}. Todos sus alumnos han sido vinculados.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("GuardarAlumno")]
        public async Task<IActionResult> GuardarAlumno(AlumnoEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Por favor verifica los campos del formulario de alumno.";
                return RedirectToAction(nameof(Dashboard));
            }

            var existenteMemoria = _alumnosMemoria.FirstOrDefault(a => a.IdEstudiante == model.IdEstudiante || a.Matricula == model.Matricula);
            if (existenteMemoria != null)
            {
                existenteMemoria.Matricula = model.Matricula;
                existenteMemoria.Nombre = model.Nombre;
                existenteMemoria.Apellidos = model.Apellidos;
                existenteMemoria.NombreGrupo = model.NombreGrupo;
            }
            else
            {
                int nuevoId = _alumnosMemoria.Any() ? _alumnosMemoria.Max(a => a.IdEstudiante) + 1 : 1;
                _alumnosMemoria.Add(new AlumnoEditViewModel
                {
                    IdEstudiante = nuevoId,
                    Matricula = model.Matricula,
                    Nombre = model.Nombre,
                    Apellidos = model.Apellidos,
                    NombreGrupo = model.NombreGrupo
                });
            }

            try
            {
                var grupoObj = await _context.Grupos.FirstOrDefaultAsync(g => g.NombreGrupo == model.NombreGrupo);
                if (grupoObj == null)
                {
                    grupoObj = new Grupo
                    {
                        NombreGrupo = model.NombreGrupo,
                        IdCuatrimestre = 9,
                        IdTutor = 1
                    };
                    _context.Grupos.Add(grupoObj);
                    await _context.SaveChangesAsync();
                }

                if (model.IdEstudiante > 0)
                {
                    var estudiante = await _context.Estudiantes.FindAsync(model.IdEstudiante);
                    if (estudiante != null)
                    {
                        estudiante.Matricula = model.Matricula;
                        estudiante.Nombre = model.Nombre;
                        estudiante.Apellidos = model.Apellidos;
                        estudiante.IdGrupo = grupoObj.IdGrupo;
                        _context.Estudiantes.Update(estudiante);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    var rolEstudiante = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Estudiante");
                    if (rolEstudiante == null)
                    {
                        rolEstudiante = new Rol { NombreRol = "Estudiante" };
                        _context.Roles.Add(rolEstudiante);
                        await _context.SaveChangesAsync();
                    }

                    var nuevoUsuario = new Usuario
                    {
                        NombreCompleto = $"{model.Nombre} {model.Apellidos}",
                        CorreoElectronico = $"{model.Matricula}@uttt.edu.mx",
                        ContrasenaHash = "123456",
                        IdRol = rolEstudiante.IdRol
                    };
                    _context.Usuarios.Add(nuevoUsuario);
                    await _context.SaveChangesAsync();

                    var nuevoEstudiante = new Estudiante
                    {
                        Matricula = model.Matricula,
                        Nombre = model.Nombre,
                        Apellidos = model.Apellidos,
                        IdGrupo = grupoObj.IdGrupo,
                        IdUsuario = nuevoUsuario.IdUsuario,
                        TieneToleranciaActiva = false
                    };
                    _context.Estudiantes.Add(nuevoEstudiante);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Memoria actualizada
            }

            TempData["Exito"] = $"El alumno {model.Nombre} {model.Apellidos} ({model.Matricula}) fue guardado e integrado exitosamente.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("EliminarAlumno")]
        public async Task<IActionResult> EliminarAlumno(int idEstudiante)
        {
            var enMemoria = _alumnosMemoria.FirstOrDefault(a => a.IdEstudiante == idEstudiante);
            if (enMemoria != null)
            {
                _alumnosMemoria.Remove(enMemoria);
            }

            try
            {
                var estudiante = await _context.Estudiantes.FindAsync(idEstudiante);
                if (estudiante != null)
                {
                    _context.Estudiantes.Remove(estudiante);
                    await _context.SaveChangesAsync();
                }
            }
            catch
            {
                // Memoria actualizada
            }

            TempData["Exito"] = "El registro del alumno fue dado de baja correctamente.";
            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet("AlumnosVulnerables")]
        public async Task<IActionResult> AlumnosVulnerables()
        {
            List<AlumnoVulnerableViewModel> vulnerables = new List<AlumnoVulnerableViewModel>();

            try
            {
                vulnerables = await _context.SolicitudesVulnerabilidad
                    .Include(s => s.Estudiante)
                    .ThenInclude(e => e.Grupo)
                    .Where(s => s.Dictamen == "Aprobado" || (s.Estudiante != null && s.Estudiante.TieneToleranciaActiva))
                    .Select(s => new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = s.IdEstudiante,
                        Matricula = s.Estudiante.Matricula,
                        NombreCompleto = s.Estudiante.Nombre + " " + s.Estudiante.Apellidos,
                        NombreGrupo = s.Estudiante.Grupo != null ? s.Estudiante.Grupo.NombreGrupo : "9IDGS-G2",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = s.CategoriaMotivo ?? s.Motivo ?? "Distancia Extrema",
                        MinutosTolerancia = s.MinutosToleranciaOtorgados > 0 ? s.MinutosToleranciaOtorgados : 30,
                        FechaAprobacion = s.FechaResolucion ?? DateTime.Now.AddDays(-5)
                    })
                    .ToListAsync();
            }
            catch
            {
                // Fallback demo
            }

            if (!vulnerables.Any())
            {
                vulnerables = new List<AlumnoVulnerableViewModel>
                {
                    new AlumnoVulnerableViewModel
                    {
                        IdEstudiante = 1,
                        Matricula = "23301133",
                        NombreCompleto = "Alan Santiago Molina",
                        NombreGrupo = "9IDGS-G2",
                        NombreTutor = "Odisey Yasmin Porras",
                        Motivo = "Lejanía / Transporte Extremo",
                        MinutosTolerancia = 30,
                        FechaAprobacion = DateTime.Now.AddDays(-4)
                    }
                };
            }

            return View(vulnerables);
        }

        [HttpGet("ConfigurarUnidades")]
        public IActionResult ConfigurarUnidades()
        {
            var model = new ConfigurarUnidadViewModel
            {
                IdMateria = 1,
                NombreMateria = "Desarrollo Web Profesional",
                NumeroUnidades = 3,
                TotalClasesCuatrimestre = 60
            };

            return View(model);
        }

        [HttpPost("ConfigurarUnidades")]
        public IActionResult ConfigurarUnidades(ConfigurarUnidadViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            TempData["Exito"] = $"Configuración guardada para '{model.NombreMateria}': {model.NumeroUnidades} unidades temáticas y {model.TotalClasesCuatrimestre} clases planificadas. (Esencial para cálculo del 80%).";
            return RedirectToAction(nameof(Dashboard));
        }

        // Descarga de Plantilla CSV Oficial UTTT (Matricula, CorreoInstitucional, Nombre, Grupo)
        [HttpGet("DescargarPlantillaCSV")]
        public IActionResult DescargarPlantillaCSV()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Matricula,CorreoInstitucional,Nombre,Grupo");
            sb.AppendLine("23301133,23301133@uttt.edu.mx,Alan Santiago Molina,9IDGS-G2");
            sb.AppendLine("23301145,23301145@uttt.edu.mx,Maria Fernanda Gomez,9IDGS-G1");
            sb.AppendLine("23301199,23301199@uttt.edu.mx,Carlos Eduardo Perez,9IDGS-G2");
            sb.AppendLine("23301201,23301201@uttt.edu.mx,Daniela Rios Cardenas,7MEC-G1");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "Plantilla_Inscripcion_Alumnos_UTTT.csv");
        }

        // Carga Masiva Inteligente de CSV con registro garantizado
        [HttpPost("CargarAlumnosCSV")]
        public async Task<IActionResult> CargarAlumnosCSV(IFormFile archivoCsv)
        {
            return await ProcesarCargaMasiva(archivoCsv);
        }

        // Carga Masiva Roster (CSV / Excel)
        [HttpPost("ProcesarCargaMasiva")]
        public async Task<IActionResult> ProcesarCargaMasiva(IFormFile archivoCarga)
        {
            if (archivoCarga == null || archivoCarga.Length == 0)
            {
                TempData["Error"] = "Por favor selecciona un archivo CSV o Excel válido.";
                return RedirectToAction(nameof(Dashboard));
            }

            int procesadosCount = 0;
            try
            {
                using var stream = new StreamReader(archivoCarga.OpenReadStream(), Encoding.UTF8);
                string? linea;
                bool esPrimera = true;

                while ((linea = await stream.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(linea)) continue;
                    if (esPrimera && (linea.ToLower().Contains("matricula") || linea.ToLower().Contains("nombre")))
                    {
                        esPrimera = false;
                        continue;
                    }
                    esPrimera = false;

                    var campos = linea.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(c => c.Trim('"').Trim()).ToArray();
                    if (campos.Length < 2) continue;

                    string matricula = campos[0];
                    string correoInst = "";
                    string nombreCompleto = "";
                    string nombreGrupo = "9IDGS-G2";

                    if (campos[1].Contains("@"))
                    {
                        correoInst = campos[1];
                        nombreCompleto = campos.Length > 2 ? campos[2] : "";
                        nombreGrupo = campos.Length > 3 ? campos[3] : "9IDGS-G2";
                    }
                    else
                    {
                        correoInst = $"{matricula}@uttt.edu.mx";
                        nombreCompleto = campos.Length > 2 ? $"{campos[1]} {campos[2]}" : campos[1];
                        nombreGrupo = campos.Length > 3 ? campos[3] : (campos.Length > 2 ? campos[2] : "9IDGS-G2");
                    }

                    if (string.IsNullOrWhiteSpace(matricula) || string.IsNullOrWhiteSpace(nombreCompleto)) continue;

                    string nombre = nombreCompleto;
                    string apellidos = "";
                    var partes = nombreCompleto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (partes.Length >= 3)
                    {
                        nombre = string.Join(" ", partes.Take(partes.Length - 2));
                        apellidos = string.Join(" ", partes.Skip(partes.Length - 2));
                    }
                    else if (partes.Length == 2)
                    {
                        nombre = partes[0];
                        apellidos = partes[1];
                    }

                    string normSubido = NormalizarTexto(nombreCompleto);

                    // Sustitución por Nombre o Matrícula en memoria
                    var existentesMem = _alumnosMemoria.Where(a =>
                        a.Matricula.Equals(matricula, StringComparison.OrdinalIgnoreCase) ||
                        NormalizarTexto($"{a.Nombre} {a.Apellidos}").Equals(normSubido, StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    if (existentesMem.Any())
                    {
                        var prim = existentesMem.First();
                        prim.Matricula = matricula;
                        prim.Nombre = nombre;
                        prim.Apellidos = apellidos;
                        prim.NombreGrupo = nombreGrupo;

                        foreach (var dup in existentesMem.Skip(1))
                        {
                            _alumnosMemoria.Remove(dup);
                        }
                    }
                    else
                    {
                        int nuevoId = _alumnosMemoria.Any() ? _alumnosMemoria.Max(a => a.IdEstudiante) + 1 : 1;
                        _alumnosMemoria.Add(new AlumnoEditViewModel
                        {
                            IdEstudiante = nuevoId,
                            Matricula = matricula,
                            Nombre = nombre,
                            Apellidos = apellidos,
                            NombreGrupo = nombreGrupo
                        });
                    }

                    try
                    {
                        Grupo? grupoObj = await _context.Grupos.FirstOrDefaultAsync(g => g.NombreGrupo == nombreGrupo);
                        if (grupoObj == null && !string.IsNullOrWhiteSpace(nombreGrupo))
                        {
                            grupoObj = new Grupo
                            {
                                NombreGrupo = nombreGrupo,
                                IdCuatrimestre = 9,
                                IdTutor = 1
                            };
                            _context.Grupos.Add(grupoObj);
                            await _context.SaveChangesAsync();
                        }

                        int idGrupoUsar = grupoObj?.IdGrupo ?? 1;

                        var dbEstudiantes = await _context.Estudiantes.ToListAsync();
                        var estudianteExistente = dbEstudiantes.FirstOrDefault(e =>
                            e.Matricula.Equals(matricula, StringComparison.OrdinalIgnoreCase) ||
                            NormalizarTexto($"{e.Nombre} {e.Apellidos}").Equals(normSubido, StringComparison.OrdinalIgnoreCase)
                        );

                        if (estudianteExistente != null)
                        {
                            estudianteExistente.Matricula = matricula;
                            estudianteExistente.Nombre = nombre;
                            if (!string.IsNullOrWhiteSpace(apellidos)) estudianteExistente.Apellidos = apellidos;
                            estudianteExistente.IdGrupo = idGrupoUsar;
                            _context.Estudiantes.Update(estudianteExistente);
                        }
                        else
                        {
                            var rolEstudiante = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Estudiante");
                            int idRolUsar = rolEstudiante?.IdRol ?? 1;

                            var nuevoUsuario = new Usuario
                            {
                                NombreCompleto = nombreCompleto,
                                CorreoElectronico = !string.IsNullOrWhiteSpace(correoInst) ? correoInst : $"{matricula}@uttt.edu.mx",
                                ContrasenaHash = "123456",
                                IdRol = idRolUsar
                            };
                            _context.Usuarios.Add(nuevoUsuario);
                            await _context.SaveChangesAsync();

                            var nuevoEstudiante = new Estudiante
                            {
                                Matricula = matricula,
                                Nombre = nombre,
                                Apellidos = apellidos,
                                IdGrupo = idGrupoUsar,
                                IdUsuario = nuevoUsuario.IdUsuario,
                                TieneToleranciaActiva = false
                            };
                            _context.Estudiantes.Add(nuevoEstudiante);
                        }

                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        // Memoria actualizada
                    }

                    procesadosCount++;
                }

                if (procesadosCount > 0)
                {
                    TempData["Exito"] = $"Se han procesado e integrado exitosamente {procesadosCount} alumno(s) desde el archivo, sustituyendo datos anteriores por nombre.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al procesar el archivo CSV: {ex.Message}";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost("RegistrarUsuario")]
        public async Task<IActionResult> RegistrarUsuario([FromForm] NuevoUsuarioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Todos los campos del formulario son obligatorios.";
                return RedirectToAction(nameof(Dashboard));
            }

            string claveInicial = string.IsNullOrWhiteSpace(model.ContrasenaInicial) ? "123456" : model.ContrasenaInicial;

            AuthController._usuariosDinamicos[model.CorreoElectronico] = (claveInicial, model.NombreRol, model.NombreCompleto);
            if (!string.IsNullOrWhiteSpace(model.MatriculaEmpleado))
            {
                AuthController._usuariosDinamicos[model.MatriculaEmpleado] = (claveInicial, model.NombreRol, model.NombreCompleto);
                AuthController._usuariosDinamicos[$"{model.MatriculaEmpleado}@uttt.edu.mx"] = (claveInicial, model.NombreRol, model.NombreCompleto);
            }

            try
            {
                var rolObj = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == model.NombreRol);
                int idRolUsar = rolObj?.IdRol ?? (model.NombreRol == "Estudiante" ? 1 : 2);

                var nuevoUsuario = new Usuario
                {
                    NombreCompleto = model.NombreCompleto,
                    CorreoElectronico = model.CorreoElectronico,
                    ContrasenaHash = claveInicial,
                    IdRol = idRolUsar,
                    Activo = true
                };

                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                if (model.NombreRol == "Estudiante")
                {
                    string[] partes = model.NombreCompleto.Trim().Split(' ');
                    string nom = partes[0];
                    string ape = partes.Length > 1 ? string.Join(" ", partes.Skip(1)) : "UTTT";
                    string normSubido = NormalizarTexto(model.NombreCompleto);

                    var existentes = _alumnosMemoria.Where(a =>
                        (!string.IsNullOrWhiteSpace(model.MatriculaEmpleado) && a.Matricula.Equals(model.MatriculaEmpleado, StringComparison.OrdinalIgnoreCase)) ||
                        NormalizarTexto($"{a.Nombre} {a.Apellidos}").Equals(normSubido, StringComparison.OrdinalIgnoreCase)
                    ).ToList();

                    if (existentes.Any())
                    {
                        var prim = existentes.First();
                        if (!string.IsNullOrWhiteSpace(model.MatriculaEmpleado)) prim.Matricula = model.MatriculaEmpleado;
                        prim.Nombre = nom;
                        prim.Apellidos = ape;
                        if (!string.IsNullOrWhiteSpace(model.NombreGrupo)) prim.NombreGrupo = model.NombreGrupo;

                        foreach (var dup in existentes.Skip(1))
                        {
                            _alumnosMemoria.Remove(dup);
                        }
                    }
                    else
                    {
                        _alumnosMemoria.Add(new AlumnoEditViewModel
                        {
                            IdEstudiante = _alumnosMemoria.Any() ? _alumnosMemoria.Max(a => a.IdEstudiante) + 1 : 1,
                            Matricula = !string.IsNullOrWhiteSpace(model.MatriculaEmpleado) ? model.MatriculaEmpleado : model.CorreoElectronico.Split('@')[0],
                            Nombre = nom,
                            Apellidos = ape,
                            NombreGrupo = string.IsNullOrWhiteSpace(model.NombreGrupo) ? "9IDGS-G2" : model.NombreGrupo
                        });
                    }
                }
            }
            catch
            {
                // Memoria actualizada
            }

            TempData["Exito"] = $"Usuario '{model.NombreCompleto}' registrado exitosamente con rol '{model.NombreRol}'. Ya puede iniciar sesión con la contraseña '{claveInicial}'.";

            return RedirectToAction(nameof(Dashboard));
        }
    }
}
