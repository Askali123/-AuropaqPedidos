import { useState } from "react";
import { UsuariosSeccion } from "../components/seguridad/UsuariosSeccion";
import { RolesSeccion } from "../components/seguridad/RolesSeccion";
import { PermisosSeccion } from "../components/seguridad/PermisosSeccion";

type Pestana = "usuarios" | "roles" | "permisos";

// docs/06-seguridad.md §52/§53 (RN-059/060, ADR-062/063): Usuario/Rol/Permiso y sus asignaciones
// (UsuarioRol/RolPermiso) comparten el mismo permiso SEGURIDAD_VER/ADMINISTRAR — por eso se
// agrupan en una sola pantalla con pestañas en vez de tres rutas separadas, igual que el backend
// las trata como un mismo bloque de autorización. UsuarioSede (05-api.md §62) queda fuera de
// esta pantalla: no fue parte del pedido ("pantalla de Usuarios/Roles/Permisos"), se deja
// señalado como posible mejora futura en progreso.md.
export function SeguridadPage() {
  const [pestana, setPestana] = useState<Pestana>("usuarios");

  return (
    <main className="mx-auto max-w-5xl px-4 py-8">
      <h1 className="text-2xl font-semibold text-slate-900">Seguridad</h1>
      <p className="mt-1 text-sm text-slate-500">
        Usuarios, roles y permisos. Requiere el permiso SEGURIDAD_VER para consultar y
        SEGURIDAD_ADMINISTRAR para crear o asignar.
      </p>

      <div className="mt-4 flex gap-2 border-b border-slate-200">
        <PestanaBoton actual={pestana} valor="usuarios" onSeleccionar={setPestana}>
          Usuarios
        </PestanaBoton>
        <PestanaBoton actual={pestana} valor="roles" onSeleccionar={setPestana}>
          Roles
        </PestanaBoton>
        <PestanaBoton actual={pestana} valor="permisos" onSeleccionar={setPestana}>
          Permisos
        </PestanaBoton>
      </div>

      <div className="mt-5">
        {pestana === "usuarios" && <UsuariosSeccion />}
        {pestana === "roles" && <RolesSeccion />}
        {pestana === "permisos" && <PermisosSeccion />}
      </div>
    </main>
  );
}

function PestanaBoton({
  actual,
  valor,
  onSeleccionar,
  children,
}: {
  actual: Pestana;
  valor: Pestana;
  onSeleccionar: (pestana: Pestana) => void;
  children: string;
}) {
  const activo = actual === valor;
  return (
    <button
      type="button"
      onClick={() => onSeleccionar(valor)}
      className={`border-b-2 px-3 py-2 text-sm font-medium ${
        activo ? "border-blue-600 text-blue-600" : "border-transparent text-slate-500 hover:text-slate-700"
      }`}
    >
      {children}
    </button>
  );
}
