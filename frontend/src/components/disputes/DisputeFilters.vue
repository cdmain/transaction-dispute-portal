<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700">
    <!-- Filters Row -->
    <div class="p-3 flex flex-wrap items-center gap-3">
      <div class="flex-shrink-0">
        <label class="sr-only">Status</label>
        <select
          v-model="localFilters.status"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white py-1.5 pl-3 pr-8 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @change="applyFilters"
        >
          <option :value="undefined">All Statuses</option>
          <option :value="0">Pending</option>
          <option :value="1">Under Review</option>
          <option :value="2">Awaiting Documents</option>
          <option :value="3">Resolved</option>
          <option :value="4">Rejected</option>
          <option :value="5">Cancelled</option>
        </select>
      </div>

      <div class="flex-shrink-0">
        <label class="sr-only">Category</label>
        <select
          v-model="localFilters.category"
          class="text-sm border border-gray-300 dark:border-gray-600 rounded-md bg-white dark:bg-gray-700 text-gray-900 dark:text-white py-1.5 pl-3 pr-8 focus:outline-none focus:ring-1 focus:ring-primary-500 focus:border-primary-500"
          @change="applyFilters"
        >
          <option :value="undefined">All Categories</option>
          <option :value="0">Unauthorized</option>
          <option :value="1">Duplicate Charge</option>
          <option :value="2">Incorrect Amount</option>
          <option :value="3">Service Not Received</option>
          <option :value="4">Product Not Received</option>
          <option :value="5">Quality Issue</option>
          <option :value="6">Refund Not Received</option>
          <option :value="7">Fraud Suspected</option>
          <option :value="8">Other</option>
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
import type { DisputeQueryParams } from '@/types'

const emit = defineEmits<{
  (e: 'filter', params: Partial<DisputeQueryParams>): void
  (e: 'reset'): void
}>()

const localFilters = reactive<Partial<DisputeQueryParams>>({
  status: undefined,
  category: undefined,
  fromDate: '',
  toDate: ''
})

function applyFilters() {
  const params: Partial<DisputeQueryParams> = {}
  
  if (localFilters.status !== undefined) params.status = localFilters.status
  if (localFilters.category !== undefined) params.category = localFilters.category
  if (localFilters.fromDate) params.fromDate = localFilters.fromDate
  if (localFilters.toDate) params.toDate = localFilters.toDate
  
  emit('filter', params)
}

function resetFilters() {
  localFilters.status = undefined
  localFilters.category = undefined
  localFilters.fromDate = ''
  localFilters.toDate = ''
  emit('reset')
}
</script>
