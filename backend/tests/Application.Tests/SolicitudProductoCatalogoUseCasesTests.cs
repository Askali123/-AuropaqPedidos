using Application.Tests.Fakes;
using AuropaqPedidos.Application.Catalogo;
using AuropaqPedidos.Application.Catalogo.Dtos;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;

namespace Application.Tests;

public class SolicitudProductoCatalogoUseCasesTests
{
    private static readonly DateTime Fecha = new(2026, 9, 15);

    private sealed class Escenario
    {
        public FakeSolicitudProductoCatalogoRepository Solicitudes { get; } = new();
        public FakeEmpresaRepository Empresas { get; } = new();
        public FakeUsuarioRepository Usuarios { get; } = new();
        public FakeProductoRepository Productos { get; } = new();
        public FakeCategoriaRepository Categorias { get; } = new();
        public FakeUnidadMedidaRepository UnidadesMedida { get; } = new();
        public FakeGeneradorDeIdentificadores Ids { get; } = new();
        public Empresa Empresa { get; } = new(1, "AUROTECH");
        public Usuario Usuario { get; }
        public Categoria Categoria { get; } = new(1, "Papelería");
        public UnidadMedida UnidadMedida { get; } = new(1, "UNIDAD", "Unidad");

        public Escenario()
        {
            Usuario = new Usuario(1, Empresa, "Martha", "martha@auropaq.com", "hash", Fecha);
            Empresas.Agregar(Empresa);
            Usuarios.Agregar(Usuario);
            Categorias.Agregar(Categoria);
            UnidadesMedida.Agregar(UnidadMedida);
        }

        public SolicitarProductoNoCatalogadoUseCase SolicitarUseCase() => new(Solicitudes, Usuarios, Ids);

        public HomologarProductoUseCase HomologarUseCase() => new(Solicitudes, Productos);

        public CrearProductoDesdeSolicitudUseCase CrearProductoUseCase() => new(Solicitudes, Productos, Categorias, UnidadesMedida, Ids);

        public RechazarSolicitudUseCase RechazarUseCase() => new(Solicitudes);

        public SolicitudProductoCatalogo CrearSolicitudPendiente()
        {
            var solicitud = SolicitarUseCase().Ejecutar(
                Usuario.Id, new SolicitarProductoRequest("Producto nuevo", "Descripción"), Fecha);
            return Solicitudes.ObtenerPorId(solicitud.Id)!;
        }
    }

    [Fact]
    public void Solicitar_producto_no_catalogado_queda_pendiente()
    {
        var escenario = new Escenario();

        var respuesta = escenario.SolicitarUseCase().Ejecutar(
            escenario.Usuario.Id, new SolicitarProductoRequest("Producto nuevo"), Fecha);

        Assert.Equal("Pendiente", respuesta.Estado);
        Assert.Equal(escenario.Empresa.Id, respuesta.EmpresaId);
        Assert.Equal(escenario.Usuario.Id, respuesta.UsuarioId);
    }

    [Fact]
    public void Solicitar_con_usuario_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.SolicitarUseCase().Ejecutar(
            999, new SolicitarProductoRequest("Producto nuevo"), Fecha));
    }

    [Fact]
    public void Listar_pendientes_incluye_la_solicitud_creada()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();

        var respuesta = new ListarSolicitudesPendientesUseCase(escenario.Solicitudes).Ejecutar();

        Assert.Contains(respuesta, s => s.Id == solicitud.Id);
    }

    [Fact]
    public void Homologar_asocia_un_producto_existente_y_cambia_el_estado()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();
        var producto = new Producto(1, "Producto existente", escenario.Categoria, escenario.UnidadMedida);
        escenario.Productos.Agregar(producto);

        var respuesta = escenario.HomologarUseCase().Ejecutar(
            solicitud.Id, new HomologarProductoRequest(producto.Id), escenario.Usuario.Id, Fecha);

        Assert.Equal("Homologado", respuesta.Estado);
        Assert.Equal(producto.Id, respuesta.ProductoResultanteId);
    }

    [Fact]
    public void Homologar_con_producto_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.HomologarUseCase().Ejecutar(
            solicitud.Id, new HomologarProductoRequest(999), escenario.Usuario.Id, Fecha));
    }

    [Fact]
    public void Crear_producto_desde_solicitud_crea_el_producto_y_lo_asocia()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();

        var respuesta = escenario.CrearProductoUseCase().Ejecutar(
            solicitud.Id,
            new CrearProductoDesdeSolicitudRequest(escenario.Categoria.Id, escenario.UnidadMedida.Id, "COD-1"),
            escenario.Usuario.Id, Fecha);

        Assert.Equal("Creado", respuesta.Estado);
        Assert.NotNull(respuesta.ProductoResultanteId);
        var producto = escenario.Productos.ObtenerPorId(respuesta.ProductoResultanteId!.Value);
        Assert.Equal("Producto nuevo", producto!.Nombre);
    }

    [Fact]
    public void Crear_producto_desde_solicitud_con_categoria_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();

        Assert.Throws<RecursoNoEncontradoException>(() => escenario.CrearProductoUseCase().Ejecutar(
            solicitud.Id, new CrearProductoDesdeSolicitudRequest(999, escenario.UnidadMedida.Id), escenario.Usuario.Id, Fecha));
    }

    [Fact]
    public void Rechazar_solicitud_exige_motivo_y_cambia_el_estado()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();

        var respuesta = escenario.RechazarUseCase().Ejecutar(
            solicitud.Id, new RechazarSolicitudRequest("No cumple las condiciones del catálogo"), escenario.Usuario.Id, Fecha);

        Assert.Equal("Rechazado", respuesta.Estado);
        Assert.Equal("No cumple las condiciones del catálogo", respuesta.MotivoResolucion);
    }

    [Fact]
    public void Rechazar_solicitud_sin_motivo_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();

        Assert.Throws<ReglaDeNegocioException>(() => escenario.RechazarUseCase().Ejecutar(
            solicitud.Id, new RechazarSolicitudRequest(""), escenario.Usuario.Id, Fecha));
    }

    [Fact]
    public void Resolver_una_solicitud_ya_resuelta_lanza_regla_de_negocio()
    {
        var escenario = new Escenario();
        var solicitud = escenario.CrearSolicitudPendiente();
        escenario.RechazarUseCase().Ejecutar(solicitud.Id, new RechazarSolicitudRequest("Motivo"), escenario.Usuario.Id, Fecha);

        Assert.Throws<ReglaDeNegocioException>(() => escenario.RechazarUseCase().Ejecutar(
            solicitud.Id, new RechazarSolicitudRequest("Otro motivo"), escenario.Usuario.Id, Fecha));
    }
}
