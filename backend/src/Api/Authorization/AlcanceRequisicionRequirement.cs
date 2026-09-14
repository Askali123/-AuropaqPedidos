using Microsoft.AspNetCore.Authorization;

namespace AuropaqPedidos.Api.Authorization;

// TASK-050. Sin parámetros: el recurso concreto (la Requisicion del route "id") se obtiene del
// contexto de autorización en tiempo de request (ver AlcanceRequisicionAuthorizationHandler),
// no de un valor fijado al construir la Policy.
public sealed class AlcanceRequisicionRequirement : IAuthorizationRequirement
{
}
