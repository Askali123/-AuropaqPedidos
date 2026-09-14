using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeUsuarioRepository : IUsuarioRepository
{
    private readonly Dictionary<int, Usuario> _usuarios = new();

    public void Agregar(Usuario usuario) => _usuarios[usuario.Id] = usuario;

    public Usuario? ObtenerPorId(int id) => _usuarios.TryGetValue(id, out var usuario) ? usuario : null;

    public Usuario? ObtenerPorCorreo(string correo) => _usuarios.Values.FirstOrDefault(u => u.Correo == correo);

    public IReadOnlyList<Usuario> ObtenerTodos() => _usuarios.Values.OrderBy(u => u.Nombre).ToList();

    public bool ExisteParaCorreo(string correo) =>
        _usuarios.Values.Any(u => u.Correo == correo);

    public void Guardar(Usuario usuario) => _usuarios[usuario.Id] = usuario;
}
