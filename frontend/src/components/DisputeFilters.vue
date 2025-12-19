<template>
  <div class="space-y-4">
    <div class="flex flex-wrap gap-4 items-end bg-white p-4 rounded-lg shadow-sm border border-gray-200">
      <div class="w-40">
        <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
        <select
          v-model="localFilters.status"
          class="block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
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

      <div class="w-48">
        <label class="block text-sm font-medium text-gray-700 mb-1">Category</label>
        <select
          v-model="localFilters.category"
          class="block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
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

      <div class="w-40">
        <label class="block text-sm font-medium text-gray-700 mb-1">From Date</label>
        <input
          v-model="localFilters.fromDate"
          type="date"
          class="block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @change="applyFilters"
        />
      </div>

      <div class="w-40">
        <label class="block text-sm font-medium text-gray-700 mb-1">To Date</label>
        <input
          v-model="localFilters.toDate"
          type="date"
          class="block w-full rounded-md border-gray-300 shadow-sm focus:border-primary-500 focus:ring-primary-500 sm:text-sm"
          @change="applyFilters"
        />
      </div>

      <button
        @click="resetFilters"
        class="inline-flex items-center px-3 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
      >
        Reset
      </button>
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
