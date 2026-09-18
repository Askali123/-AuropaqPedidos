# 05 — API

## 1. Objetivo

Este documento define el contrato de la API REST de la aplicación de gestión de requisiciones y pedidos de Auropaq S.A.S.

La API será el punto de comunicación entre:

```text
Frontend React
      ↓
ASP.NET Core Web API
      ↓
Application
      ↓
Domain
      ↓
Infrastructure
      ↓
SQL Server
```

La API debe:

- exponer las operaciones del sistema;
- validar las solicitudes;
- aplicar las reglas de negocio mediante la capa correspondiente;
- controlar autorización;
- devolver respuestas HTTP consistentes;
- manejar errores de forma centralizada;
- evitar exponer directamente las entidades de persistencia;
- mantener contratos claros y estables.

---

# 2. Principios generales

## 2.1 REST

La API seguirá principios REST utilizando HTTP y JSON.

Métodos principales:

```text
GET     Consultar
POST    Crear / ejecutar acción
PUT     Actualizar
DELETE  Eliminar cuando el negocio lo permita
```

---

# 3. Versionamiento

La API debe estar preparada para versionamiento.

La estrategia inicial será:

```text
/api/v1/...
```

Ejemplos:

```text
GET /api/v1/empresas
GET /api/v1/productos
GET /api/v1/requisiciones
```

No se debe crear una nueva versión por cambios internos que no rompan el contrato.

Una nueva versión debe justificarse cuando exista un cambio incompatible con consumidores existentes.

---

# 4. Formato

Las solicitudes y respuestas utilizarán:

```text
Content-Type: application/json
```

Ejemplo:

```json
{
  "nombre": "Producto de ejemplo",
  "cantidad": 10
}
```

La API no debe devolver directamente entidades de Entity Framework Core.

Se utilizarán:

```text
Request DTO
Response DTO
```

para controlar el contrato externo.

---

# 5. Estructura general de respuesta

Las respuestas exitosas deben ser consistentes.

Ejemplo:

```json
{
  "data": {
    "id": 15,
    "nombre": "Producto de ejemplo"
  }
}
```

Para colecciones:

```json
{
  "data": [
    {
      "id": 1,
      "nombre": "Producto A"
    },
    {
      "id": 2,
      "nombre": "Producto B"
    }
  ]
}
```

La estructura definitiva podrá ajustarse durante la implementación si existe una razón técnica clara.

---

# 6. Errores

Los errores deben manejarse de forma centralizada.

La API no debe devolver excepciones sin controlar ni stack traces al frontend.

Formato conceptual:

```json
{
  "error": {
    "code": "REQUISICION_NO_MODIFICABLE",
    "message": "La requisición no puede modificarse en su estado actual.",
    "details": []
  }
}
```

Los códigos de error deben ser estables y orientados al problema.

---

# 7. Códigos HTTP

La API utilizará códigos HTTP apropiados.

## 7.1 200 OK

Consulta o actualización exitosa.

```text
GET
PUT
```

---

## 7.2 201 Created

Creación exitosa de un recurso.

```text
POST /api/v1/empresas
```

---

## 7.3 204 No Content

Operación exitosa sin contenido de respuesta cuando sea apropiado.

---

## 7.4 400 Bad Request

Solicitud inválida.

Ejemplos:

- JSON incorrecto;
- campos inválidos;
- datos que no cumplen validaciones de entrada.

---

## 7.5 401 Unauthorized

El usuario no está autenticado.

---

## 7.6 403 Forbidden

El usuario está autenticado pero no tiene autorización.

---

## 7.7 404 Not Found

El recurso solicitado no existe o no es accesible dentro del alcance permitido.

---

## 7.8 409 Conflict

Existe un conflicto con el estado actual del recurso.

Ejemplos:

```text
Empresa + Periodo ya tiene requisición.
Requisición ya fue enviada.
Producto ya existe.
```

---

## 7.9 422 Unprocessable Entity

Puede utilizarse cuando la solicitud tiene una estructura válida pero incumple una regla de negocio específica.

Ejemplo:

```text
La suma de las distribuciones no coincide con la cantidad solicitada.
```

La estrategia definitiva deberá mantenerse consistente en toda la API.

---

## 7.10 500 Internal Server Error

Error inesperado del servidor.

No debe revelar:

- stack trace;
- credenciales;
- información interna;
- consultas SQL;
- datos sensibles.

El detalle técnico debe quedar registrado en logs.

---

# 8. Autenticación

La API debe requerir autenticación para las operaciones protegidas.

La implementación concreta del proveedor de identidad podrá definirse posteriormente.

El backend debe poder identificar como mínimo:

```text
UsuarioId
EmpresaId
Roles
Permisos
```

No se debe confiar en:

```text
EmpresaId
UsuarioId
Rol
```

enviados libremente por el frontend cuando dichos datos puedan obtenerse de la identidad autenticada.

---

# 9. Autorización

La autorización debe basarse en:

```text
Permisos
+
Alcance
```

No solamente en el nombre del rol.

Ejemplo:

```text
Permiso:
REQUISICION_VER

Alcance:
Empresa del usuario
```

Otro usuario podría tener:

```text
Permiso:
REQUISICION_VER

Alcance:
Todas las empresas autorizadas
```

La API debe validar el acceso en backend.

El frontend solamente controla la experiencia de usuario.

---

# 10. Convenciones de URL

Las rutas utilizarán nombres de recursos en plural.

Ejemplos:

```text
/api/v1/empresas
/api/v1/sedes
/api/v1/usuarios
/api/v1/productos
/api/v1/requisiciones
```

Para recursos hijos:

```text
/api/v1/empresas/{empresaId}/sedes
```

---

# 11. Empresas

## 11.1 Listar empresas

```http
GET /api/v1/empresas
```

### Objetivo

Obtener las empresas disponibles dentro del alcance autorizado del usuario.

### Respuesta

```json
{
  "data": [
    {
      "id": 1,
      "nombre": "Empresa A",
      "activo": true
    }
  ]
}
```

---

## 11.2 Obtener empresa

```http
GET /api/v1/empresas/{id}
```

### Objetivo

Consultar una empresa específica.

---

## 11.3 Crear empresa

```http
POST /api/v1/empresas
```

Request:

```json
{
  "nombre": "Empresa A",
  "nit": "900000000-1"
}
```

---

## 11.4 Actualizar empresa

```http
PUT /api/v1/empresas/{id}
```

Request:

```json
{
  "nombre": "Empresa Actualizada",
  "nit": "900000000-1",
  "activo": true
}
```

No se debe eliminar una empresa que tenga información histórica sin una regla de negocio explícita.

---

# 12. Sedes

## 12.1 Listar sedes de una empresa

```http
GET /api/v1/empresas/{empresaId}/sedes
```

---

## 12.2 Crear sede

```http
POST /api/v1/empresas/{empresaId}/sedes
```

Request:

```json
{
  "nombre": "Sede Bogotá",
  "direccion": "Dirección de ejemplo",
  "ciudad": "Bogotá",
  "departamento": "Bogotá D.C.",
  "telefono": "0000000",
  "contacto": "Contacto",
  "activo": true
}
```

El backend debe verificar que la empresa exista y esté habilitada.

---

## 12.3 Actualizar sede

```http
PUT /api/v1/sedes/{id}
```

---

# 13. Productos

## 13.1 Listar productos

```http
GET /api/v1/productos
```

Debe permitir posteriormente filtros como:

```text
categoria
activo
codigo
nombre
```

No se deben implementar filtros sin necesidad real.

---

## 13.2 Obtener producto

```http
GET /api/v1/productos/{id}
```

---

## 13.3 Crear producto

```http
POST /api/v1/productos
```

Request:

```json
{
  "codigoInterno": "1281",
  "nombre": "Abrasivo Regular",
  "descripcion": "Descripción",
  "categoriaId": 1,
  "unidadMedidaId": 1
}
```

El backend debe validar:

- código;
- categoría;
- unidad;
- duplicados;
- datos obligatorios.

---

## 13.4 Actualizar producto

```http
PUT /api/v1/productos/{id}
```

No se debe eliminar físicamente un producto con historial.

---

# 14. Categorías

```http
GET /api/v1/categorias
GET /api/v1/categorias/{id}
POST /api/v1/categorias
PUT /api/v1/categorias/{id}
```

---

# 15. Unidades de medida

```http
GET /api/v1/unidades-medida
GET /api/v1/unidades-medida/{id}
POST /api/v1/unidades-medida
PUT /api/v1/unidades-medida/{id}
```

---

# 16. Periodos

## 16.1 Listar periodos

```http
GET /api/v1/periodos
```

---

## 16.2 Obtener periodo

```http
GET /api/v1/periodos/{id}
```

---

## 16.3 Crear periodo

```http
POST /api/v1/periodos
```

Request:

```json
{
  "anio": 2026,
  "mes": 9,
  "fechaInicio": "2026-09-01",
  "fechaFin": "2026-09-30",
  "fechaInicioSolicitud": "2026-09-01",
  "fechaFinSolicitud": "2026-09-03"
}
```

El backend debe garantizar:

```text
Año + Mes = único
```

---

# 17. Requisiciones

Este es uno de los módulos centrales del sistema.

> **Contrato verificado (cierre 2026-09-11, preparación del Frontend):** las secciones 17 a 27
> siguientes describen el diseño conceptual original de este módulo. Varias partes de ese diseño
> nunca se implementaron tal cual, y otras se implementaron distinto. **§54 "Contrato real de
> Requisiciones para el Frontend" es la referencia autoritativa y verificada contra el código y
> las pruebas de integración** — React debe construirse contra §54, no contra el diseño
> conceptual de esta sección. Las correcciones puntuales de cada sub-sección quedan igual
> marcadas en línea a continuación para no perder el rastro de qué cambió y por qué.

---

## 17.1 Listar requisiciones

```http
GET /api/v1/requisiciones
```

La consulta debe respetar el alcance del usuario.

Filtros futuros posibles:

```text
periodo
empresa
estado
fecha
```

> **NO IMPLEMENTADO.** No existe este endpoint ni un método de repositorio que liste
> requisiciones de forma genérica (`IRequisicionRepository` solo tiene `ObtenerPorId`,
> `ObtenerPorEmpresaYPeriodo` y `ObtenerAprobadasPorPeriodo`). El Frontend de Requisiciones no
> puede construir hoy una pantalla de "mis requisiciones" listando por empresa/periodo/estado —
> ver §54 para el detalle de este vacío y su impacto en la primera pantalla.

---

## 17.2 Obtener requisición

```http
GET /api/v1/requisiciones/{id}
```

La respuesta debe incluir información suficiente para mostrar:

```text
Empresa
Periodo
Estado
Fecha
Detalles
Productos
Cantidades
Distribución por sede
Historial cuando corresponda
```

> **NO IMPLEMENTADO como GET independiente.** No hay ruta `GET /api/v1/requisiciones/{id}`. En la
> práctica esto no bloquea el flujo principal porque **todas** las acciones de escritura (crear,
> agregar detalle, distribuir, enviar, iniciar revisión, aprobar, devolver) devuelven el
> `RequisicionResponse` completo y actualizado — el Frontend puede mantener el estado de la
> pantalla a partir de esas respuestas sin necesitar un GET adicional. Pero no hay forma de
> **recargar** una requisición ya existente (por ejemplo, al refrescar el navegador o al volver a
> abrir el enlace) sin repetir `POST /api/v1/requisiciones` (crear-o-recuperar, §18), que si la
> requisición ya existe simplemente la devuelve sin crear una segunda. Ver §54.

---

# 18. Crear o recuperar requisición mensual

La aplicación debe permitir que un usuario continúe una requisición que ya inició.

```http
POST /api/v1/requisiciones
```

Request conceptual:

```json
{
  "periodoId": 1
}
```

El backend obtiene la empresa desde el usuario autenticado.

No debe confiar en un `empresaId` enviado por el frontend para determinar a qué empresa pertenece la requisición.

### Comportamiento

Si no existe:

```text
Empresa + Periodo
```

se crea:

```text
BORRADOR
```

Si ya existe:

```text
BORRADOR
```

se devuelve la requisición existente.

Si ya fue:

```text
ENVIADA
EN_REVISION
APROBADA
```

el backend debe aplicar las reglas correspondientes y no crear una segunda requisición.

> **Verificado con una corrección importante:** el comportamiento de "crear o recuperar sin
> importar el estado" está implementado tal cual (`IniciarOContinuarRequisicionUseCase`, RN-007/
> ADR-011) y siempre responde `200 OK` (nunca `201 Created`, ni para el caso "se creó"). La
> corrección es sobre "el backend obtiene la empresa desde el usuario autenticado": **todavía no
> existe autenticación real** (TASK-048/TASK-049, pendientes). Mientras tanto, `usuarioId` y
> `empresaId` se leen de los headers `X-Usuario-Id` / `X-Empresa-Id` — un placeholder temporal
> explícito, no la solución definitiva; ver §54 y `docs/06-seguridad.md §5/§6`. Si falta
> cualquiera de los dos headers, responde `400` con código `SOLICITUD_INVALIDA`.

---

# 19. Agregar producto a requisición

```http
POST /api/v1/requisiciones/{id}/detalles
```

Request:

```json
{
  "productoId": 128,
  "cantidadSolicitada": 10,
  "observacion": "Necesidad mensual"
}
```

El backend debe validar:

- requisición existente;
- usuario autorizado;
- estado modificable;
- producto existente;
- producto activo;
- cantidad mayor que cero;
- periodo válido.

> **Verificado con correcciones:** responde `201 Created` (no `200`). "Usuario autorizado" y
> "periodo válido" no se validan en este endpoint específico (no hay autorización por rol/permiso
> todavía — ver §49 — y el periodo ya quedó fijado al crear la requisición, §18); las demás sí
> están implementadas exactamente así en `AgregarDetalleRequisicionUseCase`: requisición existente
> (404 si no), producto existente (404 si no), producto activo (422 si no) y cantidad > 0 (422,
> validado en `DetalleRequisicion`). "Estado modificable" es `BORRADOR` o `DEVUELTA` únicamente
> (RN-012/014/020) — cualquier otro estado responde 422. Ver §54.

---

# 20. Modificar detalle

```http
PUT /api/v1/requisiciones/{id}/detalles/{detalleId}
```

Request:

```json
{
  "cantidadSolicitada": 15,
  "observacion": "Cantidad actualizada"
}
```

Solo se permite si la requisición está en un estado modificable.

> **Verificado con una ampliación (TASK-024):** ambos campos son opcionales
> (`ActualizarDetalleRequisicionRequest(int? CantidadSolicitada, string? Observacion)`) — un
> campo en `null` significa "no modificarlo", no "borrarlo"; se puede enviar solo uno de los dos.
> Responde `200 OK`. La nueva cantidad no puede ser menor que lo ya distribuido entre sedes (422).
> Ver §54.

---

# 21. Eliminar detalle

```http
DELETE /api/v1/requisiciones/{id}/detalles/{detalleId}
```

Solo se permite cuando la requisición pueda ser modificada.

No se debe permitir borrar silenciosamente información que ya forme parte de un proceso histórico.

> **Verificado con una corrección:** responde `200 OK` con el `RequisicionResponse` completo ya
> actualizado (no `204 No Content`) — así el Frontend puede refrescar la pantalla con la misma
> respuesta que usan los demás endpoints de este módulo, sin un GET adicional. Ver §54.

---

# 22. Distribuir producto por sedes

```http
PUT /api/v1/requisiciones/{id}/detalles/{detalleId}/distribucion
```

Request:

```json
{
  "distribuciones": [
    {
      "sedeId": 1,
      "cantidad": 6
    },
    {
      "sedeId": 2,
      "cantidad": 4
    }
  ]
}
```

El backend debe validar:

```text
6 + 4 = cantidadSolicitada
```

Además:

- las sedes deben pertenecer a la empresa;
- las cantidades deben ser mayores que cero;
- no deben existir duplicados de sede;
- la requisición debe estar en estado modificable.

> **NO IMPLEMENTADO tal cual — divergencia real de diseño, ya señalada en el código
> (`RequisicionesController`) como ambigüedad abierta, no una decisión de negocio.** No existe un
> único `PUT .../distribucion` que reemplace todo el arreglo de distribuciones de un detalle.
> Application solo implementa operaciones granulares, una distribución a la vez, expuestas como
> sub-recurso:
>
> ```http
> POST   /api/v1/requisiciones/{id}/detalles/{detalleId}/distribuciones
> PUT    /api/v1/requisiciones/{id}/detalles/{detalleId}/distribuciones/{distribucionId}
> DELETE /api/v1/requisiciones/{id}/detalles/{detalleId}/distribuciones/{distribucionId}
> ```
>
> Las reglas de negocio (suma ≤ cantidad solicitada, cantidad > 0, sede debe pertenecer a la
> empresa, estado modificable) sí están implementadas — solo cambia la forma del contrato HTTP:
> granular en vez de "reemplazar todo el arreglo en un solo PUT". **Esto es una decisión de forma
> de API, no de regla de negocio** (RN-010/RN-011 se cumplen igual con cualquiera de las dos
> formas), así que se documenta aquí el contrato real en vez de detener la tarea — pero como
> nunca se cerró formalmente cuál de las dos formas es la definitiva, queda pendiente decidir si
> el Frontend consume la forma granular (recomendado: ya está implementada, probada, y es la que
> usa el flujo completo probado en `RequisicionesFlujoTests`) o si en algún momento se construye
> también el PUT-reemplazo-total documentado aquí originalmente. El detalle completo de las tres
> rutas reales está en §54.

---

# 23. Enviar requisición

Esta es una acción de negocio y no debe implementarse como:

```text
PUT /requisiciones/{id}/estado
```

Se utilizará:

```http
POST /api/v1/requisiciones/{id}/enviar
```

### Objetivo

Cambiar:

```text
BORRADOR
```

a:

```text
ENVIADA
```

### Validaciones

Antes de enviar:

```text
Periodo válido
Ventana abierta
Usuario autorizado
Requisición editable
Existe al menos un detalle
Productos válidos
Cantidades válidas
Distribuciones completas
Sedes válidas
```

### Efectos

La operación debe:

```text
Actualizar estado
+
Registrar fecha de envío
+
Crear historial
+
Registrar auditoría
```

Estas operaciones deben mantener consistencia transaccional.

> **Verificado con correcciones.** `Requisicion.Enviar()` (Domain) valida exactamente:
> requisición editable (`BORRADOR`/`DEVUELTA`, 422 si no), al menos un detalle (422), **empresa
> activa** (422 — no estaba en esta lista original) y ventana de solicitud abierta del periodo
> (`FechaInicioSolicitud`/`FechaFinSolicitud`, 422). "Productos válidos", "cantidades válidas" y
> "sedes válidas" no se re-validan aquí porque ya se garantizan al agregar el detalle/producto
> (§19) y al distribuir (§22) — lo único que se re-verifica al enviar es que **todas** las
> distribuciones queden completas (`CantidadDistribuida == CantidadSolicitada` en cada detalle).
> "Usuario autorizado": implementado en "TASK-016" (2026-09-14) como `[Authorize(Policy =
> "Permiso:REQUISICION_ENVIAR")]` (401 sin JWT/JWT inválido, 403 sin el permiso) — ver §57. No es
> autorización por Rol (§49 sigue sin implementarse); es autorización por `Permiso.Codigo`
> directamente. **"TASK-050" (2026-09-14) agrega alcance por empresa**
> (`[Authorize(Policy = "AlcanceRequisicion")]`, 403 si la Requisición no es de la Empresa del
> usuario, RN-058) **y ya NO requiere el header `X-Usuario-Id`** — el `UsuarioId` registrado en
> `Historial`/`FechaEnvio` ahora viene del claim `sub` del JWT, no del cliente. Efectos: cambia
> `Estado` a `Enviada`, registra `FechaEnvio` y agrega una entrada a `Historial` — las tres cosas
> ocurren en el mismo `SaveChanges()` (una sola entidad `Requisicion`, ya atómico por diseño; no
> requirió el mecanismo de transacción explícita que sí necesitó Entrega/PedidoProveedor). No hay
> "registrar auditoría" como mecanismo aparte — `Historial` cumple ese rol para este agregado.
> Responde `200 OK`. Reutilizado sin cambios para el reenvío desde `DEVUELTA` (§26). Ver §54/§57.

---

# 24. Revisar requisiciones

> **Paso real faltante en este diseño original: "iniciar revisión".** El código implementa una
> transición explícita `ENVIADA -> EN_REVISION` que este documento nunca contempló, y que es
> **obligatoria** antes de poder aprobar o devolver (`Aprobar()`/`Devolver()` en Domain exigen
> `EnRevision`, no `Enviada`):
>
> ```http
> POST /api/v1/requisiciones/{id}/iniciar-revision
> ```
>
> Sin body. Requiere header `X-Usuario-Id`. Responde `200 OK` con el `RequisicionResponse`
> actualizado (`Estado: "EnRevision"`). `Requisicion.IniciarRevision()` solo exige que el estado
> actual sea `Enviada` (422 si no) — **ningún RN-XXX ni TASK-XXX define qué acción o rol concreto
> debe dispararla** (ambigüedad ya señalada en el propio código, sin resolver); por ahora
> cualquier llamada con un `X-Usuario-Id` válido la ejecuta. Esto es una pregunta de
> autorización/negocio abierta, no una inconsistencia de documentación — se reporta aquí en vez
> de decidirla. Ver §54.

## 24.1 Listar requisiciones pendientes de revisión

```http
GET /api/v1/requisiciones/pendientes-revision
```

La API debe devolver únicamente las requisiciones que el usuario autenticado esté autorizado a revisar.

> **NO IMPLEMENTADO.** No existe esta ruta ni un método de repositorio equivalente (mismo vacío
> que §17.1: no hay ninguna forma de listar requisiciones todavía). La pantalla de revisión del
> Frontend no se puede construir hoy sin este endpoint. Ver §54.

---

## 24.2 Aprobar requisición

```http
POST /api/v1/requisiciones/{id}/aprobar
```

Request:

```json
{
  "observacion": "Requisición aprobada"
}
```

El backend debe validar:

- usuario autorizado;
- estado actual;
- reglas de aprobación;
- integridad de la requisición.

Resultado:

```text
EN_REVISION → APROBADA
```

> **Verificado.** Exige `Estado == EnRevision` (422 si no — `AprobarRequisicionUseCase`/
> `Requisicion.Aprobar()`, RN-017). `observacion` es opcional (`string?`) y queda en el
> `Historial`, no en un campo propio de la requisición. Responde `200 OK`. **"Usuario
> autorizado": implementado en "TASK-016" (2026-09-14)** como `[Authorize(Policy =
> "Permiso:REQUISICION_APROBAR")]` (401 sin JWT/JWT inválido, 403 sin el permiso) — ver §57.
> **"TASK-050" (2026-09-14) agrega alcance por empresa** (`[Authorize(Policy =
> "AlcanceRequisicion")]`, 403 si la Requisición no es de la Empresa del usuario) **y ya NO
> requiere el header `X-Usuario-Id`** — el `UsuarioId` registrado en `Historial` ahora viene del
> JWT (RN-058). Ver §54/§57.

---

# 25. Devolver requisición

```http
POST /api/v1/requisiciones/{id}/devolver
```

Request:

```json
{
  "motivo": "Corregir cantidad del producto X."
}
```

Resultado:

```text
EN_REVISION → DEVUELTA
```

El motivo debe quedar registrado en el historial.

> **Verificado.** Exige `Estado == EnRevision` (422 si no, RN-018) y `motivo` no vacío/en blanco
> (422 — a diferencia de `AprobarRequisicionRequest.Observacion`, aquí `Motivo` es obligatorio:
> `string`, no `string?`). El motivo queda en `Historial[].Comentario` de la nueva entrada
> (`EstadoAnterior: "EnRevision"`, `EstadoNuevo: "Devuelta"`). Responde `200 OK`. Requiere header
> `X-Usuario-Id`. Ver §54.

---

# 26. Corregir y reenviar

Cuando una requisición es devuelta, el usuario debe poder corregirla.

No se debe crear automáticamente una requisición nueva.

Flujo:

```text
EN_REVISION
     ↓
DEVUELTA
     ↓
Corrección
     ↓
ENVIADA
```

Esto mantiene la trazabilidad del proceso original.

> **Verificado.** La corrección usa los mismos endpoints de §20/§21/§22 (`DEVUELTA` es editable
> igual que `BORRADOR`, RN-012/014/020), y el reenvío reutiliza el mismo `POST .../enviar` de
> §23 — no hay una ruta separada de "reenviar". La trazabilidad se mantiene porque nunca se crea
> una segunda `Requisicion`: es la misma fila, con una nueva entrada de `Historial` por cada
> transición. Ver §54.

---

# 27. Historial de requisición

```http
GET /api/v1/requisiciones/{id}/historial
```

Respuesta conceptual:

```json
{
  "data": [
    {
      "estadoAnterior": "BORRADOR",
      "estadoNuevo": "ENVIADA",
      "usuario": "Usuario",
      "fecha": "2026-09-03T10:30:00",
      "comentario": null
    }
  ]
}
```

> **NO IMPLEMENTADO como endpoint separado — y no hace falta.** No existe `GET .../historial`.
> `RequisicionResponse.Historial` va embebido en la respuesta de **todas** las acciones (crear,
> agregar detalle, distribuir, enviar, iniciar-revisión, aprobar, devolver), así que el Frontend
> ya tiene el historial completo actualizado sin una llamada adicional. La forma real de cada
> entrada difiere de la conceptual de arriba: es `{ estadoAnterior, estadoNuevo, usuarioId,
> fecha, comentario }` — `usuarioId` (`int`), no `usuario` (nombre); y los valores de estado son
> los del enum real (`"Borrador"`, `"Enviada"`, etc., no `"BORRADOR"` en mayúsculas — ver §54).

---

# 28. Productos no catalogados

## 28.1 Solicitar producto no catalogado

```http
POST /api/v1/solicitudes-producto
```

Request:

```json
{
  "nombreSolicitado": "Producto nuevo",
  "descripcion": "Descripción del producto",
  "observacion": "No aparece en el catálogo"
}
```

---

## 28.2 Listar solicitudes pendientes

```http
GET /api/v1/solicitudes-producto/pendientes
```

---

## 28.3 Homologar producto

```http
POST /api/v1/solicitudes-producto/{id}/homologar
```

Request:

```json
{
  "productoId": 25
}
```

---

## 28.4 Crear producto

```http
POST /api/v1/solicitudes-producto/{id}/crear-producto
```

El backend debe registrar qué usuario resolvió la solicitud.

---

## 28.5 Rechazar solicitud

```http
POST /api/v1/solicitudes-producto/{id}/rechazar
```

Request:

```json
{
  "motivo": "El producto no cumple las condiciones del catálogo."
}
```

---

# 29. Proveedores

```http
GET /api/v1/proveedores
GET /api/v1/proveedores/{id}
POST /api/v1/proveedores
PUT /api/v1/proveedores/{id}
```

---

# 30. Relación producto-proveedor

```http
GET /api/v1/productos/{productoId}/proveedores
GET /api/v1/proveedores/{proveedorId}/productos
POST /api/v1/productos/{productoId}/proveedores
PUT /api/v1/productos/{productoId}/proveedores/{relacionId}
```

Esta funcionalidad permitirá automatizar la búsqueda del código utilizado por cada proveedor.

---

# 31. Consolidación

> **Actualización (implementado 2026-09-17, P2-1 de `docs/2026-09-17-tareas.md`):**
> `ConsolidacionesController` ya existe, exponiendo únicamente `CrearConsolidacionUseCase`
> (TASK-036/RN-027-029) — el único caso de uso real. `GET`/`{id}/generar` siguen sin
> implementarse: no existe caso de uso ni contrato que los respalde (mismo criterio de
> minimalismo ya usado en `FacturasController`/`PedidosProveedorController`).

> **Actualización (2026-09-17, I1-1,
> `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`):** `GET` (listar/consultar) ya
> implementado — `ListarConsolidacionesUseCase`/`ObtenerConsolidacionUseCase`. `GET
> /consolidaciones` acepta `periodoId` opcional por query string.

Endpoints implementados:

```http
GET  /api/v1/consolidaciones
GET  /api/v1/consolidaciones/{id}
POST /api/v1/consolidaciones
```

No implementado (sin caso de uso/contrato que lo respalde): `POST {id}/generar`.

La consolidación debe tomar como fuente las requisiciones aprobadas.

No debe modificar las requisiciones originales.

---

# 32. Pedidos a proveedores

> **Actualización (implementado 2026-09-11):** `PedidosProveedorController` ya existe. Ciclo de estados cerrado (RN-043/RN-044, `01-reglas-negocio.md §11`): `BORRADOR → ENVIADO → PARCIALMENTE_ENTREGADO → ENTREGADO → CERRADO`, con `CANCELADO` desde los tres primeros.

> **Actualización (2026-09-17, I1-2,
> `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`):** `GET` (listar/consultar) ya
> implementado — `ListarPedidosProveedorUseCase`/`ObtenerPedidoProveedorUseCase`. `GET
> /pedidos-proveedor` acepta `consolidacionId` opcional por query string.

> **Actualización (2026-09-18, TASK-105,
> `docs/2026-09-18-auditoria-dominio-roles-frontend.md`, hallazgo D1/F4):**
> `POST /pedidos-proveedor/{id}/detalles` ahora resuelve automáticamente, del lado del servidor,
> si existe una relación `ProductoProveedor` activa entre el producto del detalle y el proveedor
> del pedido, y la incluye como fotografía (`codigoProveedorUtilizado`, `string | null`) en cada
> `DetallePedidoProveedorResponse` — sin que el llamador tenga que enviarla. Igual que
> `CantidadNecesaria`, es un snapshot inmutable: si el código se edita después en el catálogo
> (`§30`), los pedidos ya creados no cambian retroactivamente (mismo criterio que
> `DistribucionEntrega`, RN-035/ADR-019). `null` si no existía esa relación al agregar el
> detalle.

Endpoints implementados:

```http
GET  /api/v1/pedidos-proveedor
GET  /api/v1/pedidos-proveedor/{id}
POST /api/v1/pedidos-proveedor
POST /api/v1/pedidos-proveedor/{id}/detalles
POST /api/v1/pedidos-proveedor/{id}/detalles/{detalleId}/distribuciones
POST /api/v1/pedidos-proveedor/{id}/enviar
POST /api/v1/pedidos-proveedor/{id}/cerrar
POST /api/v1/pedidos-proveedor/{id}/cancelar
```

No implementado (sin caso de uso/contrato que lo respalde; mismo criterio de minimalismo que `FacturasController` — no se inventa): `PUT` (reemplazo completo).

---

# 33. Entregas

> **Actualización (implementado 2026-09-11):** creación de Entrega por HTTP ya disponible. La cabecera se crea bajo el pedido (ruta anidada, tal como estaba documentada abajo); detalle, distribución y anulación se exponen bajo `/api/v1/entregas/{id}` en `EntregasController`.

> **Actualización (2026-09-17, I1-3,
> `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`):** `GET` ya implementado —
> `ListarEntregasUseCase` (lista las entregas de un pedido, ruta anidada — mismo criterio que la
> creación) y `ObtenerEntregaUseCase` (consulta individual).

Endpoints implementados:

```http
GET  /api/v1/pedidos-proveedor/{id}/entregas
GET  /api/v1/entregas/{id}
POST /api/v1/pedidos-proveedor/{id}/entregas
POST /api/v1/entregas/{id}/detalles
POST /api/v1/entregas/{id}/detalles/{detalleId}/distribuciones
POST /api/v1/entregas/{id}/anular
```

Una entrega debe poder representar una entrega parcial.

Ejemplo:

```text
Pedido = 100

Entrega 1 = 60
Entrega 2 = 40
```

---

# 34. Facturas

> **Actualización (implementado 2026-09-11):** cardinalidad `PedidoProveedor 1 ─── N Factura` ya confirmada para el alcance actual (RN-039/D-06) — ya no depende de validar el proceso contable real (RN-038: Auropaq Pedidos no es sistema contable).
>
> **Actualización (2026-09-17, I1-4,
> `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`):** `GET` ya implementado —
> `ListarFacturasUseCase` (exige `pedidoProveedorId` por query string, reutiliza
> `IFacturaRepository.ObtenerPorPedido`) y `ObtenerFacturaUseCase`.

Endpoints implementados:

```http
GET  /api/v1/facturas?pedidoProveedorId={id}
GET  /api/v1/facturas/{id}
POST /api/v1/facturas
POST /api/v1/facturas/{id}/detalles
POST /api/v1/facturas/{id}/anular
```

No implementado (sin caso de uso ni contrato definido): `PUT`.

---

# 35. Paginación

Las consultas que puedan devolver grandes cantidades de datos deben estar preparadas para paginación.

Formato conceptual:

```http
GET /api/v1/requisiciones?page=1&pageSize=20
```

Respuesta:

```json
{
  "data": [],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

No se debe devolver una cantidad ilimitada de registros.

---

# 36. Filtros

Las consultas podrán soportar filtros cuando exista una necesidad real.

Ejemplo:

```http
GET /api/v1/requisiciones?estado=ENVIADA&periodoId=5
```

Los filtros deben validarse y no deben permitir consultas arbitrarias sobre la base de datos.

---

# 37. Ordenamiento

Cuando sea necesario:

```http
GET /api/v1/requisiciones?sortBy=fechaCreacion&sortDirection=desc
```

Los campos permitidos deben estar controlados por backend.

No se debe aceptar directamente cualquier nombre de columna recibido desde el cliente.

---

# 38. DTOs

La API utilizará DTOs específicos.

Ejemplo:

```text
CrearRequisicionRequest
RequisicionResponse
AgregarDetalleRequisicionRequest
ActualizarDetalleRequisicionRequest
DistribuirDetalleRequest
AprobarRequisicionRequest
DevolverRequisicionRequest
```

No se deben utilizar las entidades de dominio como contratos HTTP directamente.

> **Verificado con una corrección: `DistribuirDetalleRequest` no existe** (consistente con §22 —
> nunca se implementó el PUT de reemplazo total). En su lugar existen `AgregarDistribucionRequest`
> y `ModificarDistribucionRequest` (para las rutas granulares de distribución). La forma completa
> y verificada de cada DTO real está en §54.

---

# 39. Validación

La validación debe existir en diferentes niveles.

## Frontend

Responsable de:

```text
Experiencia de usuario
Campos obligatorios
Formato
Mensajes inmediatos
```

## Backend

Responsable de:

```text
Reglas reales
Seguridad
Autorización
Integridad
Consistencia
```

El frontend nunca debe considerarse la fuente de verdad.

---

# 40. Reglas de negocio en endpoints

Los Controllers no deben contener toda la lógica de negocio.

Ejemplo incorrecto:

```text
Controller
    ├── consulta base de datos
    ├── valida estado
    ├── calcula cantidades
    ├── cambia estado
    ├── crea historial
    ├── registra auditoría
    └── guarda todo
```

El Controller debe coordinar la entrada HTTP.

La lógica debe distribuirse según la arquitectura:

```text
Controller
    ↓
Application
    ↓
Domain
    ↓
Infrastructure
```

---

# 41. Endpoints de acciones de negocio

Cuando una operación representa una acción importante del negocio, se utilizará un endpoint específico.

Ejemplos:

```text
/enviar
/aprobar
/devolver
/homologar
/rechazar
/confirmar
```

En lugar de un endpoint genérico:

```text
PUT /requisiciones/{id}/estado
```

Esto permite expresar claramente la intención de la operación.

---

# 42. Idempotencia

Las acciones críticas deben considerar qué ocurre si el cliente repite una solicitud.

Ejemplo:

```text
POST /requisiciones/15/enviar
```

Si la requisición ya fue enviada, el backend no debe crear múltiples historiales de envío ni ejecutar nuevamente efectos que deban ocurrir una sola vez.

La estrategia exacta de idempotencia se definirá según cada operación.

---

# 43. Concurrencia

El sistema debe considerar que dos usuarios pueden intentar modificar el mismo recurso.

Ejemplo:

```text
Usuario A abre requisición
Usuario B abre la misma requisición
Usuario A modifica
Usuario B modifica
```

La estrategia de concurrencia debe evitar sobrescrituras silenciosas.

Puede utilizarse posteriormente:

```text
RowVersion
```

o un mecanismo equivalente de concurrencia optimista.

La implementación concreta se definirá durante la construcción de persistencia.

---

# 44. Logs

La API debe registrar eventos técnicos importantes.

Ejemplos:

```text
Request
Error
Excepción
Operación crítica
Tiempo de ejecución
```

No deben registrarse datos sensibles innecesariamente.

Los logs técnicos no sustituyen la auditoría de negocio.

---

# 45. Manejo centralizado de excepciones

ASP.NET Core debe utilizar middleware o un mecanismo centralizado equivalente.

El objetivo es transformar excepciones en respuestas HTTP consistentes.

Ejemplo:

```text
DomainException
      ↓
Middleware
      ↓
HTTP 422
      ↓
Error estructurado
```

No se deben utilizar bloques `try/catch` repetidos en todos los Controllers para resolver el mismo problema.

---

# 46. Swagger / OpenAPI

La API debe documentarse mediante OpenAPI/Swagger durante desarrollo.

La documentación debe permitir conocer:

- endpoints;
- parámetros;
- requests;
- responses;
- códigos HTTP;
- autenticación;
- modelos.

Swagger no reemplaza este documento.

Este documento define el contrato funcional/arquitectónico.

Swagger representa la implementación actual del contrato.

---

# 47. API del MVP

La primera versión funcional debe concentrarse en:

## Organización

```text
GET  /api/v1/empresas
GET  /api/v1/empresas/{id}

GET  /api/v1/empresas/{empresaId}/sedes
POST /api/v1/empresas/{empresaId}/sedes
PUT  /api/v1/sedes/{id}
```

## Catálogo

```text
GET /api/v1/productos
GET /api/v1/productos/{id}

GET /api/v1/categorias
GET /api/v1/unidades-medida
```

## Periodos

```text
GET /api/v1/periodos
GET /api/v1/periodos/{id}
```

## Requisiciones

```text
GET    /api/v1/requisiciones
GET    /api/v1/requisiciones/{id}
POST   /api/v1/requisiciones
POST   /api/v1/requisiciones/{id}/detalles
PUT    /api/v1/requisiciones/{id}/detalles/{detalleId}
DELETE /api/v1/requisiciones/{id}/detalles/{detalleId}
PUT    /api/v1/requisiciones/{id}/detalles/{detalleId}/distribucion
POST   /api/v1/requisiciones/{id}/enviar
GET    /api/v1/requisiciones/{id}/historial
POST   /api/v1/requisiciones/{id}/aprobar
POST   /api/v1/requisiciones/{id}/devolver
```

> **Lista real implementada (verificado 2026-09-11 — ver §54 para el contrato completo de cada
> una):**
>
> ```text
> POST   /api/v1/requisiciones                                                  (crear o recuperar)
> POST   /api/v1/requisiciones/{id}/detalles
> PUT    /api/v1/requisiciones/{id}/detalles/{detalleId}
> DELETE /api/v1/requisiciones/{id}/detalles/{detalleId}
> POST   /api/v1/requisiciones/{id}/detalles/{detalleId}/distribuciones
> PUT    /api/v1/requisiciones/{id}/detalles/{detalleId}/distribuciones/{distribucionId}
> DELETE /api/v1/requisiciones/{id}/detalles/{detalleId}/distribuciones/{distribucionId}
> POST   /api/v1/requisiciones/{id}/guardar          (no documentado originalmente aquí)
> POST   /api/v1/requisiciones/{id}/enviar
> POST   /api/v1/requisiciones/{id}/iniciar-revision (no documentado originalmente aquí)
> POST   /api/v1/requisiciones/{id}/aprobar
> POST   /api/v1/requisiciones/{id}/devolver
> ```
>
> **No implementados**, y bloquean pantallas del Frontend que los necesiten: `GET
> /requisiciones` (listar), `GET /requisiciones/{id}` (obtener una), `GET
> /requisiciones/{id}/historial` (innecesario — va embebido, §27), `GET
> /requisiciones/pendientes-revision` (§24.1). Ver §54.6 para el detalle.
>
> **Cerrado (2026-09-11):** los selectores de Empresa/Sede/Producto/Periodo que le faltaban al
> Frontend ya existen — `GET /api/v1/empresas`, `GET /api/v1/empresas/{empresaId}/sedes`,
> `GET /api/v1/productos`, `GET /api/v1/periodos`. Ver §54.6.1 para el contrato completo.

No se deben implementar todavía endpoints de:

```text
Consolidacion
PedidoProveedor
Entrega
Factura
```

como parte del primer vertical slice.

---

# 48. Flujo principal de la API

El flujo esperado para el MVP es:

```text
1. Usuario inicia sesión
          ↓
2. Backend identifica usuario/empresa/alcance
          ↓
3. Consulta periodo
          ↓
4. POST /requisiciones
          ↓
5. Backend crea o recupera BORRADOR
          ↓
6. POST /requisiciones/{id}/detalles
          ↓
7. PUT /requisiciones/{id}/detalles/{detalleId}
          ↓
8. PUT /requisiciones/{id}/detalles/{detalleId}/distribucion
          ↓
9. POST /requisiciones/{id}/enviar
          ↓
10. Backend valida reglas
          ↓
11. Estado = ENVIADA
          ↓
12. Gestor consulta pendientes
          ↓
13. GET /requisiciones/pendientes-revision
          ↓
14. POST /requisiciones/{id}/aprobar
             O
    POST /requisiciones/{id}/devolver
```

> **Flujo real verificado (difiere en 3 puntos — ver §54):**
>
> ```text
> 1. Frontend envía X-Usuario-Id / X-Empresa-Id (placeholder temporal, no hay login real todavía)
>           ↓
> 2. (sin paso equivalente: no hay GET de periodos — el Frontend necesita el periodoId por otra vía)
>           ↓
> 3. POST /requisiciones { periodoId }  → 200, crea o recupera BORRADOR
>           ↓
> 4. POST /requisiciones/{id}/detalles              → 201
>           ↓
> 5. PUT  /requisiciones/{id}/detalles/{detalleId}   → 200 (opcional, si hace falta corregir)
>           ↓
> 6. POST /requisiciones/{id}/detalles/{detalleId}/distribuciones  → 201 (una llamada por sede)
>           ↓
> 7. POST /requisiciones/{id}/enviar   → 200, Estado = "Enviada"
>           ↓
> 8. POST /requisiciones/{id}/iniciar-revision   → 200, Estado = "EnRevision"  (paso real que no
>    estaba en este flujo original — obligatorio antes de aprobar/devolver)
>           ↓
> 9. (sin paso equivalente: no hay GET de pendientes-revision)
>           ↓
> 10. POST /requisiciones/{id}/aprobar → 200, Estado = "Aprobada"
>              O
>     POST /requisiciones/{id}/devolver → 200, Estado = "Devuelta" → corregir (paso 5/6) → volver
>     al paso 7 (reenviar)
> ```

---

# 49. Regla de seguridad fundamental

Nunca se debe confiar en información enviada por React para determinar:

```text
Quién es el usuario
A qué empresa pertenece
Qué puede hacer
Qué sedes puede administrar
Qué requisiciones puede modificar
Qué requisiciones puede aprobar
```

El backend debe determinarlo mediante:

```text
Identidad autenticada
+
Permisos
+
Alcance
+
Reglas de negocio
```

---

# 50. Lo que NO debe hacer Claude Code

Claude Code no debe:

- inventar endpoints;
- cambiar nombres de endpoints sin autorización;
- exponer entidades EF Core directamente;
- colocar lógica de negocio compleja en Controllers;
- confiar en validaciones del frontend;
- permitir cambiar estados arbitrariamente;
- aceptar `UsuarioId` del frontend cuando debe obtenerse de la identidad;
- aceptar `EmpresaId` del frontend para saltarse el alcance;
- crear endpoints para funcionalidades todavía fuera del MVP;
- introducir GraphQL;
- introducir gRPC;
- crear microservicios;
- crear CQRS completo;
- agregar API Gateway;
- agregar Redis;
- agregar tecnologías externas sin autorización.

---

# 51. Regla ante ambigüedad

Si Claude Code encuentra una situación que no está definida en este documento o en:

```text
01-reglas-negocio.md
02-dominio.md
03-arquitectura.md
04-base-datos.md
```

debe:

1. identificar la ambigüedad;
2. explicar por qué afecta la implementación;
3. proponer alternativas si es necesario;
4. detener esa parte de la implementación;
5. solicitar una decisión.

No debe inventar una regla de negocio.

---

# 52. Evolución futura

La API podrá evolucionar posteriormente para soportar:

```text
Consolidaciones
Pedidos a proveedores
Múltiples proveedores
Entregas parciales
Distribuciones por sede
Facturas
Notificaciones
Reportes
Dashboards
Filtros avanzados
Paginación avanzada
```

Estas funcionalidades deben incorporarse cuando exista una necesidad real y una definición clara del dominio.

---

# 53. Principio final

La API debe representar acciones y recursos del negocio.

El objetivo no es construir simplemente:

```text
CRUD de tablas
```

sino exponer:

```text
CASOS DE USO DEL NEGOCIO
```

Ejemplo:

No pensar únicamente:

```text
UPDATE Requisicion SET Estado = 'ENVIADA'
```

Sino:

```text
Enviar requisición
      ↓
Validar ventana
      ↓
Validar permisos
      ↓
Validar detalles
      ↓
Validar cantidades
      ↓
Validar sedes
      ↓
Cambiar estado
      ↓
Registrar historial
      ↓
Registrar auditoría
```

Por esta razón, los endpoints deben diseñarse alrededor de las operaciones reales del negocio y no únicamente alrededor de las tablas de SQL Server.

---

# 54. Contrato real de Requisiciones para el Frontend (cierre 2026-09-11)

Esta sección es la referencia **autoritativa y verificada** para construir la primera pantalla
del Frontend (React + TypeScript + Tailwind), según la tarea "Preparar el backend para iniciar
el desarrollo del Frontend". A diferencia de §17-27/§38/§47/§48 (diseño conceptual original),
todo lo descrito aquí fue verificado contra:

- el código real (`RequisicionesController`, casos de uso de `Application/Requisiciones`,
  `Requisicion`/`DetalleRequisicion`/`DistribucionRequisicion` en Domain);
- las pruebas de integración (`Api.Tests/RequisicionesFlujoTests.cs`, contra SQL Server real).

No se modificó ninguna regla de negocio para escribir esta sección — es documentación, no código.

## 54.1 Identidad (placeholder temporal)

**Todavía no hay autenticación real** (TASK-048/TASK-049, pendientes). Todos los endpoints que
necesitan usuario y/o empresa los leen de headers HTTP:

```text
X-Usuario-Id: <entero > 0>
X-Empresa-Id: <entero > 0>   (solo en POST /api/v1/requisiciones)
```

Si falta un header requerido o no es un entero positivo, responde `400` con
`{ "error": { "code": "SOLICITUD_INVALIDA", "message": "El header '<nombre>' es obligatorio...", "details": [] } }`.
El Frontend debe enviar estos headers en cada llamada mientras no exista login real. Cuando se
implemente JWT, este es el único punto de la Api que cambia (`IdentidadTemporal.cs`).

## 54.2 Formato de respuesta

Éxito — todo envuelto en `data`:

```json
{ "data": { /* RequisicionResponse u otro DTO */ } }
```

Error — todo envuelto en `error`:

```json
{ "error": { "code": "STRING", "message": "texto para humano", "details": ["opcional"] } }
```

| Código HTTP | `code`                     | Cuándo |
|---|---|---|
| 400 | `SOLICITUD_INVALIDA`        | Falta un header de identidad, o el body no pasa la validación de modelo de ASP.NET Core (JSON malformado, tipo incorrecto). `details` trae un mensaje por campo. |
| 404 | `RECURSO_NO_ENCONTRADO`     | La requisición, el detalle, la distribución, el producto o la sede referenciados no existen. |
| 422 | `REGLA_DE_NEGOCIO_VIOLADA`  | Cualquier regla de negocio de Domain/Application (estado no editable, distribución incompleta, producto inactivo, etc.). |
| 500 | `ERROR_INTERNO`             | No controlado. Mensaje genérico, sin detalles internos (no hay stack traces expuestos). |

`401`/`403`/`409`/`204` **no se usan hoy** en este módulo (no hay autorización por rol ni
concurrencia optimista implementadas todavía).

## 54.3 Estados reales (`RequisicionEstado`) — ¡ojo con el casing!

La documentación conceptual usaba `BORRADOR`/`EN_REVISION` (mayúsculas, guión bajo). **El valor
real que viaja en el JSON es el nombre del enum de C#, en PascalCase:**

| Valor real (`Estado` en JSON) | Significado |
|---|---|
| `"Borrador"`   | Editable. Estado inicial. |
| `"Enviada"`    | No editable. Esperando iniciar revisión. |
| `"EnRevision"` | No editable. Lista para aprobar o devolver. |
| `"Devuelta"`   | Editable de nuevo (igual que `Borrador`). |
| `"Aprobada"`   | No editable. Terminal para este módulo. |

`EsEditable` (Domain) es verdadero únicamente en `Borrador` y `Devuelta` — el Frontend debe
habilitar/deshabilitar edición de detalles y distribuciones exactamente según esa regla, no
inventar una propia.

Transiciones válidas (cualquier otra combinación responde 422):

```text
Borrador   --Enviar-->        Enviada
Enviada    --IniciarRevision--> EnRevision
EnRevision --Aprobar-->       Aprobada       (terminal)
EnRevision --Devolver-->      Devuelta
Devuelta   --Enviar-->        Enviada        (reenvío; mismo endpoint que el envío inicial)
```

## 54.4 Endpoints reales

Todas las rutas cuelgan de `api/v1/requisiciones`. Todas devuelven `RequisicionResponse`
(§54.5) salvo que se indique otra cosa.

| # | Método y ruta | Headers | Body | Éxito | Errores posibles |
|---|---|---|---|---|---|
| 1 | `POST /` | `X-Usuario-Id`, `X-Empresa-Id` | `{ periodoId: number }` | `200` — crea `Borrador` si Empresa+Periodo no tiene requisición aún, o devuelve la existente (cualquier estado) | 400 (headers), 404 si el periodo no existe |
| 2 | `POST /{id}/detalles` | — | `{ productoId: number, cantidadSolicitada: number, observacion?: string \| null }` | `201` | 404 (requisición o producto no existen), 422 (producto inactivo, cantidad ≤ 0, o requisición no editable) |
| 3 | `PUT /{id}/detalles/{detalleId}` | — | `{ cantidadSolicitada?: number \| null, observacion?: string \| null }` — cada campo es "no tocar" si es `null`/ausente | `200` | 404 (requisición o detalle), 422 (no editable, cantidad ≤ 0, o nueva cantidad menor que lo ya distribuido) |
| 4 | `DELETE /{id}/detalles/{detalleId}` | — | — | `200` (¡no 204!) | 404, 422 (no editable) |
| 5 | `POST /{id}/detalles/{detalleId}/distribuciones` | — | `{ sedeId: number, cantidad: number }` | `201` | 404 (requisición, detalle o sede), 422 (no editable, sede inactiva, sede de otra empresa, o suma > cantidad solicitada) |
| 6 | `PUT /{id}/detalles/{detalleId}/distribuciones/{distribucionId}` | — | `{ nuevaCantidad: number }` | `200` | 404, 422 (no editable, o suma > cantidad solicitada) |
| 7 | `DELETE /{id}/detalles/{detalleId}/distribuciones/{distribucionId}` | — | — | `200` | 404, 422 (no editable) |
| 8 | `POST /{id}/guardar` | — | — | `200` — no cambia nada, solo confirma persistencia del estado actual (no es "enviar") | 404 |
| 9 | `POST /{id}/enviar` | `Authorization: Bearer <JWT>` (TASK-050; ya no `X-Usuario-Id`) | — | `200` — `Estado -> "Enviada"` | 401 (JWT ausente/inválido), 403 (sin permiso o fuera de alcance), 404, 422 (no editable, sin detalles, empresa inactiva, fuera de ventana de solicitud, o distribución incompleta en algún detalle) |
| 10 | `POST /{id}/iniciar-revision` | `X-Usuario-Id` | — | `200` — `Estado -> "EnRevision"` | 400, 404, 422 (estado actual no es `Enviada`) |
| 11 | `POST /{id}/aprobar` | `Authorization: Bearer <JWT>` (TASK-050; ya no `X-Usuario-Id`) | `{ observacion?: string \| null }` | `200` — `Estado -> "Aprobada"` | 401 (JWT ausente/inválido), 403 (sin permiso o fuera de alcance), 404, 422 (estado actual no es `EnRevision`) |
| 12 | `POST /{id}/devolver` | `X-Usuario-Id` | `{ motivo: string }` (obligatorio, no vacío) | `200` — `Estado -> "Devuelta"` | 400, 404, 422 (estado actual no es `EnRevision`, o motivo vacío/en blanco) |

No existen (ver detalle de impacto en §54.6): `GET /requisiciones`, `GET /requisiciones/{id}`,
`GET /requisiciones/{id}/historial`, `GET /requisiciones/pendientes-revision`, ni el
`PUT .../distribucion` de reemplazo total (§22).

## 54.5 DTOs de respuesta reales

```ts
interface RequisicionResponse {
  id: number;
  empresaId: number;
  periodoId: number;
  usuarioCreacionId: number;
  estado: "Borrador" | "Enviada" | "EnRevision" | "Devuelta" | "Aprobada";
  fechaCreacion: string;       // ISO 8601
  fechaEnvio: string | null;   // ISO 8601, null hasta el primer envío
  detalles: DetalleRequisicionResponse[];
  historial: HistorialRequisicionResponse[];
}

interface DetalleRequisicionResponse {
  id: number;
  productoId: number;
  cantidadSolicitada: number;
  observacion: string | null;
  cantidadDistribuida: number;      // calculado: suma de distribuciones
  distribucionCompleta: boolean;    // calculado: cantidadDistribuida === cantidadSolicitada
  distribuciones: DistribucionRequisicionResponse[];
}

interface DistribucionRequisicionResponse {
  id: number;
  sedeId: number;
  cantidad: number;
}

interface HistorialRequisicionResponse {
  estadoAnterior: string;   // mismos valores de estado que arriba
  estadoNuevo: string;
  usuarioId: number;
  fecha: string;            // ISO 8601
  comentario: string | null;
}
```

Los DTOs de request son los de la tabla de §54.4 (columna "Body"); en C# son
`CrearRequisicionRequest`, `AgregarDetalleRequisicionRequest`,
`ActualizarDetalleRequisicionRequest`, `AgregarDistribucionRequest`,
`ModificarDistribucionRequest`, `AprobarRequisicionRequest`, `DevolverRequisicionRequest`
(`backend/src/Application/Requisiciones/Dtos/` y `backend/src/Api/Controllers/Dtos/`).

## 54.6 Vacíos que bloqueaban pantallas del Frontend

Ninguno de estos afecta una regla de negocio ya cerrada; eran huecos de **API de lectura**.

1. **No hay forma de listar requisiciones** (§17.1) ni de listar "pendientes de revisión"
   (§24.1). Sin esto, no se puede construir "mis requisiciones" ni la bandeja del revisor.
   **Sigue sin implementarse** — fuera del alcance de la tarea de selectores (§54.6.1).
2. **No hay `GET /requisiciones/{id}`** (§17.2). Mitigado parcialmente: `POST /requisiciones`
   (crear-o-recuperar) sirve para recargar una requisición existente si se conoce `periodoId` +
   header `X-Empresa-Id`, pero no sirve para abrir una requisición por su `id` directamente
   (por ejemplo, desde un enlace o desde la futura bandeja de revisión de otro rol/empresa).
   **Sigue sin implementarse.**
3. ~~No hay ningún endpoint de lectura para Empresa, Sede, Producto ni Periodo.~~ **Cerrado
   (2026-09-11): ver §54.6.1** — se agregaron los cuatro endpoints de selector.

Los puntos 1 y 2 siguen pendientes: si una pantalla futura necesita listar requisiciones propias
o reabrir una por `id`, hace falta autorizar esos endpoints como tarea aparte.

### 54.6.1 Endpoints de selector (Empresas/Sedes/Productos/Periodos) — cierre 2026-09-11

Solo lectura, sin CRUD (instrucción explícita: "el Frontend solo necesita consultar datos
existentes"). Ningún endpoint filtra por `Activo`/`Estado` — devuelven todos los registros con
esos campos visibles, para que el Frontend decida cómo mostrarlos, sin que el backend invente
aquí una regla de disponibilidad que no existe en ningún RN-XXX.

| Método y ruta | Devuelve | Errores |
|---|---|---|
| `GET /api/v1/empresas` | `EmpresaResponse[]` — `{ id, nombre, activo }` | — (lista vacía si no hay empresas) |
| `GET /api/v1/empresas/{empresaId}/sedes` | `SedeResponse[]` — `{ id, nombre, activo }`, solo las de esa empresa | `404 RECURSO_NO_ENCONTRADO` si `empresaId` no existe (mismo patrón que el resto de la Api: no se listan sedes de una empresa inexistente) |
| `GET /api/v1/productos` | `ProductoResponse[]` — `{ id, nombre, codigoInterno, unidadMedidaCodigo, unidadMedidaNombre, activo }` | — |
| `GET /api/v1/periodos` | `PeriodoResponse[]` — `{ id, anio, mes, fechaInicio, fechaFin, fechaInicioSolicitud, fechaFinSolicitud, estado }` | — |

Casos de uso: `ListarEmpresasUseCase`, `ListarSedesPorEmpresaUseCase`, `ListarProductosUseCase`,
`ListarPeriodosUseCase` (`Application/Organizacion`, `Application/Catalogo`, `Application/Periodos`).
La regla "producto/sede inactivo no puede usarse en una Requisición" sigue viviendo únicamente en
`AgregarDetalleRequisicionUseCase`/`AgregarDistribucionRequisicionUseCase` (§19/§22) — no se
duplicó ni se movió aquí.

## 54.7 Ambigüedad de negocio abierta (no resuelta aquí, solo reportada)

`POST /{id}/iniciar-revision` (§24, transición `Enviada -> EnRevision`) existe y funciona, pero
**ningún RN-XXX ni TASK-XXX define quién (qué rol) o qué evento debe dispararla** — hoy cualquier
`X-Usuario-Id` válido puede ejecutarla sobre cualquier requisición `Enviada`. No se resuelve en
esta tarea de documentación (es una decisión de autorización/negocio, no de contrato HTTP); el
Frontend puede construir el botón "iniciar revisión" ya mismo (el endpoint funciona), pero quién
debe *ver* ese botón queda pendiente de una decisión de permisos/roles futura.

---

# 55. Usuarios — contrato real (documentado en auditoría TASK-014, cierre 2026-09-14)

`POST`/`GET /api/v1/usuarios` existen en código desde `TASK-008` (confirmado explícitamente por
el usuario en esa tarea — ver `progreso.md`), pero nunca habían quedado documentados aquí con el
mismo nivel de detalle que `§11 Empresas`/`§12 Sedes`. Esta sección documenta el contrato tal como
existe realmente en `UsuariosController` — no agrega ni cambia ningún endpoint.

Sin headers de identidad (`X-Usuario-Id` u otro) — a diferencia de `RequisicionesController`,
ningún documento exige autenticación para estos dos endpoints en esta fase (`§8 Autenticación`
sigue sin implementación real, Fase 10 pendiente).

## 55.1 Listar usuarios

```http
GET /api/v1/usuarios
```

### Objetivo

Obtener todos los usuarios registrados (sin filtrar por `Activo`, sin paginar — mismo alcance
mínimo que `ListarRolesUseCase`/`ListarPermisosUseCase`).

### Respuesta

```json
{
  "data": [
    {
      "id": 1,
      "empresaId": 1,
      "nombre": "Martha",
      "apellido": "Gómez",
      "correo": "martha@auropaq.com",
      "activo": true,
      "fechaCreacion": "2026-09-14T00:00:00Z",
      "fechaActualizacion": "2026-09-14T00:00:00Z"
    }
  ]
}
```

---

## 55.2 Crear usuario

```http
POST /api/v1/usuarios
```

Request:

```json
{
  "empresaId": 1,
  "nombre": "Martha",
  "apellido": "Gómez",
  "correo": "martha@auropaq.com",
  "password": "mínimo 8 caracteres"
}
```

`apellido` es opcional. **`password` es obligatorio desde la implementación de autenticación
("TASK-015", 2026-09-14 — ver RN-056/ADR-057)**: mínimo 8 caracteres, se hashea antes de
persistir y nunca se devuelve en ninguna respuesta. `Correo` es único GLOBAL (no por empresa): un
segundo `POST` con el mismo `correo` responde `422` (`REGLA_DE_NEGOCIO_VIOLADA`, `§7.9`).
`empresaId` inexistente responde `404` (`RECURSO_NO_ENCONTRADO`, `§7.7`). `password` con menos de
8 caracteres responde `422` (`REGLA_DE_NEGOCIO_VIOLADA`).

### Respuesta

`201 Created`, mismo `data` que `§55.1` para el usuario creado.

---

## 55.3 Fuera de alcance de estos dos endpoints (confirmado, no pendiente)

Sin `GET /api/v1/usuarios/{id}`, sin `PUT`/`PATCH`/`DELETE`, sin activar/desactivar — ninguno
existe en código ni fue solicitado por `TASK-008`.

> **Corrección (2026-09-15):** esta sección decía también que `UsuarioSede`/`Rol`/`UsuarioRol`/
> `Permiso`/`RolPermiso` no tendrían ningún endpoint, "confirmado en la auditoría TASK-014". Esa
> nota entraba en contradicción con la propia auditoría `TASK-014` (`08-tareas.md`, notas de
> `TASK-009` a `TASK-013`), que dejaba "sin API" como el motivo explícito por el que esas tareas
> no llegaban a `COMPLETADA` — es decir, la misma auditoría señalaba la falta de API como un hueco
> a resolver, no como algo cerrado para siempre. Detectada la contradicción, se presentó al
> usuario (no se resolvió unilateralmente) y decidió explícitamente: **sí construir la API real**
> de administración de `Rol`/`Permiso`/`UsuarioRol`/`UsuarioSede`/`RolPermiso`, documentada abajo
> en `§59` a `§63`. Ver el detalle de la decisión en `progreso.md` (entrada de esta fecha) y
> `07-decisiones-arquitectura.md` (ADR-061).

---

# 56. Autenticación — contrato real (implementado 2026-09-14, "TASK-015")

Ruta acordada explícitamente con el usuario (no existía documentación previa de ningún endpoint
de login — ver auditoría en `progreso.md`, entrada de "TASK-015"): separada de `/usuarios`
(recurso de dominio) porque representa una acción de sesión, no un recurso CRUD.

## 56.1 Login

```http
POST /api/v1/auth/login
```

### Objetivo

Autenticar un `Usuario` existente (correo + password) y obtener un JWT.

Request:

```json
{
  "correo": "martha@auropaq.com",
  "password": "mínimo 8 caracteres"
}
```

### Respuesta

`200 OK`:

```json
{
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "fechaExpiracion": "2026-09-14T20:31:34Z",
    "usuario": {
      "id": 25,
      "empresaId": 1,
      "nombre": "Martha",
      "apellido": null,
      "correo": "martha@auropaq.com",
      "activo": true,
      "fechaCreacion": "2026-09-14T19:31:26Z",
      "fechaActualizacion": "2026-09-14T19:31:26Z"
    }
  }
}
```

`token`: JWT firmado (HMAC-SHA256). Claims: `sub` (UsuarioId), `email` (Correo), `name`
(Nombre + Apellido), `empresaId` — **sin claims de roles ni permisos** (autorización es
`TASK-049`/`050`, tarea posterior, no implementada). `fechaExpiracion`: configurable
(`Jwt:ExpiracionMinutos`, default 60 minutos).

### Errores

Correo inexistente, password incorrecta y `Usuario.Activo == false` responden todos **el mismo**
`401 Unauthorized` con el mismo mensaje genérico (`§7.5`):

```json
{
  "error": {
    "code": "CREDENCIALES_INVALIDAS",
    "message": "Correo o contraseña incorrectos.",
    "details": []
  }
}
```

No se distingue cuál de los tres ocurrió — evita enumeración de cuentas (`06-seguridad.md §23`).

## 56.2 Fuera de alcance (confirmado, no pendiente)

Sin refresh tokens, sin cambio de contraseña, sin recuperación de contraseña, sin logout (JWT es
stateless, sin revocación implementada) — ninguno fue solicitado en esta tarea. **Actualización
("TASK-016", 2026-09-14): dos endpoints ya están protegidos con `[Authorize]`** — ver `§57`.

## 56.3 Mis permisos (agregado 2026-09-18, TASK-101)

```http
GET /api/v1/auth/mis-permisos
```

### Objetivo

Autoconsulta: el propio usuario autenticado obtiene la lista de sus permisos reales
(`Usuario → UsuarioRol → Rol → RolPermiso → Permiso`, misma fuente que `§57`), sin necesitar
`SEGURIDAD_VER` (que roles como Solicitante no tienen). Nace de
`docs/2026-09-18-auditoria-dominio-roles-frontend.md` (Decisión A): el JWT sigue sin llevar
claims de rol/permiso (`§56.1`) — se prefirió este endpoint de solo lectura, consultado en vivo,
antes que copiar los permisos dentro del JWT, para no arriesgar permisos desactualizados si un
Administrador cambia el rol de alguien a mitad de sesión (mismo principio de ADR-058: sin caché).

### Autorización

`[Authorize]` sin política de permiso específico — solo exige estar autenticado.

### Respuesta

`200 OK`:

```json
{
  "data": [
    { "id": 3, "codigo": "REQUISICION_CREAR", "nombre": "Crear requisición", "descripcion": null }
  ]
}
```

Lista vacía (`[]`, no error) si el usuario no tiene ningún rol asignado o está inactivo — mismo
criterio de "denegación por defecto" que `UsuarioTienePermisoUseCase`.

### Errores

`401 Unauthorized` sin JWT válido (`§7.5`).

---

# 57. Autorización por permiso — contrato real (implementado 2026-09-14, "TASK-016")

Usa la identidad del JWT (`§56`) + `Usuario → UsuarioRol → Rol → RolPermiso → Permiso.Codigo`
(`04-base-datos.md §9.3/§9.4`) para decidir si la operación está permitida. Mecanismo: ver
ADR-058 (`07-decisiones-arquitectura.md`).

## 57.1 Endpoints protegidos

Únicos dos endpoints con permiso documentado (`06-seguridad.md §52`, la única correspondencia
endpoint↔permiso con algún respaldo, aunque marcada "conceptual" — confirmado explícitamente por
el usuario antes de implementar):

| Método | Endpoint                                | Permiso requerido    |
| ------ | ---------------------------------------- | --------------------- |
| POST   | `/api/v1/requisiciones/{id}/enviar`      | `REQUISICION_ENVIAR`  |
| POST   | `/api/v1/requisiciones/{id}/aprobar`     | `REQUISICION_APROBAR` |

Ningún otro endpoint del sistema está protegido por permiso todavía — no existe contrato
documentado para el resto (TASK-016 §12/§14: no proteger indiscriminadamente, no inventar
permisos "por simetría").

## 57.2 Comportamiento HTTP

| Situación                              | Resultado |
| --------------------------------------- | --------- |
| Sin header `Authorization`              | `401 Unauthorized` |
| JWT inválido/expirado/mal firmado       | `401 Unauthorized` |
| JWT válido, sin el permiso requerido    | `403 Forbidden` |
| JWT válido, `Usuario.Activo == false`   | `403 Forbidden` (mismo criterio que "sin permiso" — RN-057) |
| JWT válido, con permiso, fuera de alcance (otra Empresa) | `403 Forbidden` — ver `§58` (TASK-050) |
| JWT válido, con el permiso requerido, dentro de alcance | Se ejecuta el endpoint normalmente |

`401`/`403` los genera el middleware de autorización de ASP.NET Core directamente (no
`ExcepcionesDeNegocioHandler`) — no llevan el envoltorio `{"error": {...}}` del resto de la Api
(`§6`); son las respuestas mínimas estándar de `Microsoft.AspNetCore.Authorization`.

## 57.3 Identidad unificada con el JWT (actualizado, "TASK-050", 2026-09-14)

**Resuelto.** La observación original de esta sección ("`Enviar`/`Aprobar` seguían usando
`X-Usuario-Id` para el actor de negocio, no el `UsuarioId` del JWT") quedó cerrada: ambos
endpoints dejaron de leer `X-Usuario-Id` por completo — el `UsuarioId` registrado en `Historial`
ahora proviene del claim `sub` del JWT ya validado por `[Authorize]`. Ver `§58`/RN-058/ADR-059
para el detalle completo. El resto de las rutas de `Requisicion` (crear, detalles,
distribuciones, iniciar-revisión, devolver) **no** se tocó — siguen usando `X-Usuario-Id` sin
cambios, fuera del alcance de esta tarea.

## 57.4 Fuera de alcance (confirmado, no pendiente)

`[Authorize(Roles = "...")]` NO se usó (TASK-016 §28: el modelo es `Rol → Permiso`, no
autorización directa por nombre de rol). Sin policies estáticas por permiso (`IAuthorizationPolicyProvider`
dinámico, ver ADR-058). Sin nuevos permisos inventados — solo `REQUISICION_ENVIAR`/
`REQUISICION_APROBAR`, ya documentados desde `06-seguridad.md §9`. Sin `UsuarioPermiso` ni
ninguna segunda fuente de permisos. Sin cache de permisos (Redis/MemoryCache). Sin frontend.

---

# 58. Autorización por alcance — contrato real (implementado 2026-09-14, "TASK-050")

Se apila sobre `§57` para los mismos dos endpoints. Fuente: `Usuario.Empresa.Id ==
Requisicion.Empresa.Id` (`06-seguridad.md §11`, RN-058). Mecanismo: ver ADR-059.

## 58.1 Endpoints con alcance

| Método | Endpoint                                | Permiso requerido    | Alcance |
| ------ | ---------------------------------------- | --------------------- | ------- |
| POST   | `/api/v1/requisiciones/{id}/enviar`      | `REQUISICION_ENVIAR`  | Misma Empresa |
| POST   | `/api/v1/requisiciones/{id}/aprobar`     | `REQUISICION_APROBAR` | Misma Empresa |

**Alcance por sede NO implementado — decisión explícita del usuario** (`06-seguridad.md §13`,
RN-058): `Requisicion` no tiene un único `SedeId` — puede distribuirse entre varias sedes —, lo
que planteaba una ambigüedad real; presentadas las alternativas, se decidió mantener únicamente
alcance por Empresa por ahora.

## 58.2 Comportamiento HTTP

| Situación                                          | Resultado |
| ---------------------------------------------------- | --------- |
| JWT válido + permiso, Requisición de otra Empresa    | `403 Forbidden` |
| JWT válido + permiso, Requisición inexistente        | `404 Not Found` (el alcance no bloquea — deja que el caso de uso reporte el 404 real) |
| JWT válido + permiso, Requisición de la misma Empresa | Se ejecuta el endpoint normalmente |

## 58.3 Identidad: actor de negocio ahora viene del JWT

Para estos dos endpoints, `RequisicionesController` ya **no** lee `X-Usuario-Id`. El `UsuarioId`
que queda en `Historial`/`FechaEnvio` se obtiene del claim `sub` del JWT (el mismo que
`PermisoAuthorizationHandler`/`AlcanceRequisicionAuthorizationHandler` ya validaron para llegar
hasta el Controller) — decisión confirmada explícitamente por el usuario, respaldada por un
comentario que ya existía en `EnviarRequisicionUseCase`/`AprobarRequisicionUseCase` desde
`TASK-029`/`033`, antes de que existiera autenticación real: *"usuarioId debe provenir de la
identidad autenticada, no del cliente"*. El resto de las rutas de `Requisicion` sigue usando
`X-Usuario-Id` sin cambios (ver `§57.3`).

## 58.4 Fuera de alcance (confirmado, no pendiente)

Alcance por sede (ambigüedad reportada, `§58.1`). `UsuarioEmpresa`/`UsuarioAlcance`/tablas de
alcance nuevas — se reutiliza `Usuario.Empresa` ya existente. Cache de alcance. Endpoints
distintos a `enviar`/`aprobar` (sin contrato documentado para el resto). Frontend.

---

# 59. Roles (TASK-010, decisión 2026-09-15 — ver `§55.3`)

Application/Infrastructure ya existían (`CrearRolUseCase`/`ListarRolesUseCase`); esta sección
documenta el contrato HTTP que antes no existía.

## 59.1 Listar roles

```http
GET /api/v1/roles
```

Devuelve todos los roles, sin filtrar por `Activo` (ninguna regla documentada exige ese filtro —
mismo criterio que `§11.1` para Empresas).

```json
{
  "data": [
    { "id": 1, "nombre": "Solicitante", "descripcion": "Crea y envía requisiciones", "activo": true }
  ]
}
```

## 59.2 Crear rol

```http
POST /api/v1/roles
```

Request:

```json
{
  "nombre": "Solicitante",
  "descripcion": "Crea y envía requisiciones"
}
```

`descripcion` es opcional. Rol es **GLOBAL** (sin `EmpresaId`, `04-base-datos.md §9.1`). Sin
validación de unicidad de `Nombre` (no documentada). Responde `201 Created`.

---

# 60. Permisos (TASK-011, decisión 2026-09-15 — ver `§55.3`)

## 60.1 Listar permisos

```http
GET /api/v1/permisos
```

```json
{
  "data": [
    { "id": 1, "codigo": "REQUISICION_CREAR", "nombre": "Crear requisición", "descripcion": null }
  ]
}
```

## 60.2 Crear permiso

```http
POST /api/v1/permisos
```

Request:

```json
{
  "codigo": "REQUISICION_CREAR",
  "nombre": "Crear requisición",
  "descripcion": "Permite crear una requisición en BORRADOR"
}
```

`descripcion` es opcional. `codigo` es único **GLOBAL** (RN-055/ADR-056): un segundo `POST` con el
mismo `codigo` responde `422` (`REGLA_DE_NEGOCIO_VIOLADA`). Responde `201 Created`.

---

# 61. Usuario-Rol (TASK-010/012, decisión 2026-09-15 — ver `§55.3`)

## 61.1 Asignar rol a un usuario

```http
POST /api/v1/usuarios/{usuarioId}/roles
```

Request:

```json
{ "rolId": 1 }
```

| Situación | Resultado |
| --- | --- |
| Usuario o Rol inexistente | `404 Not Found` |
| El usuario ya tiene ese rol asignado | `422 Unprocessable Entity` |
| Asignación válida | `201 Created`, `{ "data": { "usuarioId": ..., "rolId": ... } }` |

## 61.2 Consultar los roles de un usuario

```http
GET /api/v1/usuarios/{usuarioId}/roles
```

Satisface el criterio de aceptación de `TASK-012` ("un usuario puede tener uno o varios roles").
`404` si el usuario no existe. Respuesta: lista de `RolResponse` (misma forma que `§59.1`).

---

# 62. Usuario-Sede (TASK-009, decisión 2026-09-15 — ver `§55.3`)

## 62.1 Asignar una sede a un usuario

```http
POST /api/v1/usuarios/{usuarioId}/sedes
```

Request:

```json
{ "sedeId": 1 }
```

| Situación | Resultado |
| --- | --- |
| Usuario o Sede inexistente | `404 Not Found` |
| El usuario ya tiene acceso a esa sede | `422 Unprocessable Entity` |
| Usuario y Sede de empresas distintas (RN-002, aislamiento multiempresa) | `422 Unprocessable Entity` |
| Asignación válida | `201 Created`, `{ "data": { "usuarioId": ..., "sedeId": ... } }` |

## 62.2 Consultar las sedes autorizadas de un usuario

```http
GET /api/v1/usuarios/{usuarioId}/sedes
```

Satisface el criterio de aceptación de `TASK-009` ("el sistema debe poder determinar las sedes
autorizadas de un usuario"). `404` si el usuario no existe. Respuesta: lista de `SedeResponse`
(misma forma que `§12.1`).

---

# 63. Rol-Permiso (TASK-013, decisión 2026-09-15 — ver `§55.3`)

## 63.1 Asignar un permiso a un rol

```http
POST /api/v1/roles/{rolId}/permisos
```

Request:

```json
{ "permisoId": 1 }
```

| Situación | Resultado |
| --- | --- |
| Rol o Permiso inexistente | `404 Not Found` |
| El rol ya tiene ese permiso asignado | `422 Unprocessable Entity` |
| Asignación válida | `201 Created`, `{ "data": { "rolId": ..., "permisoId": ... } }` |

## 63.2 Consultar los permisos de un rol

```http
GET /api/v1/roles/{rolId}/permisos
```

`404` si el rol no existe. Respuesta: lista de `PermisoResponse` (misma forma que `§60.1`).

## 63.3 Fuera de alcance de estos 5 bloques (§59-63), confirmado — no pendiente

Sin `PUT`/`DELETE`/activar-desactivar para `Rol`/`Permiso` (no solicitado). Sin
`EliminarPermisoDeRol`/`EliminarRolDeUsuario`/`EliminarSedeDeUsuario` (quitar una asignación no
está documentado como necesidad — mismo criterio ya aplicado en la auditoría `TASK-014` para
`RolPermiso`, `08-tareas.md` `TASK-013`). Sin `[Authorize]` en ninguno de estos endpoints todavía:
la autorización real sobre ellos es un paso posterior que cubre todos los endpoints nuevos del
MVP a la vez (ver `progreso.md`, `## Próxima tarea`), no una tarea de este bloque.
