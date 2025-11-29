using Microsoft.EntityFrameworkCore;
using nhl_service_dotnet.Models;
using nhl_service_dotnet.Models.Game;

namespace nhl_service_dotnet.Data
{
    public class NhlDbContext : DbContext
    {
        public NhlDbContext(DbContextOptions<NhlDbContext> options) : base(options) { }

        public DbSet<Team> Teams { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Team>(entity =>
            {
                entity.ToTable("teams");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id).ValueGeneratedNever();
                entity.Property(e => e.name).HasMaxLength(300);
                entity.Property(e => e.shortName).HasMaxLength(300);
                entity.Property(e => e.abbreviation).HasMaxLength(20);
                entity.Property(e => e.link).HasMaxLength(500);
            });

            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("players");
                entity.HasKey(e => e.id);
                entity.Property(e => e.id).ValueGeneratedNever();
                entity.Property(e => e.fullName).HasMaxLength(300);
                entity.Property(e => e.lastName).HasMaxLength(300);
                entity.Property(e => e.nationality).HasMaxLength(20);
                entity.Property(e => e.link).HasMaxLength(500);
                entity.Property(e => e.playerType).HasConversion<int>();
                entity.Property<int?>("TeamId");

                entity
                    .HasOne<Team>()
                    .WithMany()
                    .HasForeignKey("TeamId")
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
