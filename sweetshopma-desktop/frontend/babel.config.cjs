module.exports = {
    presets: [
        // Transpile modern JavaScript to ES2015
        [
            '@babel/preset-env',
            {
                // Target ES2015 for compatibility with modern webviews
                targets: {
                    // Chrome 51+, Edge 15+, Safari 10+, Firefox 54+
                    // This covers most modern PyWebView webview versions
                    browsers: ['Chrome >= 51', 'Edge >= 15', 'Safari >= 10', 'Firefox >= 54']
                },
                // Automatically include polyfills based on usage
                useBuiltIns: 'usage',
                // Use core-js version 3 for polyfills
                corejs: 3,
                // Allow transformation of modules to CommonJS (handled by Vite)
                modules: false
            }
        ],
        // Transform JSX to React.createElement calls
        [
            '@babel/preset-react',
            {
                // Use automatic JSX runtime (no need to import React)
                runtime: 'automatic',
                // Enable development mode in development
                development: process.env.NODE_ENV === 'development',
                // Preserve line numbers for better debugging
                sourceType: 'unambiguous'
            }
        ]
    ],
    plugins: [
        // Enable re-use of Babel helper functions to reduce code size
        '@babel/plugin-transform-runtime'
    ],
    // Don't transpile node_modules
    ignore: [
        'node_modules/**',
        'build/**',
        'dist/**'
    ],
    // Source maps for debugging
    sourceMaps: true,
    // Retain lines for better error messages
    retainLines: false
};
