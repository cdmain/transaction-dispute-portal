import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

// Determine base path based on environment
const getBasePath = () => {
  if (!process.env.GITHUB_ACTIONS) return '/'
  
  const env = process.env.VITE_ENVIRONMENT || 'prod'
  const repoBase = '/transaction-dispute-portal'
  
  switch (env) {
    case 'dev': return `${repoBase}/dev/`
    case 'int': return `${repoBase}/int/`
    case 'qa': return `${repoBase}/qa/`
    case 'prod': return `${repoBase}/prod/`
    default: return `${repoBase}/`
  }
}

export default defineConfig({
  plugins: [vue()],
  // Dynamic base path for multi-environment GitHub Pages
  base: getBasePath(),
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 3000,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true
      }
    }
  }
})
