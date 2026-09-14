# 07 — DECISIONES DE ARQUITECTURA

## 1. Propósito

Este documento registra las decisiones técnicas y arquitectónicas importantes del sistema de gestión de requisiciones y pedidos de Auropaq S.A.S.

Su objetivo es conservar el razonamiento detrás de las decisiones para:

- evitar decisiones contradictorias;
- reducir retrabajo;
- mantener coherencia entre backend, frontend y base de datos;
- facilitar la evolución del sistema;
- servir como referencia para desarrolladores y herramientas de IA como Claude Code;
- evitar introducir tecnologías o patrones innecesarios;
- permitir revisar una decisión cuando cambien los requisitos del negocio.

Una decisión registrada aquí representa el estado actual de la arquitectura.

Si una decisión cambia, no debe modificarse silenciosamente. Debe registrarse una nueva decisión o una revisión de la existente.

---

# 2. Principio general

Las decisiones de arquitectura deben seguir este orden:

```text
Problema de negocio
        ↓
Regla de negocio
        ↓
Modelo de dominio
        ↓
Decisión arquitectónica
        ↓
Modelo de datos
        ↓
API
        ↓
Implementación
```

La tecnología no debe determinar el modelo de negocio.

Primero se entiende el problema y después se decide cómo implementarlo.

---

# 3. Formato de las decisiones

Cada decisión importante debe documentarse utilizando:

- **ID**
- **Fecha**
- **Estado**
- **Contexto**
- **Decisión**
- **Motivo**
- **Alternativas consideradas**
- **Consecuencias**
- **Evolución futura**

Estados permitidos:

- `PROPUESTA`
- `ACEPTADA`
- `REEMPLAZADA`
- `DESCARTADA`

---

# 4. ADR-001 — Arquitectura modular monolítica

### Estado

`ACEPTADA`

### Contexto

El sistema inicialmente será desarrollado para gestionar las requisiciones, consolidación, pedidos y entregas de varias empresas pertenecientes a una misma organización.

Aunque el sistema debe ser escalable, actualmente no existe una necesidad real que justifique distribuir el sistema en múltiples servicios independientes.

### Decisión

Se utilizará una **arquitectura modular monolítica**.

El sistema se desarrollará como una aplicación principal con módulos claramente separados por responsabilidades de negocio.

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

### Motivo

Permite:

- mantener una arquitectura relativamente sencilla;
- desarrollar más rápido;
- facilitar el aprendizaje;
- mantener transacciones dentro de una misma aplicación;
- reducir complejidad operacional;
- mantener límites de dominio claros;
- permitir una futura separación de módulos si realmente se necesita.

### Alternativas consideradas

#### Microservicios

Descartados inicialmente porque introducirían:

- comunicación entre servicios;
- despliegues independientes;
- observabilidad distribuida;
- problemas de consistencia;
- mayor complejidad operacional.

#### Aplicación monolítica sin módulos

Descartada porque dificultaría mantener límites claros entre responsabilidades.

### Consecuencia

El sistema debe diseñarse desde el principio con módulos y responsabilidades claras, aunque físicamente se despliegue como una sola aplicación.

### Evolución futura

Si un módulo requiere escalar, desplegarse o evolucionar independientemente, podrá evaluarse su extracción como servicio independiente.

---

# 5. ADR-002 — Backend con ASP.NET Core Web API

### Estado

`ACEPTADA`

### Contexto

El sistema necesita una API para que el frontend pueda consultar y modificar información.

### Decisión

El backend utilizará:

- C#
- .NET
- ASP.NET Core Web API
- REST
- JSON

### Motivo

La tecnología permite construir APIs robustas, tipadas y adecuadas para aplicaciones empresariales.

Además, coincide con el stack tecnológico definido para el proyecto.

### Consecuencia

El backend será responsable de:

- reglas de negocio;
- validaciones;
- autorización;
- persistencia;
- procesamiento de casos de uso;
- control de estados;
- auditoría;
- exposición de APIs.

El frontend no será la fuente de verdad del negocio.

---

# 6. ADR-003 — React como frontend

### Estado

`ACEPTADA`

### Decisión

El frontend utilizará:

- React
- TypeScript
- Tailwind CSS

### Motivo

Permite construir una interfaz modular y mantener separación entre presentación y lógica del negocio.

### Consecuencia

React será responsable principalmente de:

- presentación;
- interacción con el usuario;
- navegación;
- manejo del estado de interfaz;
- consumo de la API;
- validaciones orientadas a experiencia de usuario.

Las reglas críticas continuarán en el backend.

---

# 7. ADR-004 — SQL Server como sistema gestor de base de datos

### Estado

`ACEPTADA`

### Decisión

La persistencia principal utilizará SQL Server.

El acceso desde .NET se realizará mediante Entity Framework Core.

### Motivo

El dominio contiene:

- relaciones entre empresas;
- sedes;
- usuarios;
- productos;
- requisiciones;
- pedidos;
- entregas;
- facturas;
- históricos.

Estas características hacen apropiado un modelo relacional.

### Consecuencia

Se priorizarán:

- claves primarias;
- claves foráneas;
- restricciones;
- índices;
- transacciones;
- integridad referencial.

---

# 8. ADR-005 — Entity Framework Core como ORM

### Estado

`ACEPTADA`

### Decisión

Entity Framework Core será utilizado como ORM principal.

Las modificaciones estructurales de la base de datos serán gestionadas mediante migraciones.

### Motivo

Permite mantener el modelo de dominio y persistencia integrado con C# y controlar la evolución del esquema.

### Consecuencia

Las entidades y configuraciones deberán mantenerse coherentes con el modelo de datos.

No se deben realizar cambios manuales en producción que queden fuera del mecanismo definido de migraciones, salvo una decisión explícita y documentada.

---

# 9. ADR-006 — El negocio no depende de EF Core

### Estado

`ACEPTADA`

### Decisión

El dominio no debe depender directamente de:

- Entity Framework Core;
- ASP.NET Core;
- SQL Server;
- React;
- infraestructura externa.

Conceptualmente:

```text
Domain
  ↓
Application
  ↓
Infrastructure
  ↓
SQL Server
```

El dominio debe contener las reglas y conceptos propios del negocio.

### Motivo

Esto permite:

- probar reglas de negocio sin depender de la base de datos;
- reducir acoplamiento;
- facilitar cambios tecnológicos;
- mantener claridad arquitectónica.

---

# 10. ADR-007 — Las reglas de negocio críticas viven en backend

### Estado

`ACEPTADA`

### Decisión

Toda regla que afecte la integridad del negocio debe validarse en el backend.

Ejemplos:

- una empresa solo puede tener una requisición por periodo;
- una requisición enviada no puede modificarse libremente;
- una distribución debe coincidir con la cantidad solicitada;
- una requisición fuera del periodo permitido no puede enviarse;
- un usuario no puede modificar información fuera de su alcance;
- una requisición devuelta puede corregirse y reenviarse;
- solamente usuarios autorizados pueden aprobar.

### Motivo

El frontend puede ser manipulado.

Por lo tanto:

```text
Frontend = experiencia de usuario
Backend = autoridad del negocio
```

### Consecuencia

Puede existir validación duplicada:

```text
React
 ↓
validación rápida para UX

Backend
 ↓
validación obligatoria
```

La validación del frontend nunca reemplaza la del backend.

---

# 11. ADR-008 — Empresas y sedes son datos, no código

### Estado

`ACEPTADA`

### Contexto

Actualmente existen varias empresas y pueden aparecer nuevas empresas en el futuro.

### Decisión

Las empresas y sedes serán administradas como información configurable.

No se deben escribir condiciones como:

```csharp
if (empresa == "AUROTECH")
{
}
```

Ni:

```csharp
if (empresaId == 1)
{
}
```

para representar reglas generales del sistema.

### Motivo

El sistema debe poder incorporar nuevas empresas sin modificar el código.

### Consecuencia

El comportamiento dependerá de:

- Empresa;
- Sede;
- Usuario;
- Rol;
- Permisos;
- Alcance.

---

# 12. ADR-009 — Permisos separados de roles

### Estado

`ACEPTADA`

### Decisión

Los roles no serán utilizados como única fuente de autorización.

Se utilizará el modelo:

```text
Usuario
   ↓
Rol
   ↓
Permiso
```

Ejemplo:

```text
Rol: Solicitante

Permisos:
- REQUISICION_CREAR
- REQUISICION_VER
- REQUISICION_MODIFICAR
- REQUISICION_ENVIAR
```

### Motivo

Permite crear nuevos roles sin modificar el código.

### Consecuencia

La autorización debe comprobar:

```text
¿Tiene permiso?
        +
¿Tiene alcance sobre el recurso?
        +
¿El estado permite realizar la acción?
```

---

# 13. ADR-010 — Permiso y alcance son conceptos diferentes

### Estado

`ACEPTADA`

### Decisión

Se diferenciará:

**Permiso**

> Qué puede hacer el usuario.

**Alcance**

> Sobre qué información puede hacerlo.

Ejemplo:

```text
Usuario:
Martha

Permiso:
REQUISICION_APROBAR

Alcance:
Empresas autorizadas
```

Otro ejemplo:

```text
Usuario:
Solicitante

Permiso:
REQUISICION_MODIFICAR

Alcance:
Su empresa
```

### Motivo

Tener un permiso no significa necesariamente tener acceso a todos los datos.

---

# 14. ADR-011 — Una requisición mensual por empresa

### Estado

`ACEPTADA`

### Decisión

La regla será:

```text
Empresa + Periodo = máximo una Requisición
```

Una requisición puede contener productos destinados a diferentes sedes de la misma empresa.

### Motivo

Evita obligar a una empresa con múltiples sedes a crear múltiples requisiciones.

El modelo continúa siendo escalable porque cada detalle puede distribuirse entre varias sedes.

### Consecuencia

La relación conceptual será:

```text
Empresa
   │
   └── Requisición mensual
           │
           ├── Producto A
           │      ├── Sede 1
           │      └── Sede 2
           │
           └── Producto B
                  └── Sede 1
```

---

# 15. ADR-012 — Separar requisición de pedido al proveedor

### Estado

`ACEPTADA`

### Decisión

Una `Requisicion` representa una **necesidad interna**.

Un `PedidoProveedor` representa una **compra realizada al proveedor**.

No serán la misma entidad.

### Motivo

La cantidad solicitada internamente puede ser diferente a la cantidad comprada.

Ejemplo:

```text
Necesidad:
85 unidades

Compra:
100 unidades
```

Esto puede ocurrir por:

- presentación del producto;
- empaque;
- cantidades mínimas;
- disponibilidad;
- negociación;
- sustitución autorizada.

### Consecuencia

El sistema podrá conservar:

```text
Necesidad interna
        ↓
Consolidación
        ↓
Compra real
```

sin perder trazabilidad.

---

# 16. ADR-013 — Una consolidación puede generar múltiples pedidos

### Estado

`ACEPTADA`

### Decisión

Una consolidación no estará limitada a un único proveedor.

Una misma consolidación puede generar varios pedidos:

```text
Consolidación
      │
      ├── Pedido proveedor A
      ├── Pedido proveedor B
      └── Pedido proveedor C
```

### Motivo

No todos los productos necesariamente pertenecen al mismo proveedor.

### Consecuencia

El sistema debe poder distribuir productos entre diferentes proveedores sin duplicar la necesidad original.

---

# 17. ADR-014 — Producto interno separado del producto del proveedor

### Estado

`ACEPTADA`

### Decisión

El sistema tendrá un catálogo interno de productos.

La relación con los códigos del proveedor se manejará mediante una entidad de homologación:

```text
Producto
   │
   └── ProductoProveedor
             │
             ├── Proveedor
             ├── Código proveedor
             ├── Descripción proveedor
             ├── Categoría proveedor
             └── Unidad proveedor
```

### Motivo

El código interno y el código del proveedor representan conceptos diferentes.

### Consecuencia

La búsqueda manual realizada actualmente podrá evolucionar hacia una relación automatizada.

---

# 18. ADR-015 — Productos no catalogados tienen flujo propio

### Estado

`ACEPTADA`

### Decisión

Un producto que no exista en el catálogo oficial no será tratado simplemente como un `Producto` con código nulo.

Se manejará mediante una solicitud de producto no catalogado.

Flujo:

```text
Solicitante
     ↓
Producto no catalogado
     ↓
Revisión
     ↓
┌───────────────┬───────────────┬─────────────┐
│ Homologar     │ Crear producto│ Rechazar    │
│ existente     │ nuevo         │             │
└───────────────┴───────────────┴─────────────┘
```

### Motivo

Evita contaminar el catálogo oficial con productos incompletos o inconsistentes.

---

# 19. ADR-016 — Guardar historial de estados

### Estado

`ACEPTADA`

### Decisión

Las transiciones importantes de una requisición deben conservarse.

Ejemplo:

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
   ↓
APROBADA
```

Cada transición debe poder identificar:

- estado anterior;
- estado nuevo;
- usuario;
- fecha;
- acción;
- observación cuando corresponda.

### Motivo

La trazabilidad es un requisito importante del proceso.

### Consecuencia

El sistema sustituye progresivamente la trazabilidad manual realizada actualmente en Excel.

---

# 20. ADR-017 — El historial no reemplaza la auditoría

### Estado

`ACEPTADA`

### Decisión

Se diferenciarán:

### Historial de negocio

Registra cambios importantes del flujo.

Ejemplo:

```text
Requisición devuelta para corrección.
```

### Auditoría técnica/operacional

Registra acciones relevantes realizadas sobre recursos.

Ejemplo:

```text
Usuario 25
actualizó DetalleRequisicion 143
fecha: ...
```

### Motivo

No todo evento de auditoría necesariamente representa una transición de estado.

---

# 21. ADR-018 — Las entregas son independientes del pedido

### Estado

`ACEPTADA`

### Decisión

Un pedido puede tener múltiples entregas.

```text
PedidoProveedor
       │
       ├── Entrega 1
       ├── Entrega 2
       └── Entrega 3
```

### Motivo

El proveedor puede realizar entregas parciales.

### Consecuencia

La cantidad pendiente puede calcularse como:

```text
Cantidad pendiente =
Cantidad pedida - Cantidad entregada
```

---

# 22. ADR-019 — La dirección histórica debe conservarse

### Estado

`ACEPTADA`

### Decisión

Las entregas deben conservar una fotografía histórica de la dirección de destino.

No se debe depender exclusivamente de la dirección actual almacenada en `Sede`.

### Motivo

Una sede puede cambiar de dirección.

Una entrega histórica no debe cambiar de dirección automáticamente por modificar el maestro de sedes.

### Consecuencia

La distribución de entrega podrá almacenar información histórica como:

- dirección;
- contacto;
- información necesaria para identificar el destino.

---

# 23. ADR-020 — No modificar silenciosamente una requisición enviada

### Estado

`ACEPTADA`

### Decisión

Cuando una requisición haya sido enviada:

```text
ENVIADA
```

el solicitante no podrá modificarla silenciosamente.

Si Martha detecta un problema:

```text
ENVIADA
   ↓
DEVUELTA
   ↓
corrección
   ↓
ENVIADA
```

### Motivo

Permite conservar trazabilidad sobre qué fue solicitado originalmente y qué fue corregido.

### Consecuencia

La devolución debe incluir una razón cuando corresponda.

---

# 24. ADR-021 — Guardar no significa enviar

### Estado

`ACEPTADA`

### Decisión

Una requisición puede permanecer en:

```text
BORRADOR
```

sin iniciar el proceso de revisión.

La acción:

```text
POST /requisiciones/{id}/enviar
```

es la que inicia formalmente el flujo.

### Motivo

Evita enviar accidentalmente una requisición incompleta.

### Consecuencia

La interfaz debe diferenciar claramente:

```text
Guardar borrador
```

de:

```text
Enviar requisición
```

---

# 25. ADR-022 — Las acciones de negocio tendrán endpoints explícitos

### Estado

`ACEPTADA`

### Decisión

Cuando una operación representa una acción de negocio significativa se utilizará un endpoint específico.

Ejemplos:

```http
POST /requisiciones/{id}/enviar

POST /requisiciones/{id}/aprobar

POST /requisiciones/{id}/devolver
```

en lugar de:

```http
PUT /requisiciones/{id}/estado
```

### Motivo

Los endpoints expresan explícitamente la intención de negocio.

### Consecuencia

El código puede aplicar reglas específicas para cada acción.

---

# 26. ADR-023 — No utilizar repositorio genérico por defecto

### Estado

`ACEPTADA`

### Decisión

No se implementará automáticamente un:

```text
GenericRepository<T>
```

para todas las entidades.

### Motivo

Un repositorio genérico puede ocultar las necesidades reales de cada caso de uso y agregar abstracción innecesaria.

### Consecuencia

Las abstracciones de persistencia se crearán cuando exista una necesidad real.

---

# 27. ADR-024 — No utilizar CQRS completo inicialmente

### Estado

`ACEPTADA`

### Decisión

No se implementará CQRS completo desde el comienzo.

### Motivo

El proyecto inicial no presenta una complejidad que justifique introducir esa infraestructura.

### Consecuencia

Los casos de uso podrán organizarse de manera modular sin implementar una arquitectura CQRS completa.

### Evolución futura

Podrá evaluarse si aparecen necesidades como:

- alta escala;
- lectura y escritura con requisitos radicalmente diferentes;
- modelos de lectura especializados;
- procesamiento distribuido;
- eventos complejos.

---

# 28. ADR-025 — No utilizar microservicios inicialmente

### Estado

`ACEPTADA`

### Decisión

El sistema comenzará como una aplicación modular monolítica.

No se dividirá inicialmente en microservicios.

### Motivo

La escalabilidad funcional no implica automáticamente necesidad de microservicios.

Primero se necesita demostrar:

- estabilidad;
- volumen real;
- límites de módulos;
- necesidades de despliegue;
- necesidades de escalamiento.

---

# 29. ADR-026 — No introducir infraestructura innecesaria

### Estado

`ACEPTADA`

### Decisión

No se introducirán inicialmente tecnologías como:

- Redis;
- RabbitMQ;
- Kafka;
- Kubernetes;
- Docker;
- CQRS;
- microservicios;
- event sourcing;

si no existe un requisito concreto que las justifique.

### Motivo

La complejidad técnica tiene un costo.

Cada tecnología adicional debe resolver un problema real.

### Principio

> No agregar tecnología porque sea popular. Agregarla porque resuelve una necesidad demostrable.

---

# 30. ADR-027 — Diseñar para evolución, no para hipotéticos

### Estado

`ACEPTADA`

### Decisión

El sistema debe ser extensible, pero no se implementarán funcionalidades únicamente porque podrían ser necesarias algún día.

### Ejemplo

Es correcto diseñar:

```text
Empresa
   ↓
muchas Sedes
```

porque es un requisito real de escalabilidad.

No es necesario implementar desde el inicio:

```text
multi-tenant distribuido
event sourcing
microservicios
arquitectura cloud compleja
```

sin una necesidad concreta.

### Principio

```text
Escalable ≠ complejo
```

---

# 31. ADR-028 — Las cantidades solicitadas, compradas y entregadas son diferentes

### Estado

`ACEPTADA`

### Decisión

El sistema diferenciará:

```text
Cantidad solicitada
Cantidad necesaria
Cantidad pedida
Cantidad entregada
Cantidad pendiente
```

(Nombre "Cantidad necesaria" alineado con `DetalleConsolidacion.CantidadNecesaria`, `04-base-datos.md §24`, ya implementada.)

### Motivo

Estas cantidades representan diferentes etapas del proceso.

### Consecuencia

No se debe sobrescribir una cantidad anterior para representar una nueva etapa.

Debe conservarse la trazabilidad.

---

# 32. ADR-029 — La consolidación no modifica las requisiciones originales

### Estado

`ACEPTADA`

### Decisión

La consolidación será una representación derivada de las requisiciones aprobadas.

No modificará destructivamente los datos originales.

Ejemplo:

```text
AUROTECH       → 20
FAVIPAQ        → 30
COURIERBOX     → 35
                    ↓
              CONSOLIDACIÓN
                    ↓
                  85
```

La consolidación debe permitir regresar desde esos 85 hasta sus fuentes originales.

### Motivo

Mantener trazabilidad.

---

# 33. ADR-030 — Las reglas de fechas son configurables

### Estado

`ACEPTADA`

### Decisión

Los periodos y ventanas de solicitud no tendrán fechas codificadas directamente en el código.

Ejemplo:

```text
Periodo: Septiembre 2026

Inicio solicitud: 01/09/2026
Fin solicitud:    03/09/2026
```

### Motivo

Las fechas pueden cambiar entre meses.

### Consecuencia

El sistema debe consultar la configuración del periodo correspondiente.

---

# 34. ADR-031 — El plazo del proveedor es diferente al periodo de solicitud

### Estado

`ACEPTADA`

### Decisión

Se diferencian:

```text
Ventana de solicitud de filiales
```

de:

```text
Plazo de entrega del proveedor
```

Ejemplo:

```text
01–03 septiembre
     ↓
solicitudes

05 septiembre
     ↓
pedido proveedor

máximo 5 días
     ↓
entrega esperada
```

### Motivo

Son reglas de negocio diferentes y pertenecen a etapas diferentes.

---

# 35. ADR-032 — Backend como fuente de verdad

### Estado

`ACEPTADA`

### Decisión

La información crítica del sistema debe determinarse en el backend.

Esto incluye:

- identidad del usuario;
- empresa;
- sedes autorizadas;
- roles;
- permisos;
- estados;
- cantidades;
- reglas de fechas;
- relaciones;
- autorización.

### Regla

Nunca confiar exclusivamente en valores enviados por React.

Ejemplo incorrecto:

```json
{
  "usuarioId": 5,
  "empresaId": 2,
  "rol": "ADMIN"
}
```

El backend debe determinar y validar estos datos utilizando la identidad autenticada y las reglas de autorización.

---

# 36. ADR-033 — Las pruebas acompañan cada funcionalidad

### Estado

`ACEPTADA`

### Decisión

Una funcionalidad no se considera terminada únicamente porque compile.

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
Prueba de integración/API

Interfaz
        ↓
Prueba frontend
```

### Consecuencia

Las tareas de desarrollo deberán incluir criterios de aceptación y pruebas.

---

# 37. ADR-034 — Claude Code no toma decisiones de negocio por iniciativa propia

### Estado

`ACEPTADA`

### Decisión

Claude Code será utilizado como herramienta de implementación y asistencia técnica.

No debe inventar:

- reglas de negocio;
- entidades;
- relaciones;
- endpoints;
- permisos;
- flujos;
- tecnologías;
- cambios arquitectónicos.

### Regla

```text
Nosotros decidimos:
    negocio
    arquitectura
    alcance

Claude Code implementa:
    código
    pruebas
    configuración
    documentación técnica
```

Si encuentra una ambigüedad que pueda cambiar el diseño, debe detenerse y reportarla.

---

# 38. ADR-035 — Las decisiones importantes requieren análisis de alternativas

### Estado

`ACEPTADA`

### Decisión

Antes de tomar una decisión arquitectónica importante se debe analizar:

```text
Problema
   ↓
Opciones
   ↓
Ventajas
   ↓
Desventajas
   ↓
Impacto
   ↓
Decisión
```

No se debe seleccionar una solución únicamente porque:

- sea moderna;
- sea popular;
- la utilice otro proyecto;
- una IA la recomiende;
- parezca más profesional.

---

# 39. ADR-036 — Principio de mínima complejidad

### Estado

`ACEPTADA`

### Decisión

Ante dos soluciones que resuelven correctamente el mismo problema, se preferirá la que tenga menor complejidad accidental.

Esto significa preferir:

```text
solución simple + límites claros
```

sobre:

```text
solución sofisticada + complejidad innecesaria
```

si ambas satisfacen los requisitos.

### Principio

> La arquitectura debe ser tan compleja como el problema lo requiera, no más.

---

# 40. ADR-037 — El modelo de dominio precede al código

### Estado

`ACEPTADA`

### Decisión

Antes de implementar una funcionalidad importante se debe comprender:

1. problema;
2. actores;
3. entidades;
4. relaciones;
5. reglas;
6. estados;
7. casos de uso;
8. datos;
9. permisos;
10. API;
11. pruebas.

Después se implementa.

### Motivo

Evita comenzar directamente escribiendo clases y endpoints sin comprender el comportamiento del sistema.

---

# 41. ADR-038 — No hardcodear personas, empresas ni sedes

### Estado

`ACEPTADA`

### Decisión

El sistema no debe contener lógica específica basada en nombres reales.

Incorrecto:

```csharp
if (usuario.Nombre == "Martha")
{
    // ...
}
```

Incorrecto:

```csharp
if (empresa.Nombre == "AUROTECH")
{
    // ...
}
```

Correcto:

```text
Usuario
    ↓
Rol
    ↓
Permisos
    ↓
Alcance
```

### Motivo

El sistema debe representar responsabilidades, no personas concretas.

---

# 42. ADR-039 — La arquitectura debe permitir nuevas empresas

### Estado

`ACEPTADA`

### Decisión

Agregar una nueva empresa debe ser principalmente una operación de configuración/datos.

Idealmente:

```text
Crear Empresa
      ↓
Crear Sedes
      ↓
Asignar Usuarios
      ↓
Asignar Roles/Permisos
      ↓
La empresa puede utilizar el sistema
```

No debería requerir:

```text
Modificar código
↓
Compilar
↓
Desplegar
```

para cada nueva empresa.

---

# 43. ADR-040 — Estado de una entidad y permisos de una acción son conceptos diferentes

### Estado

`ACEPTADA`

### Decisión

Una acción debe validarse combinando:

```text
Permiso
+
Alcance
+
Estado actual
+
Reglas de negocio
```

Ejemplo:

Un usuario puede tener:

```text
REQUISICION_MODIFICAR
```

pero no necesariamente puede modificar una requisición:

```text
APROBADA
```

### Motivo

El permiso por sí solo no determina si una acción es válida.

---

# 44. ADR-041 — Auropaq Pedidos no es un sistema contable ni un ERP financiero

### Estado

`ACEPTADA`

### Contexto

El sistema necesita registrar información de facturación de proveedores para mantener trazabilidad de compras. Existe el riesgo de que esta necesidad se confunda con construir un módulo contable/ERP.

### Decisión

Auropaq Pedidos registra únicamente información operativa de facturación (RN-038): proveedor, pedido asociado, número de factura, fecha, detalle de productos/cantidades/precios, subtotal, impuestos (como valor informado, no como cálculo de tasa), total, estado operativo y observaciones.

No implementa: asientos contables, cuentas contables, causación, pagos, conciliación bancaria, retenciones contables, libros contables ni declaraciones tributarias.

### Motivo

El problema que resuelve el sistema es trazabilidad del ciclo de compra (requisición → ... → facturación operativa), no gestión financiera/contable. Construir un módulo contable sin que el negocio lo haya solicitado sería sobrealcance (`CLAUDE.md §71`).

### Alternativas consideradas

- Integrar un módulo contable simplificado desde ahora: descartada, no hay requisito de negocio ni proceso contable real validado.
- Dejar el alcance implícito (sin declararlo): descartada, ya generó ambigüedad documentada (`01-reglas-negocio.md §14`, Pendiente 3, antes de este refinamiento).

### Consecuencias

`Factura`/`DetalleFactura` permanecen como registro simple, sin ciclo contable. Si el negocio necesita contabilidad en el futuro, probablemente se resuelva mediante integración con un sistema contable externo, no construyéndolo dentro de Auropaq Pedidos.

### Evolución futura

Si surge una necesidad de negocio explícita (ej. integración con un sistema contable externo), debe tratarse como una nueva decisión, no como extensión silenciosa de `Factura`.

---

# 45. ADR-042 — Un pedido puede tener varias facturas; una factura pertenece a un único pedido (alcance inicial)

### Estado

`ACEPTADA` (para el alcance inicial)

> **Actualización (cierre documental 2026-09-11):** esta decisión cierra formalmente D-06 de la matriz de `01-reglas-negocio.md §15`. Se actualiza esta entrada en lugar de duplicarla en un ADR nuevo.

### Contexto

`02-dominio.md §29` y `04-base-datos.md §32` dejaban la cardinalidad `Factura ↔ PedidoProveedor` sin fijar, a la espera de validar con el proceso contable real. Mientras tanto, ya se había implementado `PedidoProveedor 1 ─── N Factura` como decisión provisional de MVP (bloque "TASK-046 — incremento MVP", `progreso.md`).

### Decisión

Se confirma `PedidoProveedor 1 ─── N Factura` como regla de negocio para el alcance inicial (RN-039). Una factura no puede cubrir varios pedidos en este alcance.

### Motivo

Es el escenario más simple que cubre la necesidad real conocida (varias facturas parciales para un mismo pedido) sin inventar una generalización (N:N) que nadie ha solicitado todavía.

### Alternativas consideradas

- Fijar 1:1 (un pedido, una factura): descartada, no cubre facturación parcial, ya evidenciada como necesidad real.
- Fijar N:N desde ahora (una factura puede cubrir varios pedidos): descartada, sin necesidad de negocio explícita ni proceso contable real que lo exija; mayor complejidad de modelo sin justificación (`CLAUDE.md §43`).

### Consecuencias

`Factura.PedidoProveedorId` permanece como FK obligatoria y única. Si en el futuro se requiere que una factura cubra varios pedidos, es un cambio de modelo (tabla puente), no una extensión aditiva.

### Evolución futura

Si el negocio confirma la necesidad de facturación consolidada entre varios pedidos, debe registrarse como una nueva decisión que reemplace explícitamente esta (siguiendo `§57` de este documento — Regla para modificar una decisión existente).

---

# 46. ADR-043 — Entrega y Factura son ramas independientes de PedidoProveedor

### Estado

`ACEPTADA`

> **Actualización (cierre documental 2026-09-11):** esta decisión cierra formalmente D-07 de la matriz de `01-reglas-negocio.md §15`. Se actualiza esta entrada en lugar de duplicarla en un ADR nuevo.

### Contexto

Un pedido a proveedor puede tener múltiples entregas físicas y múltiples facturas. Debía decidirse si una factura se relaciona con una entrega específica o si ambas son independientes.

### Decisión

`Entrega` y `Factura` cuelgan directamente de `PedidoProveedor`, cada una de forma independiente. No existe relación `Entrega → Factura` (RN-040).

```text
PedidoProveedor
      ├── Entregas
      └── Facturas
```

### Motivo

El proceso de entrega física y el proceso de facturación del proveedor avanzan a ritmos distintos y no necesariamente coinciden en número ni en cantidades (ej. 2 entregas y 3 facturas para el mismo pedido).

### Alternativas consideradas

- Relacionar `Factura` con `Entrega` (`Factura.EntregaId`): descartada por ahora — acoplaría ambos procesos sin necesidad de negocio confirmada, y complicaría el caso común de facturas/entregas que no coinciden 1:1.

### Consecuencias

No es posible hoy responder "¿qué factura corresponde a esta entrega?" directamente; solo se puede cruzar por `DetallePedidoProveedor` (mismo producto/pedido). Si el negocio necesita esa trazabilidad más fina, requiere una nueva decisión.

### Evolución futura

Si se confirma la necesidad de conciliar entregas físicas contra facturas (por ejemplo, para no facturar lo no recibido — ver Pendiente 13, D-10 en `01-reglas-negocio.md §15`), se debe evaluar entonces una relación explícita, sin asumirla de antemano.

---

# 47. ADR-044 — Selección explícita de proveedor por Compras

### Estado

`ACEPTADA`

### Contexto

Un producto puede tener más de un proveedor asociado (`ProductoProveedor`). No estaba definido quién o qué decide a qué proveedor se asigna cada `DetalleConsolidacion` al crear un `PedidoProveedor` (Pendiente 7).

### Decisión

El proveedor de cada `PedidoProveedor` se selecciona explícitamente por el usuario responsable de Compras. El sistema puede mostrar información de los proveedores asociados al producto, pero no selecciona automáticamente ninguno.

### Motivo

No existe todavía una regla empresarial (precio, disponibilidad, proveedor preferido, condiciones comerciales) que permita automatizar la selección de forma confiable. Automatizarla sin esa regla sería inventar negocio (`CLAUDE.md §6`).

### Alternativas consideradas

- Selección automática por precio/disponibilidad: descartada, no hay datos ni regla real que la respalde hoy.
- Selección automática con proveedor "preferido" marcado en `ProductoProveedor`: descartada por ahora, mismo motivo — no hay ese campo ni la regla que lo justifique.

### Consecuencias

`CrearPedidoProveedorUseCase` sigue recibiendo `ProveedorId` explícito del llamador (ya implementado así). La API (`PedidosProveedorController`, todavía no construida) deberá exigir este dato en el payload de creación.

### Evolución futura

Si el negocio define un criterio automático confiable, se documenta como una nueva decisión que reemplace esta explícitamente (`§57` — Regla para modificar una decisión existente), no como una excepción silenciosa dentro del mismo caso de uso.

---

# 48. ADR-045 — CantidadNecesaria y CantidadPedida son independientes; sin motor de reglas de empaques/mínimos

### Estado

`ACEPTADA`

### Contexto

`CantidadPedida` puede diferir de `CantidadNecesaria` por empaques, presentaciones, mínimos de compra o disponibilidad (RN-031, `04-base-datos.md §27`), pero no estaba definido si el sistema debía calcular esa diferencia estructuradamente, ni quién decide el valor final de `CantidadPedida` (Pendientes 6 y 8).

### Decisión

`CantidadPedida` es definida manualmente por Compras. El sistema no calcula automáticamente presentaciones, empaques, múltiplos de compra, mínimos del proveedor ni redondeos. Ambos valores (`CantidadNecesaria`, `CantidadPedida`) se conservan para trazabilidad (RN-031/RN-042).

### Motivo

No existe hoy un modelo de datos de empaques/presentaciones por proveedor (`ProductoProveedor` no lo tiene), ni una necesidad de negocio validada que justifique construir ese motor de reglas ahora (`CLAUDE.md §43/§71`).

### Alternativas consideradas

- Modelar `UnidadesPorEmpaque`/`CantidadMinimaCompra` en `ProductoProveedor` y calcular `CantidadPedida` automáticamente: descartada por ahora — mayor complejidad sin necesidad de negocio confirmada.

### Consecuencias

`DetallePedidoProveedor.CantidadPedida` sigue siendo un valor de entrada manual (ya implementado así), sin validación contra reglas de empaque.

### Evolución futura

Si el negocio confirma la necesidad de un cálculo estructurado (por ejemplo, porque los pedidos manuales generan errores frecuentes), se evalúa entonces modelar `ProductoProveedor` con esos campos — como una decisión nueva, no como extensión silenciosa.

---

# 49. ADR-046 — Ciclo de vida de PedidoProveedor

### Estado

`ACEPTADA`

### Contexto

`PedidoProveedor.Estado` no tenía valores ni transiciones definidas (Pendiente 10); `05-api.md §32` dejaba `confirmar`/`cancelar` como acciones conceptuales sin ciclo de vida detrás.

### Decisión

```text
BORRADOR → ENVIADO → PARCIALMENTE_ENTREGADO → ENTREGADO → CERRADO
```

con `CANCELADO` alcanzable desde `BORRADOR`, `ENVIADO` o `PARCIALMENTE_ENTREGADO`. Significado de cada estado documentado en `01-reglas-negocio.md §11` (RN-043).

### Motivo

Es el ciclo mínimo necesario para representar el flujo real ya implementado (envío al proveedor, entregas parciales, entrega completa, cierre), sin agregar estados que nadie ha pedido (`CONFIRMADO`, `EN_PROCESO`, `FACTURADO`, `PAGADO`).

### Alternativas consideradas

- Reutilizar el mismo patrón de `Requisicion.Estado` (BORRADOR/ENVIADA/EN_REVISION/...): descartada, el flujo de aprobación de una requisición no aplica a un pedido a proveedor (no hay "revisión" de un pedido, hay envío y seguimiento de entrega).
- Incluir `FACTURADO` como estado del pedido: descartada — Entrega y Factura son independientes (RN-040), un pedido no debe quedar condicionado por su facturación.

### Consecuencias

Requiere implementar el enum/las transiciones en Domain (hoy `Estado` es texto libre) y un caso de uso por transición relevante (envío, avance automático a `PARCIALMENTE_ENTREGADO`/`ENTREGADO` según entregas registradas, cierre, cancelación). No se implementa en esta tarea.

### Evolución futura

Si aparece una necesidad real de un estado adicional, se evalúa como una decisión nueva sobre esta (`§57` — Regla para modificar una decisión existente).

---

# 50. ADR-047 — Cierre explícito de PedidoProveedor, independiente de la factura

### Estado

`ACEPTADA`

### Contexto

El flujo conceptual del negocio termina en "Cierre" (`00-contexto-proyecto.md §4`), pero no había condiciones definidas para considerar cerrado un pedido (Pendiente 15, `TASK-047`).

### Decisión

Un `PedidoProveedor` pasa a `CERRADO` cuando la cantidad pendiente de entrega es cero (RN-034) **y** un usuario autorizado ejecuta explícitamente la acción de cierre. La existencia o ausencia de `Factura` no es requisito.

### Motivo

Entrega y Factura son procesos independientes (RN-040/ADR-043); exigir factura para cerrar acoplaría dos procesos que el negocio ya decidió mantener separados, y bloquearía el cierre operativo de compras cuya facturación se gestiona con retraso.

### Alternativas consideradas

- Cierre automático al llegar a `ENTREGADO` (sin acción explícita): descartada — el negocio pidió explícitamente una acción de cierre ejercida por un usuario, no un efecto automático silencioso.
- Exigir factura registrada para poder cerrar: descartada por la independencia Entrega/Factura ya decidida (RN-040).

### Consecuencias

Falta implementar el caso de uso de cierre (no existe hoy) y la validación de que la cantidad pendiente sea cero antes de permitirlo.

### Evolución futura

`TASK-047` (validar cierre) puede ampliar esta decisión si el negocio confirma condiciones adicionales (por ejemplo, requerir que no haya facturas `ANULADA` sin resolver) — como decisión nueva, no implícita.

---

# 51. ADR-048 — Sustitución de productos: explícita y trazable, implementación diferida

### Estado

`ACEPTADA` (decisión de negocio) — implementación `PENDIENTE`

### Contexto

No estaba definido qué ocurre cuando el proveedor no dispone del producto solicitado y propone uno equivalente (Pendiente 5).

### Decisión

Una sustitución de producto debe ser una acción explícita y autorizada, que conserve: producto original, producto sustituto, motivo, usuario responsable y fecha. No debe modificarse silenciosamente el producto solicitado/consolidado.

### Motivo

Sustituir sin dejar rastro rompería la trazabilidad exigida por RN-036/RN-037 y ocultaría una decisión operativa relevante de Compras.

### Alternativas consideradas

- No permitir sustituciones (rechazar el pedido y crear uno nuevo): descartada, no refleja la operación real descrita por el negocio.
- Sustitución silenciosa (sobrescribir el producto del detalle): descartada explícitamente — viola trazabilidad.

### Consecuencias

Se decide el principio, pero no se implementa mecanismo (entidad, campo o caso de uso) en esta tarea — no existe hoy código para esto. Queda como trabajo técnico futuro, no como ambigüedad de negocio.

### Evolución futura

La implementación (por ejemplo, un registro `SustitucionProducto` o campos en `DetallePedidoProveedor`) se diseña en una tarea futura dedicada, sujeta a este principio ya cerrado.

---

# 52. ADR-049 — Estados operativos de Entrega, separados de la recepción detallada

### Estado

`ACEPTADA`

### Contexto

`Entrega.Estado` no tenía valores definidos (Pendiente 11); existía además ambigüedad entre el estado administrativo del registro `Entrega` y las condiciones de recepción de mercancía (Pendiente 4).

### Decisión

```text
REGISTRADA
ANULADA
```

Las condiciones detalladas de recepción (`Aceptado`/`Rechazado`/`Faltante`/`Dañado`/`Sustituido`) no son estados de `Entrega`; quedan diferidas como concepto aparte, a modelar solo si el negocio lo requiere.

### Motivo

Separar "¿el registro de entrega es válido?" (estado administrativo) de "¿qué pasó físicamente con la mercancía?" (recepción) evita mezclar dos responsabilidades distintas en un único campo de texto libre.

### Alternativas consideradas

- Un único `Estado` con todos los valores de recepción incluidos: descartada — mezclaría el ciclo de vida del registro con el resultado de la inspección física, dos conceptos distintos.

### Consecuencias

Falta implementar el enum en Domain (hoy texto libre) y decidir en el futuro, si el negocio lo requiere, cómo modelar la recepción detallada (Pendiente 4 queda diferida, no resuelta en el sentido de "implementada").

### Evolución futura

Si el negocio confirma que necesita registrar recepción detallada, se evalúa como una entidad/concepto nuevo, no como más valores de `Entrega.Estado`.

---

# 53. ADR-050 — Estados operativos de Factura y límite de cantidad facturable

### Estado

`ACEPTADA`

### Contexto

`Factura.Estado` no tenía valores definidos; existía una propuesta de ciclo de vida contable más amplio (2026-09-10, `progreso.md`) marcada explícitamente `PROPUESTA — NO APROBADA` (Pendiente 12). Tampoco estaba definido el tope de `CantidadFacturada` (Pendiente 13).

### Decisión

Estados: `REGISTRADA`/`ANULADA`, sin estados contables (`CAUSADA`/`CONTABILIZADA`/`PAGADA`/`CONCILIADA`). Límite: `SUM(CantidadFacturada)` por `DetallePedidoProveedor` no debe superar `CantidadPedida` — no se limita contra `CantidadEntregada`, porque Entrega y Factura son independientes (RN-040).

### Motivo

Consistente con RN-038 (Auropaq Pedidos no es sistema contable): un ciclo de vida con estados contables contradiría esa decisión. El límite contra `CantidadPedida` (no `CantidadEntregada`) evita acoplar facturación a recepción física, que el negocio ya decidió mantener separadas.

### Alternativas consideradas

- Adoptar la propuesta de ciclo de vida contable de 2026-09-10 (`CAUSADA`/`CONTABILIZADA`/`PAGADA`): descartada explícitamente por RN-038.
- Limitar `CantidadFacturada` contra `CantidadEntregada`: descartada — acoplaría Factura a Entrega (RN-040/ADR-043).

### Consecuencias

Falta implementar el enum de `Estado` (hoy texto libre); el límite de cantidad ya está implementado tal cual (`Factura.AgregarDetalle`).

### Evolución futura

Si el negocio decide en el futuro que sí necesita integrarse con un proceso contable, esa integración se trata como un sistema externo (ADR-041), no como una ampliación de este ciclo de estados.

---

# 54. ADR-051 — Identificadores documentales separados del Id técnico, con unicidad contextual

### Estado

`ACEPTADA`

### Contexto

`NumeroPedido`, `NumeroRemision` y `NumeroFactura` no tenían definido su origen (autogenerado vs. ingresado) ni reglas de unicidad (Pendientes 9 y 14).

### Decisión

Los tres son referencias operacionales **ingresadas** (no autogeneradas por el sistema), distintas del `Id` técnico interno. Unicidad contextual:

```text
NumeroPedido    — único dentro del proveedor.
NumeroFactura   — único dentro del proveedor.
NumeroRemision  — único dentro del PedidoProveedor.
```

### Motivo

Estos números reflejan documentos reales del proveedor/transportador (orden de compra, remisión, factura); generarlos internamente rompería su correspondencia con el documento físico. La unicidad es contextual (por proveedor o por pedido) porque distintos proveedores pueden reutilizar numeraciones entre sí sin conflicto real.

### Alternativas consideradas

- Autogenerar los tres números desde el sistema: descartada — no reflejarían el documento real del proveedor.
- Unicidad global (no contextual): descartada — dos proveedores distintos podrían compartir el mismo número de pedido sin que eso sea un error real.

### Consecuencias

Falta implementar los índices/validaciones de unicidad (`PedidoProveedorConfiguration`, `EntregaConfiguration`, `FacturaConfiguration` no los tienen hoy). Los tres campos ya se reciben como texto libre explícito del llamador.

### Evolución futura

Si se detectan colisiones reales en producción, se prioriza implementar esta validación antes de otras mejoras, dado que ya está decidida.

---

# 55. ADR-052 — La trazabilidad de autoría es transversal, no un campo ad hoc por entidad

### Estado

`ACEPTADA`

### Contexto

`PedidoProveedor`, `Entrega` y `Factura` no registran quién realizó la acción (Pendiente 16), pero `Usuario` todavía no existe en Domain (`TASK-008`, `PENDIENTE`).

### Decisión

No se agregan campos provisionales de usuario (`UsuarioCreacionId`, etc.) en estas entidades antes de que exista autenticación real. Cuando exista, la autoría se integra de forma transversal (mismo mecanismo para todas las entidades relevantes), no entidad por entidad de forma ad hoc.

### Motivo

Agregar un campo `UsuarioCreacionId` sin autenticación real detrás sería un campo decorativo que aparenta trazabilidad sin proveerla de verdad — contradice RN-036/RN-037 (trazabilidad real, no aparente) y `CLAUDE.md §71` (no código especulativo).

### Alternativas consideradas

- Agregar `UsuarioCreacionId` como `int` opcional desde ya, "para no tener que migrar después": descartada explícitamente — es exactamente el tipo de campo provisional que esta decisión prohíbe.

### Consecuencias

Ninguna entidad de este bloque tiene hoy autoría. Cuando se implemente `Usuario`/autenticación (Fase 10, `08-tareas.md`), esta decisión determina que la integración debe ser transversal, no específica de `PedidoProveedor`/`Entrega`/`Factura`.

### Evolución futura

Se revisita como parte de la implementación de `TASK-008` y la Fase 10 de Seguridad, no antes.

---

# 56. ADR-053 — Una requisición participa en, a lo sumo, una consolidación por período

### Estado

`ACEPTADA`

### Contexto

La auditoría integral del backend (2026-09-11) detectó que `CrearConsolidacionUseCase` no verificaba si una requisición aprobada ya había sido tomada por una consolidación anterior del mismo periodo, ni existía una restricción de base de datos que lo impidiera — dos llamadas seguidas podían duplicar por completo la necesidad consolidada.

### Decisión

Un `Periodo` puede tener múltiples `Consolidacion` (sin cambios). Una `Requisicion` aprobada participa, como máximo, en una — al crear una nueva consolidación, se excluyen las requisiciones cuyos detalles ya fueron asignados en cualquier consolidación previa.

### Motivo

Es el comportamiento mínimo necesario para que "múltiples consolidaciones por periodo" (ya permitido desde el diseño original) no implique duplicar la necesidad de compra ya consolidada — sin eso, el sistema podría generar pedidos duplicados a partir de la misma necesidad real.

### Alternativas consideradas

- Consolidación única por periodo (como `Requisicion` es única por `Empresa+Periodo`): descartada — no cubre el caso real de requisiciones aprobadas en momentos distintos del mismo periodo.
- Permitir duplicidad y resolverla manualmente fuera del sistema: descartada — el propio síntoma (necesidad duplicada) es lo que se buscaba evitar.

### Consecuencias

`CrearConsolidacionUseCase` filtra las requisiciones candidatas antes de agrupar. `AsignacionConsolidacionConfiguration` agrega un índice único sobre `DetalleRequisicionOrigenId` como salvaguarda de base de datos (defensa en profundidad, mismo criterio que los índices únicos de `NumeroPedido`/`NumeroFactura`/`NumeroRemision`, RN-049/ADR-051).

### Evolución futura

Si el negocio necesita permitir excepcionalmente que una requisición se recategorice entre consolidaciones (por ejemplo, deshacer una consolidación errónea), es una decisión nueva y explícita, no una relajación silenciosa de esta.

---

# 57. ADR-054 — Anular una entrega recalcula el pedido; bloqueado si el pedido está cerrado

### Estado

`ACEPTADA`

> **Actualización (2026-09-11):** extendida a `CANCELADO` — ver RN-054 (`01-reglas-negocio.md §12`). `CANCELADO` recibe exactamente el mismo tratamiento que `CERRADO` para anular (un pedido fuera de operación no se reabre ni se recalcula), y además bloquea crear una entrega nueva y agregar detalle/distribución a una entrega ya existente que haya quedado asociada al pedido antes de cancelarse. Esto resuelve la "Evolución futura" que esta misma entrada había dejado pendiente.

### Contexto

La implementación original de `AnularEntregaUseCase` (cierre técnico anterior) anulaba la entrega sin recalcular `PedidoProveedor.Estado` ni impedir la anulación cuando el pedido ya estaba `CERRADO` — quedó documentado explícitamente como una decisión no tomada, a la espera de que el negocio la resolviera.

### Decisión

Al anular una `Entrega`, sus cantidades dejan de contar como cantidad entregada válida y se recalcula `PedidoProveedor.Estado` según las entregas válidas restantes (pudiendo volver a `ENVIADO` si no queda ninguna). No se permite anular una entrega si el pedido ya está `CERRADO`.

### Motivo

Dejar el estado del pedido "congelado" con datos que ya no reflejan la realidad (una entrega anulada que sigue contando) generaría información engañosa — por ejemplo, un pedido `ENTREGADO` sin ninguna entrega válida detrás. Bloquear la anulación sobre un pedido `CERRADO` evita reabrir un ciclo que el negocio ya dio por terminado explícitamente (RN-044).

### Alternativas consideradas

- No recalcular nada al anular (dejar el estado como estaba): descartada — es la fuente del problema detectado en la auditoría.
- Permitir anular incluso con el pedido `CERRADO`, sin recalcular su estado: descartada explícitamente por el negocio — un pedido cerrado no debe reabrirse ni verse afectado.

### Consecuencias

`EntregaFinder.CalcularCantidadYaEntregada` excluye entregas `ANULADA` (afecta también el tope de nuevas entregas y `CalcularCantidadPendienteUseCase`). `PedidoProveedor.ActualizarEstadoPorEntregas` ahora recibe un segundo booleano (`hayAlgunaCantidadEntregada`) para poder distinguir "sin ninguna entrega válida" (`ENVIADO`) de "con cantidad pendiente" (`PARCIALMENTE_ENTREGADO`).

### Evolución futura

*(Resuelta 2026-09-11 — ver actualización arriba.)* Si en el futuro aparece un tercer estado "fuera de operación" con un tratamiento distinto de `CERRADO`/`CANCELADO`, se evalúa como una decisión nueva.

---

# 58. ADR-055 — Una factura anulada libera su cantidad facturable

### Estado

`ACEPTADA`

### Contexto

RN-048/ADR-050 fijaron el tope `SUM(CantidadFacturada) ≤ CantidadPedida`, pero calculado sobre todas las facturas sin distinguir su estado — una factura `ANULADA` seguía "ocupando" cupo permanentemente, sin posibilidad de corregirla con una factura nueva.

### Decisión

El cálculo de `CantidadFacturada` acumulada excluye las facturas en estado `ANULADA`. Una factura anulada libera su cantidad para que una factura nueva la use, sin superar nunca `CantidadPedida` entre las facturas válidas.

### Motivo

Una factura se anula típicamente porque estaba mal (monto, cantidad o documento incorrecto); si su cantidad siguiera "ocupada" para siempre, el proveedor no podría volver a facturar correctamente esa parte del pedido.

### Alternativas consideradas

- Mantener el acumulado incluyendo anuladas (estado actual antes de esta decisión): descartada — bloquea permanentemente la corrección de errores de facturación.

### Consecuencias

`FacturaFinder.CalcularCantidadYaFacturada` excluye facturas `ANULADA`. No requiere cambios de esquema ni migración.

### Evolución futura

Ninguna prevista; si el negocio requiere conservar un histórico separado de "cantidad alguna vez facturada, incluyendo anulada" (por ejemplo con fines de auditoría externa), es una necesidad de reporte distinta, no un cambio a esta regla.

---

# 59. ADR-056 — Permiso.Codigo como identificador funcional global y único

### Estado

`ACEPTADA`

### Contexto

`04-base-datos.md §9.2` documentaba `Permiso.Codigo` sin ninguna regla de unicidad (TASK-011). Durante esa misma tarea se verificó explícitamente, por SQL directo, que el esquema permitía crear dos `Permiso` con el mismo `Codigo` sin error.

### Decisión

`Permiso.Codigo` se define como identificador funcional GLOBAL y ÚNICO. No pueden existir dos `Permiso` con el mismo `Codigo` en todo el sistema (no por Empresa — `Permiso` es global, ADR/RN ya cerrados para `Rol` en TASK-010).

### Motivo

`Permiso` representa una capacidad funcional del sistema. Cuando `TASK-013` establezca `Rol → RolPermiso → Permiso`, el código debe identificar una capacidad inequívoca. Permitir códigos duplicados generaría ambigüedad en la asignación de permisos a roles, en consultas, en la futura autorización, en la configuración de roles y en cualquier integración futura con policies/claims.

### Alternativas consideradas

- Mantener sin unicidad (estado dejado por TASK-011): descartada — la ambigüedad se materializaría justo al construir `RolPermiso` en TASK-013, el peor momento para descubrirla.
- Unicidad acotada a algún ámbito (por ejemplo, por "módulo" del sistema): descartada — no existe ningún concepto de "módulo" documentado para `Permiso`; introducirlo sería inventar estructura no definida solo para acotar la unicidad.

### Consecuencias

Índice único agregado sobre `Permisos.Codigo` en SQL Server (`PermisoConfiguration`, migración `AgregarUnicidadPermisoCodigo`), reforzado con validación en `CrearPermisoUseCase` antes de persistir (dos barreras, mismo criterio ya usado para `Usuario.Correo` y `Periodo` Año+Mes). Los 2 registros de datos de prueba duplicados (`REQUISICION_CREAR` × 2, insertados durante la verificación SQL manual de TASK-011) se eliminaron antes de aplicar la migración — no eran datos de negocio legítimos, eran artefactos de esa verificación.

### Evolución futura

Ninguna prevista — esta decisión deja `Permiso` listo para que `TASK-013` construya `RolPermiso` sin ambigüedad de identidad.

---

# 60. ADR-057 — Autenticación: JWT propio, no delegado a un proveedor externo

### Estado

`ACEPTADA`

### Contexto

Al iniciar la implementación de autenticación de usuarios ("TASK-015", 2026-09-14), la auditoría obligatoria previa encontró que ningún documento cerraba el mecanismo concreto: `06-seguridad.md §4` decía "se definirá posteriormente", `§30` "se definirá cuando se seleccione el mecanismo de identidad", `§31` recomendaba "evaluar primero si la autenticación puede delegarse a un proveedor de identidad" antes de construir gestión de contraseñas propia, y el catálogo oficial (`08-tareas.md`, `TASK-048 — Autenticación`) establecía explícitamente: "la tecnología concreta de autenticación debe definirse antes de implementar esta tarea". Tampoco existía ningún ADR previo sobre el tema, ni ningún paquete NuGet de autenticación/hash ya instalado. Esto se clasificó como AMBIGÜEDAD BLOQUEANTE y se detuvo la implementación hasta obtener una decisión explícita del usuario.

### Decisión

Autenticación mediante **JWT emitido y validado por esta misma API** (no delegado a un proveedor de identidad externo como Azure AD/Entra ID, Auth0 o similar). `Usuario` almacena `PasswordHash` (RN-056); no existe integración con ningún proveedor externo de identidad.

### Motivo

Decisión de negocio explícita del usuario, tomada después de presentársele ambas alternativas (JWT propio vs. delegado) con sus implicaciones.

### Alternativas consideradas

- Delegar a un proveedor de identidad externo (Azure AD/Entra ID, Auth0, IdentityServer/Duende, SSO corporativo): es la opción que `06-seguridad.md §31` recomendaba evaluar primero — descartada explícitamente por el usuario en favor de JWT propio.
- ASP.NET Core Identity completo (con `UserManager`/`SignInManager`/esquema propio de tablas): descartado — `TASK-015 §16` prohíbe agregar librerías de Identity completas si no se necesitan; se usa únicamente `PasswordHasher<T>` (`Microsoft.Extensions.Identity.Core`), sin el resto del framework.

### Consecuencias

`Usuario` gana `PasswordHash` (migración `AgregarPasswordHashAUsuario`). Nuevos paquetes: `Microsoft.Extensions.Identity.Core` (hash), `System.IdentityModel.Tokens.Jwt` (emisión, Infrastructure), `Microsoft.AspNetCore.Authentication.JwtBearer` (validación, Api) — todos oficiales de Microsoft/.NET, versión `10.0.12` (alineada con el resto del stack) salvo `System.IdentityModel.Tokens.Jwt` (`8.22.0`, versionado independiente de esa librería). `POST /api/v1/auth/login` (`05-api.md §56`) implementado. `Jwt:Key` nunca hardcodeado — configuración externa obligatoria (`dotnet user-secrets` en desarrollo). Sin `[Authorize]` en ningún controller todavía (autorización es `TASK-049`/`050`, tarea posterior).

### Evolución futura

Si en el futuro se decide migrar a un proveedor externo (SSO corporativo, por ejemplo), esta decisión tendría que revisitarse explícitamente — no está descartado para siempre, solo no se eligió ahora.

---

# 61. ADR-058 — Autorización por permiso: IAuthorizationPolicyProvider dinámico sobre Permiso.Codigo

### Estado

`ACEPTADA`

### Contexto

TASK-016 necesitaba integrar `Usuario → UsuarioRol → Rol → RolPermiso → Permiso.Codigo` (ya implementado, TASK-010/013) con el JWT de TASK-015 para autorizar operaciones. ASP.NET Core ofrece varios mecanismos (`[Authorize(Roles=...)]`, Policies estáticas, `IAuthorizationPolicyProvider` personalizado) y la tarea prohibía explícitamente dos extremos: no registrar una Policy estática por cada `Permiso.Codigo` (serían decenas/cientos, duplicando el catálogo) y no usar `[Authorize(Roles=...)]` como mecanismo principal (el modelo del proyecto es `Rol → Permiso`, no autorización directa por nombre de rol).

También se encontró que ASP.NET Core remapea por defecto claims JWT cortos ("sub" → URI larga de WS-Federation) vía `JwtSecurityTokenHandler`, lo que habría impedido leer el `UsuarioId` emitido por `JwtTokenGenerator` (TASK-015) tal como se emitió.

### Decisión

`IAuthorizationPolicyProvider` personalizado (`PermisoPolicyProvider`) que resuelve dinámicamente cualquier nombre de policy con el prefijo `"Permiso:"` como un `PermisoRequirement(código)`, sin registrar policies estáticas. Un único `AuthorizationHandler<PermisoRequirement>` (`PermisoAuthorizationHandler`) consulta `UsuarioTienePermisoUseCase` (Application, nuevo — compone `IUsuarioRepository`/`IUsuarioRolRepository`/`IRolPermisoRepository` ya existentes, sin tabla ni repositorio nuevo) contra la base de datos en cada request (fuente de verdad = BD, no el JWT). Se agregó `JwtBearerOptions.MapInboundClaims = false` (único cambio a la configuración de TASK-015) para leer el claim `"sub"` sin remapear.

`UsuarioTienePermisoUseCase` también exige `Usuario.Activo == true` (RN-057) — un JWT emitido antes de desactivar al usuario no debe seguir otorgando autorización mientras no expire.

### Motivo

Mecanismo estándar y documentado de ASP.NET Core (no un framework de autorización propio) que evita tanto la explosión de policies estáticas como la autorización por nombre de rol, manteniendo `Permiso.Codigo` (RN-055) como único identificador funcional visible en `[Authorize(Policy = "Permiso:...")]`.

### Alternativas consideradas

- Una `AuthorizationPolicy` estática por cada permiso, registrada en `Program.cs`: descartada — TASK-016 §7 la prohíbe explícitamente (duplicaría el catálogo de `Permiso` en código).
- `[Authorize(Roles = "...")]`: descartada — TASK-016 §28 la prohíbe como mecanismo principal; el modelo del proyecto es permiso, no rol.
- Incluir permisos como claims dentro del JWT (evaluarlos sin consultar BD en cada request): descartada — TASK-016 §10 exige que la BD sea la fuente de verdad; cachear permisos en el token generaría inconsistencias si cambian entre la emisión del token y su expiración.

### Consecuencias

Nuevos archivos en `Api/Authorization/` (`PermisoRequirement`, `PermisoAuthorizationHandler`, `PermisoPolicyProvider`) y `Application/Organizacion/UsuarioTienePermisoUseCase.cs`. Dos endpoints reales protegidos: `POST /requisiciones/{id}/enviar` (`Permiso:REQUISICION_ENVIAR`) y `POST /requisiciones/{id}/aprobar` (`Permiso:REQUISICION_APROBAR`) — ver RN-057 sobre por qué solo estos dos. Sin cambios de esquema de base de datos (reutiliza el modelo ya auditado en TASK-014).

### Evolución futura

Cuando se defina el contrato de más endpoints (`TASK-049`/`050`), se protegen agregando `[Authorize(Policy = "Permiso:<CODIGO>")]` — no requiere tocar `PermisoPolicyProvider`/`PermisoAuthorizationHandler`, que ya son genéricos para cualquier código.

---

# 62. ADR-059 — Autorización por alcance: AuthorizationHandler basado en recurso (HttpContext.Request.RouteValues) + JWT como actor de negocio en Requisicion

### Estado

`ACEPTADA`

### Contexto

TASK-050 necesitaba autorización por alcance (¿sobre qué `Requisicion` puede operar el usuario?, además de "¿qué puede hacer?" ya resuelto en TASK-016/ADR-058). A diferencia del permiso (evaluable solo con claims), el alcance necesita el recurso concreto — la `Requisicion` identificada por `{id}` en la ruta — lo que exige autorización basada en recurso ("resource-based authorization"), un patrón estándar mencionado en la documentación oficial de ASP.NET Core pero que requiere obtener las route values dentro de un `AuthorizationHandler`.

Durante la implementación se intentó primero castear `AuthorizationHandlerContext.Resource` a `Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext` (el patrón documentado para MVC con pipeline de filtros clásica) — los tests de integración fallaron sistemáticamente (todo intento de autorización por alcance devolvía `403`, incluso para el mismo usuario/empresa). Un diagnóstico explícito (`Resource.GetType().FullName`) confirmó que, con el enrutamiento de endpoints usado por este proyecto (ASP.NET Core 10, minimal hosting), `AuthorizationHandlerContext.Resource` es en realidad el propio `Microsoft.AspNetCore.Http.HttpContext` (`DefaultHttpContext`), no un `AuthorizationFilterContext`.

### Decisión

`AlcanceRequisicionAuthorizationHandler` castea `context.Resource` a `HttpContext` y lee el `{id}` de la ruta vía `httpContext.Request.RouteValues["id"]`. Se apila junto al permiso mediante un segundo atributo `[Authorize(Policy = "AlcanceRequisicion")]` en el mismo endpoint (semántica AND nativa de ASP.NET Core al combinar varios `[Authorize]`), evaluado por una Policy estática (no dinámica, a diferencia de `Permiso:...`, porque no varía por parámetro). Delega en `UsuarioTieneAlcanceSobreRequisicionUseCase` (Application, nuevo — compone `IUsuarioRepository`/`IRequisicionRepository` ya existentes), que compara `Usuario.Empresa.Id == Requisicion.Empresa.Id` (RN-058). Si la `Requisicion` no existe, el use case devuelve `true` (permite continuar) para que el `404` real lo reporte `RequisicionFinder.ObtenerOLanzar` — el alcance no debe enmascarar "no existe" detrás de "sin autorización".

Adicionalmente, `RequisicionesController.Enviar`/`Aprobar` dejan de leer `X-Usuario-Id`: el `UsuarioId` para `Historial` ahora se obtiene del claim `sub` del JWT ya validado por `[Authorize]` (RN-058) — decisión confirmada explícitamente por el usuario.

### Motivo

Autorización por alcance requiere el recurso; el único mecanismo confiable para obtenerlo dentro del pipeline de autorización de este proyecto (endpoint routing) es `HttpContext.Request.RouteValues`, verificado empíricamente. Mantener el alcance como Policy/Requirement/Handler (en vez de una validación manual dentro del Controller) preserva la separación exigida por TASK-050 §22 ("Authorization" como capa distinta de Domain/Application) y reutiliza el mismo mecanismo de TASK-016 (TASK-050 §16 lo exige explícitamente).

### Alternativas consideradas

- Cast a `AuthorizationFilterContext` (patrón documentado para MVC clásico): descartada — no es el tipo real de `Resource` con enrutamiento de endpoints en este proyecto (falla en tiempo de ejecución, confirmado empíricamente).
- Verificar el alcance manualmente dentro del Controller (llamando a `UsuarioTieneAlcanceSobreRequisicionUseCase` antes de invocar el caso de uso, lanzando una excepción mapeada a 403): más simple y sin dependencia de mecánica interna de ASP.NET Core, pero mezclaría una decisión de autorización dentro del Controller en vez de mantenerla en la capa `Authorization` — descartada por preferencia explícita de TASK-050 §22, no por incapacidad técnica.
- Dejar `X-Usuario-Id` sin tocar para el actor de negocio: descartada — el propio código (`EnviarRequisicionUseCase`/`AprobarRequisicionUseCase`, comentarios desde TASK-029/033) ya anticipaba la sustitución por la identidad autenticada; mantenerlo habría permitido que un usuario autenticado y con alcance válido quedara registrado como otra persona en el `Historial`.

### Consecuencias

Nuevos archivos: `Api/Authorization/{AlcanceRequisicionRequirement, AlcanceRequisicionAuthorizationHandler}`, `Application/Requisiciones/UsuarioTieneAlcanceSobreRequisicionUseCase.cs`. `RequisicionesController.Enviar`/`Aprobar` ganan `[Authorize(Policy = "AlcanceRequisicion")]` y dejan de usar `IdentidadTemporal` para el actor. Sin cambios de esquema de base de datos. Alcance por sede queda explícitamente sin implementar (RN-058) — ambigüedad reportada, no resuelta.

### Evolución futura

Si se define el contrato de alcance por sede, se puede agregar un nuevo `IAuthorizationRequirement` siguiendo el mismo patrón (recurso vía `HttpContext.Request.RouteValues`), apilado como un tercer `[Authorize]`. El mismo patrón de acceso al recurso (`HttpContext.Request.RouteValues`, no `AuthorizationFilterContext`) debe reutilizarse para cualquier autorización basada en recurso futura en este proyecto — queda documentado aquí para no repetir el mismo diagnóstico.

---

# 63. Registro de futuras decisiones

Las nuevas decisiones importantes deben agregarse al final utilizando el siguiente formato:

```markdown
# ADR-XXX — Título

### Estado

`PROPUESTA`

### Contexto

¿Qué problema estamos resolviendo?

### Decisión

¿Qué decidimos?

### Motivo

¿Por qué?

### Alternativas consideradas

¿Qué otras opciones evaluamos?

### Consecuencias

¿Qué ventajas y costos genera?

### Evolución futura

¿Cómo podría cambiar posteriormente?
```

---

# 64. Regla para modificar una decisión existente

Una decisión aceptada no debe cambiarse silenciosamente.

Si una nueva necesidad obliga a modificarla:

```text
Decisión anterior
       ↓
Nuevo problema/requisito
       ↓
Análisis
       ↓
Nueva decisión
       ↓
Decisión anterior = REEMPLAZADA
```

Debe quedar documentado:

- qué cambió;
- por qué cambió;
- qué impacto tiene;
- qué partes del sistema deben modificarse.

---

# 65. Principio final de arquitectura

La arquitectura de AuropaqPedidos debe buscar el siguiente equilibrio:

```text
              ESCALABILIDAD
                    ▲
                    │
                    │
 SIMPLICIDAD ◄──────┼──────► MANTENIBILIDAD
                    │
                    │
                    ▼
              TRAZABILIDAD
```

El objetivo no es construir la arquitectura más sofisticada.

El objetivo es construir una arquitectura que:

- represente correctamente el negocio;
- pueda evolucionar;
- sea comprensible;
- sea mantenible;
- sea segura;
- conserve trazabilidad;
- permita incorporar nuevas empresas y sedes;
- reduzca trabajo manual;
- pueda probarse;
- y no introduzca complejidad innecesaria.

La arquitectura debe servir al negocio y no convertirse en el negocio.

---

# 66. Regla maestra del proyecto

Ante cualquier nueva funcionalidad, antes de escribir código se debe responder:

```text
1. ¿Qué problema de negocio resuelve?

2. ¿Quién realiza la acción?

3. ¿Qué regla de negocio aplica?

4. ¿Qué entidades participan?

5. ¿Qué estado tienen?

6. ¿Qué relaciones existen?

7. ¿Qué permisos se necesitan?

8. ¿Qué datos deben persistirse?

9. ¿Qué API representa el caso de uso?

10. ¿Qué ocurre si algo falla?

11. ¿Cómo se prueba?

12. ¿Qué impacto tiene sobre la arquitectura existente?
```

Si estas preguntas no pueden responderse razonablemente, la funcionalidad todavía no está suficientemente definida para implementarse.

---

## Principio rector

> **Primero entendemos el problema. Después diseñamos la solución. Finalmente escribimos el código.**
