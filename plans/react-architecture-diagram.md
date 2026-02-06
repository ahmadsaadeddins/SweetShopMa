# React Architecture Diagram for SweetShopMa

## System Architecture Overview

```mermaid
graph TB
    subgraph "Desktop Environment"
        PyWebView[PyWebView Window<br/>ES5 JavaScript Engine]
    end
    
    subgraph "React Application"
        App[App.jsx<br/>Root Component]
        
        subgraph "Routing Layer"
            Router[React Router v6<br/>Client-side Routing]
        end
        
        subgraph "State Management"
            AuthContext[AuthContext<br/>Authentication State]
            ThemeContext[ThemeContext<br/>UI Preferences]
        end
        
        subgraph "Pages"
            Login[LoginPage]
            Dashboard[DashboardPage]
            POS[POSPage]
            Products[ProductsPage]
            Sales[SalesPage]
            Expenses[ExpensesPage]
            Settings[SettingsPage]
        end
        
        subgraph "Shared Components"
            Common[Common Components<br/>Button, Input, Card, Modal]
            Layout[Layout Components<br/>Header, Navigation, Sidebar]
            Features[Feature Components<br/>Cart, ProductList, SaleTable]
        end
        
        subgraph "Services Layer"
            API[useApi Hook<br/>API Communication]
            Auth[useAuth Hook<br/>Authentication]
            Sync[useSync Hook<br/>Sync Status]
        end
        
        subgraph "Utilities"
            Formatters[Currency, Date Formatting]
            Validators[Form Validation]
            Session[Custom Session Storage]
        end
    end
    
    subgraph "Backend"
        Django[Django REST Framework<br/>Port 8000]
        APIBridge[ApiBridge<br/>Python → JS Bridge]
    end
    
    PyWebView --> App
    App --> Router
    Router --> Login
    Router --> Dashboard
    Router --> POS
    Router --> Products
    Router --> Sales
    Router --> Expenses
    Router --> Settings
    
    AuthContext -.-> Login
    AuthContext -.-> Dashboard
    AuthContext -.-> POS
    AuthContext -.-> Products
    AuthContext -.-> Sales
    AuthContext -.-> Expenses
    AuthContext -.-> Settings
    
    Login --> API
    Dashboard --> API
    POS --> API
    Products --> API
    Sales --> API
    Expenses --> API
    Settings --> API
    
    Dashboard --> Common
    POS --> Common
    Products --> Common
    Sales --> Common
    Expenses --> Common
    Settings --> Common
    
    Dashboard --> Layout
    POS --> Layout
    Products --> Layout
    Sales --> Layout
    Expenses --> Layout
    Settings --> Layout
    
    POS --> Features
    Products --> Features
    Sales --> Features
    
    API --> APIBridge
    APIBridge --> Django
    
    API --> Formatters
    API --> Validators
    API --> Session
    
    style PyWebView fill:#e1f5fe
    style App fill:#c8e6c9
    style Router fill:#fff9c4
    style AuthContext fill:#ffccbc
    style APIBridge fill:#f3e5f5
    style Django fill:#e8f5e9
```

## Data Flow Diagram

```mermaid
sequenceDiagram
    participant User as User
    participant React as React Component
    participant Hook as useApi Hook
    participant Bridge as ApiBridge
    participant Django as Django API
    
    User->>React: User Action (Click, Submit)
    React->>Hook: Call API Method
    Hook->>Bridge: pywebview.api.method()
    Bridge->>Django: HTTP Request
    Django-->>Bridge: JSON Response
    Bridge-->>Hook: Return Data
    Hook-->>React: Update State
    React->>User: Re-render UI
```

## Build Pipeline

```mermaid
graph LR
    subgraph "Development"
        Dev[React/JSX Source<br/>src/]
    end
    
    subgraph "Build Process"
        Vite[Vite Bundler]
        Babel[Babel Transpiler]
        Polyfills[Core-js Polyfills]
    end
    
    subgraph "Production"
        Build[Build Output<br/>build/]
        ES5[ES5 JavaScript<br/>ES5 Compatible]
    end
    
    subgraph "Runtime"
        PyWebView[PyWebView Window]
    end
    
    Dev --> Vite
    Vite --> Babel
    Babel --> Polyfills
    Polyfills --> ES5
    ES5 --> Build
    Build --> PyWebView
    
    style Dev fill:#e3f2fd
    style Vite fill:#fff3e0
    style Babel fill:#fce4ec
    style ES5 fill:#f1f8e9
    style PyWebView fill:#e1f5fe
```

## Component Hierarchy

```mermaid
graph TB
    App[App.jsx]
    
    App --> Router[BrowserRouter]
    Router --> Routes[Routes]
    
    Routes --> LoginRoute[Route /login]
    Routes --> ProtectedRoute[Protected Routes]
    
    ProtectedRoute --> Layout[AppLayout]
    
    Layout --> Header[Header]
    Layout --> Sidebar[Sidebar]
    Layout --> Main[Main Content]
    
    Main --> DashboardRoute[Route /dashboard]
    Main --> POSRoute[Route /pos]
    Main --> ProductsRoute[Route /products]
    Main --> SalesRoute[Route /sales]
    Main --> ExpensesRoute[Route /expenses]
    Main --> SettingsRoute[Route /settings]
    
    DashboardRoute --> Dashboard[DashboardPage]
    POSRoute --> POS[POSPage]
    ProductsRoute --> Products[ProductsPage]
    SalesRoute --> Sales[SalesPage]
    ExpensesRoute --> Expenses[ExpensesPage]
    SettingsRoute --> Settings[SettingsPage]
    
    Dashboard --> StatCards[StatCards]
    Dashboard --> RecentSales[RecentSalesTable]
    Dashboard --> SyncStatus[SyncStatusCard]
    
    POS --> ProductSearch[ProductSearch]
    POS --> Cart[ShoppingCart]
    POS --> PaymentForm[PaymentForm]
    
    Products --> ProductList[ProductList]
    Products --> ProductForm[ProductForm]
    Products --> CategoryFilter[CategoryFilter]
    
    Sales --> SalesList[SalesList]
    Sales --> SaleDetails[SaleDetails]
    Sales --> DateFilter[DateRangeFilter]
    
    style App fill:#c8e6c9
    style Router fill:#fff9c4
    style Layout fill:#e1bee7
    style Dashboard fill:#bbdefb
    style POS fill:#ffccbc
    style Products fill:#c5cae9
    style Sales fill:#dcedc8
    style Expenses fill:#ffecb3
    style Settings fill:#b2dfdb
```

## State Management Flow

```mermaid
graph LR
    subgraph "Auth Context"
        AuthProvider[AuthProvider]
        AuthState[Auth State]
        Login[login function]
        Logout[logout function]
    end
    
    subgraph "Components"
        LoginPage[LoginPage]
        ProtectedPage[ProtectedPage]
        Header[Header Component]
    end
    
    AuthProvider --> AuthState
    AuthProvider --> Login
    AuthProvider --> Logout
    
    LoginPage --> Login
    ProtectedPage --> AuthState
    Header --> AuthState
    Header --> Logout
    
    style AuthProvider fill:#ffccbc
    style AuthState fill:#ffe0b2
    style Login fill:#ffe0b2
    style Logout fill:#ffe0b2
```

## API Communication Pattern

```mermaid
graph TB
    subgraph "React Component"
        Comp[Page Component]
    end
    
    subgraph "Custom Hook"
        Hook[useApi Hook]
        State[useState]
        Effect[useEffect]
    end
    
    subgraph "PyWebView Bridge"
        API[pywebview.api]
    end
    
    subgraph "Django Backend"
        Endpoint[API Endpoint]
        DB[Database]
    end
    
    Comp --> Hook
    Hook --> State
    Hook --> Effect
    Effect --> API
    API --> Endpoint
    Endpoint --> DB
    DB --> Endpoint
    Endpoint --> API
    API --> Hook
    Hook --> Comp
    
    style Comp fill:#e3f2fd
    style Hook fill:#fff3e0
    style API fill:#f3e5f5
    style Endpoint fill:#e8f5e9
```

## File Structure

```
sweetshopma-desktop/frontend/
│
├── src/
│   ├── App.jsx                    # Root component with routing
│   ├── index.js                   # Entry point
│   ├── index.html                 # HTML template
│   │
│   ├── components/
│   │   ├── common/                # Reusable UI components
│   │   │   ├── Button.jsx
│   │   │   ├── Input.jsx
│   │   │   ├── Select.jsx
│   │   │   ├── Card.jsx
│   │   │   ├── Modal.jsx
│   │   │   ├── Alert.jsx
│   │   │   ├── Table.jsx
│   │   │   ├── Badge.jsx
│   │   │   └── Spinner.jsx
│   │   │
│   │   ├── layout/                # Layout components
│   │   │   ├── AppLayout.jsx
│   │   │   ├── Header.jsx
│   │   │   ├── Sidebar.jsx
│   │   │   └── Navigation.jsx
│   │   │
│   │   └── features/              # Feature-specific components
│   │       ├── cart/
│   │       │   ├── Cart.jsx
│   │       │   ├── CartItem.jsx
│   │       │   └── CartSummary.jsx
│   │       ├── products/
│   │       │   ├── ProductList.jsx
│   │       │   ├── ProductCard.jsx
│   │       │   └── ProductForm.jsx
│   │       └── sales/
│   │           ├── SaleTable.jsx
│   │           ├── SaleRow.jsx
│   │           └── SaleFilters.jsx
│   │
│   ├── pages/                     # Page components
│   │   ├── LoginPage.jsx
│   │   ├── DashboardPage.jsx
│   │   ├── POSPage.jsx
│   │   ├── ProductsPage.jsx
│   │   ├── SalesPage.jsx
│   │   ├── ExpensesPage.jsx
│   │   └── SettingsPage.jsx
│   │
│   ├── hooks/                     # Custom React hooks
│   │   ├── useApi.js              # API communication
│   │   ├── useAuth.js             # Authentication
│   │   ├── useSync.js             # Sync status
│   │   └── useProducts.js         # Product data
│   │
│   ├── services/                  # Business logic
│   │   └── apiService.js          # API wrapper
│   │
│   ├── context/                   # React Context
│   │   ├── AuthContext.jsx        # Auth state
│   │   └── ThemeContext.jsx       # UI theme
│   │
│   ├── utils/                     # Helper functions
│   │   ├── formatters.js          # Currency, date
│   │   ├── validators.js          # Form validation
│   │   └── sessionStorage.js      # Session management
│   │
│   └── styles/                    # Styles
│       ├── global.css             # Global styles
│       ├── variables.css          # CSS variables
│       └── components/            # Component styles
│
├── public/                        # Static assets
│   ├── images/
│   └── fonts/
│
├── build/                         # Transpiled output (generated)
│   ├── assets/
│   └── index.html
│
├── templates/                     # Legacy templates (to remove)
│   ├── base.html
│   ├── login.html
│   ├── dashboard.html
│   └── ...
│
├── api.py                         # ApiBridge (keep)
├── main.py                        # PyWebView entry (modify)
├── utils.py                       # Template utils (remove later)
│
├── package.json                   # Node.js dependencies
├── vite.config.js                 # Vite configuration
├── babel.config.js                # Babel configuration
├── .eslintrc.js                   # ESLint configuration
└── README.md                      # Frontend documentation
```

## Key Design Decisions

### 1. **Why Babel Transpilation?**
- PyWebView only supports ES5 JavaScript
- Modern React requires ES6+ features
- Babel bridges this gap by transpiling to ES5

### 2. **Why Vite over Webpack?**
- Faster development server with HMR
- Simpler configuration
- Better build performance
- Native ES module support

### 3. **Why React Context over Redux?**
- Simpler for this application's needs
- Less boilerplate
- Built into React (no extra dependencies)
- Sufficient for auth and theme state

### 4. **Why Custom useApi Hook?**
- Wraps `pywebview.api` for consistent interface
- Centralizes error handling
- Manages loading states automatically
- Works with both paginated and direct array responses

### 5. **Why React Router v6?**
- Latest version with better performance
- Nested routes support
- Better TypeScript support (future-proofing)
- Simpler API than v5

---

**Document Version:** 1.0  
**Last Updated:** 2025-02-03
