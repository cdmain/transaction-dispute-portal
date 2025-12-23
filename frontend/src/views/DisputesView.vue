<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Dispute History</h1>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          View and manage your disputed transactions
        </p>
      </div>
      <div class="text-sm text-gray-500 dark:text-gray-400">
        {{ pagination.totalCount }} disputes
      </div>
    </div>

    <!-- Statistics Summary -->
    <div v-if="statistics" class="grid grid-cols-2 md:grid-cols-4 gap-4">
      <div class="bg-yellow-50 dark:bg-yellow-900/20 rounded-lg p-4 border border-yellow-200 dark:border-yellow-800">
        <p class="text-sm font-medium text-yellow-800 dark:text-yellow-200">Pending</p>
        <p class="text-2xl font-bold text-yellow-900 dark:text-yellow-100">{{ statistics.pendingDisputes }}</p>
      </div>
      <div class="bg-blue-50 dark:bg-blue-900/20 rounded-lg p-4 border border-blue-200 dark:border-blue-800">
        <p class="text-sm font-medium text-blue-800 dark:text-blue-200">Under Review</p>
        <p class="text-2xl font-bold text-blue-900 dark:text-blue-100">{{ statistics.underReviewDisputes }}</p>
      </div>
      <div class="bg-green-50 dark:bg-green-900/20 rounded-lg p-4 border border-green-200 dark:border-green-800">
        <p class="text-sm font-medium text-green-800 dark:text-green-200">Resolved</p>
        <p class="text-2xl font-bold text-green-900 dark:text-green-100">{{ statistics.resolvedDisputes }}</p>
      </div>
      <div class="bg-red-50 dark:bg-red-900/20 rounded-lg p-4 border border-red-200 dark:border-red-800">
        <p class="text-sm font-medium text-red-800 dark:text-red-200">Rejected</p>
        <p class="text-2xl font-bold text-red-900 dark:text-red-100">{{ statistics.rejectedDisputes }}</p>
      </div>
    </div>

    <!-- Filters -->
    <DisputeFilters
      @filter="handleFilter"
      @reset="handleReset"
    />

    <!-- Loading State -->
    <div v-if="isLoading" class="space-y-4">
      <LoadingSkeleton v-for="i in 5" :key="i" />
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
      <div class="flex">
        <svg class="h-5 w-5 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Error loading disputes</h3>
          <p class="text-sm text-red-700 dark:text-red-300 mt-1">{{ error }}</p>
          <button 
            @click="refetch()"
            class="mt-2 text-sm font-medium text-red-600 dark:text-red-400 hover:text-red-500"
          >
            Try again
          </button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="disputes.length === 0" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No disputes</h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">You haven't filed any disputes yet.</p>
      <RouterLink
        to="/transactions"
        class="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700"
      >
        View Transactions
      </RouterLink>
    </div>

    <!-- Dispute List -->
    <div v-else class="space-y-4">
      <DisputeCard
        v-for="dispute in disputes"
        :key="dispute.id"
        :dispute="dispute"
        @click="viewDispute"
        @cancel="handleCancel"
        @edit="handleEdit"
        @delete="handleDelete"
      />
    </div>

    <!-- Pagination -->
    <Pagination
      v-if="disputes.length > 0"
      :page="pagination.page"
      :page-size="pagination.pageSize"
      :total-count="pagination.totalCount"
      :total-pages="pagination.totalPages"
      :has-next-page="hasMore"
      :has-previous-page="pagination.page > 1"
      @next="nextPage"
      @previous="previousPage"
    />

    <!-- Cancel Confirmation Modal -->
    <ConfirmModal
      :is-open="showCancelModal"
      title="Cancel Dispute"
      :message="cancelModalMessage"
      confirm-text="Cancel Dispute"
      cancel-text="Keep Dispute"
      loading-text="Cancelling..."
      variant="danger"
      :is-loading="cancelDisputeMutation.isPending.value"
      @confirm="confirmCancel"
      @cancel="closeCancelModal"
    />

    <!-- Delete Confirmation Modal -->
    <ConfirmModal
      :is-open="showDeleteModal"
      title="Delete Dispute"
      :message="deleteModalMessage"
      confirm-text="Delete Dispute"
      cancel-text="Keep Dispute"
      loading-text="Deleting..."
      variant="danger"
      :is-loading="deleteDisputeMutation.isPending.value"
      @confirm="confirmDelete"
      @cancel="closeDeleteModal"
    />

    <!-- Edit Description Modal -->
    <EditDescriptionModal
      :is-open="showEditModal"
      :description="editingDispute?.description || ''"
      :is-loading="updateDescriptionMutation.isPending.value"
      @save="confirmEdit"
      @cancel="closeEditModal"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useDisputes, useDisputeStatistics, useCancelDispute, useUpdateDisputeDescription, useDeleteDispute } from '@/composables'
import type { Dispute, DisputeQueryParams } from '@/types'
import DisputeCard from '@/components/DisputeCard.vue'
import DisputeFilters from '@/components/DisputeFilters.vue'
import Pagination from '@/components/Pagination.vue'
import LoadingSkeleton from '@/components/LoadingSkeleton.vue'
import ConfirmModal from '@/components/ConfirmModal.vue'
import EditDescriptionModal from '@/components/EditDescriptionModal.vue'

const router = useRouter()

const {
  disputes,
  isLoading,
  error,
  pagination,
  hasMore,
  setFilters,
  resetFilters,
  nextPage,
  previousPage,
  refetch
} = useDisputes()

const { data: statistics } = useDisputeStatistics('CUST001')
const cancelDisputeMutation = useCancelDispute()
const updateDescriptionMutation = useUpdateDisputeDescription()
const deleteDisputeMutation = useDeleteDispute()

// Cancel modal state
const showCancelModal = ref(false)
const disputeToCancel = ref<Dispute | null>(null)

// Edit modal state
const showEditModal = ref(false)
const editingDispute = ref<Dispute | null>(null)

// Delete modal state
const showDeleteModal = ref(false)
const disputeToDelete = ref<Dispute | null>(null)

const cancelModalMessage = computed(() => {
  if (!disputeToCancel.value) return ''
  return `Are you sure you want to cancel the dispute for "${disputeToCancel.value.merchantName}"? This action cannot be undone.`
})

const deleteModalMessage = computed(() => {
  if (!disputeToDelete.value) return ''
  return `Are you sure you want to permanently delete the dispute for "${disputeToDelete.value.merchantName}"? This action cannot be undone.`
})

function handleFilter(params: Partial<DisputeQueryParams>) {
  setFilters({ ...params, page: 1 })
}

function handleReset() {
  resetFilters()
}

function viewDispute(dispute: Dispute) {
  router.push(`/disputes/${dispute.id}`)
}

// Cancel handlers
function handleCancel(dispute: Dispute) {
  disputeToCancel.value = dispute
  showCancelModal.value = true
}

async function confirmCancel() {
  if (disputeToCancel.value) {
    await cancelDisputeMutation.mutateAsync(disputeToCancel.value.id)
    closeCancelModal()
  }
}

function closeCancelModal() {
  showCancelModal.value = false
  disputeToCancel.value = null
}

// Edit handlers
function handleEdit(dispute: Dispute) {
  editingDispute.value = dispute
  showEditModal.value = true
}

async function confirmEdit(description: string) {
  if (editingDispute.value) {
    await updateDescriptionMutation.mutateAsync({ 
      id: editingDispute.value.id, 
      description 
    })
    closeEditModal()
  }
}

function closeEditModal() {
  showEditModal.value = false
  editingDispute.value = null
}

// Delete handlers
function handleDelete(dispute: Dispute) {
  disputeToDelete.value = dispute
  showDeleteModal.value = true
}

async function confirmDelete() {
  if (disputeToDelete.value) {
    await deleteDisputeMutation.mutateAsync(disputeToDelete.value.id)
    closeDeleteModal()
  }
}

function closeDeleteModal() {
  showDeleteModal.value = false
  disputeToDelete.value = null
}
</script>
