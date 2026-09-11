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

## Current Theme: Booty By Beighley Zodiac Branding

The active theme features the official Booty By Beighley branding:
- **Background**: Clean white (`#FFFFFF`)
- **Primary Color**: Mauve rose pink (`#D798A8`) - used for headings and primary actions
- **Secondary Color**: Sage green (`#8B8B4F`) - used for accents and borders
- **Accent Color**: Golden yellow (`#F4E34D`) - success, highlights, and emphasis
- **Text**: Dark text on light backgrounds, cream/off-white on dark backgrounds

### Theme Variables

All theme colors are defined in `frontend/app/src/styles/_theme.scss`:

```scss
--theme-primary:         #D798A8;  // Mauve Rose Pink - primary headings
--theme-primary-dark:    #B87090;  // Darker mauve
--theme-primary-light:   #E5B5C8;  // Lighter mauve
--theme-secondary:       #8B8B4F;  // Sage Green - accents
--theme-secondary-dark:  #6F6F3F;  // Darker sage
--theme-secondary-light: #ABABAF;  // Lighter sage
--theme-background:      #8B8B4F;  // Sage green background
--theme-surface:         #ABABAF;  // Light sage surface
--theme-success:         #F4E34D;  // Golden Yellow - accents
--theme-error:           #D45757;  // Muted red
--theme-text-primary:    #2D2D2D;  // Dark text
--theme-text-secondary:  #666666;  // Gray text
--theme-text-inverse:    #FFFAF0;  // Cream on dark
```

### Typography

The theme uses two custom fonts:
- **Primary (Kabel)**: Used for all headings (h1-h6) with uppercase text transform
- **Secondary (Hibernate)**: Used for body text, paragraphs, and descriptive copy
- **Fallback**: System fonts if custom fonts are not loaded

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

## Setting Up Custom Fonts

The app is configured to use custom fonts:
- **Kabel** — Primary font for headings (h1-h6)
- **Hibernate** — Secondary font for body text and paragraphs

### Adding Font Files

1. **Create fonts directory:**
   ```
   frontend/app/src/assets/fonts/
   ├── kabel/
   │   ├── kabel-regular.woff2
   │   ├── kabel-bold.woff2
   │   └── kabel-italic.woff2
   └── hibernate/
       ├── hibernate-regular.woff2
       └── hibernate-italic.woff2
   ```

2. **Add @font-face declarations to `_typography.scss`:**
   ```scss
   @font-face {
     font-family: 'Kabel';
     src: url('/assets/fonts/kabel/kabel-regular.woff2') format('woff2');
     font-weight: 400;
     font-display: swap;
   }

   @font-face {
     font-family: 'Kabel';
     src: url('/assets/fonts/kabel/kabel-bold.woff2') format('woff2');
     font-weight: 700;
     font-display: swap;
   }

   @font-face {
     font-family: 'Hibernate';
     src: url('/assets/fonts/hibernate/hibernate-regular.woff2') format('woff2');
     font-weight: 400;
     font-display: swap;
   }
   ```

3. **Or use Google Fonts (if available):**
   Update `frontend/app/src/index.html`:
   ```html
   <link href="https://fonts.googleapis.com/css2?family=Kabel&family=Hibernate&display=swap" rel="stylesheet">
   ```

### Font Variables

All fonts are defined as CSS custom properties in `_tokens.scss`:

```scss
--font-family-primary:   'Kabel', 'Segoe UI', sans-serif;
--font-family-secondary: 'Hibernate', 'Georgia', serif;
--font-family-base:      'Kabel', system-ui, -apple-system, sans-serif;
--font-family-mono:      'Fira Code', 'Cascadia Code', monospace;
```

**Note:** Fallback fonts are included to ensure text displays correctly even if custom fonts fail to load.

---

## Best Practices

1. **Always use tokens** — never hardcode colors in components
2. **Keep themes consistent** — ensure all semantic colors (success, error, etc.) are defined
3. **Test contrast** — ensure text on backgrounds meets WCAG accessibility guidelines
4. **Document custom tokens** — if you add a new token, update this guide
5. **Use theme variables only in `_theme.scss`** — keep theme logic centralized
6. **Never import `_theme.scss` in components** — it's only imported in `styles.scss`

---

## Current Branding Status

✅ **Colors:** Updated to match Booty By Beighley official branding
- Sage green backgrounds, mauve pink headings, golden yellow accents

✅ **Kabel Font:** Custom font loaded and active
- Multiple weights available: Light (300), Book/Regular (400), Bold (700), Heavy (900)
- Applied to all headings (h1-h6) with uppercase text transform
- Located in: `frontend/app/src/assets/fonts/Kabel-*.woff2`

⏳ **Hibernate Font:** Configuration ready, awaiting font files
- Fallback to Georgia serif in the meantime
- Add font file to `src/assets/fonts/` when available
- Update @font-face in `_typography.scss` when ready

🎯 **Next Steps:**
1. Obtain Hibernate font file (or find serif font equivalent)
2. Place file in `frontend/app/src/assets/fonts/`
3. Add @font-face declaration to `_typography.scss`
4. Test on all platforms to ensure fonts load correctly
5. Verify contrast ratios meet WCAG AA accessibility standards
