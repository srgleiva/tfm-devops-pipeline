import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import tailwindcss from '@tailwindcss/vite';

// En desarrollo, las llamadas a /api se redirigen a la API de .NET.
// Por defecto apunta a http://localhost:5234 (perfil "http" de launchSettings.json);
// se puede cambiar con la variable de entorno API_URL.
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: process.env.API_URL ?? 'http://localhost:5234',
        changeOrigin: true
      }
    }
  }
});
