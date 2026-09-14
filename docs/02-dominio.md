# DOMINIO DEL SISTEMA

## Sistema de Gestión de Requisiciones y Pedidos - Auropaq

**Proyecto:** AuropaqPedidos
**Documento:** Modelo de dominio
**Código:** DOM
**Estado:** En definición
**Última actualización:** 2026-09-08

---

# 1. Propósito

Este documento define el dominio funcional de AuropaqPedidos.

Su objetivo es establecer:

- Qué conceptos existen en el negocio.
- Qué responsabilidad tiene cada concepto.
- Cómo se relacionan.
- Qué cardinalidades existen.
- Qué información representa cada concepto.
- Qué estados existen.
- Cómo fluye la información entre los diferentes procesos.

Este documento representa el dominio **antes de convertirlo en una implementación técnica**.

No debe interpretarse todavía como un modelo físico de base de datos.

---

# 2. Principio fundamental

El sistema debe diseñarse alrededor del negocio y no alrededor de la tecnología.

La secuencia de diseño será:

```text
Problema real
     ↓
Conceptos del negocio
     ↓
Responsabilidades
     ↓
Relaciones
     ↓
Cardinalidades
     ↓
Reglas de negocio
     ↓
Estados
     ↓
Casos de uso
     ↓
Modelo de datos
     ↓
API
     ↓
Implementación
```

No se deben crear entidades únicamente porque sean necesarias para una pantalla.

Una entidad debe existir porque representa un concepto o responsabilidad real del dominio.

---

# 3. Áreas principales del dominio

El dominio se divide conceptualmente en las siguientes áreas:

```text
┌──────────────────────────────┐
│ ORGANIZACIÓN                 │
│ Empresa                      │
│ Sede                         │
│ Usuario                      │
│ Rol                          │
│ Permiso                      │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ CATÁLOGO                     │
│ Categoría                    │
│ Unidad de medida             │
│ Producto                     │
│ Proveedor                    │
│ Producto-Proveedor           │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ REQUISICIONES                │
│ Periodo                      │
│ Requisición                  │
│ Detalle requisición          │
│ Distribución por sede        │
│ Producto no catalogado       │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ COMPRAS                      │
│ Consolidación                │
│ Detalle consolidación        │
│ Asignación consolidación     │
│ Pedido proveedor             │
│ Detalle pedido proveedor     │
│ Distribución pedido          │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│ ENTREGA                      │
│ Entrega                      │
│ Detalle entrega              │
│ Distribución entrega         │
│ Factura                      │
└──────────────────────────────┘
```

---

# 4. Contexto de organización

## 4.1 Empresa

### Responsabilidad

Representa una empresa perteneciente al holding.

Una empresa es propietaria de sus sedes y es el contexto organizacional principal de las requisiciones.

### Relaciones

```text
Empresa 1 ─────── N Sede
Empresa 1 ─────── N Usuario
Empresa 1 ─────── N Requisición
```

### Reglas relevantes

- Una empresa puede tener múltiples sedes.
- Una empresa puede tener múltiples usuarios.
- Una empresa puede tener una requisición por periodo.
- Una empresa no debe estar hardcodeada.

---

# 5. Sede

### Responsabilidad

Representa una ubicación física perteneciente a una empresa.

La sede determina un posible destino físico para los productos.

### Relaciones

```text
Empresa 1 ─────── N Sede
Sede 1 ─────── N DistribuciónRequisición
Sede 1 ─────── N DistribuciónPedido
Sede 1 ─────── N DistribuciónEntrega
```

### Información conceptual

Una sede puede tener:

```text
Identificador
Nombre
Dirección
Ciudad
Información de contacto
Estado
```

La estructura exacta será definida posteriormente.

### Regla histórica

La dirección actual de una sede puede cambiar.

Por este motivo, las operaciones históricas de entrega no deben depender exclusivamente de la dirección actual de la entidad `Sede`.

La entrega debe conservar una fotografía histórica de la información utilizada como destino.

---

# 6. Usuarios, roles y permisos

## 6.1 Usuario

### Responsabilidad

Representa una persona que interactúa con el sistema.

El usuario no define por sí mismo qué puede hacer.

Sus capacidades están determinadas por roles y permisos.

### Credenciales

*(Confirmado en la implementación de autenticación, "TASK-015", 2026-09-14 — ver RN-056/ADR-057.)*

`Usuario` almacena `PasswordHash` (nunca la contraseña en texto plano; se hashea con un mecanismo estándar de .NET antes de persistir). El dominio no conoce el algoritmo de hash concreto — eso es responsabilidad de Infrastructure; Domain solo garantiza que exista un hash no vacío, igual que `Correo`/`Nombre`.

---

## 6.2 Rol

### Responsabilidad

Representa una responsabilidad funcional dentro del sistema.

Ejemplos conceptuales:

```text
Solicitante
Revisor
Compras
Administrador
```

Estos nombres son ejemplos y pueden cambiar cuando se defina formalmente seguridad.

No deben asociarse a nombres de personas.

### Alcance

*(Confirmado en la implementación de TASK-010, 2026-09-14: `04-base-datos.md §9.1` no declara `EmpresaId` para `Rol`.)*

`Rol` es **GLOBAL**: no pertenece a ninguna `Empresa`. Un mismo Rol puede asignarse a usuarios de cualquier empresa mediante `UsuarioRol`.

---

## 6.3 Permiso

### Responsabilidad

Representa una acción que puede ejecutar un usuario.

Ejemplos:

```text
REQUISICION_CREAR
REQUISICION_VER
REQUISICION_MODIFICAR
REQUISICION_ENVIAR
REQUISICION_APROBAR
REQUISICION_DEVOLVER
PRODUCTO_CREAR
PEDIDO_CREAR
ENTREGA_REGISTRAR
```

La lista definitiva será definida en `06-seguridad.md`.

### Alcance e identidad

*(Confirmado en la implementación de TASK-011, 2026-09-14; unicidad cerrada como decisión de hardening previa a TASK-013 — ver RN-055/ADR-056.)*

`Permiso` es **GLOBAL** (sin `EmpresaId`, igual que `Rol`). `Permiso.Codigo` es obligatorio y **único en todo el sistema** — identifica de forma inequívoca la capacidad funcional que `TASK-013` relacionará con `Rol` mediante `RolPermiso`.

---

## 6.4 Diferencia entre permiso y alcance

El dominio debe diferenciar:

```text
PERMISO
¿Qué puede hacer?
```

de:

```text
ALCANCE
¿Sobre qué información puede hacerlo?
```

Ejemplo:

```text
Usuario:
Puede APROBAR_REQUISICION

Alcance:
Empresas que tiene autorizadas para revisar
```

Esto permite evitar que la autorización dependa de nombres o condiciones hardcodeadas.

---

## 6.5 UsuarioSede

*(Confirmado en la implementación de TASK-009, 2026-09-14; auditado en TASK-014.)*

### Responsabilidad

Representa que un `Usuario` tiene acceso para operar sobre una `Sede` concreta.

### Relación

```text
Usuario N ─── N Sede
```

mediante `UsuarioSede` (`04-base-datos.md §8`): identidad natural `(UsuarioId, SedeId)`, sin `Id` propio.

### Regla de aislamiento

Un `Usuario` solo puede tener acceso a `Sede` de su propia `Empresa` — `Usuario.Empresa` debe coincidir con `Sede.Empresa` (mismo criterio de aislamiento multiempresa ya aplicado en `Requisicion`).

---

## 6.6 UsuarioRol

*(Confirmado en la implementación de TASK-010, 2026-09-14 — alcance original de TASK-012, adelantado por instrucción explícita del usuario; auditado en TASK-014.)*

### Responsabilidad

Representa que un `Usuario` tiene asignado un `Rol`.

### Relación

```text
Usuario N ─── N Rol
```

mediante `UsuarioRol` (`04-base-datos.md §9.3`): identidad natural `(UsuarioId, RolId)`, sin `Id` propio.

A diferencia de `UsuarioSede`, esta relación **no valida pertenencia a la misma empresa**: `Rol` es GLOBAL (`§6.2`), así que no existe ninguna `Empresa` que comparar.

---

## 6.7 RolPermiso

*(Confirmado en la implementación de TASK-013, 2026-09-14; auditado en TASK-014.)*

### Responsabilidad

Representa que un `Rol` tiene asignado un `Permiso`.

### Relación

```text
Rol N ─── N Permiso
```

mediante `RolPermiso` (`04-base-datos.md §9.4`): identidad natural `(RolId, PermisoId)`, sin `Id` propio.

Mismo criterio que `UsuarioRol`: sin validación de empresa, porque `Rol` y `Permiso` son ambos GLOBALES.

Con esta relación queda cerrado estructuralmente el modelo de datos de autorización:

```text
Usuario → UsuarioRol → Rol → RolPermiso → Permiso
```

La autorización real (usar este modelo para decidir si una operación está permitida) es una responsabilidad de una fase posterior — ver `§6.4`.

---

# 7. Catálogo

## 7.1 Categoría

### Responsabilidad

Clasifica los productos dentro del catálogo.

Ejemplos:

```text
Aseo
Cafetería
Papelería
Botiquín
```

Una categoría puede contener múltiples productos.

```text
Categoría 1 ─────── N Producto
```

---

# 8. Unidad de medida

### Responsabilidad

Representa la unidad utilizada para expresar cantidades.

Ejemplos:

```text
Unidad
Caja
Galón
Paquete
Kilogramo
Litro
```

Una unidad puede ser utilizada por múltiples productos.

```text
UnidadMedida 1 ─────── N Producto
```

La lista definitiva debe mantenerse como catálogo configurable.

---

# 9. Producto

### Responsabilidad

Representa un producto oficial del catálogo interno.

Información conceptual:

```text
Código interno
Nombre
Categoría
Unidad de medida
Estado
```

### Relaciones

```text
Categoría 1 ─────── N Producto

UnidadMedida 1 ─────── N Producto

Producto 1 ─────── N ProductoProveedor

Producto 1 ─────── N DetalleRequisición
```

El producto interno es independiente del código que utilice cada proveedor.

---

# 10. Proveedor

### Responsabilidad

Representa una entidad externa que suministra productos.

Un proveedor puede ofrecer múltiples productos.

```text
Proveedor 1 ─────── N ProductoProveedor
```

---

# 11. ProductoProveedor

### Responsabilidad

Representa la relación entre un producto interno y la referencia utilizada por un proveedor.

Esta relación permite resolver el problema de homologación entre:

```text
Producto interno
        ↓
Código interno
        ↓
Proveedor
        ↓
Código del proveedor
        ↓
Categoría / unidad / información del proveedor
```

### Cardinalidad

```text
Producto 1 ─────── N ProductoProveedor
Proveedor 1 ─────── N ProductoProveedor
```

Por lo tanto:

```text
Producto N ─────── N Proveedor
```

se implementa conceptualmente mediante `ProductoProveedor`.

---

# 12. Periodo

### Responsabilidad

Representa el periodo operativo mensual.

Información conceptual:

```text
Año
Mes
Fecha inicio de ventana de solicitud
Fecha fin de ventana de solicitud
Estado
```

### Relaciones

```text
Periodo 1 ─────── N Requisición
```

Sin embargo, existe una regla adicional:

```text
Empresa + Periodo = máximo una Requisición
```

---

# 13. Requisición

### Responsabilidad

Representa la necesidad interna de una empresa durante un periodo.

Una requisición:

- pertenece a una empresa;
- pertenece a un periodo;
- contiene detalles;
- tiene un estado;
- tiene historial;
- puede distribuir cantidades entre sedes.

### Relaciones

```text
Empresa 1 ─────── N Requisición

Periodo 1 ─────── N Requisición

Requisición 1 ─────── N DetalleRequisición
```

### Restricción

```text
Empresa + Periodo
        ↓
Máximo 1 Requisición
```

---

# 14. DetalleRequisición

### Responsabilidad

Representa un producto específico solicitado dentro de una requisición.

Información conceptual:

```text
Producto
Cantidad solicitada
Observación
```

### Relaciones

```text
Requisición 1 ─────── N DetalleRequisición

Producto 1 ─────── N DetalleRequisición

DetalleRequisición 1 ─────── N DistribuciónRequisición
```

---

# 15. DistribuciónRequisición

### Responsabilidad

Representa cómo se distribuye la cantidad solicitada entre las sedes de la empresa.

Ejemplo:

```text
Detalle:
Papel higiénico = 100

Distribución:

Bogotá = 60
Medellín = 40
```

### Relaciones

```text
DetalleRequisición 1 ─────── N DistribuciónRequisición

Sede 1 ─────── N DistribuciónRequisición
```

### Regla principal

```text
SUM(Distribuciones)
=
CantidadSolicitada
```

Las sedes utilizadas deben pertenecer a la empresa de la requisición.

---

# 16. Producto no catalogado

### Responsabilidad

Representa una solicitud de un producto que todavía no existe en el catálogo oficial.

No debe convertirse automáticamente en `Producto`.

### Flujo conceptual

```text
Solicitud
   ↓
Producto no catalogado
   ↓
Revisión
   ├── Homologar producto existente
   ├── Crear producto oficial
   └── Rechazar
```

### Objetivo

Evitar que los usuarios creen directamente productos oficiales con información incorrecta o duplicada.

La información original solicitada debe conservarse.

---

# 17. Estados de la requisición

La requisición tiene un ciclo de vida.

Estado conceptual:

```text
BORRADOR
   ↓
ENVIADA
   ↓
EN_REVISION
   ├──────────────→ DEVUELTA
   │                    ↓
   │                 CORRECCIÓN
   │                    ↓
   │                 ENVIADA
   │
   └──────────────→ APROBADA
```

Posteriormente:

```text
APROBADA
   ↓
CONSOLIDACIÓN
```

Los estados exactos y las transiciones permitidas deben definirse formalmente antes de implementar el workflow.

---

# 18. Consolidación

## 18.1 Consolidación

### Responsabilidad

Representa una agrupación de necesidades aprobadas.

Su propósito es determinar cuánto se necesita comprar de cada producto.

### Relaciones

```text
Periodo 1 ─────── N Consolidación

Consolidación 1 ─────── N DetalleConsolidación
```

---

# 19. DetalleConsolidación

### Responsabilidad

Representa la cantidad consolidada de un producto.

Ejemplo:

```text
Producto A

AUROTECH       30
FAVIPAQ        20
COURIERBOX     35
------------------
TOTAL          85
```

El detalle consolidado representa:

```text
Producto A = 85
```

Pero el sistema debe poder identificar de dónde provienen esas 85 unidades.

> **Aclaración — "Necesidad de Compra" (refinamiento 2026-09-11):** "Necesidad de Compra" es un **concepto funcional**, no una entidad independiente. Está representada por `DetalleConsolidacion.CantidadNecesaria`. No debe crearse `NecesidadDeCompra` ni tablas equivalentes (ver `00-contexto-proyecto.md §7.3`).

---

# 20. AsignaciónConsolidación

### Responsabilidad

Mantiene la relación entre una cantidad consolidada y las solicitudes que la originaron.

Ejemplo:

```text
Detalle consolidado:
Producto A = 85

Asignaciones:

Detalle requisición AUROTECH = 30
Detalle requisición FAVIPAQ = 20
Detalle requisición COURIERBOX = 35
```

Esto evita perder la trazabilidad.

### Relaciones

```text
DetalleConsolidación 1 ─────── N AsignaciónConsolidación

DetalleRequisición 1 ─────── N AsignaciónConsolidación
```

---

# 21. PedidoProveedor

### Responsabilidad

Representa una compra realizada a un proveedor.

Un pedido:

- pertenece a un proveedor;
- pertenece a una consolidación;
- contiene productos;
- puede distribuirse entre diferentes empresas y sedes;
- puede tener una o varias entregas.

### Relaciones

```text
Proveedor 1 ─────── N PedidoProveedor

Consolidación 1 ─────── N PedidoProveedor

PedidoProveedor 1 ─────── N DetallePedidoProveedor

PedidoProveedor 1 ─────── N Entrega

PedidoProveedor 1 ─────── N Factura   (alcance inicial — RN-039, ADR-042)
```

> **Entrega y Factura son ramas independientes de PedidoProveedor (RN-040, ADR-043):** ambas cuelgan directamente de `PedidoProveedor`, no una de otra. No existe relación `Entrega → Factura`. Ver `§29` (Factura) y `§29.1` (DetalleFactura).

### Estados

*(Cierre documental 2026-09-11, RN-043/RN-044, ADR-046/ADR-047.)*

```text
BORRADOR → ENVIADO → PARCIALMENTE_ENTREGADO → ENTREGADO → CERRADO
```

Cancelación posible desde `BORRADOR`, `ENVIADO` o `PARCIALMENTE_ENTREGADO` → `CANCELADO`. El cierre (`ENTREGADO → CERRADO`) requiere cantidad pendiente de entrega en cero y una acción explícita de un usuario autorizado; no depende de la existencia de `Factura`. Ver `01-reglas-negocio.md §11` para el detalle completo y significado de cada estado.

> **Estado de implementación (2026-09-11):** implementado — `PedidoProveedorEstado` (enum), transiciones `Enviar`/`ActualizarEstadoPorEntregas`/`Cerrar`/`Cancelar`, y `CerrarPedidoProveedorUseCase`/`EnviarPedidoProveedorUseCase`/`CancelarPedidoProveedorUseCase`. Ver `04-base-datos.md §26`.

### Importante

`PedidoProveedor` **no debe pertenecer directamente a una Empresa**.

Un mismo pedido puede contener productos destinados a diferentes empresas.

---

# 22. DetallePedidoProveedor

### Responsabilidad

Representa un producto incluido en un pedido al proveedor.

Información conceptual:

```text
Producto
Cantidad necesaria
Cantidad pedida
Precio unitario
```

Nombres alineados con las columnas de `04-base-datos.md §27` (`CantidadNecesaria`/`CantidadPedida`).

Debe mantenerse la diferencia entre:

```text
Necesidad
```

y:

```text
Compra
```

Ejemplo:

```text
Necesidad: 85
Compra: 100
```

---

# 23. DistribuciónPedido

### Responsabilidad

Representa a qué empresa y sede están destinadas las cantidades compradas.

Ejemplo:

```text
Pedido:
Producto A = 100

Distribución:

AUROTECH - Bogotá       20
AUROTECH - Medellín     15
FAVIPAQ - Bogotá        30
COURIERBOX - Bogotá     20
```

### Relaciones

```text
DetallePedidoProveedor 1 ─────── N DistribuciónPedido

Sede 1 ─────── N DistribuciónPedido
```

Debe existir trazabilidad suficiente para determinar la empresa mediante la sede correspondiente.

---

# 24. Entrega

### Responsabilidad

Representa una entrega física realizada por un proveedor.

Una orden puede tener múltiples entregas.

### Estados

*(Cierre documental 2026-09-11, RN-046, ADR-049.)*

```text
REGISTRADA
ANULADA
```

Las condiciones detalladas de recepción (aceptado/rechazado/dañado/faltante) **no** son estados de `Entrega` — quedan diferidas como concepto aparte (Pendiente 4, `01-reglas-negocio.md §15`), a modelar solo si el negocio lo requiere.

> **Estado de implementación (2026-09-11):** implementado — `EntregaEstado` (enum). `Anular()` existe en Domain; sin caso de uso/endpoint dedicado todavía. Ver `04-base-datos.md §29`.

Ejemplo:

```text
Pedido
  ↓
Entrega 1
  ↓
Entrega 2
  ↓
Entrega 3
```

### Relaciones

```text
PedidoProveedor 1 ─────── N Entrega

Entrega 1 ─────── N DetalleEntrega
```

---

# 25. DetalleEntrega

### Responsabilidad

Representa los productos efectivamente entregados dentro de una entrega.

Ejemplo:

```text
Pedido:
100 unidades

Entrega:
60 unidades
```

El detalle permite determinar cuánto se ha entregado y cuánto continúa pendiente.

---

# 26. DistribuciónEntrega

### Responsabilidad

Representa cuánto de una entrega fue destinado a una sede determinada.

Ejemplo:

```text
Entrega:
60 unidades

Bogotá: 40
Medellín: 20
```

### Relaciones

```text
DetalleEntrega 1 ─────── N DistribuciónEntrega

Sede 1 ─────── N DistribuciónEntrega
```

Debe conservarse la información histórica necesaria del destino.

---

# 26.1 Resumen — los distintos conceptos de "distribución" no deben colapsarse

*(Aclaración de cierre documental 2026-09-11, a partir de las preguntas del negocio sobre distribuciones.)*

El proyecto usa la palabra "distribución" para varios conceptos **distintos**, cada uno con su propia responsabilidad y momento del proceso. No deben tratarse como una única "Distribución" genérica:

| Concepto | Responsabilidad | Entidad real | Momento |
|---|---|---|---|
| Relación necesidad consolidada ↔ requisiciones de origen | De dónde provienen las unidades consolidadas de un producto | `AsignacionConsolidacion` (`§20`) — **no existe ni debe crearse** una entidad separada llamada `DistribucionConsolidacion` | Al consolidar |
| Distribución planificada de la compra | A qué sedes se planea destinar lo comprado | `DistribucionPedido` (`§23`) | Al crear/ajustar el pedido |
| Distribución real de lo entregado | A qué sedes se destinó físicamente lo entregado, con snapshot histórico de dirección | `DistribucionEntrega` (`§26`) | Al registrar cada entrega |
| Distribución relacionada con facturación | No existe hoy ningún concepto de este tipo | — | — |

Si en el futuro surge una necesidad real de "distribución" asociada a facturación, debe tratarse como una **responsabilidad nueva y propia** (con su propia decisión de negocio y, si corresponde, su propia entidad) — no debe reutilizarse ni confundirse con `DistribucionPedido` ni `DistribucionEntrega`.

---

# 27. Cantidades y trazabilidad

El dominio debe diferenciar claramente las siguientes cantidades:

```text
Cantidad solicitada
        ↓
Cantidad aprobada
        ↓
Cantidad necesaria
        ↓
Cantidad pedida
        ↓
Cantidad entregada
        ↓
Cantidad pendiente
```

Nombres alineados con las columnas documentadas (`CantidadNecesaria` en `04-base-datos.md §24`, `CantidadPedida` en `04-base-datos.md §27`). "Cantidad aprobada" no es un campo separado: es `DetalleRequisicion.CantidadSolicitada` una vez que la `Requisicion` está `Aprobada` (ver Pendiente 2 de `01-reglas-negocio.md §15`, reportada por separado en la auditoría 2026-09-10).

No se debe asumir que todas son iguales.

---

# 28. Regla para cantidades

El sistema debe poder establecer relaciones matemáticas entre las diferentes etapas.

Por ejemplo:

```text
PendienteEntrega
=
CantidadPedida
-
CantidadEntregada
```

Para una distribución:

```text
CantidadDistribuida
=
SUM(distribuciones)
```

Las reglas completas de cantidades deben implementarse y probarse de acuerdo con las reglas de negocio.

---

# 29. Factura

### Responsabilidad

Representa el registro operativo mínimo de la facturación asociada al proceso de compra (RN-038): trazabilidad de compras, no contabilidad.

### Cardinalidad con PedidoProveedor

> **Actualización (refinamiento 2026-09-11, RN-039/ADR-042):** para el **alcance inicial**, se confirma:
>
> ```text
> PedidoProveedor 1 ───── N Factura
> ```
>
> Es decir, un pedido puede respaldarse en varias facturas, pero cada factura pertenece a un único pedido. Esto ya coincide con la implementación actual (`Factura.PedidoProveedorId` obligatoria).
>
> **Sigue sin resolverse el caso general** `Factura N ───── N PedidoProveedor` (una factura cubriendo varios pedidos) — no debe implementarse salvo que exista una necesidad de negocio explícita (ver `01-reglas-negocio.md §15`, matriz de decisiones, D-06). Esta sección reemplaza la ambigüedad original de cardinalidad (registrada originalmente en la auditoría del proceso contable); se conserva la nota de que la relación general sigue sin validar con el proceso contable real.

### Relaciones

```text
PedidoProveedor 1 ─────── N Factura

Factura 1 ─────── N DetalleFactura   (§29.1)
```

### Estados

*(Cierre documental 2026-09-11, RN-047, ADR-050.)*

```text
REGISTRADA
ANULADA
```

Sin estados contables (`CAUSADA`/`CONTABILIZADA`/`PAGADA`/`CONCILIADA`) — Auropaq Pedidos maneja la factura desde una perspectiva operativa, no contable (RN-038).

> **Estado de implementación (2026-09-11):** implementado — `FacturaEstado` (enum). `Anular()` existe en Domain; sin caso de uso/endpoint dedicado todavía. Ver `04-base-datos.md §32`.

---

# 29.1 DetalleFactura

### Responsabilidad

Representa un producto facturado dentro de una factura.

> **Nota (refinamiento 2026-09-11):** esta entidad ya está implementada (bloque "TASK-046 — incremento MVP") pero no había sido documentada aquí. Se documenta tal como existe, marcada explícitamente como decisión **provisional de alcance MVP**, no como regla de negocio definitiva.

Información conceptual:

```text
DetallePedidoOrigen (DetallePedidoProveedor)
CantidadFacturada
PrecioUnitario
Subtotal (calculado = CantidadFacturada × PrecioUnitario)
```

Se relaciona con `DetallePedidoProveedor` directamente — **no** con `DetalleEntrega` (RN-040: Entrega y Factura son independientes).

Regla implementada (provisional, sin respaldo documental propio — ver `01-reglas-negocio.md §15`, Pendiente 13): la `CantidadFacturada` acumulada entre todas las facturas del mismo `DetallePedidoProveedor` no debe superar `CantidadPedida`.

### Relaciones

```text
DetallePedidoProveedor 1 ─────── N DetalleFactura
```

---

# 30. Historial

### Responsabilidad

Representa los eventos importantes ocurridos durante el proceso.

Ejemplo:

```text
Requisición creada
Requisición enviada
Requisición devuelta
Requisición corregida
Requisición reenviada
Requisición aprobada
```

Debe conservar como mínimo:

```text
Usuario
Fecha y hora
Acción
Entidad
Identificador
Información relevante
```

El modelo técnico de auditoría se definirá posteriormente.

---

# 31. Relaciones principales del dominio

Vista simplificada:

```text
EMPRESA
   │
   ├────────── SEDES
   │
   ├────────── USUARIOS
   │                │
   │                ├────────── USUARIOSEDE ────────── SEDE
   │                │
   │                └────────── USUARIOROL ────────── ROL (GLOBAL)
   │                                                     │
   │                                                     └────────── ROLPERMISO ────────── PERMISO (GLOBAL)
   │
   └────────── REQUISICIONES
                    │
                    ├────────── PERIODO
                    │
                    └────────── DETALLES
                                  │
                                  ├────────── PRODUCTO
                                  │
                                  └────────── DISTRIBUCIÓN
                                               │
                                               └────────── SEDE


PRODUCTO
   │
   ├────────── CATEGORÍA
   │
   ├────────── UNIDAD
   │
   └────────── PRODUCTO-PROVEEDOR
                         │
                         └────────── PROVEEDOR


REQUISICIONES APROBADAS
          │
          ▼
   CONSOLIDACIÓN
          │
          ├────────── DETALLE CONSOLIDACIÓN
          │                 │
          │                 └────────── ASIGNACIONES
          │
          ▼
   PEDIDO PROVEEDOR
          │
          ├────────── DETALLE PEDIDO
          │                 │
          │                 └────────── DISTRIBUCIÓN
          │
          ▼
       ENTREGA
          │
          ├────────── DETALLE ENTREGA
          │                 │
          │                 └────────── DISTRIBUCIÓN
          │
          ▼
       FACTURA
```

---

# 32. Flujo completo del dominio

El flujo conceptual completo es:

```text
EMPRESA
   ↓
SEDE
   ↓
PERIODO
   ↓
REQUISICIÓN
   ↓
DETALLE REQUISICIÓN
   ↓
DISTRIBUCIÓN POR SEDE
   ↓
REVISIÓN
   ↓
APROBACIÓN
   ↓
CONSOLIDACIÓN
   ↓
PEDIDO PROVEEDOR
   ↓
DISTRIBUCIÓN DEL PEDIDO
   ↓
ENTREGA
   ↓
ENTREGA PARCIAL
   ↓
FACTURA
   ↓
CIERRE
```

---

# 33. Lo que NO debe hacerse en el dominio

No se deben crear relaciones únicamente para facilitar una consulta.

No se deben crear entidades únicamente porque una pantalla necesita almacenar información temporal.

No se debe colocar `EmpresaId` en entidades donde la empresa pueda determinarse correctamente mediante otra relación y donde hacerlo genere duplicidad o contradicción.

Ejemplo:

```text
PedidoProveedor
```

no debe tener directamente:

```text
EmpresaId
```

porque un pedido puede abastecer varias empresas.

---

# 34. Principio de separación de responsabilidades

Cada concepto debe tener una responsabilidad clara.

Ejemplo:

```text
Requisición
→ representa necesidad interna.

Consolidación
→ representa necesidad total agrupada.

PedidoProveedor
→ representa compra.

Entrega
→ representa recepción física.

Factura
→ representa facturación.
```

No se deben mezclar estas responsabilidades en una sola entidad.

---

# 35. Estados como comportamiento del dominio

Los estados no deben tratarse únicamente como valores visuales para una interfaz.

Un estado determina qué acciones están permitidas.

Ejemplo:

```text
BORRADOR
→ modificar
→ agregar
→ eliminar
→ enviar

ENVIADA
→ revisar

DEVUELTA
→ corregir
→ reenviar

APROBADA
→ continuar hacia consolidación
```

Las transiciones permitidas deberán definirse formalmente antes de implementar el workflow.

---

# 36. Límites actuales del modelo

El siguiente modelo representa el dominio conceptual actual.

Todavía deben definirse con mayor precisión:

- Estados completos de cada proceso.
- Transiciones entre estados.
- Reglas de autorización.
- Modelo definitivo de productos no catalogados.
- Modelo definitivo de factura.
- Manejo de sustituciones.
- Diferencias entre cantidades solicitadas, aprobadas y compradas.
- Recepción parcial y diferencias.
- Reglas de cierre.
- Notificaciones.
- Reglas de excepción.
- Casos de devolución después del cierre de la ventana.
- Reglas para modificaciones posteriores a la aprobación.

Estas decisiones no deben ser inventadas durante la implementación.

---

# 37. Regla para Claude Code

Antes de implementar una entidad relacionada con el dominio, Claude Code debe:

1. Revisar este documento.
2. Revisar las reglas de negocio relacionadas.
3. Identificar las relaciones existentes.
4. Identificar las cardinalidades.
5. Identificar las reglas que debe cumplir.
6. No crear relaciones adicionales sin justificación.
7. No eliminar relaciones existentes sin autorización.
8. No convertir automáticamente conceptos pendientes en decisiones definitivas.
9. Informar cualquier contradicción detectada.
10. Crear pruebas para las reglas de dominio que correspondan.

---

# 38. Regla fundamental del dominio

> **Una entidad representa una responsabilidad del negocio, una relación representa una dependencia real del negocio y una regla representa un comportamiento que el sistema debe garantizar.**

El objetivo no es tener muchas entidades.

El objetivo es tener un modelo que represente correctamente el negocio y pueda evolucionar sin generar contradicciones.
