using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-009, 04-base-datos.md §8. Usuario/Sede inexistentes -> 404 (mismo criterio que
// ListarSedesPorEmpresaUseCase); relación duplicada -> 422 (mismo criterio que CrearPeriodoUseCase
// para Año+Mes); usuario y sede de empresas distintas -> 422, lanzado por el propio constructor
// de UsuarioSede (Domain) al comparar los objetos ya cargados.
public sealed class AsignarUsuarioASedeUseCase
{
    private readonly IUsuarioSedeRepository _usuariosSedes;
    private readonly IUsuarioRepository _usuarios;
    private readonly ISedeRepository _sedes;

    public AsignarUsuarioASedeUseCase(IUsuarioSedeRepository usuariosSedes, IUsuarioRepository usuarios, ISedeRepository sedes)
    {
        _usuariosSedes = usuariosSedes;
        _usuarios = usuarios;
        _sedes = sedes;
    }

    public UsuarioSedeResponse Ejecutar(int usuarioId, int sedeId)
    {
        var usuario = _usuarios.ObtenerPorId(usuarioId)
            ?? throw new RecursoNoEncontradoException($"El usuario {usuarioId} no existe.");

        var sede = _sedes.ObtenerPorId(sedeId)
            ?? throw new RecursoNoEncontradoException($"La sede {sedeId} no existe.");

        if (_usuariosSedes.Existe(usuarioId, sedeId))
            throw new ReglaDeNegocioException("El usuario ya tiene acceso a esa sede.");

        var usuarioSede = new UsuarioSede(usuario, sede);

        _usuariosSedes.Guardar(usuarioSede);

        return new UsuarioSedeResponse(usuarioId, sedeId);
    }
}
