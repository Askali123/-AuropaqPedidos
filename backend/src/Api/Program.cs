using System.Text;
using AuropaqPedidos.Api.Authorization;
using AuropaqPedidos.Api.Common;
using AuropaqPedidos.Api.ErrorHandling;
using AuropaqPedidos.Application;
using AuropaqPedidos.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

const string FrontendDevCorsPolicy = "FrontendDev";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

// TASK-015: valida los JWT emitidos por AuthController/JwtTokenGenerator.
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException(
        "Falta la configuración 'Jwt:Key'. Configúrala mediante 'dotnet user-secrets' en " +
        "desarrollo, o mediante el mecanismo seguro de secretos del ambiente correspondiente.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // TASK-016: sin esto, ASP.NET Core remapea claims JWT cortos ("sub", "email") a URIs
        // largas heredadas de WS-Federation (comportamiento por defecto de
        // JwtSecurityTokenHandler) — PermisoAuthorizationHandler necesita leer "sub" tal como
        // JwtTokenGenerator lo emitió. Único cambio a la configuración de TASK-015 necesario
        // para autorización (TASK-016 §6/§22); no toca login/hashing/generación de JWT.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "AuropaqPedidos.Api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "AuropaqPedidos.Frontend",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
    });

// TASK-016: IAuthorizationPolicyProvider personalizado (PermisoPolicyProvider) reemplaza al
// predeterminado para resolver dinámicamente cualquier policy con prefijo "Permiso:" — evita
// registrar una Policy estática por cada Permiso.Codigo (§7). El proveedor personalizado delega
// en el predeterminado (DefaultAuthorizationPolicyProvider) para cualquier nombre de policy que
// no tenga ese prefijo — por eso la Policy nombrada "AlcanceRequisicion" (TASK-050, registrada
// abajo con AddPolicy) sigue resolviéndose con normalidad.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermisoPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermisoAuthorizationHandler>();

// TASK-050: alcance por empresa (06-seguridad.md §11) para Requisicion. Policy estática única
// (no dinámica como los permisos, porque no varía por parámetro) — se apila junto al
// "Permiso:..." correspondiente mediante dos atributos [Authorize] en el mismo endpoint (ambos
// deben cumplirse, semántica AND nativa de ASP.NET Core).
builder.Services.AddScoped<IAuthorizationHandler, AlcanceRequisicionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AlcanceRequisicion", policy =>
        policy.RequireAuthenticatedUser().AddRequirements(new AlcanceRequisicionRequirement()));

    // P1-5 (docs/2026-09-17-tareas.md): seguridad por defecto (CLAUDE.md §67 — ante duda,
    // denegar). Cualquier endpoint nuevo que se agregue sin [Authorize] ni [AllowAnonymous]
    // explícito queda protegido automáticamente (exige JWT válido) en vez de quedar alcanzable
    // sin autenticación por omisión — el mismo hueco que tuvieron Pedido/Entrega/Factura hasta
    // el 2026-09-17 (P1, RN-063/ADR-066). Los endpoints que sí deben ser públicos (hoy solo
    // AuthController.Login) necesitan [AllowAnonymous] explícito.
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Incremento MVP frontend ↔ backend: el frontend de desarrollo (Vite, http://localhost:5173)
// y la Api (http://localhost:5175) son orígenes distintos para el navegador. Política mínima,
// con un único origen explícito (06-seguridad.md §34 prohíbe AllowAnyOrigin) y registrada
// solo en Development — nunca se habilita en producción.
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendDevCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// docs/05-api.md §6: mismo formato de error también para fallos de validación de modelo
// (400), no solo para excepciones de Application/Domain.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var detalles = context.ModelState
            .Where(par => par.Value?.Errors.Count > 0)
            .SelectMany(par => par.Value!.Errors.Select(e => $"{par.Key}: {e.ErrorMessage}"))
            .ToList();

        return new BadRequestObjectResult(
            new ErrorResponse(new ErrorDetail("SOLICITUD_INVALIDA", "La solicitud no es válida.", detalles)));
    };
});

builder.Services.AddExceptionHandler<ExcepcionesDeNegocioHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var app = builder.Build();

// docs/05-api.md §45/§6: manejo centralizado de excepciones, ningún Controller usa try/catch.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors(FrontendDevCorsPolicy);
}

// TASK-015/016: debe ir después de UseCors y antes de MapControllers (orden exigido por
// ASP.NET Core). TASK-016 agrega los primeros dos endpoints con [Authorize(Policy = "Permiso:...")]
// (RequisicionesController.Enviar/Aprobar) — ver comentario junto a AddAuthentication arriba.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Necesario para que WebApplicationFactory<Program> (tests de integración) pueda referenciar
// este ensamblado de nivel superior.
public partial class Program
{
}
