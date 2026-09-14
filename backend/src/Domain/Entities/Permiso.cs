using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-011, 04-base-datos.md §9.2: representa una acción autorizada. GLOBAL (sin EmpresaId,
// mismo criterio ya confirmado para Rol) y SIN Activo (a diferencia de Rol: el documento no lo
// lista como campo de Permiso, no se agrega por analogía). Solo campos escalares en el
// constructor (como Empresa/Rol): EF Core puede materializarlo directamente.
public sealed class Permiso
{
    public int Id { get; private set; }
    public string Codigo { get; private set; }
    public string Nombre { get; private set; }
    public string? Descripcion { get; private set; }

    public Permiso(int id, string codigo, string nombre, string? descripcion = null)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ReglaDeNegocioException("El código del permiso es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre del permiso es obligatorio.");

        Id = id;
        Codigo = codigo;
        Nombre = nombre;
        Descripcion = descripcion;
    }
}
