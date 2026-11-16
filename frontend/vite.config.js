import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// En desarrollo, /api se reenvía a la API de .NET (puerto 5080).
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5080'
    }
  },
  // Compila dentro de wwwroot de la API para poder servir todo desde .NET en producción.
  build: {
    outDir: '../Encriptador.Api/wwwroot',
    emptyOutDir: true
  }
})
