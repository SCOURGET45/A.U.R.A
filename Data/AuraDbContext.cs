using Microsoft.EntityFrameworkCore;
using Aura.Models;

namespace Aura.Data
{
    public class AuraDbContext : DbContext
    {
        public AuraDbContext(DbContextOptions<AuraDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Grupo> Grupos { get; set; }
        public DbSet<Estudiante> Estudiantes { get; set; }
        public DbSet<SolicitudVulnerabilidad> SolicitudesVulnerabilidad { get; set; }
        public DbSet<Justificantes> Justificantes { get; set; }
        public DbSet<Materia> Materias { get; set; }
        public DbSet<Unidad> Unidades { get; set; }
    }
}
