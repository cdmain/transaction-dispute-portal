/**
 * HTTP Client Configuration
 * 
 * Centralized Axios instance with interceptors for:
 * - Authentication token injection
 * - Response error handling
 * - Automatic logout on 401
 */
import axios, { type AxiosInstance, type AxiosError, type AxiosRequestConfig } from 'axios'
import { getAuthToken, clearAuthStorage } from '@/utils/authStorage'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

/**
 * Create configured Axios instance
 */
const createHttpClient = (): AxiosInstance => {
  const client = axios.create({
    baseURL: API_BASE_URL,
    headers: {
      'Content-Type': 'application/json',
    },
    timeout: 30000, // 30 second timeout
  })

  // Request interceptor - add auth token
  client.interceptors.request.use(
    (config) => {
      const token = getAuthToken()
      if (token && config.headers) {
        config.headers.Authorization = `Bearer ${token}`
      }
      return config
    },
    (error) => Promise.reject(error)
  )

  // Response interceptor - handle errors
  client.interceptors.response.use(
    (response) => response,
    (error: AxiosError) => {
      if (error.response?.status === 401) {
        clearAuthStorage()
        if (window.location.pathname !== '/login') {
          window.location.href = '/login'
        }
      }
      return Promise.reject(error)
    }
  )

  return client
}

export const httpClient = createHttpClient()

/**
 * Type-safe request helpers
 */
export async function get<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
  const response = await httpClient.get<T>(url, config)
  return response.data
}

export async function post<T, D = unknown>(url: string, data?: D, config?: AxiosRequestConfig): Promise<T> {
  const response = await httpClient.post<T>(url, data, config)
  return response.data
}

export async function put<T, D = unknown>(url: string, data?: D, config?: AxiosRequestConfig): Promise<T> {
  const response = await httpClient.put<T>(url, data, config)
  return response.data
}

export async function del<T>(url: string, config?: AxiosRequestConfig): Promise<T> {
  const response = await httpClient.delete<T>(url, config)
  return response.data
}
