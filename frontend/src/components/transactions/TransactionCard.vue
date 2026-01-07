<template>
  <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 hover:shadow-md transition-shadow">
    <div class="flex items-start justify-between">
      <div class="flex-1 min-w-0">
        <div class="flex items-center space-x-2">
          <span 
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
            :class="typeClass"
          >
            {{ typeLabel }}
          </span>
          <span 
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
            :class="statusClass"
          >
            {{ statusLabel }}
          </span>
          <span 
            v-if="transaction.isDisputed"
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-400"
          >
            Disputed
          </span>
        </div>
        <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white truncate">
          {{ transaction.merchantName }}
        </h3>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400 truncate">
          {{ transaction.description }}
        </p>
        <div class="mt-2 flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400">
          <span>{{ formattedDate }}</span>
          <span class="inline-flex items-center px-2 py-0.5 rounded bg-gray-100 dark:bg-gray-700 text-gray-600 dark:text-gray-300">
            {{ transaction.category }}
          </span>
          <span v-if="transaction.reference">Ref: {{ transaction.reference }}</span>
        </div>
      </div>
      <div class="ml-4 flex flex-col items-end">
        <span 
          class="text-lg font-semibold"
          :class="transaction.type === 1 ? 'text-green-600 dark:text-green-400' : 'text-gray-900 dark:text-white'"
        >
          {{ transaction.type === 1 ? '+' : '-' }}{{ formattedAmount }}
        </span>
        <span class="text-xs text-gray-500 dark:text-gray-400">{{ transaction.currency }}</span>
        <!-- Dispute button - show if not currently disputed OR has previous disputes but can re-dispute -->
        <button
          v-if="!transaction.isDisputed && transaction.status === 1"
          @click="$emit('dispute', transaction)"
          class="mt-3 inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
        >
          Dispute
        </button>
        <!-- Show dispute history link when currently disputed -->
        <div v-else-if="transaction.isDisputed" class="mt-3 flex flex-col items-end space-y-1">
          <span class="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-red-100 dark:bg-red-900/30 text-red-800 dark:text-red-400">
            <svg class="mr-1 h-3 w-3" fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M8.257 3.099c.765-1.36 2.722-1.36 3.486 0l5.58 9.92c.75 1.334-.213 2.98-1.742 2.98H4.42c-1.53 0-2.493-1.646-1.743-2.98l5.58-9.92zM11 13a1 1 0 11-2 0 1 1 0 012 0zm-1-8a1 1 0 00-1 1v3a1 1 0 002 0V6a1 1 0 00-1-1z" clip-rule="evenodd" />
            </svg>
            Active Dispute
          </span>
          <button
            @click.stop="$emit('viewHistory', transaction)"
            class="text-xs text-primary-600 dark:text-primary-400 hover:text-primary-800 dark:hover:text-primary-300 underline"
          >
            View History
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Transaction } from '@/types'
import { format } from 'date-fns'

const props = defineProps<{
  transaction: Transaction
}>()

defineEmits<{
  (e: 'dispute', transaction: Transaction): void
  (e: 'viewHistory', transaction: Transaction): void
}>()

const formattedAmount = computed(() => {
  return new Intl.NumberFormat('en-ZA', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(props.transaction.amount)
})

const formattedDate = computed(() => {
  return format(new Date(props.transaction.transactionDate), 'dd MMM yyyy, HH:mm')
})

const typeLabel = computed(() => {
  return props.transaction.type === 0 ? 'Debit' : 'Credit'
})

const typeClass = computed(() => {
  return props.transaction.type === 0 
    ? 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400' 
    : 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400'
})

const statusLabel = computed(() => {
  const statuses = ['Pending', 'Completed', 'Failed', 'Reversed']
  return statuses[props.transaction.status] || 'Unknown'
})

const statusClass = computed(() => {
  const classes: Record<number, string> = {
    0: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
    1: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
    2: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
    3: 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400'
  }
  return classes[props.transaction.status] || 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
})
</script>
