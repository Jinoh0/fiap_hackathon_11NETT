using Campanhas.Api.Auth;
using Campanhas.Api.Contracts;
using Campanhas.Api.Data;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Validation;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Services;

public class AuthService(AppDbContext db, JwtTokenService jwt)
{
    public async Task<Result<AuthResponse>> RegisterAsync(RegisterDoadorRequest request, CancellationToken ct)
    {
        if (!CpfValidator.IsValid(request.Cpf))
        {
            return Result<AuthResponse>.Fail(400, "CPF inválido.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == email, ct))
        {
            return Result<AuthResponse>.Fail(409, "Email já cadastrado.");
        }

        var usuario = new Usuario
        {
            NomeCompleto = request.NomeCompleto.Trim(),
            Email = email,
            Cpf = CpfValidator.Normalize(request.Cpf),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Role = UserRoles.Doador
        };

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync(ct);

        return Result<AuthResponse>.Created(ToResponse(usuario));
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return Result<AuthResponse>.Fail(401, "Credenciais inválidas.");
        }

        return Result<AuthResponse>.Ok(ToResponse(usuario));
    }

    private AuthResponse ToResponse(Usuario usuario) =>
        new(jwt.CreateToken(usuario), usuario.Role, usuario.Id, usuario.NomeCompleto);
}
