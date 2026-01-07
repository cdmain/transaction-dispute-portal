<template>
  <div 
    class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4 hover:shadow-md transition-shadow cursor-pointer"
    @click="$emit('click', dispute)"
  >
    <div class="flex items-start justify-between">
      <div class="flex-1 min-w-0">
        <div class="flex items-center space-x-2">
          <span 
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
            :class="statusClass"
          >
            {{ statusLabel }}
          </span>
          <span 
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 dark:bg-gray-700 text-gray-800 dark:text-gray-200"
          >
            {{ categoryLabel }}
          </span>
        </div>
        <!-- Merchant Name (Prominent) -->
        <h3 v-if="dispute.merchantName" class="mt-2 text-base font-semibold text-gray-900 dark:text-white">
          {{ dispute.merchantName }}
        </h3>
        <p class="mt-1 text-sm font-medium text-gray-700 dark:text-gray-300 truncate">
          {{ dispute.reason }}
        </p>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          {{ truncatedDescription }}
        </p>
        <div class="mt-2 flex items-center space-x-4 text-xs text-gray-500 dark:text-gray-400">
          <span>Filed: {{ formattedCreatedDate }}</span>
          <span v-if="dispute.transactionReference" class="font-mono">Ref: {{ dispute.transactionReference }}</span>
        </div>
      </div>
      <div class="ml-4 flex flex-col items-end">
        <span class="text-lg font-semibold text-gray-900 dark:text-white">
          {{ formattedAmount }}
        </span>
        <span class="text-xs text-gray-500 dark:text-gray-400">{{ dispute.currency }}</span>
      </div>
    </div>
    
    <!-- Action Buttons Row - shown at bottom for better layout -->
    <div v-if="canEdit || canCancel" class="mt-4 pt-3 border-t border-gray-100 dark:border-gray-700 flex items-center justify-end space-x-3">
      <!-- Edit Button -->
      <button
        v-if="canEdit"
        @click.stop="$emit('edit', dispute)"
        class="inline-flex items-center px-3 py-1.5 border border-gray-300 dark:border-gray-600 text-xs font-medium rounded-md text-gray-700 dark:text-gray-200 bg-white dark:bg-gray-700 hover:bg-gray-50 dark:hover:bg-gray-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-all"
        title="Edit description"
      >
        <svg class="h-3.5 w-3.5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" />
        </svg>
        Edit
      </button>
      
      <!-- Delete Button - now gray/neutral -->
      <button
        v-if="canEdit"
        @click.stop="$emit('delete', dispute)"
        class="inline-flex items-center px-3 py-1.5 border border-gray-300 dark:border-gray-600 text-xs font-medium rounded-md text-gray-600 dark:text-gray-400 bg-white dark:bg-gray-700 hover:bg-gray-100 dark:hover:bg-gray-600 hover:text-gray-800 dark:hover:text-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400 transition-all"
        title="Delete dispute"
      >
        <svg class="h-3.5 w-3.5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
        </svg>
        Delete
      </button>
      
      <!-- Cancel Dispute Button - amber/orange for distinction -->
      <button
        v-if="canCancel"
        @click.stop="$emit('cancel', dispute)"
        class="inline-flex items-center px-3 py-1.5 border border-amber-400 dark:border-amber-600 text-xs font-medium rounded-md text-amber-700 dark:text-amber-300 bg-amber-50 dark:bg-amber-900/30 hover:bg-amber-100 dark:hover:bg-amber-900/50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-amber-500 transition-all"
      >
        <svg class="h-3.5 w-3.5 mr-1" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
        Cancel Dispute
      </button>
    </div>
    
    <div v-if="dispute.resolvedAt" class="mt-3 pt-3 border-t border-gray-100 dark:border-gray-700">
      <div class="flex items-center justify-between text-xs text-gray-500 dark:text-gray-400">
        <span>Resolved: {{ formattedResolvedDate }}</span>
        <span v-if="dispute.resolutionNotes" class="truncate max-w-xs">
          {{ dispute.resolutionNotes }}
        </span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { Dispute } from '@/types'
import { DisputeStatus } from '@/types'
import { format } from 'date-fns'

const props = defineProps<{
  dispute: Dispute
}>()

defineEmits<{
  (e: 'click', dispute: Dispute): void
  (e: 'cancel', dispute: Dispute): void
  (e: 'edit', dispute: Dispute): void
  (e: 'delete', dispute: Dispute): void
}>()

const truncatedDescription = computed(() => {
  const desc = props.dispute.description || ''
  return desc.length > 10 ? desc.substring(0, 10) + '...' : desc
})

const formattedAmount = computed(() => {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: props.dispute.currency
  }).format(props.dispute.disputedAmount)
})

const formattedCreatedDate = computed(() => {
  return format(new Date(props.dispute.createdAt), 'dd MMM yyyy')
})

const formattedResolvedDate = computed(() => {
  if (!props.dispute.resolvedAt) return ''
  return format(new Date(props.dispute.resolvedAt), 'dd MMM yyyy')
})

// Only pending disputes can be edited/deleted
const canEdit = computed(() => {
  return props.dispute.status === DisputeStatus.Pending
})

const canCancel = computed(() => {
  return props.dispute.status === DisputeStatus.Pending || 
         props.dispute.status === DisputeStatus.UnderReview
})

const statusLabel = computed(() => {
  const statuses: Record<number, string> = {
    0: 'Pending',
    1: 'Under Review',
    2: 'Awaiting Documents',
    3: 'Resolved',
    4: 'Rejected',
    5: 'Cancelled'
  }
  return statuses[props.dispute.status] || 'Unknown'
})

const statusClass = computed(() => {
  const classes: Record<number, string> = {
    0: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
    1: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
    2: 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400',
    3: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
    4: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
    5: 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
  }
  return classes[props.dispute.status] || 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300'
})

const categoryLabel = computed(() => {
  const categories: Record<number, string> = {
    0: 'Unauthorized',
    1: 'Duplicate Charge',
    2: 'Incorrect Amount',
    3: 'Service Not Received',
    4: 'Product Not Received',
    5: 'Quality Issue',
    6: 'Refund Not Received',
    7: 'Fraud Suspected',
    8: 'Other'
  }
  return categories[props.dispute.category] || 'Other'
})
</script>
