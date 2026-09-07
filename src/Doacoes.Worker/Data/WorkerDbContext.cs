using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Doacoes.Worker.Data;

public class WorkerDbContext(DbContextOptions<WorkerDbContext> options) : DbContext(options)
{
    public DbSet<Campanha> Campanhas => Set<Campanha>();
    public DbSet<Doacao> Doacoes => Set<Doacao>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.NomeCompleto).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.Cpf).HasMaxLength(11);
            e.Property(x => x.SenhaHash).IsRequired();
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Campanha>(e =>
        {
            e.ToTable("campanhas");
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Descricao).HasMaxLength(4000).IsRequired();
            e.Property(x => x.MetaFinanceira).HasPrecision(18, 2);
            e.Property(x => x.ValorArrecadado).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<Doacao>(e =>
        {
            e.ToTable("doacoes");
            e.HasKey(x => x.Id);
            e.Property(x => x.ValorDoacao).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Campanha).WithMany(c => c.Doacoes).HasForeignKey(x => x.CampanhaId);
            e.HasOne(x => x.Doador).WithMany().HasForeignKey(x => x.DoadorId);
        });
    }
}
