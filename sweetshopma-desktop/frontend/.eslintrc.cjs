module.exports = {
    root: true,
    env: {
        browser: true,
        es2021: true,
        node: true
    },
    extends: [
        'eslint:recommended',
        'plugin:react/recommended',
        'plugin:react-hooks/recommended'
    ],
    parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module',
        ecmaFeatures: {
            jsx: true
        }
    },
    plugins: [
        'react',
        'react-hooks'
    ],
    settings: {
        react: {
            version: 'detect'
        }
    },
    rules: {
        // React specific rules
        'react/react-in-jsx-scope': 'off', // Not needed with automatic JSX runtime
        'react/prop-types': 'off', // Using TypeScript or no prop-types validation
        'react-hooks/rules-of-hooks': 'error',
        'react-hooks/exhaustive-deps': 'warn',

        // General code quality
        'no-console': ['warn', { allow: ['warn', 'error'] }],
        'no-debugger': 'warn',
        'no-unused-vars': ['warn', { argsIgnorePattern: '^_' }],

        // ES5 compatibility warnings
        'no-var': 'off', // Allow var for ES5 compatibility
        'prefer-const': 'warn', // Warn but allow var for ES5
        'prefer-arrow-callback': 'off', // Allow function callbacks for ES5
        'object-shorthand': 'off' // Allow full property syntax for ES5
    }
};
