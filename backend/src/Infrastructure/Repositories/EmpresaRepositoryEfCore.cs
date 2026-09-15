using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class EmpresaRepositoryEfCore : IEmpresaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public EmpresaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Empresa? ObtenerPorId(int id) =>
        _contexto.Empresas.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Empresa> ObtenerTodas() =>
        _contexto.Empresas.OrderBy(e => e.Nombre).ToList();

    // Mismo patrón que UsuarioRepositoryEfCore.Guardar: una Empresa recién creada con `new` nunca
    // fue rastreada (Detached) y debe agregarse; una ya cargada por ObtenerPorId ya está Tracked,
    // así que solo hace falta SaveChanges para persistir la mutación.
    public void Guardar(Empresa empresa)
    {
        if (_contexto.Entry(empresa).State == EntityState.Detached)
            _contexto.Empresas.Add(empresa);

        _contexto.SaveChanges();
    }
}
