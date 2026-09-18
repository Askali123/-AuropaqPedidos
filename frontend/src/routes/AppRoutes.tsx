import { Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "../components/AppLayout";
import { LoginPage } from "../pages/LoginPage";
import { RequisicionesPage } from "../pages/RequisicionesPage";
import { ConsolidacionPage } from "../pages/ConsolidacionPage";
import { PedidosProveedorPage } from "../pages/PedidosProveedorPage";
import { EntregasPage } from "../pages/EntregasPage";
import { FacturacionPage } from "../pages/FacturacionPage";
import { SeguridadPage } from "../pages/SeguridadPage";
import { CatalogoPage } from "../pages/CatalogoPage";
import { RevisionPage } from "../pages/RevisionPage";
import { SolicitudesProductoPage } from "../pages/SolicitudesProductoPage";
import { ProtectedRoute } from "./ProtectedRoute";
import { RutaConPermiso } from "./RutaConPermiso";
import { permisosDe } from "./enlaces";

// TASK-102 (docs/2026-09-18-auditoria-dominio-roles-frontend.md, hallazgo F1): cada ruta
// administrativa/operativa queda protegida por permiso real (RutaConPermiso), además de que
// AppLayout ya oculta su enlace del menú — cubre navegar directamente a la URL. permisosDe(ruta)
// lee del mismo archivo (routes/enlaces.ts) que usa el menú, para que ambos no se desincronicen.
export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        element={
          <ProtectedRoute>
            <AppLayout />
          </ProtectedRoute>
        }
      >
        <Route
          path="/requisiciones"
          element={
            <RutaConPermiso permisos={permisosDe("/requisiciones")}>
              <RequisicionesPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/consolidacion"
          element={
            <RutaConPermiso permisos={permisosDe("/consolidacion")}>
              <ConsolidacionPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/pedidos-proveedor"
          element={
            <RutaConPermiso permisos={permisosDe("/pedidos-proveedor")}>
              <PedidosProveedorPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/entregas"
          element={
            <RutaConPermiso permisos={permisosDe("/entregas")}>
              <EntregasPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/facturacion"
          element={
            <RutaConPermiso permisos={permisosDe("/facturacion")}>
              <FacturacionPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/seguridad"
          element={
            <RutaConPermiso permisos={permisosDe("/seguridad")}>
              <SeguridadPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/catalogo"
          element={
            <RutaConPermiso permisos={permisosDe("/catalogo")}>
              <CatalogoPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/revision"
          element={
            <RutaConPermiso permisos={permisosDe("/revision")}>
              <RevisionPage />
            </RutaConPermiso>
          }
        />
        <Route
          path="/solicitudes-producto"
          element={
            <RutaConPermiso permisos={permisosDe("/solicitudes-producto")}>
              <SolicitudesProductoPage />
            </RutaConPermiso>
          }
        />
        <Route path="/" element={<Navigate to="/requisiciones" replace />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
