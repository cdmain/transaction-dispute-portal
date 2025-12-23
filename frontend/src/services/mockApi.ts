// Mock data for GitHub Pages demo mode
import type { 
  Transaction, 
  Dispute, 
  PagedResult, 
  DisputeStatistics, 
  AuthResponse,
  User,
  TransactionQueryParams,
  DisputeQueryParams,
  CreateDisputeRequest
} from '@/types'
import { 
  TransactionType, 
  TransactionStatus, 
  DisputeStatus, 
  DisputeCategory 
} from '@/types'

// Check if we're in demo mode (GitHub Pages or no backend)
export const isDemoMode = (): boolean => {
  // Demo mode if running on GitHub Pages or if VITE_DEMO_MODE is set
  return import.meta.env.VITE_DEMO_MODE === 'true' || 
         window.location.hostname.includes('github.io')
}

// Demo user credentials
export const DEMO_CREDENTIALS = {
  email: 'demo@example.com',
  password: 'Demo123!'
}

// Mock user data
const mockUser: User = {
  id: 'demo-user-123',
  email: 'demo@example.com',
  firstName: 'Demo',
  lastName: 'User',
  customerId: 'cust-demo-123'
}

// Generate mock transactions
const generateMockTransactions = (): Transaction[] => {
  const merchants = [
    { name: 'Amazon', category: 'Shopping' },
    { name: 'Walmart', category: 'Groceries' },
    { name: 'Target', category: 'Shopping' },
    { name: 'Best Buy', category: 'Electronics' },
    { name: 'Apple Store', category: 'Electronics' },
    { name: 'Netflix', category: 'Entertainment' },
    { name: 'Spotify', category: 'Entertainment' },
    { name: 'Uber', category: 'Transportation' },
    { name: 'DoorDash', category: 'Food & Dining' },
    { name: 'Starbucks', category: 'Food & Dining' },
    { name: 'Shell Gas', category: 'Gas & Fuel' },
    { name: 'Costco', category: 'Groceries' },
    { name: 'Home Depot', category: 'Home' },
    { name: 'Whole Foods', category: 'Groceries' },
    { name: 'CVS Pharmacy', category: 'Health' }
  ]
  
  const transactions: Transaction[] = []
  const now = new Date()
  
  for (let i = 1; i <= 50; i++) {
    const daysAgo = Math.floor(Math.random() * 90)
    const date = new Date(now.getTime() - daysAgo * 24 * 60 * 60 * 1000)
    const merchant = merchants[Math.floor(Math.random() * merchants.length)]
    const isDisputed = Math.random() > 0.85
    
    transactions.push({
      id: `txn-${i.toString().padStart(4, '0')}`,
      customerId: mockUser.customerId,
      amount: Math.round((Math.random() * 500 + 10) * 100) / 100,
      currency: 'USD',
      merchantName: merchant.name,
      merchantCategory: merchant.category,
      category: merchant.category,
      type: Math.random() > 0.9 ? TransactionType.Credit : TransactionType.Debit,
      transactionDate: date.toISOString(),
      createdAt: date.toISOString(),
      description: `Transaction at ${merchant.name}`,
      status: Math.random() > 0.1 ? TransactionStatus.Completed : TransactionStatus.Pending,
      reference: `REF-${Date.now()}-${i}`,
      cardLastFourDigits: '4242',
      isDisputed
    })
  }
  
  return transactions.sort((a, b) => 
    new Date(b.transactionDate).getTime() - new Date(a.transactionDate).getTime()
  )
}

// Generate mock disputes
const generateMockDisputes = (transactions: Transaction[]): Dispute[] => {
  const reasons = [
    'Unauthorized transaction on my account',
    'I was charged twice for this purchase',
    'Product was never delivered',
    'Product arrived damaged/defective',
    'Charged wrong amount',
    'Subscription cancelled but still charged',
    'Suspicious fraudulent activity'
  ]
  
  const disputedTransactions = transactions.filter(t => t.isDisputed)
  
  return disputedTransactions.map((txn, i) => {
    const createdDate = new Date(txn.transactionDate)
    createdDate.setDate(createdDate.getDate() + Math.floor(Math.random() * 5) + 1)
    
    const statusValues = [
      DisputeStatus.Pending,
      DisputeStatus.UnderReview,
      DisputeStatus.Resolved,
      DisputeStatus.Rejected,
      DisputeStatus.Cancelled
    ]
    const status = statusValues[Math.floor(Math.random() * statusValues.length)]
    const isResolved = [DisputeStatus.Resolved, DisputeStatus.Rejected, DisputeStatus.Cancelled].includes(status)
    
    let resolvedAt: string | null = null
    let resolutionNotes: string | null = null
    
    if (isResolved) {
      const resolvedDate = new Date(createdDate)
      resolvedDate.setDate(resolvedDate.getDate() + Math.floor(Math.random() * 14) + 1)
      resolvedAt = resolvedDate.toISOString()
      
      if (status === DisputeStatus.Resolved) {
        resolutionNotes = 'Dispute resolved in customer favor. Refund issued.'
      } else if (status === DisputeStatus.Rejected) {
        resolutionNotes = 'After investigation, transaction was found to be valid.'
      } else {
        resolutionNotes = 'Dispute cancelled by customer.'
      }
    }
    
    const categoryValues = [
      DisputeCategory.UnauthorizedTransaction,
      DisputeCategory.DuplicateCharge,
      DisputeCategory.ProductNotReceived,
      DisputeCategory.QualityIssue,
      DisputeCategory.IncorrectAmount,
      DisputeCategory.FraudSuspected
    ]
    
    return {
      id: `disp-${(i + 1).toString().padStart(4, '0')}`,
      transactionId: txn.id,
      customerId: mockUser.customerId,
      reason: reasons[Math.floor(Math.random() * reasons.length)],
      description: `Customer disputes charge of $${txn.amount.toFixed(2)} at ${txn.merchantName}`,
      status,
      category: categoryValues[Math.floor(Math.random() * categoryValues.length)],
      disputedAmount: txn.amount,
      currency: 'USD',
      createdAt: createdDate.toISOString(),
      updatedAt: (resolvedAt || createdDate.toISOString()),
      resolvedAt,
      resolutionNotes,
      transactionReference: txn.reference,
      merchantName: txn.merchantName
    }
  })
}

// Initialize mock data
const mockTransactions = generateMockTransactions()
const mockDisputes = generateMockDisputes(mockTransactions)

// Simulate network delay
const delay = (ms: number) => new Promise(resolve => setTimeout(resolve, ms))

// Mock API implementations
export const mockAuthApi = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    await delay(500)
    
    if (email === DEMO_CREDENTIALS.email && password === DEMO_CREDENTIALS.password) {
      return {
        token: 'mock-jwt-token-' + Date.now(),
        refreshToken: 'mock-refresh-token-' + Date.now(),
        expiresAt: new Date(Date.now() + 3600000).toISOString(),
        user: mockUser
      }
    }
    
    throw new Error('Invalid credentials. Use demo@example.com / Demo123!')
  },
  
  register: async (email: string, _password: string, firstName: string, lastName: string): Promise<AuthResponse> => {
    await delay(500)
    
    // In demo mode, just return success with the provided info
    return {
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      expiresAt: new Date(Date.now() + 3600000).toISOString(),
      user: {
        id: 'new-user-' + Date.now(),
        email,
        firstName,
        lastName,
        customerId: 'cust-new-' + Date.now()
      }
    }
  },
  
  refreshToken: async (): Promise<AuthResponse> => {
    await delay(200)
    return {
      token: 'mock-jwt-token-' + Date.now(),
      refreshToken: 'mock-refresh-token-' + Date.now(),
      expiresAt: new Date(Date.now() + 3600000).toISOString(),
      user: mockUser
    }
  },
  
  logout: async (): Promise<void> => {
    await delay(200)
  }
}

export const mockTransactionApi = {
  getTransactions: async (params?: TransactionQueryParams): Promise<PagedResult<Transaction>> => {
    await delay(300)
    
    let filtered = [...mockTransactions]
    
    // Apply filters
    if (params?.searchTerm) {
      const search = params.searchTerm.toLowerCase()
      filtered = filtered.filter(t => 
        t.merchantName.toLowerCase().includes(search) ||
        t.description?.toLowerCase().includes(search)
      )
    }
    
    if (params?.category) {
      filtered = filtered.filter(t => t.category === params.category)
    }
    
    if (params?.fromDate) {
      filtered = filtered.filter(t => new Date(t.transactionDate) >= new Date(params.fromDate!))
    }
    
    if (params?.toDate) {
      filtered = filtered.filter(t => new Date(t.transactionDate) <= new Date(params.toDate!))
    }
    
    if (params?.minAmount) {
      filtered = filtered.filter(t => t.amount >= params.minAmount!)
    }
    
    if (params?.maxAmount) {
      filtered = filtered.filter(t => t.amount <= params.maxAmount!)
    }
    
    // Pagination
    const page = params?.page || 1
    const pageSize = params?.pageSize || 10
    const start = (page - 1) * pageSize
    const items = filtered.slice(start, start + pageSize)
    const totalPages = Math.ceil(filtered.length / pageSize)
    
    return {
      items,
      totalCount: filtered.length,
      page,
      pageSize,
      totalPages,
      hasNextPage: page < totalPages,
      hasPreviousPage: page > 1
    }
  },
  
  getTransactionById: async (id: string): Promise<Transaction> => {
    await delay(200)
    const txn = mockTransactions.find(t => t.id === id)
    if (!txn) throw new Error('Transaction not found')
    return txn
  },
  
  getCategories: async (): Promise<string[]> => {
    await delay(100)
    return [...new Set(mockTransactions.map(t => t.category))]
  },

  seedTransactions: async (): Promise<Transaction[]> => {
    await delay(300)
    // In demo mode, transactions are already seeded
    return mockTransactions
  }
}

export const mockDisputeApi = {
  getDisputes: async (params?: DisputeQueryParams): Promise<PagedResult<Dispute>> => {
    await delay(300)
    
    let filtered = [...mockDisputes]
    
    // Apply filters
    if (params?.status !== undefined) {
      filtered = filtered.filter(d => d.status === params.status)
    }
    
    if (params?.category !== undefined) {
      filtered = filtered.filter(d => d.category === params.category)
    }
    
    if (params?.fromDate) {
      const fromDate = new Date(params.fromDate)
      filtered = filtered.filter(d => new Date(d.createdAt) >= fromDate)
    }
    
    if (params?.toDate) {
      const toDate = new Date(params.toDate)
      toDate.setHours(23, 59, 59, 999) // End of day
      filtered = filtered.filter(d => new Date(d.createdAt) <= toDate)
    }
    
    if ((params as any)?.searchTerm) {
      const search = ((params as any).searchTerm as string).toLowerCase()
      filtered = filtered.filter(d => 
        d.reason.toLowerCase().includes(search) ||
        d.merchantName?.toLowerCase().includes(search)
      )
    }
    
    // Pagination
    const page = params?.page || 1
    const pageSize = params?.pageSize || 10
    const start = (page - 1) * pageSize
    const items = filtered.slice(start, start + pageSize)
    const totalPages = Math.ceil(filtered.length / pageSize)
    
    return {
      items,
      totalCount: filtered.length,
      page,
      pageSize,
      totalPages,
      hasNextPage: page < totalPages,
      hasPreviousPage: page > 1
    }
  },
  
  getDisputeById: async (id: string): Promise<Dispute> => {
    await delay(200)
    const dispute = mockDisputes.find(d => d.id === id)
    if (!dispute) throw new Error('Dispute not found')
    return dispute
  },
  
  createDispute: async (data: CreateDisputeRequest): Promise<Dispute> => {
    await delay(500)
    
    const txn = mockTransactions.find(t => t.id === data.transactionId)
    if (!txn) throw new Error('Transaction not found')
    
    const newDispute: Dispute = {
      id: `disp-${(mockDisputes.length + 1).toString().padStart(4, '0')}`,
      transactionId: data.transactionId,
      customerId: mockUser.customerId,
      reason: data.reason,
      description: data.description,
      status: DisputeStatus.Pending,
      category: data.category || DisputeCategory.Other,
      disputedAmount: data.disputedAmount || txn.amount,
      currency: 'USD',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      resolvedAt: null,
      resolutionNotes: null,
      transactionReference: txn.reference,
      merchantName: txn.merchantName
    }
    
    mockDisputes.unshift(newDispute)
    txn.isDisputed = true
    
    return newDispute
  },
  
  updateDisputeStatus: async (id: string, status: DisputeStatus, notes?: string): Promise<Dispute> => {
    await delay(400)
    
    const dispute = mockDisputes.find(d => d.id === id)
    if (!dispute) throw new Error('Dispute not found')
    
    dispute.status = status
    dispute.updatedAt = new Date().toISOString()
    
    if ([DisputeStatus.Resolved, DisputeStatus.Rejected, DisputeStatus.Cancelled].includes(status)) {
      dispute.resolvedAt = new Date().toISOString()
      dispute.resolutionNotes = notes || null
    }
    
    return dispute
  },
  
  cancelDispute: async (id: string): Promise<void> => {
    await delay(300)
    
    const dispute = mockDisputes.find(d => d.id === id)
    if (!dispute) throw new Error('Dispute not found')
    
    // Check if dispute can be cancelled
    if (dispute.status === DisputeStatus.Resolved || dispute.status === DisputeStatus.Rejected) {
      throw new Error('Cannot cancel a resolved or rejected dispute')
    }
    
    dispute.status = DisputeStatus.Cancelled
    dispute.updatedAt = new Date().toISOString()
    dispute.resolvedAt = new Date().toISOString()
    dispute.resolutionNotes = 'Cancelled by customer'
    
    // Unmark transaction as disputed so it can be re-disputed
    const txn = mockTransactions.find((t: { id: string }) => t.id === dispute.transactionId)
    if (txn) {
      txn.isDisputed = false
    }
  },

  updateDisputeDescription: async (id: string, description: string): Promise<Dispute> => {
    await delay(300)
    
    const dispute = mockDisputes.find(d => d.id === id)
    if (!dispute) throw new Error('Dispute not found')
    
    dispute.description = description
    dispute.updatedAt = new Date().toISOString()
    
    return dispute
  },

  deleteDispute: async (id: string): Promise<void> => {
    await delay(300)
    
    const index = mockDisputes.findIndex(d => d.id === id)
    if (index === -1) throw new Error('Dispute not found')
    
    const dispute = mockDisputes[index]
    
    // Unmark transaction as disputed
    const txn = mockTransactions.find((t: { id: string }) => t.id === dispute.transactionId)
    if (txn) {
      txn.isDisputed = false
    }
    
    // Remove from array
    mockDisputes.splice(index, 1)
  },
  
  getStatistics: async (): Promise<DisputeStatistics> => {
    await delay(200)
    
    const pending = mockDisputes.filter(d => d.status === DisputeStatus.Pending).length
    const underReview = mockDisputes.filter(d => d.status === DisputeStatus.UnderReview).length
    const resolved = mockDisputes.filter(d => d.status === DisputeStatus.Resolved).length
    const rejected = mockDisputes.filter(d => d.status === DisputeStatus.Rejected).length
    
    const totalAmount = mockDisputes.reduce((sum, d) => sum + d.disputedAmount, 0)
    const resolvedAmount = mockDisputes
      .filter(d => d.status === DisputeStatus.Resolved)
      .reduce((sum, d) => sum + d.disputedAmount, 0)
    
    return {
      totalDisputes: mockDisputes.length,
      pendingDisputes: pending,
      underReviewDisputes: underReview,
      resolvedDisputes: resolved,
      rejectedDisputes: rejected,
      totalDisputedAmount: totalAmount,
      resolvedAmount
    }
  }
}
