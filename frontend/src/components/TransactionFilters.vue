<template>
  <div class="space-y-4">
    <div class="flex flex-wrap gap-4 items-end bg-white dark:bg-gray-800 p-4 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
      <div class="flex-1 min-w-[200px]">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Search</label>
        <input
          v-model="localFilters.searchTerm"
          type="text"
          placeholder="Search transactions..."
          class="block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @input="debouncedSearch"
        />
      </div>

      <div class="w-40">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Category</label>
        <select
          v-model="localFilters.category"
          class="block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @change="applyFilters"
        >
          <option value="">All Categories</option>
          <option v-for="cat in categories" :key="cat" :value="cat">{{ cat }}</option>
        </select>
      </div>

      <div class="w-32">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Type</label>
        <select
          v-model="localFilters.type"
          class="block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @change="applyFilters"
        >
          <option :value="undefined">All</option>
          <option :value="0">Debit</option>
          <option :value="1">Credit</option>
        </select>
      </div>

      <div class="w-40">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">From Date</label>
        <input
          v-model="localFilters.fromDate"
          type="date"
          class="block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @change="applyFilters"
        />
      </div>

      <div class="w-40">
        <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">To Date</label>
        <input
          v-model="localFilters.toDate"
          type="date"
          class="block w-full rounded-md border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 text-gray-900 dark:text-white shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @change="applyFilters"
        />
      </div>

      <button
        @click="resetFilters"
        class="inline-flex items-center px-3 py-2 border border-gray-300 dark:border-gray-600 shadow-sm text-sm font-medium rounded-md text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
      >
        Reset
      </button>
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
