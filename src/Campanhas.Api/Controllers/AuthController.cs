using System.Security.Claims;
using Campanhas.Api.Auth;
using Campanhas.Api.Contracts;
using Campanhas.Api.Data;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using ConexaoSolidaria.Domain.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AppDbContext db, JwtTokenService jwt) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterDoadorRequest request, CancellationToken ct)
    {
        if (!CpfValidator.IsValid(request.Cpf))
        {
            return BadRequest(new { message = "CPF inválido." });
        }

        var email = request.Email.Trim().ToLowerInvariant();
        if (await db.Usuarios.AnyAsync(u => u.Email == email, ct))
        {
            return Conflict(new { message = "Email já cadastrado." });
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

        var token = jwt.CreateToken(usuario);
        return Created(string.Empty, new AuthResponse(token, usuario.Role, usuario.Id, usuario.NomeCompleto));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
        {
            return Unauthorized(new { message = "Credenciais inválidas." });
        }

        var token = jwt.CreateToken(usuario);
        return Ok(new AuthResponse(token, usuario.Role, usuario.Id, usuario.NomeCompleto));
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        return Ok(new
        {
            id = User.FindFirstValue("uid"),
            email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name),
            role = User.FindFirstValue(ClaimTypes.Role),
            nome = User.FindFirstValue(ClaimTypes.Name)
        });
    }
}
