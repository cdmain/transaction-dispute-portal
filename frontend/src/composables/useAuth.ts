import { ref, computed } from 'vue'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import type { AuthResponse, LoginRequest, RegisterRequest, User } from '@/types'
import { authApi } from '@/services/api'
import { 
  TOKEN_KEY, 
  REFRESH_TOKEN_KEY, 
  USER_KEY, 
  getAuthToken, 
  clearAuthStorage 
} from '@/utils/authStorage'

// Re-export getAuthToken for convenience
export { getAuthToken }

// Reactive auth state
const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
const refreshToken = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))
const user = ref<User | null>(
  localStorage.getItem(USER_KEY) ? JSON.parse(localStorage.getItem(USER_KEY)!) : null
)

export function useAuth() {
  const queryClient = useQueryClient()
  
  const isAuthenticated = computed(() => !!token.value && !!user.value)
  const currentUser = computed(() => user.value)
  const customerId = computed(() => user.value?.customerId ?? null)

  const setAuthData = (response: AuthResponse) => {
    token.value = response.token
    refreshToken.value = response.refreshToken
    user.value = response.user
    
    localStorage.setItem(TOKEN_KEY, response.token)
    localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken)
    localStorage.setItem(USER_KEY, JSON.stringify(response.user))
  }

  const clearAuthData = () => {
    token.value = null
    refreshToken.value = null
    user.value = null
    
    clearAuthStorage()
    
    // Clear all queries
    queryClient.clear()
  }

  return {
    token,
    refreshToken,
    user: currentUser,
    customerId,
    isAuthenticated,
    setAuthData,
    clearAuthData,
  }
}

export function useLogin() {
  const { setAuthData } = useAuth()
  
  return useMutation({
    mutationFn: (data: LoginRequest) => authApi.login(data),
    onSuccess: (response) => {
      setAuthData(response)
    },
  })
}

export function useRegister() {
  const { setAuthData } = useAuth()
  
  return useMutation({
    mutationFn: (data: RegisterRequest) => authApi.register(data),
    onSuccess: (response) => {
      setAuthData(response)
    },
  })
}

export function useLogout() {
  const { refreshToken: storedRefreshToken, clearAuthData } = useAuth()
  
  return useMutation({
    mutationFn: async () => {
      if (storedRefreshToken.value) {
        await authApi.logout(storedRefreshToken.value)
      }
    },
    onSettled: () => {
      clearAuthData()
    },
  })
}

export function useRefreshToken() {
  const { refreshToken: storedRefreshToken, setAuthData, clearAuthData } = useAuth()
  
  return useMutation({
    mutationFn: async () => {
      if (!storedRefreshToken.value) {
        throw new Error('No refresh token')
      }
      return authApi.refreshToken(storedRefreshToken.value)
    },
    onSuccess: (response) => {
      setAuthData(response)
    },
    onError: () => {
      clearAuthData()
    },
  })
}
