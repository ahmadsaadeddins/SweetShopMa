import React, { Suspense, lazy } from 'react';
import { HashRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { ThemeProvider } from './context/ThemeContext';
import { SidebarProvider } from './context/SidebarContext';
import { ApiProvider } from './context/ApiContext';
import LoadingSpinner from './components/common/LoadingSpinner';
import ErrorBoundary from './components/common/ErrorBoundary';

// Lazy load pages for better performance
const LoginPage = lazy(() => import('./pages/LoginPage'));
const DashboardPage = lazy(() => import('./pages/DashboardPage'));
const POSPage = lazy(() => import('./pages/POSPage'));
const ProductsPage = lazy(() => import('./pages/ProductsPage'));
const RestockRecordsPage = lazy(() => import('./pages/RestockRecordsPage'));
const SalesPage = lazy(() => import('./pages/SalesPage'));
const ExpensesPage = lazy(() => import('./pages/ExpensesPage'));
const SettingsPage = lazy(() => import('./pages/SettingsPage'));
const AttendancePage = lazy(() => import('./pages/AttendancePage'));
const UsersPage = lazy(() => import('./pages/UsersPage'));

// Layout component
import AppLayout from './components/layout/AppLayout';

// Main App component
function App({ hasPyWebView }) {
    console.log('[App] Rendering with PyWebView:', hasPyWebView);

    // Protected route wrapper - defined inside App to access AuthContext
    function ProtectedRoute({ children }) {
        const { isAuthenticated, isLoading } = useAuth();

        if (isLoading) {
            return <div>Loading...</div>;
        }

        if (!isAuthenticated) {
            return <Navigate to="/login" replace />;
        }

        return children;
    }

    // Role-based route wrapper - checks both authentication and role
    function RoleBasedRoute({ children, allowedRoles }) {
        const { isAuthenticated, isLoading, user } = useAuth();

        if (isLoading) {
            return <div>Loading...</div>;
        }

        if (!isAuthenticated) {
            return <Navigate to="/login" replace />;
        }

        // Check if user's role is in the allowed roles list
        if (allowedRoles && !allowedRoles.includes(user?.role)) {
            return <Navigate to="/pos" replace />;
        }

        return children;
    }

    return (
        <ErrorBoundary>
            <ApiProvider>
                <ThemeProvider>
                    <AuthProvider>
                        <SidebarProvider>
                            <HashRouter>
                                <Suspense fallback={<LoadingSpinner />}>
                                    <Routes>
                                        {/* Public routes */}
                                        <Route path="/login" element={<LoginPage />} />

                                        {/* Protected routes with layout */}
                                        <Route
                                            path="/"
                                            element={
                                                <ProtectedRoute>
                                                    <AppLayout />
                                                </ProtectedRoute>
                                            }
                                        >
                                            <Route index element={<Navigate to="/pos" replace />} />
                                            <Route path="dashboard" element={<DashboardPage />} />
                                            <Route path="pos" element={<POSPage />} />
                                            <Route path="products" element={<ProductsPage />} />
                                            <Route path="restocks" element={<RestockRecordsPage />} />
                                            <Route path="sales" element={<SalesPage />} />
                                            <Route path="expenses" element={<ExpensesPage />} />
                                            <Route path="attendance" element={<AttendancePage />} />
                                            <Route
                                                path="users"
                                                element={
                                                    <RoleBasedRoute allowedRoles={['Developer', 'Admin']}>
                                                        <UsersPage />
                                                    </RoleBasedRoute>
                                                }
                                            />
                                            <Route path="settings" element={<SettingsPage />} />
                                        </Route>

                                        {/* Catch all route - redirect to dashboard */}
                                        <Route path="*" element={<Navigate to="/dashboard" replace />} />
                                    </Routes>
                                </Suspense>
                            </HashRouter>
                        </SidebarProvider>
                    </AuthProvider>
                </ThemeProvider>
            </ApiProvider>
        </ErrorBoundary>
    );
}

export default App;
