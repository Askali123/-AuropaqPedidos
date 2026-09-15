using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;

namespace AuropaqPedidos.Application.Catalogo;

// TASK-018, 05-api.md §29. Devuelve todos sin filtrar por Activo.
public sealed class ListarProveedoresUseCase
{
    private readonly IProveedorRepository _proveedores;

    public ListarProveedoresUseCase(IProveedorRepository proveedores)
    {
        _proveedores = proveedores;
    }

    public IReadOnlyList<ProveedorResponse> Ejecutar() =>
        _proveedores.ObtenerTodos()
            .Select(p => new ProveedorResponse(p.Id, p.Nombre, p.Nit, p.Contacto, p.Telefono, p.Correo, p.Activo))
            .ToList();
}
