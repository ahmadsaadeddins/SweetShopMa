import React from 'react';
import { NavLink, useLocation } from 'react-router-dom';
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

    // Check if user is Seller - hide sidebar for Sellers
    const isSeller = user?.role === 'Seller';

    // DEBUG logging
    console.log('[Sidebar] user:', user);
    console.log('[Sidebar] user?.role:', user?.role);
    console.log('[Sidebar] isSeller:', isSeller);

    // If seller, don't render sidebar
    if (isSeller) {
        return null;
    }

    const sidebarStyle = {
        width: isCollapsed ? '72px' : '240px',
        backgroundColor: 'var(--color-surface)',
        borderRight: '1px solid var(--color-border)',
        padding: '16px 8px',
        overflowY: 'auto',
        transition: 'width var(--transition-normal)',
        position: 'fixed',
        top: '60px',
        left: 0,
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
            label: 'Dashboard'
        },
        {
            path: '/pos',
            icon: ShoppingCart,
            label: 'Point of Sale'
        },
        {
            path: '/products',
            icon: Package,
            label: 'Products'
        },
        {
            path: '/restocks',
            icon: ArrowUpCircle,
            label: 'Restock Records'
        },
        {
            path: '/sales',
            icon: Receipt,
            label: 'Sales'
        },
        {
            path: '/expenses',
            icon: DollarSign,
            label: 'Expenses'
        },
        {
            path: '/attendance',
            icon: Calendar,
            label: 'Attendance'
        },
        {
            path: '/users',
            icon: Users,
            label: 'Users',
            roles: ['Developer', 'Admin']  // Only show for Developer and Admin roles
        },
        {
            path: '/settings',
            icon: Settings,
            label: 'Settings'
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
