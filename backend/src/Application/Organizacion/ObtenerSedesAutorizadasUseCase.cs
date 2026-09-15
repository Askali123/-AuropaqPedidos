using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-009: satisface el criterio de aceptación "el sistema debe poder determinar las sedes
// autorizadas de un usuario". Reutiliza SedeResponse (ya existente) en vez de crear un DTO
// nuevo — misma forma que necesita este resultado. Usuario inexistente -> 404 (mismo criterio
// que ListarSedesPorEmpresaUseCase con Empresa).
public sealed class ObtenerSedesAutorizadasUseCase
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IUsuarioSedeRepository _usuariosSedes;

    public ObtenerSedesAutorizadasUseCase(IUsuarioRepository usuarios, IUsuarioSedeRepository usuariosSedes)
    {
        _usuarios = usuarios;
        _usuariosSedes = usuariosSedes;
    }

    public IReadOnlyList<SedeResponse> Ejecutar(int usuarioId)
    {
        _ = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException($"El usuario {usuarioId} no existe.");

        return _usuariosSedes.ObtenerPorUsuario(usuarioId)
            .Select(us => new SedeResponse(
                us.Sede.Id, us.Sede.Nombre, us.Sede.Direccion, us.Sede.Ciudad,
                us.Sede.Departamento, us.Sede.Telefono, us.Sede.Contacto, us.Sede.Activo))
            .ToList();
    }
}
