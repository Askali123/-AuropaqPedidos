using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas;
using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class AgregarDetalleFacturaUseCaseTests
{
    private static readonly DateTime Fecha = new(2026, 9, 2);

    // Cada instancia construye un grafo Empresa→Periodo→Producto→Requisicion(Aprobada)→
    // Consolidacion→PedidoProveedor (con un único DetallePedidoProveedor) + Proveedor, todos con
    // Ids desplazados por "numero" para poder combinar dos escenarios en una misma prueba.
    private sealed class Escenario
    {
        public FakeFacturaRepository Facturas { get; } = new();
        public FakeProveedorRepository Proveedores { get; } = new();
        public FakePedidoProveedorRepository Pedidos { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();

        public Proveedor Proveedor { get; }
        public PedidoProveedor Pedido { get; }
        public DetallePedidoProveedor DetallePedido { get; }

        // Cada Escenario tiene sus propios repositorios fake (independientes entre instancias),
        // así que reutilizar el mismo "numero" como Id entre dos Escenario distintos no genera
        // colisión: solo importa que los Ids sean únicos DENTRO de cada Escenario.
        public Escenario(int numero = 1, int cantidadPedida = 100)
        {
            var periodo = new Periodo(
                numero, 2026, 9,
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
                new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
                "ABIERTO");
            var empresa = new Empresa(numero, $"Empresa {numero}");
            var producto = new Producto(numero, $"Producto {numero}", new Categoria(numero, "Aseo"), new UnidadMedida(numero, "UNIDAD", "Unidad"));
            var requisicion = new Requisicion(numero, empresa, periodo, 10, Fecha);
            var detalleReq = requisicion.AgregarDetalle(numero, producto, cantidadPedida);
            var sede = new Sede(numero, empresa, $"Sede {numero}");
            requisicion.AgregarDistribucion(numero, detalleReq, sede, cantidadPedida);
            requisicion.Enviar(1, Fecha);
            requisicion.IniciarRevision(1, Fecha);
            requisicion.Aprobar(1, Fecha);

            var consolidacion = new Consolidacion(numero, periodo, usuarioCreacionId: 1, estado: "GENERADA", fechaCreacion: Fecha);
            consolidacion.AgregarAsignacion(numero, numero, requisicion, detalleReq, cantidadPedida);

            Proveedor = new Proveedor(numero, $"Proveedor {numero}");
            Proveedores.Agregar(Proveedor);

            Pedido = new PedidoProveedor(numero, consolidacion, Proveedor, $"PO-{numero}", Fecha);
            DetallePedido = Pedido.AgregarDetalle(numero, consolidacion.Detalles[0], cantidadPedida);
            Pedidos.Guardar(Pedido);
        }

        public RegistrarFacturaUseCase RegistrarUseCase() => new(Facturas, Proveedores, Pedidos, Ids);
        public AgregarDetalleFacturaUseCase AgregarDetalleUseCase() => new(Facturas, Pedidos, Ids);
        public AnularFacturaUseCase AnularUseCase() => new(Facturas);

        public FacturaResponse RegistrarFactura(string numeroFactura = "F-001") =>
            RegistrarUseCase().Ejecutar(Fecha, new RegistrarFacturaRequest(Proveedor.Id, Pedido.Id, numeroFactura, Impuestos: 19m));
    }

    [Fact]
    public void Agrega_detalle_calcula_subtotal_de_linea_y_actualiza_totales_de_la_factura()
    {
        var escenario = new Escenario();
        var factura = escenario.RegistrarFactura();

        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(
            factura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 60, PrecioUnitario: 12.5m));

        var detalle = Assert.Single(respuesta.Detalles);
        Assert.Equal(escenario.DetallePedido.Id, detalle.DetallePedidoProveedorId);
        Assert.Equal(60, detalle.CantidadFacturada);
        Assert.Equal(12.5m, detalle.PrecioUnitario);
        Assert.Equal(750m, detalle.Subtotal);
        Assert.Equal(750m, respuesta.Subtotal);
        Assert.Equal(769m, respuesta.Total);
    }

    [Fact]
    public void Factura_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(999, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, 10, 5m)));
    }

    [Fact]
    public void Detalle_de_pedido_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();
        var factura = escenario.RegistrarFactura();

        Assert.Throws<RecursoNoEncontradoException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(factura.Id, new AgregarDetalleFacturaRequest(999, 10, 5m)));
    }

    [Fact]
    public void Detalle_perteneciente_a_otro_pedido_lanza_regla_de_negocio()
    {
        var escenario = new Escenario(numero: 1);
        var otroEscenario = new Escenario(numero: 2);
        var factura = escenario.RegistrarFactura();

        // En producción ambos PedidoProveedor viven en la misma tabla (misma base de datos);
        // se simula aquí registrando el pedido del otro escenario también en el repositorio de
        // "escenario", para que ObtenerDetallePorId pueda resolverlo globalmente.
        escenario.Pedidos.Guardar(otroEscenario.Pedido);

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(
                factura.Id, new AgregarDetalleFacturaRequest(otroEscenario.DetallePedido.Id, 10, 5m)));
    }

    [Fact]
    public void Cantidad_facturada_menor_o_igual_a_cero_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var factura = escenario.RegistrarFactura();

        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(
                factura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 0, PrecioUnitario: 5m)));
    }

    [Fact]
    public void Cantidad_facturada_acumulada_no_puede_superar_la_cantidad_pedida()
    {
        var escenario = new Escenario(cantidadPedida: 100);
        var primeraFactura = escenario.RegistrarFactura("F-001");
        escenario.AgregarDetalleUseCase().Ejecutar(
            primeraFactura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 70, PrecioUnitario: 5m));

        var segundaFactura = escenario.RegistrarFactura("F-002");

        // 70 ya facturados + 40 nuevos = 110 > 100 (CantidadPedida).
        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(
                segundaFactura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 40, PrecioUnitario: 5m)));
    }

    [Fact]
    public void Permite_facturacion_parcial_en_varias_facturas_del_mismo_pedido()
    {
        var escenario = new Escenario(cantidadPedida: 100);
        var primeraFactura = escenario.RegistrarFactura("F-001");
        escenario.AgregarDetalleUseCase().Ejecutar(
            primeraFactura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 60, PrecioUnitario: 5m));

        var segundaFactura = escenario.RegistrarFactura("F-002");
        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(
            segundaFactura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 40, PrecioUnitario: 5m));

        Assert.Equal(40, respuesta.Detalles[0].CantidadFacturada);
    }

    // D-10/RN-048 (cierre documental 2026-09-11): SUM(CantidadFacturada) <= CantidadPedida.
    [Fact]
    public void Permite_que_el_acumulado_facturado_llegue_exactamente_a_la_cantidad_pedida()
    {
        var escenario = new Escenario(cantidadPedida: 100);
        var factura = escenario.RegistrarFactura("F-001");

        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(
            factura.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 100, PrecioUnitario: 5m));

        Assert.Equal(100, respuesta.Detalles[0].CantidadFacturada);
    }

    // D-09/RN-049: NumeroFactura único dentro del proveedor.
    [Fact]
    public void No_permite_dos_facturas_con_el_mismo_numero_para_el_mismo_proveedor()
    {
        var escenario = new Escenario();
        escenario.RegistrarFactura("F-001");

        Assert.Throws<ReglaDeNegocioException>(() => escenario.RegistrarFactura("F-001"));
    }

    // B4 (cierre técnico 2026-09-11): una Factura ANULADA deja de contar para el acumulado de
    // CantidadFacturada — su cantidad queda libre para nuevas facturas.

    [Fact]
    public void Anular_una_factura_libera_su_cantidad_para_una_nueva_factura()
    {
        var escenario = new Escenario(cantidadPedida: 100);
        var facturaValida = escenario.RegistrarFactura("F-001");
        escenario.AgregarDetalleUseCase().Ejecutar(
            facturaValida.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 60, PrecioUnitario: 5m));

        var facturaAAnular = escenario.RegistrarFactura("F-002");
        escenario.AgregarDetalleUseCase().Ejecutar(
            facturaAAnular.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 40, PrecioUnitario: 5m));

        // Ejemplo del enunciado: CantidadPedida=100, válida=60, anulada=40 -> queda libre 40.
        escenario.AnularUseCase().Ejecutar(facturaAAnular.Id);

        var facturaNueva = escenario.RegistrarFactura("F-003");
        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(
            facturaNueva.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 40, PrecioUnitario: 5m));

        Assert.Equal(40, respuesta.Detalles[0].CantidadFacturada);
    }

    [Fact]
    public void No_supera_la_cantidad_pedida_ni_siquiera_despues_de_anular_una_factura_distinta()
    {
        var escenario = new Escenario(cantidadPedida: 100);
        var facturaValida = escenario.RegistrarFactura("F-001");
        escenario.AgregarDetalleUseCase().Ejecutar(
            facturaValida.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 60, PrecioUnitario: 5m));

        var facturaAAnular = escenario.RegistrarFactura("F-002");
        escenario.AgregarDetalleUseCase().Ejecutar(
            facturaAAnular.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 30, PrecioUnitario: 5m));
        escenario.AnularUseCase().Ejecutar(facturaAAnular.Id);

        // Solo quedan libres 40 (100 - 60 válidos); pedir 50 debe seguir rechazándose.
        var facturaNueva = escenario.RegistrarFactura("F-003");
        Assert.Throws<ReglaDeNegocioException>(() =>
            escenario.AgregarDetalleUseCase().Ejecutar(
                facturaNueva.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 50, PrecioUnitario: 5m)));
    }

    [Fact]
    public void Multiples_facturas_y_multiples_anulaciones_dejan_el_acumulado_correcto()
    {
        var escenario = new Escenario(cantidadPedida: 100);

        var factura1 = escenario.RegistrarFactura("F-001");
        escenario.AgregarDetalleUseCase().Ejecutar(factura1.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, 30, 5m));

        var factura2 = escenario.RegistrarFactura("F-002");
        escenario.AgregarDetalleUseCase().Ejecutar(factura2.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, 30, 5m));

        var factura3 = escenario.RegistrarFactura("F-003");
        escenario.AgregarDetalleUseCase().Ejecutar(factura3.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, 30, 5m));

        // Válido: 30+30+30 = 90. Anulamos factura1 y factura3 -> queda válido solo factura2 (30).
        escenario.AnularUseCase().Ejecutar(factura1.Id);
        escenario.AnularUseCase().Ejecutar(factura3.Id);

        // Deben quedar libres 70 (100 - 30 de factura2, la única válida restante).
        var facturaNueva = escenario.RegistrarFactura("F-004");
        var respuesta = escenario.AgregarDetalleUseCase().Ejecutar(
            facturaNueva.Id, new AgregarDetalleFacturaRequest(escenario.DetallePedido.Id, CantidadFacturada: 70, PrecioUnitario: 5m));

        Assert.Equal(70, respuesta.Detalles[0].CantidadFacturada);
    }
}
