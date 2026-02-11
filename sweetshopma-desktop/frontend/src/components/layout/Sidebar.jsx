import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useSidebar } from '../../context/SidebarContext';
import { useAuth } from '../../context/AuthContext';
import {
    LayoutDashboard,
    ShoppingCart,
    Package,
    Receipt,
    DollarSign,
    Settings,
    TrendingUp,
    ArrowUpCircle,
    Calendar,
    Users,
    Activity
} from 'lucide-react';

/**
 * Sidebar Component
 * Navigation sidebar with menu items - hides for Seller role
 */
function Sidebar() {
    const location = useLocation();
    const { isCollapsed } = useSidebar();
    const { user } = useAuth();
    const { t, i18n } = useTranslation();

    // Check if user is Seller - hide sidebar for Sellers
    const isSeller = user?.role === 'Seller';

    // If seller, don't render sidebar
    if (isSeller) {
        return null;
    }

    const isRtl = i18n.language === 'ar';

    const sidebarStyle = {
        width: isCollapsed ? '72px' : '240px',
        backgroundColor: 'var(--color-surface)',
        borderRight: isRtl ? 'none' : '1px solid var(--color-border)',
        borderLeft: isRtl ? '1px solid var(--color-border)' : 'none',
        padding: '16px 8px',
        overflowY: 'auto',
        transition: 'width var(--transition-normal), left var(--transition-normal), right var(--transition-normal)',
        position: 'fixed',
        top: '60px',
        left: isRtl ? 'auto' : 0,
        right: isRtl ? 0 : 'auto',
        bottom: 0,
        zIndex: 100
    };

    const navListStyle = {
        listStyle: 'none',
        margin: 0,
        padding: 0
    };

    const navItemStyle = {
        marginBottom: '4px'
    };

    const getNavLinkStyle = (isActive) => ({
        display: 'flex',
        alignItems: 'center',
        justifyContent: isCollapsed ? 'center' : 'flex-start',
        gap: '12px',
        padding: '12px',
        borderRadius: 'var(--radius-md)',
        fontSize: '14px',
        fontWeight: '500',
        textDecoration: 'none',
        transition: 'all var(--transition-fast)',
        backgroundColor: isActive ? 'var(--color-primary-50)' : 'transparent',
        color: isActive ? 'var(--color-primary-700)' : 'var(--color-text-secondary)',
        border: 'none',
        cursor: 'pointer'
    });

    const iconStyle = {
        width: '20px',
        height: '20px',
        minWidth: '20px' // Prevent shrinking
    };

    const menuItems = [
        {
            path: '/dashboard',
            icon: LayoutDashboard,
            label: t('dashboard')
        },
        {
            path: '/pos',
            icon: ShoppingCart,
            label: t('pos')
        },
        {
            path: '/products',
            icon: Package,
            label: t('products')
        },
        {
            path: '/restocks',
            icon: ArrowUpCircle,
            label: t('restock_records')
        },
        {
            path: '/sales',
            icon: Receipt,
            label: t('sales')
        },
        {
            path: '/expenses',
            icon: DollarSign,
            label: t('expenses')
        },
        {
            path: '/attendance',
            icon: Calendar,
            label: t('attendance')
        },
        {
            path: '/users',
            icon: Users,
            label: t('users'),
            roles: ['Developer', 'Admin']  // Only show for Developer and Admin roles
        },
        {
            path: '/settings',
            icon: Settings,
            label: t('settings')
        }
    ];

    return (
        <aside style={sidebarStyle}>
            <nav>
                <ul style={navListStyle}>
                    {menuItems.map((item) => {
                        // Filter menu items based on user role
                        if (item.roles && !item.roles.includes(user?.role)) {
                            return null; // Skip this menu item
                        }

                        const Icon = item.icon;
                        const isActive = location.pathname === item.path ||
                            (item.path !== '/dashboard' && location.pathname.startsWith(item.path));

                        return (
                            <li key={item.path} style={navItemStyle}>
                                <NavLink
                                    to={item.path}
                                    style={({ isActive }) => getNavLinkStyle(isActive)}
                                    title={isCollapsed ? item.label : ''}
                                >
                                    <Icon size={20} style={iconStyle} />
                                    {!isCollapsed && <span>{item.label}</span>}
                                </NavLink>
                            </li>
                        );
                    })}
                </ul>
            </nav>
        </aside>
    );
}

export default Sidebar;
