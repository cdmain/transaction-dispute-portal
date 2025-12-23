<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900 dark:text-white">Transactions</h1>
        <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">
          View your transactions and file disputes
        </p>
      </div>
      <div class="text-sm text-gray-500 dark:text-gray-400">
        {{ pagination.totalCount }} transactions
      </div>
    </div>

    <!-- Filters -->
    <TransactionFilters
      :categories="categories ?? []"
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
          <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Error loading transactions</h3>
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
    <div v-else-if="transactions.length === 0" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400 dark:text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900 dark:text-white">No transactions</h3>
      <p class="mt-1 text-sm text-gray-500 dark:text-gray-400">No transactions found for your account.</p>
      <button
        @click="seedTransactions"
        :disabled="isSeeding"
        class="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
      >
        <svg v-if="isSeeding" class="animate-spin -ml-1 mr-2 h-4 w-4 text-white" fill="none" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
        </svg>
        <svg v-else class="-ml-0.5 mr-2 h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
        </svg>
        {{ isSeeding ? 'Generating...' : 'Generate Demo Transactions' }}
      </button>
    </div>

    <!-- Transaction List -->
    <div v-else class="space-y-4">
      <TransactionCard
        v-for="transaction in transactions"
        :key="transaction.id"
        :transaction="transaction"
        @dispute="openDisputeModal"
        @viewHistory="openHistoryModal"
      />
    </div>

    <!-- Pagination -->
    <Pagination
      v-if="transactions.length > 0"
      :page="pagination.page"
      :page-size="pagination.pageSize"
      :total-count="pagination.totalCount"
      :total-pages="pagination.totalPages"
      :has-next-page="hasMore"
      :has-previous-page="pagination.page > 1"
      @next="nextPage"
      @previous="previousPage"
    />

    <!-- Dispute Modal -->
    <DisputeModal
      :is-open="isModalOpen"
      :transaction="selectedTransaction"
      @close="closeDisputeModal"
      @success="handleDisputeSuccess"
    />

    <!-- Dispute History Modal -->
    <DisputeHistoryModal
      :is-open="isHistoryModalOpen"
      :transaction="historyTransaction"
      @close="closeHistoryModal"
      @redispute="handleRedispute"
    />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useTransactions, useTransactionCategories } from '@/composables'
import type { Transaction, TransactionQueryParams } from '@/types'
import TransactionCard from '@/components/TransactionCard.vue'
import TransactionFilters from '@/components/TransactionFilters.vue'
import DisputeModal from '@/components/DisputeModal.vue'
import DisputeHistoryModal from '@/components/DisputeHistoryModal.vue'
import Pagination from '@/components/Pagination.vue'
import LoadingSkeleton from '@/components/LoadingSkeleton.vue'
import { transactionApi } from '@/services/api'

const {
  transactions,
  isLoading,
  error,
  pagination,
  hasMore,
  setFilters,
  resetFilters,
  nextPage,
  previousPage,
  refetch
} = useTransactions()

const { data: categories } = useTransactionCategories()

const isModalOpen = ref(false)
const selectedTransaction = ref<Transaction | null>(null)
const isHistoryModalOpen = ref(false)
const historyTransaction = ref<Transaction | null>(null)
const isSeeding = ref(false)

function handleFilter(params: Partial<TransactionQueryParams>) {
  setFilters({ ...params, page: 1 })
}

function handleReset() {
  resetFilters()
}

async function seedTransactions() {
  isSeeding.value = true
  try {
    await transactionApi.seedTransactions()
    refetch()
  } catch (e) {
    console.error('Failed to seed transactions:', e)
  } finally {
    isSeeding.value = false
  }
}

function openDisputeModal(transaction: Transaction) {
  selectedTransaction.value = transaction
  isModalOpen.value = true
}

function closeDisputeModal() {
  isModalOpen.value = false
  selectedTransaction.value = null
}

function handleDisputeSuccess() {
  // TanStack Query will automatically invalidate and refetch
}

function openHistoryModal(transaction: Transaction) {
  historyTransaction.value = transaction
  isHistoryModalOpen.value = true
}

function closeHistoryModal() {
  isHistoryModalOpen.value = false
  historyTransaction.value = null
}

function handleRedispute(transaction: Transaction) {
  closeHistoryModal()
  openDisputeModal(transaction)
}
</script>
