import { ref, watch, onMounted } from 'vue'

/**
 * Dark mode composable for managing theme preferences
 * Persists preference to localStorage and respects system preference
 */
export function useDarkMode() {
  const isDark = ref(false)
  const STORAGE_KEY = 'theme-preference'

  /**
   * Get system color scheme preference
   */
  const getSystemPreference = (): boolean => {
    if (typeof window === 'undefined') return false
    return window.matchMedia('(prefers-color-scheme: dark)').matches
  }

  /**
   * Get stored preference or fall back to system preference
   */
  const getStoredPreference = (): boolean => {
    if (typeof localStorage === 'undefined') return getSystemPreference()
    
    const stored = localStorage.getItem(STORAGE_KEY)
    if (stored === 'dark') return true
    if (stored === 'light') return false
    
    return getSystemPreference()
  }

  /**
   * Apply theme to document
   */
  const applyTheme = (dark: boolean) => {
    if (typeof document === 'undefined') return
    
    const root = document.documentElement
    
    if (dark) {
      root.classList.add('dark')
    } else {
      root.classList.remove('dark')
    }
  }

  /**
   * Toggle between light and dark mode
   */
  const toggleDarkMode = () => {
    isDark.value = !isDark.value
  }

  /**
   * Set specific theme
   */
  const setDarkMode = (dark: boolean) => {
    isDark.value = dark
  }

  // Watch for changes and persist
  watch(isDark, (newValue) => {
    applyTheme(newValue)
    if (typeof localStorage !== 'undefined') {
      localStorage.setItem(STORAGE_KEY, newValue ? 'dark' : 'light')
    }
  })

  // Initialize on mount
  onMounted(() => {
    isDark.value = getStoredPreference()
    applyTheme(isDark.value)

    // Listen for system preference changes
    if (typeof window !== 'undefined') {
      window.matchMedia('(prefers-color-scheme: dark)')
        .addEventListener('change', (e) => {
          // Only auto-switch if no stored preference
          const stored = localStorage.getItem(STORAGE_KEY)
          if (!stored) {
            isDark.value = e.matches
          }
        })
    }
  })

  return {
    isDark,
    toggleDarkMode,
    setDarkMode
  }
}
