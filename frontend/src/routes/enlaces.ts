// Único lugar donde se declara qué permiso necesita cada sección de la app — TASK-102
// (docs/2026-09-18-auditoria-dominio-roles-frontend.md). Usado tanto para filtrar el menú
// (AppLayout.tsx) como para proteger la ruta en sí (AppRoutes.tsx vía RutaConPermiso), para que
// ambos no puedan desincronizarse. Un enlace es visible/accesible si el usuario tiene AL MENOS
// UNO de los permisos listados — misma lógica de "cualquiera de los permisos que protegen algún
// endpoint de esa pantalla" (docs/06-seguridad.md §52/§53), no una lista arbitraria.
export interface EnlaceNav {
  ruta: string;
  etiqueta: string;
  permisos: readonly string[];
}

export const ENLACES: readonly EnlaceNav[] = [
  { ruta: "/catalogo", etiqueta: "Catálogo", permisos: ["PRODUCTO_VER", "ORGANIZACION_VER", "PROVEEDOR_VER", "PERIODO_VER"] },
  {
    ruta: "/requisiciones",
    etiqueta: "Requisiciones",
    permisos: ["REQUISICION_CREAR", "REQUISICION_VER", "REQUISICION_MODIFICAR", "REQUISICION_ENVIAR"],
  },
  { ruta: "/revision", etiqueta: "Revisión", permisos: ["REQUISICION_APROBAR", "REQUISICION_DEVOLVER"] },
  {
    ruta: "/solicitudes-producto",
    etiqueta: "Solicitudes de producto",
    permisos: ["PRODUCTO_SOLICITAR", "PRODUCTO_VER", "PRODUCTO_CREAR"],
  },
  { ruta: "/consolidacion", etiqueta: "Consolidación", permisos: ["PEDIDO_VER", "PEDIDO_CONSOLIDAR"] },
  {
    ruta: "/pedidos-proveedor",
    etiqueta: "Pedidos a proveedor",
    permisos: ["PEDIDO_VER", "PEDIDO_CREAR", "PEDIDO_ENVIAR", "PEDIDO_CERRAR", "PEDIDO_CANCELAR"],
  },
  { ruta: "/entregas", etiqueta: "Entregas", permisos: ["ENTREGA_VER", "ENTREGA_REGISTRAR", "ENTREGA_ANULAR"] },
  { ruta: "/facturacion", etiqueta: "Facturación", permisos: ["FACTURA_VER", "FACTURA_REGISTRAR", "FACTURA_ANULAR"] },
  { ruta: "/seguridad", etiqueta: "Seguridad", permisos: ["SEGURIDAD_VER", "SEGURIDAD_ADMINISTRAR"] },
] as const;

export function permisosDe(ruta: string): readonly string[] {
  return ENLACES.find((enlace) => enlace.ruta === ruta)?.permisos ?? [];
}
