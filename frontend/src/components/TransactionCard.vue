<template>
  <div class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:shadow-md transition-shadow">
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
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-red-100 text-red-800"
          >
            Disputed
          </span>
        </div>
        <h3 class="mt-2 text-sm font-medium text-gray-900 truncate">
          {{ transaction.merchantName }}
        </h3>
        <p class="mt-1 text-sm text-gray-500 truncate">
          {{ transaction.description }}
        </p>
        <div class="mt-2 flex items-center space-x-4 text-xs text-gray-500">
          <span>{{ formattedDate }}</span>
          <span class="inline-flex items-center px-2 py-0.5 rounded bg-gray-100 text-gray-600">
            {{ transaction.category }}
          </span>
          <span v-if="transaction.reference">Ref: {{ transaction.reference }}</span>
        </div>
      </div>
      <div class="ml-4 flex flex-col items-end">
        <span 
          class="text-lg font-semibold"
          :class="transaction.type === 1 ? 'text-green-600' : 'text-gray-900'"
        >
          {{ transaction.type === 1 ? '+' : '-' }}{{ formattedAmount }}
        </span>
        <span class="text-xs text-gray-500">{{ transaction.currency }}</span>
        <button
          v-if="!transaction.isDisputed && transaction.status === 1"
          @click="$emit('dispute', transaction)"
          class="mt-3 inline-flex items-center px-3 py-1.5 border border-transparent text-xs font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
        >
          Dispute
        </button>
        <span 
          v-else-if="transaction.isDisputed"
          class="mt-3 text-xs text-red-600"
        >
          In dispute
        </span>
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
    ? 'bg-orange-100 text-orange-800' 
    : 'bg-green-100 text-green-800'
})

const statusLabel = computed(() => {
  const statuses = ['Pending', 'Completed', 'Failed', 'Reversed']
  return statuses[props.transaction.status] || 'Unknown'
})

const statusClass = computed(() => {
  const classes: Record<number, string> = {
    0: 'bg-yellow-100 text-yellow-800',
    1: 'bg-green-100 text-green-800',
    2: 'bg-red-100 text-red-800',
    3: 'bg-purple-100 text-purple-800'
  }
  return classes[props.transaction.status] || 'bg-gray-100 text-gray-800'
})
</script>
