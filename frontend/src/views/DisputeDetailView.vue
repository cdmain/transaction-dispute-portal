<template>
  <div class="max-w-3xl mx-auto">
    <!-- Back Button -->
    <RouterLink
      to="/disputes"
      class="inline-flex items-center text-sm font-medium text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200 mb-6"
    >
      <svg class="mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7" />
      </svg>
      Back to Disputes
    </RouterLink>

    <!-- Loading State -->
    <div v-if="isLoading" class="animate-pulse space-y-4">
      <div class="h-8 w-64 bg-gray-200 dark:bg-gray-700 rounded"></div>
      <div class="h-4 w-48 bg-gray-200 dark:bg-gray-700 rounded"></div>
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6 space-y-4">
        <div class="h-4 w-full bg-gray-200 dark:bg-gray-700 rounded"></div>
        <div class="h-4 w-3/4 bg-gray-200 dark:bg-gray-700 rounded"></div>
        <div class="h-4 w-1/2 bg-gray-200 dark:bg-gray-700 rounded"></div>
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
      <div class="flex">
        <svg class="h-5 w-5 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Error loading dispute</h3>
          <p class="text-sm text-red-700 dark:text-red-300 mt-1">{{ error }}</p>
        </div>
      </div>
    </div>

    <!-- Dispute Detail -->
    <div v-else-if="dispute" class="space-y-6">
      <!-- Header -->
      <div class="flex items-start justify-between">
        <div>
          <h1 class="text-2xl font-bold text-gray-900 dark:text-white">{{ dispute.reason }}</h1>
          <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
            Filed on {{ formatDate(dispute.createdAt) }}
          </p>
        </div>
        <span 
          class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium"
          :class="statusClass"
        >
          {{ statusLabel }}
        </span>
      </div>

      <!-- Main Info Card -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700">
          <h2 class="text-lg font-medium text-gray-900 dark:text-white">Dispute Details</h2>
        </div>
        <div class="px-6 py-4 space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Category</p>
              <p class="mt-1 text-sm text-gray-900 dark:text-white">{{ categoryLabel }}</p>
            </div>
            <div>
              <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Disputed Amount</p>
              <p class="mt-1 text-lg font-semibold text-gray-900 dark:text-white">{{ formatCurrency(dispute.disputedAmount) }}</p>
            </div>
            <div>
              <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Merchant</p>
              <p class="mt-1 text-sm text-gray-900 dark:text-white">{{ dispute.merchantName || 'N/A' }}</p>
            </div>
            <div>
              <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Transaction Reference</p>
              <p class="mt-1 text-sm text-gray-900 dark:text-white font-mono">{{ dispute.transactionReference || 'N/A' }}</p>
            </div>
          </div>

          <div>
            <p class="text-sm font-medium text-gray-500 dark:text-gray-400">Description</p>
            <p class="mt-1 text-sm text-gray-900 dark:text-white whitespace-pre-wrap">{{ dispute.description }}</p>
          </div>
        </div>
      </div>

      <!-- Timeline Card -->
      <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 overflow-hidden">
        <div class="px-6 py-4 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-700">
          <h2 class="text-lg font-medium text-gray-900 dark:text-white">Timeline</h2>
        </div>
        <div class="px-6 py-4">
          <ol class="relative border-l border-gray-200 dark:border-gray-600 ml-3">
            <li class="mb-6 ml-6">
              <span class="absolute flex items-center justify-center w-6 h-6 bg-primary-100 dark:bg-primary-900 rounded-full -left-3 ring-8 ring-white dark:ring-gray-800">
                <svg class="w-3 h-3 text-primary-600 dark:text-primary-400" fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M6 2a1 1 0 00-1 1v1H4a2 2 0 00-2 2v10a2 2 0 002 2h12a2 2 0 002-2V6a2 2 0 00-2-2h-1V3a1 1 0 10-2 0v1H7V3a1 1 0 00-1-1zm0 5a1 1 0 000 2h8a1 1 0 100-2H6z" clip-rule="evenodd"></path>
                </svg>
              </span>
              <h3 class="flex items-center text-sm font-semibold text-gray-900 dark:text-white">Dispute Filed</h3>
              <time class="block text-xs font-normal text-gray-500 dark:text-gray-400">{{ formatDateTime(dispute.createdAt) }}</time>
            </li>

            <li v-if="dispute.status >= 1" class="mb-6 ml-6">
              <span class="absolute flex items-center justify-center w-6 h-6 bg-blue-100 dark:bg-blue-900 rounded-full -left-3 ring-8 ring-white dark:ring-gray-800">
                <svg class="w-3 h-3 text-blue-600 dark:text-blue-400" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M9 2a1 1 0 000 2h2a1 1 0 100-2H9z"></path>
                  <path fill-rule="evenodd" d="M4 5a2 2 0 012-2 3 3 0 003 3h2a3 3 0 003-3 2 2 0 012 2v11a2 2 0 01-2 2H6a2 2 0 01-2-2V5zm3 4a1 1 0 000 2h.01a1 1 0 100-2H7zm3 0a1 1 0 000 2h3a1 1 0 100-2h-3zm-3 4a1 1 0 100 2h.01a1 1 0 100-2H7zm3 0a1 1 0 100 2h3a1 1 0 100-2h-3z" clip-rule="evenodd"></path>
                </svg>
              </span>
              <h3 class="text-sm font-semibold text-gray-900 dark:text-white">Under Review</h3>
              <time class="block text-xs font-normal text-gray-500 dark:text-gray-400">{{ formatDateTime(dispute.updatedAt) }}</time>
            </li>

            <li v-if="dispute.resolvedAt" class="ml-6">
              <span 
                class="absolute flex items-center justify-center w-6 h-6 rounded-full -left-3 ring-8 ring-white dark:ring-gray-800"
                :class="dispute.status === 3 ? 'bg-green-100 dark:bg-green-900' : 'bg-red-100 dark:bg-red-900'"
              >
                <svg 
                  class="w-3 h-3"
                  :class="dispute.status === 3 ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400'"
                  fill="currentColor" 
                  viewBox="0 0 20 20"
                >
                  <path v-if="dispute.status === 3" fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd"></path>
                  <path v-else fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd"></path>
                </svg>
              </span>
              <h3 class="text-sm font-semibold text-gray-900 dark:text-white">{{ statusLabel }}</h3>
              <time class="block text-xs font-normal text-gray-500 dark:text-gray-400">{{ formatDateTime(dispute.resolvedAt) }}</time>
              <p v-if="dispute.resolutionNotes" class="mt-1 text-sm text-gray-600 dark:text-gray-300">
                {{ dispute.resolutionNotes }}
              </p>
            </li>
          </ol>
        </div>
      </div>

      <!-- Actions - Prominent Cancel Button -->
      <div v-if="canCancel" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
        <div class="flex items-center justify-between">
          <div>
            <h3 class="text-sm font-medium text-red-800 dark:text-red-400">Cancel this dispute?</h3>
            <p class="mt-1 text-sm text-red-600 dark:text-red-500">This action cannot be undone. The dispute will be marked as cancelled.</p>
          </div>
          <button
            @click="openCancelModal"
            :disabled="isCancelling"
            class="inline-flex items-center px-5 py-2.5 border-2 border-red-500 shadow-sm text-sm font-semibold rounded-lg text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 transition-all"
          >
            <svg v-if="isCancelling" class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
            </svg>
            <svg v-else class="-ml-0.5 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
            </svg>
            {{ isCancelling ? 'Cancelling...' : 'Cancel Dispute' }}
          </button>
        </div>
      </div>
    </div>

    <!-- Cancel Confirmation Modal -->
    <ConfirmModal
      :is-open="showCancelModal"
      title="Cancel Dispute"
      :message="cancelModalMessage"
      confirm-text="Cancel Dispute"
      cancel-text="Keep Dispute"
      loading-text="Cancelling..."
      variant="danger"
      :is-loading="isCancelling"
      @confirm="confirmCancel"
      @cancel="closeCancelModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { useDispute, useCancelDispute } from '@/composables'
import { DisputeStatus } from '@/types'
import { format } from 'date-fns'
import ConfirmModal from '@/components/ConfirmModal.vue'

const route = useRoute()
const router = useRouter()

const disputeId = computed(() => route.params.id as string)
const { data: dispute, isLoading, error } = useDispute(disputeId)
const cancelMutation = useCancelDispute()

const isCancelling = computed(() => cancelMutation.isPending.value)

// Cancel modal state
const showCancelModal = ref(false)

const cancelModalMessage = computed(() => {
  if (!dispute.value) return ''
  return `Are you sure you want to cancel the dispute for "${dispute.value.merchantName}"? This action cannot be undone.`
})

const canCancel = computed(() => {
  if (!dispute.value) return false
  return dispute.value.status === DisputeStatus.Pending || 
         dispute.value.status === DisputeStatus.UnderReview
})

const statusLabel = computed(() => {
  if (!dispute.value) return ''
  const statuses: Record<number, string> = {
    0: 'Pending',
    1: 'Under Review',
    2: 'Awaiting Documents',
    3: 'Resolved',
    4: 'Rejected',
    5: 'Cancelled'
  }
  return statuses[dispute.value.status] || 'Unknown'
})

const statusClass = computed(() => {
  if (!dispute.value) return ''
  const classes: Record<number, string> = {
    0: 'bg-yellow-100 text-yellow-800',
    1: 'bg-blue-100 text-blue-800',
    2: 'bg-orange-100 text-orange-800',
    3: 'bg-green-100 text-green-800',
    4: 'bg-red-100 text-red-800',
    5: 'bg-gray-100 text-gray-800'
  }
  return classes[dispute.value.status] || 'bg-gray-100 text-gray-800'
})

const categoryLabel = computed(() => {
  if (!dispute.value) return ''
  const categories: Record<number, string> = {
    0: 'Unauthorized Transaction',
    1: 'Duplicate Charge',
    2: 'Incorrect Amount',
    3: 'Service Not Received',
    4: 'Product Not Received',
    5: 'Quality Issue',
    6: 'Refund Not Received',
    7: 'Fraud Suspected',
    8: 'Other'
  }
  return categories[dispute.value.category] || 'Other'
})

function formatCurrency(amount: number): string {
  return new Intl.NumberFormat('en-ZA', {
    style: 'currency',
    currency: 'ZAR'
  }).format(amount)
}

function formatDate(date: string): string {
  return format(new Date(date), 'dd MMMM yyyy')
}

function formatDateTime(date: string): string {
  return format(new Date(date), 'dd MMM yyyy, HH:mm')
}

function openCancelModal() {
  showCancelModal.value = true
}

function closeCancelModal() {
  showCancelModal.value = false
}

function confirmCancel() {
  if (!dispute.value) return
  
  cancelMutation.mutate(dispute.value.id, {
    onSuccess: () => {
      closeCancelModal()
      router.push('/disputes')
    }
  })
}
</script>
