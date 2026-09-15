using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Auditorias.Abstracciones;

public interface IAuditoriaRepository
{
    void Guardar(Auditoria auditoria);
}
