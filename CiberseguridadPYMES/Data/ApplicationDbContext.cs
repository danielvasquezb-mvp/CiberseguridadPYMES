using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CiberseguridadPYMES.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empresa> Empresas => Set<Empresa>();

        public DbSet<TipoSistema> TiposSistema => Set<TipoSistema>();

        public DbSet<Formulario> Formularios => Set<Formulario>();

        public DbSet<Dimension> Dimensiones => Set<Dimension>();

        public DbSet<Pregunta> Preguntas => Set<Pregunta>();

        public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();

        public DbSet<Respuesta> Respuestas => Set<Respuesta>();

        public DbSet<Diagnostico> Diagnosticos => Set<Diagnostico>();

        public DbSet<ResultadoDimension> ResultadosDimension
            => Set<ResultadoDimension>();

        public DbSet<Hallazgo> Hallazgos => Set<Hallazgo>();

        public DbSet<Recomendacion> Recomendaciones
            => Set<Recomendacion>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Una evaluación puede tener como máximo un diagnóstico.
            builder.Entity<Diagnostico>()
                .HasIndex(d => d.EvaluacionId)
                .IsUnique();

            builder.Entity<Diagnostico>()
                .HasOne(d => d.Evaluacion)
                .WithOne(e => e.Diagnostico)
                .HasForeignKey<Diagnostico>(d => d.EvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una respuesta pertenece a una evaluación.
            builder.Entity<Respuesta>()
                .HasOne(r => r.Evaluacion)
                .WithMany(e => e.Respuestas)
                .HasForeignKey(r => r.EvaluacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una respuesta corresponde a una pregunta.
            builder.Entity<Respuesta>()
                .HasOne(r => r.Pregunta)
                .WithMany(p => p.Respuestas)
                .HasForeignKey(r => r.PreguntaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una pregunta pertenece a un formulario.
            builder.Entity<Pregunta>()
                .HasOne(p => p.Formulario)
                .WithMany(f => f.Preguntas)
                .HasForeignKey(p => p.FormularioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una pregunta pertenece a una dimensión.
            builder.Entity<Pregunta>()
                .HasOne(p => p.Dimension)
                .WithMany(d => d.Preguntas)
                .HasForeignKey(p => p.DimensionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Una empresa pertenece a un usuario.
            builder.Entity<Empresa>()
                .HasOne(e => e.Usuario)
                .WithMany(u => u.Empresas)
                .HasForeignKey(e => e.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Evitar respuestas duplicadas para la misma pregunta
            // dentro de una evaluación.
            builder.Entity<Respuesta>()
                .HasIndex(r => new { r.EvaluacionId, r.PreguntaId })
                .IsUnique();

            // Evitar códigos de dimensión duplicados.
            builder.Entity<Dimension>()
                .HasIndex(d => d.Codigo)
                .IsUnique();
        }

    }
}
