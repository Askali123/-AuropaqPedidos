# 08 — TAREAS DEL PROYECTO

## 1. Propósito

Este documento contiene las tareas de implementación del sistema AuropaqPedidos.

Las tareas convierten las decisiones de:

- negocio;
- dominio;
- arquitectura;
- base de datos;
- API;
- seguridad;

en unidades pequeñas y controlables de trabajo.

El objetivo es evitar que una tarea demasiado grande provoque:

- cambios no autorizados;
- arquitectura improvisada;
- código innecesario;
- funcionalidades fuera del alcance;
- cambios simultáneos en demasiados módulos;
- retrabajo.

---

# 2. Regla principal

Claude Code no debe interpretar este documento como una autorización para implementar todo el proyecto de una sola vez.

Debe trabajar **tarea por tarea**.

Flujo:

```text
Tarea
   ↓
Contexto
   ↓
Reglas
   ↓
Diseño
   ↓
Implementación
   ↓
Pruebas
   ↓
Revisión
   ↓
Siguiente tarea
```

No se debe comenzar la siguiente tarea hasta verificar que la anterior cumple sus criterios de aceptación.

---

# 3. Protocolo obligatorio para Claude Code

Antes de implementar cualquier tarea, Claude Code debe:

### Paso 1 — Leer el contexto

Leer:

```text
/CLAUDE.md
/docs/00-contexto-proyecto.md
```

### Paso 2 — Leer la documentación relacionada

Según la tarea:

```text
/docs/01-reglas-negocio.md
/docs/02-dominio.md
/docs/03-arquitectura.md
/docs/04-base-datos.md
/docs/05-api.md
/docs/06-seguridad.md
/docs/07-decisiones-arquitectura.md
```

### Paso 3 — Inspeccionar el código existente

Antes de crear o modificar archivos:

- revisar la solución;
- revisar proyectos existentes;
- revisar estructura de carpetas;
- revisar convenciones;
- revisar configuraciones;
- revisar dependencias;
- revisar pruebas existentes.

No asumir que una estructura existe simplemente porque aparece descrita en la documentación.

### Paso 4 — Presentar plan

Antes de modificar archivos debe informar:

```text
Tarea:
Objetivo:

Archivos que modificará:
Archivos que creará:

Componentes afectados:

Reglas de negocio aplicadas:

Pruebas que realizará:

Dependencias:

Posibles ambigüedades:
```

### Paso 5 — Implementar

Implementar únicamente el alcance autorizado de la tarea.

### Paso 6 — Probar

Ejecutar las pruebas correspondientes.

### Paso 7 — Reportar

Al finalizar:

```text
Tarea:
Estado:

Archivos creados:
Archivos modificados:

Cambios realizados:

Pruebas ejecutadas:

Resultado:

Problemas encontrados:

Decisiones que requieren aprobación:
```

---

# 4. Estados de las tareas

Cada tarea puede tener uno de estos estados:

```text
PENDIENTE
EN_ANALISIS
EN_DESARROLLO
EN_PRUEBAS
COMPLETADA
BLOQUEADA
CANCELADA
```

No se debe marcar una tarea como `COMPLETADA` si sus criterios de aceptación no se cumplen.

---

# 5. Fases del proyecto

El desarrollo inicial se dividirá en:

```text
FASE 0 — Base técnica
        ↓
FASE 1 — Organización
        ↓
FASE 2 — Catálogo
        ↓
FASE 3 — Periodos
        ↓
FASE 4 — Requisiciones
        ↓
FASE 5 — Revisión
        ↓
FASE 6 — Consolidación
        ↓
FASE 7 — Pedidos
        ↓
FASE 8 — Entregas
        ↓
FASE 9 — Facturación y cierre
        ↓
FASE 10 — Seguridad avanzada
        ↓
FASE 11 — Calidad, rendimiento y despliegue
```

No todas las fases deben implementarse inmediatamente.

El primer objetivo es construir un **vertical slice funcional** del proceso de requisición.

---

# 6. FASE 0 — BASE TÉCNICA

## TASK-001 — Inspeccionar y definir estructura actual

### Estado

`PENDIENTE`

### Objetivo

Conocer la estructura real del repositorio antes de modificarla.

### Alcance

Revisar:

- solución .NET;
- proyectos existentes;
- frontend;
- configuración;
- dependencias;
- pruebas;
- Git;
- archivos de configuración.

### Restricciones

No realizar refactorizaciones.

No eliminar archivos.

No instalar dependencias.

### Criterios de aceptación

Debe existir un informe claro de:

- estructura actual;
- tecnologías utilizadas;
- problemas detectados;
- diferencias respecto a la arquitectura documentada;
- posibles riesgos.

### Fuera de alcance

Implementar funcionalidades.

---

# TASK-002 — Crear o ajustar estructura base del backend

### Estado

`PENDIENTE`

### Objetivo

Establecer la estructura base necesaria para soportar la arquitectura definida.

### Alcance

Preparar los límites conceptuales:

```text
Domain
Application
Infrastructure
Api
```

La estructura física final debe adaptarse al repositorio existente cuando corresponda.

### Criterios de aceptación

- La solución compila.
- Los proyectos tienen responsabilidades claras.
- Domain no depende de infraestructura.
- No existen dependencias circulares.

### Fuera de alcance

Implementar entidades de negocio.

---

# TASK-003 — Configurar persistencia

### Estado

`PENDIENTE`

### Objetivo

Preparar Entity Framework Core y SQL Server.

### Alcance

- DbContext;
- configuración de conexión;
- configuración básica de EF Core;
- separación de configuraciones;
- entorno de desarrollo.

### Criterios de aceptación

- La aplicación puede inicializar el contexto.
- La configuración no contiene secretos hardcodeados.
- La solución compila.

---

# TASK-004 — Configurar migraciones

### Estado

`PENDIENTE`

### Objetivo

Establecer el mecanismo de evolución de la base de datos.

### Alcance

- migraciones EF Core;
- configuración de desarrollo;
- creación inicial del esquema cuando corresponda.

### Criterios de aceptación

Debe poder:

```text
Crear migración
       ↓
Aplicar migración
       ↓
Actualizar base de datos
```

---

# TASK-005 — Configurar pruebas

### Estado

`PENDIENTE`

### Objetivo

Preparar la infraestructura mínima de testing.

### Alcance

Configurar:

- proyecto de pruebas unitarias;
- proyecto de integración/API cuando corresponda;
- ejecución automatizada.

### Criterios de aceptación

Debe existir al menos una prueba ejecutable que confirme que el entorno de pruebas funciona.

---

# 7. FASE 1 — ORGANIZACIÓN

## TASK-006 — Empresa

### Estado

`PENDIENTE`

### Objetivo

Implementar la entidad Empresa.

### Reglas

Una empresa:

- puede tener múltiples sedes;
- puede tener múltiples usuarios;
- puede estar activa o inactiva;
- no debe estar hardcodeada.

### Alcance

- entidad;
- configuración EF Core;
- migración;
- caso de uso;
- API necesaria;
- pruebas.

### Criterios de aceptación

Debe ser posible:

```text
Crear empresa
Consultar empresa
Modificar empresa
Activar/desactivar empresa
```

### Fuera de alcance

Roles y autenticación completa.

---

# TASK-007 — Sede

### Estado

`PENDIENTE`

### Objetivo

Implementar sedes pertenecientes a una empresa.

### Regla

```text
Empresa 1 ─── N Sedes
```

### Datos conceptuales

- nombre;
- dirección;
- ciudad;
- información de contacto;
- estado.

### Criterios de aceptación

Una empresa puede tener múltiples sedes y cada sede pertenece a una sola empresa.

---

# TASK-008 — Usuario

### Estado

`EN_DESARROLLO`

> **Nota (cierre técnico 2026-09-11):** D-11/RN-050 (`01-reglas-negocio.md §14`, ADR-052) ya decidieron que la trazabilidad de autoría en `PedidoProveedor`/`Entrega`/`Factura` es una preocupación transversal que depende de esta tarea — no se agregan campos de usuario provisionales mientras `TASK-008` siga `PENDIENTE`. Esta tarea es el prerrequisito estructural real que bloquea esa trazabilidad; no hay una solución provisional pendiente de "activar", falta el componente en sí (`Usuario` en Domain + el mecanismo de autenticación de Fase 10).
>
> **Actualización (implementación 2026-09-14):** `Usuario` ya existe en Domain/Application/Infrastructure/Api (`Empresa 1 ─── N Usuario`, Correo único GLOBAL, `POST`/`GET /api/v1/usuarios`). Sigue `EN_DESARROLLO`, no `COMPLETADA`: no se agregó trazabilidad de autoría a `PedidoProveedor`/`Entrega`/`Factura` (sigue bloqueada por D-11 hasta que exista autenticación real, Fase 10/TASK-048) ni `UsuarioSede`/`Rol`/`Permiso` (TASK-009 a 013).

### Objetivo

Implementar el concepto de usuario del sistema.

### Alcance

- identidad;
- estado;
- relación con empresa;
- información necesaria para autorización posterior.

### Restricción

No implementar todavía toda la lógica de autenticación si esta no está definida en la tarea.

---

# TASK-009 — Usuario-Sede

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14):** `UsuarioSede` implementado en Domain/Application/Infrastructure según `04-base-datos.md §8` (`Usuario N ─── N Sede`, clave primaria compuesta `UsuarioId+SedeId`, sin `Id` propio). Valida aislamiento multiempresa (`Usuario.Empresa == Sede.Empresa`) en Domain. Sin API (ningún endpoint documentado en `05-api.md`).

### Objetivo

Permitir definir las sedes sobre las cuales un usuario tiene alcance.

### Regla

Un usuario puede estar asociado a:

- una sede;
- varias sedes;

según su responsabilidad.

### Criterios de aceptación

El sistema debe poder determinar las sedes autorizadas de un usuario.

---

# TASK-010 — Rol

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14):** `Rol` implementado en Domain/Application/Infrastructure según el contrato real de `04-base-datos.md §9.1` (`Id, Nombre, Descripcion, Activo` — **GLOBAL, sin `EmpresaId`**). Por instrucción explícita del usuario, esta tarea se amplió para incluir también `UsuarioRol` (alcance original de `TASK-012`), como relación N:N sin validación de empresa (Rol es global). Sin API (ningún endpoint documentado en `05-api.md`). Sin `Permiso`/`RolPermiso` (`TASK-011`/`TASK-013`, no tocadas).

### Objetivo

Implementar roles configurables.

### Ejemplos

```text
Solicitante
Revisor
Administrador
Compras
Recepción
```

Estos son ejemplos y no deben convertirse automáticamente en requisitos definitivos.

---

# TASK-011 — Permiso

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14):** `Permiso` implementado en Domain/Application/Infrastructure según el contrato real de `04-base-datos.md §9.2` (`Id, Codigo, Nombre, Descripcion` — **GLOBAL, sin `EmpresaId`, sin `Activo`**). Sin API (ningún endpoint documentado en `05-api.md`). Sin `RolPermiso` (`TASK-013`, no tocada) — el catálogo queda preparado para que esa tarea lo relacione con `Rol`.
>
> **Actualización — hardening previo a TASK-013 (2026-09-14):** `Permiso.Codigo` se cerró como identificador **GLOBALMENTE ÚNICO** (RN-055/ADR-056) — la implementación inicial de esta misma fecha lo había dejado sin unicidad (mismo vacío que `Rol.Nombre`), pero se determinó que TASK-013 (`RolPermiso`) necesita un código inequívoco para no generar ambigüedad al asignar permisos a roles. Reforzado con índice único en SQL Server (migración `AgregarUnicidadPermisoCodigo`) y validación en `CrearPermisoUseCase`. Se eliminaron 2 registros de datos de prueba duplicados que habían quedado de la verificación SQL manual de la implementación original de esta tarea.

### Objetivo

Implementar permisos independientes de los roles.

### Ejemplos

```text
REQUISICION_CREAR
REQUISICION_VER
REQUISICION_MODIFICAR
REQUISICION_ENVIAR
REQUISICION_APROBAR
REQUISICION_DEVOLVER
```

La lista definitiva debe estar respaldada por las necesidades del sistema.

---

# TASK-012 — Usuario-Rol

### Estado

`EN_DESARROLLO`

> **Actualización (auditoría TASK-014, 2026-09-14):** el modelo `UsuarioRol` (Domain/Application/Infrastructure, sin API) quedó adelantado dentro de `TASK-010` por instrucción explícita del usuario y no requiere una segunda implementación — auditado en `TASK-014` y confirmado funcionalmente completo: `AsignarRolAUsuarioUseCase`/`ObtenerRolesDeUsuarioUseCase`, PK compuesta `(UsuarioId, RolId)` con FK `Restrict` verificada en SQL Server, y cobertura de tests (Domain + Application) que incluye explícitamente la regla de esta tarea ("un usuario puede tener uno o varios roles" — ver `AsignarRolAUsuarioUseCaseTests.Un_usuario_puede_tener_varios_roles`). Pasa de `PENDIENTE` a `EN_DESARROLLO` (mismo criterio que `TASK-010`/`TASK-011`/`TASK-013`: no se marca `COMPLETADA` porque no tiene API ni autorización real — `progreso.md`, regla de formato #3). Ver la entrada de `TASK-010` en `progreso.md` para el detalle real de lo construido.

### Objetivo

Relacionar usuarios con roles.

### Regla

Un usuario puede tener uno o varios roles según las reglas definitivas de autorización.

---

# TASK-013 — Rol-Permiso

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14):** `RolPermiso` implementado en Domain/Application/Infrastructure según el contrato real de `04-base-datos.md §9.4` (`RolId, PermisoId` — sin `Id` propio, PK compuesta, mismo patrón que `UsuarioRol`). Sin API (ningún endpoint documentado en `05-api.md`). Cierra estructuralmente el modelo `Usuario → UsuarioRol → Rol → RolPermiso → Permiso`; la autorización real (uso efectivo de estos permisos para proteger endpoints) queda fuera de alcance, pertenece a una fase posterior.
>
> **Aclaración (auditoría TASK-014, 2026-09-14) — casos de uso de Application:** esta tarea, a diferencia de otras, no nombra casos de uso específicos en su `Regla` (solo "Rol → Permisos"). La documentación no permite determinar por sí sola si `AsignarPermisoARolUseCase`/`ObtenerPermisosDeRolUseCase` son parte del contrato — no es una inconsistencia, es una tarea que deja el detalle de Application sin especificar (mismo caso que `TASK-012`, que tampoco nombraba `AsignarRolAUsuarioUseCase`/`ObtenerRolesDeUsuarioUseCase`). Se documenta la decisión como aclaración: ambos casos de uso son las operaciones de Application estrictamente necesarias para poder gestionar `RolPermiso` en absoluto (sin una operación de asignación, la tabla `RolesPermisos` sería inalcanzable; sin una de consulta, no podría verificarse su contenido) — no se inventó ninguna regla de negocio adicional sobre ellos (mismos criterios de 404/422 ya usados en el resto del bloque). No se creó `EliminarPermisoDeRol`/`ActualizarRolPermisos`/`ReemplazarPermisosDeRol`: ninguno es estrictamente necesario y ninguno está documentado.

### Objetivo

Relacionar roles con permisos.

### Regla

```text
Rol
 ↓
Permisos
```

La autorización no debe depender de nombres de personas.

---

# 8. FASE 2 — CATÁLOGO

## TASK-014 — Categoría

### Estado

`PENDIENTE`

### Objetivo

Implementar categorías oficiales de productos.

Ejemplos actuales del proceso:

```text
Aseo
Cafetería
Papelería
Botiquín
```

La lista definitiva debe ser configurable.

---

# TASK-015 — Unidad de medida

### Estado

`PENDIENTE`

### Objetivo

Implementar unidades de medida oficiales.

Ejemplos:

```text
UNIDAD
GALÓN
CAJA
PAQUETE
```

---

# TASK-016 — Producto

### Estado

`PENDIENTE`

### Objetivo

Implementar el catálogo oficial de productos.

### Conceptos

Un producto debe poder tener:

- código interno;
- nombre/descripción oficial;
- categoría;
- unidad de medida;
- estado.

### Regla

No todos los nombres enviados por una filial deben convertirse automáticamente en productos oficiales.

---

# TASK-017 — Solicitud de producto no catalogado

### Estado

`PENDIENTE`

### Objetivo

Permitir solicitar productos que todavía no existen en el catálogo.

### Flujo

```text
Solicitante
    ↓
Producto no catalogado
    ↓
Revisión
    ↓
Homologar / Crear / Rechazar
```

### Criterios

Debe conservarse:

- descripción enviada;
- usuario solicitante;
- fecha;
- estado;
- decisión;
- observación cuando corresponda.

---

# TASK-018 — Proveedor

### Estado

`PENDIENTE`

### Objetivo

Implementar proveedores.

### Fuera de alcance

Generar pedidos.

---

# TASK-019 — Producto-Proveedor

### Estado

`PENDIENTE`

### Objetivo

Relacionar productos internos con la información específica del proveedor.

### Datos conceptuales

```text
Producto interno
Proveedor
Código proveedor
Descripción proveedor
Categoría proveedor
Unidad proveedor
```

### Beneficio

Reducir la búsqueda y homologación manual de códigos del proveedor.

---

# 9. FASE 3 — PERIODOS

## TASK-020 — Periodo mensual

### Estado

`PENDIENTE`

### Objetivo

Representar el periodo operativo mensual.

### Datos

- año;
- mes;
- estado.

### Regla

Una empresa no puede tener más de una requisición mensual para el mismo periodo.

---

# TASK-021 — Ventana de solicitud

### Estado

`PENDIENTE`

### Objetivo

Definir las fechas durante las cuales las empresas pueden enviar sus requisiciones.

### Regla

Las fechas son configurables.

No deben estar hardcodeadas.

### Ejemplo

```text
Periodo:
Septiembre 2026

Ventana:
01/09/2026 → 03/09/2026
```

---

# 10. FASE 4 — REQUISICIONES

## TASK-022 — Crear o recuperar requisición mensual

### Estado

`PENDIENTE`

### Objetivo

Permitir iniciar la requisición mensual de una empresa.

### Regla

```text
Empresa + Periodo
        ↓
máximo una requisición
```

### Comportamiento

Si ya existe una requisición en borrador:

```text
→ recuperar borrador
```

Si no existe:

```text
→ crear BORRADOR
```

### Criterios de aceptación

El sistema no debe crear duplicados.

---

# TASK-023 — Agregar producto a requisición

### Estado

`PENDIENTE`

### Objetivo

Permitir agregar productos oficiales a una requisición.

### Datos mínimos

```text
Producto
Cantidad
Observación opcional
```

### Reglas

- cantidad mayor que cero;
- producto válido;
- usuario autorizado;
- requisición editable.

---

# TASK-024 — Modificar detalle

### Estado

`PENDIENTE`

### Objetivo

Permitir modificar un producto mientras la requisición esté editable.

### Puede modificarse

- cantidad;
- observación;
- distribución cuando corresponda.

---

# TASK-025 — Eliminar detalle

### Estado

`PENDIENTE`

### Objetivo

Permitir eliminar un detalle de la requisición mientras esté en estado editable.

### Restricción

No eliminar silenciosamente información que ya forme parte de un proceso cerrado o histórico.

---

# TASK-026 — Distribuir cantidad por sede

### Estado

`PENDIENTE`

### Objetivo

Permitir indicar a qué sedes se destina cada producto.

### Regla fundamental

```text
SUMA(distribuciones)
=
cantidad solicitada
```

Ejemplo:

```text
Producto:
Papel

Cantidad:
100

Sede Bogotá:
60

Sede Medellín:
40

Total:
100
```

### Criterios de aceptación

No puede enviarse una requisición con una distribución inválida.

---

# TASK-027 — Validar requisición

### Estado

`PENDIENTE`

### Objetivo

Validar que una requisición esté completa antes de enviarla.

### Validaciones mínimas

- periodo válido;
- ventana válida;
- empresa activa;
- usuario autorizado;
- productos válidos;
- cantidades válidas;
- distribución correcta;
- sedes pertenecientes a la empresa;
- no existencia de requisición duplicada.

---

# TASK-028 — Guardar borrador

### Estado

`PENDIENTE`

### Objetivo

Permitir guardar una requisición sin enviarla.

### Regla

Guardar no equivale a enviar.

Estado:

```text
BORRADOR
```

---

# TASK-029 — Enviar requisición

### Estado

`PENDIENTE`

### Objetivo

Cambiar una requisición completa de:

```text
BORRADOR
```

a:

```text
ENVIADA
```

### Reglas

El backend debe validar nuevamente todas las condiciones.

### Consecuencia

Una requisición enviada entra formalmente al flujo de revisión.

---

# TASK-030 — Registrar historial de requisición

### Estado

`PENDIENTE`

### Objetivo

Registrar las transiciones importantes de estado.

### Ejemplo

```text
BORRADOR
↓
ENVIADA
↓
EN_REVISION
↓
DEVUELTA
↓
ENVIADA
```

Debe conservar:

- estado anterior;
- estado nuevo;
- usuario;
- fecha;
- observación cuando aplique.

---

# 11. FASE 5 — REVISIÓN

## TASK-031 — Listar requisiciones pendientes de revisión

### Estado

`PENDIENTE`

### Objetivo

Permitir a un usuario autorizado consultar requisiciones que requieren revisión.

### Seguridad

El resultado debe respetar permisos y alcance.

---

# TASK-032 — Consultar detalle de requisición

### Estado

`PENDIENTE`

### Objetivo

Mostrar toda la información necesaria para revisar una requisición.

Debe incluir:

- empresa;
- sedes;
- productos;
- cantidades;
- distribución;
- observaciones;
- usuario;
- estado;
- historial relevante.

---

# TASK-033 — Aprobar requisición

### Estado

`PENDIENTE`

### Objetivo

Permitir que un usuario autorizado apruebe una requisición.

### Reglas

Debe verificarse:

```text
Permiso
+
Alcance
+
Estado
+
Reglas de negocio
```

### Resultado

```text
EN_REVISION
     ↓
APROBADA
```

---

# TASK-034 — Devolver requisición

### Estado

`PENDIENTE`

### Objetivo

Permitir devolver una requisición para corrección.

### Regla

Debe existir una razón de devolución cuando la política del negocio lo requiera.

### Resultado

```text
EN_REVISION
     ↓
DEVUELTA
```

---

# TASK-035 — Corregir y reenviar requisición

### Estado

`PENDIENTE`

### Objetivo

Permitir que el solicitante corrija una requisición devuelta y vuelva a enviarla.

### Regla

No se debe crear una nueva requisición mensual.

Se corrige la requisición existente.

### Flujo

```text
DEVUELTA
    ↓
CORRECCIÓN
    ↓
ENVIADA
```

### Motivo

Mantener una única trazabilidad del proceso mensual.

---

# 12. FASE 6 — CONSOLIDACIÓN

Esta fase no debe comenzar hasta que el flujo de requisiciones y revisión esté estable.

## TASK-036 — Crear consolidación

### Objetivo

Crear una consolidación a partir de requisiciones aprobadas.

### Regla

La consolidación representa la necesidad total.

No modifica las requisiciones originales.

---

# TASK-037 — Generar detalles de consolidación

### Objetivo

Agrupar cantidades por producto.

Ejemplo:

```text
Empresa A → Producto X → 20
Empresa B → Producto X → 30
Empresa C → Producto X → 35

Consolidación:
Producto X → 85
```

---

# TASK-038 — Mantener trazabilidad de consolidación

### Objetivo

Permitir identificar qué requisiciones originaron cada cantidad consolidada.

### Regla

Debe ser posible recorrer:

```text
Consolidación
      ↓
Detalle consolidación
      ↓
Asignación
      ↓
Detalle requisición
      ↓
Empresa / Sede
```

---

# 13. FASE 7 — PEDIDOS A PROVEEDOR

## TASK-039 — Crear pedido proveedor

### Objetivo

Crear un pedido asociado a un proveedor.

### Regla

Una consolidación puede generar múltiples pedidos.

---

# TASK-040 — Agregar productos al pedido

### Objetivo

Registrar:

- producto;
- cantidad necesaria;
- cantidad pedida;
- precio cuando corresponda.

### Regla

La cantidad pedida puede ser diferente de la necesidad consolidada.

---

# TASK-041 — Distribuir pedido entre sedes

### Objetivo

Definir qué cantidad del pedido corresponde a cada sede.

Ejemplo:

```text
Pedido:
85 unidades

Bogotá:
40

Medellín:
20

Cali:
25
```

### Regla

La distribución debe mantener coherencia con la cantidad pedida.

---

# 14. FASE 8 — ENTREGAS

## TASK-042 — Registrar entrega

### Objetivo

Registrar una entrega asociada a un pedido.

### Regla

Un pedido puede tener múltiples entregas.

---

# TASK-043 — Registrar cantidades entregadas

### Objetivo

Registrar cuánto fue recibido de cada producto.

### Ejemplo

```text
Pedido:
100

Entrega 1:
60

Entrega 2:
40
```

---

# TASK-044 — Distribuir entrega por sede

### Objetivo

Registrar qué sede recibió cada cantidad.

### Regla

Debe conservarse la información histórica necesaria del destino.

---

# TASK-045 — Calcular cantidades pendientes

### Objetivo

Determinar cantidades todavía pendientes de entrega.

### Fórmula

```text
Pendiente =
Cantidad pedida
-
Cantidad entregada acumulada
```

---

# 15. FASE 9 — FACTURACIÓN Y CIERRE

## TASK-046 — Registrar factura

### Estado

`PENDIENTE`

### Objetivo

Registrar información de factura del proveedor.

### Nota

*(Actualizada — cierre documental 2026-09-11.)* La cardinalidad `PedidoProveedor 1 ─── N Factura` ya quedó confirmada para el alcance actual (RN-039/RN-047, ADR-042, D-06/D-05 en `01-reglas-negocio.md §15`). Lo que sigue sin implementar es el enum de `Estado` (`REGISTRADA`/`ANULADA`, RN-047) y las restricciones de unicidad de `NumeroFactura` (RN-049).

---

# TASK-047 — Validar cierre

### Estado

`PENDIENTE`

### Objetivo

Implementar el cierre de `PedidoProveedor` según la decisión ya cerrada RN-044/ADR-047 (`01-reglas-negocio.md §11`, cierre documental 2026-09-11, D-08):

```text
Cantidad pendiente de entrega = 0
(CantidadPedida - SUM(CantidadEntregada), RN-034)

+

Acción explícita de un usuario autorizado
```

> **Actualización (cierre documental 2026-09-11):** la condición "Factura registrada" que aparecía aquí como posible requisito de cierre queda **descartada** — RN-044/RN-040 confirman que Entrega y Factura son independientes y que la existencia o ausencia de `Factura` no bloquea el cierre operativo del pedido.

Falta implementar: el caso de uso de cierre en Application y la transición de `Estado` en Domain (hoy `PedidoProveedor.Estado` sigue siendo texto libre, sin el enum de RN-043).

---

# 16. FASE 10 — SEGURIDAD

## TASK-048 — Autenticación

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14, bajo la etiqueta "TASK-015" usada por el usuario para pedir este bloque — distinta de la `TASK-015 — Unidad de medida` de este catálogo, sin relación entre ambas):** Autenticación JWT propia implementada. Mecanismo elegido explícitamente por el usuario (JWT emitido por esta misma API, no proveedor externo) tras auditoría que confirmó que ninguna decisión estaba tomada (`06-seguridad.md §4/§30/§31` la diferían explícitamente) — ver ADR-057 (`07-decisiones-arquitectura.md`). `Usuario` gana `PasswordHash` (hasheado con `PasswordHasher<T>`, RN-056). `POST /api/v1/auth/login` (`05-api.md §56`) valida correo+password+`Activo`, emite JWT (claims: `sub`/`email`/`name`/`empresaId`, sin roles/permisos). Sin `[Authorize]` en ningún controller — autorización es `TASK-049`/`050`, no tocada.

### Objetivo

Implementar el mecanismo de identificación de usuarios.

### Criterios

El backend debe poder identificar de forma confiable:

- usuario;
- identidad;
- claims necesarios.

La tecnología concreta de autenticación debe definirse antes de implementar esta tarea.

---

# TASK-049 — Autorización por permisos

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14, bajo la etiqueta "TASK-016" usada por el usuario para pedir este bloque):** Autorización por permiso implementada — `IAuthorizationPolicyProvider` dinámico sobre `Permiso.Codigo` (ver ADR-058), consultando `Usuario → UsuarioRol → Rol → RolPermiso → Permiso` en cada request (fuente de verdad = BD, sin caché). Dos endpoints reales protegidos: `POST /requisiciones/{id}/enviar` (`REQUISICION_ENVIAR`) y `POST /requisiciones/{id}/aprobar` (`REQUISICION_APROBAR`) — únicos con algún respaldo documental (`06-seguridad.md §52`). No depende de nombres de rol (`[Authorize(Roles=...)]` no se usó). `TASK-050` (autorización por alcance — empresa/sede) sigue sin implementarse.

### Objetivo

Implementar autorización basada en permisos.

### Regla

No depender únicamente de nombres de roles.

---

# TASK-050 — Autorización por alcance

### Estado

`EN_DESARROLLO`

> **Actualización (implementación 2026-09-14, bajo la etiqueta "TASK-050" usada por el usuario para pedir este bloque):** Alcance por **empresa** implementado para `POST /requisiciones/{id}/enviar` y `POST /requisiciones/{id}/aprobar` (`Usuario.Empresa.Id == Requisicion.Empresa.Id`, ver RN-058/ADR-059) — mismo mecanismo de TASK-016 (`IAuthorizationPolicyProvider`/`AuthorizationHandler`), apilado como un segundo `[Authorize]`. Como efecto colateral necesario, el actor de negocio de esos dos endpoints pasó de `X-Usuario-Id` al `UsuarioId` del JWT (decisión explícita del usuario). **Alcance por sede NO implementado — decisión explícita del usuario (2026-09-14), no una ambigüedad abierta**: presentadas las alternativas (todas las sedes tocadas / alguna / posponer), se decidió mantener únicamente alcance por Empresa por ahora (ver `06-seguridad.md §13`). Alcance por "recurso"/"responsabilidad" más allá de empresa tampoco se implementó (sin contrato documentado).

### Objetivo

Limitar el acceso según:

- empresa;
- sede;
- recurso;
- responsabilidad.

---

# TASK-051 — Protección contra manipulación de IDs

### Objetivo

Evitar que un usuario acceda a información simplemente modificando un ID enviado al frontend.

Ejemplo:

```http
GET /api/v1/requisiciones/123
```

Debe verificarse que el usuario tenga autorización sobre la requisición `123`.

---

# 17. FASE 11 — CALIDAD

## TASK-052 — Pruebas de reglas de negocio

### Objetivo

Cubrir reglas críticas mediante pruebas automatizadas.

### Ejemplos

- una empresa no puede tener dos requisiciones del mismo periodo;
- distribución incorrecta;
- cantidad inválida;
- requisición fuera de ventana;
- usuario sin permiso;
- modificación de requisición no editable.

---

# TASK-053 — Pruebas de API

### Objetivo

Validar:

- endpoints;
- códigos HTTP;
- validaciones;
- autorización;
- respuestas;
- errores.

---

# TASK-054 — Manejo centralizado de errores

### Objetivo

Implementar una estrategia consistente para errores de API.

### Resultado esperado

Las excepciones no deben terminar exponiendo información interna innecesaria.

---

# TASK-055 — Logging

### Objetivo

Registrar información suficiente para diagnosticar problemas.

### Regla

No registrar secretos ni información sensible innecesaria.

---

# TASK-056 — Auditoría

### Objetivo

Registrar operaciones críticas.

Ejemplos:

- aprobación;
- devolución;
- modificación importante;
- creación de pedido;
- registro de entrega;
- cambios administrativos.

---

# 18. FASE 12 — RENDIMIENTO

Estas tareas solo deben realizarse cuando exista evidencia de necesidad.

## TASK-057 — Analizar consultas lentas

### Objetivo

Identificar consultas con problemas reales de rendimiento.

---

## TASK-058 — Índices

### Objetivo

Agregar índices basados en consultas y necesidades reales.

### Regla

No crear índices indiscriminadamente.

---

## TASK-059 — Paginación y filtrado

### Objetivo

Implementar paginación, filtros y ordenamiento donde el volumen de información lo justifique.

---

## TASK-060 — Optimización

### Objetivo

Optimizar solamente después de identificar un cuello de botella.

### Principio

```text
Medir
 ↓
Identificar
 ↓
Optimizar
 ↓
Medir nuevamente
```

---

# 19. FASE 13 — DESPLIEGUE

## TASK-061 — Configuración por ambientes

### Objetivo

Separar configuración entre:

```text
Development
Testing
Production
```

---

# TASK-062 — Variables de entorno y secretos

### Objetivo

Evitar secretos dentro del código fuente.

---

# TASK-063 — Build de producción

### Objetivo

Preparar backend y frontend para construcción de producción.

---

# TASK-064 — Despliegue

### Objetivo

Definir y ejecutar el primer despliegue real.

La infraestructura concreta debe decidirse según el entorno disponible.

---

# 20. ORDEN RECOMENDADO PARA EL PRIMER MVP

Aunque existen muchas tareas documentadas, **no se deben implementar todas inmediatamente**.

El primer MVP debe concentrarse en:

```text
TASK-001
   ↓
TASK-002
   ↓
TASK-003
   ↓
TASK-004
   ↓
TASK-005
   ↓
TASK-006
   ↓
TASK-007
   ↓
TASK-008
   ↓
TASK-009
   ↓
TASK-010
   ↓
TASK-011
   ↓
TASK-012
   ↓
TASK-013
   ↓
TASK-014
   ↓
TASK-015
   ↓
TASK-016
   ↓
TASK-020
   ↓
TASK-021
   ↓
TASK-022
   ↓
TASK-023
   ↓
TASK-024
   ↓
TASK-025
   ↓
TASK-026
   ↓
TASK-027
   ↓
TASK-028
   ↓
TASK-029
   ↓
TASK-030
   ↓
TASK-031
   ↓
TASK-032
   ↓
TASK-033
   ↓
TASK-034
   ↓
TASK-035
```

Esto produce el primer flujo completo:

```text
LOGIN
  ↓
EMPRESA
  ↓
SEDE
  ↓
PERIODO
  ↓
CREAR REQUISICIÓN
  ↓
BORRADOR
  ↓
AGREGAR PRODUCTOS
  ↓
DISTRIBUIR POR SEDE
  ↓
VALIDAR
  ↓
ENVIAR
  ↓
REVISIÓN
  ↓
APROBAR / DEVOLVER
  ↓
CORREGIR
  ↓
REENVIAR
```

---

# 21. Lo que NO debe construirse durante el primer MVP

Claude Code no debe comenzar a implementar automáticamente:

```text
Consolidación
Pedidos
Entregas
Facturación
Dashboard avanzado
Reportes complejos
Redis
RabbitMQ
Kafka
Microservicios
CQRS completo
Event sourcing
Kubernetes
Infraestructura compleja
Optimización prematura
```

Estas funcionalidades podrán implementarse posteriormente.

---

# 22. Regla de alcance

Si durante una tarea aparece una funcionalidad adicional:

```text
Problema encontrado
       ↓
¿Es necesaria para completar esta tarea?
       │
       ├── Sí → resolver dentro del alcance
       │
       └── No
            ↓
        registrar como
        nueva tarea
```

No debe aprovecharse una tarea para realizar refactorizaciones o funcionalidades no relacionadas.

---

# 23. Regla para ambigüedades

Si Claude Code encuentra una ambigüedad:

### Ambigüedad que NO cambia el diseño

Puede continuar utilizando las decisiones ya documentadas.

### Ambigüedad que SÍ puede cambiar:

- entidades;
- relaciones;
- estados;
- permisos;
- reglas de negocio;
- API;
- arquitectura;
- seguridad;

debe detener la implementación de esa parte y solicitar una decisión.

---

# 24. Criterios generales para considerar una tarea terminada

Una tarea solamente puede marcarse como `COMPLETADA` cuando:

- cumple el objetivo;
- respeta las reglas de negocio;
- respeta la arquitectura;
- no introduce dependencias no autorizadas;
- no rompe funcionalidades existentes;
- compila;
- las pruebas correspondientes pasan;
- no contiene cambios fuera de alcance;
- la documentación necesaria está actualizada.

---

# 25. Regla de Git

Cada tarea completada debe poder identificarse claramente mediante Git.

Recomendación:

```text
Una tarea
    ↓
Cambios relacionados
    ↓
Pruebas
    ↓
Commit
```

Los commits deben describir la intención del cambio.

Ejemplo:

```text
feat: agregar entidad empresa
```

o:

```text
feat: implementar creación de requisición mensual
```

Evitar commits ambiguos como:

```text
cambios
arreglos
cosas
update
final
```

---

# 26. Regla de no romper lo existente

Antes de implementar una tarea:

```text
Estado actual
    ↓
Implementación
    ↓
Pruebas
    ↓
Comparación
```

Si una modificación rompe una funcionalidad existente:

1. identificar la causa;
2. evaluar el impacto;
3. corregirla si está dentro del alcance;
4. si requiere una decisión arquitectónica, detenerse y documentarla.

---

# 27. Dependencias entre tareas

Las tareas no deben ejecutarse únicamente por número.

Debe respetarse la dependencia conceptual.

Ejemplo:

```text
Empresa
   ↓
Sede
   ↓
Usuario
   ↓
Alcance
   ↓
Requisición
```

Y:

```text
Categoria
   ↓
UnidadMedida
   ↓
Producto
   ↓
DetalleRequisicion
```

Y posteriormente:

```text
Requisicion
   ↓
Consolidacion
   ↓
PedidoProveedor
   ↓
Entrega
   ↓
Factura
```

---

# 28. Regla de vertical slices

Cuando sea posible, las funcionalidades importantes deben desarrollarse como pequeños flujos completos.

Ejemplo:

```text
Crear requisición
    ↓
Persistir
    ↓
Consultar
    ↓
Agregar detalle
    ↓
Validar
    ↓
Enviar
    ↓
Registrar historial
```

Es preferible esto a construir primero:

```text
Todas las entidades
↓
Todas las tablas
↓
Todos los controllers
↓
Todo el frontend
↓
Al final intentar conectar todo
```

---

# 29. Regla de aprendizaje

El proyecto también es un medio de aprendizaje profesional.

Cada tarea debe ayudar a comprender:

```text
Problema
 ↓
Diseño
 ↓
Código
 ↓
Prueba
 ↓
Resultado
```

Cuando una tarea sea implementada con ayuda de IA, el desarrollador debe poder explicar:

- qué se construyó;
- por qué se construyó así;
- qué regla implementa;
- qué archivos participan;
- qué datos maneja;
- qué pruebas existen;
- qué alternativas fueron descartadas.

La IA no debe sustituir la comprensión del desarrollador.

---

# 30. Checklist antes de iniciar una tarea

```text
[ ] ¿Entiendo el problema?
[ ] ¿Entiendo el objetivo?
[ ] ¿Leí las reglas relacionadas?
[ ] ¿Leí el dominio relacionado?
[ ] ¿Revisé la arquitectura?
[ ] ¿Revisé las decisiones arquitectónicas?
[ ] ¿Revisé el código existente?
[ ] ¿Sé qué archivos pueden modificarse?
[ ] ¿Sé qué archivos no deben modificarse?
[ ] ¿Conozco los criterios de aceptación?
[ ] ¿Sé cómo probar la funcionalidad?
[ ] ¿Hay alguna ambigüedad importante?
```

---

# 31. Checklist antes de aprobar una tarea

```text
[ ] Compila
[ ] Las pruebas pasan
[ ] Cumple criterios de aceptación
[ ] Respeta las reglas de negocio
[ ] Respeta la arquitectura
[ ] No hay cambios fuera de alcance
[ ] No hay dependencias innecesarias
[ ] No hay lógica hardcodeada de empresas/personas
[ ] La autorización correspondiente está contemplada
[ ] La información histórica se conserva cuando corresponde
[ ] La documentación está actualizada
[ ] El cambio puede identificarse en Git
```

---

# 32. Regla maestra de ejecución

> **Una tarea debe resolver un problema concreto, dentro de un alcance concreto, con reglas concretas y criterios de aceptación verificables.**

No se debe medir el progreso por cantidad de código escrito.

Se debe medir por funcionalidades correctamente implementadas y verificadas.

---

# 33. Principio final

```text
NO:

"Claude, construye el sistema."

SÍ:

"Claude, implementa TASK-022.
Lee las reglas y arquitectura relacionadas.
Inspecciona el código existente.
Propón los archivos que modificarás.
Espera aprobación.
Implementa únicamente el alcance.
Ejecuta las pruebas.
Reporta el resultado."
```

Este será el mecanismo principal para mantener el control técnico del proyecto.
