using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-008 + TASK-015. Empresa inexistente -> 404 (mismo criterio que
// ListarSedesPorEmpresaUseCase); Correo duplicado -> 422 (mismo criterio que CrearPeriodoUseCase
// para Año+Mes: se comprueba aquí en vez de depender de capturar la violación del índice único
// de SQL Server). Password: mínimo 8 caracteres (decisión de negocio explícita del usuario,
// TASK-015) -> 422; se hashea con IPasswordHasher antes de construir Usuario — el texto plano
// nunca llega a Domain ni se persiste.
public sealed class CrearUsuarioUseCase
{
    private const int LongitudMinimaPassword = 8;

    private readonly IUsuarioRepository _usuarios;
    private readonly IEmpresaRepository _empresas;
    private readonly IGeneradorDeIdentificadores _ids;
    private readonly IPasswordHasher _passwordHasher;

    public CrearUsuarioUseCase(
        IUsuarioRepository usuarios,
        IEmpresaRepository empresas,
        IGeneradorDeIdentificadores ids,
        IPasswordHasher passwordHasher)
    {
        _usuarios = usuarios;
        _empresas = empresas;
        _ids = ids;
        _passwordHasher = passwordHasher;
    }

    public UsuarioResponse Ejecutar(CrearUsuarioRequest request, DateTime fecha)
    {
        var empresa = _empresas.ObtenerPorId(request.EmpresaId)
            ?? throw new RecursoNoEncontradoException($"La empresa {request.EmpresaId} no existe.");

        // Correo único GLOBAL (decisión confirmada explícitamente, no por empresa).
        if (_usuarios.ExisteParaCorreo(request.Correo))
            throw new ReglaDeNegocioException("Ya existe un usuario con ese correo.");

        if (request.Password.Length < LongitudMinimaPassword)
            throw new ReglaDeNegocioException($"El password debe tener al menos {LongitudMinimaPassword} caracteres.");

        var passwordHash = _passwordHasher.Hash(request.Password);

        var usuario = new Usuario(_ids.Siguiente(), empresa, request.Nombre, request.Correo, passwordHash, fecha, request.Apellido);

        _usuarios.Guardar(usuario);

        return new UsuarioResponse(
            usuario.Id,
            usuario.Empresa.Id,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Correo,
            usuario.Activo,
            usuario.FechaCreacion,
            usuario.FechaActualizacion);
    }
}
