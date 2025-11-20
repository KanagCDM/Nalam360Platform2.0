# Visual Regression Testing Setup

## Overview
Automated visual regression testing for Nalam360 Enterprise UI components using Playwright with screenshot comparison and accessibility auditing.

## Prerequisites
- Node.js 18+ and npm/yarn
- .NET 9.0 SDK
- Playwright browsers

## Project Structure
```
tests/
├── Nalam360Enterprise.UI.VisualTests/
│   ├── Nalam360Enterprise.UI.VisualTests.csproj
│   ├── playwright.config.ts
│   ├── package.json
│   ├── Tests/
│   │   ├── Components/
│   │   │   ├── InputComponents.spec.ts
│   │   │   ├── NavigationComponents.spec.ts
│   │   │   ├── DataComponents.spec.ts
│   │   │   ├── LayoutComponents.spec.ts
│   │   │   └── EnterpriseComponents.spec.ts
│   │   └── Accessibility/
│   │       ├── Axe.spec.ts
│   │       └── KeyboardNavigation.spec.ts
│   ├── Fixtures/
│   │   └── TestData.ts
│   ├── Helpers/
│   │   ├── ScreenshotHelper.ts
│   │   └── AccessibilityHelper.ts
│   └── Snapshots/
│       ├── baseline/
│       ├── actual/
│       └── diff/
└── README.md
```

## Installation

### 1. Create Test Project
```xml
<!-- Nalam360Enterprise.UI.VisualTests.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Playwright" Version="1.50.0" />
    <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.50.0" />
    <PackageReference Include="NUnit" Version="4.2.2" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\Nalam360Enterprise.UI\Nalam360Enterprise.UI\Nalam360Enterprise.UI.csproj" />
    <ProjectReference Include="..\..\samples\Nalam360Enterprise.Samples\Nalam360Enterprise.Samples\Nalam360Enterprise.Samples.csproj" />
  </ItemGroup>
</Project>
```

### 2. Install Node Dependencies
```json
{
  "name": "nalam360-ui-visual-tests",
  "version": "1.0.0",
  "scripts": {
    "test": "playwright test",
    "test:headed": "playwright test --headed",
    "test:debug": "playwright test --debug",
    "test:ui": "playwright test --ui",
    "test:update": "playwright test --update-snapshots",
    "install": "playwright install --with-deps"
  },
  "devDependencies": {
    "@playwright/test": "^1.50.0",
    "@axe-core/playwright": "^4.10.0",
    "typescript": "^5.6.3"
  }
}
```

```bash
cd tests/Nalam360Enterprise.UI.VisualTests
npm install
npx playwright install --with-deps
```

### 3. Playwright Configuration
```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './Tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'test-results.json' }],
    ['junit', { outputFile: 'test-results.xml' }]
  ],
  
  use: {
    baseURL: 'http://localhost:5000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chromium-light',
      use: { 
        ...devices['Desktop Chrome'],
        colorScheme: 'light',
      },
    },
    {
      name: 'chromium-dark',
      use: { 
        ...devices['Desktop Chrome'],
        colorScheme: 'dark',
      },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'mobile-safari',
      use: { ...devices['iPhone 13'] },
    },
    {
      name: 'tablet',
      use: { ...devices['iPad Pro'] },
    },
  ],

  webServer: {
    command: 'dotnet run --project ../../samples/Nalam360Enterprise.Samples/Nalam360Enterprise.Samples/Nalam360Enterprise.Samples.csproj',
    url: 'http://localhost:5000',
    reuseExistingServer: !process.env.CI,
    timeout: 120000,
  },
});
```

## Test Examples

### Input Components Visual Tests
```typescript
// Tests/Components/InputComponents.spec.ts
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('N360TextBox Visual Tests', () => {
  test('renders with default props', async ({ page }) => {
    await page.goto('/components/textbox');
    
    const textbox = page.locator('#textbox-default');
    await expect(textbox).toBeVisible();
    
    // Visual snapshot
    await expect(textbox).toHaveScreenshot('textbox-default.png');
  });

  test('renders with label and placeholder', async ({ page }) => {
    await page.goto('/components/textbox');
    
    const textbox = page.locator('#textbox-labeled');
    await expect(textbox).toHaveScreenshot('textbox-labeled.png');
  });

  test('shows validation error state', async ({ page }) => {
    await page.goto('/components/textbox');
    
    const textbox = page.locator('#textbox-validation');
    const input = textbox.locator('input');
    
    await input.fill('invalid');
    await input.blur();
    await page.waitForTimeout(500); // Wait for error animation
    
    await expect(textbox).toHaveScreenshot('textbox-error.png');
  });

  test('disabled state appearance', async ({ page }) => {
    await page.goto('/components/textbox');
    
    const textbox = page.locator('#textbox-disabled');
    await expect(textbox).toHaveScreenshot('textbox-disabled.png');
  });

  test('multiline textarea mode', async ({ page }) => {
    await page.goto('/components/textbox');
    
    const textbox = page.locator('#textbox-multiline');
    await expect(textbox).toHaveScreenshot('textbox-multiline.png');
  });

  test('passes accessibility audit', async ({ page }) => {
    await page.goto('/components/textbox');
    
    const results = await new AxeBuilder({ page })
      .include('#textbox-default')
      .analyze();
    
    expect(results.violations).toEqual([]);
  });
});

test.describe('N360Button Visual Tests', () => {
  const variants = ['primary', 'secondary', 'success', 'danger', 'warning', 'info'];
  const sizes = ['small', 'medium', 'large'];

  for (const variant of variants) {
    for (const size of sizes) {
      test(`renders ${variant} variant in ${size} size`, async ({ page }) => {
        await page.goto('/components/button');
        
        const button = page.locator(`#button-${variant}-${size}`);
        await expect(button).toHaveScreenshot(`button-${variant}-${size}.png`);
      });
    }
  }

  test('hover state animation', async ({ page }) => {
    await page.goto('/components/button');
    
    const button = page.locator('#button-primary-medium');
    await button.hover();
    await page.waitForTimeout(300); // Wait for hover transition
    
    await expect(button).toHaveScreenshot('button-hover.png');
  });

  test('loading state with spinner', async ({ page }) => {
    await page.goto('/components/button');
    
    const button = page.locator('#button-loading');
    await expect(button).toHaveScreenshot('button-loading.png');
  });

  test('keyboard focus indicator', async ({ page }) => {
    await page.goto('/components/button');
    
    await page.keyboard.press('Tab');
    const button = page.locator('#button-primary-medium:focus');
    await expect(button).toHaveScreenshot('button-focus.png');
  });
});

test.describe('N360DropDownList Visual Tests', () => {
  test('closed state', async ({ page }) => {
    await page.goto('/components/dropdownlist');
    
    const dropdown = page.locator('#dropdown-default');
    await expect(dropdown).toHaveScreenshot('dropdown-closed.png');
  });

  test('opened with options visible', async ({ page }) => {
    await page.goto('/components/dropdownlist');
    
    const dropdown = page.locator('#dropdown-default');
    await dropdown.click();
    await page.waitForSelector('.e-popup.e-popup-open');
    
    await expect(page).toHaveScreenshot('dropdown-open.png');
  });

  test('selected item highlighted', async ({ page }) => {
    await page.goto('/components/dropdownlist');
    
    const dropdown = page.locator('#dropdown-default');
    await dropdown.click();
    await page.locator('.e-list-item').first().click();
    
    await expect(dropdown).toHaveScreenshot('dropdown-selected.png');
  });

  test('with search filtering', async ({ page }) => {
    await page.goto('/components/dropdownlist');
    
    const dropdown = page.locator('#dropdown-searchable');
    await dropdown.click();
    await page.locator('input[role="combobox"]').fill('test');
    await page.waitForTimeout(300);
    
    await expect(page).toHaveScreenshot('dropdown-filtered.png');
  });
});
```

### Data Grid Visual Tests
```typescript
// Tests/Components/DataComponents.spec.ts
import { test, expect } from '@playwright/test';

test.describe('N360Grid Visual Tests', () => {
  test('renders empty grid', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-empty');
    await expect(grid).toHaveScreenshot('grid-empty.png');
  });

  test('renders with data rows', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-data');
    await page.waitForSelector('.e-row');
    
    await expect(grid).toHaveScreenshot('grid-data.png');
  });

  test('sorting indicator on column header', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-data');
    await page.locator('.e-headercell').first().click();
    await page.waitForTimeout(300);
    
    await expect(grid).toHaveScreenshot('grid-sorted.png');
  });

  test('selected row highlight', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-data');
    await page.locator('.e-row').first().click();
    
    await expect(grid).toHaveScreenshot('grid-selected.png');
  });

  test('pager controls', async ({ page }) => {
    await page.goto('/components/grid');
    
    const pager = page.locator('.e-gridpager');
    await expect(pager).toHaveScreenshot('grid-pager.png');
  });

  test('filter row visible', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-filterable');
    await page.locator('.e-filterbar').waitFor();
    
    await expect(grid).toHaveScreenshot('grid-filter.png');
  });

  test('grouping with expanded groups', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-grouped');
    await page.waitForSelector('.e-groupdroparea');
    
    await expect(grid).toHaveScreenshot('grid-grouped.png');
  });

  test('toolbar with export buttons', async ({ page }) => {
    await page.goto('/components/grid');
    
    const toolbar = page.locator('.e-toolbar');
    await expect(toolbar).toHaveScreenshot('grid-toolbar.png');
  });

  test('infinite scroll loading indicator', async ({ page }) => {
    await page.goto('/components/grid');
    
    const grid = page.locator('#grid-infinite-scroll');
    await page.locator('.e-content').evaluate(el => {
      el.scrollTop = el.scrollHeight;
    });
    await page.waitForSelector('.e-spinner-pane');
    
    await expect(grid).toHaveScreenshot('grid-loading.png');
  });
});
```

### Layout Components Visual Tests
```typescript
// Tests/Components/LayoutComponents.spec.ts
import { test, expect } from '@playwright/test';

test.describe('N360Dialog Visual Tests', () => {
  test('closed (not visible)', async ({ page }) => {
    await page.goto('/components/dialog');
    await expect(page.locator('.e-dialog')).not.toBeVisible();
  });

  test('opened modal with backdrop', async ({ page }) => {
    await page.goto('/components/dialog');
    
    await page.locator('#open-dialog-btn').click();
    await page.waitForSelector('.e-dialog.e-popup-open');
    
    await expect(page).toHaveScreenshot('dialog-open.png', {
      fullPage: true
    });
  });

  test('with header and footer', async ({ page }) => {
    await page.goto('/components/dialog');
    
    await page.locator('#open-dialog-btn').click();
    const dialog = page.locator('.e-dialog');
    
    await expect(dialog).toHaveScreenshot('dialog-content.png');
  });

  test('different sizes', async ({ page }) => {
    const sizes = ['small', 'medium', 'large', 'fullscreen'];
    
    for (const size of sizes) {
      await page.goto('/components/dialog');
      await page.locator(`#open-dialog-${size}`).click();
      await page.waitForSelector('.e-dialog.e-popup-open');
      
      await expect(page).toHaveScreenshot(`dialog-${size}.png`, {
        fullPage: true
      });
      
      await page.locator('.e-dlg-closeicon-btn').click();
      await page.waitForTimeout(300);
    }
  });
});

test.describe('N360Card Visual Tests', () => {
  test('basic card with content', async ({ page }) => {
    await page.goto('/components/card');
    
    const card = page.locator('#card-basic');
    await expect(card).toHaveScreenshot('card-basic.png');
  });

  test('with header and footer', async ({ page }) => {
    await page.goto('/components/card');
    
    const card = page.locator('#card-full');
    await expect(card).toHaveScreenshot('card-full.png');
  });

  test('hover elevation effect', async ({ page }) => {
    await page.goto('/components/card');
    
    const card = page.locator('#card-hoverable');
    await card.hover();
    await page.waitForTimeout(300);
    
    await expect(card).toHaveScreenshot('card-hover.png');
  });
});
```

### Accessibility Tests
```typescript
// Tests/Accessibility/Axe.spec.ts
import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Accessibility Audits', () => {
  const componentPages = [
    '/components/textbox',
    '/components/button',
    '/components/dropdownlist',
    '/components/grid',
    '/components/dialog',
    '/components/tabs',
    '/components/accordion',
    '/components/chart'
  ];

  for (const page of componentPages) {
    test(`${page} passes axe accessibility audit`, async ({ page: pw }) => {
      await pw.goto(page);
      
      const results = await new AxeBuilder({ page: pw })
        .withTags(['wcag2a', 'wcag2aa', 'wcag21a', 'wcag21aa'])
        .analyze();
      
      expect(results.violations).toEqual([]);
    });
  }

  test('entire sample app passes accessibility audit', async ({ page }) => {
    await page.goto('/');
    
    const results = await new AxeBuilder({ page })
      .withTags(['wcag2a', 'wcag2aa'])
      .exclude('.third-party-widget')
      .analyze();
    
    expect(results.violations.length).toBe(0);
    
    // Generate report
    console.log(`Accessibility Score: ${100 - results.violations.length}%`);
    console.log(`Passed Rules: ${results.passes.length}`);
  });
});

// Tests/Accessibility/KeyboardNavigation.spec.ts
test.describe('Keyboard Navigation', () => {
  test('Tab navigation through form inputs', async ({ page }) => {
    await page.goto('/components/form');
    
    const inputs = await page.locator('input, select, textarea, button').count();
    
    for (let i = 0; i < inputs; i++) {
      await page.keyboard.press('Tab');
      const focused = await page.evaluate(() => document.activeElement?.tagName);
      expect(['INPUT', 'SELECT', 'TEXTAREA', 'BUTTON']).toContain(focused);
    }
  });

  test('Arrow keys navigate menu items', async ({ page }) => {
    await page.goto('/components/menu');
    
    await page.locator('#menu').focus();
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('ArrowDown');
    await page.keyboard.press('Enter');
    
    const selected = await page.locator('.e-selected').textContent();
    expect(selected).toBeTruthy();
  });

  test('Escape closes dialog', async ({ page }) => {
    await page.goto('/components/dialog');
    
    await page.locator('#open-dialog-btn').click();
    await page.waitForSelector('.e-dialog.e-popup-open');
    
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
    
    await expect(page.locator('.e-dialog')).not.toBeVisible();
  });

  test('Space toggles checkbox', async ({ page }) => {
    await page.goto('/components/checkbox');
    
    await page.locator('#checkbox').focus();
    await page.keyboard.press('Space');
    
    const checked = await page.locator('#checkbox').isChecked();
    expect(checked).toBe(true);
  });
});
```

### Helper Classes
```typescript
// Helpers/ScreenshotHelper.ts
import { Page } from '@playwright/test';

export class ScreenshotHelper {
  static async captureComponent(
    page: Page, 
    selector: string, 
    name: string,
    options?: {
      waitForAnimations?: boolean;
      fullPage?: boolean;
      theme?: 'light' | 'dark';
    }
  ) {
    const element = page.locator(selector);
    
    if (options?.waitForAnimations) {
      await page.waitForTimeout(500);
    }
    
    if (options?.theme) {
      await page.evaluate((theme) => {
        document.documentElement.setAttribute('data-theme', theme);
      }, options.theme);
      await page.waitForTimeout(200);
    }
    
    return await element.screenshot({
      path: `Snapshots/baseline/${name}.png`,
      animations: 'disabled'
    });
  }

  static async compareScreenshots(
    page: Page,
    selector: string,
    baselineName: string,
    options?: {
      threshold?: number;
      maxDiffPixels?: number;
    }
  ) {
    const element = page.locator(selector);
    
    return await element.screenshot({
      path: `Snapshots/actual/${baselineName}.png`,
      animations: 'disabled'
    });
  }
}

// Helpers/AccessibilityHelper.ts
import { Page } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

export class AccessibilityHelper {
  static async auditComponent(
    page: Page,
    selector: string,
    tags: string[] = ['wcag2a', 'wcag2aa']
  ) {
    const results = await new AxeBuilder({ page })
      .include(selector)
      .withTags(tags)
      .analyze();
    
    return {
      violations: results.violations,
      passes: results.passes,
      score: this.calculateScore(results)
    };
  }

  private static calculateScore(results: any): number {
    const total = results.violations.length + results.passes.length;
    return total === 0 ? 100 : (results.passes.length / total) * 100;
  }

  static async checkKeyboardAccessibility(page: Page, selector: string) {
    const element = page.locator(selector);
    
    // Check if focusable
    await element.focus();
    const isFocused = await element.evaluate(el => el === document.activeElement);
    
    // Check tab index
    const tabIndex = await element.getAttribute('tabindex');
    
    // Check ARIA attributes
    const ariaLabel = await element.getAttribute('aria-label');
    const ariaDescribedBy = await element.getAttribute('aria-describedby');
    
    return {
      isFocusable: isFocused,
      tabIndex,
      hasAriaLabel: !!ariaLabel,
      hasAriaDescription: !!ariaDescribedBy
    };
  }
}
```

## Running Tests

### Local Development
```bash
# Run all tests
npm test

# Run in headed mode (see browser)
npm run test:headed

# Run with UI mode (interactive)
npm run test:ui

# Run specific test file
npx playwright test Tests/Components/InputComponents.spec.ts

# Update baseline snapshots
npm run test:update

# Debug mode
npm run test:debug
```

### CI Pipeline
```bash
# .NET commands
dotnet test tests/Nalam360Enterprise.UI.VisualTests/Nalam360Enterprise.UI.VisualTests.csproj

# Or via npm
cd tests/Nalam360Enterprise.UI.VisualTests
npm install
npm test
```

## CI/CD Integration

### GitHub Actions
```yaml
# .github/workflows/visual-tests.yml
name: Visual Regression Tests

on:
  pull_request:
    branches: [ main, develop ]
  push:
    branches: [ main ]

jobs:
  visual-tests:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: 9.0.x
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: 18
    
    - name: Install dependencies
      run: |
        cd tests/Nalam360Enterprise.UI.VisualTests
        npm install
        npx playwright install --with-deps
    
    - name: Run visual tests
      run: |
        cd tests/Nalam360Enterprise.UI.VisualTests
        npm test
    
    - name: Upload test results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: playwright-report
        path: tests/Nalam360Enterprise.UI.VisualTests/playwright-report/
    
    - name: Upload snapshots on failure
      if: failure()
      uses: actions/upload-artifact@v4
      with:
        name: failed-snapshots
        path: |
          tests/Nalam360Enterprise.UI.VisualTests/Snapshots/actual/
          tests/Nalam360Enterprise.UI.VisualTests/Snapshots/diff/
```

## Reporting

### HTML Report
After test run, view comprehensive report:
```bash
npx playwright show-report
```

### Percy Integration (Optional)
```bash
npm install --save-dev @percy/cli @percy/playwright

# Run with Percy
npx percy exec -- npx playwright test
```

## Best Practices

1. **Stable Selectors**: Use data-testid attributes
2. **Wait for Stability**: Use waitForLoadState and waitForTimeout appropriately
3. **Disable Animations**: Set animations: 'disabled' for consistent screenshots
4. **Baseline Management**: Update baselines deliberately after UI changes
5. **Cross-Browser**: Test on Chrome, Firefox, Safari
6. **Responsive**: Test mobile, tablet, desktop viewports
7. **Themes**: Test light, dark, high-contrast themes
8. **Accessibility**: Run axe audits on every component

## Maintenance

### Update Baselines
```bash
# After intentional UI changes
npm run test:update
git add Snapshots/baseline/
git commit -m "chore: update visual test baselines"
```

### Review Failures
```bash
# View HTML report
npx playwright show-report

# Check diff images in Snapshots/diff/
```

## Future Enhancements
- [ ] Percy.io integration for managed snapshot hosting
- [ ] Chromatic integration for Storybook
- [ ] Performance metrics tracking (Lighthouse CI)
- [ ] Cross-device testing on BrowserStack/Sauce Labs
- [ ] AI-powered visual diff analysis
- [ ] Automated baseline approval workflow
