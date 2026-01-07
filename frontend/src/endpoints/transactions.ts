/**
 * Transaction Endpoints
 * 
 * All transaction-related API calls with Zod validation.
 * Handles both real API and demo mode seamlessly.
 */
import { z } from 'zod'
import { get, post } from './client'
import { isDemoMode, mockTransactionApi } from '@/services/mockApi'

// ─────────────────────────────────────────────────────────────────────────────
// Enums & Constants
// ─────────────────────────────────────────────────────────────────────────────

export enum TransactionType {
  Debit = 0,
  Credit = 1,
}

export enum TransactionStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Reversed = 3,
}

// ─────────────────────────────────────────────────────────────────────────────
// Schemas
// ─────────────────────────────────────────────────────────────────────────────

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

export const TransactionQueryParamsSchema = z.object({
  customerId: z.string().optional(),
  fromDate: z.string().optional(),
  toDate: z.string().optional(),
  category: z.string().optional(),
  type: z.nativeEnum(TransactionType).optional(),
  minAmount: z.number().optional(),
  maxAmount: z.number().optional(),
  page: z.number().int().positive().optional(),
  pageSize: z.number().int().positive().max(100).optional(),
  searchTerm: z.string().optional(),
})

export const PagedResultSchema = <T extends z.ZodTypeAny>(itemSchema: T) =>
  z.object({
    items: z.array(itemSchema),
    totalCount: z.number(),
    page: z.number(),
    pageSize: z.number(),
    totalPages: z.number(),
    hasNextPage: z.boolean(),
    hasPreviousPage: z.boolean(),
  })

export const TransactionPagedResultSchema = PagedResultSchema(TransactionSchema)

// ─────────────────────────────────────────────────────────────────────────────
// Types (inferred from schemas)
// ─────────────────────────────────────────────────────────────────────────────

export type Transaction = z.infer<typeof TransactionSchema>
export type TransactionQueryParams = z.infer<typeof TransactionQueryParamsSchema>
export type TransactionPagedResult = z.infer<typeof TransactionPagedResultSchema>

// ─────────────────────────────────────────────────────────────────────────────
// API Functions
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Get paginated list of transactions with optional filters
 */
export async function getTransactions(
  params?: TransactionQueryParams
): Promise<TransactionPagedResult> {
  const validated = params ? TransactionQueryParamsSchema.parse(params) : undefined
  
  if (isDemoMode()) {
    const response = await mockTransactionApi.getTransactions(validated)
    return TransactionPagedResultSchema.parse(response)
  }
  
  const response = await get<TransactionPagedResult>('/transactions', { params: validated })
  return TransactionPagedResultSchema.parse(response)
}

/**
 * Get single transaction by ID
 */
export async function getTransactionById(id: string): Promise<Transaction> {
  if (!id) throw new Error('Transaction ID is required')
  
  if (isDemoMode()) {
    const response = await mockTransactionApi.getTransactionById(id)
    return TransactionSchema.parse(response)
  }
  
  const response = await get<Transaction>(`/transactions/${id}`)
  return TransactionSchema.parse(response)
}

/**
 * Get all transactions for a customer
 */
export async function getTransactionsByCustomer(customerId: string): Promise<Transaction[]> {
  if (!customerId) throw new Error('Customer ID is required')
  
  if (isDemoMode()) {
    const result = await mockTransactionApi.getTransactions({ pageSize: 100 })
    return z.array(TransactionSchema).parse(result.items)
  }
  
  const response = await get<Transaction[]>(`/transactions/customer/${customerId}`)
  return z.array(TransactionSchema).parse(response)
}

/**
 * Get available transaction categories
 */
export async function getCategories(): Promise<string[]> {
  if (isDemoMode()) {
    return mockTransactionApi.getCategories()
  }
  
  return get<string[]>('/transactions/categories')
}

/**
 * Seed demo transactions (dev only)
 */
export async function seedTransactions(): Promise<Transaction[]> {
  if (isDemoMode()) {
    const response = await mockTransactionApi.seedTransactions()
    return z.array(TransactionSchema).parse(response)
  }
  
  const response = await post<Transaction[]>('/transactions/seed')
  return z.array(TransactionSchema).parse(response)
}
