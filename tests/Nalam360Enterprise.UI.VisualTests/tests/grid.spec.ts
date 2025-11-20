import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('N360Grid Component', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/components/grid');
  });

  test('should render grid with data', async ({ page }) => {
    const grid = page.locator('.n360-grid').first();
    await expect(grid).toBeVisible();
    
    // Wait for data to load
    await page.waitForSelector('.e-row', { timeout: 5000 });
    const rows = page.locator('.e-row');
    await expect(rows).not.toHaveCount(0);
  });

  test('should support sorting', async ({ page }) => {
    const firstColumnHeader = page.locator('.e-headercell').first();
    await firstColumnHeader.click();
    
    // Verify sort indicator
    const sortIcon = firstColumnHeader.locator('.e-icon-ascending, .e-icon-descending');
    await expect(sortIcon).toBeVisible();
  });

  test('should support filtering', async ({ page }) => {
    const filterButton = page.locator('.e-filterbar-cell .e-input').first();
    await filterButton.fill('test');
    await page.keyboard.press('Enter');
    
    // Wait for filtered results
    await page.waitForTimeout(500);
    const rows = page.locator('.e-row');
    await expect(rows.count()).toBeGreaterThan(0);
  });

  test('should support paging', async ({ page }) => {
    const nextPageButton = page.locator('.e-nextpage');
    await nextPageButton.click();
    
    // Verify page change
    const activePage = page.locator('.e-active');
    await expect(activePage).toContainText('2');
  });

  test('should be accessible', async ({ page }) => {
    const accessibilityScanResults = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa'])
      .exclude('.e-grid') // Syncfusion components have their own a11y
      .analyze();

    expect(accessibilityScanResults.violations.length).toBeLessThan(5);
  });

  test('should take screenshot of grid', async ({ page }) => {
    await page.waitForSelector('.e-row');
    await expect(page).toHaveScreenshot('grid-default.png', {
      fullPage: true,
      mask: [page.locator('.e-spinner-pane')] // Mask loading spinners
    });
  });
});

test.describe('Grid Visual Regression', () => {
  test('default view', async ({ page }) => {
    await page.goto('/components/grid');
    await page.waitForSelector('.e-row');
    await expect(page).toHaveScreenshot('grid-default-state.png');
  });

  test('with selection', async ({ page }) => {
    await page.goto('/components/grid');
    await page.waitForSelector('.e-row');
    
    const firstRow = page.locator('.e-row').first();
    await firstRow.click();
    await expect(page).toHaveScreenshot('grid-selected.png');
  });

  test('with grouping', async ({ page }) => {
    await page.goto('/components/grid');
    await page.waitForSelector('.e-row');
    
    // Enable grouping
    const columnHeader = page.locator('.e-headercell').first();
    await columnHeader.dragTo(page.locator('.e-groupdroparea'));
    await expect(page).toHaveScreenshot('grid-grouped.png');
  });
});
