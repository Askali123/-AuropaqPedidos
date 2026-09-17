# PROGRESO — AuropaqPedidos

Tablero de seguimiento de las tareas definidas en `docs/08-tareas.md`.

> Este archivo es un tablero de trabajo, no documentación oficial de negocio/arquitectura.
> La fuente de verdad de cada tarea sigue siendo `docs/08-tareas.md`. Ante cualquier
> diferencia entre este archivo y `docs/08-tareas.md`, gana `docs/08-tareas.md`.

> **Archivo (2026-09-17, idea de método aprobada en `docs/2026-09-17-tareas.md §4`):** el
> historial completo hasta el 2026-09-16 (720+ líneas cronológicas, la mayor parte del peso
> original de este archivo) se movió, sin modificar, a `docs/progreso-archive/2026-09.md`. Este
> archivo activo conserva la cabecera, las tablas de FASE (siguen siendo la referencia rápida de
> estado por tarea) y, de aquí en adelante, solo las entradas nuevas del historial — no se
> reescribe lo ya archivado. Regla hacia adelante: cuando este archivo vuelva a crecer mucho
> (ej. al cerrar un mes o una fase grande), repetir el mismo proceso con un archivo nuevo
> (`docs/progreso-archive/AAAA-MM.md`).

**Última actualización:** 2026-09-17 (Incremento "Fase 6-9 en Frontend" **cerrado por completo**:
Prioridades 1 a 4 — 5 `GET` nuevos, distribución completa exigida antes de enviar un Pedido
(RN-065), las 4 pantallas de Frontend verificadas end-to-end en navegador, y guardia de
regresión 401/403 para todos los `GET` nuevos — 657/657 pruebas correctas — ver
`## Historial de decisiones registradas aquí`)

---

## Informe de inspección — TASK-001 (retroactivo, 2026-09-15 11:14)

> **Nota de proceso:** `TASK-001 — Inspeccionar y definir estructura actual` (`08-tareas.md` §6) quedó
> `PENDIENTE` desde el inicio del proyecto porque se empezó a construir (TASK-002 en adelante) antes
> de escribir el informe formal que exige — decisión explícita tomada con el usuario el 2026-09-08
> (ver primera entrada del historial). Este informe se escribe ahora, en retrospectiva, describiendo
> la estructura **real** del repositorio tal como existe hoy, para cumplir los criterios de aceptación
> de la tarea sin descartar ni repetir el trabajo ya hecho. No inventa una inspección que no ocurrió:
> documenta el estado actual verificado directamente sobre el código (`dotnet build`, `dotnet test`,
> conteo de archivos/carpetas), no lo que los documentos de planeación asumían al momento de escribirse.

### 1. Estructura actual del repositorio

```text
AuropaqPedidos/
├── CLAUDE.md
├── README.md
├── progreso.md
├── docs/                    (00 a 08, documentación oficial de negocio/arquitectura)
├── backend/
│   ├── AuropaqPedidos.slnx
│   ├── Arquitectura.md      (bosquejo de estructura objetivo, no siempre 1:1 con la real)
│   ├── src/
│   │   ├── Domain/          (Entities/, Enums/, Exceptions/ — sin dependencias externas)
│   │   ├── Application/     (organizado por módulo de negocio: Organizacion, Catalogo,
│   │   │                     Periodos, Requisiciones, Consolidaciones, PedidosProveedor,
│   │   │                     Entregas, Facturas, Excepciones, + DependencyInjection.cs)
│   │   ├── Infrastructure/  (Persistence/ [DbContext, Configurations, Migrations],
│   │   │                     Repositories/, Security/, + DependencyInjection.cs)
│   │   └── Api/             (Controllers/, Dtos/, Authorization/, Program.cs)
│   └── tests/
│       ├── Domain.Tests/       (182 pruebas)
│       ├── Application.Tests/  (178 pruebas)
│       └── Api.Tests/          (77 pruebas, integración HTTP real contra SQL Server)
└── frontend/
    ├── src/pages/           (RequisicionesPage, FacturacionPage)
    ├── src/components/requisiciones/  (formularios de detalle/distribución/período)
    └── src/services/, types/, hooks/, config/
```

294 archivos `.cs` en `backend/src`. 9 controllers reales: `Auth`, `Empresas`, `Entregas`, `Facturas`,
`PedidosProveedor`, `Periodos`, `Productos`, `Requisiciones`, `Usuarios`. 17 migraciones EF Core aplicadas
(desde `InitialCreate` hasta `AgregarPasswordHashAUsuario`). 26 entidades de dominio en `Domain/Entities`.

### 2. Tecnologías utilizadas (verificadas en los `.csproj`, no asumidas)

| Capa | Tecnología |
|---|---|
| Backend | C# / .NET 10 (`net10.0`), ASP.NET Core Web API |
| Persistencia | Entity Framework Core 10.0.12 + SQL Server, migraciones EF Core |
| Autenticación | JWT propio (`Microsoft.AspNetCore.Authentication.JwtBearer` 10.0.12, `System.IdentityModel.Tokens.Jwt` 8.22.0), hashing con `Microsoft.Extensions.Identity.Core` (`PasswordHasher<T>`) |
| Autorización | `IAuthorizationPolicyProvider`/`AuthorizationHandler` a medida (sin librería adicional) |
| Pruebas | xUnit (`Domain.Tests`, `Application.Tests`, `Api.Tests` con `WebApplicationFactory`) |
| Frontend | React 19, TypeScript, Vite 8, Tailwind CSS 4 |

Ninguna tecnología fuera de lo autorizado en `07-decisiones-arquitectura.md` (sin Redis/RabbitMQ/Kafka/
microservicios/CQRS/Docker/Kubernetes, consistente con ADR-023 a ADR-026).

### 3. Verificación real ejecutada en esta inspección

- `dotnet build AuropaqPedidos.slnx` → **compilación correcta, 0 advertencias, 0 errores** (los 7 proyectos: 4 de `src`, 3 de `tests`).
- `dotnet test tests/Domain.Tests` → **182/182 correctas**.
- `dotnet test tests/Application.Tests` → **178/178 correctas**.
- `Api.Tests` no se ejecutó en esta inspección (requiere SQL Server real levantado); se verificó por conteo estático que existen 77 pruebas (`[Fact]`/`[Theory]`), consistente con el último número reportado en el historial (437 = 182+178+77, entrada 2026-09-14 16:41).

### 4. Problemas / inconsistencias detectados

1. **Las tablas de fases de este archivo (`## FASE 0` en adelante) estaban desincronizadas del historial real.** El historial (más abajo, cronológico) ya registraba 437 pruebas totales desde el 2026-09-14, pero la tabla de `FASE 0` seguía diciendo "Domain.Tests (80 pruebas) y Application.Tests (29 pruebas)" y "falta Api.Tests" — cuando `Api.Tests` ya existía con 77 pruebas. Se corrige en la sección 6 de este informe.
2. **`TASK-001` quedó formalmente `PENDIENTE`** pese a que el proyecto avanzó, en la práctica, hasta la Fase 10 (Seguridad). Es la incongruencia motivo de esta sesión — se resuelve con este informe (cierra TASK-001) y con la política de cierre de fase (ADR-060, ver más abajo).
3. **`backend/Arquitectura.md` no coincide exactamente con la estructura real de `Application`** (documenta módulos como `Requisiciones/Empresas/Sedes/Usuarios/Productos/Periodos`; la estructura real usa `Organizacion/Catalogo/Periodos/Requisiciones/Consolidaciones/PedidosProveedor/Entregas/Facturas`). No es una violación de arquitectura — ambas son organizaciones modulares válidas por dominio — pero `Arquitectura.md` quedó desactualizado como boceto inicial. No se corrige en este informe (fuera del alcance autorizado de esta sesión); se deja registrado para una sesión futura si se decide mantenerlo sincronizado.
4. **Ninguna tarea del rango TASK-002 a TASK-050 cumple hoy los 8 criterios de `08-tareas.md` §24 para `COMPLETADA`** (la mayoría carece de API expuesta, autorización real, o tiene decisiones de negocio pendientes) — ver detalle en las tablas de fase existentes, que siguen siendo la fuente de verdad de este archivo para ese punto.

### 5. Diferencias respecto a la arquitectura documentada

Ninguna diferencia estructural relevante: las 4 capas (`Domain → Application → Infrastructure → Api`) existen, con las dependencias en el sentido correcto (`Domain` no referencia `Infrastructure`/`Api`/EF Core, verificado por inspección de `.csproj`), consistente con ADR-006. La única diferencia real es la nomenclatura interna de subcarpetas de `Application` vs. `backend/Arquitectura.md` (punto 3 arriba), que es cosmética.

### 6. Riesgos identificados

- **Riesgo de deuda de documentación silenciosa:** ya se detectó una tabla de progreso desincronizada (punto 4.1). Si este patrón se repite, `progreso.md` deja de ser confiable como tablero. Mitigación: la política de cierre de fase (ADR-060) exige actualizar la tabla de la fase en el mismo commit que cierra cada tarea, no solo el historial.
- **Riesgo de alcance disperso:** el trabajo ya avanzó en paralelo sobre Fases 1-10 sin que ninguna esté formalmente cerrada, lo que dificulta saber "qué falta realmente" para tener un flujo end-to-end usable. Mitigación: orden de cierre propuesto en la recomendación de esta sesión (fuera de este informe).
- **Riesgo de regresión no detectada en `Api.Tests`:** al depender de SQL Server real, no se ejecuta automáticamente en cada verificación rápida (como esta inspección) si no hay una instancia disponible — se verificó solo por conteo estático, no por ejecución, en este informe.

### Criterios de aceptación de TASK-001 (`08-tareas.md` §6) — verificación

- [x] Estructura actual documentada (sección 1).
- [x] Tecnologías utilizadas documentadas (sección 2), verificadas en `.csproj`/`package.json`, no asumidas.
- [x] Problemas detectados documentados (sección 4).
- [x] Diferencias respecto a la arquitectura documentada (sección 5).
- [x] Riesgos identificados (sección 6).

**Estado de TASK-001: `COMPLETADA`** (ver corrección de tabla en `## FASE 0` más abajo).

---

## Estados posibles

```text
PENDIENTE
EN_ANALISIS
EN_DESARROLLO
EN_PRUEBAS
COMPLETADA
BLOQUEADA
CANCELADA
```

---

## Formato estándar de una entrada de historial

Toda entrada nueva en **"Historial de decisiones registradas aquí"** debe agregarse como un ítem de lista (no como encabezado `##`) y seguir este orden. Si un campo no aplica en esa entrada, se omite — no se deja vacío.

```text
- **AAAA-MM-DD HH:MM (identificador del bloque/tarea)** — una frase: qué
  bloque se trabajó y quién lo autorizó (instrucción explícita del usuario /
  continuación de sesión anterior). Alcance: qué capas/carpetas toca (Domain,
  Application, Infrastructure, Api, docs/) y cuáles NO.
  - **Qué se hizo:** bullets por capa o por tema; citar RN-XXX / ADR-XXX /
    sección de docs que respalda cada decisión.
  - **Bloqueador encontrado y resuelto** (solo si aplica): problema técnico
    real, alternativas presentadas al usuario, decisión elegida con
    autorización explícita.
  - **Archivos creados:** lista de rutas.
  - **Archivos modificados:** lista de rutas; indicar explícitamente qué capas
    NO se tocaron.
  - **Archivos eliminados** (solo si aplica): lista de rutas + motivo.
  - **Compilación:** `dotnet build AuropaqPedidos.slnx` → resultado exacto
    (correcta/errores, advertencias).
  - **Migración** (solo si aplica): nombre, si se creó y si se aplicó a SQL
    Server local.
  - **Pruebas:** comando ejecutado → `X/X correctas` (desglose por proyecto de
    test, cuántas son nuevas).
  - **Ambigüedades / contradicciones encontradas en docs** (solo si aplica):
    numeradas, citando documento + sección, y qué se hizo mientras tanto
    (seguir prioridad de CLAUDE.md §4, o detenerse sin implementar esa parte).
  - **Decisiones de diseño reversibles** (solo si aplica): decisiones menores
    tomadas por convención, no bloqueantes, con su justificación.
  - **Decisiones pendientes de negocio** (solo si aplica): lista de
    "Pendiente N" (mismo numerado que `01-reglas-negocio.md §14` cuando
    corresponda) que siguen sin resolver.
  - **Continúa / siguiente paso:** qué queda explícitamente sin iniciar y por
    qué (instrucción del usuario, prerrequisito faltante, decisión pendiente).
```

### Reglas del formato

1. Fecha y hora siempre en `AAAA-MM-DD HH:MM` (hora local del entorno donde se
   ejecuta la sesión, formato de 24 horas — obtenerla del reloj del sistema,
   nunca inventarla). Agregar una etiqueta entre paréntesis solo cuando hace
   falta distinguir el bloque (ej. `2026-09-10 14:58 (bloque Consolidación)`).
   La hora reemplaza a la necesidad de un identificador manual para
   diferenciar varias entradas del mismo día.
2. Cada ítem de nivel superior corresponde a **una sesión/bloque de trabajo**,
   no a una TASK individual de `08-tareas.md` — si una sesión toca varias TASK
   relacionadas, se listan todas en el título (ej. `TASK-042/043/044/045`).
3. No marcar nada como `COMPLETADA` en las tablas de fases si la entrada
   reporta código sin API, sin autorización real, o con decisiones de negocio
   pendientes que limiten el alcance — usar `EN_DESARROLLO`
   (CLAUDE.md §53 / `08-tareas.md` §24).
4. Toda ambigüedad "importante" (CLAUDE.md §51 / `08-tareas.md` §23) debe
   quedar registrada aquí aunque no haya bloqueado el trabajo — nunca omitirla
   porque "no impidió avanzar".
5. Al agregar una entrada, actualizar también la línea
   `**Última actualización:**` de la cabecera de este archivo con la misma
   fecha, hora e identificador.

---

## Próxima tarea — LEER ESTO PRIMERO en una sesión nueva

> Esta sección se reescribe cada vez que cambia el estado general (no se acumula historial
> aquí — el historial completo, cronológico, está más abajo en `## Historial de decisiones
> registradas aquí`). Si algo de aquí contradice una tabla de FASE más abajo, la tabla de FASE
> es la más reciente correcta — avisar para corregir esta sección.

### Estado general (2026-09-17, última sesión)

**Sesión de análisis y tareas de proceso, sin cambios de negocio en el flujo MVP.** A petición
explícita del usuario: análisis completo código vs. `docs/00-08` vs. este archivo (detalle en
`docs/2026-09-17-tareas.md`); dos ambigüedades de negocio (Pendiente 1 y 2, `01-reglas-negocio.md
§15`) resueltas y aprobadas — `RN-061`/`ADR-064`/`D-14` (bloqueo de envío fuera de ventana) y
`RN-062`/`ADR-065`/`D-15` (requisición `APROBADA` inmutable), **decisión cerrada, implementación
en código todavía pendiente** (P3-1/P3-2). Cuatro mejoras de proceso aprobadas e implementadas:
este archivo se archivó (ver nota de cabecera), `08-tareas.md` se sincronizó con notas de estado
inline para Fases 6-9 (TASK-036 a TASK-047, antes sin `### Estado`), se agregó
`docs/decisiones-pendientes.md` (índice corto de ambigüedades realmente abiertas — hoy: 0), y se
agregó una guardia de regresión de autorización
(`backend/tests/Api.Tests/AutorizacionPedidosEntregasFacturasFlujoTests.cs`, 6 pruebas HTTP reales
marcadas `Skip` con motivo documentado, no silenciadas).

**Actualización (2026-09-17 09:32): P1 sí se implementó, en un segundo bloque de la misma
sesión**, por instrucción explícita del usuario de continuar con P1-4. Ver la entrada de historial
correspondiente más abajo para el detalle completo — el párrafo original de arriba quedó obsoleto
en cuanto a "no se implementó", se conserva sin editar por ser el registro de lo que era cierto en
ese momento del día.

**Corrección sobre la entrada de 2026-09-16 (bullet "F6 — Fase 6-9 en UI", más abajo):** decía que
la cadena Consolidación→Pedido→Entrega/Factura queda "inalcanzable desde el Frontend, no solo
Consolidación" — verificado en esta sesión que es parcialmente inexacto: `FacturacionPage.tsx`
**sí existe**, está enlazada en `AppLayout.tsx` (`ENLACES`) y ruteada en `AppRoutes.tsx`
(`/facturacion`) desde antes de esa entrada — es una pantalla provisional (IDs de
Proveedor/PedidoProveedor ingresados a mano, sin selección dinámica, comentario explícito en el
código: "Incremento MVP frontend↔backend, primer vertical slice real"). La afirmación de bloqueo
sigue siendo correcta para Consolidación, Pedidos y Entregas (esos sí son inalcanzables desde la
UI hoy) — la imprecisión era solo sobre Facturación.

**Suite de pruebas al cierre de ese primer bloque: 623/623 correctas + 6 nuevas `Skip` documentadas**
(234 `Domain.Tests` + 251 `Application.Tests` + 138 `Api.Tests` superadas, 6 `Api.Tests` omitidas
intencionalmente). `dotnet build backend` limpio (0 errores, 0 advertencias). Sin migraciones
nuevas en ese primer bloque.

**Segundo bloque, mismo día (10:08), instrucción explícita "continua con el orden ya
establecido":** P1-4 completado en la práctica (permisos sembrados en la base de datos de
desarrollo, ver historial); `ConsolidacionesController` nuevo (P2-1); verificado que las guardas
de RN-061/RN-062 (P3-1/P3-2) ya existían en Domain, sin necesitar código nuevo; `UsuarioCreacionId`
agregado a `PedidoProveedor`/`Entrega`/`Factura` (P2-2, con migración aplicada). `TASK-008` pasa a
`COMPLETADA`. Suite de pruebas al cierre de este bloque: **634/634 correctas, 0 omitidas** (235
`Domain.Tests` + 251 `Application.Tests` + 148 `Api.Tests`). Detalle completo en el historial de
esta fecha/hora.

### Estado general (2026-09-16, última sesión)

**Frontend — sesión real + reparación de Requisiciones (2026-09-16):** el Frontend estaba roto
contra el backend real desde el punto 8 (2026-09-15): `apiClient.ts` nunca enviaba
`Authorization`, y `requisicionesService.ts`/`catalogosService.ts` seguían usando el placeholder
`X-Usuario-Id`/`X-Empresa-Id` que el backend ya no lee. Se agregó `src/auth/` (login real,
sesión en `localStorage`, `AuthContext`), `apiClient.ts` adjunta el JWT automáticamente y limpia
la sesión ante un 401 con token ya guardado, se agregó `react-router-dom` (autorizado
explícitamente por el usuario) con `LoginPage`/`ProtectedRoute`/`AppLayout`, y se reparó
`RequisicionesPage`/`RequisicionPanel`/`requisicionesService` para dejar de enviar los headers
viejos (Empresa y Usuario se derivan del JWT, igual que en el backend). Probado end-to-end contra
la Api real con Chrome (login, recuperar una Requisición en Borrador existente, guardar borrador,
logout). Ver detalle completo en el historial de esta fecha.

**Bootstrap-admin resuelto (2026-09-16):** se sembró directamente en SQL Server (sin cambios de
código ni migración, decisión explícita del usuario) un usuario administrador de desarrollo:
correo `admin.frontend@auropaq.dev`, contraseña `Admin12345!`, EmpresaId 1 (Auropaq Colombia),
Rol `Administrador` (todos los permisos).

**Credenciales de prueba fijadas para Martha/Carlos (2026-09-16, confirmado explícitamente por el
usuario antes de ejecutar el `UPDATE`):** ya existían dos usuarios de una sesión anterior sin
contraseña conocida — `martha.taskdemo@auropaq.com` (Id 24, EmpresaId 1, rol `Administrador` ya
asignado) y `carlos.taskdemo@auropaq.com` (Id 55, EmpresaId 1, rol `Solicitante` ya asignado). El
usuario pidió usarlos como identidades de prueba reales (misma empresa que Martha, para poder
probar alcance por empresa entre ambos roles): se fijó la contraseña de **ambos** a
`Prueba1234!` (mismo hash, `PasswordHasher<T>` vía el mismo generador desechable ya usado para
`admin.frontend`). Login verificado por HTTP contra la Api real para los dos. Ningún otro campo
de ninguno de los dos usuarios se modificó (roles/empresa ya estaban correctos de antes).

### Estado general (2026-09-15, última sesión)

**Fases 0-5 (MVP, TASK-001 a 035): 100% `COMPLETADA`.** El gate de cierre de fase (ADR-060) ya
está satisfecho — Fases 6-13 pueden iniciar trabajo **nuevo**, no solo continuar lo
`EN_DESARROLLO`. Esto incluyó, cerrado en sesiones del 2026-09-15: EF Core + API de
Empresa/Sede/UsuarioSede/Rol/Permiso/UsuarioRol/RolPermiso/Categoría/UnidadMedida/Producto/
Solicitud producto no catalogado/Proveedor/Periodo; consulta y listado de Requisición con
alcance; y **autorización real (JWT + permiso + alcance) en absolutamente todos esos endpoints**
("punto 8" en el historial — RN-059/060, ADR-062/063 documentan el catálogo de ~20 permisos, la
matriz rol↔permiso, y el mapeo endpoint→permiso→alcance completo, ya sin ninguna ambigüedad).

**Fase 10 (Seguridad avanzada): 100% `COMPLETADA`** (TASK-048/049/050 vía el punto 8; TASK-051
corregido documentalmente el 2026-09-15 — su único ejemplo, alcance sobre `GET
/requisiciones/{id}`, ya estaba resuelto).

**Fase 11 (Calidad): 100% `COMPLETADA`** (2026-09-15): TASK-052/053/054 eran desincronización
documental (ya estaban implementadas); TASK-055 (Logging, `ILogger<T>`) y TASK-056 (Auditoría,
entidad `Auditoria` + migración `AgregarAuditoria`) se implementaron esa sesión.

**TASK-019 (Producto-Proveedor, Fase 2): `COMPLETADA`** (2026-09-15) — entidad, migración
`AgregarProductoProveedor`, API real bajo `/productos/{id}/proveedores` y
`/proveedores/{id}/productos`.

**Suite de pruebas al cierre de la última sesión: 623/623 correctas** (234 `Domain.Tests` + 251
`Application.Tests` + 138 `Api.Tests`). `dotnet build AuropaqPedidos.slnx` limpio (0 errores, 0
advertencias). Sin migraciones pendientes de aplicar (todas las creadas ya se aplicaron a la base
de datos de desarrollo local).

### Qué sigue abierto (por tipo, para elegir con criterio)

**Sin restricción — se puede empezar directamente si el usuario lo pide:**
- `TASK-061`/`TASK-062` (Fase 13: configuración por ambientes / variables de entorno y secretos)
  — no requieren decidir infraestructura de despliegue, son higiene de configuración. Sin auditar
  todavía en esta sesión.

**Requiere una decisión de negocio/diseño antes de codificar (no se puede improvisar):**
- ~~Extender autorización real (permiso + alcance + `[Authorize]`) a Fase 6-9~~ — **resuelto e
  implementado 2026-09-17** (RN-063/ADR-066/D-16): `PedidosProveedorController`/
  `EntregasController`/`FacturasController` ya exigen JWT + permiso real (sin alcance por empresa,
  `CLAUDE.md §27` — a diferencia de Requisición). Los permisos que faltaban (`Cerrar`/`Cancelar`
  de PedidoProveedor, `Anular` de Entrega/Factura) ya están catalogados. Ver la entrada de
  historial de esa fecha/hora para el detalle completo. **Pendiente operativo distinto:** esos 5
  permisos nuevos todavía no están sembrados en la base de datos de desarrollo ni asignados a
  ningún Rol real — ver `docs/decisiones-pendientes.md`.
- ~~Problema operativo de **bootstrap-admin**~~ — **resuelto 2026-09-16**, ver arriba.

**Frontend — backlog de UI acordado el 2026-09-16 (sin documento `UI-TASK-XXX`: no existe en
ningún equipo, se construyó este orden en la sesión a partir de docs/03-arquitectura.md §22/§23,
docs/05-api.md y el dominio; NO se inventó ningún endpoint ni regla nueva, solo se secuenció lo
que el backend ya expone):**
1. ~~F0 — Sesión (login, JWT, rutas protegidas)~~ y ~~F1 — Reparar Requisiciones~~ —
   **completados 2026-09-16**, ver historial de esta fecha.
2. ~~F2 — Catálogo administrativo: pantallas CRUD para Empresa/Sede/Periodo/Categoría/
   UnidadMedida/Producto/Proveedor~~ — **completado 2026-09-16**, ver historial de esta fecha.
3. ~~F3 — Seguridad: pantallas de Usuarios/Roles/Permisos y sus asignaciones~~ —
   **completado 2026-09-16**, ver historial de esta fecha. `UsuarioSede` (§62) queda fuera,
   no fue parte del pedido.
4. ~~F4 — Revisión: bandeja de pendientes-revisión, aprobar/devolver~~ — **completado
   2026-09-16**, ver historial de esta fecha.
5. ~~F5 — Solicitudes de producto no catalogado: listar pendientes, homologar/crear/rechazar~~ —
   **completado 2026-09-16**, ver historial de esta fecha.
6. **F6 — Fase 6-9 en UI: bloqueado, pausado por decisión explícita del usuario (2026-09-16).**
   Consolidación no tiene ningún endpoint ni un solo registro en la base de datos de desarrollo;
   `CrearPedidoProveedorRequest.ConsolidacionId` es obligatorio (Domain: *"Un pedido debe
   originarse en una consolidación"*, más FK real en base de datos) — no existe ningún
   `ConsolidacionId` válido, así que `POST /pedidos-proveedor` fallaría siempre. Entrega cuelga de
   un Pedido: también inalcanzable sin un `PedidoProveedorId` real.
   ~~toda la cadena Consolidación→Pedido→Entrega/Factura queda inalcanzable desde el Frontend~~
   **Corrección (2026-09-17):** Factura es la excepción — `FacturacionPage.tsx` ya existe
   (pantalla provisional con IDs ingresados a mano), construida antes de esta nota, sin que se
   reconciliara aquí. Consolidación y Pedido/Entrega sí siguen inalcanzables desde la UI. Construir
   el resto de esa UI hoy produciría pantallas que fallan siempre al usarse — el usuario prefirió
   pausar en vez de eso. Ver historial de esta fecha para el detalle completo.
Backlog de UI (F0-F5) queda **100% completado**. F6 pausado a la espera de una decisión sobre
Consolidación (ver arriba) — no es una tarea de Frontend por sí sola.

**Bloqueado por su propia regla documentada — no iniciar sin lo que piden:**
- Fase 12 (Rendimiento, TASK-057 a 060): `08-tareas.md` exige explícitamente "evidencia real de
  necesidad" antes de empezar (CLAUDE.md §44) — no existe todavía (sin tráfico de producción, sin
  consultas lentas medidas).
- `TASK-063`/`TASK-064` (Fase 13: build de producción / despliegue real): `08-tareas.md` dice
  textualmente "la infraestructura concreta debe decidirse según el entorno disponible" — requiere
  que el usuario elija el entorno de despliegue antes de que se pueda planear el trabajo.

**Sin ningún indicio de necesidad ni documentación que lo pida (no confundir con "pendiente"):**
- Nada más en Fases 0-11 sigue sin cerrar. `TASK-019` (única tarea suelta de menor número) ya se
  cerró el 2026-09-15.

### Cómo continuar en la próxima sesión

1. Leer esta sección primero (evita releer las ~1000 líneas del historial completo).
2. Si el usuario no indica prioridad, preguntar entre las opciones "sin restricción"/"requiere
   decisión" de arriba — nunca elegir Fase 12 o TASK-063/064 unilateralmente (violan su propia
   regla documentada de esperar evidencia/decisión).
3. Antes de escribir código para cualquier tarea de Fase 6-13, repetir el mismo protocolo ya usado
   toda la sesión: auditar el código real primero (suele estar desincronizado de las tablas de
   FASE) antes de asumir que algo falta.

---

## FASE 0 — Base técnica

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-001 | Inspeccionar y definir estructura actual | COMPLETADA | Informe retroactivo escrito 2026-09-15 (ver sección `## Informe de inspección — TASK-001` arriba); cumple los 5 puntos de `08-tareas.md` §6 con verificación real (`dotnet build`/`dotnet test`). |
| TASK-002 | Crear o ajustar estructura base del backend | COMPLETADA | Creados `backend/src/{Domain,Application,Infrastructure,Api}` (.NET 10 LTS) con referencias Domain←Application←Infrastructure←Api. Sin entidades/DTOs/endpoints/controllers/DbContext. Solución compila (`dotnet build` OK). |
| TASK-003 | Configurar persistencia | COMPLETADA | EF Core 10 + SQL Server. `AuropaqPedidosDbContext`, `IEntityTypeConfiguration<T>` por entidad, 5 repositorios EF Core, DI en `Infrastructure/DependencyInjection.cs`, registrado desde `Api/Program.cs`. Ver detalle en la entrada de esta fecha. |
| TASK-004 | Configurar migraciones | COMPLETADA | Migración `InitialCreate` creada y aplicada a SQL Server local (`dotnet ef database update` exitoso). |
| TASK-005 | Configurar pruebas | COMPLETADA | Corrección 2026-09-15 (esta nota estaba desincronizada del historial real): `Domain.Tests` (182 pruebas), `Application.Tests` (178 pruebas) y `Api.Tests` (77 pruebas, integración HTTP real con `WebApplicationFactory` contra SQL Server) existen y las dos primeras se confirmaron en verde en esta inspección (`dotnet test` → 182/182 y 178/178). Cumple el criterio de aceptación ("al menos una prueba ejecutable que confirme que el entorno de pruebas funciona") ampliamente. |

## FASE 1 — Organización

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-006 | Empresa | COMPLETADA | Corrección 2026-09-15 (esta fila estaba desincronizada: EF Core, migración y `GET /empresas` ya existían de una sesión anterior no reflejada aquí). Cierre real 2026-09-15: `POST/GET/GET{id}/PUT /api/v1/empresas` (05-api.md §11.1-§11.4) — crear, consultar, modificar y activar/desactivar (vía PUT). Sin unicidad de `Nit` (04-base-datos.md §5.1 la deja pendiente de confirmación del negocio; no se inventó). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:ORGANIZACION_VER/ADMINISTRAR")]`, sin alcance (Organización es administrativa/global, RN-059). |
| TASK-007 | Sede | COMPLETADA | Corrección 2026-09-15 (idem TASK-006: EF Core/migración/`GET".../sedes"` ya existían sin reflejarse aquí). Cierre real 2026-09-15: `POST /api/v1/empresas/{empresaId}/sedes` (crea, valida empresa exista y esté activa — 05-api.md §12.2) y `PUT /api/v1/sedes/{id}` (05-api.md §12.3) — actualizar datos y activar/desactivar. Empresa no se puede reasignar desde el PUT (RN-002). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:ORGANIZACION_VER/ADMINISTRAR")]`. |
| TASK-008 | Usuario | COMPLETADA | Domain/Application/Infrastructure/Api completos (`Empresa 1 ─── N Usuario`, Correo único GLOBAL, `POST`/`GET /api/v1/usuarios` con `[Authorize(Policy="Permiso:SEGURIDAD_ADMINISTRAR/VER")]` real desde 2026-09-15, punto 8). **Actualización (2026-09-17, P2-2):** `UsuarioCreacionId` agregado a `PedidoProveedor`/`Entrega`/`Factura` (D-11/RN-050, migración `AgregarUsuarioCreacionIdAPedidoEntregaFactura` aplicada) — único motivo que mantenía esta fila en `EN_DESARROLLO`, ya resuelto. Pasa a `COMPLETADA`. |
| TASK-009 | Usuario-Sede | COMPLETADA | Domain/Application/Infrastructure completos (`Usuario N ─── N Sede`, PK compuesta `UsuarioId+SedeId`, aislamiento multiempresa validado en Domain). API real (ADR-061): `POST`/`GET /api/v1/usuarios/{usuarioId}/sedes` (`05-api.md §62`). **Autorización real agregada 2026-09-15** (punto 8, RN-059/060, ADR-062/063): `[Authorize(Policy="Permiso:SEGURIDAD_ADMINISTRAR/VER")]`, sin alcance por empresa (Seguridad es administrativa/global). |
| TASK-010 | Rol | COMPLETADA | Domain/Application/Infrastructure completos (`Id, Nombre, Descripcion, Activo` — GLOBAL, sin `EmpresaId`, confirmado por `04-base-datos.md §9.1`). API real (ADR-061): `GET`/`POST /api/v1/roles` (`05-api.md §59`). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:SEGURIDAD_ADMINISTRAR/VER")]`. |
| TASK-011 | Permiso | COMPLETADA | Domain/Application/Infrastructure completos (`Id, Codigo, Nombre, Descripcion` — GLOBAL, sin `EmpresaId`, sin `Activo`, confirmado por `04-base-datos.md §9.2`). `Codigo` GLOBALMENTE ÚNICO (RN-055/ADR-056, hardening 2026-09-14 previo a TASK-013 — índice único en SQL Server + validación en Application). API real (ADR-061): `GET`/`POST /api/v1/permisos` (`05-api.md §60`). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:SEGURIDAD_ADMINISTRAR/VER")]`. |
| TASK-012 | Usuario-Rol | COMPLETADA | Modelo (`UsuarioRol`, N:N, sin validación de empresa porque Rol es global) implementado, adelantado dentro de `TASK-010`. Auditado en `TASK-014` (2026-09-14): funcionalmente completo (PK compuesta + FK Restrict verificadas en SQL Server, `AsignarRolAUsuarioUseCase`/`ObtenerRolesDeUsuarioUseCase`, tests que cubren explícitamente "un usuario puede tener uno o varios roles"). API real (ADR-061): `POST`/`GET /api/v1/usuarios/{usuarioId}/roles` (`05-api.md §61`). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:SEGURIDAD_ADMINISTRAR/VER")]` + prevención de auto-escalamiento (RN-060 punto 7: `AsignarRolAUsuarioUseCase` lanza `ReglaDeNegocioException` si `usuarioIdActor == usuarioId`). |
| TASK-013 | Rol-Permiso | COMPLETADA | Domain/Application/Infrastructure completos (`RolId, PermisoId` — sin `Id` propio, PK compuesta, mismo patrón que `UsuarioRol`, confirmado por `04-base-datos.md §9.4`). Cierra estructuralmente `Usuario → UsuarioRol → Rol → RolPermiso → Permiso`. API real (ADR-061): `POST`/`GET /api/v1/roles/{rolId}/permisos` (`05-api.md §63`). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:SEGURIDAD_ADMINISTRAR/VER")]`. |

## FASE 2 — Catálogo

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-014 | Categoría | COMPLETADA | Corrección 2026-09-15 (esta fila estaba desincronizada: EF Core/migración ya existían de una sesión anterior no reflejada aquí). Cierre real 2026-09-15: `GET/POST/GET{id}/PUT /api/v1/categorias` (05-api.md §14) — crear, consultar, modificar y activar/desactivar. **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:PRODUCTO_VER/CREAR/EDITAR")]`, sin alcance (Catálogo es global). |
| TASK-015 | Unidad de medida | COMPLETADA | Ídem TASK-014. Cierre real 2026-09-15: `GET/POST/GET{id}/PUT /api/v1/unidades-medida` (05-api.md §15). Sin unicidad de `Codigo` (no documentada, mismo criterio que `Empresa.Nit`). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:PRODUCTO_VER/CREAR/EDITAR")]`. |
| TASK-016 | Producto | COMPLETADA | Modelada la entidad de dominio `Producto`, referencia a `Categoria`/`UnidadMedida` por navegación. `CodigoInterno` opcional (04-base-datos.md: "cuando el negocio lo requiera"), sin unicidad (pendiente de confirmación del negocio, no se inventó). Cierre real 2026-09-15: `GET/POST/GET{id}/PUT /api/v1/productos` (05-api.md §13) — Categoría/UnidadMedida inexistentes devuelven 404 al crear/actualizar. **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:PRODUCTO_VER/CREAR/EDITAR")]`. |
| TASK-017 | Solicitud de producto no catalogado | COMPLETADA | Modelada `SolicitudProductoCatalogo` con `Homologar()`/`Crear()`/`Rechazar()` (RN-024/025/026); estados `Pendiente/Homologado/Creado/Rechazado`. `UsuarioId` sigue como `int` (no se cambió a navegación — fuera de alcance, ver historial 2026-09-15). Cierre real 2026-09-15: `POST /solicitudes-producto`, `GET .../pendientes`, `POST .../{id}/homologar`, `POST .../{id}/crear-producto`, `POST .../{id}/rechazar` (05-api.md §28). **Autorización real agregada 2026-09-15** (punto 8): `PRODUCTO_SOLICITAR` (Solicitar) / `PRODUCTO_VER` (Listar pendientes) / `PRODUCTO_CREAR` (Homologar/Crear/Rechazar); ya no usa `X-Usuario-Id`/`X-Empresa-Id`, el actor y la empresa se derivan del JWT. |
| TASK-018 | Proveedor | COMPLETADA | Implementación mínima previa (Id, Nombre, Nit, Contacto, Telefono, Correo, Activo — `04-base-datos.md §14`) ampliada 2026-09-15 con CRUD real: `GET/POST/GET{id}/PUT /api/v1/proveedores` (05-api.md §29). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:PROVEEDOR_VER/CREAR/EDITAR")]`, sin alcance (Proveedor es catálogo compartido). |
| TASK-019 | Producto-Proveedor | COMPLETADA | 2026-09-15: entidad `ProductoProveedor` (`Id, ProductoId, ProveedorId, CodigoProveedor, DescripcionProveedor, CategoriaProveedor, UnidadProveedor, Activo`, según el modelo exacto de `04-base-datos.md §15`) + migración `AgregarProductoProveedor` (FK a `Productos`/`Proveedores`, índice único `(ProductoId, ProveedorId)`). API real (`05-api.md §30`): `GET/POST /api/v1/productos/{productoId}/proveedores`, `PUT /api/v1/productos/{productoId}/proveedores/{relacionId}`, `GET /api/v1/proveedores/{proveedorId}/productos`. `[Authorize(Policy="Permiso:PRODUCTO_VER/CREAR/EDITAR")]` (rutas bajo `/productos`) y `PROVEEDOR_VER` (ruta bajo `/proveedores`), sin alcance (catálogo global). Sin `UnidadesPorEmpaque`/`CantidadMinimaCompra` (descartados, `01-reglas-negocio.md §15` Pendiente 8). No es prerrequisito estructural de `PedidoProveedor` (`DetallePedidoProveedor` sigue referenciando `Producto` directamente, `04-base-datos.md §27`) — sigue siendo una relación de homologación independiente. |

## FASE 3 — Periodos

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-020 | Periodo mensual | COMPLETADA | Corrección 2026-09-15 (esta fila estaba desincronizada: EF Core/migración/`Listar`/`Crear` ya existían de una sesión anterior no reflejada aquí). `Estado` sigue como texto libre (sin valores documentados, a diferencia de `SolicitudProductoCatalogo`; no se inventó). Cierre real 2026-09-15: `GET /api/v1/periodos/{id}` (05-api.md §16.2) — único endpoint que faltaba (Listar/Crear ya existían). Sin `PUT` (no documentado para Periodo). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:PERIODO_VER/CREAR")]`, sin alcance (Periodo es global). |
| TASK-021 | Ventana de solicitud | COMPLETADA | La ventana (`FechaInicioSolicitud`/`FechaFinSolicitud`) es parte de `Periodo`, totalmente configurable (RN-006, sin fechas fijas), ya expuesta en el mismo contrato de TASK-020. `EstaDentroDeVentanaDeSolicitud(fecha)` como comportamiento de dominio para RN-006/RN-015. |

## FASE 4 — Requisiciones

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-022 | Crear o recuperar requisición mensual | COMPLETADA | Corrección 2026-09-15 (esta fila estaba muy desincronizada: Infrastructure/Api ya existían completos desde sesiones anteriores, nunca reflejado aquí). `IniciarOContinuarRequisicionUseCase` + `RequisicionRepositoryEfCore` (real, SQL Server) + `POST /api/v1/requisiciones` (05-api.md §18). No crea duplicados (`ObtenerPorEmpresaYPeriodo`). |
| TASK-023 | Agregar producto a requisición | COMPLETADA | `Requisicion.AgregarDetalle()` + `AgregarDetalleRequisicionUseCase` (producto existente y activo, cantidad > 0) + `POST /requisiciones/{id}/detalles` (05-api.md §19). **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:REQUISICION_CREAR")]` + `[Authorize(Policy="AlcanceRequisicion")]`; ya no usa `X-Usuario-Id`/`X-Empresa-Id`. |
| TASK-024 | Modificar detalle | COMPLETADA | `ModificarCantidadDetalle()`/`ModificarObservacionDetalle()` (Domain) + `ActualizarDetalleRequisicionUseCase` (un solo PUT, igual que 05-api.md §20) + `ModificarDistribucionRequisicionUseCase` + endpoints reales. |
| TASK-025 | Eliminar detalle | COMPLETADA | `EliminarDetalle()` (Domain, solo mientras `EsEditable` — nunca sobre un proceso cerrado/histórico, RN de la tarea) + `EliminarDetalleRequisicionUseCase`/`EliminarDistribucionRequisicionUseCase` + `DELETE` reales (05-api.md §21). |
| TASK-026 | Distribuir cantidad por sede | COMPLETADA | `AgregarDistribucion()` (Domain, valida sede de la misma empresa — RN-002/`AsegurarSedeDeLaEmpresa`) + `AgregarDistribucionRequisicionUseCase` + `POST .../distribuciones` real. **Ambigüedad ya reportada, sin resolver (no bloqueante):** implementado granular (agregar/modificar/eliminar una distribución a la vez); 05-api.md §22 documenta un PUT que reemplaza toda la distribución de un detalle en un solo llamado — decisión ya tomada en sesión anterior de exponer el contrato granular real en vez de forzar ese PUT documentado (ver comentario en `RequisicionesController`). |
| TASK-027 | Validar requisición | COMPLETADA | `Enviar()` (Domain) valida: al menos un detalle, empresa activa, ventana de solicitud del periodo, distribución completa de cada detalle. Producto válido/cantidad válida/sede de la empresa ya se garantizan al agregar (TASK-023/026), no se revalidan de nuevo al enviar. "No duplicada" se garantiza estructuralmente en TASK-022. "Usuario autorizado" es real (permiso + alcance) en **todas** las rutas de Requisición desde 2026-09-15 (punto 8) — ya no solo en `Enviar`/`Aprobar` (TASK-049/050). |
| TASK-028 | Guardar borrador | COMPLETADA | `GuardarBorradorRequisicionUseCase` + `POST /requisiciones/{id}/guardar` real (sin ruta documentada en 05-api.md; expuesta como acción explícita, misma convención que enviar/aprobar/devolver — decisión ya tomada en sesión anterior). |
| TASK-029 | Enviar requisición | COMPLETADA | `EnviarRequisicionUseCase` + `POST /requisiciones/{id}/enviar` (05-api.md §23) con `[Authorize]` real (permiso `REQUISICION_ENVIAR` + alcance por empresa, TASK-049/050). Ambigüedad ya reportada sobre la transición ENVIADA→EN_REVISION (sin dueño documentado; se mantiene `IniciarRevision()` como acción explícita separada) — sin resolver, no bloqueante. |
| TASK-030 | Registrar historial de requisición | COMPLETADA | `HistorialRequisicion` (Domain, se registra en cada transición de estado real: Enviar/IniciarRevision/Aprobar/Devolver) expuesto en `RequisicionResponse.Historial` vía `RequisicionMapper`. |

## FASE 5 — Revisión

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-031 | Listar requisiciones pendientes de revisión | COMPLETADA | Cierre real 2026-09-15: `GET /api/v1/requisiciones/pendientes-revision` (05-api.md §24.1/§54.6). La empresa se deriva del `Usuario` autenticado (JWT), ya no de `X-Empresa-Id`. **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:REQUISICION_VER")]`. |
| TASK-032 | Consultar detalle de requisición | COMPLETADA | Cierre real 2026-09-15: `ObtenerRequisicionUseCase` + `GET /api/v1/requisiciones/{id}` (05-api.md §17.2/§54.6). **Corrección 2026-09-15 (RN-059/ADR-062):** la decisión original de esta fila ("sin alcance por empresa") se revirtió tras el análisis completo de `06-seguridad.md §60/§61` — SÍ debe llevar alcance como el resto de las rutas de Requisición; se agregó `[Authorize(Policy="Permiso:REQUISICION_VER")]` + `[Authorize(Policy="AlcanceRequisicion")]`. También se agregó `GET /api/v1/requisiciones` ("mis requisiciones", 05-api.md §17.1, misma autorización, empresa derivada del JWT) como parte del mismo cierre — cubre el otro vacío que señalaba §54.6 punto 1. |
| TASK-033 | Aprobar requisición | COMPLETADA | Corrección 2026-09-15 (misma fila desincronizada: ya tenía Api real con `[Authorize]` completo, nunca reflejado aquí). `Requisicion.Aprobar()` + `AprobarRequisicionUseCase` + `POST /requisiciones/{id}/aprobar` (05-api.md §24.2) con permiso `REQUISICION_APROBAR` + alcance por empresa reales (TASK-049/050, punto 8). |
| TASK-034 | Devolver requisición | COMPLETADA | `Requisicion.Devolver()` + `DevolverRequisicionUseCase` + `POST /requisiciones/{id}/devolver` (05-api.md §25) real. **Autorización real agregada 2026-09-15** (punto 8): `[Authorize(Policy="Permiso:REQUISICION_DEVOLVER")]` + `[Authorize(Policy="AlcanceRequisicion")]`; ya no usa `X-Usuario-Id`. |
| TASK-035 | Corregir y reenviar requisición | COMPLETADA | DEVUELTA es editable; el reenvío reutiliza `EnviarRequisicionUseCase` (misma decisión que en Domain: un solo método para envío y reenvío). Probado en `Requisicion_devuelta_puede_corregirse_y_reenviarse` y en el flujo real de Api.Tests. |

---

Con TASK-035 se cierra el **primer flujo funcional (MVP)** definido en `docs/08-tareas.md` §20:
`LOGIN → EMPRESA → SEDE → PERIODO → REQUISICIÓN → BORRADOR → AGREGAR PRODUCTOS → DISTRIBUIR → VALIDAR → ENVIAR → REVISIÓN → APROBAR/DEVOLVER → CORREGIR → REENVIAR`.

Las fases siguientes (Consolidación, Pedidos, Entregas, Facturación, Seguridad avanzada, Calidad, Rendimiento, Despliegue) **no deben iniciarse** hasta que el MVP anterior esté estable y autorizado, según `docs/08-tareas.md` §21.

---

## FASE 6 — Consolidación *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-036 | Crear consolidación | EN_DESARROLLO | Domain: `Consolidacion` (agregado raíz) + `CrearConsolidacionUseCase` (Application) + `IConsolidacionRepository`/EF Core (Infrastructure) + migración aplicada. **Sin endpoint API por decisión ya documentada** (`05-api.md §31`: Consolidación se invoca directo desde `FlujoIntegradoA2B1B4Tests` vía el contenedor de DI, no por HTTP) — no es un hueco pendiente, es una decisión tomada. **Actualización (2026-09-17):** la autorización de Fase 6-9 ya no está pospuesta (RN-063/ADR-066) — pero esta tarea sigue `EN_DESARROLLO` por su propio motivo original, sin API HTTP (ver nota de arriba), no por autorización. |
| TASK-037 | Generar detalles de consolidación | EN_DESARROLLO | `Consolidacion.AgregarAsignacion` agrupa por `Producto` (RN-004/RN-027), reutilizando `DetalleConsolidacion` existente para el mismo producto. |
| TASK-038 | Mantener trazabilidad de consolidación | EN_DESARROLLO | `AsignacionConsolidacion` referencia `DetalleRequisicion` de origen (RN-029) sin modificarlo (RN-028); probado en Domain.Tests y Application.Tests. |

## FASE 7 — Pedidos a proveedor *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-039 | Crear pedido proveedor | EN_DESARROLLO | Domain: `PedidoProveedor` (agregado raíz). Application: `CrearPedidoProveedorUseCase` (+ auditoría "creación de pedido", TASK-056). **Corrección 2026-09-15 (nota desincronizada):** decía "Pendientes 7/9 sin resolver", pero ambos están `RESUELTA` desde el cierre documental 2026-09-11 (`01-reglas-negocio.md §14`, ver RN-041/RN-049/D-01/D-09) — `ProveedorId`/`NumeroPedido` explícitos del llamador es la decisión ya tomada, no una ambigüedad abierta. **Actualización (2026-09-17):** autorización real ya implementada (`[Authorize(Policy="Permiso:PEDIDO_CREAR")]` y equivalentes, RN-063/ADR-066), permisos ya sembrados y asignados en la base de datos de desarrollo, y `UsuarioCreacionId` agregado (P2-2). El motivo original que mantenía esta fila en `EN_DESARROLLO` (autorización pendiente) queda resuelto — no se revisó en esta sesión si quedan otros criterios pendientes de `08-tareas.md` para promoverla a `COMPLETADA` (ej. sin `GET`); se deja como `EN_DESARROLLO` hasta una revisión dedicada de cierre. |
| TASK-040 | Agregar productos al pedido | EN_DESARROLLO | Domain: `PedidoProveedor.AgregarDetalle` (captura `CantidadNecesaria` como fotografía, exige `CantidadPedida` explícita — RN-031). Application: `AgregarDetallePedidoProveedorUseCase`. |
| TASK-041 | Distribuir pedido entre sedes | EN_DESARROLLO | Domain: `PedidoProveedor.AgregarDistribucion`/`DistribucionPedido` (sede de cualquier empresa). Application: `AgregarDistribucionPedidoUseCase`. Sin regla "SUM = cantidad" (no documentada para pedidos, a diferencia de RN-011 para requisiciones). |

## FASE 8 — Entregas *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-042 | Registrar entrega | EN_DESARROLLO | Domain: `Entrega` (agregado raíz). Application: `CrearEntregaUseCase`. `NumeroRemision` explícito del llamador (mismo criterio que `PedidoProveedor.NumeroPedido`). |
| TASK-043 | Registrar cantidades entregadas | EN_DESARROLLO | Domain: `Entrega.AgregarDetalle` valida que la cantidad entregada acumulada (entre todas las entregas del pedido) no supere `CantidadPedida` (04-base-datos.md §30 "Regla"). Application: `AgregarDetalleEntregaUseCase` calcula el acumulado vía `IEntregaRepository.ObtenerPorPedido`. |
| TASK-044 | Distribuir entrega por sede | EN_DESARROLLO | Domain: `DistribucionEntrega` congela dirección/ciudad/contacto de la Sede en el momento de crearse (RN-035/ADR-019, snapshot histórico). Application: `AgregarDistribucionEntregaUseCase`. |
| TASK-045 | Calcular cantidades pendientes | EN_DESARROLLO | Application: `CalcularCantidadPendienteUseCase` (consulta derivada: `CantidadPedida - SUM(CantidadEntregada)`, cruza `PedidoProveedor` + `Entrega`, no se persiste). |

## FASE 9 — Facturación y cierre *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-046 | Registrar factura | EN_DESARROLLO | `Factura` 1─N respecto a `PedidoProveedor`, `DetalleFactura`, Subtotal/Total calculados. API: `POST /api/v1/facturas`, `POST /api/v1/facturas/{id}/detalles`, `POST /api/v1/facturas/{id}/anular` (+ logging, TASK-055). **Corrección 2026-09-15 (nota desincronizada):** decía "sin ciclo de vida de Estado", pero `FacturaEstado` (`REGISTRADA`/`ANULADA`) ya existe y está implementado desde el cierre documental 2026-09-11 (RN-047/D-05) — confirmado en código (`Factura.Anular()`, `AnularFacturaUseCase`). Sigue sin `UsuarioCreacionId` (RN-050/D-11, ahora técnicamente desbloqueable — ver Pendiente 16 de `01-reglas-negocio.md`, ya existe Usuario/autenticación real) y sin `TASK-047` completa (`TASK-047` en sí ya está implementada para `PedidoProveedor`, ver su fila en FASE 9; lo que falta es la parte de Factura del "cierre" si el negocio la define). **Actualización (2026-09-17):** autorización real ya implementada (`FACTURA_REGISTRAR`/`FACTURA_ANULAR`, RN-063/ADR-066), permisos ya sembrados y asignados, y `UsuarioCreacionId` agregado (P2-2) — el motivo que decía "Sigue sin `UsuarioCreacionId`" arriba queda resuelto. No se revisó en esta sesión si quedan otros criterios pendientes para promoverla a `COMPLETADA`. |
| TASK-047 | Validar cierre | EN_DESARROLLO | **Corrección 2026-09-15 (fila muy desincronizada: decía `PENDIENTE`, pero ya estaba implementada desde antes, nunca reflejada aquí).** `PedidoProveedor.Cerrar()` (Domain, solo permite `ENTREGADO → CERRADO`, lo que ya garantiza cantidad pendiente = 0 vía RN-034/RN-043) + `CerrarPedidoProveedorUseCase` + `POST /api/v1/pedidos-proveedor/{id}/cerrar` (05-api.md §32) — probado. La regla completa de RN-044 es "cantidad pendiente = 0 **+ acción explícita de un usuario autorizado**": la primera mitad está `COMPLETADA`; la segunda (autorización real sobre `Cerrar`) sigue faltando — mismo motivo que el resto de Fase 6-9 (pospuesta). |

## FASE 10 — Seguridad avanzada *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-048 | Autenticación | COMPLETADA | JWT propio implementado (Domain/Application/Infrastructure/Api + migración + tests). Ver ADR-057. `POST /api/v1/auth/login`. **2026-09-15 (punto 8):** `[Authorize]` ya cubre prácticamente todos los controllers del MVP (Empresa/Sede/Rol/Permiso/UsuarioRol/UsuarioSede/Categoría/UnidadMedida/Producto/Proveedor/Solicitud/Periodo/Requisición) — ya no es "ningún controller". |
| TASK-049 | Autorización por permisos | COMPLETADA | `IAuthorizationPolicyProvider` dinámico sobre `Permiso.Codigo` (ver ADR-058). **2026-09-15 (punto 8, RN-059/060, ADR-062/063):** catálogo completo de ~20 permisos definido y mapeado a todos los endpoints del MVP; el propio `RequisicionesController` pasó de 2 endpoints protegidos (`REQUISICION_ENVIAR`/`REQUISICION_APROBAR`) a **todas** sus rutas (`REQUISICION_CREAR/VER/MODIFICAR/ENVIAR/APROBAR/DEVOLVER`). |
| TASK-050 | Autorización por alcance | COMPLETADA | Alcance por Empresa (`Usuario.Empresa == Requisicion.Empresa`, ver RN-058/ADR-059) extendido 2026-09-15 (punto 8) a **todas** las rutas de Requisición (antes solo `enviar`/`aprobar`), incluyendo la corrección de `GET /requisiciones/{id}` (RN-059, ver TASK-032). Alcance por sede: decisión explícita de posponerlo (2026-09-14), no una ambigüedad abierta. El resto de módulos del MVP (Organización/Seguridad/Catálogo/Proveedor/Periodo) son administrativos/globales por decisión — sin alcance por empresa (RN-059). |
| TASK-051 | Protección contra manipulación de IDs | COMPLETADA | Corrección 2026-09-15 (desincronizada: el ejemplo literal de la tarea — `GET /requisiciones/{id}` debe verificar que el usuario tenga autorización sobre esa requisición — ya está resuelto desde el punto 8 vía `[Authorize(Policy="AlcanceRequisicion")]`, ver TASK-050/032). Reforzado en la misma fecha en `ActualizarProductoProveedorUseCase` (TASK-019): el `productoId` de la ruta se valida contra la relación real antes de aplicar el `PUT`, en vez de confiar en el segmento de la URL (mismo criterio que `RequisicionFinder.ObtenerDetalleOLanzar`). Los módulos administrativos/globales (Organización/Seguridad/Catálogo/Proveedor/Periodo) no tienen este riesgo por diseño: cualquier usuario autorizado por permiso puede ver cualquier registro, no hay "alcance" que manipular. Sigue sin este control Fase 6-9 (Pedido/Entrega/Factura) — consistente con que esos módulos aún no tienen autorización real (decisión ya documentada, ver "punto 8" en el historial). |

## FASE 11 — Calidad *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-052 | Pruebas de reglas de negocio | COMPLETADA | Corrección 2026-09-15 (estaba `PENDIENTE`, desincronizada: ya existían pruebas reales para los 6 ejemplos de `08-tareas.md`). Los 6 casos (empresa+periodo duplicado, distribución incorrecta, cantidad inválida, fuera de ventana, usuario sin permiso, modificación no editable) están cubiertos en `Domain.Tests`/`Application.Tests`/`Api.Tests`. |
| TASK-053 | Pruebas de API | COMPLETADA | Corrección 2026-09-15 (idem TASK-052). 129 pruebas de integración HTTP real (`Api.Tests`, `WebApplicationFactory` + SQL Server) cubren endpoints, códigos HTTP, validaciones, autorización (401/403/alcance) y el envoltorio de error. |
| TASK-054 | Manejo centralizado de errores | COMPLETADA | Corrección 2026-09-15 (idem TASK-052). `ExcepcionesDeNegocioHandler` (`IExceptionHandler`, `Api/ErrorHandling/`) traduce toda excepción de Application/Domain a una respuesta HTTP consistente (`RECURSO_NO_ENCONTRADO`/`REGLA_DE_NEGOCIO_VIOLADA`/`CREDENCIALES_INVALIDAS`/`ERROR_INTERNO`), sin exponer stack traces ni información interna; los 500 se registran vía `ILogger`. |
| TASK-055 | Logging | COMPLETADA | 2026-09-15: `ILogger<T>` (Microsoft.Extensions.Logging.Abstractions, ya usado transitivamente por Api — sin nuevo framework de terceros) agregado a los casos de uso de operaciones de negocio relevantes: Requisición (Enviar/Aprobar/Devolver), PedidoProveedor (Crear/Cerrar/Cancelar), Entrega (Crear/Anular), Factura (Registrar/Anular). Puramente técnico/diagnóstico — no sustituye `HistorialRequisicion` ni `Auditoria` (TASK-056). |
| TASK-056 | Auditoría | COMPLETADA | 2026-09-15: entidad `Auditoria` nueva (`Id, UsuarioId?, Entidad, EntidadId, Accion, Fecha, DatosAnteriores?, DatosNuevos?`, según el modelo conceptual ya documentado en `04-base-datos.md §33`) + migración `AgregarAuditoria`. Alcance acordado con el usuario: solo los 6 ejemplos documentados, sin re-auditar Requisición (`HistorialRequisicion` ya cumple ese rol, 05-api.md §23) — se registra en `CrearPedidoProveedorUseCase` ("creación de pedido"), `CrearEntregaUseCase` ("registro de entrega") y `AsignarRolAUsuarioUseCase` ("cambios administrativos", el más sensible ya modelado). `UsuarioId` es nullable: Pedidos/Entregas todavía no exigen JWT (decisión explícita del usuario, ver nota debajo). |

## FASE 12 — Rendimiento *(solo con evidencia real de necesidad)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-057 | Analizar consultas lentas | PENDIENTE | |
| TASK-058 | Índices | PENDIENTE | |
| TASK-059 | Paginación y filtrado | PENDIENTE | |
| TASK-060 | Optimización | PENDIENTE | |

## FASE 13 — Despliegue *(posterior al MVP)*

| Tarea | Descripción | Estado | Notas |
|---|---|---|---|
| TASK-061 | Configuración por ambientes | PENDIENTE | |
| TASK-062 | Variables de entorno y secretos | PENDIENTE | |
| TASK-063 | Build de producción | PENDIENTE | |
| TASK-064 | Despliegue | PENDIENTE | |

---

## Historial de decisiones registradas aquí

> Historial anterior al 2026-09-17 (desde 2026-09-08): archivado sin modificar en
> `docs/progreso-archive/2026-09.md` (ver nota de cabecera de este archivo). Las entradas nuevas
> se agregan aquí, siguiendo el mismo `## Formato estándar de una entrada de historial` de arriba.

- **2026-09-17 09:08 (análisis de estado + tareas de proceso)** — Bloque solicitado explícitamente
  por el usuario: analizar código vs. `docs/00-08` vs. `progreso.md`, proponer una lista de tareas,
  resolver las ambigüedades de negocio abiertas, y dejar todo documentado en un archivo con la
  fecha de hoy. Alcance: `docs/` (01, 07, 08, dos archivos nuevos), `progreso.md` (este archivo,
  reestructurado), `backend/tests/Api.Tests/` (una prueba de regresión nueva). **No** se tocó
  Domain/Application/Infrastructure/Api de producción, ni el Frontend.
  - **Qué se hizo:**
    - Análisis completo código vs. documentación (agente en background) — resultado consolidado en
      `docs/2026-09-17-tareas.md`, con una corrección propia detectada al releer
      `01-reglas-negocio.md §15` directamente (el primer diagnóstico daba 7 ambigüedades abiertas
      cuando solo quedaban 2 — ver sección 0 de ese archivo).
    - Pendiente 1 (envío fuera de ventana) y Pendiente 2 (modificación tras aprobación) resueltas
      y aprobadas explícitamente por el usuario: `RN-061`/`ADR-064`/`D-14` y
      `RN-062`/`ADR-065`/`D-15` (`01-reglas-negocio.md §15`, `07-decisiones-arquitectura.md §67-68`
      — sección de ADRs renumerada de 67-70 a 69-72 para insertar los dos nuevos antes de las
      secciones de cierre, sin romper ninguna referencia cruzada existente, verificado por grep).
    - `08-tareas.md` sincronizado: TASK-036 a TASK-045 no tenían `### Estado` (se agregó,
      `EN_DESARROLLO` en todos); TASK-046/047 tenían `### Estado: PENDIENTE` desactualizado (código
      real ya las implementa) — corregido a `EN_DESARROLLO`, notas viejas marcadas obsoletas en vez
      de borradas.
    - `docs/decisiones-pendientes.md` creado — índice corto de ambigüedades de negocio realmente
      abiertas (hoy: 0), para no tener que releer `01-reglas-negocio.md §15` completo cada vez.
    - Guardia de regresión de autorización agregada:
      `AutorizacionPedidosEntregasFacturasFlujoTests.cs` (6 pruebas HTTP reales sin JWT contra
      `PedidosProveedorController`/`EntregasController`/`FacturasController`, mismo patrón que
      `AutorizacionFlujoTests`). Quedan `[Fact(Skip = "...")]` — no se implementó `[Authorize]` en
      esos controllers en esta sesión (ver "Continúa" abajo), así que forzarlas a pasar habría
      requerido inventar permisos no catalogados o dejar el build en rojo; `Skip` con motivo
      documentado deja la brecha visible en el propio reporte de pruebas sin romper la suite.
    - `progreso.md` archivado: historial 2026-09-08 a 2026-09-16 movido íntegro a
      `docs/progreso-archive/2026-09.md`; este archivo conserva cabecera, estados/formato, tablas
      de FASE (ya estaban sincronizadas, no se modificaron salvo la corrección puntual de abajo) y
      el historial reinicia aquí.
    - Corrección puntual en `### Estado general (2026-09-16...)`, bullet "F6 — Fase 6-9 en UI":
      afirmaba que Factura también era inalcanzable desde el Frontend; verificado en código
      (`FacturacionPage.tsx` existe, ruteada en `AppRoutes.tsx`, enlazada en `AppLayout.tsx`) que
      es inexacto — se corrigió con anotación explícita, sin borrar el texto original.
  - **Archivos creados:** `docs/2026-09-17-tareas.md`, `docs/decisiones-pendientes.md`,
    `docs/progreso-archive/2026-09.md`,
    `backend/tests/Api.Tests/AutorizacionPedidosEntregasFacturasFlujoTests.cs`.
  - **Archivos modificados:** `docs/01-reglas-negocio.md`, `docs/07-decisiones-arquitectura.md`,
    `docs/08-tareas.md`, `progreso.md`. Ningún archivo de `backend/src` ni `frontend/src`.
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Migración:** ninguna (sin cambios de Domain/Infrastructure).
  - **Pruebas:** `dotnet test` (desde `backend/`) → **623/623 correctas + 6 omitidas** (234
    `Domain.Tests` + 251 `Application.Tests` + 138 `Api.Tests` superadas; 6 `Api.Tests` nuevas
    `Skip`, motivo documentado en el propio código).
  - **Ambigüedades / contradicciones encontradas en docs:** ver "Corrección puntual" arriba
    (Facturación sí alcanzable desde el Frontend) — no bloqueante, ya corregida en el mismo bloque.
  - **Decisiones pendientes de negocio:** ninguna (0, ver `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** P1 (`docs/2026-09-17-tareas.md`) — agregar `[Authorize]` real a
    `PedidosProveedorController`/`EntregasController`/`FacturasController` — **no implementado en
    esta sesión**, sin autorización explícita del usuario para esa parte específica, y porque
    Enviar/Cerrar/Cancelar (`PedidoProveedor`) y Anular (`Entrega`/`Factura`) no tienen permiso
    catalogado en `06-seguridad.md §53` todavía (P1-4, decisión de negocio real: qué rol recibe
    cada permiso — no se inventa sin decisión). También pendientes: P2-1/P2-2/P2-5 y P3-1/P3-2
    (`docs/2026-09-17-tareas.md`).

- **2026-09-17 09:32 (P1-4 + P1: catálogo de permisos y cierre de la brecha de seguridad)** —
  Bloque solicitado explícitamente por el usuario, continuación de la sesión anterior del mismo
  día: "continuemos con P1-4 (decidir el catálogo de permisos faltantes) para poder cerrar la
  brecha de seguridad". Alcance: `docs/01-reglas-negocio.md`, `docs/06-seguridad.md`,
  `docs/07-decisiones-arquitectura.md` (decisión); `backend/src/Api/Controllers/`
  (`PedidosProveedorController`, `EntregasController`, `FacturasController`,
  `Api/Common/IdentidadOpcional.cs`) y `backend/tests/Api.Tests/` (6 archivos) (implementación).
  **No** se tocó Domain/Application/Infrastructure ni el Frontend.
  - **Qué se hizo:**
    - RN-063/ADR-066/D-16: catálogo nuevo `PEDIDO_ENVIAR`/`PEDIDO_CERRAR`/`PEDIDO_CANCELAR`/
      `ENTREGA_ANULAR`/`FACTURA_ANULAR`, asignados por simetría con el permiso base de la misma
      entidad (mismos roles que `PEDIDO_CREAR`/`ENTREGA_REGISTRAR`/`FACTURA_REGISTRAR`
      respectivamente — mismo criterio que RN-060). Sin alcance por empresa (`CLAUDE.md §27`).
      `06-seguridad.md §9/§52/§53` actualizado (catálogo, mapeo endpoint→permiso, matriz de
      roles — ya no "fase futura").
    - `[Authorize(Policy = "Permiso:XXX")]` agregado a las 13 acciones de los tres controllers
      (`PedidosProveedorController`: Crear/AgregarDetalle/AgregarDistribucion→`PEDIDO_CREAR`,
      Enviar→`PEDIDO_ENVIAR`, Cerrar→`PEDIDO_CERRAR`, Cancelar→`PEDIDO_CANCELAR`,
      CrearEntrega→`ENTREGA_REGISTRAR`; `EntregasController`: AgregarDetalle/AgregarDistribucion→
      `ENTREGA_REGISTRAR`, Anular→`ENTREGA_ANULAR`; `FacturasController`:
      Registrar/AgregarDetalle→`FACTURA_REGISTRAR`, Anular→`FACTURA_ANULAR`).
    - `AutorizacionPedidosEntregasFacturasFlujoTests` (agregada `Skip` en el bloque anterior del
      mismo día): quitado el `Skip` de las 6 pruebas — ahora confirman 401 real sin JWT.
    - `EscenarioPedidoProveedor`/`EscenarioFactura`: se agregó `EmpresaId` al record devuelto
      (aditivo, sin romper accesos por nombre existentes) para poder emitir un token real por
      escenario.
    - `PedidosProveedorFlujoTests`/`EntregasFlujoTests`/`EscenarioFactura`-based
      `FacturasFlujoTests`/`EntregaAtomicidadTests`/`FlujoIntegradoA2B1B4Tests`: cada uno ahora
      autentica `_cliente` (vía `AutorizacionHelper.CrearTokenConPermisosAsync` +
      `DefaultRequestHeaders.Authorization`) con los permisos que su flujo necesita — no prueban
      autorización granular (eso es `AutorizacionPedidosEntregasFacturasFlujoTests`), solo
      comportamiento de negocio, así que un único token con varios permisos por archivo es
      suficiente y consistente con el criterio ya usado en `RequisicionesFlujoTests`.
  - **Bloqueador encontrado y resuelto:** `FacturasFlujoTests` usaba `numero` 1-11 ya cubriendo
    los 11 meses disponibles de `EscenarioFactura` (Año fijo 2026); los `numero` 101/102 que
    agregué para autenticar dos pruebas sin escenario completo colisionaban con `UNIQUE(Anio,
    Mes)` (`numero % 12` repetía un mes ya usado). Resuelto con un helper nuevo,
    `AutenticarSoloAsync`, que crea solo una `Empresa` + token sin pasar por
    `EscenarioFactura.CrearAsync` (evita sembrar un `Periodo` innecesario para esas dos pruebas).
  - **Archivos modificados:** `docs/01-reglas-negocio.md`, `docs/06-seguridad.md`,
    `docs/07-decisiones-arquitectura.md` (secciones 69-73 renumeradas por la nueva ADR-066),
    `docs/2026-09-17-tareas.md`, `docs/decisiones-pendientes.md`, `progreso.md`,
    `backend/src/Api/Controllers/PedidosProveedorController.cs`,
    `backend/src/Api/Controllers/EntregasController.cs`,
    `backend/src/Api/Controllers/FacturasController.cs`,
    `backend/src/Api/Common/IdentidadOpcional.cs`,
    `backend/tests/Api.Tests/AutorizacionPedidosEntregasFacturasFlujoTests.cs`,
    `backend/tests/Api.Tests/PedidosProveedorFlujoTests.cs`,
    `backend/tests/Api.Tests/EntregasFlujoTests.cs`,
    `backend/tests/Api.Tests/FacturasFlujoTests.cs`,
    `backend/tests/Api.Tests/EntregaAtomicidadTests.cs`,
    `backend/tests/Api.Tests/FlujoIntegradoA2B1B4Tests.cs`,
    `backend/tests/Api.Tests/EscenarioPedidoProveedor.cs`,
    `backend/tests/Api.Tests/EscenarioFactura.cs`.
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Migración:** ninguna (sin cambios de Domain/Infrastructure — `[Authorize]` es una
    preocupación de Api, no de persistencia).
  - **Pruebas:** `dotnet test` (desde `backend/`) → **629/629 correctas, 0 omitidas** (234
    `Domain.Tests` + 251 `Application.Tests` + 144 `Api.Tests`, incluyendo las 6 de
    `AutorizacionPedidosEntregasFacturasFlujoTests` ahora activas).
  - **Decisiones de diseño reversibles:** un único token con varios permisos por archivo de
    prueba de flujo (no un token distinto por acción) — mismo criterio ya usado en
    `RequisicionesFlujoTests`; se puede revisar si en el futuro se necesita probar autorización
    granular dentro de esos mismos archivos (hoy vive aparte, en
    `AutorizacionPedidosEntregasFacturasFlujoTests`).
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** sembrar los 5 permisos nuevos en la base de datos de
    desarrollo y asignarlos a `COMPRAS`/`RECEPCION`/`ADMINISTRADOR` (pendiente operativo, no de
    código — ver `docs/decisiones-pendientes.md`); sin eso, el Frontend recibirá `403` al intentar
    Enviar/Cerrar/Cancelar un Pedido o Anular una Entrega/Factura, aunque el usuario esté
    autenticado. P1-5 (`FallbackPolicy` global), P2-1/P2-2/P2-5, P3-1/P3-2 siguen en el backlog
    (`docs/2026-09-17-tareas.md`).

- **2026-09-17 10:08 (seguir el orden de `docs/2026-09-17-tareas.md §6`: seeding, P2-1, P3-1/P3-2, P2-2)** —
  Bloque solicitado explícitamente por el usuario: "continua con el orden ya establecido de las
  tareas de hoy". Alcance: sembrado de permisos en la base de datos de desarrollo (`AuropaqPedidos`,
  vía `sqlcmd` directo); `backend/src/Api/Controllers/ConsolidacionesController.cs` (nuevo);
  `backend/tests/Api.Tests/ConsolidacionesFlujoTests.cs` (nuevo); `backend/tests/Domain.Tests/
  RequisicionFlujoTests.cs` (prueba nueva); Domain/Application/Infrastructure de
  `PedidoProveedor`/`Entrega`/`Factura` (`UsuarioCreacionId`) + migración + ~13 archivos de test
  actualizados. **No** se tocó Requisición/Consolidación (Domain) más allá de lo ya descrito.
  - **Qué se hizo:**
    - **Sembrado de permisos (punto 1):** `PEDIDO_ENVIAR` (Id 51), `PEDIDO_CERRAR` (52),
      `PEDIDO_CANCELAR` (53), `ENTREGA_ANULAR` (54), `FACTURA_ANULAR` (55) insertados en
      `Permisos` y asignados en `RolesPermisos` a `Compras`/`Recepcion`/`Administrador` según
      RN-063, verificado por consulta tras la inserción. Verificación adicional: los 7 permisos
      anteriores de Pedido/Entrega/Factura ya estaban sembrados y correctamente asignados (no
      hacía falta re-sembrarlos).
    - **P2-1 — `ConsolidacionesController`:** expone `POST /api/v1/consolidaciones`
      (`CrearConsolidacionUseCase`, único caso de uso real) con
      `[Authorize(Policy="Permiso:PEDIDO_CONSOLIDAR")]`, sin alcance por empresa (`CLAUDE.md
      §27`). `docs/05-api.md §31` actualizado (ya no lista `GET`/`{id}/generar` como pendientes
      del MVP: siguen sin caso de uso, no son un hueco). 4 pruebas nuevas
      (`ConsolidacionesFlujoTests`: crear+agrupar, 404 periodo inexistente, 401 sin JWT, 403 sin
      permiso).
    - **P3-1/P3-2 — verificación, no implementación:** al ir a codificar las guardas de RN-061
      (ventana) y RN-062 (inmutabilidad de `APROBADA`) se encontró que **ya existían** —
      `Requisicion.Enviar()` ya llama `Periodo.EstaDentroDeVentanaDeSolicitud()` (TASK-029) y
      `Requisicion.EsEditable` ya excluye `APROBADA` (TASK-023..026), ambas desde antes de que
      existiera este análisis. Se agregó una prueba dedicada al caso `APROBADA`
      (`No_permite_agregar_detalle_a_una_requisicion_aprobada`, antes solo cubierto
      genéricamente vía `ENVIADA`). `01-reglas-negocio.md`/`docs/2026-09-17-tareas.md`/
      `docs/decisiones-pendientes.md` corregidos para reflejar que D-14/D-15 están cerradas E
      IMPLEMENTADAS, no solo decididas.
    - **P2-2 — `UsuarioCreacionId`:** agregado a `PedidoProveedor`/`Entrega`/`Factura` (Domain:
      propiedad + parámetro de constructor obligatorio, mismo patrón que
      `Requisicion`/`Consolidacion`; Infrastructure: `EntityTypeBuilder.Property(...).IsRequired()`
      en los 3 `IEntityTypeConfiguration`; Application: `CrearPedidoProveedorUseCase`/
      `CrearEntregaUseCase`/`RegistrarFacturaUseCase.Ejecutar` reciben `usuarioId` obligatorio en
      vez de opcional, y lo pasan al constructor de la entidad además de a `Auditoria`; Api: los
      3 controllers usan `ObtenerUsuarioIdAutenticado()` (mismo patrón que
      `RequisicionesController`/`ConsolidacionesController`) en vez de `IdentidadOpcional`, que
      quedó sin ningún uso y se eliminó (`backend/src/Api/Common/IdentidadOpcional.cs`). Los tres
      `Response`/`Mapper` ahora exponen `UsuarioCreacionId` (mismo criterio que
      `ConsolidacionResponse`).
  - **Migración:** `AgregarUsuarioCreacionIdAPedidoEntregaFactura` — 3 columnas `int NOT NULL
    DEFAULT 0` (`PedidosProveedor`, `Facturas`, `Entregas`). Creada y aplicada a la base de datos
    de desarrollo (`AuropaqPedidos`) sin error; filas existentes (si las había) quedan con `0` en
    la nueva columna — no se hizo backfill retroactivo con datos reales porque no existe forma de
    reconstruir con certeza quién creó cada registro histórico.
  - **Bloqueador encontrado y resuelto:** el cambio de constructor obligatorio rompió ~10 archivos
    de prueba (Domain.Tests, Application.Tests, Api.Tests) que construían `PedidoProveedor`/
    `Entrega`/`Factura` directamente o llamaban `CrearPedidoProveedorUseCase`/
    `CrearEntregaUseCase`/`RegistrarFacturaUseCase.Ejecutar` sin el nuevo parámetro — 100 errores
    de compilación en la primera pasada. Resuelto sistemáticamente: helpers de escenario
    actualizados con `usuarioCreacionId: 10` (mismo valor de convención ya usado por
    `Requisicion`/`Consolidacion` en esos mismos archivos), llamadas a `.Ejecutar(...)` corregidas
    con `usuarioId: 10` vía sustitución dirigida por archivo (no manual una por una); 2 pruebas
    con aserciones `Assert.Null(registro.UsuarioId)` que ya no aplicaban se corrigieron a
    `Assert.Equal(10, ...)`.
  - **Archivos creados:** `backend/src/Api/Controllers/ConsolidacionesController.cs`,
    `backend/tests/Api.Tests/ConsolidacionesFlujoTests.cs`,
    `backend/src/Infrastructure/Persistence/Migrations/20260917150543_AgregarUsuarioCreacionIdAPedidoEntregaFactura.cs`
    (+ `.Designer.cs`).
  - **Archivos modificados:** `docs/01-reglas-negocio.md`, `docs/05-api.md`, `docs/08-tareas.md`,
    `docs/2026-09-17-tareas.md`, `docs/decisiones-pendientes.md`, `progreso.md`;
    `backend/src/Domain/Entities/{PedidoProveedor,Entrega,Factura}.cs`;
    `backend/src/Application/{PedidosProveedor/CrearPedidoProveedorUseCase,
    PedidosProveedor/Dtos/PedidoProveedorResponse,PedidosProveedor/PedidoProveedorMapper,
    Entregas/CrearEntregaUseCase,Entregas/Dtos/EntregaResponse,Entregas/EntregaMapper,
    Facturas/RegistrarFacturaUseCase,Facturas/Dtos/FacturaResponse,Facturas/FacturaMapper}.cs`;
    `backend/src/Infrastructure/Persistence/Configurations/{PedidoProveedorConfiguration,
    EntregaConfiguration,FacturaConfiguration}.cs`;
    `backend/src/Api/Controllers/{PedidosProveedorController,FacturasController}.cs`;
    `backend/tests/Domain.Tests/{RequisicionFlujoTests,EntregaTests,FacturaTests,
    PedidoProveedorTests}.cs`; `backend/tests/Application.Tests/{CrearPedidoProveedorUseCaseTests,
    CrearEntregaUseCaseTests,RegistrarFacturaUseCaseTests,AgregarDetalleFacturaUseCaseTests,
    AnularFacturaUseCaseTests,AnularEntregaUseCaseTests,CerrarPedidoProveedorUseCaseTests}.cs`;
    `backend/tests/Api.Tests/{EscenarioFactura,PedidosProveedorFlujoTests}.cs`.
  - **Archivos eliminados:** `backend/src/Api/Common/IdentidadOpcional.cs` (sin uso tras pasar
    `usuarioId` de opcional a obligatorio en los 3 controllers que lo usaban).
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Pruebas:** `dotnet test` (desde `backend/`) → **634/634 correctas, 0 omitidas** (235
    `Domain.Tests` + 251 `Application.Tests` + 148 `Api.Tests`).
  - **Ambigüedades / contradicciones encontradas en docs:** ninguna nueva — las correcciones de
    P3-1/P3-2 (guardas ya existentes) y el hallazgo sobre permisos ya sembrados están descritas
    arriba, ambas a favor (menos trabajo del esperado), no contradicciones que bloqueen nada.
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** P1-5 (`FallbackPolicy` global) y P2-5 (limpieza de datos de
    prueba) siguen en el backlog (`docs/2026-09-17-tareas.md §2`/§6). TASK-008 pasa a
    `COMPLETADA`; TASK-039/046 (y posiblemente otras de Fase 7-9) podrían estar más cerca de
    `COMPLETADA` de lo que sus filas de FASE indican — no se hizo una revisión formal de cierre
    de esas tareas en esta sesión, queda como trabajo futuro explícito.

- **2026-09-17 10:22 (P1-5 + P2-5: cierre del backlog de `docs/2026-09-17-tareas.md`)** — Bloque
  solicitado explícitamente por el usuario: "continua con el orden que se lleva" (los últimos dos
  puntos pendientes de `docs/2026-09-17-tareas.md §6`). Alcance: `backend/src/Api/Program.cs`,
  `backend/src/Api/Controllers/AuthController.cs`; consulta (sin escritura) de
  `Productos`/`SolicitudesProductoCatalogo` en la base de datos de desarrollo. **No** se tocó
  Domain/Application/Infrastructure ni el Frontend.
  - **Qué se hizo:**
    - **P1-5 — `FallbackPolicy` global:** `AddAuthorization` en `Program.cs` ahora define
      `options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()`
      — cualquier endpoint futuro sin `[Authorize]`/`[AllowAnonymous]` explícito queda protegido
      por defecto (`CLAUDE.md §67`, seguridad por defecto), en vez de repetir el hueco que tuvieron
      Pedido/Entrega/Factura hasta hoy (P1). Único endpoint público real detectado,
      `AuthController.Login`, marcado con `[AllowAnonymous]` explícito (antes era público solo
      por omisión, sin decirlo).
    - **P2-5 — limpieza de datos de prueba:** inspeccionada (solo `SELECT`, sin escritura) la
      base de datos de desarrollo. Se encontraron 4 `Productos` (3 de catálogo base + 1 creado vía
      solicitud) y 4 `SolicitudesProductoCatalogo`, todas del usuario de prueba
      `carlos.taskdemo@auropaq.com` (Id 24) — más datos de los que el diagnóstico original
      mencionaba (decía "3 solicitudes + 1 producto"). Presentadas las opciones al usuario (no
      borrar / borrar solo lo descartable / borrar todo): **decidió no borrar nada** — son datos
      de la sesión de pruebas manuales del Frontend del 2026-09-16, no basura accidental. Cerrado
      como "evaluado, sin acción" por decisión explícita del usuario.
  - **Archivos modificados:** `backend/src/Api/Program.cs`,
    `backend/src/Api/Controllers/AuthController.cs`, `docs/2026-09-17-tareas.md`, `progreso.md`.
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Migración:** ninguna.
  - **Pruebas:** `dotnet test` (desde `backend/`) → **634/634 correctas, 0 omitidas** — sin
    regresión: confirma que ningún endpoint dependía de quedar público por omisión salvo Login,
    ya cubierto con `[AllowAnonymous]`.
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** los 6 puntos de `docs/2026-09-17-tareas.md §6` quedan cerrados.
    Backlog restante del día: ninguno explícito — próxima sesión puede retomar Fase 6-9 desde el
    Frontend (Consolidación/Pedido/Entrega ya alcanzables por HTTP real, P2-1) o cualquier otra
    prioridad que el usuario indique.

- **2026-09-17 10:51 (Incremento "Fase 6-9 en Frontend" — ambigüedades resueltas + Prioridad 1)** —
  Bloque solicitado explícitamente por el usuario: "quiero que hagas una lista de tareas para el
  siguiente incremento" (con nombre + fecha/hora del incremento) y, en el siguiente turno, "quiero
  que resolvamos las dos ambigüedades y empecemos con la Prioridad 1". Alcance: nuevo documento
  `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`; `01-reglas-negocio.md`/
  `07-decisiones-arquitectura.md` (2 decisiones nuevas); `06-seguridad.md`/`05-api.md` (mapeo de
  los 5 `GET` nuevos); Application/Infrastructure/Api de Consolidación/Pedido/Entrega/Factura (solo
  lectura — ningún caso de uso de escritura tocado); 6 archivos de test de `Api.Tests`. **No** se
  tocó el Frontend ni la regla de distribución de Pedido (I2-1, decidida pero no implementada).
  - **Qué se hizo:**
    - Documento `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`: backlog propuesto para el
      próximo incremento (Prioridades 1-4), con 2 ambigüedades marcadas para decisión antes de
      implementar.
    - **Ambigüedad 1 (permiso de lectura de Consolidación):** resuelta con recomendación propia
      (reutilizar `PEDIDO_VER`, sin crear `CONSOLIDACION_VER`) — formalizada en `RN-064`/`ADR-067`.
    - **Ambigüedad 2 (regla de distribución de Pedido, TASK-041):** sin recomendación propia clara
      (dependía de cómo opera Compras, no documentado) — se preguntó explícitamente al usuario, que
      confirmó exigir distribución completa antes de enviar (mismo patrón que Requisición).
      Formalizada en `RN-065`/`ADR-068`. **Decisión cerrada, implementación (I2-1) todavía
      pendiente** — no se tocó `PedidoProveedor.Enviar()` en este bloque.
    - **Prioridad 1 completa (I1-1 a I1-5):** 5 endpoints `GET` nuevos —
      `GET /consolidaciones` (+ `periodoId` opcional) y `GET /consolidaciones/{id}`
      (`ListarConsolidacionesUseCase`/`ObtenerConsolidacionUseCase`, nuevo método
      `IConsolidacionRepository.Listar`); `GET /pedidos-proveedor` (+ `consolidacionId` opcional) y
      `GET /pedidos-proveedor/{id}` (`ListarPedidosProveedorUseCase`/`ObtenerPedidoProveedorUseCase`,
      nuevo método `IPedidoProveedorRepository.Listar`); `GET /pedidos-proveedor/{id}/entregas`
      (ruta anidada, mismo criterio que la creación) y `GET /entregas/{id}`
      (`ListarEntregasUseCase`/`ObtenerEntregaUseCase`, reutilizando
      `IEntregaRepository.ObtenerPorPedido` ya existente); `GET /facturas?pedidoProveedorId=` y
      `GET /facturas/{id}` (`ListarFacturasUseCase`/`ObtenerFacturaUseCase`, reutilizando
      `IFacturaRepository.ObtenerPorPedido` ya existente). Todos usan permisos `*_VER` ya
      catalogados y sembrados — ninguno nuevo salvo la decisión de la Ambigüedad 1.
  - **Archivos creados:** `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`;
    `backend/src/Application/Consolidaciones/{ListarConsolidacionesUseCase,
    ObtenerConsolidacionUseCase}.cs`; `backend/src/Application/PedidosProveedor/
    {ListarPedidosProveedorUseCase,ObtenerPedidoProveedorUseCase}.cs`;
    `backend/src/Application/Entregas/{ListarEntregasUseCase,ObtenerEntregaUseCase}.cs`;
    `backend/src/Application/Facturas/{ListarFacturasUseCase,ObtenerFacturaUseCase}.cs`.
  - **Archivos modificados:** `docs/01-reglas-negocio.md`, `docs/06-seguridad.md`,
    `docs/05-api.md`, `docs/07-decisiones-arquitectura.md` (secciones 70-75 renumeradas por las
    2 ADR nuevas), `progreso.md`;
    `backend/src/Application/Consolidaciones/Abstracciones/IConsolidacionRepository.cs`;
    `backend/src/Application/PedidosProveedor/Abstracciones/IPedidoProveedorRepository.cs`;
    `backend/src/Application/DependencyInjection.cs`;
    `backend/src/Infrastructure/Repositories/{ConsolidacionRepositoryEfCore,
    PedidoProveedorRepositoryEfCore}.cs`;
    `backend/src/Api/Controllers/{ConsolidacionesController,PedidosProveedorController,
    EntregasController,FacturasController}.cs`;
    `backend/tests/Application.Tests/Fakes/{FakeConsolidacionRepository,
    FakePedidoProveedorRepository}.cs`;
    `backend/tests/Api.Tests/{ConsolidacionesFlujoTests,PedidosProveedorFlujoTests,
    EntregasFlujoTests,FacturasFlujoTests}.cs` (9 pruebas nuevas).
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Migración:** ninguna (los 5 `GET` nuevos son solo lectura, sin cambios de esquema).
  - **Pruebas:** `dotnet test` (desde `backend/`) → **643/643 correctas, 0 omitidas** (235
    `Domain.Tests` + 251 `Application.Tests` + 157 `Api.Tests`, +9 respecto al cierre anterior).
  - **Decisiones de diseño reversibles:** `GET /entregas` y `GET /facturas` NO tienen una ruta
    "listar todo sin filtro" — Entrega exige el pedido dueño (ruta anidada) y Factura exige
    `pedidoProveedorId` por query, reutilizando los métodos de repositorio `ObtenerPorPedido` que
    ya existían para otro propósito (cálculo de cantidades acumuladas) en vez de crear una
    consulta nueva sin filtro. Si el Frontend necesita "todas las facturas de un proveedor" más
    adelante, es una extensión aparte, no asumida aquí.
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md` sigue
    vigente — no se tocó en este bloque, ninguna Pendiente nueva).
  - **Continúa / siguiente paso:** I2-1 (guarda de distribución completa en
    `PedidoProveedor.Enviar()`, RN-065 ya decidida) y Prioridad 3 (pantallas de Frontend, depende
    de la Prioridad 1 ya completa) — pendientes de que el usuario indique cuál sigue.

- **2026-09-17 11:10 (I2-1: distribución completa exigida antes de enviar un Pedido)** — Bloque
  solicitado explícitamente por el usuario: "si, quiero que continuemos con I2-1". Alcance:
  `backend/src/Domain/Entities/{DetallePedidoProveedor,PedidoProveedor}.cs`;
  `backend/src/Application/PedidosProveedor/AgregarDistribucionPedidoUseCase.cs` (comentario);
  8 archivos de test (Domain.Tests + Application.Tests + Api.Tests). **No** se tocó Infrastructure
  ni ningún controller — es una regla puramente de Domain, sin cambio de contrato HTTP.
  - **Qué se hizo:**
    - `DetallePedidoProveedor.DistribucionCompleta` (nueva propiedad, `CantidadDistribuida ==
      CantidadPedida`) — mismo criterio que `DetalleRequisicion.DistribucionCompleta` (RN-011).
    - `PedidoProveedor.Enviar()` ahora exige que ningún detalle tenga distribución incompleta
      (`_detalles.Any(d => !d.DistribucionCompleta)` → `ReglaDeNegocioException`), igual que
      `Requisicion.Enviar()`. No se agregó una regla "al menos un detalle" (esa es una decisión
      distinta que nadie pidió — un pedido sin detalles sigue pudiendo enviarse, vacuamente sin
      violar la regla nueva).
    - 2 pruebas nuevas dedicadas (`No_permite_enviar_un_pedido_con_distribucion_incompleta`/
      `Permite_enviar_un_pedido_con_distribucion_completa`, Domain.Tests).
  - **Bloqueador encontrado y resuelto:** la nueva guarda rompió 21 pruebas existentes (19
    Domain.Tests + 6 Application.Tests + 8 Api.Tests, con solape) cuyos fixtures creaban un
    pedido, le agregaban un detalle y lo enviaban sin distribuir — mismo patrón que ya había
    ocurrido históricamente al cerrar RN-011 para Requisición. Resuelto agregando la distribución
    completa (mismo `cantidadPedida`, reutilizando la Sede ya disponible en cada fixture) antes
    de cada llamada a `Enviar()`, en 8 archivos.
  - **Archivos modificados:** `docs/01-reglas-negocio.md`, `docs/08-tareas.md`,
    `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`, `progreso.md`;
    `backend/src/Domain/Entities/{DetallePedidoProveedor,PedidoProveedor}.cs`;
    `backend/src/Application/PedidosProveedor/AgregarDistribucionPedidoUseCase.cs` (solo
    comentario, sin cambio de comportamiento);
    `backend/tests/Domain.Tests/{EntregaTests,PedidoProveedorTests}.cs`;
    `backend/tests/Application.Tests/{CrearEntregaUseCaseTests,AnularEntregaUseCaseTests,
    CerrarPedidoProveedorUseCaseTests}.cs` (2 con `FakeSedeRepository` agregado al `Escenario`);
    `backend/tests/Api.Tests/{EntregasFlujoTests,PedidosProveedorFlujoTests,
    EntregaAtomicidadTests,FlujoIntegradoA2B1B4Tests}.cs`.
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Migración:** ninguna (sin cambio de esquema — `DistribucionCompleta` es una propiedad
    calculada, no persistida).
  - **Pruebas:** `dotnet test` (desde `backend/`) → **645/645 correctas, 0 omitidas** (237
    `Domain.Tests` + 251 `Application.Tests` + 157 `Api.Tests`).
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** con esto se cierra por completo el backlog del incremento
    "Fase 6-9 en Frontend" a nivel de Backend (Prioridades 1 y 2). Queda la Prioridad 3
    (pantallas de Frontend: `ConsolidacionPage`, `PedidosProveedorPage`, `EntregasPage`,
    reescribir `FacturacionPage`) y la Prioridad 4 (pruebas end-to-end manuales + cierre
    documental), pendientes de que el usuario indique cuál sigue.

- **2026-09-17 (I3-1 a I3-5: pantallas de Frontend para Consolidación → Pedido → Entrega →
  Factura)** — Bloque solicitado explícitamente por el usuario: "si queiro que continues con la
  prioridad 3 (Frontend)". Alcance: solo `frontend/src/` — ningún archivo de `backend/` se tocó
  en este bloque.
  - **Qué se hizo:**
    - `types/{consolidacion,pedidoProveedor,entrega}.ts` (nuevos) y `types/factura.ts`
      (ajustado): interfaces alineadas exactamente con los DTOs reales del backend. Al reescribir
      `factura.ts` se encontró y corrigió un campo `estado` inexistente en
      `RegistrarFacturaRequest` que arrastraba la pantalla provisional anterior — el backend
      nunca lo aceptó.
    - `services/{consolidacionesService,pedidosProveedorService,entregasService}.ts` (nuevos) y
      `services/facturasService.ts` (ampliado con `listarPorPedido`/`obtener`/`anular`).
    - `components/SelectorSedeGlobal.tsx` (nuevo): selector cascada Empresa→Sede. Necesario
      porque ninguna de las cuatro entidades tiene alcance por empresa (`CLAUDE.md §27`) y el
      backend no expone (ni se pidió que expusiera) un endpoint "listar todas las sedes" — se
      resolvió combinando los catálogos de Empresa y Sede que sí existen, sin inventar un
      endpoint nuevo.
    - `components/pedidosProveedor/PedidoDetalleRow.tsx` y
      `components/entregas/EntregaDetalleRow.tsx` (nuevos): fila de detalle con distribuciones
      existentes + formulario para agregar una nueva vía `SelectorSedeGlobal`.
    - `pages/ConsolidacionPage.tsx`, `pages/PedidosProveedorPage.tsx`, `pages/EntregasPage.tsx`
      (nuevas) y `pages/FacturacionPage.tsx` (reescrita — reemplaza la pantalla provisional de
      IDs a mano por selección real ahora que I1-2/I1-4 existen; `pages/FacturacionPage.css`
      eliminado por quedar sin uso).
    - `routes/AppRoutes.tsx` y `components/AppLayout.tsx`: rutas y enlaces de navegación para
      `/consolidacion`, `/pedidos-proveedor`, `/entregas`.
  - **Bugs encontrados y corregidos (no relacionados con reglas de negocio):**
    - Texto blanco sobre fondo claro en dos lugares: las celdas Producto/Cantidad necesaria de
      `ConsolidacionPage` y, tras inspección más amplia motivada por el primer hallazgo, el texto
      de los botones de lista seleccionados (fondo `bg-blue-50`) en las cuatro pantallas nuevas
      (`ConsolidacionPage`, `PedidosProveedorPage`, `EntregasPage`, `FacturacionPage`). Causa: a
      esos elementos les faltaba la clase Tailwind `text-slate-900` que el resto de la aplicación
      aplica de forma consistente a todo texto de datos — sin ella, el color se hereda del
      `color-scheme: light dark` del `:root` (`index.css`), invisible en navegador con tema
      oscuro del sistema operativo. Diagnosticado con `javascript_tool`
      (`getComputedStyle(td).color` → `rgb(255,255,255)`) comparando contra `get_page_text` (el
      dato sí estaba en el DOM). Corregido agregando la clase explícita en los 4 archivos;
      verificado que `components/pedidosProveedor/PedidoDetalleRow.tsx` y
      `components/entregas/EntregaDetalleRow.tsx` ya la tenían desde el principio.
    - Un `datos={detallesConsolicionSeguro(detallesConsolicionSeguro)}` accidental (función
      pasada a sí misma, con una función identidad placeholder de sobra) en
      `PedidosProveedorPage.tsx`, detectado en revisión propia antes de probar en navegador —
      corregido a `datos={detallesConsolidacionDisponibles}` y eliminada la función sin uso.
  - **Verificación manual end-to-end en navegador** (`localhost:5173` contra `localhost:5175`
    real, sesión `admin.frontend@auropaq.dev`): Requisición #21 (aprobada, periodo 20/2026-10) →
    Consolidación #82 (`POST /consolidaciones`) → Pedido PP-TEST-001 #85 (detalle Papel higienico
    10×5000, distribución completa a Sede Bogotá Norte vía `SelectorSedeGlobal`, Enviar — primera
    vez que la guarda RN-065 se ejerce desde la UI real, sin bloquear) → Entrega REM-TEST-001 #89
    (detalle 6 unidades, distribución parcial a la misma sede, con fotografía histórica
    `ciudadEntrega` visible — confirma RN-035/ADR-019 — y el Pedido pasó automáticamente a
    `ParcialmenteEntregado`) → Factura FAC-TEST-001 #93 (línea 10×5000, subtotal 50000 + impuestos
    1900 = total 51900 calculados por el backend, luego Anulada). Sin errores de consola en
    ningún punto del flujo.
  - **Archivos creados:** `frontend/src/types/{consolidacion,pedidoProveedor,entrega}.ts`;
    `frontend/src/services/{consolidacionesService,pedidosProveedorService,entregasService}.ts`;
    `frontend/src/components/SelectorSedeGlobal.tsx`;
    `frontend/src/components/pedidosProveedor/PedidoDetalleRow.tsx`;
    `frontend/src/components/entregas/EntregaDetalleRow.tsx`;
    `frontend/src/pages/{ConsolidacionPage,PedidosProveedorPage,EntregasPage}.tsx`.
  - **Archivos modificados:** `frontend/src/types/factura.ts`;
    `frontend/src/services/facturasService.ts`; `frontend/src/pages/FacturacionPage.tsx`
    (reescrita); `frontend/src/routes/AppRoutes.tsx`; `frontend/src/components/AppLayout.tsx`;
    `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md`; `progreso.md`.
  - **Archivos eliminados:** `frontend/src/pages/FacturacionPage.css`.
  - **Compilación:** ningún cambio en `backend/` — no se repitió `dotnet test` en este bloque
    (sin cambios de Domain/Application/Infrastructure/Api). Frontend verificado por uso real en
    navegador, no por suite automatizada (el proyecto todavía no tiene pruebas de Frontend,
    `CLAUDE.md §45` — fuera de alcance de este bloque).
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** con esto se cierra la Prioridad 3 completa. Queda la
    Prioridad 4 (I4-1 ya cubierta por las pruebas de integración de la Prioridad 1; I4-2 ya
    ejecutada en este mismo bloque; I4-3 sincronización final de documentación), pendiente de que
    el usuario indique si continuar.

- **2026-09-17 (Prioridad 4: cierre del incremento "Fase 6-9 en Frontend" — I4-1 a I4-3)** —
  Bloque solicitado explícitamente por el usuario: "si quiero que la cierre". Alcance: solo
  `backend/tests/Api.Tests/` — ningún archivo de producción se tocó.
  - **Corrección sobre el cierre anterior:** la entrada previa daba I4-1 por "ya cubierta por las
    pruebas de integración de la Prioridad 1" — al revisar el código de las pruebas antes de
    darlo por cerrado (`CLAUDE.md §45`: "una tarea no está terminada simplemente porque
    compile"), eso resultó ser **parcialmente inexacto**: la Prioridad 1 sí agregó 200 (camino
    feliz) y 404 (recurso inexistente) para cada `GET` nuevo, pero **ningún** `GET` nuevo tenía
    prueba de 401 (sin JWT) ni 403 (JWT válido sin el permiso `*_VER`) — esa cobertura solo
    existía para las acciones POST de estos controllers
    (`AutorizacionPedidosEntregasFacturasFlujoTests.cs`) y para el POST de Consolidación
    (`ConsolidacionesFlujoTests.cs`). El incremento explícitamente prometía "200, 404, 401, 403"
    para I4-1 — se cierra la brecha real en vez de dar por bueno lo que decía la nota anterior.
  - **Qué se hizo:**
    - `ConsolidacionesFlujoTests.cs`: +3 pruebas — `Listar_consolidaciones_sin_jwt_devuelve_401`,
      `Obtener_consolidacion_sin_jwt_devuelve_401`,
      `Listar_y_obtener_consolidacion_con_jwt_valido_sin_permiso_devuelven_403` (esta última cubre
      ambas rutas en un solo `[Fact]`, mismo criterio de minimalismo que el resto del archivo).
    - `AutorizacionPedidosEntregasFacturasFlujoTests.cs`: +9 pruebas — 6 nuevas `*_sin_jwt_
      debe_devolver_401` (una por cada `GET` de Pedido/Entrega/Factura, incluida la ruta anidada
      `GET /pedidos-proveedor/{id}/entregas`) más 3 `*_con_jwt_valido_sin_<PERMISO>_devuelven_403`
      (una por controller, agrupando `Listar`+`Obtener` cuando comparten la misma política). Los
      401 no necesitaron crear ningún escenario/usuario — `[Authorize]` rechaza antes de llegar
      al caso de uso, igual que los `*_sin_jwt` ya existentes en ese archivo para las acciones
      POST; los 403 reutilizan `Escenario.CrearAsync` (solo para obtener un `EmpresaId` válido) +
      `AutorizacionHelper.CrearTokenConPermisosAsync` sin el permiso `*_VER` correspondiente,
      mismo patrón ya usado en `ConsolidacionesFlujoTests`.
    - Verificado antes de escribir las pruebas que cada acción `GET` nueva sí tiene su propio
      `[Authorize(Policy = "Permiso:...")]` explícito (`ConsolidacionesController`,
      `PedidosProveedorController`, `EntregasController`, `FacturasController`) — las pruebas
      confirman una protección que ya existía en el código, no agregan autorización nueva.
  - **Archivos modificados:** `backend/tests/Api.Tests/ConsolidacionesFlujoTests.cs`,
    `backend/tests/Api.Tests/AutorizacionPedidosEntregasFacturasFlujoTests.cs`;
    `docs/incremento-fase-6-9-frontend-2026-09-17-1028.md` (I4-1 a I4-3 marcadas hechas, incremento
    cerrado); `docs/08-tareas.md` (notas de sincronización en TASK-036/039/042/046); `progreso.md`.
  - **Compilación:** `dotnet build` (desde `backend/`) → correcta, 0 advertencias, 0 errores.
  - **Migración:** ninguna (solo pruebas, sin cambios de esquema ni de Domain/Application/
    Infrastructure/Api).
  - **Pruebas:** `dotnet test` (desde `backend/`) → **657/657 correctas, 0 omitidas** (237
    `Domain.Tests` + 251 `Application.Tests` + 169 `Api.Tests`, +12 respecto al cierre de la
    Prioridad 2/inicio de la Prioridad 3).
  - **Decisiones pendientes de negocio:** ninguna (0, `docs/decisiones-pendientes.md`).
  - **Continúa / siguiente paso:** con esto se cierra por completo el incremento "Fase 6-9 en
    Frontend" (Prioridades 1 a 4). Sin tareas abiertas de este incremento — pendiente de que el
    usuario indique el siguiente foco de trabajo.
