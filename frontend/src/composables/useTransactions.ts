import { useQuery } from '@tanstack/vue-query'
import { ref, computed } from 'vue'
import type { TransactionQueryParams } from '@/types'
import { transactionApi } from '@/services/api'

// Query keys
export const transactionKeys = {
  all: ['transactions'] as const,
  lists: () => [...transactionKeys.all, 'list'] as const,
  list: (params: TransactionQueryParams) => [...transactionKeys.lists(), params] as const,
  details: () => [...transactionKeys.all, 'detail'] as const,
  detail: (id: string) => [...transactionKeys.details(), id] as const,
  categories: () => [...transactionKeys.all, 'categories'] as const,
}

// Default customer ID for demo
const DEFAULT_CUSTOMER_ID = 'CUST001'

// Reactive filters state
const filters = ref<TransactionQueryParams>({
  customerId: DEFAULT_CUSTOMER_ID,
  page: 1,
  pageSize: 20,
})

export function useTransactionFilters() {
  const setFilters = (newFilters: Partial<TransactionQueryParams>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = {
      customerId: DEFAULT_CUSTOMER_ID,
      page: 1,
      pageSize: 20,
    }
  }

  const nextPage = () => {
    filters.value.page = (filters.value.page || 1) + 1
  }

  const previousPage = () => {
    if ((filters.value.page || 1) > 1) {
      filters.value.page = (filters.value.page || 1) - 1
    }
  }

  return {
    filters,
    setFilters,
    resetFilters,
    nextPage,
    previousPage,
  }
}

export function useTransactions() {
  const { filters, setFilters, resetFilters, nextPage, previousPage } = useTransactionFilters()

  const query = useQuery({
    queryKey: computed(() => transactionKeys.list(filters.value)),
    queryFn: () => transactionApi.getTransactions(filters.value),
    staleTime: 30000, // 30 seconds
  })

  const pagination = computed(() => {
    const data = query.data.value
    return {
      page: data?.page ?? 1,
      pageSize: data?.pageSize ?? 20,
      totalCount: data?.totalCount ?? 0,
      totalPages: data?.totalPages ?? 0,
    }
  })

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
    queryFn: () => transactionApi.getTransactionById(id),
    enabled: !!id,
  })
}

export function useTransactionCategories() {
  return useQuery({
    queryKey: transactionKeys.categories(),
    queryFn: () => transactionApi.getCategories(),
    staleTime: 300000, // 5 minutes
  })
}
