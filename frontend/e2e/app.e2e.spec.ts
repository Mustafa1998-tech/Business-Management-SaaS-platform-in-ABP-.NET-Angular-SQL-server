import { expect, test } from '@playwright/test';

test('login and create customer flow (smoke)', async ({ page }) => {
  test.setTimeout(120000);
  const customers: Array<{
    id: string;
    name: string;
    email: string;
    phone: string;
    address: string;
    isActive: boolean;
  }> = [];

  await page.route('**/api/app/dashboard/stats', async route => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        totalCustomers: customers.length,
        activeProjects: 7,
        pendingTasks: 14,
        outstandingInvoices: 5,
        revenueByMonth: [1000, 2000, 3000]
      })
    });
  });

  await page.route('**/api/app/customers**', async route => {
    const request = route.request();
    const method = request.method().toUpperCase();
    const url = new URL(request.url());

    if (method === 'GET') {
      const skip = Number(url.searchParams.get('skipCount') ?? '0');
      const take = Number(url.searchParams.get('maxResultCount') ?? '10');
      const filter = (url.searchParams.get('filter') ?? '').trim().toLowerCase();
      const filtered = filter
        ? customers.filter(
            c => c.name.toLowerCase().includes(filter) || c.email.toLowerCase().includes(filter)
          )
        : customers;
      const paged = filtered.slice(skip, skip + take);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          totalCount: filtered.length,
          items: paged
        })
      });

      return;
    }

    if (method === 'POST') {
      const payload = request.postDataJSON() as {
        name: string;
        email: string;
        phone: string;
        address: string;
        isActive: boolean;
      };
      const created = {
        id: `e2e-${customers.length + 1}`,
        name: payload.name,
        email: payload.email,
        phone: payload.phone,
        address: payload.address,
        isActive: payload.isActive
      };
      customers.push(created);

      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify(created)
      });

      return;
    }

    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({})
    });
  });

  await page.goto('/dashboard', { waitUntil: 'domcontentloaded' });
  await expect(page).toHaveURL(/\/dashboard/);
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible({ timeout: 30000 });

  await page.goto('/customers', { waitUntil: 'domcontentloaded' });
  await expect(page).toHaveURL(/\/customers/);
  await expect(page.getByRole('heading', { name: 'Customers' })).toBeVisible();
  await page.getByRole('button', { name: 'New Customer' }).click();

  await page.fill('input[formcontrolname="name"]', 'Playwright Customer');
  await page.fill('input[formcontrolname="email"]', 'playwright@demo.com');
  await page.fill('input[formcontrolname="phone"]', '+1-555-9988');
  await page.fill('textarea[formcontrolname="address"]', 'UI Test Address');

  await page.getByRole('button', { name: 'Save' }).click();

  await expect(page.getByText('Playwright Customer')).toBeVisible();
});
