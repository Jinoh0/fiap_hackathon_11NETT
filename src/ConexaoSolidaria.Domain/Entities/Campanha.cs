using ConexaoSolidaria.Domain.Enums;

namespace ConexaoSolidaria.Domain.Entities;

public class Campanha
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Titulo { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public decimal MetaFinanceira { get; set; }
    public decimal ValorArrecadado { get; set; }
    public CampanhaStatus Status { get; set; } = CampanhaStatus.Ativa;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; set; }

    public ICollection<Doacao> Doacoes { get; set; } = new List<Doacao>();
}
