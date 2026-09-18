# Auditoría — Dominio, roles, flujo de negocio y Frontend (2026-09-18)

> Auditoría solicitada explícitamente por el usuario ("Auditoría de alineación del dominio, roles,
> flujo de negocio y frontend de AuropaqPedidos"), realizada **sin modificar código** (regla
> explícita §18 del pedido). Cubre backend (Domain/Application/Infrastructure/Api), frontend
> (rutas, layout, páginas) y documentación (`docs/00` a `docs/08`, `CLAUDE.md`, `progreso.md`).
> Metodología: tres investigaciones paralelas de solo lectura — (A) roles/permisos/protección de
> rutas, (B) modelo de dominio contra `01-reglas-negocio.md`/`02-dominio.md`/`04-base-datos.md`,
> (C) contenido real de cada pantalla del Frontend — con evidencia archivo:línea para cada
> hallazgo, no suposiciones.

---

## 1. Resumen ejecutivo

**El backend está correctamente alineado con el negocio. El Frontend no.**

- **Backend (autorización):** los 30 códigos de permiso documentados existen en código real, cada
  acción de cada uno de los 17 controllers tiene su `[Authorize(Policy=...)]` correspondiente, sin
  huecos. Los 5 roles (`Solicitante`, `Gestor_Requisiciones`, `Compras`, `Recepcion`,
  `Administrador`) están sembrados en la base de datos de desarrollo con exactamente los permisos
  que documenta `06-seguridad.md §53` — verificado contra la base real, sin desviación. **No hay
  ninguna fuga de datos ni operación administrativa que un Solicitante pueda ejecutar
  exitosamente.**
- **Frontend (navegación):** el Frontend **no filtra absolutamente nada por rol**. Cualquier
  usuario autenticado ve las 9 secciones del menú (incluidas Seguridad, Consolidación, Pedidos a
  proveedor, Entregas, Facturación) sin distinción. Esto está **documentado como un hueco conocido
  y deliberado** en el propio código (`AppLayout.tsx`), no es un descuido oculto — pero contradice
  lo que el propio proyecto ya especificó (`06-seguridad.md §21`: React "debería" ocultar botones/
  pantallas por permiso como UX, sin ser la única protección). Un Solicitante ve el panel completo
  y solo descubre que no puede actuar después de hacer clic y recibir un 403.
- **Causa raíz estructural:** el JWT no lleva claims de rol/permiso (`sub`/`email`/`name`/
  `empresaId` únicamente), y no existe ningún endpoint de "mis permisos" que un Solicitante pueda
  consultar (el único endpoint de roles, `GET /usuarios/{id}/roles`, exige `SEGURIDAD_VER`, que el
  Solicitante no tiene). **Esto no es "agregar un `if`" en React — es una decisión estructural
  pendiente.**
- **El rol "Solicitante" NO es un concepto nuevo.** Ya está formalmente decidido desde el
  2026-09-15 (RN-060/ADR-063, matriz completa en `06-seguridad.md §53`), con permisos ya
  sembrados y probados. El hueco es puramente de **implementación en el Frontend**, no de
  **definición de negocio**.
- **Modelo de dominio:** en general, sólido y ya prueba varias de las distinciones que pedía la
  auditoría (`CantidadNecesaria` vs `CantidadPedida` ya están correctamente separadas e
  inmutables donde corresponde, RN-031/RN-042/D-12). Se encontraron 2 huecos funcionales reales
  (no ambigüedades — huecos de construcción sobre decisiones ya tomadas): la relación
  `ProductoProveedor` existe y es administrable en Catálogo, pero **nunca se usa** al construir un
  `PedidoProveedor`; y `SolicitudProductoCatalogo` (homologación) es un proceso **totalmente
  desconectado** de `Requisicion`/`DetalleRequisicion`, sin ningún campo que los vincule.
  Adicionalmente, dos decisiones de negocio ya están marcadas como **abiertas** en los propios
  documentos (no cerradas por este equipo todavía): confirmación de recepción por el Solicitante
  ("Pendiente 4"), y facturación 1:N vs N:N para el caso general ("Decisión 1 de Facturación").

---

## 2. Flujo real esperado

```text
Solicitante
   ↓
Requisición (solo productos ya catalogados — DetalleRequisicion exige un Producto real)
   ↓
Revisión del Gestor (Aprobar/Devolver — solo sobre la Requisición, sin relación con homologación)
   ↓
   ⚠ "Homologación / creación de producto" NO está enlazada al paso anterior hoy.
     Es un proceso paralelo e independiente: SolicitudProductoCatalogo, creado por el
     Solicitante en una pantalla separada ("Solicitudes de producto"), sin ninguna referencia
     a ninguna Requisición. El producto homologado solo podrá usarse en una Requisición
     FUTURA, nunca retroactivamente en la que originó la necesidad.
   ↓
Consolidación (opera solo sobre Requisiciones ya aprobadas — estructuralmente no puede
  incluir productos sin homologar, porque DetalleRequisicion nunca los admite)
   ↓
Necesidad de compra (CantidadNecesaria, inmutable, derivada de la consolidación)
   ↓
PedidoProveedor (CantidadPedida, independiente y editable por Compras — RN-031/RN-042/D-12,
  ya implementado correctamente)
   ⚠ Al elegir qué código usa el proveedor para el producto, el sistema NO consulta
     ProductoProveedor — la relación existe pero no se usa aquí (contradice la intención de RN-041).
   ↓
Entrega (con fotografía histórica de dirección/ciudad/contacto — RN-035/ADR-019, correcto)
   ↓
   ⚠ "Confirmación del Solicitante" NO existe. Entrega solo tiene 2 estados
     (Registrada/Anulada), ambos del lado de Compras/Recepción. Esto está reconocido como
     decisión de negocio TODAVÍA ABIERTA en 01-reglas-negocio.md ("Pendiente 4 — Recepción de
     mercancía"), diferida explícitamente el 2026-09-11 — no fue una omisión de este código.
   ↓
Registro de Factura (validada contra CantidadPedida — lo pedido — no contra lo efectivamente
  entregado; decisión explícitamente provisional de alcance MVP, documentada como tal)
```

**Corrección al flujo propuesto por el usuario:** el paso "Homologación / creación de producto si
aplica" no puede dibujarse como una etapa lineal *dentro* del camino de una Requisición — hoy es
un carril completamente aparte. Si el negocio quiere que sea parte del mismo flujo (ej.: "este
producto de mi requisición necesita homologarse"), eso requiere una decisión de diseño nueva (ver
Ambigüedad 1, sección 8).

---

## 3. Matriz de responsabilidades

> El sistema real ya tiene **5 roles**, no 2 (`Solicitante`, `Gestor_Requisiciones`, `Compras`,
> `Recepcion`, `Administrador` — RN-060/ADR-063, `06-seguridad.md §53`, verificado sembrado en la
> base de datos). Eso es más granular — y más correcto — que el MVP de 2 roles que describe el
> pedido de auditoría. La tabla siguiente usa las 2 columnas pedidas; "Gestor/Administrador" agrupa
> los 4 roles no-Solicitante, que en la implementación real ya están further separados entre sí.

| Funcionalidad | Solicitante | Gestor/Administrador |
|---|---|---|
| Crear requisición | ✅ (`REQUISICION_CREAR`) | ❌ (no es su responsabilidad) |
| Ver requisiciones propias | ✅ (`REQUISICION_VER` + alcance por empresa) | — |
| Ver requisiciones de la empresa (revisión) | ❌ | ✅ `Gestor_Requisiciones` (`REQUISICION_VER`, `APROBAR`, `DEVOLVER`) |
| Administrar catálogo (Producto/Categoría/Unidad) | ❌ | ✅ (`PRODUCTO_CREAR/EDITAR`) |
| Solicitar producto no catalogado | ✅ (`PRODUCTO_SOLICITAR`) | — |
| Homologar productos (resolver solicitud) | ❌ | ✅ (`PRODUCTO_VER/CREAR`) |
| Crear productos | ❌ | ✅ (`PRODUCTO_CREAR`) |
| Administrar proveedores | ❌ | ✅ (`PROVEEDOR_VER/CREAR/EDITAR`) |
| Administrar código proveedor (ProductoProveedor) | ❌ | ✅ (`PRODUCTO_VER/CREAR/EDITAR`, mismo permiso que Producto) |
| Consolidar | ❌ | ✅ `Compras` (`PEDIDO_CONSOLIDAR`) |
| Crear pedido proveedor | ❌ | ✅ `Compras` (`PEDIDO_CREAR/ENVIAR/CERRAR/CANCELAR`) |
| Gestionar entregas | ❌ | ✅ `Recepcion`/`Compras` (`ENTREGA_REGISTRAR/ANULAR`) |
| Confirmar recepción | 🔵 **No existe hoy — ver Ambigüedad 2** | 🔵 **No existe hoy** |
| Registrar factura | ❌ | ✅ `Compras` (`FACTURA_REGISTRAR/ANULAR`) |
| Administrar usuarios | ❌ | ✅ `Administrador` (`SEGURIDAD_ADMINISTRAR`) |
| Administrar permisos | ❌ | ✅ `Administrador` (`SEGURIDAD_ADMINISTRAR`) |

Justificación: cada fila corresponde exactamente a un permiso ya sembrado en `Roles`/`Permisos`/
`RolesPermisos` en la base de datos, verificado por consulta directa — no se asignó nada por
suposición.

---

## 4. Problemas del Frontend

### 🔴 CRÍTICO — F1: Ningún filtrado de navegación/rutas por rol o permiso

```text
Problema:
Un Solicitante ve las 9 secciones del menú (Catálogo, Requisiciones, Revisión, Solicitudes de
producto, Consolidación, Pedidos a proveedor, Entregas, Facturación, Seguridad) exactamente igual
que un Administrador, y puede navegar a cualquiera de ellas.

Evidencia:
- frontend/src/routes/ProtectedRoute.tsx:9-17 — el único chequeo es "if (!usuario)"; sin rol,
  sin permiso.
- frontend/src/components/AppLayout.tsx:10-20,29 — ENLACES es un array estático sin filtrar,
  mapeado incondicionalmente.
- frontend/src/routes/AppRoutes.tsx:20-37 — las 9 rutas comparten el mismo único
  <ProtectedRoute> sin diferenciación.

Causa:
El JWT no lleva claims de rol/permiso (backend/src/Infrastructure/Security/
JwtTokenGenerator.cs:42-45: solo sub/email/name/empresaId) y no existe ningún endpoint de
autoconsulta ("mis permisos") sin restricción — el único endpoint de roles exige SEGURIDAD_VER,
que un Solicitante no tiene. Es un hueco estructural, documentado como tal en el propio código
(AppLayout.tsx:4-9), no un descuido oculto.

Impacto:
El backend rechaza correctamente cada acción administrativa (sin fuga de datos), pero el
Solicitante ve botones y formularios que fallarán con 403 solo después de hacer clic — mala UX
directamente contraria a lo que 06-seguridad.md §21 ya especifica.

Corrección propuesta:
Ver TASK-101/TASK-102 (sección 10).
```

### 🔴 CRÍTICO — F2: `SolicitudesProductoPage.tsx` mezcla la acción del Solicitante y del Gestor en una sola pantalla, sin diferenciar

```text
Problema:
Una única pantalla muestra el formulario "Solicitar producto" (para Solicitante,
PRODUCTO_SOLICITAR) y el panel "Resolver solicitudes pendientes" (para Gestor, PRODUCTO_VER/
CREAR) al mismo tiempo, para cualquier usuario que abra la ruta.

Evidencia:
frontend/src/pages/SolicitudesProductoPage.tsx:8-12 — el propio comentario de cabecera admite
textualmente: "un Solicitante que abra esta pantalla verá el botón 'Solicitar producto' funcionar
y la lista de pendientes fallar con 403, o viceversa."

Causa:
Misma causa raíz que F1 — sin datos de rol/permiso en el cliente, no hay forma de decidir qué
mitad de la pantalla mostrar.

Impacto:
Confusión directa para el usuario final; es el ejemplo más concreto del "Caso B" que pidió
revisar la auditoría.

Corrección propuesta:
Ver TASK-103 (sección 10).
```

### 🟠 IMPORTANTE — F3: `RequisicionesPage.tsx` no tiene ningún enlace hacia "Solicitudes de producto"

```text
Problema:
Un Solicitante que necesita un producto no catalogado tiene que saber, por su cuenta, que existe
un menú separado ("Solicitudes de producto") — no hay ningún botón/enlace desde el formulario de
agregar detalle de una Requisición hacia ese flujo.

Evidencia:
frontend/src/components/requisiciones/AgregarDetalleForm.tsx:59-71 (selector limitado al catálogo
existente, SelectorCatalogo) sin ninguna referencia a solicitudesProductoService ni a la ruta
/solicitudes-producto en todo RequisicionPanel.tsx (grep confirmado, cero coincidencias).

Impacto:
UX pobre, no un problema de seguridad. Depende de cómo se resuelva la Ambigüedad 1 (sección 8) —
si se decide vincular ambos procesos, este problema se resuelve como parte de esa tarea.
```

### 🟠 IMPORTANTE — F4: `PedidosProveedorPage.tsx` nunca muestra el código del proveedor (`ProductoProveedor`)

```text
Problema:
Al agregar un producto a un PedidoProveedor, el selector solo muestra
"{nombreProducto} (necesario: {cantidadNecesaria})" — el nombre interno del producto, nunca el
código que usa ese proveedor específico para identificarlo.

Evidencia:
frontend/src/pages/PedidosProveedorPage.tsx:300-307. Cero referencias a ProductoProveedor/
CodigoProveedor en todo el archivo ni en PedidoDetalleRow.tsx (grep confirmado).

Causa:
El backend tampoco expone esa relación en este flujo — ver Problema de dominio D1 (sección 6).
Esto es un problema conjunto de dominio + frontend, no solo de UI.

Impacto:
Contradice la intención ya documentada de RN-041 ("el sistema puede mostrar información de los
proveedores asociados al producto"). Un Gestor de Compras no puede confirmar visualmente qué
código debe comunicarle al proveedor.
```

### Verificado — NO son problemas

- `ConsolidacionPage.tsx`, `EntregasPage.tsx`, `FacturacionPage.tsx`: cada uno se mantiene
  dentro de su propio concepto, sin mezclar responsabilidades (confirmado por grep exhaustivo,
  sin referencias cruzadas indebidas).
- `RevisionPage.tsx` no muestra homologación — pero esto es consistente con que
  `DetalleRequisicion` estructuralmente nunca puede contener un producto sin homologar (ver
  sección 6). No es un enlace roto, es que la situación que uniría ambas pantallas no puede
  ocurrir hoy.
- Backend: cada acción administrativa que un Solicitante podría alcanzar desde el Frontend está
  correctamente protegida por su permiso real — no hay ninguna operación que se ejecute sin
  autorización.

---

## 5. Problemas del backend

**No se encontró ningún endpoint ni caso de uso que permita una operación que debería estar
restringida.** Los 30 permisos documentados existen en código, cada controller los aplica
correctamente, y la matriz de roles sembrada en la base de datos coincide exactamente con
`06-seguridad.md §53`. La suite de pruebas de integración (657/657, incluyendo pruebas de
regresión 401/403 dedicadas agregadas hoy mismo para los endpoints `GET` nuevos) confirma esto
empíricamente, no solo por lectura de código.

El único hallazgo de backend relevante para esta auditoría es la ausencia de un endpoint de
"mis permisos"/"mis roles" sin restricción — necesario para resolver F1 del Frontend (ver
TASK-101).

---

## 6. Problemas del dominio

### 🟠 IMPORTANTE — D1: `ProductoProveedor` nunca se usa al construir un `PedidoProveedor`

```text
Problema:
DetallePedidoProveedor solo referencia Producto — no tiene ProductoProveedorId ni ningún campo
para el código del proveedor. AgregarDetallePedidoProveedorRequest tampoco lo acepta.
PedidoProveedor.AgregarDetalle nunca consulta ProductoProveedor pese a conocer el Proveedor del
pedido en el mismo objeto.

Evidencia:
backend/src/Domain/Entities/DetallePedidoProveedor.cs:17-19; Dtos/
AgregarDetallePedidoProveedorRequest.cs; PedidoProveedor.cs:128-143.

Contexto importante:
Este hueco YA estaba presente en el diseño original — docs/04-base-datos.md:1100-1113 y
docs/02-dominio.md:829-844 tampoco especifican este campo. No es código que se desvió de la
especificación; es un hueco que viene desde el propio diseño documentado, contradiciendo la
intención de RN-041 sin que RN-041 llegara a especificar el mecanismo concreto.

Entidades afectadas: DetallePedidoProveedor, ProductoProveedor.
```

### Verificado — Correctos, sin drift

- **`SolicitudProductoCatalogo` / homologación**: `DetalleRequisicion` exige un `Producto` real
  (FK no anulable, `DetalleRequisicion.cs:11,31-34`) — no admite texto libre. Esto confirma que el
  flujo de homologación **debe** ser un proceso aparte, no una extensión de
  `DetalleRequisicion` — pero hoy ese proceso aparte (`SolicitudProductoCatalogo.cs:8-58`) no
  tiene ninguna referencia a `Requisicion`/`DetalleRequisicion` (ver Ambigüedad 1, sección 8, para
  si esa desconexión es intencional o debe cerrarse).
- **`ProductoProveedor`** (como entidad en sí, fuera del flujo de pedido): forma correcta, coincide
  exactamente con `CLAUDE.md §23` y `docs/02-dominio.md §11`.
- **`Consolidacion`/`DetalleConsolidacion`/`AsignacionConsolidacion`**: trazabilidad correcta
  (Consolidación → Detalle → Asignación → DetalleRequisicion → Empresa/Sede), sin modificar la
  requisición original, tal como exige `CLAUDE.md §25`.
- **`CantidadNecesaria` vs `CantidadPedida`**: `DetallePedidoProveedor.cs:18-19` — la primera es
  inmutable (solo `get`), la segunda editable por Compras (`private set`), sin restricción
  mín/máx entre ambas — exactamente lo que decide RN-031/RN-042/D-12 (`DECISIÓN CERRADA`).
- **`Entrega`/`DetalleEntrega`/`DistribucionEntrega`**: fotografía histórica real
  (`DireccionEntrega`/`CiudadEntrega`/`ContactoEntrega`, copiadas de `Sede` al construirse, nunca
  releídas después) — coincide con RN-035/ADR-019.
- **`Factura`/`DetalleFactura`**: sin ningún campo contable/de pago (`CuentaContable`, `Pago`,
  `Conciliacion`, etc.) — confirmado por grep, cero coincidencias. Respeta el alcance pedido en la
  sección 10 del brief de auditoría.

---

## 7. Productos fuera del catálogo — análisis profundo (sección 5/7 del pedido)

El modelo actual **ya distingue** tres conceptos, pero como **dos procesos desconectados**, no
como un pipeline lineal:

```text
Producto solicitado (SolicitudProductoCatalogo — creado libremente por el Solicitante,
  sin restricción de catálogo, sin relación con ninguna Requisición)
        ↓ (el Gestor homologa/crea/rechaza — RN-024/025/026, ya implementado)
Producto homologado (Producto real en el catálogo)
        ↓ (recién ahora puede usarse)
Producto comprado (referenciado en DetalleRequisicion → ... → DetallePedidoProveedor)
```

**No hace falta crear una entidad nueva** para representar esto — el modelo de 3 etapas que pedía
el usuario ya existe conceptualmente. Lo que falta es el **enlace** entre la primera etapa y la
Requisición que la originó (si es que el negocio quiere ese enlace — ver Ambigüedad 1). Hoy, si un
Solicitante necesita un producto no catalogado:

1. Debe ir a una pantalla completamente distinta ("Solicitudes de producto") a registrar la
   necesidad, sin poder asociarla a la Requisición en la que realmente la necesita.
2. Su Requisición actual **no puede** incluir ese producto hasta que el Gestor lo homologue.
3. Una vez homologado, el Solicitante tendría que **volver a la Requisición y agregarlo
   manualmente** como un producto más del catálogo — nadie se lo recuerda ni se lo sugiere.

Esto es funcionalmente correcto (nunca se inventa un producto no autorizado en una Requisición,
`CLAUDE.md §22`) pero operativamente desconectado. **Esta es la Ambigüedad 1** — decidir si el
negocio quiere cerrar ese enlace o si el flujo desconectado actual es aceptable.

---

## 8. Ambigüedades (requieren decisión del usuario — no se resolvieron por suposición)

### 🔵 Ambigüedad 1 — ¿Debe la solicitud de un producto no catalogado enlazarse a la Requisición que la originó?

**Alternativas:**
```text
A. Mantener el diseño actual: dos procesos completamente independientes. El Solicitante
   solicita el producto por su cuenta, y una vez homologado, debe agregarlo manualmente a
   una Requisición (la misma u otra, en el mismo periodo o uno posterior).
B. Agregar un campo opcional de enlace (ej. RequisicionId/DetalleRequisicionId nullable en
   SolicitudProductoCatalogo) solo para trazabilidad — "este producto se solicitó porque lo
   necesitaba tal Requisición" — sin que el Gestor tenga que actuar diferente.
C. Flujo guiado: al intentar agregar un producto no encontrado en el selector de una
   Requisición, el Frontend ofrece "Solicitar este producto" y, una vez homologado, lo
   agrega automáticamente al DetalleRequisicion correspondiente (requiere una cantidad
   "pendiente de homologación" en la Requisición mientras tanto — cambio de modelo más
   grande, nuevo estado a definir).
```
No hay recomendación única aquí porque cambia el modelo de datos (C) o al menos agrega una
relación nueva (B) — corresponde decidir al equipo, no inventarlo (`CLAUDE.md §51`).

### 🔵 Ambigüedad 2 — Confirmación de recepción por el Solicitante (ya reconocida como abierta en los documentos, no nueva)

Ya documentada en `01-reglas-negocio.md` como "Pendiente 4 — Recepción de mercancía", diferida
explícitamente el 2026-09-11: `Entrega.Estado` se mantiene deliberadamente en solo
`Registrada`/`Anulada`, sin estados de aceptación/rechazo/faltante/daño por parte de quien recibe.
No se descartó, solo se pospuso. **Requiere retomar esa decisión, no es un hallazgo nuevo de esta
auditoría** — se incluye aquí porque el usuario preguntó explícitamente por ella (sección 9/12 del
pedido).

### 🔵 Ambigüedad 3 — Facturación 1:1 (PedidoProveedor→Factura) vs. N:N (ya reconocida como abierta)

`Factura.cs:6-10` cita textualmente que el caso general de facturación consolidada N:N sigue
"PENDIENTE DE APROBACIÓN" — la implementación actual (`PedidoProveedor 1─N Factura`) es
explícitamente un alcance provisional de MVP, no una decisión final. Igual que la Ambigüedad 2,
no es un hallazgo nuevo — es una decisión que el equipo ya sabía que faltaba y quedó pendiente.

### No es una ambigüedad — ya está decidido, solo falta implementar

El modelo de 5 roles, la separación Solicitante/Gestor, y la matriz completa de permisos
(RN-060/ADR-063, `06-seguridad.md §53`) **ya están formalmente decididos** desde el 2026-09-15.
**No hay que volver a decidir esto** — solo falta construir el filtrado en el Frontend (F1) y,
antes que eso, la pieza estructural que lo habilita (TASK-101).

---

## 9. Cambios recomendados (por fases)

```text
FASE 1 — Seguridad y roles visibles en el cliente
  Habilitar que el Frontend sepa qué rol/permisos tiene el usuario (TASK-101), sin lo cual
  ninguna otra fase de Frontend puede resolverse correctamente.

FASE 2 — Separación de paneles
  Filtrar navegación (TASK-102) y resolver la pantalla mixta de Solicitudes de producto
  (TASK-103), usando lo construido en Fase 1.

FASE 3 — Catálogo y homologación
  Decidir Ambigüedad 1 (sección 8) y, según lo decidido, construir el enlace
  Requisición ↔ SolicitudProductoCatalogo (TASK-104, condicional a la decisión).

FASE 4 — Pedido a proveedor y ProductoProveedor
  Cerrar D1: usar ProductoProveedor al construir un PedidoProveedor (TASK-105).

FASE 5 — Entrega y confirmación de recepción
  Retomar Ambigüedad 2 (Pendiente 4) — decisión de negocio antes de cualquier código
  (TASK-106, bloqueada hasta la decisión).

FASE 6 — Factura
  Retomar Ambigüedad 3 solo si/cuando el negocio necesite el caso N:N — sin evidencia de
  necesidad real todavía (`CLAUDE.md §71`), no es una tarea a programar ahora.

FASE 7 — Prueba end-to-end con los 5 roles reales
  Una vez cerradas las Fases 1-2, repetir la verificación manual en navegador (ya hecha hoy
  para el flujo Consolidación→Pedido→Entrega→Factura) esta vez explícitamente como
  Solicitante y confirmar que el panel administrativo ya no es visible ni navegable desde el
  menú.
```

No se incluye una "Fase 8 — prueba end-to-end" separada porque cada fase ya exige su propia
verificación (`CLAUDE.md §45`); la Fase 7 cubre el cierre integral pedido.

---

## 10. Tareas propuestas

### TASK-101 — Exponer rol/permisos del usuario autenticado al Frontend

- **Objetivo:** que el cliente pueda saber, sin adivinar, qué puede hacer el usuario actual.
- **Problema que resuelve:** causa raíz de F1/F2 — hoy es estructuralmente imposible filtrar nada
  en el Frontend.
- **Capas afectadas:** Infrastructure (generación de JWT) o Application+Api (endpoint nuevo),
  Frontend (AuthContext).
- **Entidades afectadas:** ninguna nueva — lectura de `UsuarioRol`/`RolPermiso` ya existentes.
- **Endpoints afectados:** opción A — agregar claims al JWT en el login; opción B — nuevo
  `GET /api/v1/auth/mis-permisos` (o similar), sin requisito de permiso más allá de estar
  autenticado. **Requiere decisión del equipo** (impacto arquitectónico — cambia el contrato del
  JWT o agrega un endpoint nuevo, `CLAUDE.md §60/§61`) antes de implementar.
- **Frontend afectado:** `AuthContext.tsx`, `types/auth.ts`.
- **Dependencias:** ninguna — es la base de todo lo demás.
- **Criterios de aceptación:** un Solicitante autenticado puede, del lado del cliente, saber que
  no tiene `SEGURIDAD_VER`/`PEDIDO_CONSOLIDAR`/etc., sin necesidad de intentar la acción primero.
- **Pruebas necesarias:** prueba de integración (backend) confirmando el nuevo claim/endpoint;
  prueba de Frontend (Vitest) sobre `AuthContext` con un usuario mockeado.

### TASK-102 — Filtrar navegación y rutas por permiso real

- **Objetivo:** que `AppLayout`/`ProtectedRoute` oculten lo que el usuario no puede usar.
- **Problema que resuelve:** F1.
- **Capas afectadas:** Frontend únicamente.
- **Endpoints afectados:** ninguno nuevo — consume lo agregado en TASK-101.
- **Frontend afectado:** `AppLayout.tsx`, `ProtectedRoute.tsx`, `AppRoutes.tsx`.
- **Dependencias:** TASK-101.
- **Criterios de aceptación:** un Solicitante ve solo Requisiciones, Solicitudes de producto y
  Catálogo (solo lectura); un Administrador ve las 9 secciones; navegar directamente a una URL
  administrativa sin permiso redirige o muestra un mensaje claro, no un 403 después de interactuar.
- **Pruebas necesarias:** prueba de Frontend con Testing Library simulando cada rol.

### TASK-103 — Separar `SolicitudesProductoPage` en vista de Solicitante y vista de Gestor

- **Objetivo:** que cada usuario vea solo la mitad de la pantalla que le corresponde.
- **Problema que resuelve:** F2.
- **Capas afectadas:** Frontend únicamente.
- **Frontend afectado:** `SolicitudesProductoPage.tsx`, `SolicitarProductoForm.tsx`,
  `ResolverSolicitudPanel.tsx`.
- **Dependencias:** TASK-101.
- **Criterios de aceptación:** un Solicitante solo ve el formulario de solicitar; un Gestor solo
  ve el panel de resolver (o ambos si tiene ambos permisos, sin fallar nunca con 403 visible).
- **Pruebas necesarias:** prueba de Frontend por rol.

### TASK-104 — Enlazar Requisición con Solicitud de producto no catalogado (condicional a Ambigüedad 1)

- **Objetivo:** cerrar la desconexión descrita en la sección 7.
- **Problema que resuelve:** F3 y la Ambigüedad 1.
- **Bloqueada por:** decisión del equipo sobre cuál alternativa (A/B/C, sección 8) implementar.
- **Capas afectadas:** depende de la alternativa elegida — mínimo Frontend (A), hasta Domain+
  Application+Api+Frontend (C).
- **Pruebas necesarias:** depende de la alternativa.

### TASK-105 — Usar `ProductoProveedor` al construir un `PedidoProveedor`

- **Objetivo:** que el código del proveedor sea visible/seleccionable al agregar un detalle a un
  pedido.
- **Problema que resuelve:** D1 y F4.
- **Capas afectadas:** Domain (`DetallePedidoProveedor`), Application
  (`AgregarDetallePedidoProveedorUseCase`), Api (DTO), Frontend (`PedidosProveedorPage.tsx`,
  `PedidoDetalleRow.tsx`).
- **Entidades afectadas:** `DetallePedidoProveedor` (nuevo campo opcional
  `ProductoProveedorId`/snapshot del código), `ProductoProveedor` (sin cambios).
- **Dependencias:** ninguna — `ProductoProveedor` ya existe y está completo (cerrado hoy mismo).
- **Criterios de aceptación:** al agregar un detalle a un pedido de un proveedor específico, el
  formulario muestra (si existe) el código de ese proveedor para el producto elegido.
- **Pruebas necesarias:** prueba de dominio (Domain.Tests), de caso de uso (Application.Tests), y
  manual en navegador.

### TASK-106 — Retomar decisión de confirmación de recepción (bloqueada)

- **Objetivo:** decidir si/cómo el Solicitante confirma la recepción física de una Entrega.
- **Problema que resuelve:** Ambigüedad 2 ("Pendiente 4").
- **Bloqueada por:** decisión de negocio explícita del equipo — no implementar nada hasta
  entonces (`CLAUDE.md §51`).
- **Capas afectadas (si se aprueba):** Domain (`Entrega.Estado` nuevos valores o entidad
  `ConfirmacionEntrega`), Application, Api, Frontend (`EntregasPage.tsx`).
- **Pruebas necesarias:** a definir según la decisión.

---

## 11. Nota de proceso

Esta auditoría **no modificó ningún archivo de código**, solo se creó este documento. Los
próximos pasos (implementar TASK-101 en adelante) deben seguir el protocolo habitual de
`CLAUDE.md §50` uno por uno, en el orden de la sección 9, esperando aprobación explícita antes de
cada fase con impacto arquitectónico (TASK-101 en particular).

---

## 12. Plan de ejecución propuesto (2026-09-18)

> Convierte las Fases 1-7 (sección 9) en una secuencia concreta, indicando qué necesita una
> decisión del usuario antes de empezar y qué se puede ejecutar ya. Solo una decisión bloquea de
> verdad el arranque (Decisión A); el resto puede resolverse en paralelo o después sin detener el
> trabajo.

### Qué bloquea qué

```text
Decisión A (arquitectónica, TASK-101)
      ↓
TASK-101 (exponer rol/permisos al Frontend)
      ↓
   ┌──────────────┬──────────────────┐
   ↓              ↓                  ↓
TASK-102       TASK-103          (nada más depende de 101)
(filtrar nav)  (separar pantalla
                mixta)

TASK-105 (usar ProductoProveedor en pedidos) — NO depende de la Decisión A ni de TASK-101,
  puede ejecutarse en paralelo o antes, es independiente.

Decisión B (Ambigüedad 1) → TASK-104 — no bloquea nada de lo anterior, puede decidirse después.

Decisión C (Ambigüedad 2, "Pendiente 4") y Decisión D (Ambigüedad 3, facturación N:N) —
  recomendación: NO reabrirlas todavía (ver más abajo).
```

### Decisión A — necesaria para empezar TASK-101 (arquitectónica, requiere tu aprobación)

**DECISIÓN A EVALUAR**
¿Cómo debe el Frontend enterarse del rol/permisos del usuario autenticado?

**ALTERNATIVAS**
```text
A. Agregar claims de rol/permiso al JWT en el login.
B. Nuevo endpoint de autoconsulta (ej. GET /api/v1/auth/mis-permisos), sin más requisito que
   estar autenticado, que devuelve los permisos reales del usuario consultando la base de
   datos en cada llamada.
```

**RECOMENDACIÓN: B — nuevo endpoint de autoconsulta.**

Razones:
- ADR-058 (autorización por permisos) ya decidió explícitamente que la fuente de verdad de
  permisos es la base de datos, **sin caché** — meter permisos dentro del JWT los "cachearía"
  hasta el siguiente login, pudiendo quedar desactualizados si un Administrador cambia el rol
  de alguien a mitad de sesión. Un endpoint de autoconsulta respeta el mismo principio ya
  decidido.
- Cambiar el contenido del JWT es modificar un contrato existente sin necesidad real
  (`CLAUDE.md §61`) — un endpoint nuevo es puramente aditivo, sin riesgo de romper nada que ya
  dependa del JWT actual.
- Ya existe el patrón exacto para esto (`GET /usuarios/{id}/roles`), solo que exige
  `SEGURIDAD_VER` — lo nuevo sería la misma idea, pero para "a mí mismo", sin esa restricción.

**CONSECUENCIAS**
- Ventaja: cero riesgo de permisos desactualizados; no toca el contrato del JWT.
- Costo: una llamada adicional al backend al cargar la sesión (impacto de rendimiento
  insignificante a esta escala, `CLAUDE.md §44`).

**EVOLUCIÓN**
Si en el futuro el rendimiento de esa llamada extra se vuelve un problema real (con evidencia,
no antes), se puede revisar la Alternativa A como optimización — no al revés.

### Decisiones B/C/D — no bloquean el arranque, mi recomendación es no tocarlas todavía

- **Ambigüedad 1** (enlace Requisición ↔ Solicitud de producto): recomiendo empezar por la
  **Alternativa A** (dejar los procesos desconectados como están) y solo agregar el enlace visual
  simple de F3 (un botón "Solicitar producto no catalogado" desde el formulario de Requisición,
  sin cambiar el modelo de datos) — sin evidencia de que el equipo necesite trazabilidad formal
  entre ambos todavía (`CLAUDE.md §71`). Si aparece esa necesidad real, se revisa como decisión
  aparte.
- **Ambigüedad 2** ("Pendiente 4", confirmación de recepción) y **Ambigüedad 3** (facturación
  N:N): ambas ya estaban explícitamente diferidas por el equipo antes de esta auditoría.
  Recomiendo **no reabrirlas ahora** — no es parte de lo que motivó este pedido de auditoría
  (roles/paneles/homologación/ProductoProveedor) y reabrirlas sin necesidad real sería
  sobrealcance (`CLAUDE.md §49/§52`).

### Plan de ejecución concreto (una vez resuelta la Decisión A)

```text
1. ~~Decisión A~~ — hecho (2026-09-18): aprobada la opción B (endpoint de autoconsulta).
2. ~~TASK-101~~ — hecho (2026-09-18, solo backend): ObtenerMisPermisosUseCase +
   GET /api/v1/auth/mis-permisos, [Authorize] sin permiso específico. 665/665 pruebas
   correctas (+8: 5 Application.Tests + 3 Api.Tests). Falta el consumo desde el Frontend, que
   llega con TASK-102/103.
3. ~~TASK-105~~ — hecho (2026-09-18): `DetallePedidoProveedor.CodigoProveedorUtilizado`
   (snapshot, mismo criterio que `DistribucionEntrega`), resuelto automáticamente en
   `AgregarDetallePedidoProveedorUseCase`, mostrado en `PedidoDetalleRow.tsx`. Verificado en
   navegador: el código aparece solo cuando existe la relación, y queda fijo aunque el catálogo
   cambie después. 672/672 pruebas correctas (+7: 3 Domain.Tests + 3 Application.Tests +
   1 Api.Tests).
4. ~~TASK-102~~ — hecho (2026-09-18): `GET /auth/mis-permisos` consultado en `AuthContext`;
   `routes/enlaces.ts` (único mapeo ruta→permiso, usado por `AppLayout` y `AppRoutes`);
   `RutaConPermiso` protege cada ruta además del menú. Verificado con las 2 cuentas reales
   (Administrador ve las 9 secciones; Solicitante ve solo lo que sus permisos reales permiten,
   navegar directo por URL a una sección sin permiso muestra "Sin acceso"). 15/15 pruebas de
   Frontend correctas (+8, primeras pruebas de componente del proyecto).
5. ~~TASK-103~~ — hecho (2026-09-18): cada bloque de `SolicitudesProductoPage.tsx` se muestra
   según el permiso real correspondiente (`PRODUCTO_SOLICITAR`/`PRODUCTO_VER`/`PRODUCTO_CREAR`
   — 3 permisos, no 2; Solicitante sí tiene `PRODUCTO_VER` y puede ver la bandeja, solo no
   resolverla). Verificado con las 2 cuentas reales. 18/18 pruebas de Frontend correctas (+3).
6. ~~F3~~ — hecho (2026-09-18): enlace "¿No encuentras el producto? Solicítalo al catálogo" en
   `AgregarDetalleForm.tsx`, hacia `/solicitudes-producto` — navegación simple, sin enlazar la
   solicitud a la Requisición (Alternativa A de la Ambigüedad 1, sección 8).
7. ~~Prueba end-to-end manual en navegador como Solicitante real~~ — hecho (2026-09-18, cubierto
   ya por la verificación de TASK-102 y reconfirmado aquí): `carlos.taskdemo@auropaq.com` —
   panel administrativo (Revisión/Facturación/Seguridad) ni visible en el menú ni navegable por
   URL directa ("Sin acceso"). Enlace del paso 6 probado en vivo desde una Requisición real.
```

**Plan de ejecución cerrado por completo (2026-09-18).** TASK-101/102/103/105 implementadas y
verificadas; F3 (paso 6) y la prueba end-to-end final (paso 7) hechas. Quedan intencionalmente
sin tocar, por no haber sido reabiertas por el usuario: Ambigüedad 1 en su forma completa
(Alternativas B/C), Ambigüedad 2 ("Pendiente 4", confirmación de recepción — TASK-106) y
Ambigüedad 3 (facturación N:N) — ver sección 8.

Cada paso sigue el protocolo de `CLAUDE.md §50`: plan → implementar solo lo aprobado → probar →
reportar, uno a la vez.
