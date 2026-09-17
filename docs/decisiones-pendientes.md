# Decisiones de negocio pendientes — índice rápido

> Propósito: evitar el error cometido el 2026-09-17 (un primer diagnóstico afirmó que 7
> ambigüedades de negocio seguían abiertas, releyendo `01-reglas-negocio.md §15` completo — un
> archivo de 60+ KB — solo 2 lo estaban realmente). Este archivo lista **solo** las Pendientes
> genuinamente sin resolver, para no tener que releer el documento largo cada vez.
>
> **Regla de mantenimiento:** cualquier sesión que agregue una `### Pendiente N` nueva en
> `01-reglas-negocio.md §15`, o que resuelva una existente, debe actualizar este archivo en el
> mismo cambio. Si este archivo dice algo distinto de `01-reglas-negocio.md §15`, el documento
> largo es la fuente de verdad — pero si eso ocurre, es una señal de que este índice quedó
> desactualizado y debe corregirse de inmediato.

## Estado actual (2026-09-17)

**0 ambigüedades de negocio genuinamente abiertas.**

Las últimas dos (Pendiente 1 — envío fuera de ventana, Pendiente 2 — modificación después de
aprobación) se resolvieron el 2026-09-17: ver `RN-061`/`ADR-064` y `RN-062`/`ADR-065` en
`01-reglas-negocio.md §15` y `07-decisiones-arquitectura.md`. Su **implementación** en código
sigue pendiente (tareas P3-1/P3-2, `docs/2026-09-17-tareas.md`) — eso es trabajo de código
rastreado, no una ambigüedad de negocio.

## Decisiones cerradas pero sin implementar (no son ambigüedades — no requieren nueva decisión)

| Decisión | RN / ADR | Implementación pendiente |
|---|---|---|
| D-13 — Sustitución explícita y trazable de productos | RN-045 / ADR-048 | Sin entidad/caso de uso todavía |

~~D-14 (bloqueo fuera de ventana) y D-15 (`APROBADA` inmutable)~~ — **implementadas** (ya
existían en Domain desde TASK-023-029; verificado 2026-09-17, sin cambio de código de
producción). Ver RN-061/RN-062.

## Huecos de seguridad conocidos (no son ambigüedades de negocio)

~~`PedidosProveedorController`/`EntregasController`/`FacturasController` sin `[Authorize]`~~ —
**cerrado (2026-09-17)**, ver P1/RN-063/ADR-066/D-16 en `docs/2026-09-17-tareas.md` y
`01-reglas-negocio.md §15`. Suite completa 629/629 correctas.

~~**Pendiente operativo:** los 5 permisos nuevos no están sembrados~~ — **resuelto (2026-09-17)**:
sembrados en `AuropaqPedidos` (dev) y asignados a `Compras`/`Recepcion`/`Administrador` según
RN-063, verificado por consulta directa tras la inserción. Ver `docs/2026-09-17-tareas.md §6`
punto 1. Sin huecos de seguridad ni pendientes operativos conocidos sobre Pedido/Entrega/Factura.

## Dónde está la lista larga

`docs/01-reglas-negocio.md §15` ("Reglas todavía pendientes de definición") tiene el detalle
completo de cada Pendiente, resuelta o no, con su justificación. Este índice es un resumen de
navegación, no la reemplaza.
