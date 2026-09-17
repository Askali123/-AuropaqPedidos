import { useState } from "react";
import { SelectorCatalogo } from "./requisiciones/SelectorCatalogo";
import { useConsultaLista } from "../hooks/useConsultaLista";
import { catalogosService } from "../services/catalogosService";
import type { Empresa } from "../types/catalogos";

interface SelectorSedeGlobalProps {
  valorSeleccionado: number | null;
  onSeleccionar: (sedeId: number | null) => void;
  deshabilitado?: boolean;
}

// PedidoProveedor/Entrega no pertenecen a una única Empresa (CLAUDE.md §27): la sede de destino
// de una distribución puede ser de cualquier Empresa. El backend solo expone
// GET /empresas/{id}/sedes (sin "listar todas las sedes" — no se inventa ese endpoint), así que
// este selector encadena Empresa → Sede en dos pasos, reutilizado tanto por
// PedidosProveedorPage como por EntregasPage.
export function SelectorSedeGlobal({ valorSeleccionado, onSeleccionar, deshabilitado = false }: SelectorSedeGlobalProps) {
  const [empresaId, setEmpresaId] = useState<number | null>(null);
  const empresas = useConsultaLista<Empresa>(() => catalogosService.listarEmpresas(), []);
  const sedes = useConsultaLista(
    () => (empresaId != null ? catalogosService.listarSedesPorEmpresa(empresaId) : Promise.resolve([])),
    [empresaId],
  );

  function seleccionarEmpresa(id: number | null) {
    setEmpresaId(id);
    onSeleccionar(null);
  }

  return (
    <div className="grid gap-2 sm:grid-cols-2">
      <SelectorCatalogo
        etiqueta="Empresa (de la sede destino)"
        datos={empresas.datos}
        cargando={empresas.cargando}
        error={empresas.error}
        valorSeleccionado={empresaId}
        onSeleccionar={seleccionarEmpresa}
        obtenerId={(empresa) => empresa.id}
        obtenerTexto={(empresa) => empresa.nombre}
        deshabilitado={deshabilitado}
        mensajeVacio="No hay empresas registradas."
      />
      <SelectorCatalogo
        etiqueta="Sede"
        datos={sedes.datos}
        cargando={sedes.cargando}
        error={sedes.error}
        valorSeleccionado={valorSeleccionado}
        onSeleccionar={onSeleccionar}
        obtenerId={(sede) => sede.id}
        obtenerTexto={(sede) => sede.nombre}
        deshabilitado={deshabilitado || empresaId == null}
        mensajeDeshabilitado="Elige primero una empresa."
        mensajeVacio="Esta empresa no tiene sedes registradas."
      />
    </div>
  );
}
