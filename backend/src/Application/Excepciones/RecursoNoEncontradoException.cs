namespace AuropaqPedidos.Application.Excepciones;

// Distinta de ReglaDeNegocioException (Domain): esta representa "el recurso no existe"
// (05-api.md §7.7, HTTP 404), no una violación de una regla de negocio (422).
public sealed class RecursoNoEncontradoException : Exception
{
    public RecursoNoEncontradoException(string message) : base(message)
    {
    }
}
