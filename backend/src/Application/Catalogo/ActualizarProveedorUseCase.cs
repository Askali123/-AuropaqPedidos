using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-018, 05-api.md §29. Activo se aplica con Activar()/Desactivar() (mismo criterio que
// ActualizarEmpresaUseCase).
public sealed class ActualizarProveedorUseCase
{
    private readonly IProveedorRepository _proveedores;

    public ActualizarProveedorUseCase(IProveedorRepository proveedores)
    {
        _proveedores = proveedores;
    }

    public ProveedorResponse Ejecutar(int id, ActualizarProveedorRequest request)
    {
        var proveedor = _proveedores.ObtenerPorId(id)
            ?? throw new RecursoNoEncontradoException($"El proveedor {id} no existe.");

        proveedor.ActualizarDatos(request.Nombre, request.Nit, request.Contacto, request.Telefono, request.Correo);

        if (request.Activo)
            proveedor.Activar();
        else
            proveedor.Desactivar();

        _proveedores.Guardar(proveedor);

        return new ProveedorResponse(proveedor.Id, proveedor.Nombre, proveedor.Nit, proveedor.Contacto, proveedor.Telefono, proveedor.Correo, proveedor.Activo);
    }
}
