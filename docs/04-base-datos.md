# 04 — BASE DE DATOS

## 1. Objetivo

Este documento define el modelo de datos relacional de la aplicación de gestión de requisiciones y pedidos de Auropaq S.A.S.

La base de datos debe representar el dominio definido en `02-dominio.md` y respetar las decisiones arquitectónicas establecidas en `03-arquitectura.md`.

El objetivo es construir una estructura de datos:

- consistente;
- normalizada;
- trazable;
- escalable;
- preparada para múltiples empresas;
- preparada para múltiples sedes;
- preparada para múltiples proveedores;
- preparada para pedidos parciales;
- preparada para evolución futura;
- protegida mediante restricciones de integridad.

La base de datos debe almacenar información del negocio, no comportamientos propios de la interfaz.

---

# 2. Motor de base de datos

La aplicación utilizará:

- **SQL Server** como sistema gestor de base de datos.
- **Entity Framework Core** como ORM.
- **Migraciones de EF Core** para controlar la evolución del esquema.

La base de datos será relacional.

No se debe introducir otro motor de base de datos sin una decisión arquitectónica explícita.

---

# 3. Principios de diseño

## 3.1 La base de datos representa el dominio

Las tablas deben derivarse de las necesidades reales del negocio.

No se deben crear tablas simplemente porque una clase, DTO o componente de frontend exista.

Primero se define:

1. necesidad del negocio;
2. entidad o concepto;
3. relación;
4. regla;
5. persistencia.

---

## 3.2 No hardcodear empresas

No deben existir columnas, tablas, condiciones ni procedimientos específicos para:

- AUROTECH;
- FAVIPAQ;
- COURIERBOX ÁLAMOS;
- COURIERBOX;
- Martha;
- nombres específicos de usuarios.

Las empresas, usuarios, sedes, roles y proveedores son datos.

El sistema debe poder incorporar una nueva empresa mediante configuración/datos sin modificar el código fuente.

---

## 3.3 Identificadores

Las entidades persistentes deben utilizar identificadores internos estables.

Como regla general:

```text
Id
```

será la clave primaria técnica.

La clave primaria no debe depender de:

- nombre;
- código visible;
- correo;
- documento;
- código del proveedor;
- descripción del producto.

Cuando el negocio necesite un código visible, este debe ser un atributo adicional.

---

# 4. Organización lógica de las tablas

Las tablas se agrupan conceptualmente en:

```text
ORGANIZACIÓN
    Empresa
    Sede
    Usuario
    Rol
    Permiso
    UsuarioSede
    UsuarioRol
    RolPermiso

CATÁLOGO
    Categoria
    UnidadMedida
    Producto
    Proveedor
    ProductoProveedor

PERIODOS
    Periodo

REQUISICIONES
    Requisicion
    DetalleRequisicion
    DistribucionRequisicion
    SolicitudProductoCatalogo

CONSOLIDACIÓN
    Consolidacion
    DetalleConsolidacion
    AsignacionConsolidacion

PEDIDOS
    PedidoProveedor
    DetallePedidoProveedor
    DistribucionPedido

ENTREGAS
    Entrega
    DetalleEntrega
    DistribucionEntrega

FACTURACIÓN
    Factura

TRAZABILIDAD
    HistorialRequisicion
    Auditoria
```

No todas las tablas tienen que implementarse en la primera versión.

El modelo completo representa la evolución prevista del sistema.

---

# 5. Organización

## 5.1 Empresa

Representa una empresa perteneciente a la organización que utiliza el sistema.

### Campos principales

```text
Empresa
---------
Id
Nombre
Nit
Activo
FechaCreacion
FechaActualizacion
```

### Relaciones

```text
Empresa 1 ─── N Sede
Empresa 1 ─── N Usuario
Empresa 1 ─── N Requisicion
```

### Reglas

- Una empresa puede tener múltiples sedes.
- Una empresa puede tener múltiples usuarios.
- Una empresa puede realizar múltiples requisiciones.
- Una empresa puede estar activa o inactiva.
- No se deben eliminar físicamente empresas que tengan información histórica relevante.
- `Nit` debe tener una restricción de unicidad si el negocio confirma que identifica de forma única a la empresa.

---

# 6. Sede

Representa una ubicación física perteneciente a una empresa.

Una empresa puede tener varias sedes.

### Campos principales

```text
Sede
---------
Id
EmpresaId
Nombre
Direccion
Ciudad
Departamento
Telefono
Contacto
Activo
FechaCreacion
FechaActualizacion
```

### Relación

```text
Empresa 1 ─── N Sede
```

### Reglas

- Toda sede pertenece a exactamente una empresa.
- Una empresa puede tener muchas sedes.
- Una sede no puede pertenecer simultáneamente a varias empresas.
- Una sede puede recibir productos.
- La sede debe conservarse para mantener la trazabilidad histórica.
- No se debe depender únicamente de la dirección actual de la sede para reconstruir entregas históricas.

---

# 7. Usuario

Representa una persona que utiliza el sistema.

### Campos principales

```text
Usuario
---------
Id
EmpresaId
Nombre
Apellido
Correo
PasswordHash
Activo
FechaCreacion
FechaActualizacion
```

**`PasswordHash`** *(implementado, autenticación "TASK-015", 2026-09-14 — ver RN-056/ADR-057):* obligatorio, `nvarchar(256)`. Nunca la contraseña en texto plano — se hashea con `PasswordHasher<T>` (PBKDF2, mecanismo estándar de .NET) antes de persistir. La estrategia de identidad se cerró como **JWT propio emitido por esta misma API** (no proveedor externo) — ver ADR-057.

### Relación

```text
Empresa 1 ─── N Usuario
```

### Reglas

- Un usuario pertenece a una empresa.
- Un usuario puede tener uno o varios roles.
- Un usuario puede tener acceso a una o varias sedes según las reglas de autorización.
- El correo debe ser único según la estrategia de identidad que se adopte (cerrado como GLOBAL, TASK-008).
- Un usuario con `Activo == false` no puede autenticarse (RN-056).

La base de datos del negocio no debe almacenar contraseñas en texto plano — confirmado: solo se almacena `PasswordHash`.

---

# 8. UsuarioSede

Permite definir las sedes sobre las cuales un usuario puede operar.

```text
UsuarioSede
-----------
UsuarioId
SedeId
```

### Relación

```text
Usuario N ─── N Sede
```

### Reglas

- Un usuario puede tener acceso a una o varias sedes.
- Una sede puede tener varios usuarios.
- No deben existir registros duplicados `UsuarioId + SedeId`.

Esta relación permite que el sistema evolucione sin asumir que cada usuario pertenece a una única sede.

---

# 9. Roles y permisos

## 9.1 Rol

Representa una responsabilidad funcional.

```text
Rol
---------
Id
Nombre
Descripcion
Activo
```

**Alcance: GLOBAL.** Sin `EmpresaId` — deliberadamente no declarado (confirmado en la implementación de TASK-010, 2026-09-14). Un mismo `Rol` puede asignarse a usuarios de cualquier empresa.

Ejemplos:

```text
SOLICITANTE
GESTOR_REQUISICIONES
ADMINISTRADOR
COMPRAS
RECEPCION
```

Estos nombres son ejemplos conceptuales.

No se debe asumir que son definitivos sin validación del negocio.

---

## 9.2 Permiso

Representa una acción autorizada.

```text
Permiso
---------
Id
Codigo    (obligatorio, único GLOBAL)
Nombre
Descripcion
```

**Alcance: GLOBAL.** Sin `EmpresaId`, sin `Activo` (no declarado — a diferencia de `Rol`).

**Identificadores** *(decisión de hardening previa a TASK-013, 2026-09-14 — ver RN-055/ADR-056):* `Codigo` es obligatorio y **único en todo el sistema**. La versión inicial de este documento (TASK-011) no establecía unicidad; se cerró como decisión de negocio antes de implementar `RolPermiso` (TASK-013), porque un código duplicado generaría ambigüedad al identificar qué capacidad funcional otorga un permiso. Reforzada con índice único en SQL Server (`PermisoConfiguration`).

Ejemplos:

```text
REQUISICION_CREAR
REQUISICION_VER
REQUISICION_MODIFICAR
REQUISICION_ENVIAR
REQUISICION_APROBAR
REQUISICION_DEVOLVER
PRODUCTO_CREAR
PRODUCTO_EDITAR
PEDIDO_CREAR
PEDIDO_CONSOLIDAR
ENTREGA_REGISTRAR
FACTURA_REGISTRAR
```

---

## 9.3 UsuarioRol

**Implementado (TASK-010, 2026-09-14).** Clave primaria compuesta `(UsuarioId, RolId)`, sin `Id` propio. Sin validación de empresa (`Rol` es global).

```text
UsuarioRol
----------
UsuarioId
RolId
```

Relación:

```text
Usuario N ─── N Rol
```

---

## 9.4 RolPermiso

**Implementado (TASK-013, 2026-09-14).** Clave primaria compuesta `(RolId, PermisoId)`, sin `Id` propio. Sin validación de empresa (`Rol` y `Permiso` son ambos globales).

```text
RolPermiso
----------
RolId
PermisoId
```

Relación:

```text
Rol N ─── N Permiso
```

### Principio importante

El sistema debe diferenciar:

```text
PERMISO = qué puede hacer
ALCANCE = sobre qué información puede hacerlo
```

Por ejemplo:

```text
Usuario A
Permiso: REQUISICION_VER
Alcance: Empresa X
```

No se debe resolver autorización únicamente comparando nombres de roles.

---

# 10. Catálogo

## 10.1 Categoria

Representa la clasificación interna del producto.

```text
Categoria
---------
Id
Nombre
Descripcion
Activo
```

Ejemplos actuales del negocio:

```text
Aseo
Cafetería
Papelería
Botiquín
```

La lista debe ser configurable.

---

# 11. UnidadMedida

Representa la unidad utilizada para manejar cantidades.

```text
UnidadMedida
------------
Id
Codigo
Nombre
Activo
```

Ejemplos:

```text
UNIDAD
GALON
CAJA
PAQUETE
```

La unidad debe ser un dato normalizado y no texto libre repetido en cada requisición.

---

# 12. Producto

Representa un producto oficial del catálogo interno.

```text
Producto
--------
Id
CodigoInterno
Nombre
Descripcion
CategoriaId
UnidadMedidaId
Activo
FechaCreacion
FechaActualizacion
```

### Relaciones

```text
Categoria 1 ─── N Producto
UnidadMedida 1 ─── N Producto
```

### Reglas

- Un producto oficial debe tener un código interno cuando el negocio lo requiera.
- El producto pertenece a una categoría.
- El producto tiene una unidad de medida.
- Los productos deben poder desactivarse.
- No se debe eliminar físicamente un producto que tenga historial.
- `CodigoInterno` debe ser único dentro del catálogo interno cuando corresponda.

---

# 13. Producto no catalogado

Un producto solicitado por una empresa puede no existir inicialmente en el catálogo oficial.

No se debe introducir un producto incompleto dentro de `Producto` únicamente para representar esta situación.

Se recomienda una entidad separada:

```text
SolicitudProductoCatalogo
-------------------------
Id
EmpresaId
UsuarioId
NombreSolicitado
Descripcion
Observacion
Estado
ProductoId
FechaSolicitud
FechaResolucion
UsuarioResolucionId
MotivoResolucion
```

### Estados conceptuales

```text
PENDIENTE
HOMOLOGADO
CREADO
RECHAZADO
```

### Flujo

```text
Usuario solicita producto desconocido
                ↓
        Revisión del gestor
                ↓
       ┌────────┼────────┐
       ↓        ↓        ↓
  Homologar   Crear    Rechazar
       ↓        ↓        ↓
Producto existente Producto nuevo
```

Esto permite mantener separado:

```text
PRODUCTO OFICIAL
```

de:

```text
SOLICITUD DE PRODUCTO
```

---

# 14. Proveedor

Representa un proveedor externo.

```text
Proveedor
---------
Id
Nombre
Nit
Contacto
Telefono
Correo
Activo
```

Un proveedor puede ofrecer múltiples productos.

---

# 15. ProductoProveedor

Esta tabla resuelve una necesidad crítica del proceso actual.

El sistema interno utiliza un código de producto, mientras que cada proveedor puede utilizar códigos diferentes.

```text
ProductoProveedor
-----------------
Id
ProductoId
ProveedorId
CodigoProveedor
DescripcionProveedor
CategoriaProveedor
UnidadProveedor
Activo
```

### Relaciones

```text
Producto N ─── N Proveedor
```

mediante:

```text
ProductoProveedor
```

### Ejemplo conceptual

```text
Producto interno
Código: 1281

Proveedor A
Código: ABC-001

Proveedor B
Código: LIM-458
```

El sistema puede buscar automáticamente el código correspondiente al proveedor.

### Restricción

Debe existir una restricción de unicidad apropiada para evitar duplicar la misma relación producto/proveedor.

---

# 16. Periodo

Representa el periodo mensual de operación.

```text
Periodo
-------
Id
Anio
Mes
FechaInicio
FechaFin
FechaInicioSolicitud
FechaFinSolicitud
Estado
```

### Ejemplo

```text
Periodo: Septiembre 2026

Periodo operativo:
01/09/2026 - 30/09/2026

Ventana de solicitud:
01/09/2026 - 03/09/2026
```

### Reglas

- El periodo debe identificar año y mes.
- Debe existir como máximo un periodo para la combinación `Año + Mes`.
- La ventana de solicitud es configurable.
- No se deben codificar fechas como `1, 2 y 3` directamente en el código.
- El cierre de la ventana debe ser validado por backend.

---

# 17. Requisicion

Representa la necesidad mensual de una empresa.

La requisición **NO representa todavía el pedido al proveedor**.

```text
Requisicion
-----------
Id
EmpresaId
PeriodoId
UsuarioCreacionId
Estado
FechaCreacion
FechaEnvio
FechaActualizacion
Observacion
```

### Relación principal

```text
Empresa 1 ─── N Requisicion
Periodo 1 ─── N Requisicion
```

### Restricción fundamental

Debe existir como máximo una requisición por empresa y periodo:

```text
UNIQUE( EmpresaId, PeriodoId )
```

Esto significa:

```text
Empresa X + Septiembre 2026
```

solo puede tener una requisición mensual.

La requisición puede contener productos destinados a múltiples sedes.

---

# 18. Estados de Requisicion

Los estados deben representar el ciclo real del negocio.

Estado conceptual inicial:

```text
BORRADOR
ENVIADA
EN_REVISION
DEVUELTA
APROBADA
CONSOLIDADA
EN_PEDIDO
EN_ENTREGA
CERRADA
```

Los nombres definitivos pueden implementarse mediante enum en código y/o catálogo persistente según la decisión de implementación.

No se debe permitir cualquier transición arbitraria.

Ejemplo:

```text
BORRADOR
   ↓
ENVIADA
   ↓
EN_REVISION
   ├──→ DEVUELTA
   │       ↓
   │    ENVIADA
   │
   └──→ APROBADA
```

---

# 19. DetalleRequisicion

Representa cada producto solicitado dentro de una requisición.

```text
DetalleRequisicion
------------------
Id
RequisicionId
ProductoId
CantidadSolicitada
Observacion
```

### Relación

```text
Requisicion 1 ─── N DetalleRequisicion
Producto 1 ─── N DetalleRequisicion
```

### Reglas

- Una requisición puede tener muchos detalles.
- Cada detalle representa un producto oficial.
- La cantidad debe ser mayor que cero.
- No se debe permitir una cantidad negativa.
- La existencia del producto debe validarse en backend.

---

# 20. DistribucionRequisicion

Permite distribuir la cantidad solicitada entre las sedes.

```text
DistribucionRequisicion
-----------------------
Id
DetalleRequisicionId
SedeId
Cantidad
```

### Relación

```text
DetalleRequisicion 1 ─── N DistribucionRequisicion
Sede 1 ─── N DistribucionRequisicion
```

### Regla fundamental

La suma de las cantidades distribuidas debe ser igual a la cantidad solicitada:

```text
SUM(DistribucionRequisicion.Cantidad)
=
DetalleRequisicion.CantidadSolicitada
```

Ejemplo:

```text
Producto:
Papel higiénico

Cantidad solicitada:
100

Distribución:

Sede Bogotá       60
Sede Medellín     40
                  ---
                 100
```

No se debe permitir:

```text
60 + 40 = 100
```

si la cantidad solicitada fuera diferente.

---

# 21. Reglas de integridad de Requisicion

Antes de pasar de:

```text
BORRADOR
```

a:

```text
ENVIADA
```

el backend debe validar como mínimo:

- periodo válido;
- ventana de solicitud abierta;
- empresa activa;
- usuario autorizado;
- requisición perteneciente al usuario/alcance correspondiente;
- existencia de detalles;
- cantidades válidas;
- productos válidos;
- sedes pertenecientes a la empresa;
- distribución completa;
- ausencia de datos inconsistentes.

Guardar una requisición no equivale a enviarla.

---

# 22. HistorialRequisicion

Permite conservar la trazabilidad de los cambios de estado.

```text
HistorialRequisicion
--------------------
Id
RequisicionId
EstadoAnterior
EstadoNuevo
UsuarioId
Fecha
Comentario
```

Ejemplo:

```text
BORRADOR → ENVIADA
Usuario: Juan
Fecha: 03/09/2026
```

Posteriormente:

```text
EN_REVISION → DEVUELTA
Usuario: Gestor
Comentario: Corregir cantidad del producto X
```

Esto reemplaza la necesidad de llevar manualmente una hoja de Excel para saber qué ocurrió con cada requisición.

---

# 23. Consolidacion

Representa la consolidación de necesidades aprobadas.

```text
Consolidacion
-------------
Id
PeriodoId
UsuarioCreacionId
Estado
FechaCreacion
Observacion
```

### Relación

```text
Periodo 1 ─── N Consolidacion
```

Una consolidación puede tomar información de múltiples empresas.

---

# 24. DetalleConsolidacion

Representa la cantidad total requerida de un producto.

```text
DetalleConsolidacion
--------------------
Id
ConsolidacionId
ProductoId
CantidadNecesaria
```

**Implementado (verificado en auditoría 2026-09-14):** `CantidadNecesaria` **no es una columna
SQL real** — es una propiedad calculada en Domain (`SUM` de `AsignacionConsolidacion.Cantidad`,
`DetalleConsolidacion.cs`), explícitamente `Ignore()`d en `DetalleConsolidacionConfiguration`
para que EF Core no intente persistirla. Se recalcula cada vez que se cargan las asignaciones —
mismo criterio ya usado para `Requisicion.DetalleRequisicion.CantidadDistribuida`. Nunca puede
quedar desincronizada porque no se almacena por separado. `DetallePedidoProveedor.CantidadNecesaria`
(`§27`) es distinta: ahí sí es una columna real, porque representa una fotografía congelada del
valor en el momento de crear el detalle del pedido, no el total vivo.

Ejemplo:

```text
AUROTECH          20
FAVIPAQ           30
COURIERBOX        15
COURIERBOX ÁLAMOS 20
                  ---
TOTAL             85
```

La consolidación representa:

```text
NECESIDAD TOTAL
```

no necesariamente:

```text
CANTIDAD QUE SE COMPRARÁ
```

---

# 25. AsignacionConsolidacion

Permite mantener trazabilidad entre la consolidación y las requisiciones originales.

```text
AsignacionConsolidacion
-----------------------
Id
DetalleConsolidacionId
DetalleRequisicionId
Cantidad
```

Esto permite responder:

> ¿De dónde salió la cantidad total consolidada?

Ejemplo:

```text
Detalle consolidado: Papel higiénico = 85

Asignaciones:

Requisición AUROTECH       20
Requisición FAVIPAQ        30
Requisición COURIERBOX     15
Requisición ÁLAMOS         20
                           ---
                            85
```

La consolidación no debe destruir ni modificar las requisiciones originales.

---

# 26. PedidoProveedor

Representa la compra realizada a un proveedor.

```text
PedidoProveedor
---------------
Id
ConsolidacionId
ProveedorId
NumeroPedido
FechaPedido
FechaEntregaEstimada
Estado
Observacion
```

### Relaciones

```text
Consolidacion 1 ─── N PedidoProveedor
Proveedor 1 ─── N PedidoProveedor
```

Una consolidación puede generar múltiples pedidos.

Ejemplo:

```text
Consolidación
     │
     ├── Pedido proveedor A
     ├── Pedido proveedor B
     └── Pedido proveedor C
```

No se debe colocar `EmpresaId` directamente en `PedidoProveedor`.

Un pedido puede consolidar necesidades de múltiples empresas.

### Estados (cierre documental 2026-09-11, RN-043/RN-044, ADR-046/ADR-047)

```text
BORRADOR → ENVIADO → PARCIALMENTE_ENTREGADO → ENTREGADO → CERRADO
                                              (CANCELADO desde BORRADOR/ENVIADO/PARCIALMENTE_ENTREGADO)
```

Ver `01-reglas-negocio.md §11` para el significado de cada estado. **Implementado (2026-09-11):** `PedidoProveedorEstado` (enum), persistido como string.

### Identificadores (RN-049, ADR-051)

`NumeroPedido` es una referencia operacional ingresada (no autogenerada por el sistema), única dentro del proveedor. No se debe usar como sustituto del `Id` técnico. **Implementado (2026-09-11):** índice único compuesto `(ProveedorId, NumeroPedido)` en `PedidoProveedorConfiguration` + validación en `CrearPedidoProveedorUseCase`.

---

# 27. DetallePedidoProveedor

Representa los productos incluidos en el pedido al proveedor.

```text
DetallePedidoProveedor
----------------------
Id
PedidoProveedorId
ProductoId
CantidadNecesaria
CantidadPedida
PrecioUnitario
```

### Importante

Debe diferenciarse:

```text
CantidadNecesaria
```

de:

```text
CantidadPedida
```

porque el proveedor puede manejar:

- presentaciones;
- empaques;
- mínimos de compra;
- disponibilidad;
- sustituciones;
- ajustes autorizados.

Ejemplo:

```text
Necesidad: 85
Compra:    100
```

No se debe modificar la requisición original para representar esta diferencia.

---

# 28. DistribucionPedido

Representa hacia qué sedes deben dirigirse los productos comprados.

```text
DistribucionPedido
------------------
Id
DetallePedidoProveedorId
SedeId
Cantidad
```

### Ejemplo

```text
Pedido proveedor:
100 unidades

Distribución:

AUROTECH Bogotá          20
AUROTECH Medellín        15
FAVIPAQ Bogotá           30
COURIERBOX Bogotá        20
Otra sede                15
                         ---
                         100
```

Esto permite que un único pedido al proveedor atienda múltiples empresas y sedes.

---

# 29. Entrega

Representa una entrega física realizada por el proveedor.

```text
Entrega
-------
Id
PedidoProveedorId
FechaEntrega
NumeroRemision
Estado
Observacion
```

### Relación

```text
PedidoProveedor 1 ─── N Entrega
```

Un pedido puede tener múltiples entregas.

Esto permite manejar entregas parciales.

Ejemplo:

```text
Pedido: 100

Entrega 1: 60
Entrega 2: 40
```

### Estados (cierre documental 2026-09-11, RN-046, ADR-049)

```text
REGISTRADA
ANULADA
```

Sin estados de recepción detallada (`Aceptado`/`Rechazado`/`Dañado`) en el agregado `Entrega` — diferido (Pendiente 4). **Implementado (2026-09-11):** `EntregaEstado` (enum), persistido como string.

### Identificadores (RN-049, ADR-051)

`NumeroRemision` es una referencia operacional ingresada, única dentro del `PedidoProveedor`. **Implementado (2026-09-11):** índice único compuesto `(PedidoProveedorId, NumeroRemision)` en `EntregaConfiguration` + validación en `CrearEntregaUseCase`.

---

# 30. DetalleEntrega

Representa los productos entregados.

```text
DetalleEntrega
--------------
Id
EntregaId
DetallePedidoProveedorId
CantidadEntregada
```

### Regla

La cantidad entregada acumulada no debe superar la cantidad pedida, salvo que exista una regla de negocio explícita que autorice sobrantes.

Cantidad pendiente conceptual:

```text
CantidadPedida
-
SUM(CantidadEntregada)
=
CantidadPendiente
```

---

# 31. DistribucionEntrega

Representa la distribución física de una entrega hacia una sede.

```text
DistribucionEntrega
-------------------
Id
DetalleEntregaId
SedeId
Cantidad
DireccionEntrega
CiudadEntrega
ContactoEntrega
```

Los campos de dirección/contacto son un **snapshot histórico**.

Esto es importante.

Si actualmente una sede tiene:

```text
Carrera 10 # 20-30
```

y posteriormente cambia a:

```text
Carrera 50 # 80-20
```

una entrega histórica debe seguir mostrando la dirección donde realmente se realizó.

No se debe depender únicamente de:

```text
Sede.Direccion
```

para reconstruir información histórica.

---

# 32. Factura

La factura representa el documento comercial emitido por el proveedor, registrada como información operativa (RN-038) — no como registro contable.

> **Actualización (refinamiento 2026-09-11, RN-039/ADR-042):** se confirma para el alcance inicial `PedidoProveedor 1 ─── N Factura` (un pedido puede tener varias facturas; cada factura pertenece a un único pedido). Se descarta, para el alcance inicial, "una factura cubre varios pedidos" — ver `01-reglas-negocio.md §15`, matriz de decisiones, D-06, para el caso general.

Modelo implementado actualmente (bloque "TASK-046 — incremento MVP"):

```text
Factura
-------
Id
ProveedorId
PedidoProveedorId   -- FK obligatoria (RN-039)
NumeroFactura
FechaFactura
Impuestos
Estado
Observacion
```

> **Desviación técnica frente al modelo conceptual original:** `Subtotal` y `Total` **no se persisten** como columnas — son propiedades calculadas en Domain (`Subtotal = SUM(DetalleFactura.Subtotal)`, `Total = Subtotal + Impuestos`). Es una decisión técnica (evita inconsistencia entre el valor guardado y la suma real de detalles), no una decisión de negocio pendiente.

```text
DetalleFactura   -- implementado, no documentado previamente en esta sección
--------------
Id
FacturaId
DetallePedidoProveedorId
CantidadFacturada
PrecioUnitario
```

Regla (cierre documental 2026-09-11, RN-048, ADR-050 — ya no es "provisional", queda confirmada): `CantidadFacturada` acumulada por `DetallePedidoProveedor` (entre todas las facturas) no debe superar `CantidadPedida`. Ya implementada tal cual.

No se debe imponer el caso general N:N (una factura cubriendo varios pedidos) sin validar el negocio.

### Estados (cierre documental 2026-09-11, RN-047, ADR-050)

```text
REGISTRADA
ANULADA
```

Sin estados contables. **Implementado (2026-09-11):** `FacturaEstado` (enum), persistido como string.

### Identificadores (RN-049, ADR-051)

`NumeroFactura` es una referencia proporcionada por el proveedor, única dentro del proveedor. **Implementado (2026-09-11):** índice único compuesto `(ProveedorId, NumeroFactura)` en `FacturaConfiguration` + validación en `RegistrarFacturaUseCase`.

---

# 33. Auditoria

La aplicación necesita trazabilidad de acciones relevantes.

Modelo conceptual:

```text
Auditoria
---------
Id
UsuarioId
Entidad
EntidadId
Accion
Fecha
DatosAnteriores
DatosNuevos
```

Ejemplos:

```text
Usuario X
Entidad: Requisicion
EntidadId: 125
Accion: MODIFICAR
```

También:

```text
Accion: ENVIAR
Accion: APROBAR
Accion: DEVOLVER
Accion: CREAR
Accion: ELIMINAR
```

La auditoría no reemplaza el historial de negocio.

Diferencia:

```text
HistorialRequisicion
=
seguimiento del flujo de la requisición

Auditoria
=
registro técnico/operativo de acciones realizadas
```

---

# 34. Relaciones principales

El flujo completo de información debe poder recorrerse así:

```text
Empresa
   │
   ├── Usuario
   │
   └── Sede
          │
          └── Distribución
                 │
                 ▼
Periodo
   │
   ▼
Requisicion
   │
   └── DetalleRequisicion
            │
            └── DistribucionRequisicion
                       │
                       ▼
                     Sede

Requisiciones aprobadas
          │
          ▼
    Consolidacion
          │
          └── DetalleConsolidacion
                    │
                    └── AsignacionConsolidacion
                              │
                              ▼
                    DetalleRequisicion

Consolidacion
      │
      ▼
PedidoProveedor
      │
      ├── DetallePedidoProveedor
      │        │
      │        └── DistribucionPedido
      │
      └── Entrega
             │
             └── DetalleEntrega
                       │
                       └── DistribucionEntrega

PedidoProveedor
      │
      ▼
Factura
```

---

# 35. Integridad referencial

Las relaciones entre tablas deben utilizar claves foráneas.

Ejemplos:

```text
Sede.EmpresaId → Empresa.Id

Usuario.EmpresaId → Empresa.Id

Producto.CategoriaId → Categoria.Id

Producto.UnidadMedidaId → UnidadMedida.Id

Requisicion.EmpresaId → Empresa.Id

Requisicion.PeriodoId → Periodo.Id

DetalleRequisicion.RequisicionId → Requisicion.Id

DetalleRequisicion.ProductoId → Producto.Id

DistribucionRequisicion.DetalleRequisicionId
    → DetalleRequisicion.Id

DistribucionRequisicion.SedeId
    → Sede.Id
```

No se deben permitir registros huérfanos.

---

# 36. Restricciones únicas

Como mínimo se deben considerar restricciones de unicidad para:

```text
Periodo
------------
Anio + Mes
```

```text
Requisicion
------------
EmpresaId + PeriodoId
```

```text
UsuarioSede
------------
UsuarioId + SedeId
```

```text
UsuarioRol
------------
UsuarioId + RolId
```

```text
RolPermiso
------------
RolId + PermisoId
```

Para productos y proveedores se deben definir restricciones según las reglas definitivas del negocio.

---

# 37. Índices

Los índices deben crearse sobre columnas utilizadas frecuentemente para:

- búsquedas;
- filtros;
- relaciones;
- ordenamiento;
- restricciones de unicidad.

Inicialmente se deben considerar índices para:

```text
Empresa.Nit
Sede.EmpresaId
Usuario.EmpresaId
Usuario.Correo
Producto.CodigoInterno
Producto.CategoriaId
ProductoProveedor.ProductoId
ProductoProveedor.ProveedorId
Requisicion.EmpresaId
Requisicion.PeriodoId
Requisicion.Estado
DetalleRequisicion.RequisicionId
DistribucionRequisicion.SedeId
Consolidacion.PeriodoId
PedidoProveedor.ProveedorId
PedidoProveedor.ConsolidacionId
Entrega.PedidoProveedorId
```

No se deben crear índices indiscriminadamente.

Los índices deben justificarse por:

- consultas;
- relaciones;
- restricciones;
- comportamiento real del sistema.

---

# 38. Eliminación de información

No se debe utilizar eliminación física indiscriminada.

Para información que participa en procesos históricos se recomienda utilizar:

```text
Activo = true/false
```

o estados equivalentes.

Ejemplo:

```text
Producto activo
Producto inactivo
```

Si un producto dejó de venderse, normalmente debe desactivarse y no eliminarse, porque puede aparecer en:

- requisiciones anteriores;
- consolidaciones;
- pedidos;
- entregas;
- facturas.

La eliminación física solo debe realizarse cuando no comprometa integridad ni trazabilidad y cuando el negocio lo permita.

---

# 39. Fechas y auditoría temporal

Las entidades relevantes deben registrar fechas importantes.

Ejemplos:

```text
FechaCreacion
FechaActualizacion
FechaEnvio
FechaAprobacion
FechaEntrega
```

No todas las tablas necesitan todas estas columnas.

Las fechas deben representar eventos reales del negocio.

No se deben llenar fechas artificialmente solo para cumplir con una estructura.

---

# 40. Estados y transiciones

Los estados almacenados en base de datos no deben permitir que cualquier transición sea válida.

Ejemplo:

```text
BORRADOR → ENVIADA
```

es válida.

Pero:

```text
CERRADA → BORRADOR
```

no debe ser permitida normalmente.

La lógica de transición debe estar controlada por el dominio/backend.

La base de datos puede reforzar integridad estructural, pero la regla completa del flujo pertenece al dominio.

---

# 41. Transacciones

Las operaciones que modifiquen varias entidades relacionadas deben ejecutarse dentro de transacciones cuando la consistencia lo requiera.

Ejemplo:

Enviar una requisición puede implicar:

```text
Actualizar estado
+
Crear historial
+
Registrar auditoría
```

Estas operaciones deben mantener consistencia.

No debe quedar:

```text
Requisición = ENVIADA
Historial = inexistente
```

por una operación parcialmente ejecutada.

---

# 42. Consistencia de cantidades

Las cantidades son uno de los elementos críticos del sistema.

Debe mantenerse trazabilidad entre:

```text
Cantidad solicitada
        ↓
Cantidad distribuida
        ↓
Cantidad necesaria
        ↓
Cantidad pedida
        ↓
Cantidad entregada
        ↓
Cantidad pendiente
```

Estas cantidades no deben mezclarse en un único campo.

Ejemplo:

```text
Necesidad interna: 85
Compra al proveedor: 100
Entregado: 60
Pendiente de entrega: 40
```

El sistema debe poder representar esta situación sin modificar la requisición original.

---

# 43. Snapshot de información histórica

Cuando un documento represente un hecho histórico, no siempre debe depender de información mutable del catálogo.

Ejemplos:

```text
Dirección de entrega
Descripción del proveedor
Datos de contacto
Información comercial
```

Si estos datos pueden cambiar posteriormente, los documentos históricos deben conservar el valor que correspondía al momento del evento cuando sea necesario para trazabilidad.

---

# 44. Normalización

La base de datos debe evitar duplicación innecesaria.

Ejemplo incorrecto:

```text
DetalleRequisicion
------------------
Producto
Categoria
Unidad
```

cuando:

```text
Producto → Categoria
Producto → UnidadMedida
```

ya representan esas relaciones.

Sin embargo, se permite almacenar snapshots cuando existe una necesidad histórica real.

La normalización no debe aplicarse de forma dogmática cuando destruya la trazabilidad documental.

---

# 45. Catálogo y datos históricos

Debe diferenciarse entre:

```text
DATOS MAESTROS ACTUALES
```

y:

```text
DATOS HISTÓRICOS
```

Ejemplo:

El nombre actual de un producto puede cambiar.

Las requisiciones históricas deben seguir siendo interpretables.

Por eso, antes de decidir si un documento guarda únicamente `ProductoId` o también una descripción histórica, se debe validar la necesidad real de auditoría/documentación.

---

# 46. Migraciones

Entity Framework Core será responsable de gestionar la evolución del esquema.

Las migraciones deben:

- ser versionadas;
- poder ejecutarse de forma reproducible;
- reflejar cambios controlados del modelo;
- evitar modificaciones manuales no documentadas;
- ser revisadas antes de aplicarse a ambientes importantes.

No se debe modificar directamente la base de datos de producción sin una estrategia de migración controlada.

---

# 47. Datos iniciales

El sistema puede requerir datos iniciales para:

```text
Roles
Permisos
Categorías
Unidades de medida
```

Estos datos deben ser tratados como datos de configuración inicial.

No se deben insertar empresas reales automáticamente sin una decisión explícita del proyecto.

Por ejemplo, no se debe asumir que:

```text
AUROTECH
FAVIPAQ
COURIERBOX
COURIERBOX ÁLAMOS
```

deben quedar hardcodeadas en un `Seed` definitivo.

---

# 48. Modelo mínimo para el MVP

La primera versión funcional no necesita implementar todo el modelo.

El MVP debe comenzar con:

```text
Empresa
Sede
Usuario
Rol
Permiso
UsuarioSede
UsuarioRol
RolPermiso

Categoria
UnidadMedida
Producto

Periodo

Requisicion
DetalleRequisicion
DistribucionRequisicion
HistorialRequisicion
```

Con este modelo se puede implementar:

```text
Login
   ↓
Empresa
   ↓
Sede
   ↓
Periodo
   ↓
Crear requisición
   ↓
Agregar productos
   ↓
Modificar cantidades
   ↓
Distribuir por sede
   ↓
Enviar
   ↓
Revisión
   ↓
Aprobar / devolver
```

Las tablas de:

```text
Consolidacion
PedidoProveedor
Entrega
Factura
```

se implementarán posteriormente.

---

# 49. Lo que NO debe hacer Claude Code

Claude Code no debe:

- inventar columnas;
- inventar relaciones;
- inventar reglas de negocio;
- crear tablas porque "parecen útiles";
- agregar microservicios;
- crear procedimientos almacenados sin necesidad;
- crear triggers sin autorización;
- agregar Redis;
- agregar CQRS;
- agregar event sourcing;
- agregar otra base de datos;
- hardcodear empresas;
- hardcodear sedes;
- hardcodear usuarios;
- modificar las relaciones definidas en este documento sin autorización.

Si encuentra una necesidad que no está cubierta por este documento, debe reportarla antes de modificar el diseño.

---

# 50. Orden recomendado de implementación

La base de datos debe evolucionar progresivamente.

## Fase 1 — Organización

```text
Empresa
Sede
Usuario
Rol
Permiso
UsuarioSede
UsuarioRol
RolPermiso
```

## Fase 2 — Catálogo

```text
Categoria
UnidadMedida
Producto
```

## Fase 3 — Periodos

```text
Periodo
```

## Fase 4 — Requisiciones

```text
Requisicion
DetalleRequisicion
DistribucionRequisicion
HistorialRequisicion
```

## Fase 5 — Productos no catalogados

```text
SolicitudProductoCatalogo
```

## Fase 6 — Compras

```text
Proveedor
ProductoProveedor
Consolidacion
DetalleConsolidacion
AsignacionConsolidacion
PedidoProveedor
DetallePedidoProveedor
DistribucionPedido
```

## Fase 7 — Entregas

```text
Entrega
DetalleEntrega
DistribucionEntrega
```

## Fase 8 — Facturación

```text
Factura
```

## Fase 9 — Auditoría avanzada

```text
Auditoria
```

---

# 51. Principio de evolución

El modelo inicial debe ser suficientemente sólido para representar correctamente el negocio, pero no debe intentar resolver desde el primer día todos los escenarios futuros.

La evolución recomendada es:

```text
Modelo de dominio
        ↓
Modelo relacional
        ↓
MVP
        ↓
Pruebas con usuarios
        ↓
Validación del proceso real
        ↓
Nuevas reglas
        ↓
Evolución del modelo
```

No se debe diseñar complejidad únicamente porque podría ser necesaria algún día.

---

# 52. Regla fundamental para desarrollo

Antes de crear o modificar una tabla, columna, relación, índice o restricción, se debe responder:

1. ¿Qué necesidad del negocio representa?
2. ¿Qué entidad o relación del dominio representa?
3. ¿Qué regla de negocio soporta?
4. ¿Qué problema resolvería?
5. ¿Existe ya otra estructura que resuelva el mismo problema?
6. ¿Qué impacto tendrá en los datos históricos?
7. ¿Cómo evolucionará si el negocio cambia?

Si estas preguntas no tienen respuesta clara, el cambio debe revisarse antes de implementarse.

---

# 53. Estado del modelo

Este documento define el **modelo de datos objetivo y las reglas de persistencia conocidas actualmente**.

No debe considerarse inmutable.

Las modificaciones deben hacerse de forma controlada y documentada en:

```text
07-decisiones-arquitectura.md
```

cuando representen una decisión arquitectónica relevante.

El objetivo es que la base de datos evolucione junto con el conocimiento real del negocio y no mediante modificaciones improvisadas durante la programación.
