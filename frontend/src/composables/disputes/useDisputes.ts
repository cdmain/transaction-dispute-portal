/**
 * Dispute Composables
 * 
 * TanStack Query hooks for dispute data.
 * Uses the typed endpoints for API calls.
 */
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'
import { ref, computed, type Ref, type ComputedRef } from 'vue'
import * as disputeApi from '@/endpoints/disputes'
import type { DisputeQueryParams, CreateDisputeRequest } from '@/endpoints/disputes'
import { transactionKeys } from '../transactions'

// ─────────────────────────────────────────────────────────────────────────────
// Query Keys
// ─────────────────────────────────────────────────────────────────────────────

export const disputeKeys = {
  all: ['disputes'] as const,
  lists: () => [...disputeKeys.all, 'list'] as const,
  list: (params: DisputeQueryParams) => [...disputeKeys.lists(), params] as const,
  details: () => [...disputeKeys.all, 'detail'] as const,
  detail: (id: string) => [...disputeKeys.details(), id] as const,
  statistics: (customerId?: string) => [...disputeKeys.all, 'statistics', customerId] as const,
  byTransaction: (transactionId: string) => [...disputeKeys.all, 'transaction', transactionId] as const,
}

// Default customer ID for demo
const DEFAULT_CUSTOMER_ID = 'CUST001'

// ─────────────────────────────────────────────────────────────────────────────
// Filter State
// ─────────────────────────────────────────────────────────────────────────────

const filters = ref<DisputeQueryParams>({
  customerId: DEFAULT_CUSTOMER_ID,
  page: 1,
  pageSize: 20,
})

export function useDisputeFilters() {
  const setFilters = (newFilters: Partial<DisputeQueryParams>) => {
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
    filters.value.page = (filters.value.page ?? 1) + 1
  }

  const previousPage = () => {
    if ((filters.value.page ?? 1) > 1) {
      filters.value.page = (filters.value.page ?? 1) - 1
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

// ─────────────────────────────────────────────────────────────────────────────
// Queries
// ─────────────────────────────────────────────────────────────────────────────

export function useDisputes() {
  const { filters, setFilters, resetFilters, nextPage, previousPage } = useDisputeFilters()

  const query = useQuery({
    queryKey: computed(() => disputeKeys.list(filters.value)),
    queryFn: () => disputeApi.getDisputes(filters.value),
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
    queryFn: () => disputeApi.getDisputeById(disputeId.value),
    enabled: computed(() => !!disputeId.value),
  })
}

export function useDisputeStatistics(customerId?: string) {
  return useQuery({
    queryKey: disputeKeys.statistics(customerId),
    queryFn: () => disputeApi.getStatistics(customerId),
    staleTime: 60000, // 1 minute
  })
}

export function useDisputesByTransaction(transactionId: string) {
  return useQuery({
    queryKey: disputeKeys.byTransaction(transactionId),
    queryFn: () => disputeApi.getDisputesByTransaction(transactionId),
    enabled: !!transactionId,
  })
}

// ─────────────────────────────────────────────────────────────────────────────
// Mutations
// ─────────────────────────────────────────────────────────────────────────────

export function useCreateDispute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: (data: CreateDisputeRequest) => disputeApi.createDispute(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.lists() })
      queryClient.invalidateQueries({ queryKey: disputeKeys.statistics(variables.customerId) })
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() })
    },
  })
}

export function useCancelDispute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: disputeApi.cancelDispute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.all })
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() })
    },
  })
}

export function useUpdateDisputeDescription() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: ({ id, description }: { id: string; description: string }) => 
      disputeApi.updateDisputeDescription(id, description),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.all })
    },
  })
}

export function useDeleteDispute() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: disputeApi.deleteDispute,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: disputeKeys.all })
      queryClient.invalidateQueries({ queryKey: transactionKeys.lists() })
    },
  })
}
