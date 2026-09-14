namespace AuropaqPedidos.Api.Common;

// docs/05-api.md §5: toda respuesta exitosa se envuelve en { "data": ... }.
public sealed record ApiResponse<T>(T Data)
{
    public static ApiResponse<T> De(T data) => new(data);
}
