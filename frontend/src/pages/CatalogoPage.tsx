import { useState } from "react";
import { EmpresasSeccion } from "../components/catalogo/EmpresasSeccion";
import { CategoriasSeccion } from "../components/catalogo/CategoriasSeccion";
import { UnidadesMedidaSeccion } from "../components/catalogo/UnidadesMedidaSeccion";
import { ProductosSeccion } from "../components/catalogo/ProductosSeccion";
import { ProveedoresSeccion } from "../components/catalogo/ProveedoresSeccion";
import { PeriodosSeccion } from "../components/catalogo/PeriodosSeccion";

type Pestana = "empresas" | "categorias" | "unidades" | "productos" | "proveedores" | "periodos";

const PESTANAS: { valor: Pestana; etiqueta: string }[] = [
  { valor: "empresas", etiqueta: "Empresas" },
  { valor: "categorias", etiqueta: "Categorías" },
  { valor: "unidades", etiqueta: "Unidades de medida" },
  { valor: "productos", etiqueta: "Productos" },
  { valor: "proveedores", etiqueta: "Proveedores" },
  { valor: "periodos", etiqueta: "Periodos" },
];

// docs/05-api.md §11-§16/§29. Agrupa los 6 catálogos administrativos del MVP en una sola ruta con
// pestañas — mismo criterio que SeguridadPage: cada uno tiene su propio permiso
// (ORGANIZACION_*/PRODUCTO_*/PROVEEDOR_*/PERIODO_*, 06-seguridad.md §52/§53), pero todos son
// "datos maestros" en el mismo sentido de negocio (CLAUDE.md §2), así que agruparlos evita 6
// rutas separadas para pantallas de una sola tabla cada una. Sedes no tiene pestaña propia: vive
// anidada bajo Empresas (SedesPanel), igual que en la API real (/empresas/{id}/sedes).
export function CatalogoPage() {
  const [pestana, setPestana] = useState<Pestana>("empresas");

  return (
    <main className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Catálogo</h1>
      <p className="mt-1 text-sm text-slate-500">
        Datos maestros: empresas y sedes, categorías, unidades de medida, productos, proveedores y
        periodos.
      </p>

      <div className="mt-4 flex flex-wrap gap-2 border-b border-slate-200">
        {PESTANAS.map(({ valor, etiqueta }) => (
          <button
            key={valor}
            type="button"
            onClick={() => setPestana(valor)}
            className={`border-b-2 px-3 py-2 text-sm font-medium ${
              pestana === valor ? "border-blue-600 text-blue-600" : "border-transparent text-slate-500 hover:text-slate-700"
            }`}
          >
            {etiqueta}
          </button>
        ))}
      </div>

      <div className="mt-5">
        {pestana === "empresas" && <EmpresasSeccion />}
        {pestana === "categorias" && <CategoriasSeccion />}
        {pestana === "unidades" && <UnidadesMedidaSeccion />}
        {pestana === "productos" && <ProductosSeccion />}
        {pestana === "proveedores" && <ProveedoresSeccion />}
        {pestana === "periodos" && <PeriodosSeccion />}
      </div>
    </main>
  );
}
