# 06 — SEGURIDAD

## 1. Objetivo

Este documento define los principios, reglas y decisiones de seguridad de la aplicación de gestión de requisiciones y pedidos de Auropaq S.A.S.

La seguridad debe proteger:

- identidad de los usuarios;
- información de las empresas;
- información de las sedes;
- requisiciones;
- productos;
- pedidos;
- entregas;
- facturas;
- información histórica;
- operaciones críticas;
- trazabilidad del sistema.

La seguridad debe estar implementada principalmente en el backend.

El frontend nunca debe considerarse una barrera de seguridad.

---

# 2. Principio fundamental

La seguridad del sistema se divide en tres conceptos:

```text
AUTENTICACIÓN
    ↓
¿Quién eres?

AUTORIZACIÓN
    ↓
¿Qué puedes hacer?

ALCANCE
    ↓
¿Sobre qué información puedes hacerlo?
```

Ejemplo:

```text
Usuario: Juan

Autenticación:
Juan está autenticado.

Autorización:
Juan tiene permiso para crear requisiciones.

Alcance:
Juan puede crear requisiciones para su empresa
y sobre las sedes que tiene asignadas.
```

Tener permiso para realizar una acción no significa automáticamente tener acceso a todos los datos del sistema.

---

# 3. Principios de seguridad

La aplicación seguirá estos principios:

1. Mínimo privilegio.
2. Denegación por defecto.
3. Validación en backend.
4. Separación entre autenticación y autorización.
5. Separación entre permisos y alcance.
6. No confiar en datos enviados por el frontend.
7. Protección de información sensible.
8. Trazabilidad de operaciones importantes.
9. Validación de cada operación crítica.
10. No exponer información técnica innecesaria.
11. No hardcodear usuarios, empresas ni roles.
12. Seguridad desde el diseño.

---

# 4. Autenticación

La autenticación determina la identidad del usuario.

Conceptualmente:

```text
Usuario
   ↓
Inicio de sesión
   ↓
Proveedor de identidad
   ↓
Identidad autenticada
   ↓
API
```

**Implementado (autenticación "TASK-015", 2026-09-14 — ver RN-056/ADR-057).** Mecanismo elegido explícitamente por el usuario: JWT propio, emitido y validado por esta misma API (no proveedor externo/delegado, aunque `§31` seguía recomendando evaluarlo primero — se auditó y se decidió explícitamente no delegarlo). Esta decisión no alteró ninguna regla de autorización del dominio (autorización sigue sin implementarse, `TASK-049`/`050`).

---

# 5. Identidad del usuario

Una vez autenticado, el backend debe poder identificar al usuario.

Como mínimo:

```text
UsuarioId
```

Y cuando corresponda:

```text
EmpresaId
Roles
Permisos
Sedes autorizadas
```

La API debe obtener esta información desde la identidad autenticada y/o desde la base de datos.

No debe confiar en valores enviados por React para determinar la identidad real.

---

# 6. Regla contra suplantación

El frontend no debe poder decidir quién está ejecutando una operación.

Ejemplo incorrecto:

```json
{
  "usuarioId": 25,
  "empresaId": 4
}
```

y que el backend simplemente confíe en esos valores.

Un usuario malicioso podría modificar:

```text
usuarioId
empresaId
sedeId
rol
```

para intentar acceder a información ajena.

La API debe determinar estos valores mediante la identidad autenticada y las reglas de autorización.

---

# 7. Autorización

La autorización determina si un usuario puede realizar una operación.

La autorización debe considerar:

```text
Identidad
+
Permisos
+
Alcance
+
Estado del recurso
+
Reglas de negocio
```

Ejemplo:

```text
Usuario
  ↓
¿Está autenticado?
  ↓
¿Tiene permiso?
  ↓
¿Tiene alcance sobre la empresa?
  ↓
¿Tiene alcance sobre la sede?
  ↓
¿El estado permite la operación?
  ↓
PERMITIR
```

---

# 8. Roles

Un rol representa una responsabilidad funcional.

Ejemplos conceptuales:

```text
SOLICITANTE
GESTOR_REQUISICIONES
COMPRAS
RECEPCION
ADMINISTRADOR
```

Estos nombres son ejemplos.

No deben considerarse definitivos hasta validar las responsabilidades reales del negocio.

---

# 9. Permisos

Los permisos representan acciones específicas.

Ejemplos:

```text
REQUISICION_CREAR
REQUISICION_VER
REQUISICION_MODIFICAR
REQUISICION_ENVIAR
REQUISICION_APROBAR
REQUISICION_DEVOLVER

PRODUCTO_VER
PRODUCTO_CREAR
PRODUCTO_EDITAR

PEDIDO_VER
PEDIDO_CREAR
PEDIDO_CONSOLIDAR

ENTREGA_VER
ENTREGA_REGISTRAR

FACTURA_VER
FACTURA_REGISTRAR
```

Los permisos deben ser independientes de los nombres de personas.

---

# 10. Rol → Permiso

La autorización se recomienda modelar así:

```text
Usuario
   ↓
Rol
   ↓
Permiso
```

Ejemplo:

```text
SOLICITANTE
    ↓
REQUISICION_CREAR
REQUISICION_VER
REQUISICION_MODIFICAR
REQUISICION_ENVIAR
```

Otro rol podría tener:

```text
GESTOR_REQUISICIONES
    ↓
REQUISICION_VER
REQUISICION_APROBAR
REQUISICION_DEVOLVER
PRODUCTO_CREAR
PRODUCTO_EDITAR
```

No se debe implementar la seguridad únicamente mediante:

```text
if rol == "SOLICITANTE"
```

La lógica debe basarse en permisos y políticas.

---

# 11. Alcance por empresa

El sistema debe poder restringir la información por empresa.

Ejemplo:

```text
Usuario A
Empresa: AUROTECH
```

No debe poder consultar automáticamente:

```text
FAVIPAQ
COURIERBOX
COURIERBOX ÁLAMOS
```

salvo que tenga un permiso y alcance que lo permitan.

**Implementado parcialmente ("TASK-050", 2026-09-14 — ver RN-058/ADR-059).** `POST
/requisiciones/{id}/enviar` y `POST /requisiciones/{id}/aprobar` verifican
`Usuario.Empresa.Id == Requisicion.Empresa.Id` dinámicamente en cada request (sin cache, mismo
criterio de RN-057). El resto de los endpoints del sistema no tiene alcance por empresa todavía.

---

# 12. Usuarios con múltiples empresas

El diseño debe permitir que determinados usuarios tengan responsabilidades sobre más de una empresa cuando el negocio lo requiera.

Ejemplo conceptual:

```text
Usuario gestor
     │
     ├── AUROTECH
     ├── FAVIPAQ
     ├── COURIERBOX
     └── COURIERBOX ÁLAMOS
```

Esto es importante porque el sistema debe representar el rol de un gestor central sin hardcodear que una persona específica sea "Martha".

La persona puede cambiar.

La responsabilidad permanece.

---

# 13. Alcance por sede

El sistema también debe poder restringir el acceso a sedes.

Ejemplo:

```text
Usuario
   ↓
Empresa A
   ↓
Sede Bogotá
```

Ese usuario no debe poder modificar automáticamente:

```text
Sede Medellín
```

si no tiene autorización sobre ella.

**NO implementado — decisión explícita del usuario ("TASK-050", 2026-09-14, ver RN-058), no una
omisión.** `Requisicion` no tiene un único `SedeId` propio — puede distribuirse entre varias
sedes vía `DistribucionRequisicion` (`CLAUDE.md §19`) — lo que planteaba una ambigüedad real
sobre qué exigir cuando una requisición toca sedes a las que el usuario no está asignado (¿todas?
¿alguna?). Presentadas las alternativas, se decidió posponer esta dimensión y mantener
únicamente alcance por Empresa (`§11`). Puede revisitarse si el negocio lo requiere.

---

# 14. Empresa + sede

El backend debe validar siempre que una sede pertenezca a la empresa correspondiente.

Ejemplo:

```text
Empresa A
 ├── Sede Bogotá
 └── Sede Medellín

Empresa B
 └── Sede Cali
```

Una solicitud como:

```text
Empresa A
Sede Cali
```

debe rechazarse porque la sede no pertenece a la empresa.

Esto evita ataques de acceso cruzado mediante manipulación de IDs.

---

# 15. Autorización por recurso

No basta con comprobar:

```text
Usuario tiene permiso REQUISICION_MODIFICAR
```

También se debe comprobar:

```text
¿Esta requisición pertenece al alcance del usuario?
```

Ejemplo:

```text
Permiso:
REQUISICION_MODIFICAR

Requisición:
Empresa A

Usuario:
Empresa B

Resultado:
DENEGADO
```

---

# 16. Autorización por estado

Una operación puede estar autorizada por permisos pero prohibida por el estado del recurso.

Ejemplo:

```text
Usuario:
Tiene REQUISICION_MODIFICAR

Requisición:
APROBADA
```

Resultado:

```text
NO PERMITIR MODIFICACIÓN
```

Porque el estado de la requisición ya no permite esa operación.

La autorización completa debe considerar:

```text
Permiso
+
Alcance
+
Estado
+
Regla de negocio
```

---

# 17. Requisiciones

## 17.1 Crear

Un usuario puede crear una requisición cuando:

- está autenticado;
- tiene permiso;
- pertenece a una empresa válida;
- el periodo está habilitado;
- la ventana de solicitud está abierta;
- no existe otra requisición para la empresa y periodo.

---

## 17.2 Modificar

Puede modificar una requisición únicamente cuando su estado lo permita.

Inicialmente:

```text
BORRADOR
DEVUELTA
```

pueden ser estados modificables.

Los estados definitivos deben mantenerse alineados con `01-reglas-negocio.md`.

---

## 17.3 Enviar

Enviar una requisición es una operación protegida.

Debe validar:

```text
Usuario autenticado
+
Permiso
+
Empresa
+
Sedes
+
Periodo
+
Ventana
+
Detalles
+
Cantidades
+
Distribución
```

---

## 17.4 Aprobar

Aprobar requiere un permiso específico.

Ejemplo:

```text
REQUISICION_APROBAR
```

Un solicitante no debe poder aprobar automáticamente su propia requisición salvo que una regla explícita del negocio lo permita.

---

## 17.5 Devolver

Devolver también requiere un permiso específico.

Debe registrarse:

```text
Usuario
Fecha
Motivo
Estado anterior
Estado nuevo
```

---

# 18. Separación de responsabilidades

El sistema debe evitar que una misma responsabilidad implique automáticamente todas las demás.

Ejemplo:

```text
Crear requisición
≠
Aprobar requisición
≠
Crear pedido proveedor
≠
Registrar entrega
≠
Registrar factura
```

La separación definitiva dependerá de los roles reales del negocio.

---

# 19. Principio de mínimo privilegio

Cada usuario debe tener únicamente los permisos necesarios para realizar sus funciones.

Ejemplo:

Un solicitante no necesita necesariamente:

```text
PEDIDO_CREAR
FACTURA_REGISTRAR
USUARIO_ADMINISTRAR
```

aunque pueda:

```text
REQUISICION_CREAR
REQUISICION_MODIFICAR
REQUISICION_ENVIAR
```

---

# 20. Denegación por defecto

Si una operación no está explícitamente permitida, debe considerarse denegada.

Conceptualmente:

```text
¿Tiene permiso?
    │
    ├── Sí → continuar validación
    │
    └── No → DENEGAR
```

No se debe implementar:

```text
"Si no está prohibido, permitir"
```

Debe utilizarse:

```text
"Si no está permitido, denegar"
```

---

# 21. Seguridad del frontend

React puede ocultar:

```text
Botones
Menús
Pantallas
Acciones
```

según los permisos del usuario.

Pero esto únicamente mejora la experiencia de usuario.

No constituye seguridad.

Ejemplo:

Aunque React oculte:

```text
[Aprobar]
```

un usuario podría intentar manualmente:

```http
POST /api/v1/requisiciones/25/aprobar
```

Por eso el backend debe rechazar la operación si no está autorizado.

---

# 22. Protección contra manipulación de IDs

La API debe protegerse contra situaciones como:

```http
GET /api/v1/requisiciones/100
```

cuando el usuario solo tiene acceso a:

```text
Requisiciones 1–50
```

No basta con comprobar que la requisición 100 exista.

Debe comprobarse que:

```text
Usuario
   ↓
tiene acceso
   ↓
a la requisición 100
```

---

# 23. Enumeración de recursos

El sistema debe evitar facilitar la enumeración de información privada mediante IDs secuenciales.

Ejemplo:

```text
/requisiciones/1
/requisiciones/2
/requisiciones/3
...
```

Si un usuario no tiene acceso al recurso, la API debe responder de forma segura y consistente.

No debe revelar información adicional sobre recursos fuera de su alcance.

---

# 24. Validación de entrada

Todo dato recibido por la API debe considerarse no confiable.

Esto incluye:

```text
Strings
IDs
Cantidades
Fechas
Filtros
Ordenamientos
Parámetros
Archivos
```

El backend debe validar:

- tipo;
- formato;
- longitud;
- rango;
- existencia;
- relación;
- autorización;
- reglas de negocio.

---

# 25. Protección contra inyección

El acceso a datos debe utilizar Entity Framework Core y mecanismos parametrizados.

No se deben construir consultas SQL concatenando directamente datos enviados por usuarios.

Ejemplo conceptual que debe evitarse:

```text
"SELECT * FROM Productos WHERE Nombre = '" + nombre + "'"
```

Las consultas deben utilizar mecanismos seguros y parametrizados.

---

# 26. SQL Server

La cuenta utilizada por la aplicación para conectarse a SQL Server debe tener únicamente los permisos necesarios.

No se debe utilizar una cuenta administrativa de SQL Server como credencial normal de la aplicación.

Las credenciales no deben almacenarse directamente en:

```text
Código fuente
Git
CLAUDE.md
README.md
appsettings.json
```

cuando el archivo pueda terminar versionado.

---

# 27. Secretos

Nunca se deben almacenar en el repositorio:

```text
Contraseñas
Tokens
API Keys
Connection Strings con credenciales reales
Secretos JWT
Credenciales de proveedores
```

Durante desarrollo pueden utilizarse mecanismos locales seguros de configuración.

En ambientes de despliegue se utilizará un mecanismo seguro de gestión de secretos.

---

# 28. Variables de configuración

La configuración debe separarse por ambiente.

Conceptualmente:

```text
Development
Testing
Production
```

No se deben incluir credenciales reales en configuraciones destinadas al repositorio.

---

# 29. HTTPS

La comunicación entre cliente y servidor debe utilizar HTTPS en ambientes donde exista transmisión real de información.

Especialmente para:

```text
Login
Tokens
Datos de usuarios
Requisiciones
Pedidos
Facturas
```

No se debe transmitir información sensible mediante HTTP sin cifrado en producción.

---

# 30. Tokens de autenticación

**Implementado (autenticación "TASK-015", 2026-09-14).** JWT propio (HMAC-SHA256):

- expiración: configurable (`Jwt:ExpiracionMinutos`, default 60 minutos);
- validación: `ValidateIssuer`/`ValidateAudience`/`ValidateLifetime`/`ValidateIssuerSigningKey`, todas activas (`Program.cs`);
- firma: `SymmetricSecurityKey` con `Jwt:Key` — sin default, falla explícitamente al arrancar si falta (nunca un secreto hardcodeado, `§27`/`§51`); en desarrollo se configura vía `dotnet user-secrets`;
- emisor/audiencia: configurables (`Jwt:Issuer`/`Jwt:Audience`), con default documentado no secreto;
- almacenamiento seguro en cliente: responsabilidad del Frontend, fuera de alcance de esta tarea;
- renovación/revocación: **no implementadas** (refresh tokens explícitamente fuera de alcance de "TASK-015" — quedan para una fase posterior si se autoriza).

---

# 31. Contraseñas

**Implementado (autenticación "TASK-015", 2026-09-14 — ver RN-056/ADR-057).** El sistema administra directamente contraseñas: nunca se almacenan en texto plano, solo `Usuario.PasswordHash` (`PasswordHasher<T>`, PBKDF2 — mecanismo estándar de .NET, no un algoritmo propio).

La recomendación de evaluar primero la delegación a un proveedor de identidad externo **se evaluó explícitamente** antes de implementar (ver auditoría en `progreso.md`, entrada de "TASK-015") — el usuario decidió explícitamente JWT propio (ADR-057), no delegado.

---

# 32. Protección contra fuerza bruta

El mecanismo de autenticación deberá contemplar protección contra intentos repetidos de acceso.

Según la solución de identidad seleccionada pueden utilizarse:

```text
Rate limiting
Bloqueo temporal
Detección de intentos anómalos
Políticas del proveedor de identidad
```

No se debe implementar una solución improvisada si el proveedor de identidad ya ofrece mecanismos adecuados.

---

# 33. Rate limiting

La API debe estar preparada para limitar solicitudes excesivas.

Especialmente en:

```text
Login
Endpoints públicos
Búsquedas
Operaciones costosas
Endpoints sensibles
```

La configuración definitiva dependerá del ambiente y del mecanismo de autenticación.

---

# 34. CORS

La API debe permitir solicitudes únicamente desde los orígenes autorizados.

Durante desarrollo puede existir:

```text
localhost
```

pero producción debe utilizar los dominios reales autorizados.

No se debe configurar:

```text
AllowAnyOrigin
```

de forma indiscriminada en producción.

---

# 35. Protección de información sensible

La API no debe devolver información que el consumidor no necesite.

Ejemplo incorrecto:

```json
{
  "usuario": {
    "passwordHash": "...",
    "internalSecurityData": "..."
  }
}
```

Los DTOs deben controlar exactamente qué información sale de la API.

---

# 36. Errores y seguridad

Los mensajes de error para el usuario deben ser útiles pero no revelar información interna.

Incorrecto:

```text
SqlException:
Server=SQLPROD01;
Database=Auropaq;
Table=...
StackTrace=...
```

Correcto:

```text
No fue posible completar la operación.
```

El detalle técnico debe quedar en los logs internos.

---

# 37. Auditoría

Las operaciones críticas deben dejar trazabilidad.

Ejemplos:

```text
Crear requisición
Modificar requisición
Enviar requisición
Aprobar requisición
Devolver requisición
Crear producto
Modificar producto
Homologar producto
Crear pedido
Registrar entrega
Registrar factura
Cambiar permisos
```

La auditoría debe permitir responder:

```text
¿Quién?
¿Qué hizo?
¿Sobre qué recurso?
¿Cuándo?
¿Cuál era el estado anterior?
¿Cuál fue el nuevo estado?
```

---

# 38. Historial vs auditoría

No deben confundirse.

## Historial de negocio

Ejemplo:

```text
BORRADOR → ENVIADA
```

Representa la evolución funcional de una requisición.

## Auditoría

Representa la acción realizada por el usuario o sistema.

Ejemplo:

```text
Usuario 25
Acción: ENVIAR_REQUISICION
Recurso: Requisicion 100
Fecha: ...
```

Ambos mecanismos pueden coexistir.

---

# 39. Seguridad de operaciones críticas

Las operaciones críticas deben tener controles adicionales.

Ejemplos:

```text
Aprobar requisición
Crear pedido
Registrar entrega
Registrar factura
Administrar permisos
```

Antes de ejecutarlas se debe comprobar:

```text
Autenticación
+
Permiso
+
Alcance
+
Estado
+
Reglas de negocio
```

---

# 40. Protección de integridad de cantidades

La seguridad no consiste únicamente en proteger acceso.

También debe proteger la integridad de las operaciones.

Ejemplo:

Un usuario no debería poder modificar:

```text
Cantidad solicitada = 20
```

a:

```text
Cantidad solicitada = -500
```

ni distribuir:

```text
Cantidad solicitada = 20

Sede A = 20
Sede B = 50
```

El backend debe impedirlo.

---

# 41. Seguridad de productos

Un usuario no debe poder utilizar un producto inactivo como si fuera un producto vigente si las reglas del negocio lo prohíben.

Ejemplo:

```text
Producto = INACTIVO
```

y el usuario intenta:

```text
Agregar producto a nueva requisición
```

El backend debe validar el estado del producto.

---

# 42. Seguridad de sedes

El backend debe validar siempre:

```text
Sede existe
+
Sede activa
+
Sede pertenece a empresa
+
Usuario tiene alcance
```

No se debe confiar únicamente en el `sedeId` recibido.

---

# 43. Seguridad de periodos

El backend debe validar:

```text
Periodo existe
Periodo activo
Ventana de solicitud
Fecha actual
Empresa autorizada
```

No debe permitirse crear o enviar una requisición fuera de la ventana establecida cuando las reglas del negocio lo prohíban.

---

# 44. Separación entre seguridad y negocio

La seguridad y las reglas de negocio están relacionadas pero no son exactamente lo mismo.

Ejemplo:

```text
Seguridad:
¿Puede Juan aprobar requisiciones?

Negocio:
¿La requisición está en un estado que permite aprobarla?
```

La operación solo debe ejecutarse cuando ambas condiciones se cumplan.

---

# 45. Seguridad de administradores

Los usuarios administrativos deben utilizar privilegios elevados únicamente cuando sea necesario.

No se debe considerar:

```text
ADMINISTRADOR
```

como permiso absoluto para ignorar todas las reglas de negocio.

Incluso un administrador debe operar bajo controles y auditoría.

Las excepciones administrativas deben definirse explícitamente.

---

# 46. Cambios de permisos

Los cambios relacionados con:

```text
Roles
Permisos
Usuarios
Alcances
```

deben quedar auditados.

Debe poder conocerse:

```text
Quién realizó el cambio
Qué cambió
Cuándo
Sobre qué usuario/rol
```

---

# 47. Acceso a datos históricos

Los usuarios no deben poder modificar información histórica simplemente porque actualmente tengan permisos sobre el recurso.

Ejemplo:

Una entrega cerrada no debe poder modificarse libremente.

El backend debe diferenciar:

```text
Permiso actual
```

de:

```text
Estado histórico del recurso
```

---

# 48. Seguridad de documentos y archivos

Si en el futuro se permiten archivos como:

```text
Facturas
Remisiones
Soportes
Documentos
```

deberán establecerse controles sobre:

- tipo de archivo;
- tamaño;
- nombre;
- almacenamiento;
- acceso;
- descarga;
- permisos;
- virus/malware cuando corresponda.

No se debe asumir que un archivo subido por un usuario es confiable.

---

# 49. Sesiones y cierre de sesión

La estrategia de autenticación debe contemplar:

```text
Inicio de sesión
Expiración
Cierre de sesión
Renovación
Revocación
```

según el mecanismo de identidad seleccionado.

---

# 50. Seguridad del entorno de desarrollo

Durante desarrollo:

- no utilizar credenciales reales;
- no subir secretos a Git;
- no compartir tokens;
- no utilizar datos reales innecesariamente;
- separar configuraciones locales de configuraciones productivas.

---

# 51. Seguridad en Git

Nunca se deben realizar commits que contengan:

```text
.env
Secretos
Passwords
Tokens
Certificados privados
Connection strings reales
```

Debe utilizarse `.gitignore` y mecanismos apropiados de configuración.

Si un secreto se filtra accidentalmente, no basta con eliminarlo del commit actual.

Debe considerarse comprometido y reemplazarse/revocarse.

---

# 52. Seguridad de API

Todas las rutas protegidas deben declarar claramente sus requisitos de autorización.

Conceptualmente:

```text
GET /api/v1/requisiciones
    ↓
REQUISICION_VER

POST /api/v1/requisiciones/{id}/enviar
    ↓
REQUISICION_ENVIAR

POST /api/v1/requisiciones/{id}/aprobar
    ↓
REQUISICION_APROBAR
```

Esto facilita:

- mantenimiento;
- pruebas;
- auditoría;
- revisión de seguridad.

---

# 53. Matriz conceptual de permisos

La matriz definitiva debe construirse con el negocio.

Ejemplo inicial:

| Acción                       | Solicitante |  Gestor | Compras | Recepción | Administrador |
| ---------------------------- | ----------: | ------: | ------: | --------: | ------------: |
| Crear requisición            |           ✓ |       ✓ |       - |         - |             ✓ |
| Modificar requisición propia |           ✓ |       ✓ |       - |         - |             ✓ |
| Enviar requisición           |           ✓ |       ✓ |       - |         - |             ✓ |
| Ver requisiciones            |     Alcance | Alcance | Alcance |   Alcance |        Amplio |
| Aprobar requisición          |           - |       ✓ |       - |         - |             ✓ |
| Devolver requisición         |           - |       ✓ |       - |         - |             ✓ |
| Crear producto               |           - |       ✓ |       ✓ |         - |             ✓ |
| Crear pedido                 |           - |       - |       ✓ |         - |             ✓ |
| Registrar entrega            |           - |       - |       - |         ✓ |             ✓ |
| Registrar factura            |           - |       - |       ✓ |         - |             ✓ |

Esta tabla es conceptual.

No debe implementarse como verdad definitiva hasta validar los roles reales.

---

# 54. Modelo de autorización esperado

El sistema debe evolucionar hacia un modelo:

```text
Usuario
   │
   ├── Roles
   │      │
   │      └── Permisos
   │
   └── Alcances
          │
          ├── Empresas
          └── Sedes
```

La decisión exacta sobre cómo persistir los alcances puede evolucionar.

No debe hardcodearse en Controllers.

---

# 55. Middleware y autorización

La API debe utilizar los mecanismos propios de ASP.NET Core para:

```text
Autenticación
Autorización
Middleware
Policies
Claims
```

La lógica específica de negocio no debe estar toda dentro del middleware.

El middleware debe encargarse de preocupaciones transversales.

Las reglas de negocio deben permanecer en las capas correspondientes.

---

# 56. Policies

Cuando sea necesario, se recomienda utilizar políticas de autorización.

Ejemplo conceptual:

```text
Policy:
PuedeAprobarRequisicion
```

La policy puede evaluar:

```text
Permiso
+
Claims
```

La autorización sobre el recurso debe complementarse con la validación de alcance y estado.

---

# 57. Claims

Si el mecanismo de identidad utiliza claims, pueden representar información como:

```text
sub / UsuarioId
roles
permisos
tenant/company context
```

Sin embargo, no se debe almacenar en el token información que pueda cambiar frecuentemente si hacerlo genera problemas de consistencia.

Por ejemplo, si un usuario pierde acceso a una sede, el sistema debe garantizar que el cambio sea efectivo según la estrategia de identidad utilizada.

---

# 58. No confiar en claims para todo

Los claims ayudan a identificar y autorizar, pero el backend debe consultar información actualizada cuando la operación lo requiera.

Ejemplo:

```text
Token:
Usuario tiene acceso a Sede A
```

Pero posteriormente:

```text
Se revoca acceso a Sede A
```

El sistema debe tener una estrategia para que la revocación tenga efecto.

---

# 59. Seguridad de consultas

Toda consulta debe respetar el alcance del usuario.

Conceptualmente:

```text
Usuario
   ↓
Consulta requisiciones
   ↓
Filtrar por alcance
   ↓
Devolver únicamente datos autorizados
```

No se debe:

```text
SELECT todas las requisiciones
```

y después intentar ocultarlas en React.

La restricción debe ocurrir en backend.

---

# 60. Seguridad en endpoints de detalle

No basta con proteger:

```text
GET /requisiciones
```

También deben protegerse:

```text
GET /requisiciones/{id}
PUT /requisiciones/{id}
DELETE /requisiciones/{id}/detalles/{detalleId}
POST /requisiciones/{id}/aprobar
```

Cada operación debe validar el recurso específico.

---

# 61. Prevención de acceso horizontal

Debe evitarse que:

```text
Usuario A
```

pueda acceder a:

```text
Requisición de Usuario B
```

simplemente cambiando el ID.

Esto se conoce conceptualmente como un problema de autorización horizontal.

La solución es validar:

```text
Usuario
+
Empresa
+
Sede
+
Permiso
+
Recurso
```

según corresponda.

---

# 62. Prevención de escalamiento de privilegios

Un usuario no debe poder modificarse a sí mismo:

```text
Rol = ADMINISTRADOR
```

ni modificar sus propios permisos si no posee autorización para administrar seguridad.

Los cambios de:

```text
Usuario
Rol
Permiso
Alcance
```

deben estar protegidos.

---

# 63. Pruebas de seguridad

La aplicación debe incluir pruebas que verifiquen como mínimo:

### Autenticación

```text
Usuario no autenticado → 401
```

### Autorización

```text
Usuario sin permiso → 403
```

### Alcance

```text
Usuario empresa A
→ No puede acceder a empresa B
```

### Sede

```text
Usuario sin acceso a sede
→ No puede operar sobre ella
```

### Estado

```text
Requisición aprobada
→ No puede modificarse
```

### Manipulación de IDs

```text
Usuario cambia requisicionId
→ Acceso denegado
```

---

# 64. Pruebas de reglas críticas

Se deben probar especialmente:

```text
No duplicar requisición mensual
No enviar fuera de ventana
No aprobar sin permiso
No modificar requisición cerrada
No utilizar producto inactivo
No utilizar sede de otra empresa
No distribuir cantidades incorrectas
No acceder a información de otra empresa
```

---

# 65. Seguridad y trazabilidad

Toda operación que cambie información importante debe poder reconstruirse posteriormente.

Ejemplo:

```text
Quién modificó la cantidad
Quién envió
Quién devolvió
Quién aprobó
Quién creó un producto
Quién cambió permisos
Quién registró una entrega
```

La seguridad no termina cuando se bloquea una operación.

También debe poder demostrarse qué ocurrió cuando una operación fue permitida.

---

# 66. Seguridad de datos históricos

Los registros históricos deben protegerse contra modificaciones no autorizadas.

Especialmente:

```text
Requisiciones aprobadas
Pedidos
Entregas
Facturas
Auditoría
Historial
```

La capacidad de consulta y modificación debe ser diferente.

---

# 67. Seguridad ante errores

Los errores inesperados deben:

1. registrarse internamente;
2. no revelar información sensible;
3. devolver una respuesta controlada;
4. permitir correlacionar el error con logs.

Puede utilizarse un identificador de correlación:

```text
CorrelationId
```

para relacionar:

```text
Request
Log
Error
Auditoría
```

cuando corresponda.

---

# 68. Seguridad y rendimiento

Las comprobaciones de autorización deben ser seguras sin generar consultas innecesarias.

No se debe consultar repetidamente la base de datos para comprobar lo mismo si puede resolverse de forma segura mediante una estrategia apropiada.

Sin embargo, no se debe sacrificar seguridad únicamente por rendimiento.

La optimización debe hacerse después de identificar un problema real.

---

# 69. Principio de seguridad por diseño

La seguridad debe considerarse antes de implementar cada funcionalidad.

Antes de crear un endpoint se debe preguntar:

```text
¿Quién puede usarlo?
¿Qué permiso necesita?
¿Sobre qué empresa?
¿Sobre qué sede?
¿Sobre qué recurso?
¿En qué estado?
¿Qué información devuelve?
¿Qué información no debe devolver?
¿Qué debe quedar auditado?
```

---

# 70. Lo que NO debe hacer Claude Code

Claude Code no debe:

- hardcodear usuarios;
- hardcodear empresas;
- hardcodear sedes;
- hardcodear contraseñas;
- guardar secretos en el código;
- guardar tokens en Git;
- confiar en `UsuarioId` enviado por React;
- confiar en `EmpresaId` enviado por React;
- confiar en `Rol` enviado por React;
- permitir cambios de estado arbitrarios;
- ocultar seguridad únicamente en React;
- devolver entidades completas con información sensible;
- devolver stack traces al cliente;
- crear permisos globales sin justificación;
- dar acceso total a cualquier administrador automáticamente;
- desactivar autorización para facilitar desarrollo;
- introducir un proveedor de identidad sin documentarlo;
- crear un sistema de autenticación propio si existe una alternativa segura y adecuada ya disponible;
- agregar librerías de seguridad sin justificar su necesidad.

---

# 71. Regla ante ambigüedad

Si una funcionalidad requiere una decisión de seguridad que no esté definida:

1. identificar el riesgo;
2. explicar la situación;
3. proponer alternativas;
4. no asumir permisos;
5. no permitir acceso amplio como solución temporal;
6. solicitar una decisión.

Ante una duda de autorización:

```text
DENEGAR
```

es preferible a:

```text
PERMITIR POR DEFECTO
```

hasta que exista una definición clara.

---

# 72. Seguridad mínima del MVP

El MVP debe contar como mínimo con:

```text
Autenticación
      ↓
Identificación de usuario
      ↓
Roles
      ↓
Permisos
      ↓
Alcance por empresa
      ↓
Alcance por sede
      ↓
Validación de recursos
      ↓
Validación de estados
      ↓
Auditoría de operaciones críticas
      ↓
Manejo seguro de errores
```

No es necesario implementar desde el primer día todas las capacidades avanzadas de seguridad.

Pero la arquitectura no debe impedir incorporarlas posteriormente.

---

# 73. Seguridad futura

El sistema podrá evolucionar para incorporar:

```text
Multi-factor authentication
Single Sign-On
Proveedor de identidad empresarial
Refresh tokens
Revocación avanzada
Rate limiting avanzado
Detección de comportamiento anómalo
Gestión centralizada de secretos
WAF
Monitoreo de seguridad
Alertas
Escaneo de vulnerabilidades
```

Estas capacidades no forman parte necesariamente del primer vertical slice.

---

# 74. Flujo de autorización de una operación

Una operación crítica debe seguir conceptualmente:

```text
Request
   ↓
HTTPS
   ↓
Autenticación
   ↓
¿Usuario válido?
   │
   ├── NO → 401
   │
   └── SÍ
         ↓
      Permiso
         ↓
    ¿Autorizado?
         │
         ├── NO → 403
         │
         └── SÍ
               ↓
            Alcance
               ↓
      ¿Recurso permitido?
               │
               ├── NO → Denegar
               │
               └── SÍ
                     ↓
                Estado actual
                     ↓
              Regla de negocio
                     ↓
                  Ejecutar
                     ↓
                 Auditar
```

Este flujo representa la forma de pensar la seguridad en el sistema.

---

# 75. Principio final

La seguridad no debe agregarse después de terminar el sistema.

Debe formar parte del diseño desde el principio.

El sistema debe poder responder de forma clara:

```text
¿Quién eres?
    ↓
Autenticación

¿Qué puedes hacer?
    ↓
Permisos

¿Sobre qué información?
    ↓
Alcance

¿Puedes hacerlo ahora?
    ↓
Estado + reglas de negocio

¿Qué ocurrió?
    ↓
Auditoría + historial
```

La seguridad debe proteger el negocio sin convertir la aplicación en una estructura innecesariamente compleja.

La implementación debe comenzar con controles sólidos y comprensibles, y evolucionar según las necesidades reales del sistema.
