# CLAUDE.md

# AuropaqPedidos — Guía de trabajo para Claude Code

## 1. Propósito del archivo

Este archivo define las reglas obligatorias que Claude Code debe seguir al trabajar en el proyecto **AuropaqPedidos**.

Claude Code debe utilizar este archivo como guía principal de comportamiento dentro del repositorio.

La documentación ubicada en `/docs` contiene las definiciones de negocio, dominio, arquitectura, base de datos, API, seguridad, decisiones y tareas.

La regla fundamental es:

> **El negocio y la arquitectura son definidos por el equipo. Claude Code implementa lo que está definido y autorizado.**

Claude Code no debe inventar requisitos ni tomar decisiones arquitectónicas importantes por iniciativa propia.

---

# 2. Contexto del proyecto

AuropaqPedidos es un sistema para digitalizar y gestionar el proceso de:

```text
Requisiciones
      ↓
Revisión
      ↓
Consolidación
      ↓
Pedidos a proveedores
      ↓
Distribución
      ↓
Entregas
      ↓
Facturación
      ↓
Cierre
```

El sistema debe permitir trabajar con múltiples:

- empresas;
- sedes;
- usuarios;
- roles;
- permisos;
- productos;
- proveedores.

Las empresas y sedes son información configurable.

No deben estar hardcodeadas en el código.

---

# 3. Documentación oficial del proyecto

Antes de realizar cambios relevantes, Claude Code debe consultar la documentación relacionada.

```text
/docs
│
├── 00-contexto-proyecto.md
├── 01-reglas-negocio.md
├── 02-dominio.md
├── 03-arquitectura.md
├── 04-base-datos.md
├── 05-api.md
├── 06-seguridad.md
├── 07-decisiones-arquitectura.md
└── 08-tareas.md
```

Estas fuentes deben interpretarse de la siguiente manera:

```text
00 → Contexto general
01 → Reglas del negocio
02 → Modelo de dominio
03 → Arquitectura
04 → Base de datos
05 → API
06 → Seguridad
07 → Decisiones arquitectónicas
08 → Tareas de implementación
```

Cuando exista una contradicción entre código y documentación, Claude Code debe:

1. identificar la contradicción;
2. informar cuál es;
3. no modificar automáticamente el diseño;
4. solicitar una decisión cuando afecte el negocio o la arquitectura.

---

# 4. Regla de prioridad

Al implementar una tarea, Claude Code debe considerar esta prioridad:

```text
Reglas de negocio
       ↓
Dominio
       ↓
Decisiones arquitectónicas
       ↓
Arquitectura
       ↓
Base de datos
       ↓
API
       ↓
Implementación
```

La implementación debe adaptarse a las decisiones anteriores.

No debe hacerse lo contrario.

---

# 5. Regla principal de comportamiento

Claude Code debe:

- leer antes de modificar;
- comprender antes de implementar;
- analizar antes de crear abstracciones;
- probar después de modificar;
- informar antes de ampliar el alcance.

Claude Code NO debe:

- inventar requisitos;
- inventar reglas de negocio;
- inventar entidades innecesarias;
- inventar endpoints;
- introducir tecnologías sin autorización;
- realizar refactorizaciones no solicitadas;
- modificar arquitectura sin autorización;
- modificar contratos existentes sin autorización;
- eliminar código sin justificación;
- implementar funcionalidades fuera de la tarea.

---

# 6. No inventar requisitos

Si una funcionalidad no está definida en:

- documentación;
- tarea;
- decisión aprobada;
- requerimiento explícito del usuario;

Claude Code no debe asumirla como requisito.

Ejemplo:

Si no está definido si una factura puede estar asociada a varios pedidos, Claude Code no debe inventar la relación.

Debe informar:

```text
Existe una ambigüedad:

No está definido si una factura puede cubrir:
- un pedido;
- varios pedidos;
- parcialmente un pedido.

Esto puede afectar el modelo de datos.

Se requiere decisión antes de implementar esta parte.
```

---

# 7. No inventar arquitectura

Claude Code no debe introducir por iniciativa propia:

- microservicios;
- CQRS;
- event sourcing;
- Redis;
- RabbitMQ;
- Kafka;
- Kubernetes;
- Docker;
- repositorios genéricos;
- patrones adicionales;
- nuevas capas;
- nuevas tecnologías;

si no existe una necesidad documentada o autorización explícita.

La arquitectura actual debe mantenerse simple y modular.

---

# 8. Arquitectura actual

La arquitectura definida inicialmente es:

```text
Frontend
React + TypeScript + Tailwind
        ↓
ASP.NET Core Web API
        ↓
Application
        ↓
Domain
        ↓
Infrastructure
        ↓
Entity Framework Core
        ↓
SQL Server
```

Conceptualmente:

```text
Domain
Application
Infrastructure
Api
```

El dominio no debe depender directamente de infraestructura.

---

# 9. Arquitectura modular monolítica

El proyecto utilizará inicialmente una arquitectura modular monolítica.

Conceptualmente:

```text
AuropaqPedidos
│
├── Organización
├── Catálogo
├── Periodos
├── Requisiciones
├── Consolidación
├── Pedidos
├── Entregas
├── Facturación
├── Seguridad
└── Auditoría
```

No convertir automáticamente estos módulos en microservicios.

La separación física de proyectos y carpetas debe respetar la estructura real del repositorio y las decisiones documentadas.

---

# 10. No hardcodear empresas

Nunca implementar lógica de negocio basada directamente en nombres de empresas.

Incorrecto:

```csharp
if (empresa.Nombre == "AUROTECH")
{
    // lógica especial
}
```

Incorrecto:

```csharp
if (empresaId == 1)
{
    // lógica especial
}
```

salvo que exista una razón técnica excepcional explícitamente autorizada y documentada.

El sistema debe funcionar mediante:

```text
Empresa
Sede
Usuario
Rol
Permiso
Alcance
```

---

# 11. No hardcodear personas

Nunca implementar reglas como:

```csharp
if (usuario.Nombre == "Martha")
{
}
```

Las responsabilidades deben representarse mediante:

```text
Usuario
 ↓
Rol
 ↓
Permisos
 ↓
Alcance
```

El sistema representa responsabilidades, no personas específicas.

---

# 12. No hardcodear sedes

No asumir que existen únicamente:

```text
Bogotá
Medellín
Cali
```

ni cualquier otro conjunto específico de sedes.

Las sedes pertenecen a empresas y deben ser datos configurables.

---

# 13. Reglas de negocio en backend

El backend es la fuente de verdad del negocio.

El frontend puede realizar validaciones para mejorar la experiencia del usuario, pero nunca debe ser la única protección.

Ejemplo:

```text
React
 ↓
validación UX

Backend
 ↓
validación obligatoria
```

Las reglas críticas deben validarse en backend.

---

# 14. Seguridad

Claude Code nunca debe confiar únicamente en información enviada desde React.

No confiar directamente en:

```json
{
  "usuarioId": 5,
  "empresaId": 2,
  "rol": "ADMIN"
}
```

El backend debe determinar y validar:

- usuario autenticado;
- empresa;
- sedes;
- roles;
- permisos;
- alcance.

Toda operación debe evaluar cuando corresponda:

```text
Permiso
+
Alcance
+
Estado
+
Reglas de negocio
```

---

# 15. Permisos y roles

Los roles y permisos son conceptos diferentes.

Modelo:

```text
Usuario
   ↓
Rol
   ↓
Permiso
```

No utilizar el nombre del rol como única regla de autorización cuando el sistema requiera permisos específicos.

---

# 16. Alcance de acceso

Tener un permiso no significa automáticamente tener acceso a todos los recursos.

Debe comprobarse el alcance.

Ejemplo:

```text
Permiso:
REQUISICION_VER

Alcance:
Empresa X
```

Un usuario con ese permiso no necesariamente puede consultar requisiciones de todas las empresas.

---

# 17. Estado de las entidades

No asumir que un usuario puede realizar cualquier acción solamente porque posee un permiso.

Ejemplo:

```text
Permiso:
REQUISICION_MODIFICAR
```

no significa necesariamente que pueda modificar una requisición:

```text
APROBADA
```

Debe verificarse:

```text
Permiso
+
Alcance
+
Estado actual
+
Reglas de negocio
```

---

# 18. Requisiciones

Una requisición representa una necesidad interna.

No debe confundirse con:

```text
Consolidación
PedidoProveedor
Entrega
Factura
```

La cadena conceptual es:

```text
Requisición
    ↓
Consolidación
    ↓
PedidoProveedor
    ↓
Entrega
    ↓
Factura
```

Cada etapa tiene responsabilidades diferentes.

---

# 19. Una requisición mensual por empresa

La regla conceptual es:

```text
Empresa + Periodo
        ↓
máximo una Requisición
```

No implementar:

```text
Empresa + Sede + Periodo
```

como regla de unicidad de requisiciones.

Una requisición puede contener productos destinados a varias sedes de la misma empresa.

---

# 20. Borrador y envío

Guardar una requisición no significa enviarla.

Debe diferenciarse:

```text
Guardar borrador
```

de:

```text
Enviar requisición
```

Una requisición puede permanecer:

```text
BORRADOR
```

hasta que el usuario realice la acción de envío.

---

# 21. No modificar silenciosamente requisiciones enviadas

Una requisición enviada no debe modificarse silenciosamente.

Si necesita corrección:

```text
ENVIADA
   ↓
DEVUELTA
   ↓
CORRECCIÓN
   ↓
REENVÍO
```

Debe mantenerse la trazabilidad.

---

# 22. Productos no catalogados

No convertir automáticamente cualquier descripción enviada por un usuario en un producto oficial.

Cuando un producto no exista:

```text
Producto no catalogado
        ↓
Revisión
        ↓
┌──────────────┬──────────────┬────────────┐
│ Homologar    │ Crear        │ Rechazar   │
│              │ producto     │            │
└──────────────┴──────────────┴────────────┘
```

La decisión debe conservarse.

---

# 23. Producto interno y producto del proveedor

No asumir que el código interno del producto es igual al código del proveedor.

Debe utilizarse la relación:

```text
Producto
   ↓
ProductoProveedor
   ↓
Proveedor
```

Esto permite que un producto interno tenga diferentes identificadores según el proveedor.

---

# 24. Cantidades

No sobrescribir cantidades de diferentes etapas del proceso.

Diferenciar:

```text
Cantidad solicitada
Cantidad consolidada
Cantidad pedida
Cantidad entregada
Cantidad pendiente
```

Ejemplo:

```text
Solicitud:
85

Compra:
100

Entrega:
60

Pendiente:
40
```

La diferencia entre estas cantidades debe conservarse.

---

# 25. Consolidación

La consolidación representa una necesidad total derivada de requisiciones aprobadas.

No debe destruir ni modificar las requisiciones originales.

Debe mantenerse trazabilidad:

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

# 26. Pedidos a proveedores

Una consolidación puede generar múltiples pedidos.

No asumir:

```text
1 Consolidación = 1 Pedido
```

La relación debe permitir:

```text
Consolidación
      ↓
Pedido proveedor A
Pedido proveedor B
Pedido proveedor C
```

---

# 27. Empresa y pedido proveedor

No agregar `EmpresaId` directamente a `PedidoProveedor` solamente para saber qué empresas participaron.

Un pedido puede consolidar necesidades de varias empresas.

La trazabilidad debe mantenerse mediante sus detalles y distribuciones.

---

# 28. Entregas

Un pedido puede tener múltiples entregas.

Ejemplo:

```text
Pedido = 100

Entrega 1 = 60
Entrega 2 = 40
```

No asumir que un pedido tiene una única entrega.

---

# 29. Dirección histórica

Cuando una entrega se registre, debe conservarse la información histórica necesaria del destino.

No depender exclusivamente de la dirección actual de `Sede`.

Una modificación futura de la sede no debe alterar la historia de una entrega anterior.

---

# 30. Fechas y periodos

No hardcodear fechas como:

```text
1 de cada mes
3 de cada mes
5 días
```

cuando representen reglas de negocio configurables.

Los periodos y ventanas deben ser datos/configuración.

El plazo del proveedor y la ventana de solicitud son reglas diferentes.

---

# 31. API

La API debe representar casos de uso y acciones de negocio.

Ejemplo:

```http
POST /api/v1/requisiciones/{id}/enviar
POST /api/v1/requisiciones/{id}/aprobar
POST /api/v1/requisiciones/{id}/devolver
```

Preferir acciones explícitas cuando representan una operación de negocio significativa.

No reemplazar automáticamente todas las operaciones por un CRUD genérico.

---

# 32. Versionado de API

La API actual debe seguir el estándar definido:

```text
/api/v1/
```

No cambiar la versión de la API sin una razón documentada.

No modificar contratos existentes sin autorización.

---

# 33. DTOs

Los endpoints no deben exponer directamente las entidades de persistencia cuando el diseño requiera DTOs.

Los DTOs deben representar las necesidades de entrada y salida de la API.

No exponer información interna innecesaria.

---

# 34. Manejo de errores

Los errores deben seguir una estrategia consistente y centralizada.

No crear respuestas de error completamente diferentes en cada controller.

No exponer:

- stack traces;
- credenciales;
- secretos;
- información interna innecesaria;
- detalles de infraestructura;

en respuestas destinadas al cliente.

---

# 35. Controllers

Los controllers deben mantenerse delgados.

Su responsabilidad principal es:

```text
HTTP
 ↓
Validación inicial
 ↓
Caso de uso
 ↓
Respuesta HTTP
```

No colocar reglas complejas de negocio directamente en controllers.

---

# 36. Domain

El dominio debe representar conceptos y reglas del negocio.

No colocar en el dominio dependencias innecesarias de:

- ASP.NET Core;
- SQL Server;
- EF Core;
- HTTP;
- React.

---

# 37. Application

Application coordina los casos de uso.

Debe encargarse de aspectos como:

- orquestación;
- DTOs;
- ejecución de casos de uso;
- coordinación entre dominio e infraestructura.

No convertir Application en un lugar para acumular arbitrariamente toda la lógica del sistema.

---

# 38. Infrastructure

Infrastructure contiene aspectos técnicos como:

- EF Core;
- DbContext;
- configuraciones de persistencia;
- acceso a datos;
- integraciones externas.

No utilizar Infrastructure para esconder reglas de negocio.

---

# 39. Entity Framework Core

EF Core es la tecnología ORM definida.

No introducir otro ORM sin autorización.

Las modificaciones del esquema deben gestionarse mediante migraciones.

---

# 40. Base de datos

La base de datos principal es:

```text
SQL Server
```

Debe mantenerse:

- integridad referencial;
- claves;
- restricciones;
- índices justificados;
- relaciones;
- consistencia transaccional.

No agregar tablas simplemente porque una clase necesita persistirse.

Primero determinar si el concepto realmente necesita persistencia.

---

# 41. No crear entidades innecesarias

No todo sustantivo del negocio tiene que convertirse automáticamente en una tabla.

Antes de crear una entidad, evaluar:

```text
¿Tiene identidad?
¿Tiene comportamiento?
¿Tiene ciclo de vida?
¿Debe persistirse?
¿Tiene relaciones importantes?
¿Es realmente un concepto del dominio?
```

---

# 42. No usar Generic Repository automáticamente

No crear automáticamente:

```text
GenericRepository<T>
```

para todas las entidades.

Las abstracciones deben resolver necesidades reales.

---

# 43. No sobrearquitecturar

No implementar automáticamente:

```text
CQRS completo
Microservicios
Event sourcing
Redis
Mensajería
Kubernetes
Infraestructura distribuida
```

La complejidad debe estar justificada por requisitos reales.

---

# 44. Rendimiento

No optimizar prematuramente.

Regla:

```text
Medir
 ↓
Identificar problema
 ↓
Optimizar
 ↓
Medir nuevamente
```

No agregar índices, cachés o mecanismos complejos sin evidencia de necesidad.

---

# 45. Pruebas

Una tarea no está terminada simplemente porque compile.

Debe verificarse mediante pruebas apropiadas.

Según el caso:

```text
Regla de dominio
      ↓
Prueba unitaria

Caso de uso
      ↓
Prueba de aplicación

API
      ↓
Prueba de integración

Frontend
      ↓
Prueba correspondiente
```

---

# 46. Regla de no romper funcionalidades existentes

Antes de modificar código existente:

1. inspeccionarlo;
2. comprender su propósito;
3. identificar dependencias;
4. evaluar impacto.

No eliminar ni reemplazar código simplemente porque exista una alternativa que parezca mejor.

Si es necesario realizar una refactorización:

- explicar por qué;
- identificar impacto;
- mantener el alcance controlado;
- solicitar aprobación si afecta arquitectura o comportamiento.

---

# 47. No realizar refactorizaciones oportunistas

Si Claude Code encuentra:

```text
código mejorable
```

pero no afecta la tarea actual:

No modificarlo automáticamente.

Registrar:

```text
Posible mejora futura:
...
```

La mejora puede convertirse posteriormente en una tarea independiente.

---

# 48. No instalar dependencias sin autorización

No agregar paquetes, librerías o frameworks únicamente porque faciliten la implementación.

Antes de agregar una dependencia:

```text
1. Identificar problema.
2. Explicar por qué la dependencia es necesaria.
3. Evaluar alternativas.
4. Informar impacto.
5. Obtener autorización cuando corresponda.
```

---

# 49. No modificar múltiples áreas sin necesidad

Una tarea debe modificar únicamente las partes necesarias.

Ejemplo:

Si la tarea consiste en crear `Empresa`, no aprovecharla para:

- rediseñar autenticación;
- cambiar toda la API;
- modificar React completo;
- implementar permisos avanzados;
- refactorizar todo el proyecto.

---

# 50. Protocolo obligatorio antes de implementar

Para cada TASK de `/docs/08-tareas.md`:

### 1. Leer

```text
CLAUDE.md
```

### 2. Leer tarea

```text
/docs/08-tareas.md
```

### 3. Leer documentación relacionada

Según corresponda:

```text
00
01
02
03
04
05
06
07
```

### 4. Inspeccionar repositorio

Revisar código real.

### 5. Presentar plan

Informar:

```text
Tarea:
Objetivo:

Archivos que modificaré:

Archivos que crearé:

Componentes afectados:

Reglas aplicadas:

Pruebas:

Dependencias:

Ambigüedades:
```

### 6. Esperar autorización

Si la tarea tiene impacto arquitectónico o ambigüedades relevantes, no implementar esa parte hasta recibir decisión.

### 7. Implementar

Implementar únicamente la tarea aprobada.

### 8. Probar

Ejecutar las pruebas correspondientes.

### 9. Revisar

Comprobar:

```text
Compilación
Pruebas
Reglas
Seguridad
Alcance
Arquitectura
```

### 10. Reportar

Informar:

```text
Tarea:
Estado:

Archivos creados:
Archivos modificados:

Cambios:

Pruebas ejecutadas:

Resultado:

Problemas:

Decisiones pendientes:
```

---

# 51. Qué hacer cuando exista una ambigüedad

## Ambigüedad menor

Si no cambia:

- modelo;
- reglas;
- arquitectura;
- seguridad;
- API;

puede utilizarse la convención existente del proyecto.

## Ambigüedad importante

Si puede cambiar:

- entidades;
- relaciones;
- estados;
- permisos;
- seguridad;
- API;
- arquitectura;
- persistencia;

Claude Code debe detener esa parte e informar.

Nunca rellenar silenciosamente una decisión importante.

---

# 52. Regla de alcance de una TASK

Una TASK debe tratarse como un contrato de trabajo.

Claude Code debe respetar:

```text
Objetivo
Alcance
Restricciones
Reglas
Criterios de aceptación
Fuera de alcance
```

Si una mejora no está dentro de la tarea:

```text
NO IMPLEMENTAR
```

Debe registrarse como posible tarea futura.

---

# 53. Criterios de finalización

Una tarea puede marcarse como `COMPLETADA` únicamente cuando:

```text
[ ] Objetivo cumplido
[ ] Criterios de aceptación cumplidos
[ ] Reglas de negocio respetadas
[ ] Arquitectura respetada
[ ] Seguridad considerada
[ ] Código compila
[ ] Pruebas pasan
[ ] No hay cambios fuera de alcance
[ ] No se agregaron dependencias innecesarias
[ ] No se introdujo lógica hardcodeada
[ ] Documentación actualizada si corresponde
```

---

# 54. Git

Los cambios deben ser fácilmente identificables.

Cuando corresponda:

```text
Una TASK
    ↓
Cambios relacionados
    ↓
Pruebas
    ↓
Commit
```

Preferir mensajes descriptivos.

Ejemplos:

```text
feat: agregar entidad empresa
```

```text
feat: implementar creación de requisición mensual
```

```text
test: agregar pruebas de distribución de requisición
```

Evitar mensajes ambiguos:

```text
cambios
update
arreglos
final
cosas
```

---

# 55. Vertical Slices

Cuando una funcionalidad importante lo permita, preferir desarrollar pequeños flujos completos.

Ejemplo:

```text
Crear requisición
      ↓
Guardar
      ↓
Consultar
      ↓
Agregar detalle
      ↓
Distribuir
      ↓
Validar
      ↓
Enviar
      ↓
Historial
```

Evitar construir primero todo el backend, después todo el frontend y solamente al final intentar integrar el sistema completo.

---

# 56. Primer objetivo funcional

El primer flujo completo del sistema debe ser:

```text
Usuario
  ↓
Empresa
  ↓
Sede
  ↓
Periodo
  ↓
Requisición mensual
  ↓
BORRADOR
  ↓
Agregar productos
  ↓
Distribuir cantidades
  ↓
Validar
  ↓
Enviar
  ↓
Revisión
  ↓
Aprobar / Devolver
  ↓
Corregir
  ↓
Reenviar
```

No avanzar prematuramente a funcionalidades posteriores si este flujo base todavía no es estable.

---

# 57. Fuera del primer MVP

No implementar inicialmente, salvo autorización:

```text
Consolidación avanzada
Pedidos a proveedores
Entregas
Facturación
Dashboards avanzados
Reportes complejos
Redis
RabbitMQ
Kafka
Microservicios
CQRS completo
Event sourcing
Kubernetes
Optimización avanzada
```

Estas funcionalidades pertenecen a fases posteriores.

---

# 58. Principio de aprendizaje

Este proyecto también tiene un objetivo educativo.

Claude Code debe ayudar a construir software, pero no debe ocultar las decisiones detrás del código.

Cuando sea relevante, debe explicar:

```text
¿Qué se hizo?
¿Por qué?
¿Qué regla implementa?
¿Qué archivos participan?
¿Qué entidades intervienen?
¿Qué pruebas existen?
¿Qué alternativas fueron consideradas?
```

El desarrollador debe poder comprender y defender las decisiones implementadas.

---

# 59. Regla para explicar decisiones

Cuando Claude Code proponga una solución técnica relevante, utilizar preferiblemente:

```text
DECISIÓN
¿Por qué?

ALTERNATIVAS
¿Qué otras opciones existen?

RECOMENDACIÓN
¿Qué opción se propone?

CONSECUENCIAS
¿Qué ventajas y costos tiene?

EVOLUCIÓN
¿Cómo podría cambiar en el futuro?
```

---

# 60. Regla para cambios arquitectónicos

Si una tarea requiere cambiar una decisión registrada en:

```text
/docs/07-decisiones-arquitectura.md
```

Claude Code debe:

1. identificar la decisión afectada;
2. explicar por qué necesita cambiar;
3. explicar impacto;
4. proponer alternativa;
5. no realizar el cambio silenciosamente;
6. actualizar la documentación solamente después de aprobar la nueva decisión.

---

# 61. Regla para cambios de API

No cambiar silenciosamente:

- rutas;
- métodos HTTP;
- DTOs;
- nombres de propiedades;
- códigos de respuesta;
- contratos.

Si un cambio es necesario:

```text
Contrato actual
      ↓
Problema
      ↓
Alternativas
      ↓
Decisión
      ↓
Cambio
```

---

# 62. Regla para cambios de base de datos

No eliminar columnas, relaciones o tablas solamente porque parezcan innecesarias.

Antes de una modificación estructural:

```text
1. Identificar dependencia.
2. Revisar documentación.
3. Revisar código.
4. Evaluar migración.
5. Evaluar datos existentes.
6. Evaluar impacto.
```

Los cambios destructivos requieren especial cuidado.

---

# 63. Regla de datos históricos

Cuando un dato histórico pueda cambiar posteriormente en el maestro:

```text
Sede
Producto
Proveedor
Usuario
```

evaluar si el proceso histórico necesita conservar una fotografía/snapshot.

No utilizar automáticamente los datos actuales para reconstruir información histórica.

---

# 64. Regla de transacciones

Cuando una operación de negocio requiera modificar múltiples datos que deben mantenerse consistentes, evaluar explícitamente el uso de una transacción.

Ejemplo conceptual:

```text
Enviar requisición
      ↓
Cambiar estado
      +
Registrar historial
      +
Actualizar información necesaria
```

Estas operaciones no deben quedar parcialmente aplicadas si la regla de negocio exige atomicidad.

---

# 65. Regla de idempotencia

Para operaciones que puedan recibir solicitudes repetidas, evaluar si deben ser idempotentes.

Ejemplo:

```text
Enviar requisición
```

No debería generar accidentalmente múltiples efectos si el mismo comando se procesa repetidamente.

La estrategia concreta debe definirse según el caso de uso.

---

# 66. Regla de concurrencia

Si dos usuarios pueden modificar el mismo recurso simultáneamente, evaluar mecanismos de control de concurrencia.

No asumir que solamente existirá un usuario trabajando sobre el sistema.

---

# 67. Regla de seguridad por defecto

Ante una duda de autorización:

```text
DENEGAR
```

hasta que exista una regla explícita que permita la operación.

Nunca asumir:

```text
"Probablemente este usuario sí puede."
```

---

# 68. Regla de mínimo privilegio

Los usuarios deben recibir únicamente los permisos y alcance necesarios para realizar sus responsabilidades.

No crear automáticamente:

```text
ADMIN
```

para resolver problemas de autorización.

---

# 69. Regla de observabilidad

Los problemas deben poder diagnosticarse mediante:

- logs;
- errores estructurados;
- historial;
- auditoría;

según corresponda.

No registrar secretos ni información sensible innecesaria.

---

# 70. Regla contra soluciones mágicas

Claude Code debe evitar respuestas del tipo:

```text
"Esto se soluciona agregando una capa."
```

o:

```text
"Esto se soluciona usando un patrón."
```

sin explicar el problema concreto.

Toda abstracción debe justificar qué problema resuelve.

---

# 71. Regla contra código especulativo

No crear código para funcionalidades hipotéticas.

Incorrecto:

```text
Crear 20 servicios
porque quizás algún día sean necesarios.
```

Correcto:

```text
Implementar solamente lo necesario
para el caso de uso actual.
```

---

# 72. Regla de evolución

El sistema debe diseñarse para poder evolucionar, pero sin implementar complejidad especulativa.

Principio:

```text
Escalable
≠
Complejo
```

La arquitectura debe permitir crecimiento sin obligar a construir toda la complejidad desde el primer día.

---

# 73. Regla maestra para cada nueva funcionalidad

Antes de implementar una funcionalidad importante, responder:

```text
1. ¿Qué problema de negocio resuelve?

2. ¿Quién realiza la acción?

3. ¿Qué regla de negocio aplica?

4. ¿Qué entidades participan?

5. ¿Qué relaciones existen?

6. ¿Qué estado tienen?

7. ¿Qué permisos se necesitan?

8. ¿Qué alcance tiene el usuario?

9. ¿Qué datos deben persistirse?

10. ¿Qué API representa el caso de uso?

11. ¿Qué sucede si falla?

12. ¿Cómo se prueba?

13. ¿Qué impacto tiene sobre la arquitectura?
```

Si estas preguntas no pueden responderse razonablemente, la funcionalidad puede no estar suficientemente definida para implementarse.

---

# 74. Regla de comunicación

Cuando termine una tarea, Claude Code debe ser claro y directo.

Formato recomendado:

```text
## TASK-XXX — Resultado

### Estado
COMPLETADA / BLOQUEADA / PARCIAL

### Implementado

- ...

### Archivos creados

- ...

### Archivos modificados

- ...

### Pruebas

- ...

### Resultado de pruebas

- ...

### Problemas encontrados

- ...

### Fuera de alcance

- ...

### Decisiones pendientes

- ...
```

No ocultar problemas.

---

# 75. Regla final

Claude Code debe recordar siempre:

```text
NO DECIDIMOS EL NEGOCIO DESDE EL CÓDIGO.

PRIMERO:
Problema

DESPUÉS:
Reglas

DESPUÉS:
Dominio

DESPUÉS:
Arquitectura

DESPUÉS:
Datos y API

FINALMENTE:
Código
```

La implementación debe reflejar el diseño.

No se debe utilizar el código para decidir retroactivamente cómo funciona el negocio.

---

# 76. PRINCIPIO RECTOR DEL PROYECTO

> **Nosotros decidimos el negocio y la arquitectura. Claude Code implementa, prueba y reporta.**

Y ante cualquier duda que pueda afectar significativamente el sistema:

> **Es mejor detenerse y preguntar que implementar una decisión incorrecta que después genere retrabajo.**
