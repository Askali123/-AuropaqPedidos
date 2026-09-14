namespace AuropaqPedidos.Application.Excepciones;

// TASK-015 (05-api.md §7.5, HTTP 401). Un único tipo/mensaje genérico para "correo no existe",
// "password incorrecta" y "usuario inactivo" — TASK-015 §15 exige no revelar cuál de los tres
// ocurrió (evita enumeración de cuentas, mismo principio que 06-seguridad.md §23).
public sealed class CredencialesInvalidasException : Exception
{
    public CredencialesInvalidasException(string message) : base(message)
    {
    }
}
