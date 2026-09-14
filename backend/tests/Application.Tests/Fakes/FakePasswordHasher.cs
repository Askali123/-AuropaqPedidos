using AuropaqPedidos.Application.Requisiciones.Abstracciones;

namespace Application.Tests.Fakes;

// TASK-015. No usa un algoritmo real (no hace falta para probar Application) — solo un prefijo
// reconocible, suficiente para verificar que CrearUsuarioUseCase nunca guarda el password en
// texto plano y que LoginUseCase verifica correctamente.
internal sealed class FakePasswordHasher : IPasswordHasher
{
    private const string Prefijo = "hash:";

    public string Hash(string password) => Prefijo + password;

    public bool Verificar(string hash, string password) => hash == Prefijo + password;
}
