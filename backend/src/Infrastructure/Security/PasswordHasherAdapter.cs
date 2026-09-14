using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuropaqPedidos.Infrastructure.Security;

// TASK-015. Usa únicamente PasswordHasher<T> (PBKDF2, mecanismo estándar de .NET) — NO se agrega
// ASP.NET Core Identity completo (UserManager/SignInManager/tablas propias): ese paquete no
// define ningún esquema de persistencia, solo expone esta clase de hashing (TASK-015 §16: no
// agregar librerías de Identity completas si no se necesitan). El parámetro de tipo <Usuario> es
// un requisito genérico de la API de PasswordHasher<T>; no lee ninguna propiedad de Usuario.
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<Usuario> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(null!, password);

    public bool Verificar(string hash, string password) =>
        _hasher.VerifyHashedPassword(null!, hash, password) != PasswordVerificationResult.Failed;
}
