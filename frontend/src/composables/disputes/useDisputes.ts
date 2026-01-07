import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { ref, computed, type Ref, type ComputedRef } from 'vue'
import * as api from '@/features/disputes/api/disputesApi'
import type { DisputeQuery, CreateDispute } from '@/features/disputes/api/disputesApi'
import { transactionKeys } from '../transactions'

export const disputeKeys = {
  all: ['disputes'] as const,
  lists: () => [...disputeKeys.all, 'list'] as const,
  list: (params: DisputeQuery) => [...disputeKeys.lists(), params] as const,
  details: () => [...disputeKeys.all, 'detail'] as const,
  detail: (id: string) => [...disputeKeys.details(), id] as const,
  statistics: () => [...disputeKeys.all, 'statistics'] as const,
  byTransaction: (transactionId: string) => [...disputeKeys.all, 'transaction', transactionId] as const,
}

const DEFAULT_CUSTOMER_ID = 'CUST001'

const filters = ref<DisputeQuery>({
  customerId: DEFAULT_CUSTOMER_ID,
  page: 1,
  pageSize: 20,
})

export function useDisputeFilters() {
  const setFilters = (newFilters: Partial<DisputeQuery>) => {
    filters.value = { ...filters.value, ...newFilters }
  }

  const resetFilters = () => {
    filters.value = { customerId: DEFAULT_CUSTOMER_ID, page: 1, pageSize: 20 }
  }

  const nextPage = () => { filters.value.page = (filters.value.page ?? 1) + 1 }
  const previousPage = () => { if ((filters.value.page ?? 1) > 1) filters.value.page = (filters.value.page ?? 1) - 1 }

  return { filters, setFilters, resetFilters, nextPage, previousPage }
}

export function useDisputes() {
  const { filters, setFilters, resetFilters, nextPage, previousPage } = useDisputeFilters()

  const query = useQuery({
    queryKey: computed(() => disputeKeys.list(filters.value)),
    queryFn: () => api.getDisputes(filters.value),
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
    disputes: computed(() => query.data.value?.items ?? []),
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

export function useDispute(id: Ref<string> | ComputedRef<string> | string) {
  const disputeId = computed(() => typeof id === 'string' ? id : id.value)
  return useQuery({
    queryKey: computed(() => disputeKeys.detail(disputeId.value)),
    queryFn: () => api.getDisputeById(disputeId.value),
    enabled: computed(() => !!disputeId.value),
  })
}

export function useDisputeStatistics() {
  return useQuery({
    queryKey: disputeKeys.statistics(),
    queryFn: () => api.getStatistics(),
    staleTime: 60000,
  })
}

export function useDisputesByTransaction(transactionId: string) {
  return useQuery({
    queryKey: disputeKeys.byTransaction(transactionId),
    queryFn: () => api.getDisputesByTransaction(transactionId),
    enabled: !!transactionId,
  })
}

export function useCreateDispute() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: (data: CreateDispute) => api.createDispute(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.lists() })
      queryClient.invalidateQueries({ queryKey: disputeKeys.statistics() })
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() })
    },
  })
}

export function useCancelDispute() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.cancelDispute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.all })
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() })
    },
  })
}

export function useUpdateDisputeDescription() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: ({ id, description }: { id: string; description: string }) => api.updateDescription(id, description),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: disputeKeys.all }),
  })
}

export function useDeleteDispute() {
  const queryClient = useQueryClient()
  return useMutation({
    mutationFn: api.deleteDispute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.all })
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() })
    },
  })
}
