using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-018, 05-api.md §29.
public sealed class ObtenerProveedorUseCase
{
    private readonly IProveedorRepository _proveedores;

    public ObtenerProveedorUseCase(IProveedorRepository proveedores)
    {
        _proveedores = proveedores;
    }

    public ProveedorResponse Ejecutar(int id)
    {
        var proveedor = _proveedores.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"El proveedor {id} no existe.");

        return new ProveedorResponse(proveedor.Id, proveedor.Nombre, proveedor.Nit, proveedor.Contacto, proveedor.Telefono, proveedor.Correo, proveedor.Activo);
    }
}
