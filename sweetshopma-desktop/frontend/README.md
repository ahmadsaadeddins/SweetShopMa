# SweetShopMa Desktop - React Frontend

Modern React-based frontend for the SweetShopMa PyWebView desktop application.

## Overview

This frontend uses React 18 with modern tooling (Vite, Babel) to transpile ES6+ code to ES5 JavaScript compatible with PyWebView's embedded webview engine.

## Technology Stack

- **React 18** - UI framework
- **React Router DOM v6** - Client-side routing
- **Vite** - Build tool and dev server
- **Babel** - ES6+ to ES5 transpilation
- **Core-js** - Polyfills for ES5 compatibility
- **Lucide React** - Icon library

## Project Structure

```
frontend/
├── src/
│   ├── components/         # React components
│   │   ├── common/        # Reusable UI components
│   │   ├── layout/        # Layout components (Header, Sidebar, etc.)
│   │   └── features/      # Feature-specific components
│   ├── pages/             # Page components
│   ├── hooks/             # Custom React hooks
│   ├── services/          # API and business logic
│   ├── context/           # React Context providers
│   ├── utils/             # Helper functions
│   ├── styles/            # CSS styles
│   ├── App.jsx            # Root component
│   └── index.js           # Entry point
├── public/                # Static assets
├── build/                 # Transpiled ES5 output (generated)
├── templates/             # Legacy templates (to remove after migration)
├── api.py                 # ApiBridge (keep - used by React)
├── main.py                # PyWebView entry point (modify)
├── package.json           # Node.js dependencies
├── vite.config.js         # Vite configuration
└── babel.config.js        # Babel configuration
```

## Getting Started

### Prerequisites

- Node.js 16+ and npm 8+
- Python 3.8+ (for PyWebView backend)

### Installation

1. Install dependencies:
```bash
cd sweetshopma-desktop/frontend
npm install
```

### Development

Run the development server (for browser testing):
```bash
npm run dev
```

The dev server runs on `http://localhost:3000` and proxies API requests to the Django backend at `http://127.0.0.1:8000`.

### Build for Production

Build the React app for PyWebView:
```bash
npm run build
```

This creates ES5-compatible JavaScript in the `build/` directory.

### Linting

Run ESLint:
```bash
npm run lint
```

### Formatting

Format code with Prettier:
```bash
npm run format
```

## PyWebView Integration

### Loading the React App

Update `main.py` to load the built React app:

```python
def create_window(self):
    """Create PyWebView window with React app"""
    from pathlib import Path
    from api import ApiBridge
    
    self.api_bridge = ApiBridge()
    
    # Load built React app
    react_html_path = Path(__file__).parent / 'build' / 'index.html'
    
    with open(react_html_path, 'r', encoding='utf-8') as f:
        html = f.read()
    
    # Create window
    self.window = webview.create_window(
        'SweetShopMa Desktop',
        html=html,
        js_api=self.api_bridge,
        width=1280,
        height=720,
        min_size=(1024, 768)
    )
```

### API Communication

React components communicate with the Python backend via `window.pywebview.api`:

```javascript
// Example API call
const response = await window.pywebview.api.some_method(params);
```

## Key Features

### Authentication

The `AuthContext` provides authentication state and methods:

```javascript
import { useAuth } from './context/AuthContext';

function MyComponent() {
  const { user, login, logout, isAuthenticated } = useAuth();
  
  // Use authentication state
}
```

### Theme Management

The `ThemeContext` provides light/dark mode support:

```javascript
import { useTheme } from './context/ThemeContext';

function MyComponent() {
  const { theme, toggleTheme } = useTheme();
  
  // Use theme state
}
```

### Routing

Protected routes require authentication:

```javascript
// Protected route
<Route path="/dashboard" element={
  <ProtectedRoute>
    <DashboardPage />
  </ProtectedRoute>
} />
```

## ES5 Compatibility

This project is configured to transpile modern JavaScript (ES6+) to ES5 for PyWebView compatibility:

- **Babel preset-env** targets IE11 for maximum ES5 compatibility
- **Core-js** provides polyfills for missing features
- **Vite** builds to ES5 output

### What Works

✅ Modern React (hooks, functional components)
✅ JSX syntax
✅ Async/await (transpiled to Promises)
✅ Destructuring (transpiled to ES5)
✅ Template literals (transpiled to string concatenation)
✅ Arrow functions (transpiled to regular functions)

### What to Avoid

❌ Features that cannot be transpiled to ES5:
- Proxy objects (use Object.defineProperty instead)
- WeakMap/WeakSet (use regular Map/Set instead)
- Some newer browser APIs

## Migration Status

### Phase 1: Build System Setup ✅

- [x] Initialize Node.js project
- [x] Configure Vite for ES5 output
- [x] Configure Babel for transpilation
- [x] Create base project structure
- [x] Set up ESLint and Prettier

### Phase 2: Core Infrastructure 🚧

- [x] Create common UI components (ErrorBoundary, LoadingSpinner)
- [x] Create layout components (AppLayout, Header, Sidebar)
- [x] Set up routing with React Router
- [x] Create authentication context
- [x] Create theme context
- [ ] Create API service hooks
- [ ] Create utility functions

### Phase 3: Page Migration 📋

- [x] LoginPage (basic implementation)
- [ ] DashboardPage (full implementation)
- [ ] POSPage (full implementation)
- [ ] ProductsPage (full implementation)
- [ ] SalesPage (full implementation)
- [ ] ExpensesPage (full implementation)
- [ ] SettingsPage (full implementation)

### Phase 4: Advanced Features 📋

- [ ] Hardware integration (cash drawer, printer)
- [ ] Offline/sync features
- [ ] Performance optimization
- [ ] Error handling improvements

### Phase 5: Testing & Cleanup 📋

- [ ] Manual testing
- [ ] Performance testing
- [ ] Remove legacy templates
- [ ] Update documentation

## Troubleshooting

### Build Errors

If you encounter build errors:

1. Clear node_modules and reinstall:
```bash
rm -rf node_modules package-lock.json
npm install
```

2. Clear Vite cache:
```bash
rm -rf .vite dist build
npm run build
```

### PyWebView Not Loading

If the React app doesn't load in PyWebView:

1. Check that the build directory exists and contains `index.html`
2. Verify the path in `main.py` points to the correct location
3. Check the browser console for JavaScript errors
4. Ensure all polyfills are loaded correctly

### API Calls Failing

If API calls fail:

1. Verify CORS is enabled in Django settings
2. Check that `pywebview.api` is available in the browser console
3. Ensure the backend server is running on port 8000

## Contributing

When adding new features:

1. Use functional components with hooks
2. Follow the existing component structure
3. Use CSS variables for styling
4. Implement proper error handling
5. Test in both browser and PyWebView environments

## License

This project is part of SweetShopMa Desktop.
