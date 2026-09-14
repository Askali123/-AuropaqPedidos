using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// Habilita el selector de sede del Frontend de Requisiciones (docs/05-api.md §54.6). Mismo
// patrón que el resto de Application para validar existencia del recurso padre (p.ej.
// RequisicionFinder/EntregaFinder): si la empresa no existe, 404 en vez de una lista vacía.
public sealed class ListarSedesPorEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly ISedeRepository _sedes;

    public ListarSedesPorEmpresaUseCase(IEmpresaRepository empresas, ISedeRepository sedes)
    {
        _empresas = empresas;
        _sedes = sedes;
    }

    public IReadOnlyList<SedeResponse> Ejecutar(int empresaId)
    {
        _ = _empresas.ObtenerPorId(empresaId)
            ?? throw new RecursoNoEncontradoException($"La empresa {empresaId} no existe.");

        return _sedes.ObtenerPorEmpresa(empresaId)
            .Select(s => new SedeResponse(s.Id, s.Nombre, s.Activo))
            .ToList();
    }
}
