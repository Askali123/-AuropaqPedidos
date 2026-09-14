// Configuración centralizada de la URL de la API. Los componentes/servicios nunca deben
// escribir la URL de la API directamente (CLAUDE.md, incremento MVP frontend ↔ backend).
export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5175";
