const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5050/api'

interface RequestConfig {
  params?: Record<string, unknown>
  headers?: Record<string, string>
}

function getAuthHeaders(): Record<string, string> {
  const token = localStorage.getItem('auth_token')
  return token ? { Authorization: `Bearer ${token}` } : {}
}

function buildUrl(endpoint: string, params?: Record<string, unknown>): string {
  const url = new URL(`${API_URL}${endpoint}`)
  if (params) {
    Object.entries(params).forEach(([key, value]) => {
      if (value !== undefined && value !== null) {
        url.searchParams.append(key, String(value))
      }
    })
  }
  return url.toString()
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (response.status === 401) {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('refresh_token')
    window.location.href = '/login'
    throw new Error('Unauthorized')
  }
  
  if (!response.ok) {
    const error = await response.json().catch(() => ({ message: 'Request failed' }))
    throw new Error(error.message || `HTTP ${response.status}`)
  }
  
  if (response.status === 204) return undefined as T
  return response.json()
}

export async function get<T>(endpoint: string, config?: RequestConfig): Promise<T> {
  const response = await fetch(buildUrl(endpoint, config?.params), {
    method: 'GET',
    headers: { 'Content-Type': 'application/json', ...getAuthHeaders(), ...config?.headers },
  })
  return handleResponse<T>(response)
}

export async function post<T>(endpoint: string, data?: unknown, config?: RequestConfig): Promise<T> {
  const response = await fetch(buildUrl(endpoint, config?.params), {
    method: 'POST',
    headers: { 'Content-Type': 'application/json', ...getAuthHeaders(), ...config?.headers },
    body: data ? JSON.stringify(data) : undefined,
  })
  return handleResponse<T>(response)
}

export async function put<T>(endpoint: string, data?: unknown, config?: RequestConfig): Promise<T> {
  const response = await fetch(buildUrl(endpoint, config?.params), {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json', ...getAuthHeaders(), ...config?.headers },
    body: data ? JSON.stringify(data) : undefined,
  })
  return handleResponse<T>(response)
}

export async function del<T>(endpoint: string, config?: RequestConfig): Promise<T> {
  const response = await fetch(buildUrl(endpoint, config?.params), {
    method: 'DELETE',
    headers: { 'Content-Type': 'application/json', ...getAuthHeaders(), ...config?.headers },
  })
  return handleResponse<T>(response)
}
