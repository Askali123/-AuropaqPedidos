using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-018, 05-api.md §29. Sin validación de unicidad de Nit: 04-base-datos.md §14 no la
// documenta (mismo criterio ya aplicado a Empresa.Nit).
public sealed class CrearProveedorUseCase
{
    private readonly IProveedorRepository _proveedores;
    private readonly IGeneradorDeIdentificadores _ids;

    public CrearProveedorUseCase(IProveedorRepository proveedores, IGeneradorDeIdentificadores ids)
    {
        _proveedores = proveedores;
        _ids = ids;
    }

    public ProveedorResponse Ejecutar(CrearProveedorRequest request)
    {
        var proveedor = new Proveedor(_ids.Siguiente(), request.Nombre, request.Nit, request.Contacto, request.Telefono, request.Correo);

        _proveedores.Guardar(proveedor);

        return new ProveedorResponse(proveedor.Id, proveedor.Nombre, proveedor.Nit, proveedor.Contacto, proveedor.Telefono, proveedor.Correo, proveedor.Activo);
    }
}
