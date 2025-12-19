<template>
  <div class="space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between">
      <div>
        <h1 class="text-2xl font-bold text-gray-900">Transactions</h1>
        <p class="mt-1 text-sm text-gray-500">
          View your transactions and file disputes
        </p>
      </div>
      <div class="text-sm text-gray-500">
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
    <div v-else-if="error" class="bg-red-50 border border-red-200 rounded-lg p-4">
      <div class="flex">
        <svg class="h-5 w-5 text-red-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
        </svg>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-red-800">Error loading transactions</h3>
          <p class="text-sm text-red-700 mt-1">{{ error }}</p>
          <button 
            @click="refetch()"
            class="mt-2 text-sm font-medium text-red-600 hover:text-red-500"
          >
            Try again
          </button>
        </div>
      </div>
    </div>

    <!-- Empty State -->
    <div v-else-if="transactions.length === 0" class="text-center py-12">
      <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 10h18M7 15h1m4 0h1m-7 4h12a3 3 0 003-3V8a3 3 0 00-3-3H6a3 3 0 00-3 3v8a3 3 0 003 3z" />
      </svg>
      <h3 class="mt-2 text-sm font-medium text-gray-900">No transactions</h3>
      <p class="mt-1 text-sm text-gray-500">No transactions found matching your criteria.</p>
    </div>

    <!-- Transaction List -->
    <div v-else class="space-y-4">
      <TransactionCard
        v-for="transaction in transactions"
        :key="transaction.id"
        :transaction="transaction"
        @dispute="openDisputeModal"
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
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useTransactions, useTransactionCategories } from '@/composables'
import type { Transaction, TransactionQueryParams } from '@/types'
import TransactionCard from '@/components/TransactionCard.vue'
import TransactionFilters from '@/components/TransactionFilters.vue'
import DisputeModal from '@/components/DisputeModal.vue'
import Pagination from '@/components/Pagination.vue'
import LoadingSkeleton from '@/components/LoadingSkeleton.vue'

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

function handleFilter(params: Partial<TransactionQueryParams>) {
  setFilters({ ...params, page: 1 })
}

function handleReset() {
  resetFilters()
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
</script>
