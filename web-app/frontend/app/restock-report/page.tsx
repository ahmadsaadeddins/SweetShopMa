'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth-context'
import { useLocalization } from '@/lib/localization-context'
import api from '@/lib/api'
import { RestockRecord } from '@/lib/types'

export default function RestockReportPage() {
  const router = useRouter()
  const { user, logout } = useAuth()
  const { t } = useLocalization()
  const [records, setRecords] = useState<RestockRecord[]>([])
  const [loading, setLoading] = useState(true)
  const [filterProduct, setFilterProduct] = useState<string>('')
  const [filterUser, setFilterUser] = useState<string>('')

  useEffect(() => {
    if (!user || !user.can_manage_stock) {
      router.push('/shop')
      return
    }
    loadRecords()
  }, [user, router])

  const loadRecords = async () => {
    try {
      const response = await api.get('/restock-records/')
      setRecords(response.data.results || response.data)
    } catch (err) {
      console.error('Failed to load records:', err)
    } finally {
      setLoading(false)
    }
  }

  const filteredRecords = records.filter(record => {
    const matchesProduct = !filterProduct || 
      record.product_name.toLowerCase().includes(filterProduct.toLowerCase())
    const matchesUser = !filterUser || 
      record.user_name.toLowerCase().includes(filterUser.toLowerCase())
    return matchesProduct && matchesUser
  })

  if (loading) {
    return <div className="flex items-center justify-center min-h-screen">Loading...</div>
  }

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-primary text-white p-6 rounded-lg mb-4 flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold">{t('RestockReport')}</h1>
            <p className="text-sm opacity-90">{user?.name || user?.username}</p>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => router.push('/admin')}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('BackToAdmin')}
            </button>
            <button
              onClick={logout}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('Logout')}
            </button>
          </div>
        </div>

        {/* Filters */}
        <div className="bg-white p-4 rounded-lg shadow mb-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('FilterByProduct')}</label>
              <input
                type="text"
                value={filterProduct}
                onChange={(e) => setFilterProduct(e.target.value)}
                placeholder={t('Search')}
                className="w-full px-3 py-2 border rounded-md"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('FilterByUser')}</label>
              <input
                type="text"
                value={filterUser}
                onChange={(e) => setFilterUser(e.target.value)}
                placeholder={t('Search')}
                className="w-full px-3 py-2 border rounded-md"
              />
            </div>
          </div>
        </div>

        {/* Records Table */}
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('Date')}</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('Products')}</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('QuantityAdded')}</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('StockBefore')}</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('StockAfter')}</th>
                  <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">{t('RestockedBy')}</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-200">
                {filteredRecords.map((record) => (
                  <tr key={record.id} className="hover:bg-gray-50">
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      {new Date(record.restock_date).toLocaleString()}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <div className="flex items-center gap-2">
                        <span className="text-xl">{record.product_emoji}</span>
                        <span className="text-sm font-medium">{record.product_name}</span>
                      </div>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-semibold text-green-600">
                      +{parseFloat(record.quantity_added).toFixed(3)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                      {parseFloat(record.stock_before).toFixed(3)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm font-medium">
                      {parseFloat(record.stock_after).toFixed(3)}
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm">
                      {record.user_name}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {filteredRecords.length === 0 && (
              <div className="text-center py-12 text-gray-500">
                {t('NoRecords')}
              </div>
            )}
          </div>
        </div>

        {/* Summary */}
        {filteredRecords.length > 0 && (
          <div className="mt-4 bg-white p-4 rounded-lg shadow">
            <div className="text-sm text-gray-600">
              Showing {filteredRecords.length} of {records.length} records
            </div>
          </div>
        )}
      </div>
    </div>
  )
}

