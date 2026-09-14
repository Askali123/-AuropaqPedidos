namespace AuropaqPedidos.Domain.Exceptions;

public sealed class ReglaDeNegocioException : Exception
{
    public ReglaDeNegocioException(string message) : base(message)
    {
    }
}
