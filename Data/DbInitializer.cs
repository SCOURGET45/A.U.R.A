using System;
using System.Linq;
using Aura.Models;

namespace Aura.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AuraDbContext context)
        {
            try
            {
                context.Database.EnsureCreated();

                // 1. Roles
                if (!context.Roles.Any())
                {
                    var roles = new[]
                    {
                        new Rol { NombreRol = "Estudiante" },
                        new Rol { NombreRol = "Docente" },
                        new Rol { NombreRol = "Tutor" },
                        new Rol { NombreRol = "Secretaria" },
                        new Rol { NombreRol = "Director" }
                    };

                    context.Roles.AddRange(roles);
                    context.SaveChanges();
                }

                // 2. Usuarios base del personal
                if (!context.Usuarios.Any(u => u.CorreoElectronico == "secretaria@uttt.edu.mx"))
                {
                    int idRolSecretaria = context.Roles.Where(r => r.NombreRol == "Secretaria").Select(r => r.IdRol).FirstOrDefault();
                    if (idRolSecretaria == 0) idRolSecretaria = 4;

                    int idRolDocente = context.Roles.Where(r => r.NombreRol == "Docente").Select(r => r.IdRol).FirstOrDefault();
                    if (idRolDocente == 0) idRolDocente = 2;

                    int idRolTutor = context.Roles.Where(r => r.NombreRol == "Tutor").Select(r => r.IdRol).FirstOrDefault();
                    if (idRolTutor == 0) idRolTutor = 3;

                    int idRolDirector = context.Roles.Where(r => r.NombreRol == "Director").Select(r => r.IdRol).FirstOrDefault();
                    if (idRolDirector == 0) idRolDirector = 5;

                    context.Usuarios.AddRange(new[]
                    {
                        new Usuario { NombreCompleto = "Secretaría Académica UTTT", CorreoElectronico = "secretaria@uttt.edu.mx", ContrasenaHash = "123456", IdRol = idRolSecretaria, Activo = true },
                        new Usuario { NombreCompleto = "Odisey Yasmin Porras Beltrán", CorreoElectronico = "docente@uttt.edu.mx", ContrasenaHash = "123456", IdRol = idRolDocente, Activo = true },
                        new Usuario { NombreCompleto = "Prof. Tutor Institucional", CorreoElectronico = "tutor@uttt.edu.mx", ContrasenaHash = "123456", IdRol = idRolTutor, Activo = true },
                        new Usuario { NombreCompleto = "Director de Carrera TI", CorreoElectronico = "director@uttt.edu.mx", ContrasenaHash = "123456", IdRol = idRolDirector, Activo = true }
                    });

                    context.SaveChanges();
                }

                // 3. Grupos
                if (!context.Grupos.Any())
                {
                    int idTutorUsar = context.Usuarios.Where(u => u.CorreoElectronico == "tutor@uttt.edu.mx").Select(u => u.IdUsuario).FirstOrDefault();
                    if (idTutorUsar == 0) idTutorUsar = 1;

                    context.Grupos.AddRange(new[]
                    {
                        new Grupo { NombreGrupo = "9IDGS-G2", IdCuatrimestre = 9, IdTutor = idTutorUsar },
                        new Grupo { NombreGrupo = "9IDGS-G1", IdCuatrimestre = 9, IdTutor = idTutorUsar },
                        new Grupo { NombreGrupo = "7MEC-G1", IdCuatrimestre = 7, IdTutor = idTutorUsar }
                    });

                    context.SaveChanges();
                }

                int idGrupo9IDGS = context.Grupos.Where(g => g.NombreGrupo == "9IDGS-G2").Select(g => g.IdGrupo).FirstOrDefault();
                if (idGrupo9IDGS == 0) idGrupo9IDGS = 1;

                int idRolEstudiante = context.Roles.Where(r => r.NombreRol == "Estudiante").Select(r => r.IdRol).FirstOrDefault();
                if (idRolEstudiante == 0) idRolEstudiante = 1;

                // 4. Cargar nómina completa de 26+ alumnos si la tabla de estudiantes está vacía
                if (!context.Estudiantes.Any())
                {
                    var alumnosOficiales = new[]
                    {
                        new { Mat = "23301133", Nom = "ALAN", Ape = "SANTIAGO MOLINA" },
                        new { Mat = "23301145", Nom = "MARÍA", Ape = "FERNANDA GÓMEZ" },
                        new { Mat = "23301199", Nom = "CARLOS", Ape = "EDUARDO PÉREZ" },
                        new { Mat = "23301201", Nom = "DANIELA", Ape = "RÍOS CÁRDENAS" },
                        new { Mat = "23301205", Nom = "ALBERTO", Ape = "CRUZ ZEPEDA" },
                        new { Mat = "23301206", Nom = "ALDO ALEXIS", Ape = "MEZA ARGUELLES" },
                        new { Mat = "23301210", Nom = "BRIDGED CITLALI", Ape = "CORNEJO YAÑEZ" },
                        new { Mat = "23301211", Nom = "CHRISTOPHER", Ape = "CAMARGO GONZALEZ" },
                        new { Mat = "23301215", Nom = "DELIA LESLIE", Ape = "JIMENEZ NERI" },
                        new { Mat = "23301216", Nom = "DIEGO", Ape = "PARRA CRUZ" },
                        new { Mat = "23301220", Nom = "DORIAN ALEJANDRO", Ape = "TREJO VEGA" },
                        new { Mat = "23301221", Nom = "FATIMA XIMENA", Ape = "GARCIA GONZALEZ" },
                        new { Mat = "23301225", Nom = "FELICITAS RUBI", Ape = "DIEGO GARCIA" },
                        new { Mat = "23301230", Nom = "JESSUI", Ape = "FLORES PACHECO" },
                        new { Mat = "23301231", Nom = "JOSE DE JESUS", Ape = "LOPEZ ISLAS" },
                        new { Mat = "23301235", Nom = "LEONARDO ISAAC", Ape = "BARRERA TEJEDA" },
                        new { Mat = "23301236", Nom = "LIZETH", Ape = "PEREZ ATANACIO" },
                        new { Mat = "23301240", Nom = "MARIA DEL ROCIO", Ape = "CRUZ CERVANTES" },
                        new { Mat = "23301241", Nom = "MARISOL", Ape = "GONZALEZ VILLA" },
                        new { Mat = "23301245", Nom = "MELANIE JOLIEE", Ape = "BONILLA DOMINGUEZ" },
                        new { Mat = "23301246", Nom = "OMAR", Ape = "PICAZO ARANZOLO" },
                        new { Mat = "23301250", Nom = "OSCAR JOSE", Ape = "SALINAS ESCOBAR" },
                        new { Mat = "23301251", Nom = "RODRIGO", Ape = "DOMINGUEZ CRESPO" },
                        new { Mat = "23301255", Nom = "RODRIGO", Ape = "SANCHEZ CRUZ" },
                        new { Mat = "23301256", Nom = "VICTOR MANUEL", Ape = "RUFIN PIÑA" },
                        new { Mat = "23301260", Nom = "YAEL", Ape = "MONROY CRUZ" }
                    };

                    foreach (var a in alumnosOficiales)
                    {
                        var usr = new Usuario
                        {
                            NombreCompleto = $"{a.Nom} {a.Ape}",
                            CorreoElectronico = $"{a.Mat}@uttt.edu.mx",
                            ContrasenaHash = "123456",
                            IdRol = idRolEstudiante,
                            Activo = true
                        };
                        context.Usuarios.Add(usr);
                        context.SaveChanges();

                        var est = new Estudiante
                        {
                            Matricula = a.Mat,
                            Nombre = a.Nom,
                            Apellidos = a.Ape,
                            IdGrupo = idGrupo9IDGS,
                            IdUsuario = usr.IdUsuario,
                            TieneToleranciaActiva = a.Mat == "23301133" || a.Mat == "23301145"
                        };
                        context.Estudiantes.Add(est);
                    }

                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Advertencia en Sembrado de BD: " + ex.Message);
            }
        }
    }
}
