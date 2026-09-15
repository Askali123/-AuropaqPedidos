using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-006, 05-api.md §11.3. Sin validación de unicidad de Nit ni de Nombre: 04-base-datos.md
// §5.1 deja la unicidad de Nit pendiente de confirmación del negocio (no se inventa aquí, mismo
// criterio ya aplicado en EmpresaConfiguration) y ninguna regla documentada exige Nombre único.
public sealed class CrearEmpresaUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearEmpresaUseCase(IEmpresaRepository empresas, IGeneradorDeIdentificadores ids)
    {
        _empresas = empresas;
        _ids = ids;
    }

    public EmpresaResponse Ejecutar(CrearEmpresaRequest request)
    {
        var empresa = new Empresa(_ids.Siguiente(), request.Nombre, request.Nit);

        _empresas.Guardar(empresa);

        return new EmpresaResponse(empresa.Id, empresa.Nombre, empresa.Activo);
    }
}
