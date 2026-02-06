import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig({
    // Use relative paths for PyWebView compatibility
    base: './',

    plugins: [
        react({
            // Use Babel for JSX transformation
            jsxImportSource: undefined,
            babel: {
                // Use babel.config.js
                babelrc: true,
                configFile: true,
                // Apply Babel to all JS/JSX files
                include: ['**/*.jsx', '**/*.js'],
                // Don't use esbuild for JSX - use Babel instead
                exclude: []
            }
        })
    ],

    // Build configuration for ES2015 output (ES6 compatible with most modern webviews)
    build: {
        target: 'es2015',  // ES2015 is widely supported and esbuild can handle it
        outDir: 'build',
        emptyOutDir: true,
        sourcemap: true,

        // Configure chunk splitting
        rollupOptions: {
            output: {
                // Ensure consistent chunk names
                chunkFileNames: 'assets/[name]-[hash].js',
                entryFileNames: 'assets/[name]-[hash].js',
                assetFileNames: 'assets/[name]-[hash].[ext]'
            }
        },

        // Minification settings
        minify: 'terser',
        terserOptions: {
            compress: {
                drop_console: false, // Keep console for debugging
                ecma: 2015
            },
            format: {
                ecma: 2015
            }
        }
    },

    // Development server configuration
    server: {
        port: 3000,
        strictPort: true,
        // Enable CORS for PyWebView
        cors: true,
        // Proxy API requests to Django backend during development
        proxy: {
            '/api': {
                target: 'http://127.0.0.1:8000',
                changeOrigin: true,
                secure: false
            }
        }
    },

    // Path aliases
    resolve: {
        alias: {
            '@': path.resolve(__dirname, './src'),
            '@components': path.resolve(__dirname, './src/components'),
            '@pages': path.resolve(__dirname, './src/pages'),
            '@hooks': path.resolve(__dirname, './src/hooks'),
            '@utils': path.resolve(__dirname, './src/utils'),
            '@services': path.resolve(__dirname, './src/services'),
            '@context': path.resolve(__dirname, './src/context'),
            '@styles': path.resolve(__dirname, './src/styles')
        }
    },

    // Optimize dependency pre-bundling
    optimizeDeps: {
        include: ['react', 'react-dom', 'react-dom/client', 'react-router-dom']
    },

    // Define global constants
    define: {
        'process.env.NODE_ENV': JSON.stringify(process.env.NODE_ENV || 'production')
    },

    // CSS configuration
    css: {
        modules: {
            localsConvention: 'camelCase'
        }
    }
});
