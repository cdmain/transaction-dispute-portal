import axios, { AxiosError } from 'axios'
import type { 
  Transaction, 
  Dispute, 
  PagedResult, 
  TransactionQueryParams, 
  DisputeQueryParams,
  CreateDisputeRequest,
  DisputeStatistics,
  LoginRequest,
  RegisterRequest,
  AuthResponse
} from '@/types'
import { DisputeStatus } from '@/types'
import { getAuthToken, clearAuthStorage } from '@/utils/authStorage'
import { 
  isDemoMode, 
  mockAuthApi, 
  mockTransactionApi, 
  mockDisputeApi 
} from './mockApi'

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '/api'

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

// Add auth token to requests
api.interceptors.request.use((config) => {
  const token = getAuthToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Handle 401 responses
api.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      // Clear stored auth on 401
      clearAuthStorage()
      
      // Redirect to login if not already there
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

// Auth API
export const authApi = {
  login: async (data: LoginRequest): Promise<AuthResponse> => {
    if (isDemoMode()) {
      return mockAuthApi.login(data.email, data.password)
    }
    const response = await api.post('/auth/login', data)
    return response.data
  },

  register: async (data: RegisterRequest): Promise<AuthResponse> => {
    if (isDemoMode()) {
      return mockAuthApi.register(data.email, data.password, data.firstName, data.lastName)
    }
    const response = await api.post('/auth/register', data)
    return response.data
  },

  refreshToken: async (refreshToken: string): Promise<AuthResponse> => {
    if (isDemoMode()) {
      return mockAuthApi.refreshToken()
    }
    const response = await api.post('/auth/refresh', { refreshToken })
    return response.data
  },

  logout: async (refreshToken: string): Promise<void> => {
    if (isDemoMode()) {
      return mockAuthApi.logout()
    }
    await api.post('/auth/logout', { refreshToken })
  }
}

// Transaction API
export const transactionApi = {
  getTransactions: async (params?: TransactionQueryParams): Promise<PagedResult<Transaction>> => {
    if (isDemoMode()) {
      return mockTransactionApi.getTransactions(params)
    }
    const response = await api.get('/transactions', { params })
    return response.data
  },

  getTransactionById: async (id: string): Promise<Transaction> => {
    if (isDemoMode()) {
      return mockTransactionApi.getTransactionById(id)
    }
    const response = await api.get(`/transactions/${id}`)
    return response.data
  },

  getTransactionsByCustomer: async (customerId: string): Promise<Transaction[]> => {
    if (isDemoMode()) {
      const result = await mockTransactionApi.getTransactions({ pageSize: 100 })
      return result.items
    }
    const response = await api.get(`/transactions/customer/${customerId}`)
    return response.data
  },

  getCategories: async (): Promise<string[]> => {
    if (isDemoMode()) {
      return mockTransactionApi.getCategories()
    }
    const response = await api.get('/transactions/categories')
    return response.data
  }
}

// Dispute API
export const disputeApi = {
  getDisputes: async (params?: DisputeQueryParams): Promise<PagedResult<Dispute>> => {
    if (isDemoMode()) {
      return mockDisputeApi.getDisputes(params)
    }
    const response = await api.get('/disputes', { params })
    return response.data
  },

  getDisputeById: async (id: string): Promise<Dispute> => {
    if (isDemoMode()) {
      return mockDisputeApi.getDisputeById(id)
    }
    const response = await api.get(`/disputes/${id}`)
    return response.data
  },

  getDisputesByCustomer: async (customerId: string): Promise<Dispute[]> => {
    if (isDemoMode()) {
      const result = await mockDisputeApi.getDisputes({ pageSize: 100 })
      return result.items
    }
    const response = await api.get(`/disputes/customer/${customerId}`)
    return response.data
  },

  getDisputesByTransaction: async (transactionId: string): Promise<Dispute[]> => {
    if (isDemoMode()) {
      const result = await mockDisputeApi.getDisputes({ pageSize: 100 })
      return result.items.filter(d => d.transactionId === transactionId)
    }
    const response = await api.get(`/disputes/transaction/${transactionId}`)
    return response.data
  },

  createDispute: async (data: CreateDisputeRequest): Promise<Dispute> => {
    if (isDemoMode()) {
      return mockDisputeApi.createDispute(data)
    }
    const response = await api.post('/disputes', data)
    return response.data
  },

  updateDisputeStatus: async (id: string, status: number, resolutionNotes?: string): Promise<Dispute> => {
    if (isDemoMode()) {
      // Map number to DisputeStatus enum
      return mockDisputeApi.updateDisputeStatus(id, status as DisputeStatus, resolutionNotes)
    }
    const response = await api.put(`/disputes/${id}/status`, { status, resolutionNotes })
    return response.data
  },

  cancelDispute: async (id: string): Promise<void> => {
    if (isDemoMode()) {
      return mockDisputeApi.cancelDispute(id)
    }
    await api.post(`/disputes/${id}/cancel`)
  },

  getStatistics: async (customerId?: string): Promise<DisputeStatistics> => {
    if (isDemoMode()) {
      return mockDisputeApi.getStatistics()
    }
    const response = await api.get('/disputes/statistics', { params: { customerId } })
    return response.data
  }
}
