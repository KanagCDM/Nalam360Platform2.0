# Documentation Site - Design System

**Version:** 1.0  
**Last Updated:** November 20, 2024

## Overview

The Nalam360 Enterprise UI documentation site features a modern, professional design system built with accessibility, usability, and visual appeal in mind.

## Color Palette

### Primary Colors
```css
--primary-50:  #eff6ff  /* Light background */
--primary-100: #dbeafe  /* Subtle highlights */
--primary-200: #bfdbfe  /* Borders */
--primary-300: #93c5fd  /* Hover states */
--primary-400: #60a5fa  /* Interactive elements */
--primary-500: #3b82f6  /* Primary brand */
--primary-600: #2563eb  /* Primary buttons */
--primary-700: #1d4ed8  /* Active states */
--primary-800: #1e40af  /* Dark accents */
--primary-900: #1e3a8a  /* Text on light */
```

### Neutral Colors
```css
--neutral-50:  #f9fafb  /* Page background */
--neutral-100: #f3f4f6  /* Card backgrounds */
--neutral-200: #e5e7eb  /* Dividers */
--neutral-300: #d1d5db  /* Input borders */
--neutral-400: #9ca3af  /* Placeholder text */
--neutral-500: #6b7280  /* Secondary text */
--neutral-600: #4b5563  /* Body text */
--neutral-700: #374151  /* Headings */
--neutral-800: #1f2937  /* Dark text */
--neutral-900: #111827  /* Deepest black */
```

### Semantic Colors
```css
--success: #10b981       /* Success states */
--success-light: #d1fae5 /* Success backgrounds */
--warning: #f59e0b       /* Warning states */
--warning-light: #fef3c7 /* Warning backgrounds */
--error: #ef4444         /* Error states */
--error-light: #fee2e2   /* Error backgrounds */
--info: #06b6d4          /* Info states */
--info-light: #cffafe    /* Info backgrounds */
```

## Typography

### Font Families
- **Sans-serif:** Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto
- **Monospace:** 'Fira Code', 'Courier New', monospace

### Font Sizes
```css
h1: 2.5rem   (40px) - Component titles
h2: 2rem     (32px) - Section headers
h3: 1.5rem   (24px) - Subsection headers
h4: 1.25rem  (20px) - Card titles
p:  1rem     (16px) - Body text
small: 0.875rem (14px) - Helper text
```

### Font Weights
- **Regular:** 400 - Body text
- **Medium:** 500 - Navigation, buttons
- **Semibold:** 600 - Subheadings
- **Bold:** 700 - Headings
- **Extrabold:** 800 - Hero titles

## Spacing System

```css
--spacing-xs:  0.25rem  (4px)
--spacing-sm:  0.5rem   (8px)
--spacing-md:  1rem     (16px)
--spacing-lg:  1.5rem   (24px)
--spacing-xl:  2rem     (32px)
--spacing-2xl: 3rem     (48px)
--spacing-3xl: 4rem     (64px)
```

## Border Radius

```css
--radius-sm:   0.25rem  (4px)  - Small elements
--radius-md:   0.5rem   (8px)  - Buttons, inputs
--radius-lg:   0.75rem  (12px) - Cards
--radius-xl:   1rem     (16px) - Large cards
--radius-full: 9999px          - Pills, badges
```

## Shadows

```css
--shadow-sm: 0 1px 2px rgba(0,0,0,0.05)
  /* Subtle elevation */

--shadow-md: 0 4px 6px rgba(0,0,0,0.1)
  /* Standard cards */

--shadow-lg: 0 10px 15px rgba(0,0,0,0.1)
  /* Elevated elements */

--shadow-xl: 0 20px 25px rgba(0,0,0,0.1)
  /* Modal, dropdown */
```

## Transitions

```css
--transition-fast: 150ms ease-in-out  /* Hover effects */
--transition-base: 250ms ease-in-out  /* Standard transitions */
--transition-slow: 350ms ease-in-out  /* Page transitions */
```

## Component Patterns

### Buttons

**Primary Button**
- Background: Linear gradient (primary-600 to primary-700)
- Text: White
- Padding: 0.625rem 1.25rem
- Border radius: --radius-md
- Hover: Gradient shift + translateY(-1px)
- Shadow: --shadow-md on hover

**Secondary Button**
- Background: White
- Text: neutral-700
- Border: 1px solid neutral-300
- Hover: Background neutral-50

### Cards

**Standard Card**
- Background: White
- Padding: --spacing-2xl
- Border radius: --radius-xl
- Shadow: --shadow-md
- Hover: --shadow-lg + translateY(-2px)

### Navigation

**Active Link**
- Background: Linear gradient (primary-50 to primary-100)
- Text: primary-700
- Font weight: 600
- Box shadow: inset 3px 0 0 primary-600

**Hover Link**
- Background: neutral-100
- Text: primary-600
- Transform: translateX(2px)

### Tabs

**Active Tab**
- Border bottom: 3px solid primary-600
- Background: Gradient to top (primary-50 to transparent)
- Text: primary-700

**Tab Hover**
- Background: primary-50
- Border bottom: 3px solid primary-300
- Text: primary-600

### Badges

**Structure**
- Display: inline-flex
- Padding: 0.25rem 0.75rem
- Font size: 0.75rem
- Font weight: 600
- Border radius: --radius-full
- Text transform: uppercase

**Variants**
- Primary: primary-100 background, primary-800 text
- Success: success-light background, dark green text
- Info: info-light background, dark cyan text
- AI: Linear gradient (purple to violet)

## Animations

### Fade In
```css
@keyframes fadeIn {
  from { opacity: 0; transform: translateY(20px); }
  to   { opacity: 1; transform: translateY(0); }
}
```

### Slide In
```css
@keyframes slideIn {
  from { opacity: 0; transform: translateY(10px); }
  to   { opacity: 1; transform: translateY(0); }
}
```

### Spin (Loading)
```css
@keyframes spin {
  to { transform: rotate(360deg); }
}
```

## Layout Structure

### App Container
```
┌─────────────────────────────────────┐
│         Sidebar (280px fixed)       │
│  ┌─────────────────────────────┐    │
│  │     Header (sticky)         │    │
│  ├─────────────────────────────┤    │
│  │                             │    │
│  │     Main Content            │    │
│  │     (max-width: 1400px)     │    │
│  │                             │    │
│  ├─────────────────────────────┤    │
│  │     Footer                  │    │
│  └─────────────────────────────┘    │
└─────────────────────────────────────┘
```

### Responsive Breakpoints
- **Desktop:** > 1024px - Full layout
- **Tablet:** 768px - 1024px - Stacked playground
- **Mobile:** < 768px - Collapsible sidebar, icon-only tabs

## Accessibility

### WCAG 2.1 Level AA Compliance
- ✅ Color contrast ratios meet 4.5:1 minimum
- ✅ Focus indicators visible on all interactive elements
- ✅ Keyboard navigation supported
- ✅ Screen reader compatible
- ✅ Semantic HTML structure

### Focus States
- Border: 2px solid primary-500
- Box shadow: 0 0 0 3px primary-100
- Outline: None (replaced with custom styling)

### Hover States
- Cursor: pointer on interactive elements
- Transition: All --transition-fast
- Visual feedback: Color change or transform

## Code Style Guide

### Class Naming
- **BEM-inspired:** component-element--modifier
- **Utility classes:** Use CSS variables
- **State classes:** .active, .disabled, .loading

### CSS Organization
1. Layout properties (display, position)
2. Box model (margin, padding, border)
3. Typography (font, text)
4. Visual (background, color, shadow)
5. Transitions and animations

## Icon System

### Navigation Icons
- 🏠 Home
- 🚀 Getting Started
- 📦 All Components
- ✏️ Input Components
- 📊 Data Components
- 📈 Charts
- 🏥 Healthcare
- 🤖 AI-Powered
- ⚙️ Enterprise

### Tab Icons
- 📋 Overview
- 🎮 Playground
- 📚 API Reference
- 💻 Examples
- ♿ Accessibility

### Status Icons
- ✓ Success/Completed
- ⚠️ Warning
- ✕ Error/Close
- ℹ️ Information
- ⚡ Generated/Quick

## Best Practices

### DO ✅
- Use design tokens (CSS variables) for consistency
- Apply smooth transitions to interactive elements
- Maintain proper spacing hierarchy
- Use semantic HTML for accessibility
- Test on multiple screen sizes
- Ensure proper color contrast

### DON'T ❌
- Hard-code color values
- Skip hover/focus states
- Ignore mobile responsiveness
- Use only icons without labels (except mobile)
- Create inconsistent spacing
- Forget loading/error states

## Future Enhancements

### Potential Additions
1. **Dark Mode** - Toggle between light/dark themes
2. **Theme Customizer** - Live color palette editor
3. **Motion Preferences** - Respect prefers-reduced-motion
4. **Font Size Adjustment** - User-controlled text scaling
5. **High Contrast Mode** - Enhanced contrast option
6. **Print Styles** - Optimized documentation printing

---

**Design System Version:** 1.0  
**Maintained by:** Nalam360 Design Team  
**License:** MIT
