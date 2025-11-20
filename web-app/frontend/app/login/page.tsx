'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth-context'
import { useLocalization } from '@/lib/localization-context'

export default function LoginPage() {
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [loading, setLoading] = useState(false)
  const router = useRouter()
  const { login } = useAuth()
  const { language, setLanguage, t } = useLocalization()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    setLoading(true)

    try {
      await login(username, password)
      router.push('/shop')
    } catch (err: any) {
      setError(err.response?.data?.error || 'Login failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 px-4">
      <div className="max-w-md w-full space-y-8 bg-white p-8 rounded-lg shadow-md">
        <div className="text-center">
          <div className="text-4xl mb-2">🍬</div>
          <h2 className="text-3xl font-bold text-primary">Sweet Shop</h2>
          <p className="mt-2 text-gray-600">Secure Login</p>
        </div>

        <div className="flex justify-end gap-2 mb-4">
          <button
            onClick={() => setLanguage('en')}
            className={`px-3 py-1 rounded text-sm ${language === 'en' ? 'bg-primary text-white' : 'bg-gray-200'}`}
          >
            EN
          </button>
          <button
            onClick={() => setLanguage('ar')}
            className={`px-3 py-1 rounded text-sm ${language === 'ar' ? 'bg-primary text-white' : 'bg-gray-200'}`}
          >
            AR
          </button>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              👤 {t('Username')}
            </label>
            <input
              type="text"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary"
              placeholder="Enter username"
              autoFocus
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              🔒 {t('Password')}
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-primary"
              placeholder="Enter password"
            />
          </div>

          {error && (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={loading}
            className="w-full bg-primary text-white py-2 px-4 rounded-md hover:bg-primary/90 disabled:opacity-50 font-semibold"
          >
            {loading ? 'Logging in...' : '🔓 ' + t('Login')}
          </button>
        </form>

        <div className="text-center text-sm text-gray-500 mt-4">
          <p>Demo Credentials:</p>
          <p>ama / AsrAma12@#</p>
        </div>
      </div>
    </div>
  )
}

