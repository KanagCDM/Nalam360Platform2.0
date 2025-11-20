# Nalam360Enterprise.UI Visual & E2E Tests

This directory contains visual regression tests and end-to-end tests using Playwright.

## Setup

### Prerequisites
- Node.js 20+
- .NET 9.0 SDK
- Playwright

### Installation

```bash
cd tests/Nalam360Enterprise.UI.VisualTests
npm install
npx playwright install
```

## Running Tests

### All Tests
```bash
npm test
```

### Headed Mode (see browser)
```bash
npm run test:headed
```

### Debug Mode
```bash
npm run test:debug
```

### Interactive UI Mode
```bash
npm run test:ui
```

### Specific Test File
```bash
npx playwright test button.spec.ts
```

### Specific Browser
```bash
npx playwright test --project=chromium
```

## Test Structure

### `/tests/button.spec.ts`
- Component rendering tests
- Interaction tests
- Accessibility tests
- Visual regression for all button states

### `/tests/grid.spec.ts`
- Grid functionality (sorting, filtering, paging)
- Selection and grouping
- Visual regression tests

### `/tests/form.spec.ts`
- Form validation
- Field interactions
- Submit workflows
- Visual regression

### `/tests/e2e.spec.ts`
- Complete user journeys
- Patient management workflow
- Clinical decision support workflow
- Analytics dashboard interaction
- Appointment scheduling

### `/tests/accessibility.spec.ts`
- WCAG 2.1 AA compliance
- Keyboard navigation
- Screen reader support
- Color contrast
- Heading hierarchy
- High contrast mode
- RTL support

## Visual Regression Testing

Playwright automatically captures and compares screenshots. On first run, baseline images are created.

### Update Baselines
```bash
npx playwright test --update-snapshots
```

### View Test Report
```bash
npm run report
```

## Accessibility Testing

Tests use `@axe-core/playwright` to validate WCAG 2.1 AA compliance.

### Run Only Accessibility Tests
```bash
npx playwright test accessibility.spec.ts
```

## CI Integration

Tests run automatically in GitHub Actions on:
- Pull requests to main/develop
- Manual workflow dispatch

See `.github/workflows/visual-test.yml` for configuration.

## Percy Integration (Optional)

To enable visual regression with Percy:

1. Sign up at https://percy.io
2. Add `PERCY_TOKEN` to GitHub secrets
3. Uncomment Percy step in `.github/workflows/visual-test.yml`
4. Install Percy CLI:
   ```bash
   npm install -D @percy/cli @percy/playwright
   ```
5. Run tests:
   ```bash
   npx percy exec -- npx playwright test
   ```

## Troubleshooting

### Tests Failing Locally
- Ensure Blazor app is running on port 5000
- Clear Playwright cache: `npx playwright install --force`
- Update snapshots if intentional changes: `npm test -- --update-snapshots`

### Slow Tests
- Run specific test files instead of all tests
- Use headed mode to see what's happening
- Increase timeouts in playwright.config.ts

### Screenshot Differences
- Baseline images are platform-specific
- CI uses Ubuntu, local may be Windows/Mac
- Use Percy for cross-platform visual regression

## Best Practices

1. **Keep tests focused** - One behavior per test
2. **Use data-testid** - For stable selectors
3. **Wait for elements** - Use `waitForSelector` for dynamic content
4. **Mask dynamic content** - Use `mask` option for timestamps, spinners
5. **Test keyboard navigation** - Verify accessibility
6. **Run accessibility checks** - On every page
7. **Update baselines carefully** - Review visual changes before committing

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Playwright Best Practices](https://playwright.dev/docs/best-practices)
- [axe-core Rules](https://github.com/dequelabs/axe-core/blob/develop/doc/rule-descriptions.md)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
