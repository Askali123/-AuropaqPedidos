namespace AuropaqPedidos.Application.Organizacion.Dtos;

// TASK-008 + TASK-015 (Password agregado por decisión explícita del usuario: es la única forma
// de que un Usuario pueda autenticarse, ya que no existe ningún otro mecanismo para establecer
// credenciales). Password viaja en texto plano únicamente en este request (HTTPS lo protege en
// tránsito); CrearUsuarioUseCase lo hashea antes de persistir y nunca lo devuelve en ningún DTO.
public sealed record CrearUsuarioRequest(
    int EmpresaId,
    string Nombre,
    string Correo,
    string Password,
    string? Apellido = null);
