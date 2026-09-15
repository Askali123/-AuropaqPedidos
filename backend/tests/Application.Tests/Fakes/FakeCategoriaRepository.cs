using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests.Fakes;

internal sealed class FakeCategoriaRepository : ICategoriaRepository
{
    private readonly Dictionary<int, Categoria> _categorias = new();

    public void Agregar(Categoria categoria) => _categorias[categoria.Id] = categoria;

    public Categoria? ObtenerPorId(int id) => _categorias.TryGetValue(id, out var categoria) ? categoria : null;

    public IReadOnlyList<Categoria> ObtenerTodas() => _categorias.Values.OrderBy(c => c.Nombre).ToList();

    public void Guardar(Categoria categoria) => _categorias[categoria.Id] = categoria;
}
