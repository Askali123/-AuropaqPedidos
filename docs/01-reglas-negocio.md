# REGLAS DE NEGOCIO

## Sistema de Gestión de Requisiciones y Pedidos - Auropaq

**Proyecto:** AuropaqPedidos
**Documento:** Reglas de negocio
**Código:** RN
**Estado:** En definición
**Última actualización:** 2026-09-08

---

# 1. Propósito del documento

Este documento contiene las reglas que determinan cómo debe comportarse el negocio dentro del sistema AuropaqPedidos.

Las reglas aquí definidas tienen prioridad sobre decisiones de implementación.

Una regla de negocio:

- No depende de React.
- No depende de C#.
- No depende de SQL Server.
- No depende de una pantalla específica.
- Debe representar una necesidad real del proceso empresarial.

La implementación técnica debe adaptarse a estas reglas y no modificar su significado.

---

# 2. Principios de las reglas de negocio

## 2.1. No inventar reglas

Si una situación del negocio no está definida en este documento, Claude Code no debe asumir una solución definitiva.

Debe:

1. Identificar la ambigüedad.
2. Informarla.
3. Proponer alternativas si es necesario.
4. Esperar una decisión cuando la ambigüedad afecte el diseño.

---

## 2.2. Las reglas importantes deben validarse en backend

El frontend puede realizar validaciones para mejorar la experiencia del usuario.

Sin embargo, las reglas de negocio deben ser garantizadas por el backend.

Por ejemplo:

```text
Frontend:
"Esta cantidad parece inválida."

Backend:
"La cantidad no cumple la regla de negocio y la operación es rechazada."
```

El frontend nunca debe considerarse la fuente definitiva de seguridad o validación.

---

# 3. Organización empresarial

## RN-001 — Empresas configurables

El sistema debe permitir registrar múltiples empresas.

Las empresas no deben estar codificadas directamente en el sistema.

Debe ser posible agregar una nueva empresa mediante configuración/datos sin modificar el código fuente.

### Ejemplo

Inicialmente pueden existir:

```text
FAVIPAQ
COURIERBOX ÁLAMOS
AUROTECH
COURIERBOX
```

Posteriormente podría agregarse otra empresa sin modificar las reglas generales del sistema.

---

## RN-002 — Una empresa puede tener múltiples sedes

Una empresa puede tener una o varias sedes.

Cada sede pertenece a una única empresa.

La sede debe representar una ubicación física real donde pueden recibirse productos.

---

## RN-003 — La sede determina el destino físico

Cuando una requisición requiere distribución por sedes, las cantidades deben poder asociarse a una o varias sedes pertenecientes a la empresa solicitante.

Una sede de otra empresa no puede ser utilizada como destino de una requisición.

---

## RN-004 — Los usuarios tienen un ámbito organizacional

Un usuario debe estar asociado al contexto organizacional que corresponda según su responsabilidad.

El sistema debe poder determinar sobre qué empresa y/o sedes puede operar un usuario.

La autorización de acceso debe diferenciar:

```text
Qué puede hacer el usuario
```

de:

```text
Sobre qué información puede hacerlo
```

Esto corresponde respectivamente a:

```text
Permisos
Ámbito / alcance
```

---

# 4. Periodos y solicitudes mensuales

## RN-005 — Existe un periodo operativo mensual

Las requisiciones se realizan dentro de un periodo mensual.

El periodo debe identificar como mínimo:

```text
Año
Mes
Estado
```

Las fechas específicas de recepción de solicitudes deben ser configurables.

---

## RN-006 — La ventana de solicitud es configurable

Cada periodo debe permitir definir una ventana durante la cual las empresas pueden enviar sus requisiciones.

La duración de la ventana no debe estar codificada como un número fijo de días.

Ejemplo:

```text
Periodo: Septiembre 2026
Inicio de solicitudes: 01/09/2026
Fin de solicitudes: 03/09/2026
```

El siguiente periodo podría utilizar fechas diferentes.

---

## RN-007 — Una empresa tiene una requisición mensual

Una empresa puede tener como máximo una requisición correspondiente a un periodo mensual.

La unicidad conceptual es:

```text
Empresa + Periodo
```

No:

```text
Empresa + Sede + Periodo
```

Esto permite que una única requisición pueda contener necesidades destinadas a varias sedes de la misma empresa.

---

# 5. Requisiciones

## RN-008 — La requisición representa una necesidad interna

Una requisición representa lo que una empresa necesita solicitar durante un periodo.

La requisición no representa todavía una compra al proveedor.

Debe existir una separación clara entre:

```text
Necesidad interna
```

y:

```text
Compra al proveedor
```

---

## RN-009 — Una requisición contiene detalles

Una requisición puede contener uno o varios detalles de productos.

Cada detalle debe representar como mínimo:

```text
Producto
Cantidad solicitada
```

Podrá contener información adicional como observaciones y distribución por sede.

---

## RN-010 — Una requisición puede distribuir cantidades entre sedes

Una empresa puede solicitar un mismo producto para diferentes sedes.

Por ejemplo:

```text
Producto: Papel higiénico
Cantidad total solicitada: 100

Sede Bogotá: 60
Sede Medellín: 40
```

La distribución debe mantener la relación entre la cantidad total solicitada y las cantidades asignadas a las sedes.

---

## RN-011 — La suma de las distribuciones debe coincidir con la cantidad solicitada

Cuando un detalle de requisición tenga distribución por sedes:

```text
SUM(cantidades distribuidas) = cantidad solicitada
```

Ejemplo válido:

```text
Cantidad solicitada: 100

Bogotá: 60
Medellín: 40

Total distribuido: 100
```

Ejemplo inválido:

```text
Cantidad solicitada: 100

Bogotá: 60
Medellín: 30

Total distribuido: 90
```

También es inválido distribuir más de la cantidad solicitada.

---

# 6. Estado de una requisición

## RN-012 — Una requisición puede permanecer como borrador

Una requisición puede guardarse en estado:

```text
BORRADOR
```

Mientras se encuentre en este estado, el usuario autorizado puede continuar construyéndola.

Puede:

- Agregar productos.
- Modificar cantidades.
- Eliminar productos.
- Modificar la distribución por sede.
- Modificar observaciones.
- Guardar cambios.

---

## RN-013 — Guardar no significa enviar

La acción de guardar una requisición no significa que haya sido enviada para revisión.

Estas son acciones diferentes:

```text
Guardar
↓
BORRADOR
```

y:

```text
Enviar requisición
↓
ENVIADA
```

La requisición solamente entra al proceso de revisión cuando el usuario ejecuta explícitamente la acción de envío.

---

## RN-014 — Una requisición enviada no puede modificarse silenciosamente

Una vez enviada una requisición:

```text
BORRADOR → ENVIADA
```

el solicitante no debe poder modificarla silenciosamente.

Los cambios posteriores deben estar controlados por el flujo de estados y deben conservar trazabilidad.

---

## RN-015 — El envío debe respetar la ventana de solicitud

Una requisición solamente puede enviarse dentro de la ventana de solicitud definida para el periodo, salvo que posteriormente se defina explícitamente una regla de excepción.

La validación debe realizarse en backend.

---

# 7. Revisión

## RN-016 — Una requisición enviada entra en revisión

Una requisición enviada queda disponible para el usuario o rol autorizado para realizar la revisión.

El revisor debe poder consultar:

- Empresa.
- Periodo.
- Sedes.
- Productos.
- Cantidades.
- Observaciones.
- Productos no catalogados.
- Historial relevante.

---

## RN-017 — Una requisición puede ser aprobada

El usuario autorizado puede aprobar una requisición cuando esta cumpla las reglas requeridas para continuar el proceso.

Una requisición aprobada queda disponible para las siguientes etapas del proceso.

---

## RN-018 — Una requisición puede ser devuelta para corrección

El revisor puede devolver una requisición cuando encuentre información que requiere corrección.

La devolución debe registrar como mínimo:

```text
Usuario que devuelve
Fecha y hora
Motivo de devolución
```

---

## RN-019 — La devolución no crea automáticamente una nueva requisición

Cuando una requisición es devuelta para corrección, el solicitante debe corregir la requisición existente.

No se debe crear automáticamente una segunda requisición para el mismo:

```text
Empresa + Periodo
```

Esto permite conservar la trazabilidad completa del proceso.

---

## RN-020 — Una requisición corregida puede reenviarse

Después de corregir una requisición devuelta, el usuario autorizado puede volver a enviarla para revisión.

El historial debe conservar:

```text
Envío inicial
↓
Devolución
↓
Corrección
↓
Reenvío
```

La corrección no debe eliminar el historial anterior.

---

# 8. Productos

## RN-021 — El catálogo interno es la referencia oficial

El sistema debe manejar un catálogo interno de productos.

Cada producto oficial puede tener:

```text
Código interno
Nombre
Categoría
Unidad de medida
Estado
```

El código interno representa la identificación del producto dentro del sistema.

---

## RN-022 — El código del proveedor es diferente al código interno

Un proveedor puede utilizar un código diferente para representar un producto.

Por lo tanto, el sistema debe diferenciar:

```text
Código interno
```

de:

```text
Código del proveedor
```

No se debe asumir que ambos códigos son iguales.

---

## RN-023 — Un producto interno puede tener referencias de diferentes proveedores

Un producto interno puede estar relacionado con uno o varios proveedores.

Cada relación puede almacenar la información específica utilizada por ese proveedor.

Ejemplo conceptual:

```text
Producto interno:
PAP-001

Proveedor A:
Código: 45871

Proveedor B:
Código: X-9821
```

---

# 9. Productos no catalogados

## RN-024 — Un usuario puede solicitar un producto no catalogado

Si un producto solicitado no existe en el catálogo oficial, el usuario debe poder informar la necesidad sin crear directamente un producto oficial.

Esto evita contaminar el catálogo con información no validada.

---

## RN-025 — Los productos no catalogados requieren revisión

Un producto no catalogado debe pasar por un proceso de revisión.

El responsable autorizado podrá:

```text
1. Homologarlo con un producto existente.
2. Crear un nuevo producto oficial.
3. Rechazar la solicitud.
```

---

## RN-026 — El producto no catalogado debe conservar su información original

La información suministrada originalmente por el solicitante debe conservarse para efectos de trazabilidad.

La homologación posterior no debe eliminar la evidencia de lo que originalmente fue solicitado.

---

# 10. Consolidación

## RN-027 — La consolidación agrupa necesidades aprobadas

La consolidación debe utilizar requisiciones aprobadas.

Su objetivo es determinar la necesidad total de productos para el proceso de compra.

Ejemplo:

```text
AUROTECH:
Producto A = 30

FAVIPAQ:
Producto A = 20

COURIERBOX:
Producto A = 35

Necesidad consolidada:
Producto A = 85
```

---

## RN-028 — La consolidación no debe modificar las requisiciones originales

La consolidación representa información derivada de las requisiciones.

No debe modificar ni sobrescribir las cantidades originales solicitadas.

Debe ser posible regresar desde una cantidad consolidada hasta las requisiciones que la originaron.

---

## RN-029 — La consolidación debe conservar trazabilidad

Debe ser posible identificar:

```text
Total consolidado
↓
Empresa
↓
Requisición
↓
Detalle de requisición
↓
Sede
↓
Cantidad original
```

Esto permite explicar de dónde salió cada cantidad consolidada.

---

## RN-051 — Un período puede tener varias consolidaciones; una requisición participa en, a lo sumo, una

*(Cierre de negocio 2026-09-11, decisión A2. Ver ADR-053.)*

Un mismo `Periodo` puede tener múltiples `Consolidacion`. Sin embargo, una `Requisicion` aprobada solo puede participar (con todos sus detalles) en **una** consolidación — nunca en dos.

Consecuencias:

```text
Una nueva consolidación del mismo periodo toma
únicamente las requisiciones aprobadas que
TODAVÍA NO fueron consolidadas.

Las requisiciones ya consolidadas no vuelven
a generar necesidad en una consolidación posterior.

Una requisición aprobada DESPUÉS de una primera
consolidación sí puede entrar en una segunda
consolidación del mismo periodo.
```

Ejemplo:

```text
Consolidación #1 (periodo X): toma Requisición A (aprobada).

Requisición B se aprueba después.

Consolidación #2 (mismo periodo X): toma solo Requisición B.
No vuelve a tomar Requisición A.
```

Esto reemplaza la ambigüedad original ("¿puede consolidarse el mismo periodo más de una vez?", detectada en la auditoría 2026-09-11) — queda cerrada como decisión de negocio e implementada (`CrearConsolidacionUseCase`, con salvaguarda de base de datos: índice único sobre `AsignacionConsolidacion.DetalleRequisicionOrigenId`).

---

# 11. Pedidos a proveedores

## RN-030 — Una consolidación puede generar varios pedidos

Una consolidación no necesariamente debe generar un único pedido.

Puede generar varios pedidos cuando diferentes productos deban adquirirse a diferentes proveedores.

Ejemplo:

```text
Consolidación
      ↓
 ┌────┴────┐
 ↓         ↓
Proveedor A  Proveedor B
Pedido 1     Pedido 2
```

---

## RN-031 — El pedido representa la compra real

El pedido al proveedor representa lo que efectivamente se solicita comprar.

Puede diferir de la necesidad consolidada.

Ejemplo:

```text
Necesidad consolidada: 85
Compra al proveedor: 100
```

La diferencia debe conservarse y ser explicable.

---

## RN-041 — El proveedor se selecciona explícitamente por Compras

*(Cierra D-01 — cierre documental 2026-09-11. Ver ADR-044.)*

Cuando un producto tiene más de un proveedor asociado (`ProductoProveedor`, `02-dominio.md §11`), el proveedor de cada `PedidoProveedor` es seleccionado **explícitamente por el usuario responsable de Compras** al crearlo.

El sistema puede mostrar información de los proveedores asociados al producto, pero **no seleccionará automáticamente** un proveedor por precio, disponibilidad, proveedor preferido, tiempo de entrega o condiciones comerciales.

```text
DetalleConsolidacion
→ selección explícita del usuario (Compras)
→ PedidoProveedor
```

No debe implementarse lógica automática de selección de proveedor mientras no exista una regla empresarial explícita que la justifique.

> **Actualización (2026-09-18, TASK-105):** "el sistema puede mostrar información de los
> proveedores asociados al producto" ya está implementado — al agregar un detalle a un
> `PedidoProveedor`, si existe una relación `ProductoProveedor` activa entre el producto y el
> proveedor ya elegido para ese pedido, su `CodigoProveedor` se captura como fotografía en el
> detalle (`DetallePedidoProveedor.CodigoProveedorUtilizado`) y se muestra en el Frontend. Sigue
> sin haber selección automática de proveedor — la selección la sigue haciendo Compras
> explícitamente, esto solo hace visible la información que ya existía en el catálogo pero nunca
> se usaba en el flujo de compra (hallazgo D1/F4,
> `docs/2026-09-18-auditoria-dominio-roles-frontend.md`).

---

## RN-042 — CantidadPedida se decide manualmente; sin cálculo automático de empaques o mínimos

*(Cierra D-02 y, junto con RN-031, D-12 — cierre documental 2026-09-11. Ver ADR-045.)*

El sistema **no calcula automáticamente** presentaciones comerciales, empaques, múltiplos de compra, cantidades mínimas del proveedor ni redondeos de cantidades.

`CantidadPedida` es definida manualmente por Compras (`DetallePedidoProveedor.CantidadPedida`), y puede diferir de `CantidadNecesaria` por cualquiera de esos motivos (RN-031) sin que el sistema exija ni calcule esa diferencia.

No debe implementarse un motor de reglas comerciales de empaques/mínimos en esta etapa.

---

## RN-043 — Ciclo de estados de PedidoProveedor

*(Cierra D-03 — cierre documental 2026-09-11. Ver ADR-046.)*

```text
BORRADOR
  ↓
ENVIADO
  ↓
PARCIALMENTE_ENTREGADO
  ↓
ENTREGADO
  ↓
CERRADO
```

Cancelación controlada, disponible desde cualquiera de los tres primeros estados:

```text
BORRADOR / ENVIADO / PARCIALMENTE_ENTREGADO
  → CANCELADO
```

| Estado | Significado |
|---|---|
| `BORRADOR` | Pedido en construcción, aún no enviado al proveedor. |
| `ENVIADO` | Pedido enviado al proveedor. |
| `PARCIALMENTE_ENTREGADO` | Existe al menos una entrega, pero todavía hay cantidad pendiente por entregar (RN-034). |
| `ENTREGADO` | La cantidad pendiente de entrega es cero. |
| `CERRADO` | El pedido fue cerrado explícitamente por un usuario autorizado, después de completar la entrega (RN-044). |
| `CANCELADO` | El pedido deja de continuar operativamente. |

No deben agregarse estados como `CONFIRMADO`, `EN_PROCESO`, `FACTURADO` o `PAGADO` mientras no exista una necesidad empresarial documentada que los justifique.

> **Estado de implementación (2026-09-11):** implementado. `PedidoProveedor.Estado` es un enum (`PedidoProveedorEstado`) con las transiciones descritas arriba (`Enviar`/`ActualizarEstadoPorEntregas`/`Cerrar`/`Cancelar`), persistido como string (ver `04-base-datos.md §26`).

---

## RN-044 — Cierre explícito de PedidoProveedor

*(Cierra D-08 — cierre documental 2026-09-11. Ver ADR-047.)*

Un `PedidoProveedor` puede pasar a `CERRADO` (RN-043) cuando se cumplen **ambas** condiciones:

```text
Cantidad pendiente de entrega = 0
(RN-034: CantidadPedida - SUM(CantidadEntregada))

y

un usuario autorizado ejecuta explícitamente la acción de cierre.
```

La existencia o ausencia de `Factura` **no** es requisito para cerrar el pedido — Entrega y Factura son procesos independientes (RN-040).

```text
ENTREGADO
  → CERRADO
```

---

## RN-045 — La sustitución de productos debe ser explícita y trazable

*(Cierra D-13 — cierre documental 2026-09-11. Ver ADR-048.)*

Cuando el proveedor no dispone del producto solicitado y propone uno equivalente, la sustitución **no puede realizarse silenciosamente**.

Debe conservarse:

```text
Producto original
Producto sustituto
Motivo de la sustitución
Usuario responsable
Fecha de la sustitución
```

No debe modificarse silenciosamente el producto solicitado/consolidado durante el proceso de compra.

> **Estado de implementación:** esta regla queda **decidida conceptualmente**, pero su implementación (mecanismo de registro de la sustitución) queda pendiente de una tarea futura — no existe hoy código, entidad ni caso de uso para esto.

---

# 12. Distribución y entrega

## RN-032 — Un pedido puede abastecer varias sedes

Un pedido a proveedor puede contener productos destinados a diferentes empresas y sedes.

Por lo tanto, el sistema debe conservar la distribución de las cantidades compradas.

Ejemplo:

```text
Pedido proveedor:
Producto A = 85

Distribución:

AUROTECH - Bogotá:       20
AUROTECH - Medellín:     15
FAVIPAQ - Bogotá:        30
COURIERBOX - Bogotá:     20
```

---

## RN-033 — Una orden puede tener entregas parciales

Un pedido a proveedor puede entregarse en una o varias entregas.

Ejemplo:

```text
Pedido:
100 unidades

Entrega 1:
60 unidades

Entrega 2:
40 unidades
```

El sistema debe mantener cada entrega individualmente.

---

## RN-034 — La cantidad pendiente debe poder determinarse

La cantidad pendiente de entrega debe poder calcularse a partir de:

```text
Cantidad pedida - cantidad entregada
```

(Nombre alineado con `DetallePedidoProveedor.CantidadPedida`, `04-base-datos.md §27/§30`.)

Ejemplo:

```text
Pedido: 100
Entregado: 60

Pendiente: 40
```

---

## RN-035 — La entrega debe conservar el destino

Cada cantidad entregada debe poder relacionarse con la sede correspondiente.

La información histórica del destino debe conservarse incluso si posteriormente cambia la dirección registrada de la sede.

Por esta razón, la información de dirección utilizada durante una entrega debe conservarse como información histórica de la operación.

---

## RN-046 — Estados operativos de Entrega

*(Cierra D-04 — cierre documental 2026-09-11. Ver ADR-049.)*

Los estados operativos de `Entrega` son:

```text
REGISTRADA
ANULADA
```

No deben utilizarse como estados del agregado `Entrega` valores como `ACEPTADA`, `RECHAZADA` o `DAÑADA` — esas son condiciones de la **recepción de mercancía**, un concepto distinto (Pendiente 4), que podrá manejarse posteriormente mediante reglas propias si el negocio las requiere. Esta regla cierra Pendiente 4 en el sentido de que decide **no** implementar esas condiciones detalladas como parte del estado de `Entrega` en esta etapa; la necesidad de modelarlas como un concepto aparte queda diferida, no descartada.

> **Estado de implementación (2026-09-11):** implementado. `Entrega.Estado` es un enum (`EntregaEstado`: `Registrada`/`Anulada`), persistido como string (ver `04-base-datos.md §29`). Una entrega solo puede crearse contra un pedido `Enviado`/`ParcialmenteEntregado`.

---

## RN-052 — Anular una entrega recalcula el pedido; no se permite si el pedido está CERRADO

*(Cierre de negocio 2026-09-11, decisión B1. Ver ADR-054.)*

Al anular una `Entrega` (RN-046: `REGISTRADA → ANULADA`):

```text
Sus cantidades dejan de contar como cantidad
entregada válida (tanto para el tope de nuevas
entregas como para el estado del pedido).

Se recalcula PedidoProveedor.Estado según las
entregas VÁLIDAS (no ANULADAS) restantes:

  - Si ya no queda ninguna cantidad entregada
    válida -> ENVIADO.
  - Si queda alguna cantidad entregada válida
    pero todavía hay cantidad pendiente -> PARCIALMENTE_ENTREGADO.
  - Si la cantidad entregada válida cubre
    exactamente lo pedido -> ENTREGADO.
```

**No se permite anular una entrega si el `PedidoProveedor` ya está `CERRADO`.** Un pedido cerrado no se reabre ni se recalcula; la operación debe rechazarse y la entrega debe permanecer sin cambios. *(Para `CANCELADO`, ver RN-054 — misma prohibición de anular, y además bloquea crear/modificar entregas.)*

Ejemplo:

```text
Pedido = 100

Entrega 1 = 60 (pedido: PARCIALMENTE_ENTREGADO)
Entrega 2 = 40 (pedido: ENTREGADO)

Se anula Entrega 2 -> queda válido solo Entrega 1 (60)
-> pedido vuelve a PARCIALMENTE_ENTREGADO.

Se anula también Entrega 1 -> no queda ninguna
cantidad entregada válida -> pedido vuelve a ENVIADO.
```

---

## RN-054 — Un pedido CANCELADO queda fuera de operación para Entregas

*(Cierre de negocio 2026-09-11. Formaliza y completa RN-046/RN-052 para el estado `CANCELADO`. Ver ADR-054, actualizado.)*

`CANCELADO` representa un `PedidoProveedor` fuera de operación. Ninguna operación normal sobre `Entrega` puede afectarlo ni a sus entregas asociadas:

```text
1. No se puede crear una nueva Entrega para un
   pedido CANCELADO (ya se deducía de RN-046: solo
   ENVIADO/PARCIALMENTE_ENTREGADO admiten entregas
   nuevas; aquí queda explícito para CANCELADO).

2. No se pueden agregar detalles a una Entrega
   REGISTRADA que quedó asociada a un pedido que
   luego se canceló (Cancelar() es válido desde
   PARCIALMENTE_ENTREGADO, así que puede existir
   una Entrega previa cuando el pedido pasa a
   CANCELADO).

3. No se pueden agregar distribuciones a esa
   Entrega, por el mismo motivo.

4. No se puede anular esa Entrega (mismo criterio
   que RN-052 para CERRADO: CANCELADO tampoco se
   reabre ni se recalcula).

5. Ninguna de estas operaciones rechazadas modifica
   el estado del pedido ni los datos ya existentes
   de la Entrega — la excepción se lanza antes de
   persistir cualquier cambio.
```

Esta regla no introduce un concepto de negocio nuevo: completa, para `CANCELADO`, exactamente el mismo criterio que RN-052 ya aplicaba a `CERRADO` ("un pedido fuera de operación no se reabre ni se recalcula"), y lo extiende a crear/agregar detalle/agregar distribución (operaciones que RN-052 no cubría porque solo hablaba de anular).

---

# 13. Facturación

## RN-038 — Auropaq Pedidos no es un sistema contable

*(Confirmada explícitamente por el negocio — refinamiento 2026-09-11.)*

El sistema registra información operativa de facturas del proveedor, únicamente para mantener trazabilidad de las compras.

Dentro del alcance:

```text
Proveedor
Número de factura
Fecha
Pedido asociado
Productos, cantidades y precios (detalle)
Subtotal
Impuestos
Total
Observaciones
Estado operativo (sin ciclo contable)
```

Fuera del alcance (salvo que el negocio lo solicite explícitamente en el futuro):

```text
Asientos contables / cuentas contables
Causación
Pagos y su seguimiento
Conciliación bancaria
Retenciones contables
Libros contables
Declaraciones tributarias
Gestión tributaria integral (tasas configurables, impuestos como módulo)
```

Ver `00-contexto-proyecto.md §9.2` y ADR-041 (`07-decisiones-arquitectura.md`).

---

## RN-039 — Un pedido puede respaldarse en varias facturas; una factura pertenece a un único pedido (alcance inicial)

*(Confirmada para el alcance inicial — refinamiento 2026-09-11. Ver ADR-042.)*

```text
PedidoProveedor
    ├── Factura 1
    ├── Factura 2
    └── Factura 3
```

Cada `Factura` está asociada a un único `PedidoProveedor` (`Factura N ─── 1 PedidoProveedor`). Esto ya coincide con la implementación actual (`Factura.PedidoProveedorId` obligatoria).

**No está confirmado ni implementado** el caso general en el que una factura cubra varios pedidos (`Factura N ─── N PedidoProveedor`). Esa generalización queda pendiente hasta que exista una necesidad de negocio explícita (ver `§15`, matriz de decisiones, D-06).

---

## RN-040 — Entregas y facturas de un pedido son independientes entre sí

*(Confirmada explícitamente por el negocio — refinamiento 2026-09-11. Ver ADR-043.)*

```text
PedidoProveedor
      ├── Entregas
      └── Facturas
```

No existe relación obligatoria `Entrega → Factura`. Ambas cuelgan de `PedidoProveedor` de forma independiente, porque el proceso de entrega física y el proceso de facturación pueden avanzar a ritmos distintos y no necesariamente coinciden.

Ejemplo:

```text
Pedido = 100

Entregas: 40 + 60
Facturas: 100
```

o:

```text
Pedido = 100

Entregas: 50 + 50
Facturas: 30 + 20 + 50
```

No debe asumirse correspondencia 1:1, ni por cantidad ni por número de registros, entre entregas y facturas de un mismo pedido.

---

## RN-047 — Estados operativos de Factura

*(Cierra D-05 — cierre documental 2026-09-11. Ver ADR-050.)*

Los estados operativos de `Factura` son:

```text
REGISTRADA
ANULADA
```

No deben introducirse estados contables como `CAUSADA`, `CONTABILIZADA`, `PAGADA` o `CONCILIADA` — Auropaq Pedidos maneja la factura desde una perspectiva operativa, no contable (RN-038).

Esta regla reemplaza la propuesta de ciclo de vida redactada el 2026-09-10 (`progreso.md`, "propuesta de negocio FASE 9"), que había quedado explícitamente `PROPUESTA — NO APROBADA`; queda ahora aprobada en esta versión simplificada (`REGISTRADA`/`ANULADA`), no en la propuesta original de múltiples estados.

> **Estado de implementación (2026-09-11):** implementado. `Factura.Estado` es un enum (`FacturaEstado`: `Registrada`/`Anulada`), persistido como string (ver `04-base-datos.md §32`).

---

## RN-048 — Cantidad facturada acumulada no debe superar la cantidad pedida

*(Cierra D-10 — cierre documental 2026-09-11. Ver ADR-050.)*

```text
SUM(CantidadFacturada) ≤ CantidadPedida
```

para cada `DetallePedidoProveedor`, sumando todas las facturas registradas sobre ese detalle.

No debe limitarse la cantidad facturada directamente por `CantidadEntregada` — Entrega y Factura permanecen como procesos independientes (RN-040), así que facturar no depende de haber registrado la entrega correspondiente.

> **Nota de implementación:** ya implementado tal como se describe (`Factura.AgregarDetalle`, ver `02-dominio.md §29.1`).

---

## RN-053 — Una factura anulada deja de contar para el acumulado facturado

*(Cierre de negocio 2026-09-11, decisión B4. Ver ADR-055.)*

`SUM(CantidadFacturada) ≤ CantidadPedida` (RN-048) se calcula únicamente sobre facturas en estado `REGISTRADA`. Una `Factura` en estado `ANULADA` (RN-047) deja de contar en esa suma: su cantidad queda libre para que una factura nueva la utilice.

Ejemplo:

```text
CantidadPedida = 100

Factura válida = 60
Factura anulada = 40

CantidadFacturada válida = 60

Nueva factura permitida = hasta 40
```

---

## RN-049 — Identificadores documentales y unicidad contextual

*(Cierra D-09 (y Pendientes 9/14) — cierre documental 2026-09-11. Ver ADR-051.)*

Debe diferenciarse:

| Campo | Significado |
|---|---|
| `Id` | Identificador interno técnico de la entidad. No debe usarse como referencia de negocio. |
| `NumeroPedido` | Referencia operacional del pedido al proveedor. |
| `NumeroRemision` | Número de remisión proporcionado para la entrega. |
| `NumeroFactura` | Número de factura proporcionado por el proveedor. |

Los números documentales no deben usarse como sustitutos del `Id` técnico (por ejemplo, para buscar o enlazar registros internamente).

Reglas de unicidad:

```text
NumeroPedido    — único dentro del proveedor.
NumeroFactura   — único dentro del proveedor.
NumeroRemision  — único dentro del PedidoProveedor.
```

> **Estado de implementación (2026-09-11):** implementado. `PedidoProveedorConfiguration`/`EntregaConfiguration`/`FacturaConfiguration` tienen índices únicos compuestos (`ProveedorId+NumeroPedido`, `PedidoProveedorId+NumeroRemision`, `ProveedorId+NumeroFactura` respectivamente — migración `ImplementarEstadosEIdentificadoresCierreDocumental`), y los casos de uso de creación validan la duplicidad antes de persistir (`ReglaDeNegocioException`, 422). Los tres campos se reciben como texto libre explícito del llamador (no autogenerados).

---

# 14. Trazabilidad

## RN-036 — Las acciones importantes deben quedar registradas

El sistema debe mantener historial de las acciones relevantes realizadas sobre las entidades del proceso.

Como mínimo se debe poder registrar:

```text
Usuario
Fecha y hora
Acción
Entidad afectada
Identificador de la entidad
Información relevante
```

---

## RN-037 — Los cambios de estado deben conservar historial

Cuando una requisición cambie de estado, debe quedar registrado:

```text
Estado anterior
Estado nuevo
Usuario
Fecha y hora
Motivo / observación cuando corresponda
```

Ejemplo:

```text
BORRADOR → ENVIADA
Usuario: Juan
Fecha: 03/09/2026 14:32

ENVIADA → DEVUELTA
Usuario: Revisor
Fecha: 04/09/2026 09:15
Motivo: Corregir cantidad del producto X
```

---

## RN-050 — La trazabilidad de autoría es una preocupación transversal, no un campo por entidad

*(Cierra D-11 (Pendiente 16) — cierre documental 2026-09-11. Ver ADR-052.)*

Las operaciones importantes del sistema deben permitir determinar quién realizó la acción, cuándo, qué acción fue, y sobre qué entidad/documento (RN-036/RN-037).

La atribución real de usuario depende del mecanismo de autenticación/autorización del sistema (`Usuario`, `TASK-008`, todavía `PENDIENTE`).

No deben inventarse campos provisionales de usuario (`UsuarioCreacionId`, etc.) en `PedidoProveedor`, `Entrega` o `Factura` únicamente para aparentar trazabilidad antes de que exista autenticación real.

Cuando exista autenticación real, la autoría debe integrarse transversalmente (mismo mecanismo para todas las entidades), no entidad por entidad de forma ad hoc.

## RN-055 — Permiso.Codigo es un identificador funcional global y único

*(Decisión de hardening previa a TASK-013, 2026-09-14, sobre el catálogo de Roles y permisos de TASK-010/011 — no es trazabilidad de autoría, se agrupa aquí para no renumerar las secciones siguientes de este documento, referenciadas por número desde otros archivos. Ver ADR-056.)*

`Permiso.Codigo` es obligatorio y **único en todo el sistema** (no por Empresa — `Permiso` es global, mismo alcance ya confirmado para `Rol` en TASK-010).

La versión inicial de `04-base-datos.md §9.2` (TASK-011) no establecía unicidad de `Codigo`. Se determinó que permitir códigos duplicados produciría ambigüedad cuando `TASK-013` relacione `Rol` con `Permiso` mediante `RolPermiso`: el código debe identificar una capacidad funcional inequívoca para la asignación de permisos, las consultas, la futura autorización y la configuración de roles.

Reforzada mediante restricción `UNIQUE` en SQL Server (`PermisoConfiguration`), además de la validación en `CrearPermisoUseCase` antes de persistir — ambas capas, no solo una.

## RN-056 — Contraseñas: hash obligatorio y longitud mínima

*(Decisión de negocio explícita del usuario, tomada durante la implementación de autenticación — "TASK-015", 2026-09-14. Se agrupa aquí por el mismo motivo que RN-055: no renumerar las secciones siguientes de este documento. Ver ADR-057.)*

`Usuario.PasswordHash` es obligatorio; la contraseña en texto plano **nunca se persiste** — se hashea con un mecanismo estándar de .NET (`PasswordHasher<T>`, PBKDF2) antes de guardar, y nunca se devuelve en ningún DTO de respuesta.

La contraseña debe tener **mínimo 8 caracteres** al crearse (`CrearUsuarioUseCase`). No existe (todavía) ninguna otra política de complejidad — no se inventó una regla adicional que el usuario no pidió explícitamente.

`Usuario.Activo == false` impide autenticarse (`LoginUseCase`) — mismo criterio que el resto del sistema aplica a "denegación por defecto" (`06-seguridad.md §3/§20/§71`), no una regla nueva inventada para este caso.

## RN-057 — Autorización por permiso: fuente única y estado del usuario

*(Decisión tomada durante la implementación de autorización — "TASK-016", 2026-09-14. Se agrupa aquí por el mismo motivo que RN-055/RN-056: no renumerar las secciones siguientes de este documento. Ver ADR-058.)*

La autorización se evalúa exclusivamente mediante `Usuario → UsuarioRol → Rol → RolPermiso → Permiso.Codigo`, consultado dinámicamente en cada operación — no existe una segunda fuente de permisos, no se cachean permisos en el JWT, no se hardcodea qué usuarios poseen qué permisos.

`Usuario.Activo == false` también impide pasar cualquier verificación de autorización, **incluso con un JWT previamente emitido que todavía no haya expirado** — mismo principio de RN-056 (denegación por defecto), aplicado ahora también a autorización, no solo a autenticación. Esto no implementa revocación de tokens: el JWT en sí sigue siendo válido criptográficamente hasta su expiración, pero un usuario inactivo no puede ejercer ningún permiso mientras lo esté.

Los dos primeros endpoints protegidos son `POST /api/v1/requisiciones/{id}/enviar` (requiere `REQUISICION_ENVIAR`) y `POST /api/v1/requisiciones/{id}/aprobar` (requiere `REQUISICION_APROBAR`) — la única correspondencia endpoint↔permiso con algún respaldo documental (`06-seguridad.md §52`, marcado "conceptual") para rutas que existen realmente en código. El resto de los endpoints del sistema permanece sin autorización por permiso hasta que exista un contrato documentado para cada uno (ver `08-tareas.md TASK-049`).

## RN-058 — Alcance por empresa sobre Requisición; actor de negocio desde el JWT

*(Decisión tomada durante la implementación de autorización por alcance — "TASK-050", 2026-09-14. Se agrupa aquí por el mismo motivo que RN-055/056/057: no renumerar las secciones siguientes de este documento. Ver ADR-059.)*

Un usuario solo puede ejecutar `POST /requisiciones/{id}/enviar` y `POST /requisiciones/{id}/aprobar` sobre una `Requisicion` de **su propia Empresa** (`Usuario.Empresa.Id == Requisicion.Empresa.Id`, `06-seguridad.md §11`) — verificado dinámicamente en cada request, sin cachear el alcance en el JWT (mismo principio que RN-057 para permisos). Si la `Requisicion` no existe, el alcance no bloquea: se deja que el propio caso de uso reporte `404`, para no enmascarar "no existe" detrás de "sin autorización".

**Alcance por sede NO se implementa** para estos dos endpoints — **decisión explícita del usuario (2026-09-14), no una omisión**: `Requisicion` no tiene un único `SedeId` (puede distribuirse entre varias sedes vía `DistribucionRequisicion`, `CLAUDE.md §19`), lo que planteaba una ambigüedad real (¿acceso a *todas* las sedes tocadas, o basta con *alguna*?). Presentadas las alternativas, se decidió explícitamente posponer esa dimensión y mantener el alcance únicamente por Empresa. No es una decisión definitiva para siempre — puede revisitarse si el negocio lo requiere.

Para estos dos endpoints, el `UsuarioId` registrado como actor de negocio (`Historial`) ahora proviene del claim `sub` del JWT — **ya no** del header `X-Usuario-Id` — decisión confirmada explícitamente por el usuario, respaldada además por el propio código: `EnviarRequisicionUseCase`/`AprobarRequisicionUseCase` (`TASK-029`/`033`) ya declaraban desde antes de que existiera autenticación real que "`usuarioId` debe provenir de la identidad autenticada, no del cliente". Confiar en `X-Usuario-Id` para el actor de negocio habría permitido que un usuario autenticado y con alcance válido quedara registrado en el `Historial` como una persona distinta. El resto de los endpoints de `Requisicion` (crear, detalles, distribuciones, iniciar-revisión, devolver) sigue usando `X-Usuario-Id` sin cambios — unificarlo es una decisión de alcance mayor, no pedida por esta tarea.

## RN-059 — Catálogo definitivo de permisos y alcance por empresa, extendido a todos los módulos

*(Decisión 2026-09-15, por instrucción explícita del usuario: analizar `docs/` completo, el dominio real y la lógica de negocio, y decidir de una vez el catálogo de permisos que quedaba pendiente desde el cierre de TASK-031/032. Ver ADR-062 para las alternativas consideradas y `06-seguridad.md §9/§52` para el catálogo y el mapeo completos.)*

`06-seguridad.md §9` documentaba el catálogo de permisos como "Ejemplos" desde el diseño original — suficiente para Requisición (ya implementado, RN-057) pero no para los ~50 endpoints que hoy existen sin ninguna autorización por permiso (Empresa, Sede, Usuario, Rol, Permiso, Categoría, UnidadMedida, Producto, Proveedor, Solicitud de producto no catalogado, Periodo, y el resto de Requisición más allá de Enviar/Aprobar). Se decide el catálogo completo y su mapeo a cada endpoint, con dos criterios de negocio verificados contra el dominio real antes de decidir:

1. **Qué módulos comparten un mismo permiso.** Categoria/UnidadMedida se agrupan bajo `PRODUCTO_*` (no tienen ciclo de vida ni responsable propios: son datos de apoyo de `Producto`, `04-base-datos.md §10/§11`, sin ningún RN que les dé un flujo de aprobación independiente). Empresa/Sede se agrupan bajo `ORGANIZACION_*` (Sede es un sub-recurso de Empresa, RN-002, sin flujo propio). Usuario/Rol/Permiso/UsuarioRol/UsuarioSede/RolPermiso se agrupan bajo `SEGURIDAD_*` (todos son configuración de quién-puede-qué, no procesos de negocio distintos entre sí — separarlos violaría `CLAUDE.md §43` sin necesidad real). Proveedor y Periodo no se agrupan con ningún otro módulo: son entidades de negocio independientes con su propio ciclo de vida documentado (`04-base-datos.md §14/§16`).

2. **Qué acción de Solicitud de producto no catalogado usa qué permiso.** `06-seguridad.md §53` (matriz conceptual) muestra explícitamente que el rol Solicitante **no** tiene "Crear producto" — pero `TASK-017`/RN-024 exige que un Solicitante pueda enviar una solicitud de producto no catalogado. Usar `PRODUCTO_CREAR` para *enviar* la solicitud sería darle al Solicitante una capacidad que la propia matriz le niega. Se crea `PRODUCTO_SOLICITAR`, distinto de `PRODUCTO_CREAR`/`PRODUCTO_EDITAR` (usados para *resolver* la solicitud — homologar/crear-producto/rechazar —, acción de un gestor de catálogo).

**Alcance por empresa** (RN-058) se extiende a **todos** los endpoints de Requisición con permiso propio (crear, modificar, ver — incluyendo `GET /requisiciones/{id}`, corrigiendo la implementación de TASK-032 que no lo aplicaba —, enviar, aprobar, devolver, iniciar-revisión): un usuario solo opera sobre requisiciones de su propia empresa, sin excepción. **No** se aplica a Organización/Seguridad (administran empresas por definición, no pueden depender de una) ni a Catálogo/Proveedor/Periodo (son datos globales compartidos por todas las empresas, `06-seguridad.md §11` no exige alcance sobre datos que no pertenecen a ninguna empresa en particular).

**Esto es una decisión, no una implementación.** Sigue pendiente: agregar los `[Authorize]` reales y sembrar los ~20 permisos nuevos en el catálogo (ya existe API real para crearlos, TASK-011). La asignación roles↔permisos se decidió por separado en RN-060 (misma fecha).

## RN-060 — Catálogo definitivo de roles y su asignación de permisos

*(Decisión 2026-09-15, por instrucción explícita del usuario: "revisa primero el catálogo/mapeo documentado, define bien las reglas de negocio, y deja definida la asignación rol↔permiso antes de implementar". Ver ADR-063 para las alternativas consideradas y `06-seguridad.md §8/§53` para el catálogo de roles y la matriz completa.)*

`06-seguridad.md §8` documentaba 5 roles (`SOLICITANTE`, `GESTOR_REQUISICIONES`, `COMPRAS`,
`RECEPCION`, `ADMINISTRADOR`) como "ejemplos conceptuales", y `§53` los cruzaba contra un
subconjunto de acciones (solo Requisición + 4 acciones sueltas de Pedido/Entrega/Factura/Producto)
también marcado "conceptual". Se decide usar estos mismos 5 roles (ninguno se agrega ni se quita
— `CLAUDE.md §6` no autoriza inventar responsabilidades nuevas) y se completa la matriz contra el
catálogo íntegro de RN-059.

**Criterios aplicados, verificados contra la matriz original y el dominio real:**

1. **Se respeta literalmente cada casilla ya marcada en la matriz conceptual original.** Ninguna
   asignación ya decidida (Solicitante crea/modifica/envía pero no aprueba; Gestor aprueba/
   devuelve; Compras crea producto/pedido/factura; Recepción solo registra entrega; Administrador
   tiene todo) se cambió — solo se completó lo que la matriz no cubría.
2. **`GESTOR_REQUISICIONES` recibe `PRODUCTO_CREAR`/`PRODUCTO_EDITAR`** porque la matriz original
   ya le daba "Crear producto" — pese a que su nombre sugiere un alcance más estrecho ("de
   requisiciones"), se preserva la asignación ya decidida en vez de inventar un rol nuevo más
   angosto que nadie pidió.
3. **`COMPRAS` recibe `PROVEEDOR_VER`/`CREAR`/`EDITAR`** — no estaba en la matriz original (no
   existía el módulo Proveedor con API todavía), pero es la extensión más directa del nombre y
   propósito del rol ("Compras" negocia con proveedores) y no compite con ninguna asignación ya
   existente.
4. **`SOLICITANTE`/`GESTOR_REQUISICIONES` reciben `PERIODO_VER`** (necesitan saber qué periodos
   están abiertos para operar) pero **no** `PERIODO_CREAR` — abrir un periodo operativo mensual es
   una decisión administrativa de calendario, no una responsabilidad de quien solicita o revisa
   requisiciones; se asigna solo a `ADMINISTRADOR`.
5. **`ORGANIZACION_VER` se asigna a los 5 roles** — todos necesitan ver su propia empresa/sedes
   para operar (por ejemplo, distribuir una requisición por sede). `ORGANIZACION_ADMINISTRAR`
   (crear/modificar empresas y sedes) y ambos permisos de `SEGURIDAD_*` quedan **exclusivos de
   `ADMINISTRADOR`** — administrar la organización y la seguridad del sistema es, por definición,
   una responsabilidad administrativa (`06-seguridad.md §19` mínimo privilegio).
6. **`ADMINISTRADOR` NO obtiene una excepción de alcance por empresa.** La matriz conceptual
   original mostraba "Ver requisiciones: Amplio" para Administrador, distinto de "Alcance" para
   los demás roles. Se decide **no implementar ese bypass todavía**: `06-seguridad.md §45`
   exige que las excepciones administrativas se definan explícitamente, no se asuman; no existe
   hoy ningún requisito de negocio documentado que obligue a un Administrador a ver
   requisiciones de empresas ajenas a la suya, y el mecanismo actual de alcance (RN-058,
   `AlcanceRequisicionAuthorizationHandler`) no distingue roles — añadir esa distinción sería una
   ampliación de alcance mayor, no pedida. Administrador queda sujeto a la misma regla de alcance
   por empresa que cualquier otro rol. Si el negocio confirma la necesidad de un superadministrador
   sin restricción de empresa, debe registrarse como una decisión nueva que reemplace este punto.
7. **Prevención de escalamiento de privilegios (`06-seguridad.md §62`), regla de negocio explícita
   a implementar junto con el punto 8:** ningún usuario puede asignarse un rol a sí mismo, ni
   asignar/quitar sus propios permisos, aunque tenga `SEGURIDAD_ADMINISTRAR`. `AsignarRolAUsuarioUseCase`
   debe rechazar la operación cuando el usuario que ejecuta la acción (identidad autenticada) es
   el mismo usuario destino de la asignación.

Las filas de Pedido/Entrega/Factura en la matriz (`06-seguridad.md §53`) quedan asignadas para
cuando esas fases se implementen (Fases 7-9, posteriores al MVP) — no se activa ningún
`[Authorize]` para ellas todavía porque los endpoints no existen.

**Esto sigue siendo una decisión, no una implementación.** La siguiente sesión implementa el
punto 8 del bloque A (`progreso.md`) con este catálogo y esta matriz ya definidos, sin más
decisiones de negocio pendientes para autorización del MVP.

> **Actualización (2026-09-17):** los endpoints de Pedido/Entrega/Factura **ya existen**
> (`PedidosProveedorController`/`EntregasController`/`FacturasController`, Fases 7-9), así que la
> condición que justificaba no activar `[Authorize]` para ellos ya no aplica. Ver
> `docs/2026-09-17-tareas.md` Prioridad 1 (P1-1 a P1-4) y `docs/decisiones-pendientes.md` — es un
> hueco de implementación de seguridad ya identificado, no una ambigüedad de negocio nueva.

## RN-061 — Envío de requisición fuera de la ventana de solicitud: bloqueo sin excepción

*(Cierra Pendiente 1 — cierre documental 2026-09-17. Propuesta presentada y aprobada por el
usuario en `docs/2026-09-17-tareas.md §3`. Ver ADR-064.)*

Si una empresa intenta enviar una requisición (`POST /requisiciones/{id}/enviar`) después de que
se cerró la ventana de solicitud del periodo (`EstaDentroDeVentanaDeSolicitud()`, TASK-021), la
operación se **bloquea por completo, sin excepción**. No existe ningún permiso ni rol que permita
enviar fuera de ventana en el alcance actual (ninguna variante de la Alternativa B se implementa
sin una decisión nueva y explícita).

Reabrir la ventana (Alternativa C) tampoco se implementa: contradice el propósito de tener una
ventana en primer lugar (`CLAUDE.md §30`).

**Actualización (verificación 2026-09-17, mismo día, P3-1):** al implementar la guarda se
descubrió que **ya existía** — `Requisicion.Enviar()` (Domain, TASK-029) ya llama
`Periodo.EstaDentroDeVentanaDeSolicitud(fechaEnvio)` y lanza `ReglaDeNegocioException` si falla,
sin ninguna excepción ni bypass, exactamente como decide esta regla. Ya estaba probado
(`RequisicionFlujoTests.No_permite_enviar_fuera_de_la_ventana_del_periodo`, Domain.Tests). No fue
necesario ningún cambio de código — la ambigüedad era sobre si esta era la decisión final, no
sobre código faltante. P3-1 queda cerrada.

## RN-062 — Una requisición en estado APROBADA es inmutable

*(Cierra Pendiente 2 — cierre documental 2026-09-17. Propuesta presentada y aprobada por el
usuario en `docs/2026-09-17-tareas.md §3`. Ver ADR-065.)*

Ninguna operación de edición directa (agregar/modificar/eliminar detalle, modificar
distribución) puede ejecutarse sobre una `Requisicion` en estado `APROBADA`, sin excepción — ni
siquiera para `ADMINISTRADOR` (mismo criterio de RN-060 punto 6: las excepciones administrativas
se definen explícitamente, no se asumen).

Esto protege la trazabilidad de cualquier `Consolidacion` que ya haya derivado datos de esa
requisición (`CLAUDE.md §25`: la consolidación no debe modificar las requisiciones originales, y
tampoco debe quedar expuesta a que su origen cambie por debajo).

Si en el futuro el negocio necesita corregir una requisición ya aprobada, el mecanismo es una
transición de estado explícita y trazable (mismo patrón `ENVIADA → DEVUELTA → CORRECCIÓN →
REENVÍO`, `CLAUDE.md §21`) — **no** edición directa del registro aprobado. Definir ese mecanismo
es una decisión de arquitectura aparte, a plantear solo si aparece la necesidad real.

**Actualización (verificación 2026-09-17, mismo día, P3-2):** al implementar la guarda se
descubrió que **ya existía** — `Requisicion.EsEditable` (Domain, TASK-023..026) ya es `false` para
cualquier estado distinto de `BORRADOR`/`DEVUELTA` (incluida `APROBADA`), y cada operación de
edición (`AgregarDetalle`/`ModificarCantidadDetalle`/`ModificarObservacionDetalle`/
`EliminarDetalle`/`AgregarDistribucion`/`ModificarDistribucion`/`EliminarDistribucion`) llama
`AsegurarEditable()` primero, sin excepción para ningún rol (ni `ADMINISTRADOR`). Se agregó una
prueba dedicada que fija explícitamente el caso `APROBADA`
(`RequisicionFlujoTests.No_permite_agregar_detalle_a_una_requisicion_aprobada`, Domain.Tests) —
antes solo se cubría genéricamente vía el estado `ENVIADA`. No fue necesario ningún cambio de
código de producción — la ambigüedad era sobre si esta era la decisión final, no sobre código
faltante. P3-2 queda cerrada.

## RN-063 — Catálogo de permisos completo para Pedido/Entrega/Factura y su asignación de roles

*(Decisión 2026-09-17, por instrucción explícita del usuario: continuar con P1-4
(`docs/2026-09-17-tareas.md`) para poder cerrar la brecha de seguridad de
`PedidosProveedorController`/`EntregasController`/`FacturasController` sin `[Authorize]`. Ver
ADR-066 para las alternativas consideradas.)*

`06-seguridad.md §9` catalogaba `PEDIDO_VER/CREAR/CONSOLIDAR`, `ENTREGA_VER/REGISTRAR` y
`FACTURA_VER/REGISTRAR` como "fase futura" (RN-059/ADR-062), pero **no cubría** las acciones de
transición de estado que ya existen en código desde el cierre documental 2026-09-11 (D-01 a
D-13): `Enviar`/`Cerrar`/`Cancelar` sobre `PedidoProveedor`, y `Anular` sobre `Entrega`/`Factura`.
Sin esos permisos catalogados, esas acciones no se podían proteger con `[Authorize]` sin inventar
código de seguridad sobre la marcha (`CLAUDE.md §67`).

**Catálogo nuevo** (`06-seguridad.md §9`):

```text
PEDIDO_ENVIAR
PEDIDO_CERRAR
PEDIDO_CANCELAR

ENTREGA_ANULAR

FACTURA_ANULAR
```

**Asignación de roles** — mismo criterio ya usado en RN-060 ("se preserva la asignación ya
decidida" para el permiso base de la misma entidad, sin inventar un rol nuevo ni un matiz que
nadie pidió):

- `PEDIDO_ENVIAR`/`PEDIDO_CERRAR`/`PEDIDO_CANCELAR` → mismos roles que `PEDIDO_CREAR`
  (`COMPRAS`, `ADMINISTRADOR`) — Compras es dueño del ciclo de vida completo de `PedidoProveedor`
  en la matriz ya decidida (crea, consolida; ahora también envía, cierra y cancela el mismo
  documento que crea).
- `ENTREGA_ANULAR` → mismos roles que `ENTREGA_REGISTRAR` (`RECEPCION`, `ADMINISTRADOR`) — quien
  registra una entrega es quien puede anularla, simetría directa.
- `FACTURA_ANULAR` → mismos roles que `FACTURA_REGISTRAR` (`COMPRAS`, `ADMINISTRADOR`) — misma
  simetría registrar/anular que `ENTREGA_ANULAR`.

**Endpoint → permiso → alcance** (extiende `06-seguridad.md §52`, que hoy no cubre ningún
endpoint de Pedido/Entrega/Factura):

| Endpoint | Permiso |
| --- | --- |
| `POST /pedidos-proveedor`, `POST .../detalles`, `POST .../detalles/{id}/distribuciones` | `PEDIDO_CREAR` |
| `POST /pedidos-proveedor/{id}/enviar` | `PEDIDO_ENVIAR` |
| `POST /pedidos-proveedor/{id}/cerrar` | `PEDIDO_CERRAR` |
| `POST /pedidos-proveedor/{id}/cancelar` | `PEDIDO_CANCELAR` |
| `POST /pedidos-proveedor/{id}/entregas` (crea una `Entrega`, aunque la ruta cuelgue de Pedido) | `ENTREGA_REGISTRAR` |
| `POST /entregas/{id}/detalles`, `POST .../detalles/{id}/distribuciones` | `ENTREGA_REGISTRAR` |
| `POST /entregas/{id}/anular` | `ENTREGA_ANULAR` |
| `POST /facturas`, `POST /facturas/{id}/detalles` | `FACTURA_REGISTRAR` |
| `POST /facturas/{id}/anular` | `FACTURA_ANULAR` |

**Sin alcance por empresa** en ninguno de estos endpoints — mismo motivo ya documentado en
`CLAUDE.md §27`: un `PedidoProveedor` puede consolidar necesidades de varias empresas, no
pertenece a una sola (a diferencia de `Requisicion`).

**Esto es una decisión de negocio cerrada, implementada en el mismo cambio** (a diferencia de
RN-061/RN-062): agrega `[Authorize]` real a los tres controllers usando este catálogo y este
mapeo. Ver `docs/2026-09-17-tareas.md` P1.

## RN-064 — Consulta de Consolidación usa el mismo permiso que Pedido

*(Decisión 2026-09-17, incremento "Fase 6-9 en Frontend" —
`docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`, Ambigüedad 1. Ver ADR-067.)*

Los endpoints de lectura de `Consolidacion` (`GET /api/v1/consolidaciones`,
`GET /api/v1/consolidaciones/{id}`) usan el permiso `PEDIDO_VER` — no se cataloga un permiso
`CONSOLIDACION_VER` propio. Consolidación no tiene un ciclo de vida ni una responsabilidad
distinta de Pedido (es el paso inmediatamente anterior en la misma cadena
Consolidación→Pedido→Entrega→Factura, siempre operada por el mismo rol `COMPRAS`); crear un
permiso nuevo repetiría exactamente el criterio ya descartado en RN-059 para
Categoría/UnidadMedida (agrupar bajo el permiso del módulo relacionado cuando no hay necesidad de
negocio documentada que exija separarlos). Sin alcance por empresa, mismo motivo que RN-063
(`CLAUDE.md §27`).

## RN-065 — Distribución de PedidoProveedor debe estar completa antes de enviar

*(Decisión 2026-09-17, incremento "Fase 6-9 en Frontend" —
`docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`, Ambigüedad 2, confirmada explícitamente
por el usuario. Ver ADR-068.)*

Un `PedidoProveedor` no puede pasar de `BORRADOR` a `ENVIADO` (`PedidoProveedor.Enviar()`) si la
suma de `DistribucionPedido` de algún `DetallePedidoProveedor` no es igual a su
`CantidadPedida` — mismo criterio que `Requisicion.DistribucionCompleta` (RN-011), aplicado ahora
también a Pedido. No se permite enviar con distribución parcial o sin distribuir.

**Actualización (implementación 2026-09-17, I2-1):** ya implementada —
`DetallePedidoProveedor.DistribucionCompleta` (mismo criterio que
`DetalleRequisicion.DistribucionCompleta`) y la guarda correspondiente en
`PedidoProveedor.Enviar()`. Probado en Domain.Tests
(`No_permite_enviar_un_pedido_con_distribucion_incompleta`/
`Permite_enviar_un_pedido_con_distribucion_completa`).

---

# 15. Reglas todavía pendientes de definición

Las siguientes decisiones requieren validación antes de convertirse en reglas definitivas:

### Pendiente 1 — Fechas fuera de la ventana

¿Qué ocurre si una empresa intenta enviar una requisición después del cierre?

Posibles alternativas:

```text
A. Bloquear completamente.
B. Permitir excepción a un rol autorizado.
C. Abrir nuevamente la ventana.
```

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-17)** — ver RN-061 (D-14). Se bloquea completamente el envío fuera de ventana, sin excepción, para el alcance actual. Decidida conceptualmente; implementación pendiente de tarea futura (P3-1).

---

### Pendiente 2 — Modificación después de aprobación

Debe definirse si una requisición aprobada puede ser modificada y bajo qué condiciones.

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-17)** — ver RN-062 (D-15). `APROBADA` es inmutable para edición directa, sin excepción. Decidida conceptualmente; implementación pendiente de tarea futura (P3-2).

---

### Pendiente 3 — Facturas

Debe definirse la relación real entre:

```text
Pedido
Factura
```

Por ejemplo, si:

```text
1 Pedido → 1 Factura
```

o:

```text
1 Pedido → varias Facturas
```

o:

```text
Varias órdenes → 1 Factura
```

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-039/RN-047 (D-06). Se confirma `1 Pedido → N Facturas` para el alcance actual; `Varias órdenes → 1 Factura` queda descartado para este alcance (no se implementará salvo necesidad de negocio explícita).

---

### Pendiente 4 — Recepción de mercancía

Debe definirse si el sistema inicialmente manejará únicamente:

```text
Entregado / no entregado
```

o si posteriormente manejará:

```text
Recibido
Aceptado
Rechazado
Faltante
Dañado
Sustituido
```

**Estado:** ~~Pendiente~~ **RESUELTA parcialmente (cierre documental 2026-09-11)** — ver RN-046 (D-04). El estado del agregado `Entrega` se limita a `REGISTRADA`/`ANULADA`; las condiciones detalladas de recepción (`Aceptado`/`Rechazado`/`Faltante`/`Dañado`/`Sustituido`) quedan explícitamente diferidas como concepto aparte, a modelar solo si el negocio lo requiere — no se descartan, pero no se implementan ahora.

---

### Pendiente 5 — Sustitución de productos

Debe definirse qué ocurre cuando el proveedor no dispone del producto solicitado y propone un producto equivalente.

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-045 (D-13). La sustitución debe ser explícita y trazable (producto original, sustituto, motivo, usuario, fecha). Decidida conceptualmente; implementación pendiente de tarea futura.

---

### Pendiente 6 — Cantidades superiores o inferiores

Debe definirse quién puede autorizar diferencias entre:

```text
Cantidad solicitada
Cantidad necesaria
Cantidad pedida
Cantidad entregada
```

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-042 (D-02/D-12). Compras define `CantidadPedida` directamente, sin un flujo de autorización separado; la diferencia con `CantidadNecesaria` se conserva pero no requiere aprobación adicional documentada en esta etapa. La diferencia respecto a `CantidadEntregada`/`CantidadFacturada` sigue las reglas RN-034/RN-048 (topes, no autorización).

---

### Pendiente 7 — Selección del proveedor por producto *(agregada en auditoría 2026-09-10)*

Un producto puede tener más de un proveedor asociado (`ProductoProveedor`, `02-dominio.md §11`: `Producto N ─── N Proveedor`). RN-030/ADR-013 establecen que una consolidación puede generar varios pedidos, pero ningún documento define el criterio para decidir a qué proveedor se asigna cada `DetalleConsolidacion` al crear un `PedidoProveedor`:

```text
¿Selección manual por el usuario de Compras?
¿Criterio automático (precio, disponibilidad, proveedor preferido)?
¿Combinación de ambos?
```

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-041 (D-01). Selección manual y explícita por el usuario de Compras; sin criterio automático mientras no exista una regla empresarial que lo justifique.

---

### Pendiente 8 — Empaques, presentaciones y mínimos de compra *(agregada en auditoría 2026-09-10)*

`04-base-datos.md §27` y `00-contexto-proyecto.md §7.3` mencionan empaques, presentaciones y mínimos de compra únicamente como ejemplos de **por qué** `CantidadNecesaria` puede diferir de `CantidadPedida`. Ningún documento define un campo, regla o cálculo para representarlos (ni en `ProductoProveedor` ni en `DetallePedidoProveedor`).

```text
¿Debe modelarse UnidadesPorEmpaque / CantidadMinimaCompra en ProductoProveedor?
¿O el ajuste de CantidadPedida es puramente manual, sin datos estructurados de soporte?
```

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-042 (D-02). El ajuste es puramente manual por Compras, sin datos estructurados de soporte (`UnidadesPorEmpaque`/`CantidadMinimaCompra`) en esta etapa.

---

### Pendiente 9 — Creación de PedidoProveedor y origen de NumeroPedido *(agregada en auditoría 2026-09-10)*

`04-base-datos.md §26` define el campo `NumeroPedido` en `PedidoProveedor`, pero no indica su origen ni cómo se selecciona qué consolidar:

```text
¿NumeroPedido es autogenerado por el sistema o ingresado manualmente
(reflejando el número real de orden de compra del proveedor)?

¿La creación de un PedidoProveedor incluye automáticamente todos los
DetalleConsolidacion pendientes de un producto/proveedor, o el usuario
selecciona manualmente cuáles incluir y en qué cantidad (CantidadPedida)?
```

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-041/RN-042/RN-049 (D-01/D-09). `NumeroPedido` se recibe explícito del llamador (no autogenerado por el sistema en esta etapa); la creación de un `PedidoProveedor` requiere selección manual de qué incluir y en qué `CantidadPedida` (no inclusión automática de todo lo pendiente).

---

### Pendiente 10 — Estados de PedidoProveedor *(agregada en refinamiento 2026-09-11)*

`PedidoProveedor.Estado` es texto libre (`04-base-datos.md §26` no enumera valores). No hay transiciones definidas; `confirmar`/`cancelar` (`05-api.md §32`) están marcados ahí mismo como "conceptuales".

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-043/RN-044 (D-03/D-08). Ciclo `BORRADOR → ENVIADO → PARCIALMENTE_ENTREGADO → ENTREGADO → CERRADO`, con `CANCELADO` desde los tres primeros. Implementación (enum + transiciones + API) queda pendiente como tarea aparte.

---

### Pendiente 11 — Estados de Entrega *(agregada en refinamiento 2026-09-11)*

`Entrega.Estado` es texto libre (`04-base-datos.md §29` no enumera valores). Relacionada con Pendiente 4 (recepción de mercancía), pero distinta: Pendiente 4 es sobre qué información de recepción se maneja (aceptado/rechazado/dañado); esta es sobre el ciclo de vida administrativo de la `Entrega` como registro (ej. ¿puede anularse una entrega ya registrada?).

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-046 (D-04). Estados: `REGISTRADA`/`ANULADA`.

---

### Pendiente 12 — Estados de Factura *(agregada en refinamiento 2026-09-11)*

`Factura.Estado` es texto libre. El 2026-09-10 se redactó una propuesta de ciclo de vida (`progreso.md`, entrada "propuesta de negocio FASE 9") marcada explícitamente `PROPUESTA — NO APROBADA`. Sigue sin aprobarse.

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-047 (D-05). Estados: `REGISTRADA`/`ANULADA` (versión simplificada; la propuesta original de múltiples estados contables queda descartada por RN-038/RN-047, no aprobada tal cual).

---

### Pendiente 13 — Tope de CantidadFacturada *(agregada en refinamiento 2026-09-11)*

La implementación actual limita `CantidadFacturada` acumulada a `CantidadPedida` (mismo patrón que `Entrega`), por analogía técnica, sin regla de negocio propia documentada. Alternativa real: limitar a `CantidadEntregada` (no facturar lo que no se ha recibido físicamente) — pero eso acoplaría Factura a Entrega, lo cual RN-040 mantiene deliberadamente separado.

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-048 (D-10). Se confirma el tope contra `CantidadPedida` (no contra `CantidadEntregada`), consistente con RN-040. Ya implementado tal cual (`Factura.AgregarDetalle`).

---

### Pendiente 14 — Origen y unicidad de NumeroPedido / NumeroRemision / NumeroFactura *(agregada en refinamiento 2026-09-11; NumeroPedido ya cubierto parcialmente por Pendiente 9)*

Ningún documento define si estos números son autogenerados por el sistema o corresponden al número real asignado por el proveedor/transportador, ni si deben ser únicos. Hoy los tres son texto libre sin restricción de unicidad.

**Estado:** ~~Pendiente~~ **RESUELTA e IMPLEMENTADA (cierre documental 2026-09-11)** — ver RN-049 (D-09). Los tres son ingresados (no autogenerados por el sistema); unicidad: `NumeroPedido`/`NumeroFactura` únicos por proveedor, `NumeroRemision` único por pedido.

---

### Pendiente 15 — Qué significa el "cierre" de una compra *(agregada en refinamiento 2026-09-11)*

El flujo conceptual termina en `Cierre` (`00-contexto-proyecto.md §4`), pero ningún documento define las condiciones para considerar cerrado un ciclo `Pedido → Entrega → Factura` (¿todo entregado?, ¿todo facturado?, ¿ambos?, ¿aprobación manual?). Corresponde a `TASK-047` (`08-tareas.md`), no iniciada.

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-044 (D-08). Cierre = cantidad pendiente de entrega igual a cero + acción explícita de un usuario autorizado. La factura NO es requisito. `TASK-047` sigue sin implementarse.

---

### Pendiente 16 — Trazabilidad de autoría (UsuarioCreacionId) *(agregada en refinamiento 2026-09-11)*

RN-036/RN-037 exigen registrar usuario y fecha en acciones relevantes, pero `PedidoProveedor`, `Entrega` y `Factura` no tienen todavía un campo de usuario creador — depende de que `Usuario` exista en Domain (`TASK-008`, `PENDIENTE`).

**Estado:** ~~Pendiente~~ **RESUELTA (cierre documental 2026-09-11)** — ver RN-050 (D-11). No se agregan campos provisionales de usuario; la autoría se integrará transversalmente cuando exista autenticación real (`TASK-008` + Fase 10 Seguridad).

---

## Matriz consolidada de decisiones pendientes

*(Cierre documental 2026-09-11: D-01 a D-13 quedan `DECISIÓN CERRADA`. "Cerrada" significa que la regla de negocio está definida y documentada — no que ya esté implementada en código. Ver `§15.1` para lo que falta implementar y el reporte de la tarea para las contradicciones código↔documentación detectadas.)*

| ID | Decisión | Impacto | Cierra en | Estado |
|---|---|---|---|---|
| D-01 | Selección manual de proveedor por Compras | PedidoProveedor | RN-041, ADR-044 | DECISIÓN CERRADA |
| D-02 | Sin cálculo automático de empaques/mínimos; CantidadPedida manual | PedidoProveedor, DetallePedidoProveedor | RN-042, ADR-045 | DECISIÓN CERRADA |
| D-03 | Ciclo de estados de PedidoProveedor definido | PedidoProveedor / API | RN-043, ADR-046 | DECISIÓN CERRADA |
| D-04 | Estados de Entrega definidos | Entrega / API | RN-046, ADR-049 | DECISIÓN CERRADA |
| D-05 | Estados de Factura definidos | Factura / API | RN-047, ADR-050 | DECISIÓN CERRADA |
| D-06 | PedidoProveedor 1:N Facturas | Factura | RN-039, ADR-042 | DECISIÓN CERRADA |
| D-07 | Entrega y Factura independientes | Facturación | RN-040, ADR-043 | DECISIÓN CERRADA |
| D-08 | Cierre explícito después de entrega completa | PedidoProveedor | RN-044, ADR-047 | DECISIÓN CERRADA |
| D-09 | Identificadores documentales y unicidad contextual | Trazabilidad | RN-049, ADR-051 | DECISIÓN CERRADA |
| D-10 | Facturación acumulada ≤ cantidad pedida | Factura | RN-048, ADR-050 | DECISIÓN CERRADA |
| D-11 | Trazabilidad de autoría transversal | Trazabilidad | RN-050, ADR-052 | DECISIÓN CERRADA |
| D-12 | CantidadPedida independiente de CantidadNecesaria | PedidoProveedor | RN-031, RN-042, ADR-045 | DECISIÓN CERRADA |
| D-13 | Sustitución explícita y trazable | PedidoProveedor/Entrega | RN-045, ADR-048 | DECISIÓN CERRADA (implementación pendiente) |
| D-14 | Envío fuera de ventana: bloqueo sin excepción | Requisicion | RN-061, ADR-064 | DECISIÓN CERRADA E IMPLEMENTADA (ya existía, verificado 2026-09-17) |
| D-15 | Requisición APROBADA es inmutable | Requisicion | RN-062, ADR-065 | DECISIÓN CERRADA E IMPLEMENTADA (ya existía, verificado 2026-09-17) |
| D-16 | Catálogo de permisos Enviar/Cerrar/Cancelar/Anular + asignación de roles | PedidoProveedor/Entrega/Factura | RN-063, ADR-066 | DECISIÓN CERRADA E IMPLEMENTADA |
| D-17 | Consulta de Consolidación usa PEDIDO_VER, sin permiso propio | Consolidacion | RN-064, ADR-067 | DECISIÓN CERRADA (implementación pendiente) |
| D-18 | Distribución de Pedido debe estar completa antes de enviar | PedidoProveedor | RN-065, ADR-068 | DECISIÓN CERRADA E IMPLEMENTADA |

Todas las decisiones D-01 a D-18 quedan cerradas a nivel de regla de negocio.

> **Actualización (2026-09-11, cierre técnico del flujo PedidoProveedor):** D-01 a D-12 ya están implementadas en código (Domain/Application/Infrastructure/Api), incluyendo `Anular()` para `Entrega`/`Factura` con sus casos de uso y endpoints, con 2 migraciones aplicadas y pruebas (254/254). Ver `progreso.md` para el detalle. Solo D-13 (sustitución de productos) permanece sin implementar, tal como estaba decidido (decisión de negocio cerrada, mecanismo de registro pendiente de una tarea futura).

### 15.1 Qué queda pendiente de implementación

- Mecanismo de registro de sustitución de productos (RN-045/D-13) — decidido conceptualmente, sin entidad/caso de uso todavía. **Fuera de alcance mientras D-13 no se implemente explícitamente.**
- `PedidosProveedorController`/`EntregasController`/`FacturasController`: cada uno implementa únicamente las acciones respaldadas por un caso de uso y una decisión cerrada. No incluyen `GET` (listar/consultar), `PUT`, ni creación de `Entrega` por HTTP (sin caso de uso ni contrato definido todavía).
- Trazabilidad de autoría real (D-11) — depende de `Usuario`/autenticación (`TASK-008`, `PENDIENTE`). No es una tarea de código pendiente de "conectar": falta el componente `Usuario` en sí. Ver `08-tareas.md`, `TASK-008`.
- ~~Guarda de ventana de solicitud~~ / ~~Guarda de inmutabilidad de `APROBADA`~~ (RN-061/RN-062,
  D-14/D-15) — **ya implementadas**, verificado 2026-09-17 (P3-1/P3-2 cerradas sin cambio de
  código, ver notas de RN-061/RN-062 arriba).

> **Nota (2026-09-17):** la actualización de arriba (línea "Actualización (2026-09-17)" antes de RN-061) también deja constancia de que `PedidosProveedorController`/`EntregasController`/`FacturasController` **ya existen**, pero sin `[Authorize]` — eso es un hueco de seguridad (P1, `docs/2026-09-17-tareas.md`), no una decisión de negocio pendiente.

---

# 16. Regla para Claude Code

Claude Code debe tratar las reglas `RN-XXX` de este documento como reglas de negocio.

Cuando implemente una funcionalidad:

1. Debe identificar qué reglas de negocio aplica.
2. Debe implementar las validaciones necesarias.
3. Debe crear pruebas que demuestren el cumplimiento de las reglas.
4. No debe cambiar una regla sin autorización.
5. Si una regla es ambigua, debe informarlo antes de implementar una solución que afecte el dominio.

Una regla de negocio no debe ser modificada simplemente porque resulte más fácil de implementar.

---

# 17. Relación con otros documentos

Este documento define **qué debe cumplirse en el negocio**.

No define todavía:

- La estructura exacta de las tablas.
- Las clases C#.
- Los endpoints definitivos.
- La estructura de React.
- La arquitectura de proyectos.
- Las tecnologías adicionales.
- Las decisiones de infraestructura.

Esas decisiones corresponden a:

```text
02-dominio.md
03-arquitectura.md
04-base-datos.md
05-api.md
06-seguridad.md
07-decisiones-arquitectura.md
08-tareas.md
```

---

# 18. Regla fundamental

> **Una decisión técnica nunca debe cambiar silenciosamente una regla de negocio.**

Si una implementación requiere modificar una regla existente, la modificación debe ser identificada, discutida y aprobada antes de implementarse.
