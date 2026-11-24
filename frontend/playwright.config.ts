import { defineConfig, devices } from '@playwright/test';

// Pruebas extremo a extremo contra la aplicación ya desplegada.
// BASE_URL permite apuntar al entorno local (Docker Compose), a preproducción o a producción.
export default defineConfig({
  testDir: './e2e',
  timeout: 30_000,
  retries: process.env.CI ? 1 : 0,
  reporter: [
    ['list'],
    ['junit', { outputFile: 'reports/e2e-junit.xml' }],
    ['html', { open: 'never', outputFolder: 'reports/playwright-report' }],
  ],
  use: {
    baseURL: process.env.BASE_URL ?? 'http://localhost:8080',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
