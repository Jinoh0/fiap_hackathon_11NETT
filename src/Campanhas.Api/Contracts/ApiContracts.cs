using System.ComponentModel.DataAnnotations;
using ConexaoSolidaria.Domain.Enums;

namespace Campanhas.Api.Contracts;

public record RegisterDoadorRequest(
    [Required] string NomeCompleto,
    [Required, EmailAddress] string Email,
    [Required] string Cpf,
    [Required, MinLength(6)] string Senha);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Senha);

public record AuthResponse(string Token, string Role, Guid UserId, string NomeCompleto);

public record CreateCampanhaRequest(
    [Required] string Titulo,
    [Required] string Descricao,
    [Required] DateTime DataInicio,
    [Required] DateTime DataFim,
    [Required] decimal MetaFinanceira,
    CampanhaStatus? Status);

public record UpdateCampanhaRequest(
    [Required] string Titulo,
    [Required] string Descricao,
    [Required] DateTime DataInicio,
    [Required] DateTime DataFim,
    [Required] decimal MetaFinanceira,
    [Required] CampanhaStatus Status);

public record CampanhaResponse(
    Guid Id,
    string Titulo,
    string Descricao,
    DateTime DataInicio,
    DateTime DataFim,
    decimal MetaFinanceira,
    decimal ValorArrecadado,
    string Status);

public record CampanhaPublicaResponse(
    Guid Id,
    string Titulo,
    decimal MetaFinanceira,
    decimal ValorArrecadado);

public record CreateDoacaoRequest(
    [Required] Guid IdCampanha,
    [Required] decimal ValorDoacao);

public record DoacaoResponse(
    Guid Id,
    Guid CampanhaId,
    decimal ValorDoacao,
    string Status,
    DateTime CriadoEm);
