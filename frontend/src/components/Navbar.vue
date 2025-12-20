<template>
  <nav class="bg-white dark:bg-gray-900 shadow-sm border-b border-gray-200 dark:border-gray-700 transition-colors">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between h-16">
        <div class="flex">
          <div class="flex-shrink-0 flex items-center">
            <svg class="h-8 w-8 text-primary-600 dark:text-primary-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" 
                d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
            </svg>
            <span class="ml-2 text-xl font-bold text-gray-900 dark:text-white">DisputeHub</span>
          </div>
          <div class="hidden sm:ml-8 sm:flex sm:space-x-8">
            <RouterLink
              to="/"
              class="inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium transition-colors"
              :class="isActive('/') 
                ? 'border-primary-500 text-gray-900 dark:text-white' 
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:border-gray-300 hover:text-gray-700 dark:hover:text-gray-200'"
            >
              Dashboard
            </RouterLink>
            <RouterLink
              to="/transactions"
              class="inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium transition-colors"
              :class="isActive('/transactions') 
                ? 'border-primary-500 text-gray-900 dark:text-white' 
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:border-gray-300 hover:text-gray-700 dark:hover:text-gray-200'"
            >
              Transactions
            </RouterLink>
            <RouterLink
              to="/disputes"
              class="inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium transition-colors"
              :class="isActive('/disputes') 
                ? 'border-primary-500 text-gray-900 dark:text-white' 
                : 'border-transparent text-gray-500 dark:text-gray-400 hover:border-gray-300 hover:text-gray-700 dark:hover:text-gray-200'"
            >
              Disputes
            </RouterLink>
          </div>
        </div>
        <div class="flex items-center space-x-4">
          <!-- Dark Mode Toggle -->
          <button
            @click="toggleDarkMode"
            class="p-2 rounded-lg text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-800 focus:outline-none focus:ring-2 focus:ring-primary-500 transition-colors"
            :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
          >
            <!-- Sun icon (shown in dark mode) -->
            <svg v-if="isDark" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z" />
            </svg>
            <!-- Moon icon (shown in light mode) -->
            <svg v-else class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z" />
            </svg>
          </button>

          <div v-if="isAuthenticated && user" class="flex items-center space-x-4">
            <div class="flex items-center space-x-2">
              <div class="h-8 w-8 rounded-full bg-primary-600 flex items-center justify-center text-white font-medium text-sm">
                {{ userInitials }}
              </div>
              <div class="hidden md:block">
                <p class="text-sm font-medium text-gray-900 dark:text-white">{{ user.firstName }} {{ user.lastName }}</p>
                <p class="text-xs text-gray-500 dark:text-gray-400">{{ user.customerId }}</p>
              </div>
            </div>
            <button
              @click="handleLogout"
              :disabled="logout.isPending.value"
              class="inline-flex items-center px-3 py-1.5 border border-gray-300 dark:border-gray-600 shadow-sm text-sm font-medium rounded-md text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50 transition-colors"
            >
              <svg v-if="logout.isPending.value" class="animate-spin -ml-0.5 mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
              Logout
            </button>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRoute, useRouter } from 'vue-router'
import { useAuth, useLogout } from '@/composables/useAuth'
import { useDarkMode } from '@/composables/useDarkMode'

const route = useRoute()
const router = useRouter()
const { isAuthenticated, user } = useAuth()
const logout = useLogout()
const { isDark, toggleDarkMode } = useDarkMode()

const userInitials = computed(() => {
  if (!user.value) return '?'
  return `${user.value.firstName[0]}${user.value.lastName[0]}`.toUpperCase()
})

function isActive(path: string): boolean {
  if (path === '/') {
    return route.path === '/'
  }
  return route.path.startsWith(path)
}

function handleLogout() {
  logout.mutate(undefined, {
    onSuccess: () => {
      router.push('/login')
    }
  })
}
</script>
