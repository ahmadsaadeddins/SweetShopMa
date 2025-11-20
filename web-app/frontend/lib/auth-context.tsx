'use client'

import React, { createContext, useContext, useState, useEffect } from 'react'
import Cookies from 'js-cookie'
import api from './api'
import { User } from './types'

interface AuthContextType {
  user: User | null
  loading: boolean
  login: (username: string, password: string) => Promise<void>
  logout: () => void
  isAuthenticated: boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    // Check if user is already logged in
    const token = Cookies.get('access_token')
    if (token) {
      fetchUser()
    } else {
      setLoading(false)
    }
  }, [])

  const fetchUser = async () => {
    try {
      const response = await api.get('/auth/login/')
      setUser(response.data.user)
    } catch {
      Cookies.remove('access_token')
      Cookies.remove('refresh_token')
      setUser(null)
    } finally {
      setLoading(false)
    }
  }

  const login = async (username: string, password: string) => {
    const response = await api.post('/auth/login/', { username, password })
    Cookies.set('access_token', response.data.access)
    Cookies.set('refresh_token', response.data.refresh)
    setUser(response.data.user)
  }

  const logout = () => {
    Cookies.remove('access_token')
    Cookies.remove('refresh_token')
    setUser(null)
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        loading,
        login,
        logout,
        isAuthenticated: !!user,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}

