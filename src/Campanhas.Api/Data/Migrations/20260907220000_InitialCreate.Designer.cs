using Campanhas.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campanhas.Api.Data.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260907220000_InitialCreate")]
partial class InitialCreate
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder.HasAnnotation("ProductVersion", "8.0.11");
        modelBuilder.HasAnnotation("Relational:MaxIdentifierLength", 63);

        modelBuilder.Entity("ConexaoSolidaria.Domain.Entities.Campanha", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<DateTime?>("AtualizadoEm")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime>("CriadoEm")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime>("DataFim")
                .HasColumnType("timestamp with time zone");

            b.Property<DateTime>("DataInicio")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Descricao")
                .IsRequired()
                .HasMaxLength(4000)
                .HasColumnType("character varying(4000)");

            b.Property<decimal>("MetaFinanceira")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<string>("Titulo")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<decimal>("ValorArrecadado")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.HasKey("Id");

            b.ToTable("campanhas", (string?)null);
        });

        modelBuilder.Entity("ConexaoSolidaria.Domain.Entities.Doacao", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<Guid>("CampanhaId")
                .HasColumnType("uuid");

            b.Property<DateTime>("CriadoEm")
                .HasColumnType("timestamp with time zone");

            b.Property<Guid>("DoadorId")
                .HasColumnType("uuid");

            b.Property<DateTime?>("ProcessadoEm")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Status")
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("character varying(20)");

            b.Property<decimal>("ValorDoacao")
                .HasPrecision(18, 2)
                .HasColumnType("numeric(18,2)");

            b.HasKey("Id");

            b.HasIndex("CampanhaId");

            b.HasIndex("DoadorId");

            b.ToTable("doacoes", (string?)null);
        });

        modelBuilder.Entity("ConexaoSolidaria.Domain.Entities.Usuario", b =>
        {
            b.Property<Guid>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("uuid");

            b.Property<string>("Cpf")
                .HasMaxLength(11)
                .HasColumnType("character varying(11)");

            b.Property<DateTime>("CriadoEm")
                .HasColumnType("timestamp with time zone");

            b.Property<string>("Email")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<string>("NomeCompleto")
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("character varying(200)");

            b.Property<string>("Role")
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("character varying(50)");

            b.Property<string>("SenhaHash")
                .IsRequired()
                .HasColumnType("text");

            b.HasKey("Id");

            b.HasIndex("Email")
                .IsUnique();

            b.ToTable("usuarios", (string?)null);
        });

        modelBuilder.Entity("ConexaoSolidaria.Domain.Entities.Doacao", b =>
        {
            b.HasOne("ConexaoSolidaria.Domain.Entities.Campanha", "Campanha")
                .WithMany("Doacoes")
                .HasForeignKey("CampanhaId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.HasOne("ConexaoSolidaria.Domain.Entities.Usuario", "Doador")
                .WithMany()
                .HasForeignKey("DoadorId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            b.Navigation("Campanha");

            b.Navigation("Doador");
        });

        modelBuilder.Entity("ConexaoSolidaria.Domain.Entities.Campanha", b =>
        {
            b.Navigation("Doacoes");
        });
#pragma warning restore 612, 618
    }
}
