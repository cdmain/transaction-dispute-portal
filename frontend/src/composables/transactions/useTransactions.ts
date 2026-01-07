import { useQuery } from '@tanstack/vue-query'
import { ref, computed } from 'vue'
import * as api from '@/features/transactions/api/transactionsApi'
import type { TransactionQuery } from '@/features/transactions/api/transactionsApi'

export const transactionKeys = {
  all: ['transactions'] as const,
  lists: () => [...transactionKeys.all, 'list'] as const,
  list: (params: TransactionQuery) => [...transactionKeys.lists(), params] as const,
  details: () => [...transactionKeys.all, 'detail'] as const,
  detail: (id: string) => [...transactionKeys.details(), id] as const,
  categories: () => [...transactionKeys.all, 'categories'] as const,
}

const DEFAULT_CUSTOMER_ID = 'CUST001'

const filters = ref<TransactionQuery>({
  customerId: DEFAULT_CUSTOMER_ID,
  page: 1,
  pageSize: 20,
})

export function useTransactionFilters() {
  const setFilters = (newFilters: Partial<TransactionQuery>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = { customerId: DEFAULT_CUSTOMER_ID, page: 1, pageSize: 20 }
  }

  const nextPage = () => { filters.value.page = (filters.value.page ?? 1) + 1 }
  const previousPage = () => { if ((filters.value.page ?? 1) > 1) filters.value.page = (filters.value.page ?? 1) - 1 }

  return { filters, setFilters, resetFilters, nextPage, previousPage }
}

export function useTransactions() {
  const { filters, setFilters, resetFilters, nextPage, previousPage } = useTransactionFilters()

  const query = useQuery({
    queryKey: computed(() => transactionKeys.list(filters.value)),
    queryFn: () => api.getTransactions(filters.value),
    staleTime: 30000,
  })

  const pagination = computed(() => ({
    page: query.data.value?.page ?? 1,
    pageSize: query.data.value?.pageSize ?? 20,
    totalCount: query.data.value?.totalCount ?? 0,
    totalPages: query.data.value?.totalPages ?? 0,
  }))

  const hasMore = computed(() => pagination.value.page < pagination.value.totalPages)

  return {
    transactions: computed(() => query.data.value?.items ?? []),
    isLoading: query.isLoading,
    error: computed(() => query.error.value?.message ?? null),
    pagination,
    hasMore,
    filters,
    setFilters,
    resetFilters,
    nextPage,
    previousPage,
    refetch: query.refetch,
  }
}

export function useTransaction(id: string) {
  return useQuery({
    queryKey: transactionKeys.detail(id),
    queryFn: () => api.getTransactionById(id),
    enabled: !!id,
  })
}

export function useTransactionCategories() {
  return useQuery({
    queryKey: transactionKeys.categories(),
    queryFn: api.getCategories,
    staleTime: 300000,
  })
}
