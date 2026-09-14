namespace AuropaqPedidos.Api.Common;

// SOLO MIENTRAS NO EXISTA AUTENTICACIÓN REAL (TASK-048/TASK-049, pendiente, decisión explícita
// del usuario 2026-09-10): usuarioId/empresaId se leen de headers HTTP en lugar de la identidad
// autenticada. docs/06-seguridad.md §5/§6 exige que estos valores provengan de la identidad
// autenticada, no del cliente — esto es un placeholder temporal, no la solución definitiva.
// Cuando exista JWT/autenticación real, este es el único punto que debe cambiar en la Api.
internal static class IdentidadTemporal
{
    private const string HeaderUsuarioId = "X-Usuario-Id";
    private const string HeaderEmpresaId = "X-Empresa-Id";

    public static bool TryObtenerUsuarioId(HttpRequest request, out int usuarioId) =>
        TryObtenerHeaderEntero(request, HeaderUsuarioId, out usuarioId);

    public static bool TryObtenerEmpresaId(HttpRequest request, out int empresaId) =>
        TryObtenerHeaderEntero(request, HeaderEmpresaId, out empresaId);

    private static bool TryObtenerHeaderEntero(HttpRequest request, string nombreHeader, out int valor)
    {
        valor = 0;
        return request.Headers.TryGetValue(nombreHeader, out var valores)
            && int.TryParse(valores.ToString(), out valor)
            && valor > 0;
    }
}
