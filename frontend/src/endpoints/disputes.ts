/**
 * Dispute Endpoints
 * 
 * All dispute-related API calls with Zod validation.
 * Includes full CRUD operations and status management.
 */
import { z } from 'zod'
import { get, post, put, del } from './client'
import { isDemoMode, mockDisputeApi } from '@/services/mockApi'

// ─────────────────────────────────────────────────────────────────────────────
// Enums & Constants
// ─────────────────────────────────────────────────────────────────────────────

export enum DisputeStatus {
  Pending = 0,
  UnderReview = 1,
  AwaitingDocuments = 2,
  Resolved = 3,
  Rejected = 4,
  Cancelled = 5,
}

export enum DisputeCategory {
  UnauthorizedTransaction = 0,
  DuplicateCharge = 1,
  IncorrectAmount = 2,
  ServiceNotReceived = 3,
  ProductNotReceived = 4,
  QualityIssue = 5,
  RefundNotReceived = 6,
  FraudSuspected = 7,
  Other = 8,
}

export const DISPUTE_STATUS_LABELS: Record<DisputeStatus, string> = {
  [DisputeStatus.Pending]: 'Pending',
  [DisputeStatus.UnderReview]: 'Under Review',
  [DisputeStatus.AwaitingDocuments]: 'Awaiting Documents',
  [DisputeStatus.Resolved]: 'Resolved',
  [DisputeStatus.Rejected]: 'Rejected',
  [DisputeStatus.Cancelled]: 'Cancelled',
}

export const DISPUTE_CATEGORY_LABELS: Record<DisputeCategory, string> = {
  [DisputeCategory.UnauthorizedTransaction]: 'Unauthorized Transaction',
  [DisputeCategory.DuplicateCharge]: 'Duplicate Charge',
  [DisputeCategory.IncorrectAmount]: 'Incorrect Amount',
  [DisputeCategory.ServiceNotReceived]: 'Service Not Received',
  [DisputeCategory.ProductNotReceived]: 'Product Not Received',
  [DisputeCategory.QualityIssue]: 'Quality Issue',
  [DisputeCategory.RefundNotReceived]: 'Refund Not Received',
  [DisputeCategory.FraudSuspected]: 'Fraud Suspected',
  [DisputeCategory.Other]: 'Other',
}

// ─────────────────────────────────────────────────────────────────────────────
// Schemas
// ─────────────────────────────────────────────────────────────────────────────

export const DisputeSchema = z.object({
  id: z.string(),
  transactionId: z.string(),
  customerId: z.string(),
  reason: z.string(),
  description: z.string(),
  status: z.nativeEnum(DisputeStatus),
  category: z.nativeEnum(DisputeCategory),
  disputedAmount: z.number(),
  currency: z.string(),
  createdAt: z.string(),
  updatedAt: z.string(),
  resolvedAt: z.string().nullable().optional(),
  resolutionNotes: z.string().nullable().optional(),
  transactionReference: z.string().nullable().optional(),
  merchantName: z.string().nullable().optional(),
})

export const DisputeQueryParamsSchema = z.object({
  customerId: z.string().optional(),
  transactionId: z.string().optional(),
  status: z.nativeEnum(DisputeStatus).optional(),
  category: z.nativeEnum(DisputeCategory).optional(),
  fromDate: z.string().optional(),
  toDate: z.string().optional(),
  page: z.number().int().positive().optional(),
  pageSize: z.number().int().positive().max(100).optional(),
  searchTerm: z.string().optional(),
})

export const CreateDisputeRequestSchema = z.object({
  transactionId: z.string().min(1, 'Transaction ID is required'),
  customerId: z.string().min(1, 'Customer ID is required'),
  reason: z.string().min(10, 'Reason must be at least 10 characters').max(200),
  description: z.string().min(20, 'Description must be at least 20 characters').max(2000),
  category: z.nativeEnum(DisputeCategory),
  disputedAmount: z.number().positive('Amount must be greater than 0'),
  currency: z.string().default('ZAR'),
  transactionReference: z.string().optional(),
  merchantName: z.string().optional(),
})

export const UpdateDisputeStatusSchema = z.object({
  status: z.nativeEnum(DisputeStatus),
  resolutionNotes: z.string().max(1000).optional(),
})

export const DisputeStatisticsSchema = z.object({
  totalDisputes: z.number(),
  pendingDisputes: z.number(),
  underReviewDisputes: z.number(),
  resolvedDisputes: z.number(),
  rejectedDisputes: z.number(),
  totalDisputedAmount: z.number(),
  resolvedAmount: z.number(),
})

const PagedResultSchema = <T extends z.ZodTypeAny>(itemSchema: T) =>
  z.object({
    items: z.array(itemSchema),
    totalCount: z.number(),
    page: z.number(),
    pageSize: z.number(),
    totalPages: z.number(),
    hasNextPage: z.boolean(),
    hasPreviousPage: z.boolean(),
  })

export const DisputePagedResultSchema = PagedResultSchema(DisputeSchema)

// ─────────────────────────────────────────────────────────────────────────────
// Types (inferred from schemas)
// ─────────────────────────────────────────────────────────────────────────────

export type Dispute = z.infer<typeof DisputeSchema>
export type DisputeQueryParams = z.infer<typeof DisputeQueryParamsSchema>
export type CreateDisputeRequest = z.infer<typeof CreateDisputeRequestSchema>
export type UpdateDisputeStatus = z.infer<typeof UpdateDisputeStatusSchema>
export type DisputeStatistics = z.infer<typeof DisputeStatisticsSchema>
export type DisputePagedResult = z.infer<typeof DisputePagedResultSchema>

// ─────────────────────────────────────────────────────────────────────────────
// API Functions
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Get paginated list of disputes with optional filters
 */
export async function getDisputes(
  params?: DisputeQueryParams
): Promise<DisputePagedResult> {
  const validated = params ? DisputeQueryParamsSchema.parse(params) : undefined
  
  if (isDemoMode()) {
    const response = await mockDisputeApi.getDisputes(validated)
    return DisputePagedResultSchema.parse(response)
  }
  
  const response = await get<DisputePagedResult>('/disputes', { params: validated })
  return DisputePagedResultSchema.parse(response)
}

/**
 * Get single dispute by ID
 */
export async function getDisputeById(id: string): Promise<Dispute> {
  if (!id) throw new Error('Dispute ID is required')
  
  if (isDemoMode()) {
    const response = await mockDisputeApi.getDisputeById(id)
    return DisputeSchema.parse(response)
  }
  
  const response = await get<Dispute>(`/disputes/${id}`)
  return DisputeSchema.parse(response)
}

/**
 * Get disputes by transaction ID
 */
export async function getDisputesByTransaction(transactionId: string): Promise<Dispute[]> {
  if (!transactionId) throw new Error('Transaction ID is required')
  
  if (isDemoMode()) {
    const result = await mockDisputeApi.getDisputes({ pageSize: 100 })
    const filtered = result.items.filter(d => d.transactionId === transactionId)
    return z.array(DisputeSchema).parse(filtered)
  }
  
  const response = await get<Dispute[]>(`/disputes/transaction/${transactionId}`)
  return z.array(DisputeSchema).parse(response)
}

/**
 * Get dispute statistics
 */
export async function getStatistics(customerId?: string): Promise<DisputeStatistics> {
  if (isDemoMode()) {
    // Mock API doesn't use customerId parameter
    const response = await mockDisputeApi.getStatistics()
    return DisputeStatisticsSchema.parse(response)
  }
  
  const response = await get<DisputeStatistics>('/disputes/statistics', {
    params: customerId ? { customerId } : undefined,
  })
  return DisputeStatisticsSchema.parse(response)
}

/**
 * Create new dispute
 */
export async function createDispute(data: CreateDisputeRequest): Promise<Dispute> {
  const validated = CreateDisputeRequestSchema.parse(data)
  
  if (isDemoMode()) {
    const response = await mockDisputeApi.createDispute(validated)
    return DisputeSchema.parse(response)
  }
  
  const response = await post<Dispute>('/disputes', validated)
  return DisputeSchema.parse(response)
}

/**
 * Update dispute status
 */
export async function updateDisputeStatus(
  id: string,
  status: DisputeStatus,
  resolutionNotes?: string
): Promise<Dispute> {
  if (!id) throw new Error('Dispute ID is required')
  
  const validated = UpdateDisputeStatusSchema.parse({ status, resolutionNotes })
  
  if (isDemoMode()) {
    const response = await mockDisputeApi.updateDisputeStatus(id, status, resolutionNotes)
    return DisputeSchema.parse(response)
  }
  
  const response = await put<Dispute>(`/disputes/${id}/status`, validated)
  return DisputeSchema.parse(response)
}

/**
 * Cancel a dispute
 */
export async function cancelDispute(id: string): Promise<void> {
  if (!id) throw new Error('Dispute ID is required')
  
  if (isDemoMode()) {
    await mockDisputeApi.cancelDispute(id)
    return
  }
  
  await post<void>(`/disputes/${id}/cancel`)
}

/**
 * Update dispute description
 */
export async function updateDisputeDescription(
  id: string,
  description: string
): Promise<Dispute> {
  if (!id) throw new Error('Dispute ID is required')
  if (description.length < 20) throw new Error('Description must be at least 20 characters')
  
  if (isDemoMode()) {
    const response = await mockDisputeApi.updateDisputeDescription(id, description)
    return DisputeSchema.parse(response)
  }
  
  const response = await put<Dispute>(`/disputes/${id}/description`, { description })
  return DisputeSchema.parse(response)
}

/**
 * Delete a dispute
 */
export async function deleteDispute(id: string): Promise<void> {
  if (!id) throw new Error('Dispute ID is required')
  
  if (isDemoMode()) {
    await mockDisputeApi.deleteDispute(id)
    return
  }
  
  await del<void>(`/disputes/${id}`)
}
