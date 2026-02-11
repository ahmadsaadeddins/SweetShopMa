import React, { useState } from 'react';
import { Card, Button, Input, Checkbox, Alert } from '../components/common';
import { useSyncStatus } from '../hooks';
import { useApi } from '../services';
import { getStorageItem, setStorageItem, STORAGE_KEYS } from '../utils';
import { useTranslation } from 'react-i18next';

/**
 * SettingsPage Component
 * Application settings and configuration
 */
function SettingsPage() {
    var { t } = useTranslation();
    const [activeTab, setActiveTab] = useState('general');
    const [saveMessage, setSaveMessage] = useState(null);

    // General settings
    const [shopName, setShopName] = useState(getStorageItem(STORAGE_KEYS.SHOP_NAME, 'Sweet Shop'));
    const [currency, setCurrency] = useState(getStorageItem(STORAGE_KEYS.CURRENCY, 'EGP'));
    const [taxRate, setTaxRate] = useState(getStorageItem(STORAGE_KEYS.TAX_RATE, '0'));

    // Display settings
    const [theme, setTheme] = useState(getStorageItem(STORAGE_KEYS.THEME, 'light'));
    const [lowStockAlerts, setLowStockAlerts] = useState(getStorageItem(STORAGE_KEYS.LOW_STOCK_ALERTS, true));

    // Sync settings
    const { data: syncStatus, refetch: refetchSync } = useSyncStatus();
    const { syncNow } = useApi();

    const handleSaveGeneral = () => {
        setStorageItem(STORAGE_KEYS.SHOP_NAME, shopName);
        setStorageItem(STORAGE_KEYS.CURRENCY, currency);
        setStorageItem(STORAGE_KEYS.TAX_RATE, taxRate);
        setSaveMessage({ type: 'success', text: t('success_general_settings') });
        setTimeout(() => setSaveMessage(null), 3000);
    };

    const handleSaveDisplay = () => {
        setStorageItem(STORAGE_KEYS.THEME, theme);
        setStorageItem(STORAGE_KEYS.LOW_STOCK_ALERTS, lowStockAlerts);
        setSaveMessage({ type: 'success', text: t('success_display_settings') });
        setTimeout(() => setSaveMessage(null), 3000);
    };

    const handleSyncNow = async () => {
        try {
            const result = await syncNow();
            if (result.success) {
                setSaveMessage({ type: 'success', text: t('success_sync') });
                refetchSync();
            } else {
                setSaveMessage({ type: 'danger', text: result.message || t('error_sync') });
            }
            setTimeout(() => setSaveMessage(null), 3000);
        } catch (error) {
            setSaveMessage({ type: 'danger', text: `${t('error_sync')}: ${error.message}` });
        }
    };

    const tabs = [
        { id: 'general', label: t('general_settings'), icon: '⚙️' },
        { id: 'display', label: t('display_settings'), icon: '🎨' },
        { id: 'sync', label: t('sync_settings'), icon: '🔄' },
        { id: 'about', label: t('about_tab'), icon: 'ℹ️' },
    ];

    return (
        <div className="p-4">
            {/* Header */}
            <div className="mb-4">
                <h1 className="h3 mb-1">{t('settings')}</h1>
                <p className="text-muted mb-0">{t('configure_app_prefs')}</p>
            </div>

            {/* Save Message */}
            {saveMessage && (
                <Alert
                    variant={saveMessage.type}
                    message={saveMessage.text}
                    dismissible
                    onDismiss={() => setSaveMessage(null)}
                />
            )}

            {/* Tabs */}
            <div className="mb-4">
                <ul className="nav nav-tabs">
                    {tabs.map(tab => (
                        <li className="nav-item" key={tab.id}>
                            <button
                                className={`nav-link ${activeTab === tab.id ? 'active' : ''}`}
                                onClick={() => setActiveTab(tab.id)}
                            >
                                {tab.icon} {tab.label}
                            </button>
                        </li>
                    ))}
                </ul>
            </div>

            {/* Tab Content */}
            <div className="tab-content">
                {/* General Settings */}
                {activeTab === 'general' && (
                    <Card title={t('general_settings')}>
                        <div className="row g-3">
                            <div className="col-12 col-md-6">
                                <Input
                                    label={t('shop_name')}
                                    value={shopName}
                                    onChange={(e) => setShopName(e.target.value)}
                                    placeholder={t('enter_shop_name')}
                                />
                            </div>
                            <div className="col-12 col-md-6">
                                <Input
                                    label={t('currency')}
                                    value={currency}
                                    onChange={(e) => setCurrency(e.target.value)}
                                    placeholder="e.g., EGP, USD"
                                />
                            </div>
                            <div className="col-12 col-md-6">
                                <Input
                                    label={t('tax_rate')}
                                    type="number"
                                    value={taxRate}
                                    onChange={(e) => setTaxRate(e.target.value)}
                                    placeholder="0"
                                    min="0"
                                    max="100"
                                    step="0.01"
                                />
                            </div>
                        </div>
                        <div className="mt-4">
                            <Button variant="primary" onClick={handleSaveGeneral}>
                                {t('save')}
                            </Button>
                        </div>
                    </Card>
                )}

                {/* Display Settings */}
                {activeTab === 'display' && (
                    <Card title={t('display_settings')}>
                        <div className="row g-3">
                            <div className="col-12 col-md-6">
                                <label className="form-label">{t('theme')}</label>
                                <select
                                    className="form-select"
                                    value={theme}
                                    onChange={(e) => setTheme(e.target.value)}
                                >
                                    <option value="light">{t('light')}</option>
                                    <option value="dark">{t('dark')}</option>
                                    <option value="auto">{t('auto_system')}</option>
                                </select>
                            </div>
                            <div className="col-12">
                                <Checkbox
                                    label={t('enable_low_stock_alerts')}
                                    checked={lowStockAlerts}
                                    onChange={(e) => setLowStockAlerts(e.target.checked)}
                                    helpText={t('low_stock_help_text')}
                                />
                            </div>
                        </div>
                        <div className="mt-4">
                            <Button variant="primary" onClick={handleSaveDisplay}>
                                {t('save')}
                            </Button>
                        </div>
                    </Card>
                )}

                {/* Sync Settings */}
                {activeTab === 'sync' && (
                    <>
                        <Card title={t('sync_status')} className="mb-4">
                            {syncStatus ? (
                                <div className="row g-3">
                                    <div className="col-12 col-md-6">
                                        <strong>{t('status')}:</strong>{' '}
                                        <span className={`badge bg-${syncStatus.is_online ? 'success' : 'danger'}`}>
                                            {syncStatus.is_online ? t('online') : t('offline')}
                                        </span>
                                    </div>
                                    <div className="col-12 col-md-6">
                                        <strong>{t('syncing')}:</strong>{' '}
                                        {syncStatus.is_syncing ? (
                                            <span className="badge bg-warning">{t('in_progress')}</span>
                                        ) : (
                                            <span className="badge bg-success">{t('idle')}</span>
                                        )}
                                    </div>
                                    <div className="col-12 col-md-6">
                                        <strong>{t('last_sync')}:</strong>{' '}
                                        {syncStatus.last_successful_sync
                                            ? new Date(syncStatus.last_successful_sync).toLocaleString()
                                            : t('never')}
                                    </div>
                                    <div className="col-12 col-md-6">
                                        <strong>{t('sync_count')}:</strong> {syncStatus.sync_count || 0}
                                    </div>
                                    {syncStatus.last_error && (
                                        <div className="col-12">
                                            <strong>{t('last_error')}:</strong>{' '}
                                            <span className="text-danger">{syncStatus.last_error}</span>
                                        </div>
                                    )}
                                </div>
                            ) : (
                                <p className="text-muted mb-0">{t('sync_service_unavailable')}</p>
                            )}
                            <div className="mt-4">
                                <Button
                                    variant="primary"
                                    onClick={handleSyncNow}
                                    disabled={syncStatus?.is_syncing}
                                >
                                    {syncStatus?.is_syncing ? t('syncing') : t('sync_now')}
                                </Button>
                            </div>
                        </Card>

                        <Card title={t('sync_config')}>
                            <div className="row g-3">
                                <div className="col-12">
                                    <Checkbox
                                        label={t('enable_auto_sync')}
                                        checked={syncStatus?.sync_interval > 0}
                                        helpText={t('auto_sync_help')}
                                        disabled
                                    />
                                </div>
                                <div className="col-12 col-md-6">
                                    <Input
                                        label={t('sync_interval')}
                                        type="number"
                                        value={syncStatus?.sync_interval ? Math.round(syncStatus.sync_interval / 60) : 5}
                                        disabled
                                        helpText={t('sync_interval_help')}
                                    />
                                </div>
                            </div>
                        </Card>
                    </>
                )}

                {/* About */}
                {activeTab === 'about' && (
                    <Card title={t('about_sweetshopma')}>
                        <div className="text-center mb-4">
                            <h2 className="h3">SweetShopMa Desktop</h2>
                            <p className="text-muted">{t('version')} 1.0.0</p>
                        </div>
                        <hr />
                        <div className="row g-3">
                            <div className="col-12">
                                <h5 className="mb-3">{t('app_info')}</h5>
                                <dl className="row">
                                    <dt className="col-sm-4">{t('version')}:</dt>
                                    <dd className="col-sm-8">1.0.0</dd>

                                    <dt className="col-sm-4">{t('build')}:</dt>
                                    <dd className="col-sm-8">2025.02.03</dd>

                                    <dt className="col-sm-4">Platform:</dt>
                                    <dd className="col-sm-8">
                                        {navigator.platform || t('unknown')}
                                    </dd>

                                    <dt className="col-sm-4">{t('user_agent')}:</dt>
                                    <dd className="col-sm-8">
                                        <small className="text-muted">
                                            {navigator.userAgent}
                                        </small>
                                    </dd>
                                </dl>
                            </div>
                            <div className="col-12">
                                <h5 className="mb-3">{t('technologies')}</h5>
                                <ul className="list-unstyled">
                                    <li>⚛️ React 18.2.0</li>
                                    <li>🎨 React Router DOM 6.20.0</li>
                                    <li>⚡ Vite 5.0.0</li>
                                    <li>🐍 Django REST Framework</li>
                                    <li>🪟 PyWebView</li>
                                </ul>
                            </div>
                            <div className="col-12">
                                <h5 className="mb-3">{t('license')}</h5>
                                <p className="text-muted mb-0">
                                    Copyright © 2025 SweetShopMa. {t('copyright')}
                                </p>
                            </div>
                        </div>
                    </Card>
                )}
            </div>
        </div>
    );
}

export default SettingsPage;
