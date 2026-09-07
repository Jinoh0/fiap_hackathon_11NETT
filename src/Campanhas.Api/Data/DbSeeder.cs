using Campanhas.Api.Data;
using ConexaoSolidaria.Domain.Entities;
using ConexaoSolidaria.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Campanhas.Api.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Usuarios.AnyAsync(u => u.Role == UserRoles.GestorOng))
        {
            db.Usuarios.Add(new Usuario
            {
                NomeCompleto = "Gestor Esperança Solidária",
                Email = "gestor@esperanca.org",
                Cpf = null,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword("Gestor@123"),
                Role = UserRoles.GestorOng
            });
            await db.SaveChangesAsync();
        }
    }
}
