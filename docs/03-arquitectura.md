# ARQUITECTURA DEL SISTEMA

## Sistema de Gestión de Requisiciones y Pedidos - Auropaq

**Proyecto:** AuropaqPedidos
**Documento:** Arquitectura
**Código:** ARC
**Estado:** En definición
**Última actualización:** 2026-09-08

---

# 1. Propósito

Este documento define la arquitectura técnica inicial del sistema AuropaqPedidos.

Su objetivo es establecer:

- Cómo se organiza el backend.
- Cómo se organiza el frontend.
- Cómo se comunican frontend y backend.
- Dónde viven las reglas de negocio.
- Dónde vive el acceso a datos.
- Cómo se manejan errores.
- Cómo se estructuran las funcionalidades.
- Qué dependencias puede tener cada capa.
- Qué patrones se utilizarán.
- Qué complejidad se evitará inicialmente.
- Cómo debe evolucionar el sistema.

La arquitectura debe facilitar:

- Mantenibilidad.
- Escalabilidad.
- Testabilidad.
- Separación de responsabilidades.
- Evolución futura.
- Comprensión del código.
- Reducción del acoplamiento.

---

# 2. Principio arquitectónico principal

> **El dominio y las reglas del negocio determinan la arquitectura; la tecnología implementa esas decisiones.**

No se debe comenzar diseñando la aplicación alrededor de:

```text
Controllers
Tables
React components
Repositories
Endpoints
```

Primero se entiende el negocio.

Después se decide cómo representarlo técnicamente.

---

# 3. Arquitectura general

El sistema estará compuesto inicialmente por dos aplicaciones principales:

```text
┌───────────────────────────────────────────────┐
│                  FRONTEND                     │
│                                               │
│ React + Tailwind CSS                          │
│                                               │
│ UI / Pages / Components / Services / Auth     │
└──────────────────────┬────────────────────────┘
                       │
                       │ HTTP / JSON
                       │ REST API
                       ▼
┌───────────────────────────────────────────────┐
│                  BACKEND                      │
│                                               │
│ ASP.NET Core Web API                          │
│                                               │
│ API → Application → Domain → Infrastructure   │
└──────────────────────┬────────────────────────┘
                       │
                       │ EF Core
                       ▼
┌───────────────────────────────────────────────┐
│                  SQL SERVER                   │
│                                               │
│ Persistencia de datos                         │
└───────────────────────────────────────────────┘
```

---

# 4. Estructura del proyecto

La estructura raíz será:

```text
AuropaqPedidos/
│
├── backend/
│
├── frontend/
│
├── docs/
│
├── CLAUDE.md
│
└── README.md
```

La carpeta `docs` contiene la documentación que representa la fuente de verdad del proyecto.

---

# 5. Backend

El backend utilizará:

```text
C#
.NET
ASP.NET Core Web API
Entity Framework Core
SQL Server
```

La responsabilidad principal del backend es:

- Exponer la API.
- Ejecutar casos de uso.
- Aplicar reglas de negocio.
- Validar operaciones.
- Gestionar autorización.
- Persistir información.
- Mantener consistencia.
- Registrar errores y eventos relevantes.

---

# 6. Arquitectura interna del backend

Se utilizará inicialmente una separación por responsabilidades:

```text
backend/
└── src/
    ├── Domain/
    ├── Application/
    ├── Infrastructure/
    └── Api/
```

Conceptualmente:

```text
┌───────────────────────┐
│          API          │
│ Controllers           │
│ Middleware            │
│ Authentication        │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│     APPLICATION       │
│ Use Cases             │
│ Commands / Queries    │
│ DTOs                  │
│ Application Services  │
└───────────┬───────────┘
            │
            ▼
┌───────────────────────┐
│        DOMAIN         │
│ Entities              │
│ Business Rules        │
│ Value Objects*        │
│ Domain Exceptions     │
│ Domain Behavior       │
└───────────▲───────────┘
            │
            │
┌───────────┴───────────┐
│    INFRASTRUCTURE     │
│ EF Core               │
│ DbContext             │
│ Configurations        │
│ Persistence           │
│ External integrations │
└───────────────────────┘
```

`Value Objects` se utilizarán únicamente cuando realmente aporten valor al dominio. No deben crearse por obligación.

---

# 7. Capa Domain

## Responsabilidad

Representa las reglas y conceptos centrales del negocio.

Aquí deben vivir los elementos que representan el comportamiento del dominio.

Ejemplos:

```text
Empresa
Sede
Producto
Requisición
DetalleRequisición
Consolidación
PedidoProveedor
Entrega
```

También pueden existir:

```text
Estados
Reglas
Excepciones de dominio
Value Objects
```

cuando estén justificados.

---

# 8. Regla para Domain

La capa Domain no debe depender de:

```text
ASP.NET Core
Entity Framework Core
SQL Server
React
HTTP
Controllers
```

El dominio debe poder entenderse sin conocer la infraestructura.

Esto permite que las reglas del negocio sean más fáciles de probar y mantener.

---

# 9. Entidades de dominio

Una entidad debe representar un concepto con identidad y responsabilidad dentro del negocio.

Ejemplo:

```text
Requisición
```

no existe simplemente porque SQL necesita una tabla.

Existe porque representa una necesidad interna concreta de una empresa durante un periodo.

---

# 10. Comportamiento de las entidades

Las entidades no deben convertirse en simples contenedores de propiedades cuando exista comportamiento real que pertenezca al dominio.

Ejemplo conceptual:

```text
Requisición.Enviar()
```

puede representar la intención de negocio de enviar una requisición.

Antes de realizar la acción deben cumplirse las reglas correspondientes.

Por ejemplo:

```text
Requisición debe estar en BORRADOR
Debe existir información requerida
Debe cumplir las reglas de distribución
Debe estar dentro de la ventana permitida
```

La implementación exacta se definirá durante el desarrollo.

---

# 11. Capa Application

## Responsabilidad

La capa Application coordina los casos de uso del sistema.

No debe convertirse en una copia de los Controllers ni en un lugar para colocar arbitrariamente cualquier lógica.

Ejemplos de casos de uso:

```text
Crear requisición
Obtener requisición
Agregar producto
Modificar cantidad
Eliminar detalle
Distribuir producto por sede
Enviar requisición
Aprobar requisición
Devolver requisición
Corregir requisición
Reenviar requisición
```

---

# 12. Application y Domain

La capa Application coordina el proceso.

El Domain representa las reglas y comportamiento del negocio.

Conceptualmente:

```text
Application
     │
     │ ejecuta caso de uso
     ▼
Domain
     │
     │ valida comportamiento
     ▼
Resultado
```

La Application no debe duplicar innecesariamente reglas que pertenecen al Domain.

---

# 13. DTOs

Los DTOs se utilizarán para definir los contratos de entrada y salida de la API.

Ejemplo conceptual:

```text
CrearRequisicionRequest
AgregarDetalleRequisicionRequest
ActualizarDetalleRequisicionRequest
DistribuirRequisicionRequest
RequisicionResponse
```

Los DTOs no deben convertirse automáticamente en entidades de dominio.

El sistema debe transformar explícitamente la información cuando corresponda.

---

# 14. Capa Infrastructure

## Responsabilidad

Implementa los detalles técnicos externos al dominio.

Incluye inicialmente:

```text
Entity Framework Core
DbContext
Configuraciones EF
Persistencia
Migraciones
Acceso a SQL Server
```

Posteriormente puede contener:

```text
Integraciones externas
Servicios de correo
Servicios de almacenamiento
Proveedores externos
```

cuando realmente sean necesarios.

---

# 15. Entity Framework Core

Entity Framework Core será utilizado como mecanismo de persistencia.

Su responsabilidad será:

```text
Domain
   ↓
Infrastructure
   ↓
EF Core
   ↓
SQL Server
```

Las configuraciones de persistencia no deben contaminar innecesariamente las reglas del dominio.

---

# 16. DbContext

El `DbContext` pertenece a Infrastructure.

No debe utilizarse directamente desde:

```text
Controllers
```

La capa API no debe conocer detalles internos de EF Core.

---

# 17. Repositorios

No se creará automáticamente un repositorio genérico para todas las entidades.

La necesidad de repositorios deberá evaluarse según los casos de uso reales.

No se utilizará:

```text
GenericRepository<T>
```

simplemente porque sea un patrón conocido.

Si un caso de uso requiere una abstracción específica de persistencia, esta se diseñará según la necesidad.

---

# 18. CQRS

No se implementará CQRS completo inicialmente.

Los casos de uso podrán distinguir conceptualmente entre:

```text
Commands
```

y:

```text
Queries
```

cuando mejore la claridad.

Sin embargo, no se introducirá infraestructura compleja de CQRS, Mediator u otros frameworks sin necesidad real.

---

# 19. API

La API será una:

```text
REST API
```

utilizando:

```text
HTTP
JSON
```

Los Controllers serán responsables principalmente de:

- Recibir solicitudes HTTP.
- Validar estructura básica de entrada.
- Invocar casos de uso.
- Transformar resultados en respuestas HTTP.

No deben contener reglas de negocio complejas.

---

# 20. Controllers

Los Controllers deben mantenerse delgados.

Ejemplo conceptual:

```text
HTTP Request
      ↓
Controller
      ↓
Application
      ↓
Domain
      ↓
Infrastructure
```

No se debe implementar lógica de negocio importante directamente dentro del Controller.

Debe evitarse código como:

```text
if (...)
{
    // decenas de líneas de reglas de negocio
}
```

cuando dicha lógica pertenece al dominio o al caso de uso.

---

# 21. Endpoints orientados a acciones

Cuando una operación represente una acción de negocio específica, se podrán utilizar endpoints explícitos.

Ejemplos:

```text
POST /api/requisiciones/{id}/enviar

POST /api/requisiciones/{id}/aprobar

POST /api/requisiciones/{id}/devolver
```

Esto expresa mejor la intención de negocio que un endpoint genérico como:

```text
PUT /api/requisiciones/{id}/estado
```

La definición completa de endpoints corresponde a:

```text
docs/05-api.md
```

---

# 22. Frontend

El frontend utilizará:

```text
React
Tailwind CSS
```

Su responsabilidad principal es:

- Presentar información.
- Recibir interacción del usuario.
- Gestionar estado de interfaz.
- Consumir la API.
- Mostrar errores.
- Facilitar validaciones de experiencia de usuario.
- Gestionar navegación y sesión.

El frontend no es la fuente de verdad del negocio.

---

# 23. Estructura conceptual del frontend

Inicialmente:

```text
frontend/
└── src/
    ├── components/
    ├── pages/
    ├── services/
    ├── hooks/
    ├── auth/
    ├── types/
    └── routes/
```

La estructura podrá evolucionar cuando el proyecto crezca.

No se deben crear carpetas o abstracciones únicamente por seguir una plantilla.

---

# 24. Comunicación Frontend → Backend

La comunicación seguirá:

```text
React
   ↓
HTTP
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

La respuesta seguirá el camino inverso.

El frontend no debe acceder directamente a SQL Server.

---

# 25. Validaciones

Existirán dos niveles de validación.

## Frontend

Su objetivo principal es mejorar la experiencia del usuario.

Ejemplos:

```text
Campo requerido
Formato incorrecto
Cantidad inválida
Formulario incompleto
```

## Backend

Es la fuente definitiva de validación de las reglas de negocio.

Ejemplos:

```text
La requisición ya fue enviada.
La sede no pertenece a la empresa.
La distribución no coincide con la cantidad solicitada.
La ventana de solicitud está cerrada.
El usuario no tiene autorización.
```

---

# 26. Autenticación y autorización

El sistema debe diferenciar:

```text
Autenticación
¿Quién es el usuario?
```

de:

```text
Autorización
¿Qué puede hacer?
¿Sobre qué información puede hacerlo?
```

La seguridad detallada se definirá en:

```text
docs/06-seguridad.md
```

No se deben implementar roles o permisos definitivos antes de definir dicho documento.

---

# 27. Manejo de errores

La API debe proporcionar un mecanismo consistente para manejar errores.

Se debe evitar que cada Controller implemente su propio mecanismo de manejo de excepciones.

Se utilizará un mecanismo centralizado para transformar errores internos en respuestas HTTP apropiadas.

La implementación específica será definida durante el desarrollo de la API.

---

# 28. Middleware

ASP.NET Core Middleware podrá utilizarse para responsabilidades transversales como:

```text
Manejo global de excepciones
Logging
Autenticación
Autorización
Correlación de solicitudes
```

No debe utilizarse Middleware para implementar reglas específicas de una requisición o producto.

---

# 29. Logging

El sistema debe registrar información técnica suficiente para diagnosticar problemas.

El logging no debe utilizarse como sustituto de la auditoría de negocio.

Existe una diferencia entre:

```text
LOG TÉCNICO
```

y:

```text
AUDITORÍA DE NEGOCIO
```

Ejemplo de log técnico:

```text
Error ejecutando consulta SQL.
```

Ejemplo de auditoría:

```text
Usuario X devolvió la requisición 125
por motivo Y.
```

---

# 30. Auditoría

La auditoría forma parte del comportamiento transversal del sistema.

Debe permitir conservar acciones relevantes como:

```text
Creación
Envío
Devolución
Corrección
Aprobación
Cambios importantes
```

La implementación exacta se definirá en conjunto con el modelo de datos.

---

# 31. Dependencias entre capas

La dirección de dependencias debe mantenerse controlada.

Conceptualmente:

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↓
Application / Domain
```

El objetivo es evitar que:

```text
Domain
```

dependa directamente de:

```text
Infrastructure
```

o:

```text
SQL Server
```

---

# 32. Regla de dependencia

Una tecnología externa no debe determinar cómo funciona el negocio.

Por ejemplo:

```text
EF Core
```

no debe decidir qué significa:

```text
"Enviar una requisición"
```

La regla debe provenir del dominio y del negocio.

EF Core únicamente proporciona persistencia.

---

# 33. Transacciones

Las operaciones que modifiquen varias partes relacionadas del dominio deberán ejecutarse de forma consistente.

Ejemplo conceptual:

```text
Enviar requisición
        ↓
Cambiar estado
        ↓
Registrar historial
        ↓
Guardar cambios
```

Si la operación requiere consistencia transaccional, debe garantizarse que no quede parcialmente aplicada.

La estrategia concreta se definirá en Infrastructure.

---

# 34. Concurrencia

El sistema debe considerar que dos usuarios pueden intentar modificar información simultáneamente.

Especialmente en procesos como:

```text
Revisión
Aprobación
Modificación
Consolidación
Entrega
```

No se debe asumir que solamente existe un usuario conectado.

La estrategia de concurrencia se definirá cuando el caso de uso lo requiera.

---

# 35. Escalabilidad

El sistema debe poder crecer en:

```text
Empresas
Sedes
Usuarios
Productos
Proveedores
Requisiciones
Pedidos
Entregas
```

La escalabilidad inicial será principalmente mediante una arquitectura modular y un modelo de datos correctamente diseñado.

No se introducirán microservicios únicamente para anticipar crecimiento.

---

# 36. Monolito modular

La primera versión utilizará una arquitectura de aplicación modular dentro de un sistema centralizado.

Conceptualmente:

```text
             AuropaqPedidos
                    │
       ┌────────────┼────────────┐
       │            │            │
 Organización    Catálogo    Requisiciones
       │            │            │
       └────────────┼────────────┘
                    │
                Compras
                    │
                 Entregas
```

Esto permite separar responsabilidades sin introducir la complejidad operacional de múltiples servicios.

---

# 37. Microservicios

Los microservicios quedan fuera del alcance inicial.

No se implementarán solamente por razones como:

```text
"Es más escalable"
"Es arquitectura moderna"
"Es más profesional"
```

Una futura separación en servicios solamente deberá realizarse cuando exista una necesidad real y demostrable.

---

# 38. Docker

Docker no forma parte de la primera implementación funcional del MVP.

Podrá incorporarse posteriormente para:

- Entornos consistentes.
- Desarrollo.
- CI/CD.
- Despliegue.

No debe introducirse antes de que exista una necesidad concreta.

---

# 39. Redis y caché

Redis y otros mecanismos de caché no forman parte del MVP inicial.

La caché solamente deberá introducirse cuando exista:

1. Una necesidad de rendimiento identificada.
2. Una operación susceptible de cachearse.
3. Una estrategia clara de invalidación.
4. Una medición que justifique la optimización.

No se optimizará prematuramente.

---

# 40. Integraciones externas

Las integraciones con plataformas de proveedores se consideran parte de una etapa posterior.

La arquitectura debe permitir incorporarlas posteriormente sin obligar a modificar el dominio central innecesariamente.

Ejemplo futuro:

```text
AuropaqPedidos
       ↓
Adaptador / Integración
       ↓
Plataforma proveedor
```

Cada integración deberá aislar las particularidades del proveedor.

---

# 41. Seguridad como responsabilidad transversal

La seguridad no será responsabilidad exclusiva del frontend.

El backend debe garantizar:

```text
Autenticación
Autorización
Validación de permisos
Validación de alcance
Protección de datos
```

El frontend únicamente adapta la interfaz según los permisos conocidos, pero el backend debe volver a verificar las operaciones.

---

# 42. Pruebas

El proyecto debe utilizar diferentes niveles de pruebas.

## Pruebas unitarias

Para reglas y comportamiento aislado.

Ejemplo:

```text
Una distribución que supera la cantidad solicitada debe ser rechazada.
```

## Pruebas de integración

Para verificar interacción entre componentes reales.

Ejemplo:

```text
Application
+
EF Core
+
SQL Server
```

## Pruebas de API

Para verificar contratos y comportamiento HTTP.

## Pruebas frontend

Para comportamientos importantes de interfaz cuando resulte necesario.

---

# 43. Regla sobre pruebas

Una funcionalidad no se considera completamente implementada únicamente porque compile.

Debe existir evidencia de que funciona correctamente.

Conceptualmente:

```text
Código
  +
Reglas
  +
Pruebas
  =
Funcionalidad terminada
```

---

# 44. Organización por funcionalidades

Aunque el backend tenga capas técnicas, las funcionalidades deben poder identificarse claramente.

Ejemplo:

```text
Application/
├── Requisiciones/
├── Empresas/
├── Sedes/
├── Productos/
└── Periodos/
```

Esto facilita que Claude Code trabaje por tareas y reduzca modificaciones accidentales en otras áreas.

---

# 45. Regla contra sobreingeniería

No se deben introducir automáticamente:

```text
CQRS completo
MediatR
Event Sourcing
Microservicios
Redis
Kafka
Service Mesh
Arquitectura distribuida
```

si el problema actual no lo requiere.

La arquitectura debe ser:

```text
Simple
Clara
Modular
Testeable
Evolutiva
```

---

# 46. Evolución de la arquitectura

La arquitectura inicial no debe considerarse inmutable.

Puede evolucionar cuando aparezcan nuevas necesidades.

Sin embargo, un cambio arquitectónico debe:

1. Tener una razón.
2. Identificar el problema que resuelve.
3. Analizar alternativas.
4. Evaluar consecuencias.
5. Documentarse en:

```text
docs/07-decisiones-arquitectura.md
```

6. Ser aprobado antes de implementarse si modifica decisiones fundamentales.

---

# 47. Flujo de una operación

Ejemplo: enviar una requisición.

```text
React
  │
  │ POST /api/requisiciones/{id}/enviar
  ▼
Controller
  │
  ▼
Application
  │
  ├── Identifica usuario
  ├── Verifica autorización
  ├── Obtiene requisición
  │
  ▼
Domain
  │
  ├── Verifica estado
  ├── Verifica reglas
  ├── Ejecuta transición
  │
  ▼
Infrastructure
  │
  ├── Persiste cambios
  └── Registra historial
  │
  ▼
Application
  │
  ▼
API
  │
  ▼
React
```

La implementación concreta puede variar, pero las responsabilidades deben mantenerse separadas.

---

# 48. Flujo de una consulta

Ejemplo:

```text
GET /api/requisiciones/{id}
```

Conceptualmente:

```text
React
   ↓
Controller
   ↓
Application
   ↓
Consulta / Persistencia
   ↓
Infrastructure
   ↓
SQL Server
   ↓
Application
   ↓
DTO
   ↓
API
   ↓
React
```

Las consultas no deben cargar innecesariamente todo el dominio si únicamente se necesita información de lectura.

La estrategia concreta se definirá según el caso de uso.

---

# 49. Responsabilidad de cada capa

| Capa           | Responsabilidad principal           |
| -------------- | ----------------------------------- |
| Frontend       | Interfaz y experiencia de usuario   |
| API            | HTTP y contratos externos           |
| Application    | Casos de uso y coordinación         |
| Domain         | Reglas y comportamiento del negocio |
| Infrastructure | Persistencia e integraciones        |
| SQL Server     | Almacenamiento                      |

---

# 50. Regla de oro de arquitectura

Antes de colocar código en una capa, debe poder responderse:

> **¿Por qué esta responsabilidad pertenece aquí?**

Si la respuesta es únicamente:

```text
"Porque siempre se hace así"
```

no es suficiente.

Debe existir una razón relacionada con:

- Responsabilidad.
- Dependencias.
- Mantenibilidad.
- Testabilidad.
- Dominio.

---

# 51. Lo que Claude Code debe hacer antes de implementar

Antes de modificar código, Claude Code debe:

1. Leer `CLAUDE.md`.
2. Leer la tarea correspondiente.
3. Leer las reglas de negocio relacionadas.
4. Leer el dominio relacionado.
5. Revisar esta arquitectura.
6. Inspeccionar el código existente.
7. Identificar los archivos que necesita modificar.
8. Identificar los nuevos archivos que necesita crear.
9. Identificar las pruebas necesarias.
10. Informar cualquier contradicción o ambigüedad.

No debe comenzar a modificar archivos sin comprender el contexto.

---

# 52. Lo que Claude Code NO debe hacer

Claude Code no debe:

- Cambiar la arquitectura por iniciativa propia.
- Introducir microservicios.
- Crear repositorios genéricos sin necesidad.
- Introducir CQRS completo sin autorización.
- Agregar frameworks innecesarios.
- Agregar librerías externas sin autorización.
- Crear capas adicionales sin justificación.
- Mover archivos únicamente por preferencias personales.
- Realizar refactorizaciones no relacionadas con la tarea.
- Implementar funcionalidades fuera del alcance.
- Colocar reglas de negocio importantes dentro de Controllers.
- Confiar exclusivamente en validaciones del frontend.
- Acceder a SQL Server directamente desde React.
- Hardcodear empresas, sedes, usuarios o productos.

---

# 53. Arquitectura objetivo inicial

La arquitectura inicial puede resumirse así:

```text
                    USUARIO
                       │
                       ▼
                  REACT + UI
                       │
                    HTTP/JSON
                       │
                       ▼
              ASP.NET CORE API
                       │
                       ▼
                 APPLICATION
                       │
                       ▼
                    DOMAIN
                       │
                       ▼
                INFRASTRUCTURE
                       │
                    EF CORE
                       │
                       ▼
                  SQL SERVER
```

Con responsabilidades transversales:

```text
          ┌────────────────────────────┐
          │ Seguridad                  │
          │ Logging                    │
          │ Auditoría                  │
          │ Manejo de excepciones      │
          │ Validaciones               │
          └────────────────────────────┘
```

---

# 54. Regla final

> **La arquitectura debe reducir la complejidad del sistema, no aumentarla.**

La mejor arquitectura para AuropaqPedidos no será la que utilice más patrones o tecnologías.

Será la que permita:

```text
Entender el negocio
        ↓
Modificar una funcionalidad
        ↓
Sin romper otras
        ↓
Probarla
        ↓
Desplegarla
        ↓
Y continuar evolucionando
```

sin introducir complejidad innecesaria.
