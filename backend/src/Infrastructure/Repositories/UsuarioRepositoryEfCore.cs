using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class UsuarioRepositoryEfCore : IUsuarioRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public UsuarioRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public Usuario? ObtenerPorId(int id) =>
        Consulta().FirstOrDefault(u => u.Id == id);

    // TASK-015: Correo único GLOBAL (índice UNIQUE) garantiza a lo sumo un resultado.
    public Usuario? ObtenerPorCorreo(string correo) =>
        Consulta().FirstOrDefault(u => u.Correo == correo);

    public IReadOnlyList<Usuario> ObtenerTodos() =>
        Consulta().OrderBy(u => u.Nombre).ToList();

    public bool ExisteParaCorreo(string correo) =>
        _contexto.Usuarios.Any(u => u.Correo == correo);

    // Mismo patrón que RequisicionRepositoryEfCore.Guardar/PeriodoRepositoryEfCore.Guardar: un
    // Usuario recién creado con `new` nunca fue rastreado (Detached) y debe agregarse.
    public void Guardar(Usuario usuario)
    {
        if (_contexto.Entry(usuario).State == EntityState.Detached)
            _contexto.Usuarios.Add(usuario);

        _contexto.SaveChanges();
    }

    private IQueryable<Usuario> Consulta() =>
        _contexto.Usuarios.Include(u => u.Empresa);
}
