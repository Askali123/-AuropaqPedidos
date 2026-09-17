# Incremento — Fase 6-9 en Frontend (Consolidación → Pedido → Entrega → Factura)

**Fecha y hora del incremento:** 2026-09-17 10:28

> Documento de planificación, derivado del estado real del proyecto verificado en
> `docs/2026-09-17-tareas.md` (mismo día). No reemplaza `docs/08-tareas.md` (lista oficial de
> tareas) ni `progreso.md` (tablero de seguimiento) — es la propuesta de alcance para el próximo
> incremento de trabajo, pendiente de aprobación antes de implementar.

---

## 1. Por qué este incremento

El primer flujo funcional (MVP, Fases 0-5) y Seguridad/Calidad (Fases 10-11) ya están cerrados.
Las Fases 6-9 (Consolidación, Pedidos, Entregas, Facturación) ya tienen, tras el incremento del
2026-09-17:

- Domain/Application/Infrastructure completos para las cuatro entidades.
- API HTTP real para crear/operar cada una (`ConsolidacionesController`,
  `PedidosProveedorController`, `EntregasController`, `FacturasController`).
- Autorización real (`[Authorize]` + permisos catalogados y sembrados, RN-063/ADR-066).

Pero el **Frontend no tiene ninguna pantalla** para Consolidación, Pedido ni Entrega, y la única
pantalla de Facturación (`FacturacionPage.tsx`) es **provisional**: los IDs de Proveedor y
PedidoProveedor se ingresan a mano, porque no existe ningún endpoint para listarlos o buscarlos
(`docs/05-api.md §31-34`: los cuatro controllers documentan explícitamente "no implementado: GET
(listar/consultar)" como decisión de minimalismo, no como error).

El objetivo de este incremento es cerrar esa brecha: dar al Frontend la forma de listar/consultar
lo necesario y construir las pantallas reales para el flujo completo
`Consolidación → Pedido → Entrega → Factura` (`CLAUDE.md §55`, vertical slice).

---

## 2. Lista de tareas propuestas (orden sugerido)

### Prioridad 1 — Backend: GET necesarios para que el Frontend pueda funcionar

Sin esto, cualquier pantalla nueva repetiría el mismo patrón provisional de
`FacturacionPage.tsx` (IDs a mano). Los permisos `*_VER` para las cuatro entidades ya están
catalogados y sembrados (`PEDIDO_VER`, `ENTREGA_VER`, `FACTURA_VER` — falta agregar uno análogo
para Consolidación, ver Prioridad 1 punto 5).

| # | Tarea | Relacionado |
|---|---|---|
| I1-1 | ✅ Hecho (2026-09-17) — `GET /api/v1/consolidaciones` (con `periodoId` opcional) y `GET /api/v1/consolidaciones/{id}` (2 pruebas nuevas) | TASK-036, `05-api.md §31` |
| I1-2 | ✅ Hecho (2026-09-17) — `GET /api/v1/pedidos-proveedor` (con `consolidacionId` opcional) y `GET /api/v1/pedidos-proveedor/{id}` (2 pruebas nuevas) | TASK-039, `05-api.md §32` |
| I1-3 | ✅ Hecho (2026-09-17) — `GET /api/v1/pedidos-proveedor/{id}/entregas` (listar, ruta anidada) y `GET /api/v1/entregas/{id}` (3 pruebas nuevas) | TASK-042, `05-api.md §33` |
| I1-4 | ✅ Hecho (2026-09-17) — `GET /api/v1/facturas` (exige `pedidoProveedorId`) y `GET /api/v1/facturas/{id}` (2 pruebas nuevas) | TASK-046, `05-api.md §34` |
| I1-5 | ✅ Resuelto (2026-09-17, RN-064/ADR-067) — Consolidación reutiliza `PEDIDO_VER`, sin permiso propio | RN-059/RN-063/RN-064 |

**Suite de pruebas al cierre de la Prioridad 1: 643/643 correctas, 0 omitidas** (9 pruebas
nuevas: 2 Consolidación + 3 Pedido + 2 Entrega + 2 Factura).

**Suite de pruebas al cierre de la Prioridad 2: 645/645 correctas, 0 omitidas** (2 pruebas
nuevas dedicadas a RN-065).

### Prioridad 2 — Backend: regla pendiente de Pedido

| # | Tarea | Relacionado |
|---|---|---|
| I2-1 | ✅ Hecho (2026-09-17) — `DetallePedidoProveedor.DistribucionCompleta` + guarda en `PedidoProveedor.Enviar()` (mismo criterio que Requisición, RN-011). 5 fixtures de test ajustados para distribuir antes de enviar; 2 pruebas nuevas dedicadas a la regla | TASK-041 |

### Prioridad 3 — Frontend: pantallas nuevas

Depende de Prioridad 1 (necesita los GET para listar/seleccionar en vez de pedir IDs a mano).

| # | Tarea | Relacionado |
|---|---|---|
| I3-1 | ✅ Hecho (2026-09-17) — `ConsolidacionPage` — listar consolidaciones del periodo, ver detalle (requisiciones/productos agrupados), botón "Generar consolidación" (`POST /consolidaciones`) | I1-1 |
| I3-2 | ✅ Hecho (2026-09-17) — `PedidosProveedorPage` — listar pedidos, crear pedido desde una consolidación (selector, no ID a mano), agregar detalle/distribución, enviar/cerrar/cancelar | I1-2 |
| I3-3 | ✅ Hecho (2026-09-17) — `EntregasPage` — listar entregas de un pedido, registrar entrega (selector de pedido, no ID a mano), agregar detalle/distribución, anular | I1-3 |
| I3-4 | ✅ Hecho (2026-09-17) — Reescrita `FacturacionPage.tsx` para usar selección real (proveedor/pedido desde `GET`) en vez de campos de texto libre — mismo patrón ya usado en `RequisicionesPage`/`RevisionPage` | I1-4 |
| I3-5 | ✅ Hecho (2026-09-17) — Enlazadas las 3 pantallas nuevas en `AppLayout.tsx`/`AppRoutes.tsx` (mismo patrón que `FacturacionPage`) | — |

**Verificación manual end-to-end en navegador (2026-09-17):** login real → Consolidación #82
(desde Requisición #21 aprobada, periodo 20/2026-10) → Pedido PP-TEST-001 #85 (detalle +
distribución completa vía `SelectorSedeGlobal` → Enviado, ejerciendo la guarda RN-065) →
Entrega REM-TEST-001 #89 (detalle + distribución parcial 6/10 → Pedido pasó a
`ParcialmenteEntregado` automáticamente) → Factura FAC-TEST-001 #93 (línea 10×5000, subtotal/
impuestos/total calculados por backend, luego Anulada). Sin errores de consola. Se encontraron y
corrigieron 2 bugs visuales (texto blanco sobre fondo claro por falta de clase Tailwind explícita
de color, ver `progreso.md`), sin relación con reglas de negocio.

### Prioridad 4 — Pruebas y cierre

| # | Tarea | Relacionado |
|---|---|---|
| I4-1 | ✅ Hecho (2026-09-17) — Pruebas de integración (`Api.Tests`) para cada `GET` nuevo (200, 404, 401, 403 — mismo patrón que el resto de controllers) | I1-1 a I1-4 |
| I4-2 | ✅ Hecho (2026-09-17) — Prueba manual end-to-end en el navegador: Requisición aprobada → Consolidación → Pedido → Entrega → Factura, completamente desde el Frontend, sin ningún ID ingresado a mano | I3-1 a I3-4 |
| I4-3 | ✅ Hecho (2026-09-17) — Actualizados `docs/08-tareas.md`/`progreso.md` al cerrar cada tarea (mismo criterio ya aplicado en el resto del incremento) | — |

**Cierre de I4-1 (2026-09-17):** los 200/404 de cada `GET` nuevo ya estaban cubiertos desde la
Prioridad 1. Faltaban 401 (sin JWT) y 403 (JWT válido sin el permiso `*_VER` correspondiente) —
agregados como guardia de regresión, mismo patrón ya usado para las acciones POST de estos
controllers:

- `ConsolidacionesFlujoTests.cs`: +3 pruebas (`Listar`/`Obtener` sin JWT → 401; `Listar`+`Obtener`
  con JWT sin `PEDIDO_VER` → 403).
- `AutorizacionPedidosEntregasFacturasFlujoTests.cs`: +9 pruebas (6× sin JWT → 401 para
  `Listar`/`Obtener` de Pedido, `ListarEntregas`/`Obtener` de Entrega, `Listar`/`Obtener` de
  Factura; 3× con JWT sin el permiso `*_VER` correspondiente → 403, una por controller).

**Suite de pruebas al cierre de la Prioridad 4: 657/657 correctas, 0 omitidas** (237
`Domain.Tests` + 251 `Application.Tests` + 169 `Api.Tests`, +12 respecto al cierre de la
Prioridad 2).

---

## 3. Ambigüedades — RESUELTAS (2026-09-17)

> Ambas quedaron cerradas antes de implementar la Prioridad 1, siguiendo `CLAUDE.md §51`.

### Ambigüedad 1 — Permiso de lectura de Consolidación — RESUELTA: alternativa A

`06-seguridad.md §9` no tiene un permiso `CONSOLIDACION_VER` — el único permiso de Consolidación
catalogado es `PEDIDO_CONSOLIDAR` (crear). Dos alternativas:

```text
A. Reutilizar PEDIDO_VER para leer Consolidación (misma cadena conceptual Consolidación→Pedido,
   ya lo tienen Compras/Recepcion/Administrador).
B. Catalogar CONSOLIDACION_VER como permiso propio (más granular, pero nadie lo pidió y no hay
   necesidad de negocio documentada que separe "ver consolidaciones" de "ver pedidos").
```

**Resuelta: A** — mismo criterio ya usado para agrupar Categoria/UnidadMedida bajo `PRODUCTO_*`
(RN-059): no crear un permiso nuevo sin necesidad de negocio real. Formalizada en
`RN-064`/`ADR-067` e implementada (I1-1, I1-2, I1-3).

### Ambigüedad 2 — Regla de distribución de Pedido (TASK-041) — RESUELTA: alternativa A

¿Debe forzarse que la suma de `DistribucionPedido` sea exactamente igual a `CantidadPedida` antes
de poder enviar el pedido (mismo patrón que `Requisicion.DistribucionCompleta`, RN-011), o puede
quedar parcialmente distribuido?

```text
A. Igual que Requisición: exigir distribución completa antes de Enviar.
B. Permitir enviar con distribución parcial o sin distribuir (Compras decide después).
```

**Resuelta: A** — confirmada explícitamente por el usuario (no había recomendación preliminar
clara, dependía de cómo opera realmente el área de Compras). Formalizada en `RN-065`/`ADR-068`.
**Implementación pendiente** (I2-1, Prioridad 2 — guarda en `PedidoProveedor.Enviar()`).

---

## 4. Fuera de alcance de este incremento

- Sustitución de productos (D-13/RN-045) — decisión de negocio ya cerrada, implementación
  diferida hasta que haya necesidad real (`01-reglas-negocio.md §15.1`).
- `PUT`/`DELETE` en cualquiera de los cuatro controllers — sin caso de uso ni necesidad
  documentada (mismo criterio de minimalismo del resto del proyecto).
- Fase 12 (Rendimiento) y Fase 13 (Despliegue) — bloqueadas por su propia regla documentada
  (evidencia real de lentitud / decisión de entorno), no por falta de trabajo.

---

## 5. Próximos pasos

1. ~~Resolver las dos ambigüedades de la sección 3~~ — **hecho (2026-09-17)**: RN-064/ADR-067 y
   RN-065/ADR-068.
2. ~~Aprobar el orden de prioridades de la sección 2~~ — **hecho (2026-09-17)**.
3. ~~Empezar por Prioridad 1 (los `GET` de backend)~~ — **hecho (2026-09-17)**: I1-1 a I1-5
   completos, 643/643 pruebas correctas.
4. ~~Prioridad 2 (I2-1)~~ — **hecho (2026-09-17)**: 645/645 pruebas correctas.
5. ~~Prioridad 3 (pantallas de Frontend)~~ — **hecho (2026-09-17)**: I3-1 a I3-5 completos,
   verificados manualmente en navegador contra el backend real (flujo completo Consolidación →
   Pedido → Entrega → Factura).
6. ~~Prioridad 4 (I4-1 a I4-3)~~ — **hecho (2026-09-17)**: 12 pruebas de integración nuevas
   (401/403 para los `GET` de I1-1 a I1-4), prueba manual end-to-end ya ejecutada en el cierre de
   la Prioridad 3, y documentación sincronizada. 657/657 pruebas correctas.

**Incremento "Fase 6-9 en Frontend" cerrado por completo (2026-09-17).**
