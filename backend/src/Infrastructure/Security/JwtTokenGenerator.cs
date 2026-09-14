using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuropaqPedidos.Infrastructure.Security;

// TASK-015. Issuer/Audience/ExpiracionMinutos: configuración externa con default documentado
// (TASK-015 §10). Jwt:Key: sin default — falla explícitamente si falta (nunca un secreto
// hardcodeado, 06-seguridad.md §27/§51). Claims: únicamente sub/email/name/empresaId
// (06-seguridad.md §5: "como mínimo UsuarioId; cuando corresponda EmpresaId") — sin
// roles/permisos (TASK-015 §11/§23, autorización es una tarea posterior).
public sealed class JwtTokenGenerator : IGeneradorDeToken
{
    private readonly string _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiracionMinutos;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "Falta la configuración 'Jwt:Key'. Configúrala mediante 'dotnet user-secrets' " +
                "en desarrollo, o mediante el mecanismo seguro de secretos del ambiente " +
                "correspondiente. No debe versionarse un secreto real en appsettings.json.");
        _issuer = configuration["Jwt:Issuer"] ?? "AuropaqPedidos.Api";
        _audience = configuration["Jwt:Audience"] ?? "AuropaqPedidos.Frontend";
        _expiracionMinutos = int.TryParse(configuration["Jwt:ExpiracionMinutos"], out var minutos) ? minutos : 60;
    }

    public TokenGenerado Generar(Usuario usuario, DateTime fecha)
    {
        var expiracion = fecha.AddMinutes(_expiracionMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Correo),
            new Claim(JwtRegisteredClaimNames.Name, ConstruirNombreCompleto(usuario)),
            new Claim("empresaId", usuario.Empresa.Id.ToString()),
        };

        var credenciales = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            notBefore: fecha,
            expires: expiracion,
            signingCredentials: credenciales);

        return new TokenGenerado(new JwtSecurityTokenHandler().WriteToken(token), expiracion);
    }

    private static string ConstruirNombreCompleto(Usuario usuario) =>
        usuario.Apellido is null ? usuario.Nombre : $"{usuario.Nombre} {usuario.Apellido}";
}
