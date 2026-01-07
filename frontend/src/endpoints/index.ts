/**
 * Endpoints Index
 * 
 * Central export for all API endpoints.
 * Import from '@/endpoints' for clean, organized API access.
 */

// Auth endpoints
export {
  login,
  register,
  refreshToken,
  logout,
  type LoginRequest,
  type RegisterRequest,
  type AuthResponse,
  type User,
} from './auth'

// Transaction endpoints
export {
  getTransactions,
  getTransactionById,
  getTransactionsByCustomer,
  getCategories,
  seedTransactions,
  TransactionType,
  TransactionStatus,
  type Transaction,
  type TransactionQueryParams,
  type TransactionPagedResult,
} from './transactions'

// Dispute endpoints
export {
  getDisputes,
  getDisputeById,
  getDisputesByTransaction,
  getStatistics,
  createDispute,
  updateDisputeStatus,
  cancelDispute,
  updateDisputeDescription,
  deleteDispute,
  DisputeStatus,
  DisputeCategory,
  DISPUTE_STATUS_LABELS,
  DISPUTE_CATEGORY_LABELS,
  type Dispute,
  type DisputeQueryParams,
  type CreateDisputeRequest,
  type DisputeStatistics,
  type DisputePagedResult,
} from './disputes'

// HTTP client (for advanced usage)
export { httpClient } from './client'
