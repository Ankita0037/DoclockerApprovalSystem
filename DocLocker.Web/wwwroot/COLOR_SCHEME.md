# DocLocker Color Scheme Guide

## 🎨 Logo-Inspired Color Palette

Based on the DocLocker approval workflow logo, the following professional color scheme has been implemented:

### Primary Colors
- **Dark Blue (Primary)**: `#0D2B6C` - Main brand color, used for headers, text, and primary buttons
- **Bright Blue (Primary Light)**: `#0085CA` - Accent color, used for hover states and highlights
- **Dark Navy (Primary Dark)**: `#051D47` - Used for footer and active states

### Success & Approval
- **Green (Success)**: `#1EB854` - Used for approval buttons, success messages, and positive actions
- **Green Hover**: `#18A649` - Darker shade for hover states

### Secondary Colors
- **Light Blue Background**: `#E8F2F8` - Soft background for auth pages and accents
- **Light Accent**: `#F0F6FA` - Subtle background for feature sections
- **Text Dark**: `#1A1A1A` - Primary text color
- **Text Muted**: `#616161` - Secondary text and descriptions

## 📍 Where Colors Are Used

### Navigation Bar
- **Background**: White with gradient to light blue
- **Logo Text**: Dark Blue (#0D2B6C)
- **Nav Links**: Dark text with blue hover effect
- **Buttons**: Dark blue primary, bright blue hover

### Hero Section
- **Background**: Gradient from Dark Blue to Bright Blue
- **Text**: White
- **CTA Buttons**: White with blue hover effect

### Features Section
- **Background**: Gradient from white to light blue
- **Feature Icons**: Bright Blue (#0085CA)
- **Card Titles**: Dark Blue (#0D2B6C)
- **Card Descriptions**: Muted text (#616161)
- **Left Border**: Bright Blue on hover

### Approval/Success Elements
- **Approve Button**: Green (#1EB854)
- **Success Badges**: Green background
- **Check Icons**: Green color
- **Success Alerts**: Light green background

### Login/Register Pages
- **Background**: Gradient (Light accent colors)
- **Card Border**: Bright Blue top border
- **Form Labels**: Dark Blue
- **Form Focus**: Bright Blue border and shadow
- **Submit Button**: Dark Blue primary button
- **Links**: Bright Blue (#0085CA)

### Footer
- **Background**: Gradient from Dark Blue to Navy
- **Top Border**: Bright Blue
- **Text**: White
- **Links Hover**: Bright Blue

## 🔧 CSS Variables

All colors are defined as CSS custom properties in `:root`:

```css
:root {
    --primary-color: #0D2B6C;           /* Dark Blue */
    --primary-light: #0085CA;           /* Bright Blue */
    --primary-dark: #051D47;            /* Navy */
    --success-color: #1EB854;           /* Green */
    --success-hover: #18A649;           /* Green Hover */
    --light-color: #E8F2F8;             /* Light Blue */
    --light-accent: #F0F6FA;            /* Light Accent */
    --dark-color: #0D2B6C;              /* Dark Blue */
    --text-dark: #1A1A1A;               /* Text Dark */
    --text-muted: #616161;              /* Text Muted */
}
```

## 🎯 Using Colors in HTML

### Button Examples
```html
<!-- Primary Dark Blue Button -->
<button class="btn btn-primary">Primary Action</button>

<!-- Success Green Button -->
<button class="btn btn-success">Approve</button>

<!-- Outline Button -->
<button class="btn btn-outline-primary">Secondary</button>
```

### Badge Examples
```html
<!-- Primary Badge -->
<span class="badge badge-primary">Status</span>

<!-- Success Badge -->
<span class="badge badge-success">Approved</span>
```

### Background Classes
```html
<!-- Primary Background -->
<div class="bg-primary text-white">Content</div>

<!-- Light Accent Background -->
<div class="bg-light-accent">Content</div>
```

### Border Classes
```html
<!-- Top Border -->
<div class="border-top-primary">Content</div>

<!-- Left Border -->
<div class="border-left-primary">Content</div>
```

## 📱 Responsive Behavior

All colors maintain consistency across devices:
- Desktop: Full gradient effects and shadows
- Tablet: Adjusted spacing with same colors
- Mobile: Simplified shadows, same color scheme

## ✨ Interactive States

### Buttons
- **Normal**: Primary color
- **Hover**: Lighter shade with elevation effect
- **Active**: Darker shade
- **Focus**: Colored shadow outline

### Form Elements
- **Focused**: Bright blue border and glow effect
- **Valid**: Green accent
- **Invalid**: Red accent
- **Disabled**: Muted gray

## 🎨 Brand Consistency

The color scheme ensures:
- ✅ Professional appearance with blue/green trustworthy palette
- ✅ Consistent with document approval workflow theme
- ✅ High contrast for accessibility
- ✅ Clear distinction between actions (approve = green, primary = blue)
- ✅ Smooth transitions and hover effects

## 📝 Notes

- Always use CSS variables (`var(--primary-color)`) instead of hardcoded hex values
- Maintain the gradient patterns for hero and footer sections
- Keep borders and shadows using the color variables for consistency
- Test colors on different devices for accessibility
