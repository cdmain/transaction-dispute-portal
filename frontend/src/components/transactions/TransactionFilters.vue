<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
    <!-- Search Row -->
    <div class="p-3 border-b border-gray-200 dark:border-gray-700">
      <div class="relative">
        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <svg class="h-4 w-4 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
          </svg>
        </div>
        <input
          v-model="localFilters.searchTerm"
          type="text"
          placeholder="Search transactions..."
          class="block w-full pl-9 pr-3 py-2 text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @input="debouncedSearch"
        />
      </div>
    </div>

    <!-- Filters Row -->
    <div class="p-3 flex flex-wrap items-center gap-3">
      <div class="flex-shrink-0">
        <label class="sr-only">Category</label>
        <select
          v-model="localFilters.category"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white py-1.5 pl-3 pr-8 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @change="applyFilters"
        >
          <option value="">All Categories</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>
      </div>

      <div class="flex-shrink-0">
        <label class="sr-only">Type</label>
        <select
          v-model="localFilters.type"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white py-1.5 pl-3 pr-8 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @change="applyFilters"
        >
          <option :value="undefined">All Types</option>
          <option :value="0">Debit</option>
          <option :value="1">Credit</option>
        </select>
      </div>

      <div class="flex items-center gap-2 flex-shrink-0">
        <span class="text-xs text-gray-500 dark:text-gray-400">From</span>
        <input
          v-model="localFilters.fromDate"
          type="date"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white py-1.5 px-2 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @change="applyFilters"
        />
      </div>

      <div class="flex items-center gap-2 flex-shrink-0">
        <span class="text-xs text-gray-500 dark:text-gray-400">To</span>
        <input
          v-model="localFilters.toDate"
          type="date"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white py-1.5 px-2 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @change="applyFilters"
        />
      </div>

      <div class="ml-auto">
        <button
          @click="resetFilters"
          class="inline-flex items-center px-3 py-1.5 text-xs font-medium text-gray-600 dark:text-gray-300 hover:text-gray-900 dark:hover:text-white"
        >
          <svg class="h-3.5 w-3.5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" />
          </svg>
          Reset
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { reactive } from 'vue'
import type { TransactionQueryParams } from '@/types'

defineProps<{
  categories: string[]
}>()

const emit = defineEmits<{
  (e: 'filter', params: Partial<TransactionQueryParams>): void
  (e: 'reset'): void
}>()

const localFilters = reactive<Partial<TransactionQueryParams>>({
  searchTerm: '',
  category: '',
  type: undefined,
  fromDate: '',
  toDate: ''
})

let searchTimeout: ReturnType<typeof setTimeout> | null = null

function debouncedSearch() {
  if (searchTimeout) {
    clearTimeout(searchTimeout)
  }
  searchTimeout = setTimeout(() => {
    applyFilters()
  }, 300)
}

function applyFilters() {
  const params: Partial<TransactionQueryParams> = {}
  
  if (localFilters.searchTerm) params.searchTerm = localFilters.searchTerm
  if (localFilters.category) params.category = localFilters.category
  if (localFilters.type !== undefined) params.type = localFilters.type
  if (localFilters.fromDate) params.fromDate = localFilters.fromDate
  if (localFilters.toDate) params.toDate = localFilters.toDate
  
  emit('filter', params)
}

function resetFilters() {
  localFilters.searchTerm = ''
  localFilters.category = ''
  localFilters.type = undefined
  localFilters.fromDate = ''
  localFilters.toDate = ''
  emit('reset')
}
</script>
