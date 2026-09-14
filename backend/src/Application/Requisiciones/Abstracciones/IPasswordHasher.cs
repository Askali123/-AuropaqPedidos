namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

// TASK-015. El algoritmo concreto (estándar de .NET, no propio) es un detalle de Infrastructure
// — Application/Domain solo conocen esta abstracción, nunca la contraseña en texto plano más
// allá de este único punto de entrada.
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verificar(string hash, string password);
}
