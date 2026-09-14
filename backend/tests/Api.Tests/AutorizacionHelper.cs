using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

// TASK-016/TASK-050. Crea (de forma idempotente: puede llamarse varias veces con el mismo
// "numero", por ejemplo cuando un mismo test dispara varios envíos/aprobaciones) un Usuario de
// la Empresa indicada, con un Rol que tiene los Permiso.Codigo indicados, y emite un JWT real
// para él usando el mismo IGeneradorDeToken registrado en el host de pruebas (mismo Jwt:Key que
// valida el middleware) — sin pasar por POST /api/v1/auth/login (ya cubierto por
// AuthFlujoTests), directo vía DbContext, igual que el resto de los fixtures de Api.Tests
// (Escenario.cs).
//
// "empresaId" (TASK-050): el Usuario creado pertenece a esa Empresa — necesario porque la
// autorización ahora también verifica alcance (Usuario.Empresa == Requisicion.Empresa); antes de
// esto el helper creaba su propia Empresa nueva, sin relación con la de la Requisicion bajo
// prueba, y toda prueba "con permiso" habría empezado a fallar por alcance.
//
// "numero" identifica de forma exclusiva a este Usuario/Rol dentro de la clase de prueba que
// llama (mismo criterio que Escenario.CrearAsync): evita dos problemas reales encontrados al
// implementar esto —
//   1. Colisión de PK: un "numero" fijo o un generador compartido (IGeneradorDeIdentificadores)
//      puede producir el mismo Id que Escenario.CrearAsync(db, numero) usa para su propia
//      Empresa/Usuario en la misma base de datos de pruebas.
//   2. Fuga de permisos entre tests: si dos pruebas de la misma clase reutilizaran el mismo
//      Usuario/Rol (p. ej. por un correo fijo), un permiso otorgado en una prueba "con permiso"
//      seguía presente al ejecutar después una prueba "sin permiso" — los permisos nunca se
//      revocan, solo se agregan.
// Usar un "numero" distinto por caso de prueba (igual que ya hace Escenario.CrearAsync) evita
// ambos problemas sin necesitar limpiar datos entre pruebas.
internal static class AutorizacionHelper
{
    public static async Task<string> CrearTokenConPermisosAsync(
        ApiWebApplicationFactory factory, AuropaqPedidosDbContext db, int numero, int empresaId,
        params string[] codigosPermiso)
    {
        var baseId = 600_000 + (numero * 100);

        var usuario = await db.Usuarios.Include(u => u.Empresa).FirstOrDefaultAsync(u => u.Id == baseId + 1);
        if (usuario is null)
        {
            var empresa = await db.Empresas.FirstAsync(e => e.Id == empresaId);
            usuario = new Usuario(
                baseId + 1, empresa, "Autorizacion", $"autorizacion.tests.{numero}@auropaq.com",
                "hash-no-usado-en-estos-tests", DateTime.UtcNow);
            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }

        var rol = await db.Roles.FirstOrDefaultAsync(r => r.Id == baseId + 2);
        if (rol is null)
        {
            rol = new Rol(baseId + 2, $"RolAutorizacionTests{numero}");
            db.Roles.Add(rol);
            await db.SaveChangesAsync();

            db.UsuariosRoles.Add(new UsuarioRol(usuario, rol));
            await db.SaveChangesAsync();
        }

        var siguienteIdPermiso = baseId + 3;
        foreach (var codigo in codigosPermiso)
        {
            var permiso = await db.Permisos.FirstOrDefaultAsync(p => p.Codigo == codigo);
            if (permiso is null)
            {
                permiso = new Permiso(siguienteIdPermiso, codigo, codigo);
                db.Permisos.Add(permiso);
                await db.SaveChangesAsync();
            }

            siguienteIdPermiso++;

            var yaTienePermiso = await db.RolesPermisos.AnyAsync(rp =>
                EF.Property<int>(rp, "RolId") == rol.Id && EF.Property<int>(rp, "PermisoId") == permiso.Id);
            if (!yaTienePermiso)
            {
                db.RolesPermisos.Add(new RolPermiso(rol, permiso));
                await db.SaveChangesAsync();
            }
        }

        using var scope = factory.Services.CreateScope();
        var generadorDeToken = scope.ServiceProvider.GetRequiredService<IGeneradorDeToken>();
        return generadorDeToken.Generar(usuario, DateTime.UtcNow).Token;
    }
}
