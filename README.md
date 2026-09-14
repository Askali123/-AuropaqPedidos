# Auropaq Pedidos

Sistema web para la gestión, consolidación y seguimiento de requisiciones mensuales de productos de aseo, cafetería, papelería y botiquín para las empresas pertenecientes a Auropaq S.A.S.

El sistema busca reemplazar progresivamente el proceso manual basado en archivos Excel, correos electrónicos y registros independientes, proporcionando una solución centralizada, trazable y escalable.

---

## 1. Objetivo del proyecto

Digitalizar y centralizar el proceso mediante el cual las empresas de la organización:

1. Registran sus necesidades mensuales.
2. Seleccionan productos del catálogo.
3. Solicitan productos que aún no estén catalogados.
4. Distribuyen las cantidades solicitadas entre sus diferentes sedes.
5. Envían la requisición para revisión.
6. Reciben observaciones o aprobación.
7. Permiten consolidar las necesidades de varias empresas.
8. Generan pedidos a proveedores.
9. Distribuyen los productos comprados hacia las sedes correspondientes.
10. Registran entregas parciales o completas.
11. Registran y validan facturas.
12. Mantienen trazabilidad de todo el proceso.

El objetivo no es solamente digitalizar el Excel actual, sino construir una base tecnológica que pueda evolucionar conforme crezcan las empresas, usuarios, sedes, productos y reglas del negocio.

---

# 2. Alcance inicial

El MVP se concentra inicialmente en el proceso de **requisición mensual**.

### Incluye

- Empresas.
- Sedes.
- Usuarios.
- Roles.
- Permisos.
- Categorías.
- Unidades de medida.
- Productos.
- Periodos mensuales.
- Ventanas de solicitud.
- Creación de requisiciones.
- Detalles de requisición.
- Distribución por sede.
- Guardado como borrador.
- Envío de requisiciones.
- Revisión.
- Aprobación.
- Devolución para corrección.
- Corrección y reenvío.
- Historial de la requisición.

### Evolución posterior

Después de estabilizar el MVP se incorporarán:

- Consolidación de requisiciones.
- Pedidos a proveedores.
- Relación producto/proveedor.
- Distribución de pedidos.
- Entregas.
- Entregas parciales.
- Cantidades pendientes.
- Facturación.
- Cierre del proceso.
- Auditoría.
- Notificaciones.
- Reportes.
- Mejoras de rendimiento.
- Despliegue productivo.

> **Nota (refinamiento 2026-09-11):** esta lista describe el plan original de evolución tras el primer MVP. Varias de estas piezas (Consolidación, Pedidos a proveedor, Entregas, Facturación) ya tienen una primera implementación técnica, con decisiones de negocio todavía pendientes de aprobación formal. El alcance funcional vigente y lo que queda explícitamente fuera de alcance (contabilidad, ERP, etc.) están consolidados en `docs/00-contexto-proyecto.md §9.1/§9.2`; el estado real capa por capa está en `progreso.md`.

---

# 3. Flujo general del negocio

El flujo principal del sistema es:

```text
Empresa
   │
   └── Sede(s)
          │
          └── Usuario
                 │
                 ▼
              Periodo
                 │
                 ▼
          Requisición mensual
                 │
                 ├── Producto
                 │
                 ├── Cantidad
                 │
                 └── Distribución por sede
                         │
                         ▼
                      Envío
                         │
                         ▼
                      Revisión
                    /          \
                   /            \
             Aprobada         Devuelta
                │                 │
                │                 ▼
                │              Corrección
                │                 │
                │                 ▼
                │              Reenvío
                │
                ▼
          Consolidación
                │
                ▼
       Pedido a proveedor
                │
                ▼
             Entrega
                │
                ▼
             Factura
                │
                ▼
              Cierre
```

Las etapas posteriores a la requisición serán desarrolladas progresivamente.

---

# 4. Arquitectura

El sistema utilizará inicialmente una arquitectura de **monolito modular**, evitando introducir complejidad innecesaria durante las primeras etapas.

## Backend

- C#
- .NET
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

## Frontend

- React
- TypeScript
- Tailwind CSS

## Control de versiones

- Git

---

# 5. Arquitectura lógica

La comunicación principal será:

```text
┌─────────────────────────────┐
│          React              │
│       TypeScript            │
│       Tailwind CSS          │
└──────────────┬──────────────┘
               │
               │ HTTP / JSON
               ▼
┌─────────────────────────────┐
│     ASP.NET Core Web API    │
│        Controllers          │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│        Application          │
│       Use Cases / DTOs      │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│           Domain            │
│ Entities / Rules / States   │
└──────────────┬──────────────┘
               ▼
┌─────────────────────────────┐
│       Infrastructure        │
│      EF Core / SQL Server   │
└─────────────────────────────┘
```

El dominio no debe depender directamente de ASP.NET Core, EF Core o SQL Server.

---

# 6. Estructura del proyecto

La estructura conceptual inicial es:

```text
AuropaqPedidos/
│
├── backend/
│   └── src/
│       ├── Domain/
│       │   ├── Entities/
│       │   ├── Enums/
│       │   ├── Rules/
│       │   └── Exceptions/
│       │
│       ├── Application/
│       │   ├── Requisiciones/
│       │   ├── Empresas/
│       │   ├── Productos/
│       │   └── Periodos/
│       │
│       ├── Infrastructure/
│       │   ├── Persistence/
│       │   ├── Configurations/
│       │   └── Repositories/
│       │
│       └── Api/
│           ├── Controllers/
│           ├── Middleware/
│           └── Program.cs
│
├── frontend/
│   └── src/
│       ├── components/
│       ├── pages/
│       ├── services/
│       ├── hooks/
│       ├── auth/
│       ├── types/
│       └── routes/
│
├── docs/
│   ├── 00-contexto-proyecto.md
│   ├── 01-reglas-negocio.md
│   ├── 02-dominio.md
│   ├── 03-arquitectura.md
│   ├── 04-base-datos.md
│   ├── 05-api.md
│   ├── 06-seguridad.md
│   ├── 07-decisiones-arquitectura.md
│   └── 08-tareas.md
│
├── CLAUDE.md
└── README.md
```

> La estructura física definitiva debe respetar la solución existente. Antes de crear o modificar proyectos, archivos o carpetas se debe inspeccionar el repositorio actual.

---

# 7. Documentación del proyecto

La documentación se encuentra en `/docs`.

| Documento                       | Propósito                                                        |
| ------------------------------- | ---------------------------------------------------------------- |
| `00-contexto-proyecto.md`       | Contexto general y problema que se quiere resolver               |
| `01-reglas-negocio.md`          | Reglas que gobiernan el comportamiento del sistema               |
| `02-dominio.md`                 | Entidades, relaciones, responsabilidades y conceptos del negocio |
| `03-arquitectura.md`            | Arquitectura técnica y responsabilidades de cada capa            |
| `04-base-datos.md`              | Modelo de datos, tablas, relaciones e integridad                 |
| `05-api.md`                     | Contratos y endpoints de la API                                  |
| `06-seguridad.md`               | Autenticación, autorización, permisos y alcance                  |
| `07-decisiones-arquitectura.md` | Decisiones arquitectónicas y sus razones                         |
| `08-tareas.md`                  | Backlog técnico y orden de implementación                        |

Además:

```text
CLAUDE.md
```

define las reglas que debe seguir Claude Code al analizar, modificar e implementar el proyecto.

---

# 8. Principios fundamentales

## 8.1 El negocio primero

Las decisiones de negocio deben definirse antes de implementar código.

```text
Problema
   ↓
Regla de negocio
   ↓
Dominio
   ↓
Modelo de datos
   ↓
Caso de uso
   ↓
API
   ↓
Frontend
   ↓
Pruebas
```

La tecnología no debe determinar artificialmente el comportamiento del negocio.

---

## 8.2 Backend como fuente de verdad

Las validaciones críticas y reglas de negocio deben ejecutarse en el backend.

El frontend puede validar datos para mejorar la experiencia del usuario, pero nunca debe considerarse la autoridad final.

---

## 8.3 Configuración en lugar de hardcode

Las empresas, sedes, usuarios, roles, permisos, productos y demás elementos configurables deben manejarse como datos.

No se deben crear condiciones como:

```csharp
if (empresa == "AUROTECH")
{
    // ...
}
```

ni:

```csharp
if (usuario == "Martha")
{
    // ...
}
```

El sistema debe funcionar mediante entidades, relaciones, permisos y reglas configurables.

---

## 8.4 Separar permiso y alcance

El sistema diferencia:

```text
Permiso
    ↓
¿Qué puede hacer?

Alcance
    ↓
¿Sobre qué información puede hacerlo?
```

Por ejemplo, un usuario puede tener permiso para consultar requisiciones, pero solamente las pertenecientes a su empresa o a determinadas sedes.

---

## 8.5 Requisición ≠ pedido al proveedor

Una requisición representa una **necesidad interna**.

Un pedido al proveedor representa una **compra realizada**.

Por lo tanto:

```text
Requisición
    ↓
Necesidad interna

Consolidación
    ↓
Necesidad total

Pedido proveedor
    ↓
Compra

Entrega
    ↓
Recepción física
```

Estas etapas no deben mezclarse.

---

# 9. Requisición mensual

Cada empresa puede tener como máximo una requisición mensual para un periodo determinado.

Conceptualmente:

```text
Empresa + Periodo
       ↓
1 Requisición
       ↓
muchos Detalles
       ↓
cada detalle puede distribuirse
entre una o varias Sedes
```

Una empresa con varias sedes no necesita crear una requisición independiente por cada sede.

---

# 10. Estados de la requisición

La requisición seguirá un flujo controlado.

```text
BORRADOR
   │
   ▼
ENVIADA
   │
   ▼
EN REVISION
   │
   ├───────────────┐
   │               │
   ▼               ▼
APROBADA        DEVUELTA
                   │
                   ▼
               CORRECCIÓN
                   │
                   ▼
                ENVIADA
```

No se debe modificar silenciosamente una requisición enviada.

Cuando una requisición sea devuelta para corrección, se conserva la misma requisición y su historial.

---

# 11. Productos no catalogados

No todos los productos solicitados necesariamente existirán en el catálogo oficial.

El flujo será:

```text
Usuario solicita producto
        │
        ▼
¿Existe en catálogo?
      /     \
    Sí       No
    │         │
    │         ▼
    │    Solicitud de
    │    producto no catalogado
    │         │
    │         ▼
    │      Revisión
    │      /  |  \
    │     /   |   \
    │    ▼    ▼    ▼
    │ Homologar Crear Rechazar
    │
    ▼
Requisición
```

No se debe contaminar la entidad `Producto` con información incompleta solamente para permitir solicitudes desconocidas.

---

# 12. Trazabilidad

El sistema debe permitir responder preguntas como:

- ¿Quién creó la requisición?
- ¿Cuándo fue creada?
- ¿Cuándo fue enviada?
- ¿Quién la revisó?
- ¿Fue devuelta?
- ¿Por qué fue devuelta?
- ¿Quién la aprobó?
- ¿Qué productos fueron solicitados?
- ¿Qué cantidades fueron solicitadas?
- ¿A qué sede estaban destinadas?
- ¿Qué cantidad terminó comprándose?
- ¿Qué cantidad fue entregada?
- ¿Qué cantidad sigue pendiente?
- ¿Qué proveedor realizó la entrega?
- ¿Qué factura corresponde al proceso?

La trazabilidad debe estar almacenada en el sistema y no depender de archivos personales externos.

---

# 13. Seguridad

El sistema deberá aplicar:

- Autenticación.
- Autorización.
- Roles.
- Permisos.
- Alcance por empresa.
- Alcance por sede.
- Validación de acceso al recurso.
- Principio de mínimo privilegio.
- Protección contra manipulación de identificadores.
- Protección de secretos.
- HTTPS en ambientes correspondientes.
- Registro de operaciones críticas.

Nunca se debe confiar en que un `id`, `empresaId`, `sedeId`, `rol` o permiso enviado desde el frontend sea válido únicamente porque llegó en la petición.

---

# 14. API

La API utilizará:

```text
/api/v1/
```

Los endpoints deben representar casos de uso del negocio.

Ejemplo:

```http
POST /api/v1/requisiciones/{id}/enviar
POST /api/v1/requisiciones/{id}/aprobar
POST /api/v1/requisiciones/{id}/devolver
```

En lugar de depender exclusivamente de operaciones genéricas como:

```http
PUT /api/v1/requisiciones/{id}/estado
```

Las acciones importantes deben representar explícitamente la intención del negocio.

---

# 15. Base de datos

La persistencia utilizará:

```text
SQL Server
     │
     ▼
Entity Framework Core
     │
     ▼
ASP.NET Core
```

Las modificaciones del esquema deberán gestionarse mediante migraciones de Entity Framework Core.

Las relaciones, restricciones, índices y reglas de integridad deberán diseñarse de acuerdo con el dominio definido en la documentación.

---

# 16. Pruebas

Las funcionalidades importantes deben acompañarse de pruebas.

Dependiendo del caso se utilizarán:

- Pruebas unitarias.
- Pruebas de integración.
- Pruebas de API.
- Pruebas de frontend.

Especialmente deben probarse:

- Reglas de negocio.
- Cambios de estado.
- Permisos.
- Alcance por empresa/sede.
- Distribución de cantidades.
- Productos no catalogados.
- Creación de requisiciones mensuales.
- Envío.
- Aprobación.
- Devolución.
- Corrección y reenvío.

---

# 17. Cómo trabajar en el proyecto

El desarrollo seguirá el siguiente principio:

> **Nosotros decidimos el negocio y la arquitectura. Claude Code implementa, prueba y reporta.**

Antes de realizar cambios importantes, se debe:

1. Leer `CLAUDE.md`.
2. Revisar la tarea correspondiente en `docs/08-tareas.md`.
3. Revisar las reglas de negocio relacionadas.
4. Revisar el dominio involucrado.
5. Revisar las decisiones arquitectónicas relacionadas.
6. Inspeccionar el código existente.
7. Definir el alcance del cambio.
8. Implementar únicamente lo solicitado.
9. Ejecutar las pruebas correspondientes.
10. Reportar los archivos modificados y los resultados.

---

# 18. Orden de implementación

El proyecto se desarrollará progresivamente.

### Fase 0 — Base

```text
TASK-001
TASK-002
TASK-003
TASK-004
TASK-005
```

### Fase 1 — Organización

```text
Empresa
Sede
Usuario
Roles
Permisos
Alcances
```

### Fase 2 — Catálogo

```text
Categoría
Unidad de medida
Producto
Producto no catalogado
Proveedor
Producto-Proveedor
```

### Fase 3 — Periodos

```text
Periodo
Ventana de solicitud
```

### Fase 4 — Requisiciones

```text
Crear requisición
Agregar productos
Modificar detalles
Eliminar detalles
Distribuir por sede
Validar
Guardar borrador
Enviar
Historial
```

### Fase 5 — Revisión

```text
Consultar pendientes
Consultar detalle
Aprobar
Devolver
Corregir y reenviar
```

### Fases posteriores

```text
Consolidación
     ↓
Pedido proveedor
     ↓
Distribución
     ↓
Entrega
     ↓
Entregas parciales
     ↓
Factura
     ↓
Cierre
```

El detalle de cada tarea se encuentra en:

```text
docs/08-tareas.md
```

---

# 19. MVP

El primer objetivo funcional es completar este recorrido:

```text
Login
  ↓
Empresa
  ↓
Sede
  ↓
Periodo
  ↓
Crear/continuar requisición
  ↓
Agregar productos
  ↓
Modificar cantidades
  ↓
Distribuir por sede
  ↓
Validar
  ↓
Guardar borrador
  ↓
Enviar
  ↓
Revisión
  ↓
Aprobar / Devolver
  ↓
Corregir y reenviar
```

Este flujo debe estar estable antes de avanzar hacia consolidaciones, proveedores y entregas.

---

# 20. Qué NO se implementará inicialmente

Para evitar sobreingeniería, no se introducirán inicialmente:

- Microservicios.
- CQRS completo.
- Redis.
- Kubernetes.
- Arquitectura distribuida.
- Event sourcing.
- Sistemas de mensajería complejos.
- Infraestructura innecesaria.
- Optimizaciones prematuras.
- Abstracciones genéricas sin necesidad.
- Repositorios genéricos obligatorios.

Estas tecnologías podrán evaluarse posteriormente si existe una necesidad real y documentada.

---

# 21. Evolución esperada

La arquitectura debe permitir evolucionar desde:

```text
MVP
  ↓
Sistema interno estable
  ↓
Más empresas
  ↓
Más sedes
  ↓
Más usuarios
  ↓
Más proveedores
  ↓
Mayor volumen
  ↓
Integraciones
  ↓
Automatización
```

La evolución debe realizarse sobre necesidades reales y no sobre complejidad hipotética.

---

# 22. Reglas para contribuir

Antes de realizar un cambio:

- Entender el problema.
- Revisar la documentación relacionada.
- Verificar si existe una regla de negocio.
- Verificar si existe una decisión arquitectónica.
- Revisar el código existente.
- Evitar modificar componentes no relacionados.
- No introducir dependencias sin justificación.
- No cambiar contratos existentes sin autorización.
- No eliminar código funcional sin una razón documentada.
- Agregar o actualizar pruebas cuando corresponda.

---

# 23. Commits

Los commits deben representar cambios coherentes y pequeños.

Ejemplos:

```text
feat: add monthly requisition draft
feat: add requisition detail management
feat: add requisition site distribution
feat: add requisition submission
feat: add requisition review
fix: validate requisition distribution quantities
test: add requisition domain tests
refactor: simplify requisition validation
docs: update requisition architecture
```

No se deben mezclar cambios funcionales independientes en un mismo commit sin necesidad.

---

# 24. Documentación como fuente de verdad

Cuando exista una duda sobre el comportamiento del sistema, se debe revisar en este orden:

```text
01-reglas-negocio.md
        ↓
02-dominio.md
        ↓
07-decisiones-arquitectura.md
        ↓
03-arquitectura.md
        ↓
04-base-datos.md
        ↓
05-api.md
        ↓
Código
```

Si existe una contradicción importante entre la documentación y el código, no se debe asumir automáticamente que el código tiene la razón.

La contradicción debe identificarse y resolverse antes de realizar cambios que puedan ampliar el problema.

---

# 25. Principio final

Este proyecto no debe construirse pensando únicamente en:

> "hacer que funcione".

Debe construirse pensando en:

```text
¿El problema está correctamente entendido?
        ↓
¿La regla de negocio está clara?
        ↓
¿El dominio representa correctamente el negocio?
        ↓
¿La información está correctamente modelada?
        ↓
¿La API representa los casos de uso?
        ↓
¿La seguridad está contemplada?
        ↓
¿La funcionalidad puede probarse?
        ↓
¿Puede evolucionar sin rehacer todo?
        ↓
Entonces implementar.
```

El objetivo es construir un sistema **claro, mantenible, escalable y defendible técnicamente**, evitando tanto el código improvisado como la sobreingeniería.

---

## Estado actual

**Proyecto:** Auropaq Pedidos
**Tipo:** Aplicación web empresarial
**Arquitectura:** Monolito modular
**Backend:** C# / .NET / ASP.NET Core
**Frontend:** React / TypeScript / Tailwind CSS
**Persistencia:** SQL Server / Entity Framework Core
**Control de versiones:** Git
**Estado:** Diseño y construcción progresiva del MVP

---

## Próximo objetivo

Completar el primer vertical slice funcional de requisiciones mensuales:

```text
Empresa
  ↓
Sede
  ↓
Usuario
  ↓
Periodo
  ↓
Requisición
  ↓
Detalle
  ↓
Distribución
  ↓
Envío
  ↓
Revisión
  ↓
Aprobación / Devolución
```

Una vez estabilizado este flujo, se continuará con la consolidación de necesidades y el proceso de compra a proveedores.
