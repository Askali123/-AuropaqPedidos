using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Organizacion.Dtos;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Application.Organizacion;

// TASK-007, 05-api.md §12.2: "El backend debe verificar que la empresa exista y esté habilitada"
// -> empresa inexistente = 404 (mismo criterio que ListarSedesPorEmpresaUseCase); empresa inactiva
// = 422 (mismo patrón ya usado para Sede/Producto/Proveedor inactivos en otros casos de uso, p.ej.
// AgregarDistribucionRequisicionUseCase).
public sealed class CrearSedeUseCase
{
    private readonly IEmpresaRepository _empresas;
    private readonly ISedeRepository _sedes;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearSedeUseCase(IEmpresaRepository empresas, ISedeRepository sedes, IGeneradorDeIdentificadores ids)
    {
        _empresas = empresas;
        _sedes = sedes;
        _ids = ids;
    }

    public SedeResponse Ejecutar(int empresaId, CrearSedeRequest request)
    {
        var empresa = _empresas.ObtenerPorId(empresaId)
            ?? throw new RecursoNoEncontradoException($"La empresa {empresaId} no existe.");

        if (!empresa.Activo)
            throw new ReglaDeNegocioException("La empresa no está activa.");

        var sede = new Sede(
            _ids.Siguiente(),
            empresa,
            request.Nombre,
            request.Direccion,
            request.Ciudad,
            request.Departamento,
            request.Telefono,
            request.Contacto);

        _sedes.Guardar(sede);

        return new SedeResponse(sede.Id, sede.Nombre, sede.Direccion, sede.Ciudad, sede.Departamento, sede.Telefono, sede.Contacto, sede.Activo);
    }
}
