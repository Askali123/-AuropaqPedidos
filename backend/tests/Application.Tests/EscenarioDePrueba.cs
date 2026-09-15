using Application.Tests.Fakes;
using AuropaqPedidos.Domain.Entities;

namespace Application.Tests;

internal sealed class EscenarioDePrueba
{
    public FakeRequisicionRepository Requisiciones { get; } = new();
    public FakeEmpresaRepository Empresas { get; } = new();
    public FakeUsuarioRepository Usuarios { get; } = new();
    public FakePeriodoRepository Periodos { get; } = new();
    public FakeProductoRepository Productos { get; } = new();
    public FakeSedeRepository Sedes { get; } = new();
    public FakeGeneradorDeIdentificadores Ids { get; } = new();

    public Empresa Empresa { get; } = new(1, "AUROTECH");

    // Id=10: mismo convenio ya usado en varios tests de este archivo/paquete para "usuarioId".
    public Usuario Usuario { get; }

    public Periodo Periodo { get; } = new(
        1, 2026, 9,
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 30),
        new DateTime(2026, 9, 1), new DateTime(2026, 9, 3),
        "ABIERTO");

    public Producto Producto { get; } =
        new(1, "Papel higiénico", new Categoria(1, "Aseo"), new UnidadMedida(1, "UNIDAD", "Unidad"));

    public Sede Sede { get; }
    public Sede SedeAlterna { get; }

    public EscenarioDePrueba()
    {
        Sede = new Sede(1, Empresa, "Bogotá");
        SedeAlterna = new Sede(2, Empresa, "Medellín");
        Usuario = new Usuario(10, Empresa, "Usuario de prueba", "usuario.prueba@auropaq.com", "hash-de-prueba", new DateTime(2026, 9, 1));

        Empresas.Agregar(Empresa);
        Usuarios.Guardar(Usuario);
        Periodos.Agregar(Periodo);
        Productos.Agregar(Producto);
        Sedes.Agregar(Sede);
        Sedes.Agregar(SedeAlterna);
    }
}
