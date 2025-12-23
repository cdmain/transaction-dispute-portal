<template>
  <Teleport to="body">
    <Transition
      enter-active-class="transition ease-out duration-200"
      enter-from-class="opacity-0"
      enter-to-class="opacity-100"
      leave-active-class="transition ease-in duration-150"
      leave-from-class="opacity-100"
      leave-to-class="opacity-0"
    >
      <div v-if="isOpen" class="fixed inset-0 z-50 overflow-y-auto">
        <!-- Backdrop -->
        <div class="fixed inset-0 bg-black bg-opacity-50" @click="$emit('close')"></div>
        
        <!-- Modal -->
        <div class="flex min-h-full items-center justify-center p-4">
          <div class="relative bg-white rounded-lg shadow-xl max-w-2xl w-full max-h-[80vh] overflow-hidden">
            <!-- Header -->
            <div class="px-6 py-4 border-b border-gray-200 bg-gray-50">
              <div class="flex items-center justify-between">
                <div>
                  <h2 class="text-lg font-semibold text-gray-900">Dispute History</h2>
                  <p class="mt-1 text-sm text-gray-500">
                    {{ transaction?.merchantName }} - {{ formatCurrency(transaction?.amount ?? 0) }}
                  </p>
                </div>
                <button
                  @click="$emit('close')"
                  class="text-gray-400 hover:text-gray-500 transition-colors"
                >
                  <svg class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
            </div>

            <!-- Content -->
            <div class="px-6 py-4 overflow-y-auto max-h-[50vh]">
              <!-- Loading -->
              <div v-if="isLoading" class="space-y-4">
                <div v-for="i in 3" :key="i" class="animate-pulse">
                  <div class="h-20 bg-gray-200 rounded"></div>
                </div>
              </div>

              <!-- Error -->
              <div v-else-if="error" class="text-center py-8">
                <p class="text-red-600">{{ error }}</p>
              </div>

              <!-- No History -->
              <div v-else-if="!disputes || disputes.length === 0" class="text-center py-8">
                <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
                </svg>
                <h3 class="mt-2 text-sm font-medium text-gray-900">No dispute history</h3>
                <p class="mt-1 text-sm text-gray-500">This transaction has no previous disputes.</p>
              </div>

              <!-- Dispute History List -->
              <div v-else class="space-y-4">
                <div 
                  v-for="dispute in disputes" 
                  :key="dispute.id"
                  class="border rounded-lg p-4"
                  :class="getDisputeBorderClass(dispute.status)"
                >
                  <div class="flex items-start justify-between">
                    <div class="flex-1">
                      <div class="flex items-center space-x-2">
                        <span 
                          class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium"
                          :class="getStatusClass(dispute.status)"
                        >
                          {{ getStatusLabel(dispute.status) }}
                        </span>
                        <span class="text-xs text-gray-500">
                          {{ formatDate(dispute.createdAt) }}
                        </span>
                      </div>
                      <p class="mt-2 text-sm font-medium text-gray-900">{{ dispute.reason }}</p>
                      <p class="mt-1 text-sm text-gray-600">{{ dispute.description }}</p>
                      <div class="mt-2 text-xs text-gray-500">
                        <span>Amount: {{ formatCurrency(dispute.disputedAmount) }}</span>
                        <span v-if="dispute.resolvedAt" class="ml-4">
                          Resolved: {{ formatDate(dispute.resolvedAt) }}
                        </span>
                      </div>
                      <p v-if="dispute.resolutionNotes" class="mt-2 text-sm text-gray-600 italic">
                        "{{ dispute.resolutionNotes }}"
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            </div>

            <!-- Footer with Re-dispute option -->
            <div class="px-6 py-4 border-t border-gray-200 bg-gray-50">
              <div class="flex items-center justify-between">
                <p class="text-sm text-gray-500">
                  <span v-if="canRedispute">
                    You can file a new dispute for this transaction.
                  </span>
                  <span v-else class="text-amber-600">
                    An active dispute exists for this transaction.
                  </span>
                </p>
                <div class="flex space-x-3">
                  <button
                    @click="$emit('close')"
                    class="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                  >
                    Close
                  </button>
                  <button
                    v-if="canRedispute"
                    @click="$emit('redispute', transaction)"
                    class="px-4 py-2 text-sm font-medium text-white bg-primary-600 border border-transparent rounded-md hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
                  >
                    File New Dispute
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, watch, ref } from 'vue'
import { useDisputesByTransaction } from '@/composables'
import type { Transaction, Dispute } from '@/types'
import { DisputeStatus } from '@/types'
import { format } from 'date-fns'

const props = defineProps<{
  isOpen: boolean
  transaction: Transaction | null
}>()

defineEmits<{
  (e: 'close'): void
  (e: 'redispute', transaction: Transaction): void
}>()

const transactionId = computed(() => props.transaction?.id ?? '')
const { data: disputes, isLoading, error } = useDisputesByTransaction(transactionId.value)

// Check if there's an active (non-resolved/cancelled) dispute
const canRedispute = computed(() => {
  if (!disputes.value) return true
  return !disputes.value.some(d => 
    d.status === DisputeStatus.Pending || 
    d.status === DisputeStatus.UnderReview ||
    d.status === DisputeStatus.AwaitingDocuments
  )
})

function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
  }).format(amount)
}

function formatDate(dateStr: string) {
  return format(new Date(dateStr), 'dd MMM yyyy, HH:mm')
}

function getStatusLabel(status: DisputeStatus): string {
  const labels: Record<number, string> = {
    0: 'Pending',
    1: 'Under Review',
    2: 'Awaiting Documents',
    3: 'Resolved',
    4: 'Rejected',
    5: 'Cancelled'
  }
  return labels[status] || 'Unknown'
}

function getStatusClass(status: DisputeStatus): string {
  const classes: Record<number, string> = {
    0: 'bg-yellow-100 text-yellow-800',
    1: 'bg-blue-100 text-blue-800',
    2: 'bg-orange-100 text-orange-800',
    3: 'bg-green-100 text-green-800',
    4: 'bg-red-100 text-red-800',
    5: 'bg-gray-100 text-gray-800'
  }
  return classes[status] || 'bg-gray-100 text-gray-800'
}

function getDisputeBorderClass(status: DisputeStatus): string {
  const classes: Record<number, string> = {
    0: 'border-yellow-200 bg-yellow-50',
    1: 'border-blue-200 bg-blue-50',
    2: 'border-orange-200 bg-orange-50',
    3: 'border-green-200 bg-green-50',
    4: 'border-red-200 bg-red-50',
    5: 'border-gray-200 bg-gray-50'
  }
  return classes[status] || 'border-gray-200'
}
</script>
