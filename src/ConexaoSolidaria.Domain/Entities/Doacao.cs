using ConexaoSolidaria.Domain.Enums;

namespace ConexaoSolidaria.Domain.Entities;

public class Doacao
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampanhaId { get; set; }
    public Guid DoadorId { get; set; }
    public decimal ValorDoacao { get; set; }
    public DoacaoStatus Status { get; set; } = DoacaoStatus.Pendente;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessadoEm { get; set; }

    public Campanha? Campanha { get; set; }
    public Usuario? Doador { get; set; }
}
