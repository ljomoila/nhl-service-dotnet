using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using nhl_service_dotnet.Data;

#nullable disable

namespace nhl_service_dotnet.Migrations
{
    [DbContext(typeof(NhlDbContext))]
    partial class NhlDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.17");

            modelBuilder.Entity("nhl_service_dotnet.Team", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    b.Property<string>("abbreviation")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<string>("link")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("name")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<string>("shortName")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.HasKey("id");

                    b.ToTable("teams", (string)null);
                });

            modelBuilder.Entity("nhl_service_dotnet.Models.Player", b =>
                {
                    b.Property<int>("id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    b.Property<int>("playerType")
                        .HasColumnType("integer");

                    b.Property<string>("fullName")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<string>("lastName")
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<string>("link")
                        .HasMaxLength(500)
                        .HasColumnType("character varying(500)");

                    b.Property<string>("nationality")
                        .HasMaxLength(20)
                        .HasColumnType("character varying(20)");

                    b.Property<int?>("TeamId")
                        .HasColumnType("integer");

                    b.HasKey("id");

                    b.HasIndex("TeamId");

                    b.ToTable("players", (string)null);
                });

            modelBuilder.Entity("nhl_service_dotnet.Models.Player", b =>
                {
                    b.HasOne("nhl_service_dotnet.Team", null)
                        .WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
