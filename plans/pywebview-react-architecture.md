# PyWebView React Architecture Diagram

## System Architecture Overview

```mermaid
graph TB
    subgraph "Desktop Application Layer"
        A[main.py<br/>Entry Point]
        B[ApiBridge<br/>Python API Layer]
        C[Django Backend<br/>http://127.0.0.1:8000]
        D[SyncService<br/>Background Sync]
    end
    
    subgraph "PyWebView Layer"
        E[webview.create_window<br/>Native Window]
        F[build/index.html<br/>React Entry Point]
    end
    
    subgraph "React Application Layer"
        G[React App<br/>App.jsx]
        H[React Router<br/>Navigation]
        I[Context Providers<br/>Auth/Theme]
        J[Page Components<br/>Dashboard/POS/etc.]
    end
    
    subgraph "React Services Layer"
        K[usePyWebView Hook<br/>API Bridge]
        L[useApi Hook<br/>Data Fetching]
        M[Utility Functions<br/>Formatters/Validators]
    end
    
    A -->|Starts| C
    A -->|Creates| E
    A -->|Initializes| D
    E -->|Loads| F
    F -->|Renders| G
    G -->|Uses| H
    G -->|Provides| I
    H -->|Routes to| J
    J -->|Calls| L
    L -->|Uses| K
    K -->|Calls via| B
    B -->|HTTP Requests| C
    J -->|Uses| M
    
    style A fill:#e1f5ff
    style B fill:#fff4e1
    style C fill:#ffe1e1
    style D fill:#e1ffe1
    style E fill:#f0e1ff
    style F fill:#f0e1ff
    style G fill:#e1f5ff
    style H fill:#e1f5ff
    style I fill:#e1f5ff
    style J fill:#e1f5ff
    style K fill:#ffe1f0
    style L fill:#ffe1f0
    style M fill:#ffe1f0
```

## Data Flow Diagram

```mermaid
sequenceDiagram
    participant User
    participant React as React Component
    participant Hook as useApi Hook
    participant PyWebView as usePyWebView Hook
    participant Bridge as ApiBridge (Python)
    participant Django as Django Backend
    
    User->>React: User Action (e.g., Load Products)
    React->>Hook: const { getProducts } = useApi()
    Hook->>PyWebView: callApi('get_products', filters)
    PyWebView->>Bridge: window.pywebview.api.get_products()
    Bridge->>Django: HTTP GET /api/products/
    Django-->>Bridge: JSON Response
    Bridge-->>PyWebView: Return Data
    PyWebView-->>Hook: Parsed Data
    Hook-->>React: { data, loading, error }
    React->>User: Render Products
```

## Component Hierarchy

```mermaid
graph TD
    A[App.jsx<br/>Root Component] --> B[ErrorBoundary]
    B --> C[ThemeProvider]
    C --> D[AuthProvider]
    D --> E[BrowserRouter]
    E --> F[Suspense]
    F --> G{Routes}
    
    G --> H[LoginPage<br/>Public Route]
    G --> I[ProtectedRoute]
    
    I --> J[AppLayout]
    J --> K[Header]
    J --> L[Sidebar]
    J --> M{Protected Routes}
    
    M --> N[DashboardPage]
    M --> O[POSPage]
    M --> P[ProductsPage]
    M --> Q[SalesPage]
    M --> R[ExpensesPage]
    M --> S[SettingsPage]
    
    N --> T[Card Components]
    N --> U[Table Components]
    
    O --> V[ProductGrid]
    O --> W[CartPanel]
    
    P --> X[ProductTable]
    P --> Y[ProductForm]
    
    style A fill:#e1f5ff
    style G fill:#fff4e1
    style M fill:#ffe1e1
    style N fill:#e1ffe1
    style O fill:#e1ffe1
    style P fill:#e1ffe1
    style Q fill:#e1ffe1
    style R fill:#e1ffe1
    style S fill:#e1ffe1
```

## Phase 0: PyWebView Integration

### Current State (Template-Based)

```mermaid
graph LR
    A[main.py] -->|render_template| B[login.html]
    A -->|ApiBridge| C[get_page_html]
    C -->|renders| D[dashboard.html]
    C -->|renders| E[pos.html]
    
    style A fill:#ffe1e1
    style B fill:#ffe1e1
    style C fill:#ffe1e1
    style D fill:#ffe1e1
    style E fill:#ffe1e1
```

### Target State (React-Based)

```mermaid
graph LR
    A[main.py] -->|Loads from file| B[build/index.html]
    B -->|React Router| C[LoginPage]
    B -->|React Router| D[DashboardPage]
    B -->|React Router| E[POSPage]
    A -->|js_api| F[ApiBridge]
    C -->|Calls| F
    D -->|Calls| F
    E -->|Calls| F
    
    style A fill:#e1ffe1
    style B fill:#e1f5ff
    style C fill:#e1f5ff
    style D fill:#e1f5ff
    style E fill:#e1f5ff
    style F fill:#fff4e1
```

## Phase 2: Core Infrastructure

### API Service Layer

```mermaid
graph TD
    A[useApi Hook] --> B[usePyWebView Hook]
    B --> C[window.pywebview.api]
    C --> D[ApiBridge Methods]
    
    A --> E[getProducts]
    A --> F[getSales]
    A --> G[createSale]
    A --> H[getDashboardStats]
    A --> I[getExpenses]
    A --> J[getSyncStatus]
    
    E --> K[normalizeResponse]
    F --> K
    G --> K
    H --> K
    I --> K
    J --> K
    
    K --> L{Check Type}
    L -->|Array| M[Return Array]
    L -->|Object with results| N[Return results]
    
    style A fill:#e1f5ff
    style B fill:#fff4e1
    style C fill:#ffe1e1
    style K fill:#ffe1f0
    style L fill:#ffe1f0
```

### Data Fetching Hooks

```mermaid
graph TD
    A[useApiData Hook] --> B[useApi Hook]
    B --> C[API Call]
    
    A --> D[useProducts]
    A --> E[useSales]
    A --> F[useDashboardStats]
    A --> G[useRecentSales]
    
    D --> H[Returns]
    E --> H
    F --> H
    G --> H
    
    H --> I[data]
    H --> J[loading]
    H --> K[error]
    H --> L[refetch]
    
    style A fill:#e1f5ff
    style B fill:#e1f5ff
    style H fill:#e1ffe1
```

## Phase 3: Page Migration

### Dashboard Page Data Flow

```mermaid
graph TD
    A[DashboardPage] --> B[useDashboardStats]
    A --> C[useRecentSales]
    
    B --> D[getDashboardStats API]
    C --> E[getRecentSales API]
    
    D --> F[Today's Sales Card]
    D --> G[Today's Expenses Card]
    D --> H[Net Revenue Card]
    D --> I[Low Stock Card]
    
    E --> J[Recent Sales Table]
    
    F --> K[formatCurrency]
    G --> K
    H --> K
    
    J --> L[Table Component]
    L --> M[formatDateTime]
    
    style A fill:#e1f5ff
    style B fill:#fff4e1
    style C fill:#fff4e1
    style F fill:#e1ffe1
    style G fill:#e1ffe1
    style H fill:#e1ffe1
    style I fill:#e1ffe1
    style J fill:#e1ffe1
```

### POS Page Component Structure

```mermaid
graph TD
    A[POSPage] --> B[ProductGrid]
    A --> C[CartPanel]
    A --> D[CheckoutForm]
    
    B --> E[useProducts Hook]
    B --> F[Search/Filter]
    
    C --> G[Cart State]
    C --> H[Cart Items]
    C --> I[Cart Totals]
    
    D --> J[Payment Method]
    D --> K[Discount Input]
    D --> L[Complete Sale Button]
    
    L --> M[createSale API]
    M --> N[Clear Cart]
    M --> O[Show Receipt]
    
    style A fill:#e1f5ff
    style B fill:#e1ffe1
    style C fill:#e1ffe1
    style D fill:#e1ffe1
    style M fill:#fff4e1
```

## File Structure After Migration

```
sweetshopma-desktop/frontend/
├── build/                          # Built React app (loaded by PyWebView)
│   ├── index.html                  # Entry point
│   └── assets/                     # JS/CSS bundles
│       ├── index-*.js
│       ├── index-*.css
│       └── *Page-*.js              # Lazy-loaded page chunks
├── src/
│   ├── components/
│   │   ├── common/
│   │   │   ├── Button.jsx          # NEW
│   │   │   ├── Input.jsx           # NEW
│   │   │   ├── Card.jsx            # NEW
│   │   │   ├── Table.jsx           # NEW
│   │   │   ├── ErrorBoundary.jsx   # Existing
│   │   │   └── LoadingSpinner.jsx  # Existing
│   │   └── layout/
│   │       ├── AppLayout.jsx       # Existing
│   │       ├── Header.jsx          # Existing
│   │       └── Sidebar.jsx         # Existing
│   ├── context/
│   │   ├── AuthContext.jsx         # Existing - Update with real auth
│   │   └── ThemeContext.jsx        # Existing
│   ├── hooks/
│   │   ├── usePyWebView.js         # NEW - PyWebView bridge
│   │   └── useApiData.js           # NEW - Data fetching hooks
│   ├── pages/
│   │   ├── LoginPage.jsx           # Update - Real auth
│   │   ├── DashboardPage.jsx       # Update - Real data
│   │   ├── POSPage.jsx             # Update - Full functionality
│   │   ├── ProductsPage.jsx        # Update - CRUD operations
│   │   ├── SalesPage.jsx           # Update - History and details
│   │   ├── ExpensesPage.jsx        # Update - CRUD operations
│   │   └── SettingsPage.jsx        # Update - Configuration
│   ├── services/
│   │   └── apiService.js           # NEW - API service layer
│   ├── styles/
│   │   ├── global.css              # Existing - Add component styles
│   │   └── variables.css           # Existing
│   ├── utils/
│   │   ├── formatters.js           # NEW - Format functions
│   │   ├── validators.js           # NEW - Validation functions
│   │   └── storage.js              # NEW - Storage utilities
│   ├── App.jsx                     # Existing
│   └── index.js                    # Existing
├── templates/                      # OLD - No longer used by PyWebView
│   ├── login.html                  # Deprecated
│   ├── dashboard.html              # Deprecated
│   └── ...                         # All deprecated
├── main.py                         # UPDATE - Load build/index.html
├── api.py                          # UPDATE - Deprecate get_page_html
└── package.json                    # Existing
```

## Key Changes Summary

### Phase 0: PyWebView Integration
- **main.py**: Change from template rendering to loading `build/index.html`
- **api.py**: Mark `get_page_html()` as deprecated
- **New**: `usePyWebView.js` hook for API bridge communication

### Phase 2: Core Infrastructure
- **New**: `apiService.js` - Centralized API layer
- **New**: `useApiData.js` - Data fetching hooks
- **New**: `formatters.js` - Currency, date, number formatting
- **New**: `validators.js` - Form validation functions
- **New**: `storage.js` - Local storage utilities
- **New**: Button, Input, Card, Table components

### Phase 3: Page Migration
- **LoginPage**: Real authentication integration
- **DashboardPage**: Real stats and data display
- **POSPage**: Full cart and checkout functionality
- **ProductsPage**: CRUD operations for products
- **SalesPage**: Sales history and details
- **ExpensesPage**: Expense tracking
- **SettingsPage**: Configuration management

## Migration Benefits

### Before (Template-Based)
- ❌ Server-side rendering
- ❌ Page reloads on navigation
- ❌ Limited interactivity
- ❌ Manual DOM manipulation
- ❌ ES5 JavaScript restrictions
- ❌ Template duplication

### After (React-Based)
- ✅ Client-side rendering
- ✅ Single Page Application (SPA)
- ✅ Rich interactivity
- ✅ Declarative UI
- ✅ Modern JavaScript (ES2015+)
- ✅ Component reusability
- ✅ Better state management
- ✅ Improved developer experience
- ✅ Hot module replacement in dev
- ✅ Code splitting and lazy loading
