using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AURA.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Grupos",
                columns: table => new
                {
                    IdGrupo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCuatrimestre = table.Column<int>(type: "int", nullable: false),
                    NombreGrupo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdTutor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupos", x => x.IdGrupo);
                });

            migrationBuilder.CreateTable(
                name: "Justificantes",
                columns: table => new
                {
                    IdJustificante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    IdTutorEmisor = table.Column<int>(type: "int", nullable: false),
                    DiasAmparados = table.Column<int>(type: "int", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaEmision = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Justificantes", x => x.IdJustificante);
                });

            migrationBuilder.CreateTable(
                name: "Materias",
                columns: table => new
                {
                    IdMateria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdCuatrimestre = table.Column<int>(type: "int", nullable: false),
                    NombreMateria = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materias", x => x.IdMateria);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    IdRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreRol = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.IdRol);
                });

            migrationBuilder.CreateTable(
                name: "Sesiones",
                columns: table => new
                {
                    IdSesion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdGrupo = table.Column<int>(type: "int", nullable: false),
                    IdDocente = table.Column<int>(type: "int", nullable: false),
                    FechaSesion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    TokenUltrasónicoActivo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sesiones", x => x.IdSesion);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                columns: table => new
                {
                    IdUnidad = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMateria = table.Column<int>(type: "int", nullable: false),
                    NombreUnidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalClasesProgramadas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.IdUnidad);
                });

            migrationBuilder.CreateTable(
                name: "UnidadesAcademicas",
                columns: table => new
                {
                    IdUnidadAcademica = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdMateria = table.Column<int>(type: "int", nullable: false),
                    NumeroUnidad = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalClasesProgramadas = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnidadesAcademicas", x => x.IdUnidadAcademica);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    IdUsuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdRol = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CorreoElectronico = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContrasenaHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    RolIdRol = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.IdUsuario);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolIdRol",
                        column: x => x.RolIdRol,
                        principalTable: "Roles",
                        principalColumn: "IdRol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Asistencias",
                columns: table => new
                {
                    IdAsistencia = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSesion = table.Column<int>(type: "int", nullable: false),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    FechaHoraRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ValidacionUltrasonica = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asistencias", x => x.IdAsistencia);
                    table.ForeignKey(
                        name: "FK_Asistencias_Sesiones_IdSesion",
                        column: x => x.IdSesion,
                        principalTable: "Sesiones",
                        principalColumn: "IdSesion",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Estudiantes",
                columns: table => new
                {
                    IdEstudiante = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Matricula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TieneToleranciaActiva = table.Column<bool>(type: "bit", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    UsuarioIdUsuario = table.Column<int>(type: "int", nullable: false),
                    IdGrupo = table.Column<int>(type: "int", nullable: false),
                    GrupoIdGrupo = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estudiantes", x => x.IdEstudiante);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Grupos_GrupoIdGrupo",
                        column: x => x.GrupoIdGrupo,
                        principalTable: "Grupos",
                        principalColumn: "IdGrupo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Estudiantes_Usuarios_UsuarioIdUsuario",
                        column: x => x.UsuarioIdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesVulnerabilidad",
                columns: table => new
                {
                    IdSolicitud = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdEstudiante = table.Column<int>(type: "int", nullable: false),
                    EstudianteIdEstudiante = table.Column<int>(type: "int", nullable: false),
                    CategoriaMotivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JustificacionTutor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dictamen = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinutosToleranciaOtorgados = table.Column<int>(type: "int", nullable: false),
                    IdTutor = table.Column<int>(type: "int", nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaJuntaComision = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdDirectorResolucion = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesVulnerabilidad", x => x.IdSolicitud);
                    table.ForeignKey(
                        name: "FK_SolicitudesVulnerabilidad_Estudiantes_EstudianteIdEstudiante",
                        column: x => x.EstudianteIdEstudiante,
                        principalTable: "Estudiantes",
                        principalColumn: "IdEstudiante",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asistencias_IdSesion",
                table: "Asistencias",
                column: "IdSesion");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_GrupoIdGrupo",
                table: "Estudiantes",
                column: "GrupoIdGrupo");

            migrationBuilder.CreateIndex(
                name: "IX_Estudiantes_UsuarioIdUsuario",
                table: "Estudiantes",
                column: "UsuarioIdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesVulnerabilidad_EstudianteIdEstudiante",
                table: "SolicitudesVulnerabilidad",
                column: "EstudianteIdEstudiante");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolIdRol",
                table: "Usuarios",
                column: "RolIdRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asistencias");

            migrationBuilder.DropTable(
                name: "Justificantes");

            migrationBuilder.DropTable(
                name: "Materias");

            migrationBuilder.DropTable(
                name: "SolicitudesVulnerabilidad");

            migrationBuilder.DropTable(
                name: "Unidades");

            migrationBuilder.DropTable(
                name: "UnidadesAcademicas");

            migrationBuilder.DropTable(
                name: "Sesiones");

            migrationBuilder.DropTable(
                name: "Estudiantes");

            migrationBuilder.DropTable(
                name: "Grupos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
