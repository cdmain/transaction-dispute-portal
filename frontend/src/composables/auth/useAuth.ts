import { ref, computed } from 'vue'
import { useMutation, useQueryClient } from '@tanstack/vue-query'
import * as api from '@/features/auth/api/authApi'
import type { AuthResponse, User } from '@/features/auth/api/authApi'
import { TOKEN_KEY, REFRESH_TOKEN_KEY, USER_KEY, getAuthToken, clearAuthStorage } from '@/utils/authStorage'

export { getAuthToken }

const token = ref<string | null>(localStorage.getItem(TOKEN_KEY))
const refreshToken = ref<string | null>(localStorage.getItem(REFRESH_TOKEN_KEY))
const user = ref<User | null>(
  localStorage.getItem(USER_KEY) ? JSON.parse(localStorage.getItem(USER_KEY) as string) : null
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
    queryClient.clear()
  }

  return { token, refreshToken, user: currentUser, customerId, isAuthenticated, setAuthData, clearAuthData }
}

export function useLogin() {
  const { setAuthData } = useAuth()
  return useMutation({ mutationFn: api.login, onSuccess: setAuthData })
}

export function useRegister() {
  const { setAuthData } = useAuth()
  return useMutation({ mutationFn: api.register, onSuccess: setAuthData })
}

export function useLogout() {
  const { refreshToken: storedRefreshToken, clearAuthData } = useAuth()
  return useMutation({
    mutationFn: async () => { if (storedRefreshToken.value) await api.logout(storedRefreshToken.value) },
    onSettled: clearAuthData,
  })
}

export function useRefreshToken() {
  const { refreshToken: storedRefreshToken, setAuthData, clearAuthData } = useAuth()
  return useMutation({
    mutationFn: async () => {
      if (!storedRefreshToken.value) throw new Error('No refresh token')
      return api.refreshToken(storedRefreshToken.value)
    },
    onSuccess: setAuthData,
    onError: clearAuthData,
  })
}
