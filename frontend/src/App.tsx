import { useState } from "react";
import { FacturacionPage } from "./pages/FacturacionPage";
import { RequisicionesPage } from "./pages/RequisicionesPage";

type Vista = "requisiciones" | "facturacion";

// Sin librería de rutas todavía (no se justifica para dos pantillas): un selector de vista
// mínimo mantiene accesible la pantalla de Facturación ya existente (TASK-046) al agregar la de
// Requisiciones, sin quitar funcionalidad previa.
function App() {
  const [vista, setVista] = useState<Vista>("requisiciones");

  return (
    <div className="min-h-screen bg-slate-100">
      <nav className="flex gap-2 border-b border-slate-200 bg-white px-4 py-2">
        <BotonVista actual={vista} valor="requisiciones" onSeleccionar={setVista}>
          Requisiciones
        </BotonVista>
        <BotonVista actual={vista} valor="facturacion" onSeleccionar={setVista}>
          Facturación
        </BotonVista>
      </nav>

      {vista === "requisiciones" ? <RequisicionesPage /> : <FacturacionPage />}
    </div>
  );
}

function BotonVista({
  actual,
  valor,
  onSeleccionar,
  children,
}: {
  actual: Vista;
  valor: Vista;
  onSeleccionar: (vista: Vista) => void;
  children: string;
}) {
  const activo = actual === valor;
  return (
    <button
      type="button"
      onClick={() => onSeleccionar(valor)}
      className={`rounded px-3 py-1.5 text-sm font-medium ${
        activo ? "bg-blue-600 text-white" : "text-slate-600 hover:bg-slate-100"
      }`}
    >
      {children}
    </button>
  );
}

export default App;
