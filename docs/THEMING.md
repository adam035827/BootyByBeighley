# Theming & Branding Guide

This document explains how the app's theming system works and how to create or modify themes.

---

## Overview

The app uses **CSS custom properties (variables)** for all design tokens, enabling themes to be swapped easily without modifying component code.

### Architecture

```
styles.scss
├── _theme.scss      ← Brand colors (import here to change themes)
├── _tokens.scss     ← Design tokens (references theme colors)
├── _reset.scss      ← CSS reset
└── _typography.scss ← Typography styles
```

---

## Current Theme: White & Vibrant Red

The default theme features:
- **Background**: Pure white (`#ffffff`)
- **Primary Color**: Vibrant red (`#ff1744`)
- **Surface**: Light gray (`#f5f5f5`)
- **Text**: Dark text on light backgrounds

### Theme Variables

All theme colors are defined in `frontend/app/src/styles/_theme.scss`:

```scss
--theme-primary:         #ff1744;  // Vibrant Red
--theme-primary-dark:    #d01436;  // Darker Red
--theme-primary-light:   #ff5a7d;  // Lighter Red
--theme-secondary:       #f5f5f5;  // Light Gray
--theme-background:      #ffffff;  // White
--theme-success:         #4caf50;
--theme-warning:         #ff9800;
--theme-error:           #f44336;
--theme-info:            #2196f3;
--theme-text-primary:    #212121;
--theme-text-secondary:  #757575;
--theme-text-inverse:    #ffffff;
```

---

## How Theming Works

### 1. **Design Tokens**

Components use design tokens, not raw colors:

```scss
// ❌ DON'T do this:
.button { color: #ff1744; }

// ✅ DO this:
.button { color: var(--color-primary); }
```

### 2. **Tokens Reference Theme**

Tokens are defined in `_tokens.scss` and reference theme variables:

```scss
--color-primary: var(--theme-primary);
--color-background: var(--theme-background);
```

### 3. **Components Use Tokens**

All component styles use tokens, never raw colors:

```scss
.card {
  background-color: var(--color-surface);
  color: var(--color-text-primary);
  border: 1px solid var(--color-border);
}
```

---

## Creating a New Theme

### Step 1: Create a Theme File

Create a new file in `frontend/app/src/styles/` named `_theme-[name].scss`:

```scss
// frontend/app/src/styles/_theme-dark.scss

:root {
  // --- Brand colors ---
  --theme-primary:         #bb86fc;  // Purple
  --theme-primary-dark:    #9a67ea;
  --theme-primary-light:   #d9b0ff;
  --theme-secondary:       #03dac6;  // Teal
  --theme-background:      #121212;  // Dark
  --theme-surface:         #1e1e1e;
  --theme-text-primary:    #ffffff;
  --theme-text-secondary:  #bdbdbd;
  --theme-text-inverse:    #121212;
  
  // ... other theme variables
}
```

### Step 2: Update Import in `styles.scss`

Change which theme is imported:

```scss
// OLD:
@use 'styles/theme';

// NEW:
@use 'styles/theme-dark';
```

### Step 3: Test

Rebuild and verify all colors update automatically.

---

## Theme Variables Explained

### Brand Colors

- `--theme-primary` — Main brand color (buttons, links, accents)
- `--theme-primary-dark` — Darker variant (hover states, active states)
- `--theme-primary-light` — Lighter variant (backgrounds, disabled states)
- `--theme-secondary` — Secondary accent color
- `--theme-secondary-dark` / `--theme-secondary-light` — Variants

### Surfaces & Backgrounds

- `--theme-background` — Page/body background
- `--theme-surface` — Cards, panels, surfaces
- `--theme-surface-variant` — Slightly different surface shade (alternative panels)

### Semantic Colors

- `--theme-success` — Positive actions (✓ check marks)
- `--theme-warning` — Cautions (⚠ warnings)
- `--theme-error` — Destructive actions (✗ errors, deletes)
- `--theme-info` — Informational (ℹ info messages)

### Text Colors

- `--theme-text-primary` — Main text color
- `--theme-text-secondary` — Secondary/muted text
- `--theme-text-inverse` — Text on dark/primary backgrounds (usually white)

---

## Token Names (for Components)

Components should always use these tokens, not theme variables directly:

### Colors

```scss
--color-primary              // Primary brand color
--color-primary-dark         // Darker variant
--color-primary-light        // Lighter variant
--color-secondary            // Secondary color
--color-success              // Success state
--color-warning              // Warning state
--color-error                // Error state
--color-info                 // Info state
--color-background           // Page background
--color-surface              // Card/panel background
--color-border               // Border color
--color-text                 // Primary text (alias)
--color-text-primary         // Primary text
--color-text-secondary       // Secondary text
--color-text-muted           // Alias for secondary
--color-text-inverse         // Inverse text
--color-danger               // Alias for error
```

### Spacing

```scss
--space-1  through --space-16   // 4px to 64px
```

### Typography

```scss
--font-family-base           // Main font
--font-family-mono           // Monospace font
--font-size-xs to --font-size-4xl   // 12px to 36px
--font-weight-normal         // 400
--font-weight-medium         // 500
--font-weight-semibold       // 600
--font-weight-bold           // 700
--line-height-tight          // 1.25
--line-height-normal         // 1.5
--line-height-loose          // 1.75
```

### Spacing & Layout

```scss
--radius-sm to --radius-full // Border radii
--shadow-sm, --shadow-md, --shadow-lg  // Drop shadows
--transition-fast, --transition-normal, --transition-slow  // Animations
--z-dropdown through --z-toast  // Z-index scale
```

---

## Example: Styling a Component

```scss
// ✅ CORRECT — uses tokens
.primary-button {
  background-color: var(--color-primary);
  color: var(--color-text-inverse);
  padding: var(--space-4);
  border-radius: var(--radius-lg);
  font-weight: var(--font-weight-semibold);
  transition: background-color var(--transition-normal);
  
  &:hover {
    background-color: var(--color-primary-dark);
  }
  
  &:disabled {
    background-color: var(--color-primary-light);
    opacity: 0.6;
  }
}

// Card component
.card {
  background-color: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-6);
  box-shadow: var(--shadow-md);
  
  .card-title {
    color: var(--color-text-primary);
    font-size: var(--font-size-xl);
    font-weight: var(--font-weight-bold);
    margin-bottom: var(--space-4);
  }
  
  .card-text {
    color: var(--color-text-secondary);
    line-height: var(--line-height-normal);
  }
}
```

---

## Switching Themes at Runtime

To support runtime theme switching (e.g., light/dark mode toggle), update `_theme.scss` dynamically via JavaScript:

```typescript
// example-theme-switcher.ts
export function setTheme(themeName: 'light' | 'dark') {
  const root = document.documentElement;
  
  if (themeName === 'dark') {
    root.style.setProperty('--theme-primary', '#bb86fc');
    root.style.setProperty('--theme-background', '#121212');
    // ... set all theme variables
  } else {
    root.style.setProperty('--theme-primary', '#ff1744');
    root.style.setProperty('--theme-background', '#ffffff');
    // ... set all theme variables
  }
  
  // Optionally save preference
  localStorage.setItem('app-theme', themeName);
}
```

---

## Best Practices

1. **Always use tokens** — never hardcode colors in components
2. **Keep themes consistent** — ensure all semantic colors (success, error, etc.) are defined
3. **Test contrast** — ensure text on backgrounds meets WCAG accessibility guidelines
4. **Document custom tokens** — if you add a new token, update this guide
5. **Use theme variables only in `_theme.scss`** — keep theme logic centralized
6. **Never import `_theme.scss` in components** — it's only imported in `styles.scss`

---

## Future: Dynamic Theming

When Booty by Beighley's full brand identity is defined, you can:
- Add multiple theme files (e.g., `_theme-light.scss`, `_theme-dark.scss`)
- Implement a theme switcher component
- Store user theme preference in the backend
- Update this guide with new brand colors and guidelines
