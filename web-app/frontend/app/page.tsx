'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useAuth } from '@/lib/auth-context'

export default function Home() {
  const router = useRouter()
  const { isAuthenticated, loading } = useAuth()

  useEffect(() => {
    if (!loading) {
      if (isAuthenticated) {
        router.push('/shop')
      } else {
        router.push('/login')
      }
    }
  }, [isAuthenticated, loading, router])

  return (
    <div className="flex items-center justify-center min-h-screen">
      <div className="text-center">
        <div className="text-4xl mb-4">🍬</div>
        <div className="text-xl text-gray-600">Loading...</div>
      </div>
    </div>
  )
}

