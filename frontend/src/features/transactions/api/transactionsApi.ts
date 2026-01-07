import { z } from 'zod'
import { get, post } from '@/shared/api/client'
import { isDemoMode, mockTransactionApi } from '@/services/mockApi'

// Enums
export enum TransactionType { Debit = 0, Credit = 1 }
export enum TransactionStatus { Pending = 0, Completed = 1, Failed = 2, Reversed = 3 }

// Schemas
export const TransactionSchema = z.object({
  id: z.string(),
  customerId: z.string(),
  description: z.string(),
  amount: z.number(),
  currency: z.string(),
  category: z.string(),
  merchantName: z.string(),
  merchantCategory: z.string(),
  type: z.nativeEnum(TransactionType),
  status: z.nativeEnum(TransactionStatus),
  transactionDate: z.string(),
  createdAt: z.string(),
  reference: z.string().nullable().optional(),
  cardLastFourDigits: z.string().nullable().optional(),
  isDisputed: z.boolean(),
})

export const TransactionQuerySchema = z.object({
  customerId: z.string().optional(),
  fromDate: z.string().optional(),
  toDate: z.string().optional(),
  category: z.string().optional(),
  type: z.nativeEnum(TransactionType).optional(),
  minAmount: z.number().optional(),
  maxAmount: z.number().optional(),
  page: z.number().optional(),
  pageSize: z.number().optional(),
  searchTerm: z.string().optional(),
})

export const PagedResultSchema = z.object({
  items: z.array(TransactionSchema),
  totalCount: z.number(),
  page: z.number(),
  pageSize: z.number(),
  totalPages: z.number(),
  hasNextPage: z.boolean(),
  hasPreviousPage: z.boolean(),
})

// Types
export type Transaction = z.infer<typeof TransactionSchema>
export type TransactionQuery = z.infer<typeof TransactionQuerySchema>
export type PagedResult = z.infer<typeof PagedResultSchema>

// API
export async function getTransactions(params?: TransactionQuery): Promise<PagedResult> {
  const validated = params ? TransactionQuerySchema.parse(params) : undefined
  if (isDemoMode()) {
    return PagedResultSchema.parse(await mockTransactionApi.getTransactions(validated))
  }
  return PagedResultSchema.parse(await get<PagedResult>('/transactions', { params: validated }))
}

export async function getTransactionById(id: string): Promise<Transaction> {
  if (isDemoMode()) {
    return TransactionSchema.parse(await mockTransactionApi.getTransactionById(id))
  }
  return TransactionSchema.parse(await get<Transaction>(`/transactions/${id}`))
}

export async function getCategories(): Promise<string[]> {
  if (isDemoMode()) return mockTransactionApi.getCategories()
  return get<string[]>('/transactions/categories')
}

export async function seedTransactions(): Promise<Transaction[]> {
  if (isDemoMode()) {
    return z.array(TransactionSchema).parse(await mockTransactionApi.seedTransactions())
  }
  return z.array(TransactionSchema).parse(await post<Transaction[]>('/transactions/seed'))
}
