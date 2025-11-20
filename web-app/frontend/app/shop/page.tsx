'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth-context'
import { useLocalization } from '@/lib/localization-context'
import api from '@/lib/api'
import { Product, CartItem, Order } from '@/lib/types'
import { printReceipt } from '@/lib/receipt'
import { showToast } from '@/lib/toast'

export default function ShopPage() {
  const router = useRouter()
  const { user, logout, isAuthenticated } = useAuth()
  const { t } = useLocalization()
  const [products, setProducts] = useState<Product[]>([])
  const [cartItems, setCartItems] = useState<CartItem[]>([])
  const [searchText, setSearchText] = useState('')
  const [selectedProduct, setSelectedProduct] = useState<Product | null>(null)
  const [quantity, setQuantity] = useState('1')
  const [total, setTotal] = useState(0)
  const [loading, setLoading] = useState(true)
  const [addingToCart, setAddingToCart] = useState(false)
  const [checkingOut, setCheckingOut] = useState(false)

  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login')
      return
    }
    loadData()
  }, [isAuthenticated, router])

  useEffect(() => {
    calculateTotal()
  }, [cartItems])

  const loadData = async () => {
    try {
      const [productsRes, cartRes] = await Promise.all([
        api.get('/products/'),
        api.get('/cart/'),
      ])
      setProducts(productsRes.data.results || productsRes.data)
      setCartItems(cartRes.data.results || cartRes.data)
    } catch (err) {
      console.error('Failed to load data:', err)
    } finally {
      setLoading(false)
    }
  }

  const calculateTotal = async () => {
    try {
      const res = await api.get('/cart/total/')
      setTotal(parseFloat(res.data.total))
    } catch {
      setTotal(0)
    }
  }

  const handleSearch = (value: string) => {
    setSearchText(value)
    const found = products.find(
      (p) => p.barcode === value || p.name.toLowerCase().includes(value.toLowerCase())
    )
    if (found) {
      setSelectedProduct(found)
      setQuantity(found.is_sold_by_weight ? '0.5' : '1')
    } else {
      setSelectedProduct(null)
    }
  }

  const addToCart = async () => {
    if (!selectedProduct || addingToCart) return

    setAddingToCart(true)
    try {
      await api.post('/cart/', {
        product_id: selectedProduct.id,
        quantity: parseFloat(quantity),
      })
      showToast(`${selectedProduct.emoji} ${selectedProduct.name} added to cart`, 'success')
      await loadData()
      setSearchText('')
      setSelectedProduct(null)
      setQuantity('1')
    } catch (err: any) {
      showToast(err.response?.data?.error || 'Failed to add to cart', 'error')
    } finally {
      setAddingToCart(false)
    }
  }

  const removeFromCart = async (itemId: number) => {
    try {
      await api.delete(`/cart/${itemId}/`)
      await loadData()
    } catch (err) {
      console.error('Failed to remove item:', err)
    }
  }

  const checkout = async () => {
    if (cartItems.length === 0 || checkingOut) return

    if (!confirm(`${t('ConfirmCheckout')} ${cartItems.length} item(s) for $${total.toFixed(2)}?`)) {
      return
    }

    setCheckingOut(true)
    try {
      const response = await api.post('/orders/checkout/')
      const order: Order = response.data
      
      showToast(`${t('OrderPlaced')} Order #${order.id}`, 'success')
      
      // Show print dialog
      setTimeout(() => {
        const action = confirm(
          `${t('OrderPlaced')}\n\n` +
          `Order #${order.id}\n` +
          `Total: $${parseFloat(order.total).toFixed(2)}\n\n` +
          `Click OK to ${t('PrintReceipt')}, or Cancel to continue.`
        )
        
        if (action) {
          printReceipt(order)
        }
      }, 500)
      
      await loadData()
    } catch (err: any) {
      showToast(err.response?.data?.error || 'Checkout failed', 'error')
    } finally {
      setCheckingOut(false)
    }
  }

  if (loading) {
    return <div className="flex items-center justify-center min-h-screen">Loading...</div>
  }

  const filteredProducts = products.filter(
    (p) =>
      p.name.toLowerCase().includes(searchText.toLowerCase()) ||
      p.barcode.includes(searchText)
  )

  return (
    <div className="min-h-screen bg-gray-50 p-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="bg-primary text-white p-6 rounded-lg mb-4 flex justify-between items-center">
          <div>
            <h1 className="text-3xl font-bold">🍬 {t('Shop')}</h1>
            <p className="text-sm opacity-90">{t('LoggedInAs')}: {user?.name || user?.username}</p>
          </div>
          <div className="flex gap-2">
            {user?.can_manage_stock && (
              <button
                onClick={() => router.push('/admin')}
                className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
              >
                {t('AdminPanel')}
              </button>
            )}
            {user?.can_use_attendance_tracker && (
              <button
                onClick={() => router.push('/attendance')}
                className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
              >
                {t('Attendance')}
              </button>
            )}
            <button
              onClick={logout}
              className="bg-white text-primary px-4 py-2 rounded hover:bg-gray-100"
            >
              {t('Logout')}
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
          {/* Left: Products */}
          <div className="lg:col-span-2 space-y-4">
            {/* Search */}
            <div className="bg-white p-4 rounded-lg shadow">
              <label className="block text-sm font-medium mb-2">{t('BarcodeSearch')}</label>
              <input
                type="text"
                value={searchText}
                onChange={(e) => handleSearch(e.target.value)}
                placeholder={t('ScanBarcode')}
                className="w-full px-3 py-2 border rounded-md"
                autoFocus
              />
            </div>

            {/* Selected Product */}
            {selectedProduct && (
              <div className="bg-white p-4 rounded-lg shadow">
                <div className="flex items-center gap-4 mb-4">
                  <span className="text-4xl">{selectedProduct.emoji}</span>
                  <div>
                    <h3 className="font-bold text-lg">{selectedProduct.name}</h3>
                    <p className="text-gray-600">${selectedProduct.price} / {selectedProduct.unit_label}</p>
                  </div>
                </div>
                <div className="flex gap-2">
                  <input
                    type="number"
                    value={quantity}
                    onChange={(e) => setQuantity(e.target.value)}
                    min="0.001"
                    step="0.001"
                    className="flex-1 px-3 py-2 border rounded-md"
                    placeholder="Quantity"
                  />
                  <button
                    onClick={addToCart}
                    disabled={addingToCart}
                    className="bg-primary text-white px-6 py-2 rounded-md hover:bg-primary/90 disabled:opacity-50"
                  >
                    {addingToCart ? '...' : t('Add')}
                  </button>
                </div>
              </div>
            )}

            {/* Products Grid */}
            <div className="bg-white p-4 rounded-lg shadow">
              <h2 className="font-bold text-lg mb-4">{t('Products')}</h2>
              <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
                {filteredProducts.map((product) => (
                  <div
                    key={product.id}
                    onClick={() => {
                      setSelectedProduct(product)
                      setQuantity(product.is_sold_by_weight ? '0.5' : '1')
                    }}
                    className="p-4 border rounded-lg cursor-pointer hover:bg-gray-50 text-center"
                  >
                    <div className="text-3xl mb-2">{product.emoji}</div>
                    <div className="font-semibold">{product.name}</div>
                    <div className="text-sm text-gray-600">${product.price}</div>
                    <div className="text-xs text-gray-500">{t('Stock')}: {product.stock} {product.unit_label}</div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Right: Cart */}
          <div className="bg-white p-4 rounded-lg shadow">
            <h2 className="font-bold text-lg mb-4">{t('Cart')}</h2>
            <div className="space-y-2 mb-4 max-h-96 overflow-y-auto">
              {cartItems.map((item) => (
                <div key={item.id} className="flex justify-between items-center p-2 border rounded">
                  <div>
                    <div className="font-semibold">{item.product.emoji} {item.product.name}</div>
                    <div className="text-sm text-gray-600">
                      {item.quantity} {item.product.unit_label} × ${item.product.price}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <span className="font-bold">${parseFloat(item.item_total).toFixed(2)}</span>
                    <button
                      onClick={() => removeFromCart(item.id)}
                      className="text-red-500 hover:text-red-700"
                    >
                      ×
                    </button>
                  </div>
                </div>
              ))}
              {cartItems.length === 0 && (
                <div className="text-center text-gray-500 py-8">{t('CartEmpty')}</div>
              )}
            </div>
            <div className="border-t pt-4">
              <div className="flex justify-between font-bold text-lg mb-4">
                <span>{t('Total')}:</span>
                <span>${total.toFixed(2)}</span>
              </div>
              <button
                onClick={checkout}
                disabled={cartItems.length === 0 || checkingOut}
                className="w-full bg-primary text-white py-2 px-4 rounded-md hover:bg-primary/90 disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {checkingOut ? 'Processing...' : t('Checkout')}
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

