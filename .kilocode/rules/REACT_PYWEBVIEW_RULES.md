# React + PyWebView Development Rules

**Last Updated:** 2025-02-03
**Context:** React migration for SweetShopMa Desktop with PyWebView

---

## Critical Rules for PyWebView + React Projects

### 1. Build Configuration Rules

#### Rule: Always Use Relative Paths
**Issue:** PyWebView can't resolve absolute paths (`/assets/...`) when loading from file:// protocol
**Solution:** Set `base: './'` in vite.config.js

```javascript
// vite.config.js
export default defineConfig({
    base: './',  // REQUIRED for PyWebView file:// protocol
    // ... rest of config
});
```

**Why:** When PyWebView loads HTML from file://, absolute paths look for files at the filesystem root instead of relative to the HTML file.

---

#### Rule: Use HashRouter, Not BrowserRouter
**Issue:** BrowserRouter uses HTML5 History API which doesn't work with file:// protocol
**Error:** `SecurityError: Failed to execute 'replaceState' on 'History'`

```javascript
// ❌ WRONG - BrowserRouter
import { BrowserRouter } from 'react-router-dom';

// ✅ CORRECT - HashRouter
import { HashRouter } from 'react-router-dom';

function App() {
    return (
        <HashRouter>  {/* Use HashRouter for PyWebView */}
            <Routes>
                {/* routes */}
            </Routes>
        </HashRouter>
    );
}
```

**Why:** HashRouter uses URL hashes (#/dashboard) which work with any protocol including file://.

---

### 2. Module & Import Rules

#### Rule: Always Check Module Existence Before Accessing
**Issue:** `module` is undefined in production/PyWebView environments
**Error:** `ReferenceError: module is not defined`

```javascript
// ❌ WRONG
if (module.hot) {
    module.hot.accept();
}

// ✅ CORRECT
if (typeof module !== 'undefined' && module.hot) {
    module.hot.accept();
}
```

**Why:** The `module` global only exists in webpack/Vite dev environments, not in production or PyWebView.

---

#### Rule: Import Everything You Use
**Issue:** Forgetting to import hooks causes "is not defined" errors
**Error:** `ReferenceError: useAuth is not defined`

```javascript
// ❌ WRONG - Missing import
import { AuthProvider } from './context/AuthContext';

function App() {
    function ProtectedRoute() {
        const { isAuthenticated } = useAuth();  // ERROR!
    }
}

// ✅ CORRECT - Import everything
import { AuthProvider, useAuth } from './context/AuthContext';

function App() {
    function ProtectedRoute() {
        const { isAuthenticated } = useAuth();  // Works!
    }
}
```

**Why:** JavaScript doesn't automatically include exports - you must explicitly import them.

---

### 3. Context Provider Rules

#### Rule: Define Context-Using Components Inside Providers
**Issue:** Components outside providers can't access context
**Error:** `useAuth is not defined` or context returns undefined

```javascript
// ❌ WRONG - ProtectedRoute outside provider
function ProtectedRoute({ children }) {
    const { isAuthenticated } = useAuth();  // ERROR!
    // ...
}

function App() {
    return (
        <AuthProvider>
            <ProtectedRoute>{/*...*/}</ProtectedRoute>
        </AuthProvider>
    );
}

// ✅ CORRECT - ProtectedRoute inside App component
function App() {
    // Define inside App where it has access to AuthContext
    function ProtectedRoute({ children }) {
        const { isAuthenticated } = useAuth();  // Works!
        // ...
    }
    
    return (
        <AuthProvider>
            <ProtectedRoute>{/*...*/}</ProtectedRoute>
        </AuthProvider>
    );
}
```

**Why:** React Context only works for components that are descendants of the Provider component.

---

### 4. PyWebView Integration Rules

#### Rule: Use file:// URL, Not HTML String
**Issue:** Loading HTML as string breaks relative path resolution
**Solution:** Convert file path to URI

```python
# ❌ WRONG - HTML content as string
html_content = open('index.html').read()
window = webview.create_window('App', html=html_content)

# ✅ CORRECT - file:// URL
from pathlib import Path
index_html = Path('frontend/build/index.html')
file_url = index_html.resolve().as_uri()
window = webview.create_window('App', url=file_url)
```

**Why:** file:// URLs properly resolve relative paths (./assets/...) relative to the HTML file location.

---

#### Rule: Check API Method Existence Before Calling
**Issue:** PyWebView API methods may not exist in ApiBridge
**Solution:** Use typeof check before calling

```javascript
// ❌ WRONG - Assumes method exists
if (window.pywebview && window.pywebview.api) {
    await window.pywebview.api.authenticate_user(user, pass);  // ERROR!
}

// ✅ CORRECT - Check method exists
if (window.pywebview && 
    window.pywebview.api && 
    typeof window.pywebview.api.authenticate_user === 'function') {
    await window.pywebview.api.authenticate_user(user, pass);  // Works!
} else {
    // Fallback to mock/alternative
}
```

**Why:** Prevents "is not a function" errors when methods don't exist in the API bridge.

---

### 5. Build & Development Rules

#### Rule: Always Rebuild After Source Changes
**Issue:** PyWebView loads built files, not source
**Checklist:**
- [ ] Modified .jsx files? → Rebuild
- [ ] Modified .js files? → Rebuild
- [ ] Modified vite.config.js? → Rebuild
- [ ] Modified CSS files? → Rebuild

```bash
# After ANY source code change:
cd sweetshopma-desktop/frontend
npm run build
```

**Why:** PyWebView loads from build/ directory, not src/ directory.

---

#### Rule: Test in Browser Before PyWebView
**Workflow:**
1. Test in browser: `npm run dev` (http://localhost:3000)
2. Verify functionality works
3. Then build: `npm run build`
4. Then test in PyWebView

**Why:** Browser dev tools are better for debugging. PyWebView has limited debugging.

---

### 6. API Integration Rules

#### Rule: Handle Both Paginated and Direct Array Responses
**Issue:** DRF ViewSets return different formats
**Solution:** Always normalize responses

```javascript
// ✅ CORRECT - Handle both formats
const normalizeResponse = (response) => {
    if (Array.isArray(response)) {
        return response;  // Direct array
    }
    if (response && response.results && Array.isArray(response.results)) {
        return response.results;  // Paginated
    }
    return [];
};

// Use it
const data = normalizeResponse(await getProducts());
```

**Why:** Django REST Framework can return either format depending on pagination settings.

---

#### Rule: Separate API Service from Data Fetching Hooks
**Architecture:**
- `services/apiService.js` - Raw API calls via `useApi()`
- `hooks/useApiData.js` - Data fetching with loading/error states
- Components use hooks, not service directly

```javascript
// ✅ CORRECT - Component uses hook
function DashboardPage() {
    const { data, loading } = useDashboardStats();
    // ...
}

// ❌ WRONG - Component uses service directly
function DashboardPage() {
    const { getDashboardStats } = useApi();
    const [data, setData] = useState();
    
    useEffect(() => {
        getDashboardStats().then(setData);  // Missing error handling!
    }, []);
}
```

**Why:** Hooks provide consistent loading/error handling and automatic refetching.

---

### 7. Import Path Rules

#### Rule: Know Where Things Are Exported
**Common Mistake:** Importing from wrong location

```javascript
// ✅ CORRECT - API service from services
import { useApi } from '../services';  // NOT from '../hooks'

// ✅ CORRECT - Data hooks from hooks
import { useProducts, useDashboardStats } from '../hooks';

// ✅ CORRECT - Utilities from utils
import { formatCurrency } from '../utils';
```

**Why:** Barrel exports (index.js) make this easier, but you still need to know the correct source.

---

### 8. Error Handling Rules

#### Rule: Always Handle Errors in Async Operations
**Issue:** Unhandled promise rejections crash the app

```javascript
// ✅ CORRECT - Try-catch with user feedback
const handleLogin = async (username, password) => {
    try {
        setIsLoading(true);
        const result = await login(username, password);
        
        if (result.success) {
            navigate('/dashboard');
        } else {
            setError(result.error);
        }
    } catch (err) {
        console.error('[Login] Error:', err);
        setError('Login failed. Please try again.');
    } finally {
        setIsLoading(false);
    }
};
```

**Why:** Users need feedback when things go wrong. Silent failures are confusing.

---

### 9. CSS & Styling Rules

#### Rule: Use CSS Variables for Theming
**Issue:** Hard-coded colors make theming difficult

```css
/* ✅ CORRECT - CSS variables */
:root {
    --color-primary: #0066cc;
    --color-background: #ffffff;
}

.button {
    background-color: var(--color-primary);
}

/* ❌ WRONG - Hard-coded values */
.button {
    background-color: #0066cc;
}
```

**Why:** Variables make it easy to implement dark mode and theme switching.

---

#### Rule: Add Utility Classes for Common Patterns
**Examples:** Flexbox, spacing, text alignment

```css
/* ✅ CORRECT - Reusable utilities */
.d-flex { display: flex; }
.justify-between { justify-content: space-between; }
.gap-3 { gap: 1rem; }
.text-center { text-align: center; }
```

**Why:** Reduces CSS duplication and speeds up development.

---

### 10. Performance Rules

#### Rule: Lazy Load Pages
**Issue:** Loading all pages upfront slows initial load

```javascript
// ✅ CORRECT - Lazy loading
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const POSPage = lazy(() => import('./pages/POSPage'));

function App() {
    return (
        <Suspense fallback={<LoadingSpinner />}>
            <Routes>
                <Route path="/dashboard" element={<DashboardPage />} />
                <Route path="/pos" element={<POSPage />} />
            </Routes>
        </Suspense>
    );
}
```

**Why:** Code splitting reduces initial bundle size and improves load time.

---

## Pre-Flight Checklist

### Before Building for PyWebView
- [ ] Set `base: './'` in vite.config.js
- [ ] Using HashRouter, not BrowserRouter
- [ ] All imports are present and correct
- [ ] Context-using components inside providers
- [ ] module.hot has safety check
- [ ] API methods have existence checks
- [ ] Build completes without errors
- [ ] build/ directory exists with index.html

### Before Running PyWebView
- [ ] Django backend is running on port 8000
- [ ] CORS is enabled in Django settings
- [ ] Build is up to date (rebuilt after source changes)
- [ ] main.py points to correct build/index.html
- [ ] API bridge methods exist or have fallbacks

---

## Common Errors & Solutions

### Error: "Failed to execute 'replaceState' on 'History'"
**Cause:** Using BrowserRouter with file:// protocol
**Fix:** Change to HashRouter

### Error: "module is not defined"
**Cause:** Accessing `module.hot` without checking
**Fix:** `if (typeof module !== 'undefined' && module.hot)`

### Error: "useAuth is not defined"
**Cause:** Missing import or component outside provider
**Fix:** Import useAuth and/or move component inside provider

### Error: "authenticate_user is not a function"
**Cause:** Method doesn't exist in ApiBridge
**Fix:** Check `typeof window.pywebview.api.authenticate_user === 'function'`

### Error: White page, no content
**Cause:** Multiple possible issues:
1. Build not updated → Rebuild
2. Absolute paths → Set `base: './'`
3. JS errors → Check browser console
4. module.hot error → Add safety check

---

## Development Workflow

### Standard Development Cycle
1. **Write code** in src/ directory
2. **Test in browser:** `npm run dev`
3. **Fix issues** using browser DevTools
4. **Build for PyWebView:** `npm run build`
5. **Test in PyWebView:** `python frontend/main.py`
6. **Debug PyWebView issues** (if any)

### Hot Reloading in PyWebView
**Current limitation:** PyWebView doesn't support hot reloading
**Workaround:** After code changes:
1. Rebuild: `npm run build`
2. Restart PyWebView app

---

## File Organization Best Practices

### Barrel Exports (index.js)
Create index.js files to group exports:

```javascript
// hooks/index.js
export { usePyWebView } from './usePyWebView';
export { useApiData, useProducts, useDashboardStats } from './useApiData';

// Usage
import { useProducts, useDashboardStats } from '../hooks';
```

### Component Organization
```
src/
├── components/
│   ├── common/          # Reusable UI components
│   └── layout/           # Layout-specific components
├── pages/                # Route pages
├── services/             # API service layer
├── hooks/                # Custom React hooks
├── utils/                # Helper functions
└── context/              # React Context providers
```

---

## Testing Strategy

### Unit Testing
- Test utility functions with various inputs
- Test formatters with edge cases (null, undefined, empty)
- Test validators with valid and invalid data

### Integration Testing
- Test API calls return expected data
- Test hooks manage loading/error states
- Test components render with mock data

### End-to-End Testing
- Test complete user flows (login → dashboard → POS → logout)
- Test navigation between all pages
- Test CRUD operations work end-to-end

---

## Documentation Requirements

### Code Comments
- JSDoc comments for all components
- Parameter descriptions for functions
- Return type documentation
- Usage examples for complex hooks

### README Updates
Document:
- Build process
- PyWebView setup
- Environment variables
- Troubleshooting guide

---

## Security Considerations

### PyWebView API Security
- Validate all API responses
- Sanitize user inputs before API calls
- Handle errors gracefully
- Don't expose sensitive data in console logs

### Authentication
- Store tokens securely (sessionStorage for PyWebView)
- Clear auth data on logout
- Implement token refresh if using JWT
- Handle expired tokens

---

## Performance Optimization

### Code Splitting
- Lazy load routes
- Split vendor bundles
- Dynamic imports for large libraries

### Asset Optimization
- Minify CSS and JS in production
- Compress images
- Use modern image formats (WebP)

### Bundle Size
- Keep under 500KB for initial load
- Lazy load non-critical features
- Use tree shaking to remove unused code

---

## Browser Compatibility

### Target: ES2015 (ES6)
**Supported:**
- Chrome 51+
- Edge 15+
- Safari 10+
- Firefox 54+
- Modern PyWebView webview engines

**Avoid:**
- ES2020+ features (optional chaining, nullish coalescing)
- Experimental APIs
- Browser-specific features

---

## Troubleshooting Guide

### Build Issues
**Problem:** Build fails with errors
**Solutions:**
1. Clear node_modules: `rm -rf node_modules && npm install`
2. Clear build: `rm -rf build`
3. Check for syntax errors in source files
4. Verify all imports are correct

### PyWebView Issues
**Problem:** App loads but doesn't work
**Solutions:**
1. Check browser console for errors
2. Verify Django backend is running
3. Check CORS settings
4. Verify API bridge methods exist
5. Check build is up to date

### Navigation Issues
**Problem:** Routes don't work
**Solutions:**
1. Verify using HashRouter
2. Check route definitions
3. Verify URL format (use #/ for HashRouter)
4. Check for ProtectedRoute blocking access

---

## Success Criteria

A PyWebView + React app is ready when:
- [ ] Builds without errors
- [ ] Loads in PyWebView without white screen
- [ ] Navigation works between all pages
- [ ] Login/logout functionality works
- [ ] API calls succeed
- [ ] Data displays correctly
- [ ] No console errors
- [ ] Responsive on different screen sizes

---

## Maintenance

### Regular Tasks
- Update dependencies monthly
- Review and update documentation
- Check for security vulnerabilities
- Optimize bundle sizes
- Test on target PyWebView version

### When Upgrading
- Test in browser first
- Check breaking changes in React/Router
- Update build configuration if needed
- Test all pages after upgrade
- Update documentation

---

**Remember:** PyWebView has limitations compared to full browsers. Always test in PyWebView before considering features "done".

**Last Updated:** 2025-02-03
**Status:** Active - Based on real migration experience
