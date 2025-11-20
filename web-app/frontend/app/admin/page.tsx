'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth-context'
import { useLocalization } from '@/lib/localization-context'
import api from '@/lib/api'
import { User, Product, Reports } from '@/lib/types'
import { showToast } from '@/lib/toast'

export default function AdminPage() {
  const router = useRouter()
  const { user, logout } = useAuth()
  const { t } = useLocalization()
  const [users, setUsers] = useState<User[]>([])
  const [products, setProducts] = useState<Product[]>([])
  const [reports, setReports] = useState<Reports | null>(null)
  const [activeTab, setActiveTab] = useState<'users' | 'products' | 'reports'>('reports')
  const [restockingProduct, setRestockingProduct] = useState<number | null>(null)
  const [restockQuantity, setRestockQuantity] = useState('')
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    if (!user || !user.can_manage_stock) {
      router.push('/shop')
      return
    }
    loadData()
  }, [user, router])

  const loadData = async () => {
    setLoading(true)
    try {
      if (user?.can_manage_users) {
        const usersRes = await api.get('/users/')
        setUsers(usersRes.data.results || usersRes.data)
      }
      const [productsRes, reportsRes] = await Promise.all([
        api.get('/products/'),
        api.get('/reports/'),
      ])
      setProducts(productsRes.data.results || productsRes.data)
      setReports(reportsRes.data)
    } catch (err) {
      console.error('Failed to load data:', err)
      showToast('Failed to load data', 'error')
    } finally {
      setLoading(false)
    }
  }

  const handleRestock = async (productId: number) => {
    const quantity = parseFloat(restockQuantity)
    if (!quantity || quantity <= 0) {
      showToast('Please enter a valid quantity', 'error')
      return
    }

    setRestockingProduct(productId)
    try {
      const response = await api.post(`/products/${productId}/restock/`, {
        quantity: quantity.toString(),
      })
      showToast(`Restocked ${quantity} units successfully`, 'success')
      setRestockQuantity('')
      setRestockingProduct(null)
      await loadData()
    } catch (err: any) {
      showToast(err.response?.data?.error || 'Failed to restock', 'error')
    } finally {
      setRestockingProduct(null)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-primary text-white p-6 rounded-lg mb-4 flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold">{t('AdminPanel')}</h1>
            <p className="text-sm opacity-90">{user?.name || user?.username}</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => router.push('/shop')}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('BackToShop')}
            </button>
            <button
              onClick={logout}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('Logout')}
            </button>
          </div>
        </div>

        {/* Tabs */}
        <div className="bg-white rounded-lg shadow mb-4">
          <div className="flex border-b">
            <button
              onClick={() => setActiveTab('reports')}
              className={`px-6 py-3 ${activeTab === 'reports' ? 'border-b-2 border-primary text-primary' : 'text-gray-600'}`}
            >
              {t('ReportsInsights')}
            </button>
            {user?.can_manage_users && (
              <button
                onClick={() => setActiveTab('users')}
                className={`px-6 py-3 ${activeTab === 'users' ? 'border-b-2 border-primary text-primary' : 'text-gray-600'}`}
              >
                {t('Users')}
              </button>
            )}
            <button
              onClick={() => setActiveTab('products')}
              className={`px-6 py-3 ${activeTab === 'products' ? 'border-b-2 border-primary text-primary' : 'text-gray-600'}`}
            >
              {t('Products')}
            </button>
            <button
              onClick={() => router.push('/restock-report')}
              className="px-6 py-3 text-gray-600 hover:text-primary"
            >
              {t('RestockReport')}
            </button>
          </div>
        </div>

        {/* Content */}
        <div className="bg-white rounded-lg shadow p-6">
          {loading && (
            <div className="text-center py-12">{t('Loading')}</div>
          )}

          {!loading && activeTab === 'reports' && reports && (
            <div className="space-y-6">
              <h2 className="text-2xl font-bold mb-4">{t('ReportsInsights')}</h2>
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                <div className="p-4 bg-blue-50 rounded-lg">
                  <div className="text-sm text-gray-600">{t('TotalSales')}</div>
                  <div className="text-2xl font-bold">${parseFloat(reports.total_sales).toFixed(2)}</div>
                </div>
                <div className="p-4 bg-green-50 rounded-lg">
                  <div className="text-sm text-gray-600">{t('TotalOrders')}</div>
                  <div className="text-2xl font-bold">{reports.total_orders}</div>
                </div>
                <div className="p-4 bg-yellow-50 rounded-lg">
                  <div className="text-sm text-gray-600">{t('AverageOrderValue')}</div>
                  <div className="text-2xl font-bold">${parseFloat(reports.average_order_value).toFixed(2)}</div>
                </div>
                <div className="p-4 bg-purple-50 rounded-lg">
                  <div className="text-sm text-gray-600">{t('Last7Days')}</div>
                  <div className="text-2xl font-bold">${parseFloat(reports.last_7_days_sales).toFixed(2)}</div>
                </div>
              </div>

              <div>
                <h3 className="text-xl font-bold mb-4">{t('TopProducts')}</h3>
                <div className="space-y-2">
                  {reports.top_products.map((product, idx) => (
                    <div key={idx} className="flex justify-between items-center p-3 bg-gray-50 rounded">
                      <div className="flex items-center gap-2">
                        <span className="text-2xl">{product.product_emoji}</span>
                        <span className="font-semibold">{product.product_name}</span>
                      </div>
                      <span className="font-bold">{parseFloat(product.total_sold).toFixed(2)} sold</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          )}

          {!loading && activeTab === 'users' && user?.can_manage_users && (
            <div>
              <h2 className="text-2xl font-bold mb-4">{t('UserManagement')}</h2>
              <div className="space-y-2">
                {users.map((u) => (
                  <div key={u.id} className="flex justify-between items-center p-3 border rounded">
                    <div>
                      <div className="font-semibold">{u.name} ({u.username})</div>
                      <div className="text-sm text-gray-600">{u.role}</div>
                    </div>
                    <div className="flex items-center gap-2">
                      <span className={`px-2 py-1 rounded text-sm ${u.is_enabled ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                        {u.is_enabled ? t('Enabled') : t('Disabled')}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          {!loading && activeTab === 'products' && (
            <div>
              <h2 className="text-2xl font-bold mb-4">{t('Products')}</h2>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {products.map((product) => (
                  <div key={product.id} className="p-4 border rounded-lg">
                    <div className="text-center mb-3">
                      <div className="text-4xl mb-2">{product.emoji}</div>
                      <div className="font-semibold text-lg">{product.name}</div>
                      <div className="text-sm text-gray-600">${product.price} / {product.unit_label}</div>
                      <div className="text-sm font-medium mt-1">
                        {t('Stock')}: {product.stock} {product.unit_label}
                      </div>
                    </div>
                    {restockingProduct === product.id ? (
                      <div className="space-y-2">
                        <input
                          type="number"
                          value={restockQuantity}
                          onChange={(e) => setRestockQuantity(e.target.value)}
                          placeholder={t('QuantityAdded')}
                          min="0.001"
                          step="0.001"
                          className="w-full px-3 py-2 border rounded-md"
                          autoFocus
                        />
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleRestock(product.id)}
                            className="flex-1 bg-green-500 text-white px-3 py-2 rounded-md hover:bg-green-600"
                          >
                            {t('Save')}
                          </button>
                          <button
                            onClick={() => {
                              setRestockingProduct(null)
                              setRestockQuantity('')
                            }}
                            className="flex-1 bg-gray-300 text-gray-700 px-3 py-2 rounded-md hover:bg-gray-400"
                          >
                            {t('Cancel')}
                          </button>
                        </div>
                      </div>
                    ) : (
                      <button
                        onClick={() => {
                          setRestockingProduct(product.id)
                          setRestockQuantity('')
                        }}
                        className="w-full bg-primary text-white px-3 py-2 rounded-md hover:bg-primary/90"
                      >
                        {t('RestockProduct')}
                      </button>
                    )}
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}

