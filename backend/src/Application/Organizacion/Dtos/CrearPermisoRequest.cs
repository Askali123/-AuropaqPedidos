namespace AuropaqPedidos.Application.Organizacion.Dtos;

public sealed record CrearPermisoRequest(string Codigo, string Nombre, string? Descripcion = null);
