using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace AuropaqPedidos.Application.Organizacion;

// Habilita el selector de empresa del Frontend de Requisiciones (docs/05-api.md §54.6). Devuelve
// todas las empresas sin filtrar por Activo: no hay una regla de negocio que defina que una
// empresa inactiva deba ocultarse de la consulta (instrucción explícita: no inventar filtros).
public sealed class ListarEmpresasUseCase
{
    private readonly IEmpresaRepository _empresas;

    public ListarEmpresasUseCase(IEmpresaRepository empresas)
    {
        _empresas = empresas;
    }

    public IReadOnlyList<EmpresaResponse> Ejecutar() =>
        _empresas.ObtenerTodas()
            .Select(e => new EmpresaResponse(e.Id, e.Nombre, e.Activo))
            .ToList();
}
