using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

// TASK-056, 04-base-datos.md §33.
public class AuditoriaTests
{
    private static readonly DateTime Fecha = new(2026, 9, 15);

    [Fact]
    public void Crea_un_registro_de_auditoria_valido()
    {
        var auditoria = new Auditoria(1, usuarioId: 5, entidad: "PedidoProveedor", entidadId: 10, accion: "CREAR", fecha: Fecha);

        Assert.Equal(1, auditoria.Id);
        Assert.Equal(5, auditoria.UsuarioId);
        Assert.Equal("PedidoProveedor", auditoria.Entidad);
        Assert.Equal(10, auditoria.EntidadId);
        Assert.Equal("CREAR", auditoria.Accion);
        Assert.Equal(Fecha, auditoria.Fecha);
        Assert.Null(auditoria.DatosAnteriores);
        Assert.Null(auditoria.DatosNuevos);
    }

    // Fase 6-9 todavía no exige autenticación real (ver progreso.md, "punto 8" no extendido a
    // esas fases todavía): el actor puede no conocerse en el momento de auditar.
    [Fact]
    public void Permite_usuarioId_nulo()
    {
        var auditoria = new Auditoria(1, usuarioId: null, entidad: "Entrega", entidadId: 10, accion: "CREAR", fecha: Fecha);

        Assert.Null(auditoria.UsuarioId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void No_permite_entidad_vacia(string? entidad)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new Auditoria(1, usuarioId: 5, entidad: entidad!, entidadId: 10, accion: "CREAR", fecha: Fecha));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void No_permite_accion_vacia(string? accion)
    {
        Assert.Throws<ReglaDeNegocioException>(
            () => new Auditoria(1, usuarioId: 5, entidad: "PedidoProveedor", entidadId: 10, accion: accion!, fecha: Fecha));
    }
}
