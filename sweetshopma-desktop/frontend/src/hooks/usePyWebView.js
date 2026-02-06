/**
 * usePyWebView.js - Legacy Hook (Backward Compatibility)
 * 
 * This file is kept for backward compatibility.
 * The actual implementation is now in ApiContext.jsx (singleton pattern).
 * 
 * All imports now use the shared ApiContext via hooks/index.js
 */

export { usePyWebView } from '../context/ApiContext';
