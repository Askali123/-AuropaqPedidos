using AuropaqPedidos.Domain.Entities;

namespace AuropaqPedidos.Application.Requisiciones.Abstracciones;

public interface IUsuarioRepository
{
    Usuario? ObtenerPorId(int id);

    // TASK-015: necesario para LoginUseCase (buscar por credenciales, el correo es el
    // identificador funcional de login). Correo único GLOBAL ya garantiza que exista a lo sumo un
    // resultado.
    Usuario? ObtenerPorCorreo(string correo);

    IReadOnlyList<Usuario> ObtenerTodos();

    // TASK-008: Correo único GLOBAL (decisión confirmada). Se comprueba en Application (mismo
    // criterio ya usado para Periodo Año+Mes y NumeroPedido/NumeroFactura) en vez de depender de
    // capturar la violación del índice único de SQL Server.
    bool ExisteParaCorreo(string correo);

    void Guardar(Usuario usuario);
}
