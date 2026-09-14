using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AuropaqPedidos.Infrastructure.Repositories;

public sealed class SedeRepositoryEfCore : ISedeRepository
{
    private readonly AuropaqPedidosDbContext _contexto;

    public SedeRepositoryEfCore(AuropaqPedidosDbContext contexto)
    {
        _contexto = contexto;
    }

    // Requisicion.AsegurarSedeDeLaEmpresa compara sede.Empresa con la empresa de la requisición:
    // Empresa debe estar cargada, o la comparación fallaría incorrectamente contra null.
    public Sede? ObtenerPorId(int id) =>
        _contexto.Sedes.Include(s => s.Empresa).FirstOrDefault(s => s.Id == id);

    public IReadOnlyList<Sede> ObtenerPorEmpresa(int empresaId) =>
        _contexto.Sedes
            .Include(s => s.Empresa)
            .Where(s => EF.Property<int>(s, "EmpresaId") == empresaId)
            .OrderBy(s => s.Nombre)
            .ToList();
}
