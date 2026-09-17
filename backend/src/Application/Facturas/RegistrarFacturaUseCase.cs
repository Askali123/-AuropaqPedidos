using AuropaqPedidos.Application.Excepciones;
using AuropaqPedidos.Application.Facturas.Abstracciones;
using AuropaqPedidos.Application.Facturas.Dtos;
using AuropaqPedidos.Application.PedidosProveedor;
using AuropaqPedidos.Application.PedidosProveedor.Abstracciones;
using AuropaqPedidos.Application.Requisiciones.Abstracciones;
using AuropaqPedidos.Domain.Entities;
using AuropaqPedidos.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AuropaqPedidos.Application.Facturas;

// TASK-046 (incremento MVP): registra la cabecera de una Factura asociada a un Proveedor y a un
// PedidoProveedor existentes (decisión provisional de alcance MVP: PedidoProveedor 1 ─── N
// Facturas). La coherencia proveedor↔pedido la valida el constructor de Factura (Domain).
//
// "Proveedor no activo" se valida aquí (no en Domain), mismo precedente ya usado en
// CrearPedidoProveedorUseCase: Domain solo exige que el proveedor no sea null.
//
// usuarioId agregado 2026-09-17 (P2-2/P1, docs/2026-09-17-tareas.md): el endpoint ya exige JWT
// real (RN-063/ADR-066), se usa para Factura.UsuarioCreacionId (RN-050/D-11).
public sealed class RegistrarFacturaUseCase
{
    private readonly IFacturaRepository _facturas;
    private readonly IProveedorRepository _proveedores;
    private readonly IPedidoProveedorRepository _pedidos;
    private readonly IGeneradorDeIdentificadores _ids;
    private readonly ILogger<RegistrarFacturaUseCase> _logger;

    public RegistrarFacturaUseCase(
        IFacturaRepository facturas, IProveedorRepository proveedores, IPedidoProveedorRepository pedidos,
        IGeneradorDeIdentificadores ids, ILogger<RegistrarFacturaUseCase> logger)
    {
        _facturas = facturas;
        _proveedores = proveedores;
        _pedidos = pedidos;
        _ids = ids;
        _logger = logger;
    }

    public FacturaResponse Ejecutar(DateTime fechaFactura, RegistrarFacturaRequest request, int usuarioId)
    {
        var proveedor = _proveedores.ObtenerPorId(request.ProveedorId)
            ?? throw new RecursoNoEncontradoException("El proveedor indicado no existe.");

        if (!proveedor.Activo)
            throw new ReglaDeNegocioException("El proveedor no está activo.");

        var pedido = PedidoProveedorFinder.ObtenerOLanzar(_pedidos, request.PedidoProveedorId);

        // D-09/RN-049: NumeroFactura único dentro del proveedor.
        if (_facturas.ExisteNumeroFacturaParaProveedor(proveedor.Id, request.NumeroFactura))
            throw new ReglaDeNegocioException(
                $"Ya existe una factura con el número '{request.NumeroFactura}' para este proveedor.");

        var factura = new Factura(
            id: _ids.Siguiente(),
            proveedor: proveedor,
            pedidoProveedor: pedido,
            numeroFactura: request.NumeroFactura,
            usuarioCreacionId: usuarioId,
            fechaFactura: fechaFactura,
            impuestos: request.Impuestos,
            observacion: request.Observacion);

        _facturas.Guardar(factura);

        _logger.LogInformation(
            "Factura {FacturaId} registrada (proveedor {ProveedorId}, pedido {PedidoId})", factura.Id, proveedor.Id, pedido.Id);

        return FacturaMapper.AResponse(factura);
    }
}
