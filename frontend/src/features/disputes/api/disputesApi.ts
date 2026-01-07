import { z } from 'zod'
import { get, post, put, del } from '@/shared/api/client'
import { isDemoMode, mockDisputeApi } from '@/services/mockApi'

// Enums
export enum DisputeStatus { Pending = 0, UnderReview = 1, AwaitingDocuments = 2, Resolved = 3, Rejected = 4, Cancelled = 5 }
export enum DisputeCategory { UnauthorizedTransaction = 0, DuplicateCharge = 1, IncorrectAmount = 2, ServiceNotReceived = 3, ProductNotReceived = 4, QualityIssue = 5, RefundNotReceived = 6, FraudSuspected = 7, Other = 8 }

// Schemas
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

export const DisputeQuerySchema = z.object({
  customerId: z.string().optional(),
  transactionId: z.string().optional(),
  status: z.nativeEnum(DisputeStatus).optional(),
  category: z.nativeEnum(DisputeCategory).optional(),
  fromDate: z.string().optional(),
  toDate: z.string().optional(),
  page: z.number().optional(),
  pageSize: z.number().optional(),
  searchTerm: z.string().optional(),
})

export const CreateDisputeSchema = z.object({
  transactionId: z.string(),
  customerId: z.string(),
  reason: z.string().min(10).max(200),
  description: z.string().min(20).max(2000),
  category: z.nativeEnum(DisputeCategory),
  disputedAmount: z.number().positive(),
  currency: z.string().default('ZAR'),
  transactionReference: z.string().optional(),
  merchantName: z.string().optional(),
})

export const DisputeStatsSchema = z.object({
  totalDisputes: z.number(),
  pendingDisputes: z.number(),
  underReviewDisputes: z.number(),
  resolvedDisputes: z.number(),
  rejectedDisputes: z.number(),
  totalDisputedAmount: z.number(),
  resolvedAmount: z.number(),
})

export const PagedDisputesSchema = z.object({
  items: z.array(DisputeSchema),
  totalCount: z.number(),
  page: z.number(),
  pageSize: z.number(),
  totalPages: z.number(),
  hasNextPage: z.boolean(),
  hasPreviousPage: z.boolean(),
})

// Types
export type Dispute = z.infer<typeof DisputeSchema>
export type DisputeQuery = z.infer<typeof DisputeQuerySchema>
export type CreateDispute = z.infer<typeof CreateDisputeSchema>
export type DisputeStats = z.infer<typeof DisputeStatsSchema>
export type PagedDisputes = z.infer<typeof PagedDisputesSchema>

// API
export async function getDisputes(params?: DisputeQuery): Promise<PagedDisputes> {
  const validated = params ? DisputeQuerySchema.parse(params) : undefined
  if (isDemoMode()) {
    return PagedDisputesSchema.parse(await mockDisputeApi.getDisputes(validated))
  }
  return PagedDisputesSchema.parse(await get<PagedDisputes>('/disputes', { params: validated }))
}

export async function getDisputeById(id: string): Promise<Dispute> {
  if (isDemoMode()) return DisputeSchema.parse(await mockDisputeApi.getDisputeById(id))
  return DisputeSchema.parse(await get<Dispute>(`/disputes/${id}`))
}

export async function getDisputesByTransaction(transactionId: string): Promise<Dispute[]> {
  if (isDemoMode()) {
    const result = await mockDisputeApi.getDisputes({ pageSize: 100 })
    return z.array(DisputeSchema).parse(result.items.filter(d => d.transactionId === transactionId))
  }
  return z.array(DisputeSchema).parse(await get<Dispute[]>(`/disputes/transaction/${transactionId}`))
}

export async function getStatistics(): Promise<DisputeStats> {
  if (isDemoMode()) return DisputeStatsSchema.parse(await mockDisputeApi.getStatistics())
  return DisputeStatsSchema.parse(await get<DisputeStats>('/disputes/statistics'))
}

export async function createDispute(data: CreateDispute): Promise<Dispute> {
  const validated = CreateDisputeSchema.parse(data)
  if (isDemoMode()) return DisputeSchema.parse(await mockDisputeApi.createDispute(validated))
  return DisputeSchema.parse(await post<Dispute>('/disputes', validated))
}

export async function cancelDispute(id: string): Promise<void> {
  if (isDemoMode()) { await mockDisputeApi.cancelDispute(id); return }
  await post<void>(`/disputes/${id}/cancel`)
}

export async function updateDescription(id: string, description: string): Promise<Dispute> {
  if (isDemoMode()) return DisputeSchema.parse(await mockDisputeApi.updateDisputeDescription(id, description))
  return DisputeSchema.parse(await put<Dispute>(`/disputes/${id}/description`, { description }))
}

export async function deleteDispute(id: string): Promise<void> {
  if (isDemoMode()) { await mockDisputeApi.deleteDispute(id); return }
  await del<void>(`/disputes/${id}`)
}
