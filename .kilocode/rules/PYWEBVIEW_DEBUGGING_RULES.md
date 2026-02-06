# PyWebView Desktop App Debugging Rules

## Critical Issues & Solutions

### 1. JavaScript ES6 Compatibility ❌

**Issue:** PyWebView's embedded webview only supports ES5 JavaScript syntax.

**Symptoms:**
- Scripts not executing at all
- Syntax errors in console
- Silent failures

**What Doesn't Work:**
```javascript
// ❌ ES6 - NOT SUPPORTED
const apiKey = 'xxx';
let data = [];
() => {};  // Arrow functions
`${variable}`;  // Template literals
...spread;  // Spread operator
```

**What Works:**
```javascript
// ✅ ES5 - REQUIRED
var apiKey = 'xxx';
var data = [];
function() {};  // Function declarations
variable + 'string';  // String concatenation
```

**Rule:** Always use ES5 syntax in PyWebView frontend templates. No `const`, `let`, arrow functions, or template literals.

---

### 2. Script Execution with innerHTML ❌

**Issue:** Browsers do NOT execute `<script>` tags when content is injected via `innerHTML`.

**Symptoms:**
- Page loads but scripts don't run
- No console errors
- Functions undefined

**Wrong Way:**
```javascript
// ❌ Scripts won't execute
contentDiv.innerHTML = responseHTML;
```

**Right Way:**
```javascript
// ✅ Manually extract and execute scripts
window.loadPage = function(pageName) {
    // Load HTML
    var tempDiv = document.createElement('div');
    tempDiv.innerHTML = htmlContent;
    
    // Extract scripts
    var scripts = tempDiv.getElementsByTagName('script');
    var scriptContents = [];
    for (var i = 0; i < scripts.length; i++) {
        scriptContents.push(scripts[i].textContent);
    }
    
    // Inject HTML
    contentDiv.innerHTML = htmlContent;
    
    // Execute each script
    for (var i = 0; i < scriptContents.length; i++) {
        var script = document.createElement('script');
        script.textContent = scriptContents[i];
        document.head.appendChild(script);
    }
    
    // Trigger page initialization
    if (typeof window.initPage === 'function') {
        window.initPage();
    }
};
```

**Rule:** Never rely on `window 'load'` event when using `innerHTML`. Create explicit initialization functions.

---

### 3. CORS Configuration Required ❌

**Issue:** PyWebView frontend cannot communicate with Django backend without CORS enabled.

**Symptoms:**
- "Failed to fetch" errors
- Network errors in console
- Empty responses

**Solution:**

**Install:**
```bash
pip install django-cors-headers
```

**Configure [`settings.py`](sweetshopma-desktop/backend/sweetshop/settings.py):**
```python
INSTALLED_APPS = [
    # ...
    'corsheaders',
]

MIDDLEWARE = [
    'corsheaders.middleware.CorsMiddleware',  # MUST be first
    'django.middleware.common.CommonMiddleware',
    # ...
]

CORS_ALLOW_ALL_ORIGINS = True  # For development
```

**Rule:** Always enable CORS when using PyWebView frontend with Django backend.

---

### 4. API Response Format Handling ⚠️

**Issue:** DRF ViewSets can return either paginated responses (`{results: []}`) or direct arrays (`[]`).

**Symptoms:**
- "No recent sales found" when data exists
- Debug shows data but UI shows nothing

**Wrong Way:**
```javascript
// ❌ Only handles paginated format
if (recentSales && recentSales.results && recentSales.results.length > 0) {
    // Process recentSales.results
}
```

**Right Way:**
```javascript
// ✅ Handles both formats
var salesArray = null;
if (Array.isArray(recentSales)) {
    salesArray = recentSales;  // Direct array
} else if (recentSales && recentSales.results && Array.isArray(recentSales.results)) {
    salesArray = recentSales.results;  // Paginated
} else {
    // No data
    return;
}

// Process salesArray
for (var i = 0; i < salesArray.length; i++) {
    var sale = salesArray[i];
    // ...
}
```

**Rule:** Always check for both `Array.isArray(response)` AND `response.results` when handling DRF API responses.

---

### 5. DRF Serializer Field Declaration ❌

**Issue:** If a serializer declares a field but doesn't include it in `Meta.fields`, Django throws AssertionError.

**Symptoms:**
- 500 Internal Server Error
- "Got AttributeError when accessing <field>"
- AssertionError in logs

**Wrong Way:**
```python
# ❌ Field declared but not in fields list
class SaleSerializer(serializers.ModelSerializer):
    items = SaleItemSerializer(many=True, read_only=True)
    
    class Meta:
        model = Sale
        fields = ['id', 'total', 'status']  # Missing 'items'!
```

**Right Way:**
```python
# ✅ All declared fields included
class SaleSerializer(serializers.ModelSerializer):
    items = SaleItemSerializer(many=True, read_only=True)
    
    class Meta:
        model = Sale
        fields = ['id', 'total', 'status', 'items']  # Includes 'items'
```

**Rule:** Every field declared in a serializer MUST be listed in `Meta.fields`.

---

### 6. Invalid Settings References ❌

**Issue:** Django fails to start if `REST_FRAMEWORK` settings reference non-existent modules.

**Symptoms:**
- Django won't start
- ModuleNotFoundError or ImportError
- Server crashes on startup

**Wrong Way:**
```python
# ❌ References non-existent module
REST_FRAMEWORK = {
    'EXCEPTION_HANDLER': 'api.exceptions.custom_exception_handler',  # Doesn't exist!
}
```

**Right Way:**
```python
# ✅ Only reference existing modules
REST_FRAMEWORK = {
    'DEFAULT_PAGINATION_CLASS': 'rest_framework.pagination.PageNumberPagination',
    'PAGE_SIZE': 100,
}
```

**Rule:** Never reference modules in settings.py that don't exist. Always verify the module path is correct.

---

## Debugging Best Practices

### 1. Add Visual Debug Console
```html
<div id="debugPanel" style="background: #1e1e1e; color: #00ff00; padding: 15px;">
    <div id="debugOutput"></div>
</div>

<script>
function debugLog(message, data) {
    var timestamp = new Date().toLocaleTimeString();
    var debugOutput = document.getElementById('debugOutput');
    if (debugOutput) {
        var logEntry = '<div>[' + timestamp + '] ' + message;
        if (data) {
            logEntry += '<div style="color:#ffff00">' + JSON.stringify(data) + '</div>';
        }
        logEntry += '</div>';
        debugOutput.innerHTML += logEntry;
        debugOutput.scrollTop = debugOutput.scrollHeight;
    }
}
</script>
```

### 2. Verify DOM Elements Exist
```javascript
var requiredElements = ['todaySales', 'recentSalesTable', 'recentSalesBody'];
for (var i = 0; i < requiredElements.length; i++) {
    var element = document.getElementById(requiredElements[i]);
    if (!element) {
        debugLog('❌ Missing element: ' + requiredElements[i]);
        return;
    }
    debugLog('✓ Found: ' + requiredElements[i]);
}
```

### 3. Log API Responses
```javascript
apiCall('GET', 'sales/recent/').then(function(response) {
    debugLog('📊 API Response:', response);
    
    // Validate response structure
    if (!response) {
        debugLog('❌ Response is null');
        return;
    }
    
    // Continue processing...
});
```

### 4. Use PyWebView API Bridge Instead of fetch()
```javascript
// ❌ fetch() may be blocked in PyWebView
fetch('/api/dashboard/').then(...)

// ✅ Use PyWebView's exposed methods
pywebview.api.get_dashboard_stats().then(...)
```

---

## Quick Checklist for New Features

- [ ] Using ES5 syntax (var, not const/let)
- [ ] Scripts execute after innerHTML injection
- [ ] CORS enabled in Django settings
- [ ] API responses handle both array and paginated formats
- [ ] Serializers include all declared fields
- [ ] Settings references exist
- [ ] Debug console added for troubleshooting
- [ ] DOM elements verified before manipulation
- [ ] API responses logged for debugging

---

## Related Files

- Frontend templates: [`sweetshopma-desktop/frontend/templates/`](sweetshopma-desktop/frontend/templates/)
- Django settings: [`sweetshopma-desktop/backend/sweetshop/settings.py`](sweetshopma-desktop/backend/sweetshop/settings.py)
- API serializers: [`sweetshopma-desktop/backend/api/serializers.py`](sweetshopma-desktop/backend/api/serializers.py)
- API bridge: [`sweetshopma-desktop/frontend/api.py`](sweetshopma-desktop/frontend/api.py)
