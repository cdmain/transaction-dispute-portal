<template>
  <div class="min-h-screen bg-gray-50">
    <!-- Demo Mode Banner -->
    <div v-if="isDemo && showNavbar" class="bg-gradient-to-r from-blue-600 to-purple-600 text-white text-center py-2 text-sm">
      🎮 <strong>Demo Mode</strong> — Using mock data. 
      <a href="https://github.com/cdmain/transaction-dispute-portal" target="_blank" class="underline hover:text-blue-200">View on GitHub</a>
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

onMounted(() => {
  isDemo.value = isDemoMode()
})

const showNavbar = computed(() => {
  return !['login', 'register'].includes(route.name as string)
})
</script>
