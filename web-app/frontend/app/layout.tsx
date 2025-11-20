import type { Metadata } from 'next'
import { Inter } from 'next/font/google'
import './globals.css'
import { AuthProvider } from '@/lib/auth-context'
import { LocalizationProvider } from '@/lib/localization-context'
import { ToastContainer } from '@/lib/toast'

const inter = Inter({ subsets: ['latin', 'arabic'] })

export const metadata: Metadata = {
  title: 'SweetShopMa - Point of Sale',
  description: 'Point of Sale system for sweet shops',
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <LocalizationProvider>
          <AuthProvider>
            {children}
            <ToastContainer />
          </AuthProvider>
        </LocalizationProvider>
      </body>
    </html>
  )
}

