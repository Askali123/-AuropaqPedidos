using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-015. Correo inexistente, password incorrecta y usuario inactivo -> el mismo
// CredencialesInvalidasException (401) con el mismo mensaje genérico: TASK-015 §15 exige no
// revelar cuál de los tres ocurrió (evita enumeración de cuentas). "Usuario.Activo == false ->
// autenticación rechazada" se resuelve por el principio de denegación por defecto ya documentado
// (06-seguridad.md §3.2/§20/§71: "ante una duda de autorización, DENEGAR es preferible a
// PERMITIR POR DEFECTO") — permitir el login de un usuario inactivo necesitaría una regla
// explícita que lo autorice, y no existe ninguna.
public sealed class LoginUseCase
{
    private const string MensajeCredencialesInvalidas = "Correo o contraseña incorrectos.";

    private readonly IUsuarioRepository _usuarios;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IGeneradorDeToken _generadorDeToken;

    public LoginUseCase(IUsuarioRepository usuarios, IPasswordHasher passwordHasher, IGeneradorDeToken generadorDeToken)
    {
        _usuarios = usuarios;
        _passwordHasher = passwordHasher;
        _generadorDeToken = generadorDeToken;
    }

    public LoginResponse Ejecutar(LoginRequest request, DateTime fecha)
    {
        var usuario = _usuarios.ObtenerPorCorreo(request.Correo);

        if (usuario is null || !usuario.Activo || !_passwordHasher.Verificar(usuario.PasswordHash, request.Password))
            throw new CredencialesInvalidasException(MensajeCredencialesInvalidas);

        var token = _generadorDeToken.Generar(usuario, fecha);

        var usuarioResponse = new UsuarioResponse(
            usuario.Id,
            usuario.Empresa.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Correo,
            usuario.Activo,
            usuario.FechaCreacion,
            usuario.FechaActualizacion);

        return new LoginResponse(token.Token, token.FechaExpiracion, usuarioResponse);
    }
}
