using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// TASK-009, 04-base-datos.md §8: representa que un Usuario tiene acceso a operar en una Sede
// (Usuario N --- N Sede). Sin Id propio — el documento solo lista UsuarioId/SedeId como campos,
// y ese mismo par es su clave primaria compuesta en Infrastructure; no se agrega un campo no
// documentado.
public sealed class UsuarioSede
{
    public Usuario Usuario { get; private set; }
    public Sede Sede { get; private set; }

    // Solo para EF Core (materialización desde la base de datos). No ejecuta ninguna
    // regla de negocio; los campos se asignan por reflexión después de construir.
#pragma warning disable CS8618
    private UsuarioSede()
    {
    }
#pragma warning restore CS8618

    // Regla fundamental de multiempresa (aislamiento entre Empresas, ya definida para esta
    // tarea): un Usuario solo puede tener acceso a Sedes de su propia Empresa. Comparación
    // directa entre objetos ya cargados (no consulta repositorios) — mismo patrón que
    // Requisicion.AsegurarSedeDeLaEmpresa.
    public UsuarioSede(Usuario usuario, Sede sede)
    {
        if (usuario is null)
            throw new ReglaDeNegocioException("Una asignación de sede debe tener un usuario.");

        if (sede is null)
            throw new ReglaDeNegocioException("Una asignación de sede debe tener una sede.");

        if (usuario.Empresa != sede.Empresa)
            throw new ReglaDeNegocioException("El usuario y la sede deben pertenecer a la misma empresa.");

        Usuario = usuario;
        Sede = sede;
    }
}
