import { Navigate, Route, Routes } from "react-router-dom";
import { AppLayout } from "../components/AppLayout";
import { LoginPage } from "../pages/LoginPage";
import { RequisicionesPage } from "../pages/RequisicionesPage";
import { FacturacionPage } from "../pages/FacturacionPage";
import { SeguridadPage } from "../pages/SeguridadPage";
import { CatalogoPage } from "../pages/CatalogoPage";
import { RevisionPage } from "../pages/RevisionPage";
import { SolicitudesProductoPage } from "../pages/SolicitudesProductoPage";
import { ProtectedRoute } from "./ProtectedRoute";

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
        <Route path="/requisiciones" element={<RequisicionesPage />} />
        <Route path="/facturacion" element={<FacturacionPage />} />
        <Route path="/seguridad" element={<SeguridadPage />} />
        <Route path="/catalogo" element={<CatalogoPage />} />
        <Route path="/revision" element={<RevisionPage />} />
        <Route path="/solicitudes-producto" element={<SolicitudesProductoPage />} />
        <Route path="/" element={<Navigate to="/requisiciones" replace />} />
      </Route>

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
