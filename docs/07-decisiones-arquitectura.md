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

# 63. ADR-060 — Política de cierre de fase antes de avanzar

### Estado

`ACEPTADA`

### Contexto

`08-tareas.md` §5/§20/§21 define un orden de fases (FASE 0 a FASE 13) y establece explícitamente que
las fases posteriores al MVP (Consolidación, Pedidos a proveedor, Entregas, Facturación, Seguridad
avanzada, Calidad, Rendimiento, Despliegue) **no deben iniciarse** hasta que el primer flujo funcional
(TASK-001 a TASK-035, Fases 0-5) esté estable.

Una auditoría del estado real del proyecto (2026-09-15) confirmó que esto no se respetó: se avanzó en
paralelo, en forma de vertical slices (`CLAUDE.md §55`), sobre Fases 6, 7, 8, 9 y 10 mientras varias
tareas de Fases 0-5 seguían sin cerrar (`TASK-001` seguía `PENDIENTE`; `TASK-031`/`TASK-032` seguían
`PENDIENTE`; ninguna tarea de Fases 1-5 cumplía los 8 criterios de `08-tareas.md` §24 para
`COMPLETADA`). El resultado es un tablero (`progreso.md`) donde casi todo está `EN_DESARROLLO` a la
vez, sin un flujo end-to-end formalmente cerrado, y con el riesgo de que la documentación de progreso
se desincronice del código real (ya ocurrió una vez: la tabla de `FASE 0` reportaba conteos de pruebas
obsoletos respecto al historial real — ver informe de `TASK-001`, `progreso.md`).

### Decisión

1. **No se revierte el trabajo ya construido en Fases 6-10.** Es código real, compilado y en varios
   casos probado (`Api.Tests` cubre Consolidación/Pedidos/Entregas/Facturación end-to-end). Revertirlo
   violaría `CLAUDE.md §46` (no eliminar código sin justificación) sin ningún beneficio real.
2. **Se declara formalmente el estado real como una desviación documentada, no un error silencioso.**
   Este ADR es esa declaración.
3. **A partir de esta decisión, se aplica un gate de cierre de fase — con una excepción explícita para
   trabajo ya empezado:**
   - **Bloqueado:** iniciar cualquier tarea que hoy está `PENDIENTE` (sin código real todavía) en una
     fase `N+1` mientras exista al menos una tarea de la fase `N` que no esté `COMPLETADA` según los 8
     criterios de `08-tareas.md` §24 (compila, pruebas pasan, cumple criterios de aceptación, respeta
     reglas de negocio/arquitectura, sin dependencias no autorizadas, sin cambios fuera de alcance, sin
     lógica hardcodeada, documentación actualizada).
   - **Permitido:** continuar y cerrar (llevar a `COMPLETADA`) una tarea que ya está `EN_DESARROLLO`
     en Fases 6-10, aunque su fase esté por delante de una fase anterior todavía sin cerrar. Esto no es
     "trabajo nuevo": es terminar algo que ya existe, evitando dejarlo indefinidamente a medio construir.
   - Con el estado real de `progreso.md` al 2026-09-15, esto significa en concreto:
     - **Se puede seguir cerrando** (ya `EN_DESARROLLO`, código real existente): `TASK-036` a `TASK-046`,
       `TASK-048` a `TASK-050`.
     - **Sigue bloqueado hasta cerrar el MVP** (hoy `PENDIENTE`, sin código): `TASK-047`, `TASK-051`, y
       toda Fase 11-13 (Calidad, Rendimiento, Despliegue) completa.
     - Esta lista es un corte en el tiempo; si una tarea listada como "se puede seguir cerrando" llega a
       `COMPLETADA` y aparece una tarea *nueva* no contemplada en `08-tareas.md` para esa misma fase,
       esa tarea nueva cuenta como bloqueada, no como continuación.
4. El orden concreto para cerrar lo que falta de Fases 0-5 (MVP) queda documentado en
   `progreso.md` (`## Próxima tarea`), no en este ADR, porque es una lista operativa que puede
   ajustarse sin que eso implique una nueva decisión arquitectónica.
5. Una vez que Fases 0-5 estén 100% `COMPLETADA`, se retoma el trabajo ya iniciado en Fases 6-10
   (y solo entonces se inician las fases que aún no tienen ningún código, como Calidad/Rendimiento/
   Despliegue), aplicando el mismo gate hacia adelante.

### Motivo

`08-tareas.md` §2 es explícito: "Claude Code no debe interpretar este documento como una autorización
para implementar todo el proyecto de una sola vez [...] Debe trabajar tarea por tarea [...] No se debe
comenzar la siguiente tarea hasta verificar que la anterior cumple sus criterios de aceptación." El
gate de esta decisión es la forma concreta de hacer cumplible esa regla hacia adelante, después de que
ya se incumplió una vez.

### Alternativas consideradas

- **Revertir el código de Fases 6-10 hasta cerrar el MVP:** descartada — descarta trabajo funcional
  real sin ningún beneficio (el código no es incorrecto, solo estaba fuera de secuencia), y viola
  `CLAUDE.md §46`.
- **Ignorar la desviación y seguir avanzando donde convenga sin gate:** descartada — es la causa raíz
  del problema (permite que el tablero de progreso se vuelva no confiable, como ya ocurrió), y
  contradice `08-tareas.md` §2/§21 explícitamente.
- **Reescribir `08-tareas.md` para reordenar las fases según lo ya construido:** descartada — el orden
  de `08-tareas.md` §20 (LOGIN → EMPRESA → SEDE → PERIODO → REQUISICIÓN → ... → REENVIAR) sigue siendo
  el flujo de negocio correcto y deseado; el problema no es el orden documentado, es que no se respetó.
- **Gate total sin excepción (bloquear incluso cerrar tareas ya `EN_DESARROLLO` en Fases 6-10 hasta
  terminar el MVP):** descartada por decisión explícita del usuario — dejaría trabajo real a medio
  construir indefinidamente (algunas de esas tareas están cerca de `COMPLETADA`) sin ningún beneficio
  adicional sobre la alternativa elegida; el riesgo que motiva el gate (documentación desincronizada,
  demasiados frentes abiertos a la vez) ya queda controlado bloqueando solo el trabajo **nuevo**.

### Consecuencias

Ninguna tarea `PENDIENTE` de Fase 6 en adelante recibe trabajo nuevo hasta que Fases 0-5 estén
formalmente `COMPLETADA` — en particular, `TASK-047`, `TASK-051` y toda Fase 11-13 quedan bloqueadas.
Las tareas de Fases 6-10 que ya están `EN_DESARROLLO` (`TASK-036` a `TASK-046`, `TASK-048` a `TASK-050`)
sí pueden seguir avanzando hasta `COMPLETADA` — no se las deja a medio terminar. Esto evita acumular
trabajo `EN_DESARROLLO` en frentes completamente nuevos, sin desperdiciar el esfuerzo ya invertido en
los frentes que sí están en marcha. No requiere cambios de código ni de esquema de base de datos — es
una decisión de proceso.

### Evolución futura

Si en el futuro el equipo decide que el desarrollo en paralelo por fases es preferible (por ejemplo,
con el equipo dividido en módulos independientes), eso debe registrarse como una nueva decisión que
reemplace explícitamente este ADR (`§68` — Regla para modificar una decisión existente), no asumirse
de nuevo tácitamente.

---

# 64. ADR-061 — Exponer API real de administración de Rol/Permiso/UsuarioRol/UsuarioSede/RolPermiso

### Estado

`ACEPTADA`

### Contexto

`TASK-009` a `TASK-013` (`08-tareas.md`) implementaron `UsuarioSede`/`Rol`/`Permiso`/`UsuarioRol`/
`RolPermiso` completos en Domain/Application/Infrastructure, pero sin ningún endpoint HTTP.
`05-api.md §55.3` documentaba esto como una decisión **"confirmada, no pendiente"** (de una
auditoría previa, etiquetada "TASK-014"). Sin embargo, la nota de esa misma auditoría dentro de
`08-tareas.md` (`TASK-012`) decía lo contrario: que estas tareas no se marcaban `COMPLETADA`
**justamente porque** les faltaba API — es decir, la propia auditoría dejó la pregunta abierta en
vez de cerrarla, y dos documentos terminaron afirmando cosas opuestas sobre el mismo punto.

Detectada la contradicción (sesión 2026-09-15, al retomar `TASK-009/010/011/012/013` desde
`progreso.md`), se presentó al usuario en vez de resolverla unilateralmente — con 3 alternativas:
construir la API ahora, dejarlas sin API redefiniendo el criterio de cierre, o decidir caso por
caso. El usuario eligió explícitamente: **construir la API ahora**.

### Decisión

Se expone un contrato HTTP real de administración para estos 5 conceptos (documentado en
`05-api.md §59` a `§63`):

```text
GET/POST  /api/v1/roles
GET/POST  /api/v1/permisos
GET/POST  /api/v1/usuarios/{usuarioId}/roles
GET/POST  /api/v1/usuarios/{usuarioId}/sedes
GET/POST  /api/v1/roles/{rolId}/permisos
```

Los Controllers son delgados: traducen HTTP a los casos de uso de Application que **ya existían**
(`CrearRolUseCase`, `ListarRolesUseCase`, `CrearPermisoUseCase`, `ListarPermisosUseCase`,
`AsignarRolAUsuarioUseCase`, `ObtenerRolesDeUsuarioUseCase`, `AsignarUsuarioASedeUseCase`,
`ObtenerSedesAutorizadasUseCase`, `AsignarPermisoARolUseCase`, `ObtenerPermisosDeRolUseCase`) —
esta decisión no agrega lógica de negocio nueva, solo la expone.

### Motivo

Sin una forma de crear roles/permisos y asignarlos que no dependa de escribir SQL a mano, el
sistema no tiene un camino real hacia la autorización por permisos que ya exige `TASK-049`/`050`
sobre más endpoints en el futuro — alguien tiene que poder administrar ese catálogo. Mantener la
"confirmación" de `§55.3` habría dejado esa necesidad sin resolver indefinidamente.

### Alternativas consideradas

- **Mantener la decisión original (sin API)** y redefinir el criterio de `COMPLETADA` de estas 5
  tareas para que no la exija: descartada por el usuario — deja sin resolver cómo se administran
  roles/permisos en un entorno real (fuera de tests/seeds).
- **Decidir caso por caso** (API solo para Rol/Permiso, no para las relaciones N:N): descartada —
  sin una API de asignación, el catálogo de Rol/Permiso quedaría creado pero inalcanzable para
  relacionarlo con usuarios, igual que ya se había aclarado para `RolPermiso` en la auditoría
  `TASK-014` original (`08-tareas.md`, nota de `TASK-013`).

### Consecuencias

Nuevos Controllers (`RolesController`, `PermisosController`) y nuevos endpoints en
`UsuariosController`. Sin cambios de Domain/Application/Infrastructure (ya existían). Sin
`[Authorize]` todavía en ninguno de estos endpoints — se agrega en el mismo paso posterior que
cubre el resto de endpoints nuevos del MVP (ver `progreso.md`, `## Próxima tarea`, bloque A punto
8), no aquí. Sin `PUT`/`DELETE`/quitar-asignación (no solicitado, `05-api.md §63.3`).

### Evolución futura

Si más adelante se necesita revocar una asignación (quitar un rol de un usuario, un permiso de un
rol, una sede de un usuario), es una tarea nueva y acotada (un endpoint `DELETE` por relación), no
una extensión silenciosa de esta decisión.

---

# 65. ADR-062 — Catálogo definitivo de permisos y su mapeo a endpoints (punto 8 del bloque A)

### Estado

`ACEPTADA`

### Contexto

Al cerrar `TASK-031/032` (2026-09-15), el bloque de tareas que cierra el MVP (Fases 0-5) quedó con
un único punto abierto: autorización real (permiso + alcance) sobre ~50 endpoints nuevos que hoy
no tienen `[Authorize]`. `06-seguridad.md §9` solo documentaba permisos como "Ejemplos" para
Requisición/Producto/Pedido/Entrega/Factura — insuficiente para decidir qué permiso protege cada
endpoint real (Empresa, Sede, Usuario, Rol, Permiso, Categoría, UnidadMedida, Proveedor, Solicitud
de producto no catalogado, Periodo, y el resto de Requisición). Implementar `[Authorize]` sin este
catálogo habría significado inventar política de seguridad sobre la marcha, endpoint por
endpoint, sin coherencia entre módulos — exactamente lo que `CLAUDE.md §67`/`06-seguridad.md §71`
prohíben.

Por instrucción explícita del usuario ("analiza `docs/` y el dominio, toma una decisión coherente
y déjala documentada"), se analizó la carpeta `docs/` completa (especialmente `01-reglas-negocio.md`,
`02-dominio.md`, `06-seguridad.md` completo) y el dominio real (entidades, relaciones, flujos) para
decidir el catálogo y su mapeo — ver el detalle completo en RN-059 y `06-seguridad.md §9/§52`.

### Decisión

1. **Catálogo de permisos por módulo**, agrupando módulos sin ciclo de vida propio bajo el permiso
   del módulo que sí lo tiene (Categoría/UnidadMedida → `PRODUCTO_*`; Sede → `ORGANIZACION_*`;
   Usuario/Rol/Permiso/UsuarioRol/UsuarioSede/RolPermiso → `SEGURIDAD_*`), y permiso propio para
   los módulos con ciclo de vida independiente (Empresa, Proveedor, Periodo, Requisición). Ver la
   lista completa en `06-seguridad.md §9`.
2. **`PRODUCTO_SOLICITAR` como permiso nuevo**, distinto de `PRODUCTO_CREAR`/`PRODUCTO_EDITAR`,
   para enviar una solicitud de producto no catalogado — justificado porque la matriz conceptual
   de `06-seguridad.md §53` no le da a "Solicitante" el permiso de crear productos, pero sí debe
   poder solicitar uno que falta (RN-024).
3. **Alcance por empresa extendido a todos los endpoints de Requisición** (no solo Enviar/Aprobar),
   incluyendo una corrección explícita a `GET /requisiciones/{id}` (implementado en TASK-032 sin
   alcance, antes de este análisis completo de `06-seguridad.md §60/§61`).
4. **Sin alcance por empresa en Organización/Seguridad/Catálogo/Proveedor/Periodo** — son datos
   administrativos o de catálogo global, no pertenecen a una empresa específica.
5. **Mapeo completo endpoint → permiso → alcance documentado en `06-seguridad.md §52`.**

### Motivo

Sin un catálogo decidido, cada sesión futura que toque un endpoint nuevo tendría que volver a
decidir su seguridad desde cero, con riesgo real de incoherencia entre módulos similares (por
qué Producto sí necesita `PRODUCTO_EDITAR` y Proveedor no, por ejemplo). Decidirlo una sola vez,
con la lógica de negocio ya verificada contra el dominio real, evita ese riesgo y deja un punto de
referencia único para implementar el punto 8 del bloque A.

### Alternativas consideradas

- **Un único permiso `ADMIN` que lo cubra todo:** descartada — viola mínimo privilegio
  (`06-seguridad.md §19`/§45) y el principio explícito de no dar acceso total a cualquier
  administrador automáticamente (`§70`).
- **Un permiso por cada operación CRUD de cada entidad individual** (ej. `SEDE_CREAR` separado de
  `EMPRESA_CREAR`, `CATEGORIA_CREAR` separado de `PRODUCTO_CREAR`): descartada — ningún RN ni
  flujo de negocio documentado justifica que alguien pueda administrar Categorías pero no
  Productos, o Sedes pero no Empresas; habría sido complejidad sin necesidad real
  (`CLAUDE.md §43`/§71).
- **Reutilizar `PRODUCTO_CREAR` también para solicitar un producto no catalogado** (sin crear
  `PRODUCTO_SOLICITAR`): descartada — contradice explícitamente la matriz conceptual de
  `06-seguridad.md §53`, que no le da ese permiso al Solicitante.
- **Dejar alcance por empresa fuera de `GET /requisiciones/{id}`** (mantener lo ya implementado en
  TASK-032): descartada tras leer `06-seguridad.md §60/§61` completos — protegen explícitamente
  ese mismo endpoint como ejemplo de lo que debe cubrirse.

### Consecuencias

Este ADR **decide** el catálogo y el mapeo; **no implementa** los atributos `[Authorize]` ni siembra
los permisos nuevos — eso sigue siendo el punto 8 del bloque A (`progreso.md`, `## Próxima tarea`),
una tarea de código separada. La matriz rol↔permiso (`06-seguridad.md §53`) sigue siendo conceptual:
qué rol recibe cada permiso es una decisión de negocio distinta, todavía no tomada.

### Evolución futura

Si el negocio define roles reales (más allá de la matriz conceptual) o necesita permisos más
granulares dentro de un módulo agrupado (por ejemplo, separar quién administra Categorías de quién
administra Productos), debe registrarse como una decisión nueva que reemplace explícitamente este
ADR, no como una excepción silenciosa.

---

# 66. ADR-063 — Catálogo definitivo de roles y matriz rol↔permiso

### Estado

`ACEPTADA`

### Contexto

ADR-062 decidió el catálogo de permisos y su mapeo a endpoints, pero dejó explícitamente abierta
la asignación rol↔permiso ("`06-seguridad.md §53` sigue siendo conceptual"). Por instrucción
explícita del usuario ("revisa el catálogo/mapeo ya documentado, define bien las reglas de
negocio, y deja definida la asignación rol↔permiso antes de pasar a implementación"), se completó
esa asignación usando los 5 roles ya documentados en `06-seguridad.md §8` (nunca se propuso un rol
nuevo) contra el catálogo íntegro de permisos de ADR-062/RN-059.

### Decisión

1. Se preservan literalmente todas las casillas ya decididas en la matriz conceptual original
   (`06-seguridad.md §53` anterior a esta sesión).
2. Se completan las casillas nuevas (Organización, Seguridad, Proveedor, Periodo, `PRODUCTO_SOLICITAR`)
   siguiendo el criterio de mínimo privilegio: solo `ADMINISTRADOR` administra Organización/
   Seguridad/Periodo; `COMPRAS` administra Proveedor; todos los roles operativos ven
   `ORGANIZACION_VER`/`PERIODO_VER` (los necesitan para operar) pero no para administrar.
3. **Administrador NO recibe una excepción de alcance por empresa** — queda sujeto a la misma
   regla de alcance que cualquier otro rol para Requisición. La matriz conceptual original sí
   sugería ese bypass ("Ver requisiciones: Amplio"); se decide explícitamente no implementarlo
   todavía, por falta de un requisito de negocio documentado y por `06-seguridad.md §45`
   (las excepciones administrativas deben definirse explícitamente, nunca asumirse).
4. Se agrega una regla de negocio nueva, no derivada solo de la matriz sino de
   `06-seguridad.md §62` (prevención de escalamiento de privilegios): ningún usuario puede
   asignarse un rol o permiso a sí mismo, ni con `SEGURIDAD_ADMINISTRAR`. Debe implementarse como
   una validación explícita en `AsignarRolAUsuarioUseCase` al mismo tiempo que el punto 8.

Detalle completo, fila por fila, en RN-060 y la matriz en `06-seguridad.md §53`.

### Motivo

Sin esta matriz, sembrar `RolPermiso` en el punto 8 habría requerido decidir la asignación
ad hoc en medio de la implementación — exactamente el patrón que ya generó la ambigüedad
resuelta en ADR-062. Decidirla antes, con la lógica de negocio explícita, deja la implementación
como trabajo puramente mecánico (crear los registros, aplicar los atributos).

### Alternativas consideradas

- **Dar a `Administrador` alcance amplio (sin restricción de empresa) como sugería la matriz
  original:** descartada por ahora — no hay necesidad de negocio documentada, y el mecanismo de
  alcance actual (`AlcanceRequisicionAuthorizationHandler`) no distingue roles; agregar esa
  distinción es una ampliación de alcance mayor que nadie pidió todavía. Queda como evolución
  futura explícita, no como decisión de esta sesión.
- **Inventar roles más granulares** (ej. separar "Gestor de catálogo" de "Gestor de
  requisiciones", dado que `GESTOR_REQUISICIONES` termina con permisos de ambos): descartada — la
  matriz original ya le daba ambos conjuntos de permisos al mismo rol; dividirlo sería cambiar una
  decisión de negocio ya tomada sin que el usuario lo pidiera.
- **No decidir la prevención de auto-escalamiento ahora** (dejarla para cuando se implemente el
  punto 8, sin registrarla como regla): descartada — `06-seguridad.md §62` ya la exige
  explícitamente; documentarla ahora evita que se omita al implementar.

### Consecuencias

`06-seguridad.md §8/§53` dejan de ser conceptuales. El punto 8 del bloque A (`progreso.md`) puede
implementarse sin ninguna decisión de negocio pendiente sobre permisos, roles o su asignación.
`AsignarRolAUsuarioUseCase` necesitará conocer la identidad del usuario que ejecuta la operación
(hoy no la recibe) para poder rechazar la auto-asignación — esto es un cambio de firma menor a
implementar junto con el resto del punto 8, no una decisión de negocio nueva.

### Evolución futura

Si el negocio confirma la necesidad de un superadministrador con alcance amplio entre empresas, o
de roles más granulares, deben registrarse como decisiones nuevas que reemplacen explícitamente
los puntos 3 y 2 de esta decisión respectivamente — no como excepciones silenciosas.

---

# 67. Registro de futuras decisiones

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

# 68. Regla para modificar una decisión existente

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

# 69. Principio final de arquitectura

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

# 70. Regla maestra del proyecto

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
