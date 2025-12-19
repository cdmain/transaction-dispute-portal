import { z } from 'zod'

// Transaction schemas
export const TransactionTypeSchema = z.enum(['Debit', 'Credit'])
export const TransactionStatusSchema = z.enum(['Pending', 'Completed', 'Failed', 'Reversed'])

export const TransactionSchema = z.object({
  id: z.string().uuid(),
  customerId: z.string(),
  description: z.string(),
  amount: z.number(),
  currency: z.string().default('ZAR'),
  category: z.string(),
  merchantName: z.string(),
  merchantCategory: z.string(),
  type: z.number(), // 0 = Debit, 1 = Credit
  status: z.number(), // 0 = Pending, 1 = Completed, 2 = Failed, 3 = Reversed
  transactionDate: z.string(),
  createdAt: z.string(),
  reference: z.string().nullable().optional(),
  cardLastFourDigits: z.string().nullable().optional(),
  isDisputed: z.boolean()
})

export const PagedResultSchema = <T extends z.ZodTypeAny>(itemSchema: T) => z.object({
  items: z.array(itemSchema),
  totalCount: z.number(),
  page: z.number(),
  pageSize: z.number(),
  totalPages: z.number(),
  hasNextPage: z.boolean(),
  hasPreviousPage: z.boolean()
})

export const TransactionPagedResultSchema = PagedResultSchema(TransactionSchema)

// Dispute schemas
export const DisputeStatusSchema = z.enum([
  'Pending',
  'UnderReview',
  'AwaitingDocuments',
  'Resolved',
  'Rejected',
  'Cancelled'
])

export const DisputeCategorySchema = z.enum([
  'UnauthorizedTransaction',
  'DuplicateCharge',
  'IncorrectAmount',
  'ServiceNotReceived',
  'ProductNotReceived',
  'QualityIssue',
  'RefundNotReceived',
  'FraudSuspected',
  'Other'
])

export const DisputeSchema = z.object({
  id: z.string().uuid(),
  transactionId: z.string().uuid(),
  customerId: z.string(),
  reason: z.string(),
  description: z.string(),
  status: z.number(), // Maps to DisputeStatus enum
  category: z.number(), // Maps to DisputeCategory enum
  disputedAmount: z.number(),
  currency: z.string().default('ZAR'),
  createdAt: z.string(),
  updatedAt: z.string(),
  resolvedAt: z.string().nullable().optional(),
  resolutionNotes: z.string().nullable().optional(),
  transactionReference: z.string().nullable().optional(),
  merchantName: z.string().nullable().optional()
})

export const DisputePagedResultSchema = PagedResultSchema(DisputeSchema)

export const CreateDisputeSchema = z.object({
  transactionId: z.string().uuid(),
  customerId: z.string().min(1, 'Customer ID is required'),
  reason: z.string().min(10, 'Reason must be at least 10 characters').max(200),
  description: z.string().min(20, 'Description must be at least 20 characters').max(2000),
  category: z.number().min(0).max(8),
  disputedAmount: z.number().positive('Amount must be greater than 0'),
  currency: z.string().default('ZAR'),
  transactionReference: z.string().optional(),
  merchantName: z.string().optional()
})

export const UpdateDisputeStatusSchema = z.object({
  status: z.number().min(0).max(5),
  resolutionNotes: z.string().max(1000).optional()
})

export const DisputeStatisticsSchema = z.object({
  totalDisputes: z.number(),
  pendingDisputes: z.number(),
  underReviewDisputes: z.number(),
  resolvedDisputes: z.number(),
  rejectedDisputes: z.number(),
  totalDisputedAmount: z.number(),
  resolvedAmount: z.number()
})

// Type exports
export type Transaction = z.infer<typeof TransactionSchema>
export type TransactionPagedResult = z.infer<typeof TransactionPagedResultSchema>
export type Dispute = z.infer<typeof DisputeSchema>
export type DisputePagedResult = z.infer<typeof DisputePagedResultSchema>
export type CreateDispute = z.infer<typeof CreateDisputeSchema>
export type UpdateDisputeStatus = z.infer<typeof UpdateDisputeStatusSchema>
export type DisputeStatistics = z.infer<typeof DisputeStatisticsSchema>
