'use client'

import { useState, useEffect } from 'react'
import { createPortal } from 'react-dom'

export type ToastType = 'success' | 'error' | 'info' | 'warning'

interface Toast {
  id: string
  message: string
  type: ToastType
}

let toastId = 0
let toasts: Toast[] = []
let listeners: Set<(toasts: Toast[]) => void> = new Set()

function notify() {
  const currentToasts = [...toasts]
  listeners.forEach(listener => {
    try {
      listener(currentToasts)
    } catch (error) {
      console.error('Error notifying toast listener:', error)
    }
  })
}

export function showToast(message: string, type: ToastType = 'info') {
  const id = `toast-${toastId++}`
  const newToast: Toast = { id, message, type }
  
  toasts = [...toasts, newToast]
  notify()
  
  // Auto remove after 5 seconds (longer for errors)
  const duration = type === 'error' ? 5000 : 3000
  setTimeout(() => {
    toasts = toasts.filter(t => t.id !== id)
    notify()
  }, duration)
}

export function useToasts() {
  const [state, setState] = useState<Toast[]>([])
  
  useEffect(() => {
    // Set initial state
    setState([...toasts])
    
    // Create a stable callback
    const updateState = (newToasts: Toast[]) => {
      setState([...newToasts])
    }
    
    // Add listener
    listeners.add(updateState)
    
    // Cleanup
    return () => {
      listeners.delete(updateState)
    }
  }, [])
  
  return state
}

export function ToastContainer() {
  const toasts = useToasts()
  const [mounted, setMounted] = useState(false)
  
  useEffect(() => {
    setMounted(true)
  }, [])
  
  if (toasts.length === 0 || !mounted) {
    return null
  }
  
  // Check if we're on the attendance page and should use custom positioning
  const isAttendancePage = typeof window !== 'undefined' && window.location.pathname.includes('/attendance')
  const customContainer = typeof window !== 'undefined' ? document.getElementById('attendance-toast-container') : null
  
  const toastContent = (
    <div className="space-y-2 pointer-events-none w-full">
      {toasts.map((toast) => {
        const bgColor = {
          success: 'bg-green-500',
          error: 'bg-red-600',
          info: 'bg-blue-500',
          warning: 'bg-yellow-500',
        }[toast.type]
        
        const icon = {
          success: '✓',
          error: '✕',
          info: 'ℹ',
          warning: '⚠',
        }[toast.type]
        
        return (
          <div
            key={toast.id}
            className={`${bgColor} text-white px-6 py-4 rounded-lg shadow-2xl w-full pointer-events-auto flex items-center gap-3 border-2 border-white/20`}
            style={{
              animation: 'slideInDown 0.3s ease-out',
              backgroundColor: toast.type === 'error' ? '#dc2626' : undefined, // Force red for errors
            }}
          >
            <span className="text-xl font-bold flex-shrink-0">{icon}</span>
            <span className="flex-1 font-medium text-sm leading-relaxed">{toast.message}</span>
          </div>
        )
      })}
    </div>
  )
  
  // If on attendance page and container exists, render there using portal
  if (isAttendancePage && customContainer) {
    return createPortal(toastContent, customContainer)
  }
  
  // Default positioning for other pages
  return (
    <div 
      className="fixed top-4 left-1/2 -translate-x-1/2 z-[9999] space-y-2 pointer-events-none" 
      style={{ 
        maxWidth: '90vw', 
        width: 'auto',
        position: 'fixed',
        top: '1rem',
        left: '50%',
        transform: 'translateX(-50%)',
        zIndex: 9999
      }}
    >
      {toasts.map((toast) => {
        const bgColor = {
          success: 'bg-green-500',
          error: 'bg-red-600',
          info: 'bg-blue-500',
          warning: 'bg-yellow-500',
        }[toast.type]
        
        const icon = {
          success: '✓',
          error: '✕',
          info: 'ℹ',
          warning: '⚠',
        }[toast.type]
        
        return (
          <div
            key={toast.id}
            className={`${bgColor} text-white px-6 py-4 rounded-lg shadow-2xl min-w-[300px] max-w-[500px] pointer-events-auto flex items-center gap-3 border-2 border-white/20`}
            style={{
              animation: 'slideInDown 0.3s ease-out',
              marginBottom: '0.5rem',
              backgroundColor: toast.type === 'error' ? '#dc2626' : undefined, // Force red for errors
            }}
          >
            <span className="text-xl font-bold flex-shrink-0">{icon}</span>
            <span className="flex-1 font-medium text-sm leading-relaxed">{toast.message}</span>
          </div>
        )
      })}
    </div>
  )
}

