using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace AuropaqPedidos.Api.ErrorHandling;

// Manejo centralizado de excepciones (docs/05-api.md §45, CLAUDE.md §34/§54): traduce
// excepciones de Application/Domain a respuestas HTTP consistentes. Ningún Controller debe
// necesitar su propio try/catch para esto.
public sealed class ExcepcionesDeNegocioHandler : IExceptionHandler
{
    private readonly ILogger<ExcepcionesDeNegocioHandler> _logger;

    public ExcepcionesDeNegocioHandler(ILogger<ExcepcionesDeNegocioHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, code, mensaje) = exception switch
        {
            RecursoNoEncontradoException => (StatusCodes.Status404NotFound, "RECURSO_NO_ENCONTRADO", exception.Message),
            ReglaDeNegocioException => (StatusCodes.Status422UnprocessableEntity, "REGLA_DE_NEGOCIO_VIOLADA", exception.Message),
            CredencialesInvalidasException => (StatusCodes.Status401Unauthorized, "CREDENCIALES_INVALIDAS", exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "ERROR_INTERNO", "No fue posible completar la operación.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado procesando {Metodo} {Ruta}", httpContext.Request.Method, httpContext.Request.Path);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(new ErrorDetail(code, mensaje, Array.Empty<string>())),
            cancellationToken);

        return true;
    }
}
