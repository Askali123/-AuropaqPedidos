using Application.Tests.Fakes;
using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Requisiciones;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

public class RequisicionConsultaUseCasesTests
{
    private static readonly DateTime Fecha = new(2026, 9, 15);

    private sealed class Escenario
    {
        public FakeRequisicionRepository Requisiciones { get; } = new();
        public FakeUsuarioRepository Usuarios { get; } = new();
        public Empresa EmpresaA { get; } = new(1, "AUROTECH");
        public Empresa EmpresaB { get; } = new(2, "FAVIPAQ");
        public Usuario UsuarioA { get; }
        public Usuario UsuarioB { get; }
        public Periodo Periodo { get; } = new(
            1, 2026, 9,
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
            "ABIERTO");

        public Categoria Categoria { get; } = new(1, "Papelería");
        public UnidadMedida UnidadMedida { get; } = new(1, "UNIDAD", "Unidad");
        public Producto Producto { get; } = new(1, "Papel", new Categoria(1, "Papelería"), new UnidadMedida(1, "UNIDAD", "Unidad"));

        public Escenario()
        {
            UsuarioA = new Usuario(1, EmpresaA, "Usuario A", "usuarioa@auropaq.com", "hash", Fecha);
            UsuarioB = new Usuario(2, EmpresaB, "Usuario B", "usuariob@auropaq.com", "hash", Fecha);
            Usuarios.Guardar(UsuarioA);
            Usuarios.Guardar(UsuarioB);
        }

        public ListarRequisicionesUseCase ListarUseCase() => new(Requisiciones, Usuarios);

        public ListarRequisicionesPendientesDeRevisionUseCase ListarPendientesUseCase() => new(Requisiciones, Usuarios);

        public Requisicion CrearRequisicion(int id, Empresa empresa) =>
            new(id, empresa, Periodo, usuarioCreacionId: 1, Fecha);

        public Requisicion CrearRequisicionEnRevision(int id, Empresa empresa)
        {
            var requisicion = CrearRequisicion(id, empresa);
            var sede = new Sede(id, empresa, "Sede principal");
            var detalle = requisicion.AgregarDetalle(id, Producto, cantidadSolicitada: 10);
            requisicion.AgregarDistribucion(id, detalle, sede, cantidad: 10);
            requisicion.Enviar(usuarioId: 1, Fecha);
            requisicion.IniciarRevision(usuarioId: 1, Fecha);
            return requisicion;
        }
    }

    [Fact]
    public void Obtener_devuelve_la_requisicion_solicitada()
    {
        var escenario = new Escenario();
        var requisicion = escenario.CrearRequisicion(1, escenario.EmpresaA);
        escenario.Requisiciones.Guardar(requisicion);

        var respuesta = new ObtenerRequisicionUseCase(escenario.Requisiciones).Ejecutar(requisicion.Id);

        Assert.Equal(requisicion.Id, respuesta.Id);
        Assert.Equal("Borrador", respuesta.Estado);
    }

    [Fact]
    public void Obtener_requisicion_inexistente_lanza_no_encontrado()
    {
        var escenario = new Escenario();

        Assert.Throws<RecursoNoEncontradoException>(() => new ObtenerRequisicionUseCase(escenario.Requisiciones).Ejecutar(999));
    }

    [Fact]
    public void Listar_devuelve_solo_las_requisiciones_de_la_empresa_indicada()
    {
        var escenario = new Escenario();
        escenario.Requisiciones.Guardar(escenario.CrearRequisicion(1, escenario.EmpresaA));
        escenario.Requisiciones.Guardar(escenario.CrearRequisicion(2, escenario.EmpresaB));

        var respuesta = escenario.ListarUseCase().Ejecutar(escenario.UsuarioA.Id);

        var unica = Assert.Single(respuesta);
        Assert.Equal(1, unica.Id);
    }

    [Fact]
    public void Listar_pendientes_de_revision_solo_incluye_las_en_revision_de_la_empresa()
    {
        var escenario = new Escenario();
        var enRevisionMismaEmpresa = escenario.CrearRequisicionEnRevision(1, escenario.EmpresaA);
        escenario.Requisiciones.Guardar(enRevisionMismaEmpresa);

        var borradorMismaEmpresa = escenario.CrearRequisicion(2, escenario.EmpresaA);
        escenario.Requisiciones.Guardar(borradorMismaEmpresa);

        var enRevisionOtraEmpresa = escenario.CrearRequisicionEnRevision(3, escenario.EmpresaB);
        escenario.Requisiciones.Guardar(enRevisionOtraEmpresa);

        var respuesta = escenario.ListarPendientesUseCase().Ejecutar(escenario.UsuarioA.Id);

        var unica = Assert.Single(respuesta);
        Assert.Equal(enRevisionMismaEmpresa.Id, unica.Id);
        Assert.Equal("EnRevision", unica.Estado);
    }
}
