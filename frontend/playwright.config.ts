import { defineConfig } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  use: {
    baseURL: 'http://127.0.0.1:4301'
  },
  webServer: {
    command: 'npx ng serve --host 127.0.0.1 --port 4301',
    port: 4301,
    timeout: 120000,
    reuseExistingServer: false
  }
});
