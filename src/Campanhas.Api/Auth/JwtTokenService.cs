using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConexaoSolidaria.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Campanhas.Api.Auth;

public class JwtTokenService(IConfiguration configuration)
{
    public string CreateToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresMinutes = int.Parse(configuration["Jwt:ExpiresMinutes"] ?? "120");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.NomeCompleto),
            new(ClaimTypes.Role, usuario.Role),
            new("uid", usuario.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
