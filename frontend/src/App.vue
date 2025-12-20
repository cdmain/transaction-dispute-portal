<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Environment Banner -->
    <div v-if="isDemo && showNavbar" :class="envBannerClass">
      <div class="flex items-center justify-center gap-4 text-sm">
        <span>
          <span class="font-bold">{{ envLabel }}</span> Environment
          <span v-if="appVersion" class="opacity-75 ml-2">v{{ appVersion }}</span>
        </span>
        <span class="opacity-75">•</span>
        <span>🎮 Demo Mode — Using mock data</span>
        <span class="opacity-75">•</span>
        <a href="https://github.com/cdmain/transaction-dispute-portal" target="_blank" class="underline hover:opacity-75">
          View on GitHub
        </a>
      </div>
    </div>
    
    <Navbar v-if="showNavbar" />
    <main :class="showNavbar ? 'max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8' : ''">
      <RouterView />
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import Navbar from './components/Navbar.vue'
import { isDemoMode } from './services/mockApi'

const route = useRoute()
const isDemo = ref(false)
const environment = ref<string>('prod')
const appVersion = ref<string>('')

onMounted(() => {
  isDemo.value = isDemoMode()
  environment.value = import.meta.env.VITE_ENVIRONMENT || 'prod'
  appVersion.value = import.meta.env.VITE_APP_VERSION || ''
})

const envLabel = computed(() => {
  const labels: Record<string, string> = {
    dev: '🚧 DEV',
    int: '🔗 INT',
    qa: '🧪 QA',
    prod: '🚀 PROD'
  }
  return labels[environment.value] || '🚀 PROD'
})

const envBannerClass = computed(() => {
  const baseClasses = 'text-white text-center py-2'
  const envColors: Record<string, string> = {
    dev: 'bg-gradient-to-r from-green-600 to-green-500',
    int: 'bg-gradient-to-r from-blue-600 to-blue-500',
    qa: 'bg-gradient-to-r from-amber-600 to-amber-500',
    prod: 'bg-gradient-to-r from-red-600 to-red-500'
  }
  return `${baseClasses} ${envColors[environment.value] || envColors.prod}`
})

const showNavbar = computed(() => {
  return !['login', 'register'].includes(route.name as string)
})
</script>
