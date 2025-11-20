import { test, expect } from '@playwright/test';

test.describe('E2E: Patient Dashboard Journey', () => {
  test('complete patient management workflow', async ({ page }) => {
    // Step 1: Login
    await page.goto('/login');
    await page.locator('input[name="username"]').fill('admin');
    await page.locator('input[name="password"]').fill('password');
    await page.locator('button[type="submit"]').click();
    
    // Verify login success
    await expect(page).toHaveURL(/dashboard/);
    
    // Step 2: Navigate to patients
    await page.locator('a[href*="patients"]').click();
    await expect(page).toHaveURL(/patients/);
    
    // Step 3: Search for patient
    const searchBox = page.locator('input[placeholder*="Search"]');
    await searchBox.fill('John Doe');
    await page.keyboard.press('Enter');
    
    // Wait for search results
    await page.waitForSelector('.patient-card', { timeout: 5000 });
    
    // Step 4: View patient details
    const firstPatient = page.locator('.patient-card').first();
    await firstPatient.click();
    await expect(page).toHaveURL(/patients\/\d+/);
    
    // Step 5: Verify patient information loaded
    await expect(page.locator('.patient-name')).toBeVisible();
    await expect(page.locator('.patient-mrn')).toBeVisible();
    
    // Step 6: View medical history
    await page.locator('button:has-text("Medical History")').click();
    await expect(page.locator('.medical-history-section')).toBeVisible();
    
    // Step 7: Take screenshot of complete patient view
    await page.screenshot({ 
      path: 'e2e-screenshots/patient-complete-view.png',
      fullPage: true 
    });
  });
});

test.describe('E2E: Clinical Decision Support', () => {
  test('AI-powered clinical recommendations workflow', async ({ page }) => {
    await page.goto('/clinical-decision-support');
    
    // Step 1: Enter patient symptoms
    await page.locator('textarea[name="symptoms"]').fill('Chest pain, shortness of breath');
    
    // Step 2: Add vital signs
    await page.locator('input[name="bloodPressure"]').fill('140/90');
    await page.locator('input[name="heartRate"]').fill('95');
    await page.locator('input[name="temperature"]').fill('98.6');
    
    // Step 3: Request AI analysis
    await page.locator('button:has-text("Analyze")').click();
    
    // Step 4: Wait for AI response
    await page.waitForSelector('.ai-recommendations', { timeout: 10000 });
    
    // Step 5: Verify recommendations displayed
    const recommendations = page.locator('.recommendation-card');
    await expect(recommendations).not.toHaveCount(0);
    
    // Step 6: Verify confidence scores
    const confidenceScore = page.locator('.confidence-score').first();
    await expect(confidenceScore).toBeVisible();
    
    // Step 7: Take screenshot
    await page.screenshot({ 
      path: 'e2e-screenshots/clinical-decision-support.png',
      fullPage: true 
    });
  });
});

test.describe('E2E: Healthcare Analytics Dashboard', () => {
  test('navigate and interact with analytics', async ({ page }) => {
    await page.goto('/analytics');
    
    // Step 1: Wait for dashboard to load
    await page.waitForSelector('.analytics-dashboard', { timeout: 5000 });
    
    // Step 2: Verify charts rendered
    const charts = page.locator('.chart-container');
    await expect(charts).not.toHaveCount(0);
    
    // Step 3: Change date range
    await page.locator('button.date-range-picker').click();
    await page.locator('button:has-text("Last 30 Days")').click();
    
    // Step 4: Wait for data refresh
    await page.waitForTimeout(2000);
    
    // Step 5: Export report
    await page.locator('button:has-text("Export")').click();
    await page.locator('button:has-text("PDF")').click();
    
    // Step 6: Verify export initiated
    const downloadPromise = page.waitForEvent('download');
    const download = await downloadPromise;
    expect(download.suggestedFilename()).toContain('analytics');
  });
});

test.describe('E2E: Appointment Scheduling', () => {
  test('complete appointment booking flow', async ({ page }) => {
    await page.goto('/appointments/new');
    
    // Step 1: Select patient
    await page.locator('input[name="patientSearch"]').fill('Jane Smith');
    await page.keyboard.press('Enter');
    await page.waitForSelector('.patient-suggestion');
    await page.locator('.patient-suggestion').first().click();
    
    // Step 2: Select provider
    await page.locator('select[name="provider"]').selectOption('Dr. John Johnson');
    
    // Step 3: Select date and time
    await page.locator('input[type="date"]').fill('2025-12-01');
    await page.locator('select[name="time"]').selectOption('10:00 AM');
    
    // Step 4: Select appointment type
    await page.locator('select[name="appointmentType"]').selectOption('Follow-up');
    
    // Step 5: Add notes
    await page.locator('textarea[name="notes"]').fill('Routine follow-up visit');
    
    // Step 6: Submit appointment
    await page.locator('button[type="submit"]').click();
    
    // Step 7: Verify confirmation
    await expect(page.locator('.appointment-confirmation')).toBeVisible({ timeout: 5000 });
    await expect(page.locator('.confirmation-message')).toContainText(/successfully scheduled/i);
  });
});
