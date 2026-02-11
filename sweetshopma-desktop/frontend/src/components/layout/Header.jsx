import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import { useTheme } from '../../context/ThemeContext';
import { useSidebar } from '../../context/SidebarContext';
import { Menu, Sun, Moon, LogOut, User, Settings, Languages } from 'lucide-react';
import LicenseCountdown from '../common/LicenseCountdown';

/**
 * Header Component
 * Top navigation bar with user info, theme toggle, and logout
 * Simplified for Seller role - hides menu, theme, and settings
 */
function Header() {
    const { user, logout } = useAuth();
    const { theme, toggleTheme } = useTheme();
    const { toggleSidebar } = useSidebar();
    const navigate = useNavigate();
    const { t, i18n } = useTranslation();

    const toggleLanguage = () => {
        const newLang = i18n.language === 'ar' ? 'en' : 'ar';
        i18n.changeLanguage(newLang);
        document.dir = newLang === 'ar' ? 'rtl' : 'ltr';
        document.documentElement.lang = newLang;
        localStorage.setItem('i18nextLng', newLang);
    };

    // Check if user is Seller
    const isSeller = user?.role === 'Seller';

    const headerStyle = {
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: isSeller ? '12px 24px' : '16px 24px',
        backgroundColor: 'var(--color-surface)',
        borderBottom: '1px solid var(--color-border)',
        boxShadow: 'var(--shadow-sm)',
        zIndex: 'var(--z-index-sticky)'
    };

    const logoStyle = {
        display: 'flex',
        alignItems: 'center',
        gap: '12px',
        cursor: isSeller ? 'pointer' : 'pointer'
    };

    const logoTextStyle = {
        fontSize: '20px',
        fontWeight: '700',
        color: 'var(--color-primary-600)'
    };

    const actionsStyle = {
        display: 'flex',
        alignItems: 'center',
        gap: '16px'
    };

    const buttonStyle = {
        display: 'flex',
        alignItems: 'center',
        gap: '8px',
        padding: '8px 16px',
        borderRadius: 'var(--radius-md)',
        backgroundColor: 'transparent',
        color: 'var(--color-text-secondary)',
        fontSize: '14px',
        fontWeight: '500',
        cursor: 'pointer',
        transition: 'all var(--transition-fast)',
        border: '1px solid transparent'
    };

    const userInfoStyle = {
        display: 'flex',
        alignItems: 'center',
        gap: '12px',
        padding: '8px 16px',
        borderRadius: 'var(--radius-md)',
        backgroundColor: 'var(--color-gray-100)'
    };

    const avatarStyle = {
        width: '32px',
        height: '32px',
        borderRadius: '50%',
        backgroundColor: 'var(--color-primary-600)',
        color: 'white',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        fontSize: '14px',
        fontWeight: '600'
    };

    const handleLogout = async () => {
        try {
            await logout();
            navigate('/login');
        } catch (error) {
            console.error('[Header] Logout error:', error);
        }
    };

    const getUserInitials = () => {
        if (!user) return 'U';
        const first = user.first_name || user.username || '';
        const last = user.last_name || '';
        return (first[0] + (last[0] || '')).toUpperCase();
    };

    const getDisplayName = () => {
        if (!user) return t('user');
        if (user.first_name && user.last_name) {
            return `${user.first_name} ${user.last_name}`;
        }
        return user.username || t('user');
    };

    return (
        <header style={headerStyle}>
            {/* Logo / Menu Toggle */}
            <div style={logoStyle} onClick={isSeller ? () => navigate('/pos') : toggleSidebar}>
                {isSeller ? (
                    // For Sellers: Show simplified logo
                    <h1 style={logoTextStyle}>SweetShopMa POS</h1>
                ) : (
                    // For other users: Show menu toggle with logo
                    <>
                        <Menu size={24} color="var(--color-text-secondary)" />
                        <h1 style={logoTextStyle}>SweetShopMa</h1>
                    </>
                )}
            </div>

            {/* Actions */}
            <div style={actionsStyle}>

                {/* License Countdown */}
                <LicenseCountdown />

                {/* Language Switcher */}
                <button
                    onClick={toggleLanguage}
                    style={{
                        ...buttonStyle,
                        backgroundColor: 'var(--color-gray-100)',
                        border: '1px solid var(--color-gray-200)'
                    }}
                    title={t('language')}
                >
                    <Languages size={18} />
                    <span>{i18n.language === 'ar' ? 'English' : 'العربية'}</span>
                </button>

                {/* Theme Toggle - Hide for Sellers */}
                {!isSeller && (
                    <button
                        onClick={toggleTheme}
                        style={buttonStyle}
                        title={theme === 'light' ? t('switch_dark_mode') : t('switch_light_mode')}
                    >
                        {theme === 'light' ? <Moon size={18} /> : <Sun size={18} />}
                    </button>
                )}

                {/* Settings - Hide for Sellers */}
                {!isSeller && (
                    <button
                        onClick={() => navigate('/settings')}
                        style={buttonStyle}
                        title={t('settings')}
                    >
                        <Settings size={18} />
                    </button>
                )}

                {/* User Info */}
                <div style={userInfoStyle}>
                    <div style={avatarStyle}>
                        {getUserInitials()}
                    </div>
                    {!isSeller && (
                        <div>
                            <div style={{ fontSize: '14px', fontWeight: '500', color: 'var(--color-text-primary)' }}>
                                {getDisplayName()}
                            </div>
                            <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>
                                {user?.role || t('user')}
                            </div>
                        </div>
                    )}
                </div>

                {/* Logout */}
                <button
                    onClick={handleLogout}
                    style={{
                        ...buttonStyle,
                        color: 'var(--color-error-600)'
                    }}
                    title={t('logout')}
                >
                    <LogOut size={18} />
                </button>
            </div>
        </header>
    );
}

export default Header;
