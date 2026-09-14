using AuropaqPedidos.Domain.Exceptions;

namespace AuropaqPedidos.Domain.Entities;

// RN-001: las empresas son datos configurables, no deben hardcodearse en el sistema.
public sealed class Empresa
{
    public int Id { get; private set; }
    public string Nombre { get; private set; }
    public string? Nit { get; private set; }
    public bool Activo { get; private set; }

    public Empresa(int id, string nombre, string? nit = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ReglaDeNegocioException("El nombre de la empresa es obligatorio.");

        Id = id;
        Nombre = nombre;
        Nit = nit;
        Activo = true;
    }

    public void Activar() => Activo = true;

    public void Desactivar() => Activo = false;
}
