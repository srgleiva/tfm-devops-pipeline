import { expect, test } from '@playwright/test';

test('la aplicación y la base de datos están disponibles', async ({ request }) => {
  const response = await request.get('/health/ready');
  expect(response.ok()).toBeTruthy();
  expect(await response.text()).toBe('Healthy');
});

test('crear, completar y borrar una tarea', async ({ page }) => {
  const title = `Tarea E2E ${Date.now()}`;

  await page.goto('/');
  await expect(page.getByRole('heading', { name: 'Tasks' })).toBeVisible();

  // Crear
  await page.getByPlaceholder('What needs to be done?').fill(title);
  await page.getByRole('button', { name: 'Add task' }).click();
  const item = page.getByRole('listitem').filter({ hasText: title });
  await expect(item).toBeVisible();

  // Completar
  await item.getByRole('checkbox').click();
  await expect(item.getByRole('checkbox')).toBeChecked();

  // Borrar
  await item.getByRole('button', { name: /Delete/ }).click();
  await expect(page.getByRole('listitem').filter({ hasText: title })).toHaveCount(0);
});

test('la tarea creada se mantiene al recargar la página', async ({ page }) => {
  const title = `Persistente ${Date.now()}`;

  await page.goto('/');
  await page.getByPlaceholder('What needs to be done?').fill(title);
  await page.getByRole('button', { name: 'Add task' }).click();
  await expect(page.getByText(title)).toBeVisible();

  await page.reload();
  await expect(page.getByText(title)).toBeVisible();

  // Limpieza
  await page.getByRole('listitem').filter({ hasText: title }).getByRole('button', { name: /Delete/ }).click();
  await expect(page.getByText(title)).toHaveCount(0);
});
