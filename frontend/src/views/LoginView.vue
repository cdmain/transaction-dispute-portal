<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { z } from 'zod'
import { useLogin } from '@/composables/useAuth'
import { isDemoMode, DEMO_CREDENTIALS } from '@/services/mockApi'

const router = useRouter()
const login = useLogin()

const DEMO_EMAIL = DEMO_CREDENTIALS.email
const DEMO_PASSWORD = DEMO_CREDENTIALS.password

const isDemo = ref(false)

onMounted(() => {
  isDemo.value = isDemoMode()
  // Auto-fill demo credentials in demo mode
  if (isDemo.value) {
    form.value.email = DEMO_EMAIL
    form.value.password = DEMO_PASSWORD
  }
})

const loginSchema = z.object({
  email: z.string().email('Please enter a valid email'),
  password: z.string().min(8, 'Password must be at least 8 characters')
})

const form = ref({
  email: '',
  password: ''
})

const validationErrors = ref<Record<string, string>>({})
const copiedField = ref<string | null>(null)

const validateForm = () => {
  try {
    loginSchema.parse(form.value)
    validationErrors.value = {}
    return true
  } catch (error) {
    if (error instanceof z.ZodError) {
      validationErrors.value = error.errors.reduce((acc, err) => {
        acc[err.path[0] as string] = err.message
        return acc
      }, {} as Record<string, string>)
    }
    return false
  }
}

const handleSubmit = async () => {
  if (!validateForm()) return
  
  login.mutate(form.value, {
    onSuccess: () => {
      router.push('/')
    }
  })
}

const fillDemoCredentials = () => {
  form.value.email = DEMO_EMAIL
  form.value.password = DEMO_PASSWORD
}

const copyToClipboard = async (text: string, field: string) => {
  try {
    await navigator.clipboard.writeText(text)
    copiedField.value = field
    setTimeout(() => {
      copiedField.value = null
    }, 2000)
  } catch (err) {
    console.error('Failed to copy:', err)
  }
}

const errorMessage = computed(() => {
  if (!login.error.value) return null
  const error = login.error.value as any
  return error.response?.data?.message || 'Login failed. Please check your credentials.'
})
</script>

<template>
  <div class="min-h-screen flex items-center justify-center bg-gray-50 dark:bg-gray-900 py-12 px-4 sm:px-6 lg:px-8">
    <div class="max-w-md w-full space-y-8">
      <!-- Demo Mode Banner -->
      <div v-if="isDemo" class="rounded-lg bg-gradient-to-r from-blue-500 to-purple-600 p-4 text-white shadow-lg">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-lg font-bold flex items-center gap-2">
              🎮 Demo Mode
            </h3>
            <p class="text-sm text-blue-100 mt-1">
              This is a live demo with mock data. Credentials are pre-filled!
            </p>
          </div>
          <div class="text-right text-sm">
            <p class="font-mono bg-white/20 rounded px-2 py-1">{{ DEMO_EMAIL }}</p>
          </div>
        </div>
      </div>

      <div>
        <h2 class="mt-6 text-center text-3xl font-extrabold text-gray-900 dark:text-white">
          Transaction Dispute Portal
        </h2>
        <p class="mt-2 text-center text-sm text-gray-600 dark:text-gray-400">
          Sign in to your account
        </p>
      </div>
      
      <form class="mt-8 space-y-6" @submit.prevent="handleSubmit">
        <!-- Error Alert -->
        <div v-if="errorMessage" class="rounded-md bg-red-50 dark:bg-red-900/20 p-4">
          <div class="flex">
            <div class="ml-3">
              <h3 class="text-sm font-medium text-red-800 dark:text-red-400">{{ errorMessage }}</h3>
            </div>
          </div>
        </div>
        
        <div class="rounded-md shadow-sm -space-y-px">
          <div>
            <label for="email" class="sr-only">Email address</label>
            <input
              id="email"
              v-model="form.email"
              name="email"
              type="email"
              autocomplete="email"
              required
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 placeholder-gray-500 dark:placeholder-gray-400 text-gray-900 dark:text-white dark:bg-gray-800 rounded-t-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              :class="{ 'border-red-300 dark:border-red-500': validationErrors.email }"
              placeholder="Email address"
            />
            <p v-if="validationErrors.email" class="mt-1 text-xs text-red-600 dark:text-red-400">
              {{ validationErrors.email }}
            </p>
          </div>
          <div>
            <label for="password" class="sr-only">Password</label>
            <input
              id="password"
              v-model="form.password"
              name="password"
              type="password"
              autocomplete="current-password"
              required
              class="appearance-none rounded-none relative block w-full px-3 py-2 border border-gray-300 dark:border-gray-600 placeholder-gray-500 dark:placeholder-gray-400 text-gray-900 dark:text-white dark:bg-gray-800 rounded-b-md focus:outline-none focus:ring-blue-500 focus:border-blue-500 focus:z-10 sm:text-sm"
              :class="{ 'border-red-300 dark:border-red-500': validationErrors.password }"
              placeholder="Password"
            />
            <p v-if="validationErrors.password" class="mt-1 text-xs text-red-600 dark:text-red-400">
              {{ validationErrors.password }}
            </p>
          </div>
        </div>

        <div>
          <button
            type="submit"
            :disabled="login.isPending.value"
            class="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            <span v-if="login.isPending.value" class="absolute left-0 inset-y-0 flex items-center pl-3">
              <svg class="animate-spin h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
              </svg>
            </span>
            {{ login.isPending.value ? 'Signing in...' : 'Sign in' }}
          </button>
        </div>

        <div class="text-center">
          <router-link to="/register" class="font-medium text-blue-600 hover:text-blue-500 dark:text-blue-400 dark:hover:text-blue-300">
            Don't have an account? Register
          </router-link>
        </div>

        <!-- Demo credentials with copy and fill buttons -->
        <div class="mt-4 p-4 bg-gradient-to-r from-blue-50 to-indigo-50 dark:from-blue-900/20 dark:to-indigo-900/20 rounded-lg border border-blue-200 dark:border-blue-800">
          <p class="text-sm font-semibold text-blue-800 dark:text-blue-300 text-center mb-3">Demo Credentials</p>
          
          <div class="space-y-2">
            <div class="flex items-center justify-between bg-white dark:bg-gray-800 rounded-md px-3 py-2 border border-gray-200 dark:border-gray-700">
              <div class="flex-1">
                <span class="text-xs text-gray-500 dark:text-gray-400">Email:</span>
                <span class="ml-2 text-sm font-mono text-gray-800 dark:text-gray-200">demo@example.com</span>
              </div>
              <button 
                type="button"
                @click="copyToClipboard('demo@example.com', 'email')"
                class="ml-2 p-1.5 text-gray-500 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded transition-colors"
                title="Copy email"
              >
                <svg v-if="copiedField === 'email'" class="w-4 h-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                </svg>
                <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path>
                </svg>
              </button>
            </div>
            
            <div class="flex items-center justify-between bg-white dark:bg-gray-800 rounded-md px-3 py-2 border border-gray-200 dark:border-gray-700">
              <div class="flex-1">
                <span class="text-xs text-gray-500 dark:text-gray-400">Password:</span>
                <span class="ml-2 text-sm font-mono text-gray-800 dark:text-gray-200">Demo123!</span>
              </div>
              <button 
                type="button"
                @click="copyToClipboard('Demo123!', 'password')"
                class="ml-2 p-1.5 text-gray-500 dark:text-gray-400 hover:text-blue-600 dark:hover:text-blue-400 hover:bg-blue-50 dark:hover:bg-blue-900/30 rounded transition-colors"
                title="Copy password"
              >
                <svg v-if="copiedField === 'password'" class="w-4 h-4 text-green-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path>
                </svg>
                <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path>
                </svg>
              </button>
            </div>
          </div>
          
          <button
            type="button"
            @click="fillDemoCredentials"
            class="mt-3 w-full py-2 px-4 border border-blue-300 dark:border-blue-700 text-sm font-medium rounded-md text-blue-700 dark:text-blue-300 bg-white dark:bg-gray-800 hover:bg-blue-50 dark:hover:bg-blue-900/30 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition-colors"
          >
            ✨ Fill Demo Credentials
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
