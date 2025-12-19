<template>
  <div 
    class="bg-white rounded-lg shadow-sm border border-gray-200 p-4 hover:shadow-md transition-shadow cursor-pointer"
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
            class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800"
          >
            {{ categoryLabel }}
          </span>
        </div>
        <h3 class="mt-2 text-sm font-medium text-gray-900 truncate">
          {{ dispute.reason }}
        </h3>
        <p class="mt-1 text-sm text-gray-500 line-clamp-2">
          {{ dispute.description }}
        </p>
        <div class="mt-2 flex items-center space-x-4 text-xs text-gray-500">
          <span>Filed: {{ formattedCreatedDate }}</span>
          <span v-if="dispute.merchantName">{{ dispute.merchantName }}</span>
          <span v-if="dispute.transactionReference">Ref: {{ dispute.transactionReference }}</span>
        </div>
      </div>
      <div class="ml-4 flex flex-col items-end">
        <span class="text-lg font-semibold text-gray-900">
          {{ formattedAmount }}
        </span>
        <span class="text-xs text-gray-500">{{ dispute.currency }}</span>
        <button
          v-if="canCancel"
          @click.stop="$emit('cancel', dispute)"
          class="mt-3 inline-flex items-center px-3 py-1.5 border border-gray-300 text-xs font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 transition-colors"
        >
          Cancel
        </button>
      </div>
    </div>
    <div v-if="dispute.resolvedAt" class="mt-3 pt-3 border-t border-gray-100">
      <div class="flex items-center justify-between text-xs text-gray-500">
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
}>()

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
    0: 'bg-yellow-100 text-yellow-800',
    1: 'bg-blue-100 text-blue-800',
    2: 'bg-orange-100 text-orange-800',
    3: 'bg-green-100 text-green-800',
    4: 'bg-red-100 text-red-800',
    5: 'bg-gray-100 text-gray-800'
  }
  return classes[props.dispute.status] || 'bg-gray-100 text-gray-800'
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
