import axios from 'axios'
import Cookies from 'js-cookie'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000/api'

const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Add token to requests
api.interceptors.request.use((config) => {
  const token = Cookies.get('access_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle token refresh on 401
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    // Only log unexpected errors (not 409 conflicts which are expected)
    if (error.response?.status !== 409) {
      console.log('API Error Interceptor:', {
        status: error.response?.status,
        data: error.response?.data,
        config: error.config?.url
      })
    }
    
    if (error.response?.status === 401) {
      const refreshToken = Cookies.get('refresh_token')
      if (refreshToken) {
        try {
          const response = await axios.post(`${API_URL}/auth/refresh/`, {
            refresh: refreshToken,
          })
          Cookies.set('access_token', response.data.access)
          error.config.headers.Authorization = `Bearer ${response.data.access}`
          return api.request(error.config)
        } catch {
          Cookies.remove('access_token')
          Cookies.remove('refresh_token')
          window.location.href = '/login'
        }
      } else {
        window.location.href = '/login'
      }
    }
    
    // Ensure error response data is preserved
    if (error.response && !error.response.data) {
      error.response.data = { error: 'Unknown error occurred' }
    }
    
    return Promise.reject(error)
  }
)

export default api

