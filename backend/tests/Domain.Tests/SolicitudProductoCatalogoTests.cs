using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Enums;
using AuropaqPedidos.Domain.Exceptions;

namespace Domain.Tests;

public class SolicitudProductoCatalogoTests
{
    private static readonly DateTime FechaSolicitud = new(2026, 9, 1);

    private static Empresa CrearEmpresa() => new(1, "AUROTECH");

    private static Producto CrearProducto() =>
        new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));

    private static SolicitudProductoCatalogo CrearSolicitud() => new(
        id: 1,
        empresa: CrearEmpresa(),
        usuarioId: 10,
        nombreSolicitado: "Desinfectante nuevo",
        fechaSolicitud: FechaSolicitud);

    [Fact]
    public void Crear_solicitud_valida_queda_pendiente_y_conserva_informacion_original()
    {
        var solicitud = CrearSolicitud();

        Assert.Equal(SolicitudProductoCatalogoEstado.Pendiente, solicitud.Estado);
        Assert.Equal("Desinfectante nuevo", solicitud.NombreSolicitado);
        Assert.Null(solicitud.ProductoResultante);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void No_permite_crear_solicitud_sin_nombre_solicitado(string? nombreInvalido)
    {
        Assert.Throws<ReglaDeNegocioException>(() => new SolicitudProductoCatalogo(
            1, CrearEmpresa(), 10, nombreInvalido!, FechaSolicitud));
    }

    [Fact]
    public void No_permite_crear_solicitud_sin_empresa()
    {
        Assert.Throws<ReglaDeNegocioException>(() => new SolicitudProductoCatalogo(
            1, null!, 10, "Desinfectante nuevo", FechaSolicitud));
    }

    [Fact]
    public void Homologar_conserva_informacion_original_y_cambia_estado()
    {
        var solicitud = CrearSolicitud();
        var producto = CrearProducto();

        solicitud.Homologar(producto, usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(1));

        Assert.Equal(SolicitudProductoCatalogoEstado.Homologado, solicitud.Estado);
        Assert.Equal(producto, solicitud.ProductoResultante);
        Assert.Equal("Desinfectante nuevo", solicitud.NombreSolicitado); // RN-026: info original se conserva
    }

    [Fact]
    public void Crear_asigna_producto_nuevo_y_cambia_estado()
    {
        var solicitud = CrearSolicitud();
        var producto = CrearProducto();

        solicitud.Crear(producto, usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(1));

        Assert.Equal(SolicitudProductoCatalogoEstado.Creado, solicitud.Estado);
        Assert.Equal(producto, solicitud.ProductoResultante);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Rechazar_requiere_motivo(string? motivoInvalido)
    {
        var solicitud = CrearSolicitud();

        Assert.Throws<ReglaDeNegocioException>(() =>
            solicitud.Rechazar(usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(1), motivo: motivoInvalido!));
    }

    [Fact]
    public void Rechazar_con_motivo_cambia_estado()
    {
        var solicitud = CrearSolicitud();

        solicitud.Rechazar(usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(1), motivo: "Producto duplicado");

        Assert.Equal(SolicitudProductoCatalogoEstado.Rechazado, solicitud.Estado);
        Assert.Equal("Producto duplicado", solicitud.MotivoResolucion);
    }

    [Fact]
    public void No_permite_resolver_una_solicitud_ya_resuelta()
    {
        var solicitud = CrearSolicitud();
        solicitud.Rechazar(usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(1), motivo: "Producto duplicado");

        Assert.Throws<ReglaDeNegocioException>(() =>
            solicitud.Homologar(CrearProducto(), usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(2)));
    }

    [Fact]
    public void No_permite_fecha_de_resolucion_anterior_a_la_solicitud()
    {
        var solicitud = CrearSolicitud();

        Assert.Throws<ReglaDeNegocioException>(() =>
            solicitud.Rechazar(usuarioResolucionId: 5, fechaResolucion: FechaSolicitud.AddDays(-1), motivo: "motivo"));
    }
}
