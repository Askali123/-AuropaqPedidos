import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
// "vitest/config" reexporta defineConfig de Vite con el campo "test" añadido — misma
// configuración para dev/build y para pruebas, sin un segundo archivo que pueda desincronizarse.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
  },
})
