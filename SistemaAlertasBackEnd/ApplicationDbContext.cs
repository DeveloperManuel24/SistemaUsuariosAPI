using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SistemaAlertasBackEnd
{
    public class ApplicationDbContext : IdentityDbContext//Aqui agregas automaticamente las tablas y todo lo necesario para los usuarios
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }


        //ApiFluente:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Genero>().Property(p => p.Nombre).HasMaxLength(50);
            //modelBuilder.Entity<Actor>().Property(p => p.Nombre).HasMaxLength(150);
            //modelBuilder.Entity<Actor>().Property(p => p.Foto).IsUnicode(false);
            //modelBuilder.Entity<Pelicula>().Property(p => p.Titulo).HasMaxLength(150);
            //modelBuilder.Entity<Pelicula>().Property(p => p.Poster).IsUnicode(false);
            //modelBuilder.Entity<GeneroPelicula>().HasKey(g => new { g.GeneroId, g.PeliculaId });
            //modelBuilder.Entity<ActorPelicula>().HasKey(a => new { a.PeliculaId, a.ActorId });


            //Personalización para las tablas de los usuarios:
            modelBuilder.Entity<IdentityUser>().ToTable("Usuarios");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RolesClaims");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UsuariosClaims");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UsuariosLogins");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UsuariosRoles");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UsuariosTokens");

        }




        //public DbSet<Genero> Generos { get; set; }
        //public DbSet<Actor> Actores { get; set; }
        //public DbSet<Pelicula> Peliculas { get; set; }
        //public DbSet<Comentario> Comentarios { get; set; }
        //public DbSet<GeneroPelicula> GeneroPeliculas { get; set; }
        //public DbSet<ActorPelicula> ActorPeliculas { get; set; }
        //public DbSet<Error> Errores { get; set; }

    }

}
