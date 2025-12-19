// Transaction types
export interface Transaction {
  id: string
  customerId: string
  description: string
  amount: number
  currency: string
  category: string
  merchantName: string
  merchantCategory: string
  type: TransactionType
  status: TransactionStatus
  transactionDate: string
  createdAt: string
  reference?: string | null
  cardLastFourDigits?: string | null
  isDisputed: boolean
}

export enum TransactionType {
  Debit = 0,
  Credit = 1
}

export enum TransactionStatus {
  Pending = 0,
  Completed = 1,
  Failed = 2,
  Reversed = 3
}

// Dispute types
export interface Dispute {
  id: string
  transactionId: string
  customerId: string
  reason: string
  description: string
  status: DisputeStatus
  category: DisputeCategory
  disputedAmount: number
  currency: string
  createdAt: string
  updatedAt: string
  resolvedAt?: string | null
  resolutionNotes?: string | null
  transactionReference?: string | null
  merchantName?: string | null
}

export enum DisputeStatus {
  Pending = 0,
  UnderReview = 1,
  AwaitingDocuments = 2,
  Resolved = 3,
  Rejected = 4,
  Cancelled = 5
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
  Other = 8
}

// Paged result type
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

// Statistics
export interface DisputeStatistics {
  totalDisputes: number
  pendingDisputes: number
  underReviewDisputes: number
  resolvedDisputes: number
  rejectedDisputes: number
  totalDisputedAmount: number
  resolvedAmount: number
}

// Create dispute request
export interface CreateDisputeRequest {
  transactionId: string
  customerId: string
  reason: string
  description: string
  category: DisputeCategory
  disputedAmount: number
  currency: string
  transactionReference?: string
  merchantName?: string
}

// Query parameters
export interface TransactionQueryParams {
  customerId?: string
  fromDate?: string
  toDate?: string
  category?: string
  type?: TransactionType
  minAmount?: number
  maxAmount?: number
  page?: number
  pageSize?: number
  searchTerm?: string
}

export interface DisputeQueryParams {
  customerId?: string
  transactionId?: string
  status?: DisputeStatus
  category?: DisputeCategory
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

// Auth types
export interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  customerId: string
}

export interface AuthResponse {
  token: string
  refreshToken: string
  expiresAt: string
  user: User
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  firstName: string
  lastName: string
}
