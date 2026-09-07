namespace ConexaoSolidaria.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Cpf { get; set; }
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
