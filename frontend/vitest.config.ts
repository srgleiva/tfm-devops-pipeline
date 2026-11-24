import { defineConfig, mergeConfig } from 'vitest/config';
import viteConfig from './vite.config';

// Pruebas unitarias y de componentes del frontend (Vitest + Testing Library).
export default mergeConfig(
  viteConfig,
  defineConfig({
    test: {
      environment: 'jsdom',
      setupFiles: ['./src/test/setup.ts'],
      include: ['src/**/*.test.{ts,tsx}'],
      reporters: ['default', 'junit'],
      outputFile: { junit: './reports/vitest-junit.xml' },
      coverage: {
        provider: 'v8',
        include: ['src/**/*.{ts,tsx}'],
        exclude: ['src/**/*.test.{ts,tsx}', 'src/test/**', 'src/main.tsx', 'src/vite-env.d.ts'],
        reporter: ['text', 'cobertura', 'html'],
        reportsDirectory: './reports/coverage',
      },
    },
  })
);
