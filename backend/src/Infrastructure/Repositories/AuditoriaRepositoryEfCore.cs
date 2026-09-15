using AuropaqPedidos.Application.Auditorias.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class AuditoriaRepositoryEfCore : IAuditoriaRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public AuditoriaRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    public void Guardar(Auditoria auditoria)
    {
        if (_contexto.Entry(auditoria).State == EntityState.Detached)
            _contexto.Auditorias.Add(auditoria);

        _contexto.SaveChanges();
    }
}
