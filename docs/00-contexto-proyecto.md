# CONTEXTO DEL PROYECTO

## Sistema de Gestión de Requisiciones y Pedidos - Auropaq

**Proyecto:** AuropaqPedidos
**Tipo:** Aplicación web empresarial
**Propósito:** Digitalización y gestión del proceso interno de requisiciones, consolidación, compras y entregas.

---

# 1. Propósito del proyecto

AuropaqPedidos es un sistema web diseñado para digitalizar y centralizar el proceso mediante el cual las empresas pertenecientes a un holding realizan solicitudes mensuales de productos de:

- Aseo
- Cafetería
- Papelería
- Botiquín

El sistema busca reemplazar progresivamente el proceso actual basado principalmente en archivos Excel, correos electrónicos, búsquedas manuales de productos, digitación repetitiva y registros independientes de trazabilidad.

El objetivo es construir una solución **escalable, mantenible y configurable**, capaz de incorporar nuevas empresas, sedes, usuarios, roles, productos y proveedores sin necesidad de modificar el código fuente para cada incorporación.

> **Declaración formal del problema (refinamiento 2026-09-11):**
>
> Auropaq Pedidos gestiona y da trazabilidad al ciclo de una necesidad interna de compra, desde su registro mediante una requisición, pasando por su aprobación y consolidación, hasta la gestión de pedidos a proveedores, entregas y trazabilidad operativa de facturación.
>
> **El sistema NO pretende ser un ERP ni un sistema contable.** No implementa asientos contables, cuentas contables, causación, pagos, conciliación bancaria, retenciones, libros contables ni declaraciones tributarias. Ver `01-reglas-negocio.md` (RN-038) y `07-decisiones-arquitectura.md` (ADR-041) para el detalle de esta decisión y `9.1`/`9.2` de este documento para el alcance funcional vigente.

---

# 2. Contexto empresarial

El sistema será utilizado inicialmente dentro de un holding empresarial compuesto por diferentes empresas.

Entre las empresas involucradas actualmente se encuentran:

- FAVIPAQ
- COURIERBOX ÁLAMOS
- AUROTECH
- COURIERBOX

Estas empresas pueden realizar solicitudes de productos para sus necesidades operativas.

Existe un proceso centralizado de gestión de solicitudes y compras. Actualmente, una persona encargada recibe y consolida las solicitudes de las diferentes empresas y posteriormente gestiona la compra con uno o varios proveedores.

El sistema debe permitir que este proceso sea gestionado digitalmente y que la solución pueda crecer a medida que se incorporen nuevas empresas.

---

# 3. Problema actual

El proceso actual presenta varios problemas:

1. Las solicitudes se realizan mediante archivos Excel.
2. Los nombres de los productos pueden ser informales o variar entre solicitudes.
3. La clasificación de productos se realiza manualmente.
4. La identificación del código interno del producto se realiza mediante búsquedas manuales.
5. La información debe ser digitada nuevamente en la plataforma del proveedor.
6. El proveedor utiliza sus propios códigos y referencias.
7. Existe duplicación de información y trabajo manual.
8. La trazabilidad del proceso se mantiene parcialmente mediante archivos Excel personales.
9. La información histórica no se encuentra centralizada.
10. El proceso puede generar errores humanos durante la clasificación, digitación y consolidación.
11. Las empresas pueden tener múltiples sedes y los productos deben llegar a la ubicación correspondiente.
12. Los productos que no existen en el catálogo requieren una gestión especial.
13. No existe actualmente un flujo digital completo desde la solicitud hasta la entrega.

---

# 4. Proceso que se busca digitalizar

El proceso general que se desea representar mediante el sistema es:

```text
Empresa
   ↓
Solicitud mensual
   ↓
Revisión
   ↓
Homologación / validación de productos
   ↓
Aprobación
   ↓
Consolidación de necesidades
   ↓
Pedido a proveedor
   ↓
Distribución por empresa y sede
   ↓
Entrega
   ↓
Factura
   ↓
Cierre
```

Este flujo representa el objetivo general del sistema.

No significa que todas las etapas deban implementarse inmediatamente.

El desarrollo será incremental mediante funcionalidades pequeñas y verificables.

---

# 5. Conceptos principales del negocio

El sistema debe diferenciar claramente los siguientes conceptos:

### Empresa

Representa una empresa perteneciente al holding.

Las empresas son datos configurables y **no deben estar codificadas directamente en el sistema**.

---

### Sede

Representa una ubicación física perteneciente a una empresa.

Una empresa puede tener una o varias sedes.

Una sede puede ser utilizada como destino de los productos solicitados o comprados.

---

### Usuario

Representa una persona que utiliza el sistema.

Un usuario puede estar relacionado con una empresa y puede tener permisos determinados mediante roles.

---

### Rol

Representa un conjunto de responsabilidades dentro del sistema.

Los roles no deben depender de nombres específicos de personas.

Ejemplo:

```text
Solicitante
Revisor
Administrador
Compras
```

Estos nombres son ejemplos y deberán definirse formalmente en las reglas de negocio y seguridad.

---

### Producto

Representa un producto oficial del catálogo interno.

Un producto puede tener:

- Código interno
- Nombre
- Categoría
- Unidad de medida
- Estado

El catálogo interno es independiente de los códigos utilizados por los proveedores.

---

### Producto no catalogado

Representa un producto solicitado por un usuario que todavía no existe como producto oficial del catálogo.

Debe existir un proceso para revisar este tipo de solicitudes.

Las posibles decisiones incluyen:

```text
Homologar con producto existente
Crear nuevo producto
Rechazar solicitud
```

---

### Periodo

Representa el periodo operativo mensual sobre el cual se realizan las requisiciones.

El periodo debe permitir definir una ventana de fechas durante la cual las empresas pueden enviar sus solicitudes.

Las fechas no deben estar codificadas directamente en el sistema.

---

### Requisición

Representa la necesidad interna de una empresa durante un periodo determinado.

Una requisición:

- pertenece a una empresa;
- pertenece a un periodo;
- contiene uno o varios productos;
- contiene cantidades solicitadas;
- puede distribuir cantidades entre diferentes sedes;
- tiene un estado;
- tiene historial de acciones.

La requisición representa **la necesidad interna**, no la compra al proveedor.

---

### Consolidación

Representa la agrupación de las necesidades aprobadas de diferentes empresas.

Su objetivo es determinar la cantidad total requerida de cada producto.

La consolidación debe mantener la trazabilidad hacia las requisiciones originales.

---

### Pedido al proveedor

Representa la compra realizada a un proveedor.

Un pedido puede contener productos provenientes de la consolidación de varias empresas.

Por esta razón, el pedido al proveedor **no pertenece directamente a una única empresa**.

---

### Entrega

Representa la entrega física realizada por el proveedor.

Un pedido puede tener una o varias entregas.

Las entregas pueden ser parciales.

---

### Factura

Representa el registro operativo mínimo de la facturación del proveedor, necesario para trazabilidad de compras. No es un registro contable (RN-038, ADR-041).

Para el alcance inicial, un pedido puede respaldarse en varias facturas, pero cada factura pertenece a un único pedido (RN-039, ADR-042). El caso general (una factura cubriendo varios pedidos) permanece como decisión pendiente — ver `01-reglas-negocio.md §15`, matriz de decisiones, D-06.

---

# 6. Objetivos del sistema

## Objetivo general

Digitalizar y centralizar el proceso de gestión de requisiciones y compras de las empresas del holding, proporcionando trazabilidad, control, reducción de tareas manuales y capacidad de crecimiento.

## Objetivos específicos

- Centralizar las requisiciones.
- Estandarizar el catálogo de productos.
- Reducir la digitación manual.
- Gestionar productos no catalogados.
- Permitir múltiples empresas.
- Permitir múltiples sedes por empresa.
- Controlar usuarios, roles y permisos.
- Gestionar ventanas de solicitud.
- Mantener trazabilidad de las acciones.
- Consolidar necesidades de diferentes empresas.
- Asociar productos internos con referencias de proveedores.
- Gestionar pedidos a proveedores.
- Gestionar entregas parciales.
- Mantener histórico de las operaciones.
- Evitar depender de archivos Excel para la trazabilidad principal.

---

# 7. Principios fundamentales del proyecto

Estos principios deben respetarse durante el desarrollo.

## 7.1. La solución debe ser genérica

No se debe construir una solución exclusivamente para las empresas actuales.

El sistema debe permitir agregar nuevas empresas mediante configuración y datos.

No se deben utilizar condiciones como:

```text
if empresa == "AUROTECH"
```

para implementar reglas generales del sistema.

---

## 7.2. No hardcodear información empresarial

No se deben hardcodear:

- Empresas
- Sedes
- Usuarios
- Nombres de personas
- Roles específicos de personas
- Productos
- Proveedores
- Códigos de productos
- Fechas de solicitud

Esta información debe ser gestionada mediante datos y configuración.

---

## 7.3. Separar necesidad de compra

El sistema debe diferenciar:

```text
Lo que una empresa necesita
```

de:

```text
Lo que finalmente se compra
```

Por ejemplo:

```text
Necesidad total: 85 unidades
Compra al proveedor: 100 unidades
```

La diferencia puede producirse por empaques, mínimos de compra, disponibilidad u otras reglas del proveedor.

> **Aclaración (auditoría 2026-09-10):** "Lo que una empresa necesita" (la necesidad total ya agrupada) **no es una entidad independiente**. Está representada por `DetalleConsolidacion.CantidadNecesaria` (`02-dominio.md §19`, `04-base-datos.md §24`), ya implementada. La diferencia con "lo que finalmente se compra" se representa en la etapa siguiente, mediante `DetallePedidoProveedor.CantidadNecesaria`/`CantidadPedida` (`02-dominio.md §22`, `04-base-datos.md §27`), que pertenece a `PedidoProveedor`.

---

## 7.4. Mantener trazabilidad

El sistema debe permitir responder preguntas como:

```text
¿Quién creó la requisición?
¿Cuándo la creó?
¿Quién la envió?
¿Quién la revisó?
¿Por qué fue devuelta?
¿Quién la aprobó?
¿Qué productos fueron solicitados?
¿Qué cantidad se solicitó?
¿Para qué sede?
¿Qué cantidad se consolidó?
¿Qué cantidad se compró?
¿Qué cantidad se entregó?
¿Cuándo se entregó?
```

La trazabilidad es una característica fundamental del sistema.

---

## 7.5. El backend es la fuente de verdad

Las reglas de negocio importantes deben validarse en el backend.

El frontend puede realizar validaciones para mejorar la experiencia del usuario, pero no debe ser la única capa responsable de garantizar las reglas del negocio.

---

## 7.6. Diseñar antes de implementar

Antes de implementar una funcionalidad importante se debe comprender:

```text
Problema
↓
Reglas de negocio
↓
Entidades
↓
Relaciones
↓
Estados
↓
Casos de uso
↓
API
↓
Implementación
↓
Pruebas
```

No se debe comenzar creando tablas, controladores o componentes React sin comprender primero el comportamiento requerido.

---

# 8. Alcance inicial

El primer MVP se enfocará en el flujo de requisiciones.

El alcance inicial será:

```text
Login
  ↓
Empresa
  ↓
Sede
  ↓
Periodo
  ↓
Requisición mensual
  ↓
Agregar productos
  ↓
Modificar cantidades
  ↓
Distribuir cantidades por sede
  ↓
Guardar borrador
  ↓
Enviar requisición
  ↓
Revisión
  ↓
Aprobar / devolver
  ↓
Corregir y reenviar
```

---

# 9. Funcionalidades fuera del primer MVP

Las siguientes funcionalidades forman parte de la evolución futura del sistema, pero no deben implementarse durante el primer MVP salvo que una tarea específica las autorice:

- Consolidación de compras
- Pedidos a proveedores
- Integración con plataformas externas de proveedores
- Entregas
- Entregas parciales
- Facturación
- Dashboards avanzados
- Notificaciones avanzadas
- Redis
- CQRS
- Microservicios
- Docker
- Arquitecturas distribuidas
- Integraciones externas

Estas funcionalidades no deben desarrollarse anticipadamente.

> **Nota (refinamiento 2026-09-11):** las secciones 8 y 9 describen el alcance del **primer MVP** (flujo de requisiciones), que ya se completó (`progreso.md`, TASK-022 a TASK-035). Consolidación, Pedidos a proveedor, Entregas y Facturación —listadas arriba como "fuera del primer MVP"— ya cuentan con una primera implementación técnica (Domain/Application/Infrastructure, y Facturación también con API), autorizada explícitamente sesión a sesión, con varias decisiones de negocio todavía sin aprobar formalmente (ver `01-reglas-negocio.md §15` y la matriz de decisiones). El **alcance funcional vigente** del proyecto —más allá de ese primer MVP— es el de las secciones 9.1 y 9.2 siguientes, que reemplazan a esta sección como referencia de alcance total del proyecto.

---

## 9.1 Alcance funcional vigente

El alcance funcional del proyecto, más allá del primer MVP de requisiciones, es:

1. Configuración básica (Empresa, Sede, Usuario, Rol, Permiso, Producto, Proveedor, Periodo).
2. Requisiciones.
3. Aprobación (revisión, aprobar, devolver, corregir, reenviar).
4. Consolidación.
5. Gestión de compra (diferencia entre necesidad y pedido — RN-031).
6. Pedidos a proveedores.
7. Distribución de pedidos (por sede).
8. Entregas parciales y múltiples.
9. Distribución de entregas.
10. Registro operativo de facturas (RN-038/RN-039/RN-040).
11. Trazabilidad de todo lo anterior.

Este es el alcance funcional objetivo; no implica que todo deba implementarse de una sola vez, ni que lo ya construido esté completo — varias piezas siguen `EN_DESARROLLO` con decisiones de negocio pendientes (ver `progreso.md` y la matriz de `01-reglas-negocio.md §15`).

## 9.2 Fuera de alcance (permanente)

A diferencia de la sección 9 (funcionalidades diferidas del primer MVP, que sí se espera construir después), lo siguiente está fuera del alcance del proyecto salvo que el negocio lo solicite explícitamente en el futuro:

- Contabilidad: asientos contables, cuentas contables, causación.
- Pagos y su seguimiento.
- Conciliación bancaria, tesorería.
- Retenciones contables, libros contables, declaraciones tributarias.
- Un ERP financiero completo.
- Microservicios, CQRS completo, event sourcing.
- Redis, Kubernetes.
- Docker como requisito funcional (puede usarse como herramienta de despliegue, no como parte del dominio).
- Cualquier otra funcionalidad no necesaria para resolver el problema descrito en la sección 1.

Ver RN-038 (`01-reglas-negocio.md`) y ADR-041 (`07-decisiones-arquitectura.md`) para el detalle de la exclusión de contabilidad/ERP.

---

# 10. Stack tecnológico objetivo

El proyecto utilizará inicialmente:

## Backend

- C#
- .NET
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server

## Frontend

- React
- Tailwind CSS

## Control de versiones

- Git

La tecnología debe estar al servicio del dominio y de las necesidades del sistema.

No se deben introducir nuevas tecnologías, librerías o patrones arquitectónicos sin una justificación y autorización correspondiente.

---

# 11. Arquitectura evolutiva

La solución debe comenzar con una arquitectura suficientemente clara para separar responsabilidades, pero sin introducir complejidad innecesaria.

El sistema debe poder evolucionar posteriormente sin requerir una reconstrucción completa.

Las decisiones arquitectónicas específicas estarán documentadas en:

```text
docs/03-arquitectura.md
docs/07-decisiones-arquitectura.md
```

---

# 12. Regla para Claude Code

Claude Code debe utilizar este documento como contexto general del proyecto.

Antes de implementar una funcionalidad debe consultar también los documentos específicos relacionados con dicha funcionalidad.

Claude Code:

- No debe inventar requisitos.
- No debe modificar decisiones de negocio por iniciativa propia.
- No debe implementar funcionalidades fuera del alcance de la tarea.
- No debe hardcodear información empresarial.
- No debe introducir tecnologías no autorizadas.
- No debe realizar refactorizaciones no relacionadas con la tarea.
- Debe respetar las reglas de negocio documentadas.
- Debe señalar cualquier ambigüedad que pueda afectar el diseño.

Las instrucciones operativas completas para Claude Code estarán definidas en:

```text
/CLAUDE.md
```

---

# 13. Documentos de referencia

La documentación del proyecto se organiza de la siguiente manera:

```text
docs/
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

Cada documento tiene una responsabilidad específica.

No se deben duplicar reglas o decisiones entre documentos sin necesidad.

Cuando exista una contradicción entre documentos, se debe identificar la contradicción antes de implementar.

---

# 14. Estado actual del proyecto

Este documento representa el **contexto general del proyecto**.

Las reglas detalladas del negocio todavía deben definirse formalmente en:

```text
docs/01-reglas-negocio.md
```

El modelo de dominio se definirá en:

```text
docs/02-dominio.md
```

La arquitectura técnica se definirá en:

```text
docs/03-arquitectura.md
```

La base de datos se definirá en:

```text
docs/04-base-datos.md
```

La API se definirá en:

```text
docs/05-api.md
```

La seguridad se definirá en:

```text
docs/06-seguridad.md
```

Las decisiones arquitectónicas se registrarán en:

```text
docs/07-decisiones-arquitectura.md
```

Las tareas autorizadas de implementación estarán en:

```text
docs/08-tareas.md
```

---

# 15. Regla principal del proyecto

> **Primero definimos el negocio y la arquitectura. Después implementamos.**

Claude Code es responsable de ayudar a implementar las decisiones documentadas.

Claude Code **no es responsable de decidir por sí mismo cómo debe funcionar el negocio**.
