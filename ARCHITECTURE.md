# Architecture Documentation

Technical architecture and design decisions for the Transaction Dispute Portal.

---

## Table of Contents

- [System Overview](#system-overview)
- [Backend Architecture](#backend-architecture)
- [Frontend Architecture](#frontend-architecture)
- [Error Handling](#error-handling)
- [Validation Strategy](#validation-strategy)
- [Security](#security)
- [Caching Strategy](#caching-strategy)

---

## System Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Client Layer                             │
│    ┌─────────────────────────────────────────────────────────┐  │
│    │                Vue 3 SPA (Port 3000)                    │  │
│    │  ┌───────────┐ ┌───────────┐ ┌───────────────────────┐ │  │
│    │  │  Router   │ │ TanStack  │ │    Zod Validation     │ │  │
│    │  │  Guards   │ │   Query   │ │       Schemas         │ │  │
│    │  └───────────┘ └───────────┘ └───────────────────────┘ │  │
│    └─────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                       API Gateway Layer                          │
│    ┌─────────────────────────────────────────────────────────┐  │
│    │           YARP Reverse Proxy (Port 5000)                │  │
│    │  ┌───────────┐ ┌───────────┐ ┌───────────────────────┐ │  │
│    │  │   Route   │ │   JWT     │ │       Request         │ │  │
│    │  │  Matching │ │ Validate  │ │     Forwarding        │ │  │
│    │  └───────────┘ └───────────┘ └───────────────────────┘ │  │
│    └─────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
          ┌───────────────────┼───────────────────┐
          ▼                   ▼                   ▼
┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐
│  Auth Service   │ │  Transaction    │ │  Dispute        │
│   (Port 5003)   │ │    Service      │ │   Service       │
│                 │ │   (Port 5001)   │ │  (Port 5002)    │
│ ┌─────────────┐ │ │ ┌─────────────┐ │ │ ┌─────────────┐ │
│ │   BCrypt    │ │ │ │    Redis    │ │ │ │   Status    │ │
│ │  Hashing    │ │ │ │   Caching   │ │ │ │  Workflow   │ │
│ └─────────────┘ │ │ └─────────────┘ │ │ └─────────────┘ │
│ ┌─────────────┐ │ │ ┌─────────────┐ │ │ ┌─────────────┐ │
│ │     JWT     │ │ │ │   SQLite    │ │ │ │   SQLite    │ │
│ │   Tokens    │ │ │ │     DB      │ │ │ │     DB      │ │
│ └─────────────┘ │ │ └─────────────┘ │ │ └─────────────┘ │
└─────────────────┘ └─────────────────┘ └─────────────────┘
```

### Service Communication

| From | To | Protocol | Auth |
|------|-----|----------|------|
| Frontend | API Gateway | HTTP/JSON | JWT Bearer |
| API Gateway | Auth Service | HTTP/JSON | None (internal) |
| API Gateway | Transaction Service | HTTP/JSON | JWT forwarded |
| API Gateway | Dispute Service | HTTP/JSON | JWT forwarded |

---

## Backend Architecture

### Microservice Design

Each service follows Clean Architecture principles:

```
Service/
├── Controllers/          # HTTP layer (API endpoints)
├── Models/              # Domain entities and DTOs
├── Data/                # DbContext and repositories
├── Services/            # Business logic (optional)
├── Program.cs           # Composition root
└── appsettings.json     # Configuration
```

### API Gateway (YARP)

The API Gateway uses Microsoft YARP for reverse proxy routing:

```csharp
// Route configuration
"ReverseProxy": {
  "Routes": {
    "auth-route": {
      "ClusterId": "auth-cluster",
      "Match": { "Path": "/api/auth/{**catch-all}" }
    },
    "transaction-route": {
      "ClusterId": "transaction-cluster",
      "Match": { "Path": "/api/transactions/{**catch-all}" }
    },
    "dispute-route": {
      "ClusterId": "dispute-cluster", 
      "Match": { "Path": "/api/disputes/{**catch-all}" }
    }
  }
}
```

### Database Design

**AuthService (auth.db)**
```sql
CREATE TABLE Users (
    Id TEXT PRIMARY KEY,
    Email TEXT UNIQUE NOT NULL,
    PasswordHash TEXT NOT NULL,
    CustomerId TEXT NOT NULL,
    CreatedAt TEXT NOT NULL,
    RefreshToken TEXT,
    RefreshTokenExpiry TEXT
);
```

**TransactionService (transactions.db)**
```sql
CREATE TABLE Transactions (
    Id TEXT PRIMARY KEY,
    CustomerId TEXT NOT NULL,
    Amount REAL NOT NULL,
    Currency TEXT NOT NULL,
    Type TEXT NOT NULL,
    Status TEXT NOT NULL,
    MerchantName TEXT NOT NULL,
    Category TEXT NOT NULL,
    Description TEXT NOT NULL,
    TransactionDate TEXT NOT NULL,
    CreatedAt TEXT NOT NULL
);
```

**DisputeService (disputes.db)**
```sql
CREATE TABLE Disputes (
    Id TEXT PRIMARY KEY,
    TransactionId TEXT NOT NULL,
    CustomerId TEXT NOT NULL,
    Reason TEXT NOT NULL,
    Description TEXT NOT NULL,
    Status TEXT NOT NULL,
    DisputedAmount REAL NOT NULL,
    Resolution TEXT,
    CreatedAt TEXT NOT NULL,
    UpdatedAt TEXT NOT NULL,
    ResolvedAt TEXT
);
```

---

## Frontend Architecture

### Directory Structure

```
frontend/src/
├── api/
│   └── index.ts         # Axios instance with interceptors
├── composables/
│   ├── useAuth.ts       # Authentication logic (TanStack mutations)
│   ├── useTransactions.ts # Transaction queries
│   └── useDisputes.ts   # Dispute queries & mutations
├── schemas/
│   └── index.ts         # Zod validation schemas
├── utils/
│   └── authStorage.ts   # Token storage (separated for circular dep)
├── views/
│   ├── DashboardView.vue
│   ├── TransactionsView.vue
│   ├── DisputesView.vue
│   ├── LoginView.vue
│   └── RegisterView.vue
├── components/
│   ├── Navbar.vue
│   └── TransactionCard.vue
├── router/
│   └── index.ts         # Routes with auth guards
└── App.vue
```

### State Management

**TanStack Query** handles all server state:

```typescript
// Query (read)
const { data, isLoading, error } = useQuery({
  queryKey: ['transactions', page, filters],
  queryFn: () => api.get('/transactions', { params: { page, ...filters } })
})

// Mutation (write)
const { mutate, isPending } = useMutation({
  mutationFn: (dispute) => api.post('/disputes', dispute),
  onSuccess: () => {
    queryClient.invalidateQueries({ queryKey: ['disputes'] })
  }
})
```

### Component Communication

```
App.vue
├── Navbar.vue ──── useAuth() ──── Logout mutation
└── RouterView
    ├── DashboardView.vue ──── useTransactions() ──── Query
    ├── TransactionsView.vue ──── useTransactions() + useDisputes()
    ├── DisputesView.vue ──── useDisputes() ──── Query + Mutations
    ├── LoginView.vue ──── useAuth() ──── Login mutation
    └── RegisterView.vue ──── useAuth() ──── Register mutation
```

---

## Error Handling

### Backend Error Handling

#### Controller Layer

```csharp
[HttpGet("{id}")]
[ProducesResponseType(typeof(Transaction), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> GetById(string id)
{
    try
    {
        var transaction = await _context.Transactions.FindAsync(id);
        
        if (transaction == null)
            return NotFound(new { error = "Transaction not found" });
            
        return Ok(transaction);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving transaction {Id}", id);
        return StatusCode(500, new { error = "An error occurred" });
    }
}
```

#### Standard Error Response

```json
{
  "error": "Human-readable error message",
  "code": "ERROR_CODE",
  "details": { }
}
```

#### HTTP Status Codes

| Code | Usage |
|------|-------|
| 200 | Success |
| 201 | Created |
| 400 | Validation error, business rule violation |
| 401 | Authentication required |
| 403 | Forbidden (no permission) |
| 404 | Resource not found |
| 409 | Conflict (duplicate) |
| 500 | Server error |

### Frontend Error Handling

#### API Interceptor

```typescript
// src/api/index.ts
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      removeAuthToken()
      removeRefreshToken()
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)
```

#### Component Error States

```vue
<template>
  <!-- Loading state -->
  <div v-if="isLoading">Loading...</div>
  
  <!-- Error state -->
  <div v-else-if="error" class="text-red-600">
    {{ error.message }}
  </div>
  
  <!-- Success state -->
  <div v-else>
    {{ data }}
  </div>
</template>

<script setup lang="ts">
const { data, isLoading, error } = useTransactions()
</script>
```

#### Form Validation Errors

```vue
<script setup lang="ts">
const validationErrors = ref<Record<string, string>>({})

const submit = () => {
  const result = CreateDisputeSchema.safeParse(form)
  
  if (!result.success) {
    validationErrors.value = result.error.flatten().fieldErrors
    return
  }
  
  mutate(result.data)
}
</script>

<template>
  <input v-model="form.reason" />
  <span v-if="validationErrors.reason" class="text-red-500">
    {{ validationErrors.reason[0] }}
  </span>
</template>
```

---

## Validation Strategy

### Frontend Validation (Zod)

All data is validated client-side before API calls:

```typescript
// src/schemas/index.ts
import { z } from 'zod'

export const CreateDisputeSchema = z.object({
  transactionId: z.string().uuid(),
  reason: z.enum(['Unauthorized', 'Duplicate', 'NotReceived', 'WrongAmount', 'Other']),
  description: z.string().min(10).max(500),
  disputedAmount: z.number().positive()
})

// Usage
const result = CreateDisputeSchema.safeParse(formData)
if (!result.success) {
  // Handle validation errors
  console.log(result.error.flatten().fieldErrors)
}
```

### Backend Validation

Data annotations and model validation:

```csharp
public class CreateDisputeRequest
{
    [Required]
    public string TransactionId { get; set; }
    
    [Required]
    [EnumDataType(typeof(DisputeReason))]
    public DisputeReason Reason { get; set; }
    
    [Required]
    [MinLength(10)]
    [MaxLength(500)]
    public string Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal DisputedAmount { get; set; }
}
```

### Validation Flow

```
User Input
    │
    ▼
┌─────────────────┐
│  Zod Schema     │  ← Client-side (immediate feedback)
│  Validation     │
└────────┬────────┘
         │
         ▼
    API Request
         │
         ▼
┌─────────────────┐
│  Model Binding  │  ← Server-side (data annotations)
│  Validation     │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│  Business Logic │  ← Domain validation
│  Validation     │
└────────┬────────┘
         │
         ▼
    Database
```

---

## Security

### Authentication Flow

```
1. Login Request
   POST /api/auth/login
   { email, password }
        │
        ▼
2. Validate Credentials
   BCrypt.Verify(password, hash)
        │
        ▼
3. Generate Tokens
   JWT Access Token (60 min)
   Refresh Token (7 days)
        │
        ▼
4. Store in localStorage
   accessToken, refreshToken
        │
        ▼
5. Subsequent Requests
   Authorization: Bearer {token}
        │
        ▼
6. Token Expired?
   Use refresh token to get new access token
```

### JWT Token Structure

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id",
    "email": "user@example.com",
    "customer_id": "CUST001",
    "exp": 1234567890,
    "iss": "TransactionDisputePortal",
    "aud": "TransactionDisputePortal"
  }
}
```

### Route Protection

**Backend:**
```csharp
[Authorize]  // Requires valid JWT
[HttpGet]
public async Task<IActionResult> GetTransactions() { }
```

**Frontend:**
```typescript
// router/index.ts
router.beforeEach((to, from, next) => {
  const token = getAuthToken()
  
  if (to.meta.requiresAuth && !token) {
    next('/login')
  } else if (to.meta.guest && token) {
    next('/dashboard')
  } else {
    next()
  }
})
```

---

## Caching Strategy

### Redis Caching

Transaction data is cached to reduce database load:

```csharp
public async Task<IActionResult> GetTransactions()
{
    var cacheKey = $"transactions:{customerId}:{page}";
    
    // Try cache first
    var cached = await _cache.GetStringAsync(cacheKey);
    if (cached != null)
    {
        return Ok(JsonSerializer.Deserialize<PagedResult>(cached));
    }
    
    // Query database
    var result = await _context.Transactions
        .Where(t => t.CustomerId == customerId)
        .ToListAsync();
    
    // Cache for 5 minutes
    await _cache.SetStringAsync(cacheKey, 
        JsonSerializer.Serialize(result),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
    
    return Ok(result);
}
```

### Cache Invalidation

```csharp
// Invalidate on write operations
public async Task<IActionResult> CreateDispute(CreateDisputeRequest request)
{
    // ... create dispute
    
    // Invalidate related caches
    await _cache.RemoveAsync($"disputes:{customerId}");
    
    return Created(...);
}
```

### TanStack Query Caching

Frontend caches are managed by TanStack Query:

```typescript
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,     // 5 minutes
      gcTime: 1000 * 60 * 30,       // 30 minutes
      refetchOnWindowFocus: false,
      retry: 1
    }
  }
})
```

---

## Performance Considerations

### Database Indexes

```sql
-- TransactionService
CREATE INDEX idx_transactions_customer ON Transactions(CustomerId);
CREATE INDEX idx_transactions_date ON Transactions(TransactionDate);
CREATE INDEX idx_transactions_status ON Transactions(Status);

-- DisputeService
CREATE INDEX idx_disputes_customer ON Disputes(CustomerId);
CREATE INDEX idx_disputes_transaction ON Disputes(TransactionId);
CREATE INDEX idx_disputes_status ON Disputes(Status);
```

### Pagination

All list endpoints use cursor-based or offset pagination:

```csharp
var query = _context.Transactions
    .Where(t => t.CustomerId == customerId)
    .OrderByDescending(t => t.TransactionDate)
    .Skip((page - 1) * pageSize)
    .Take(pageSize);
```

### Connection Pooling

SQLite connection pooling is handled by EF Core.
Redis connections are pooled via StackExchange.Redis `ConnectionMultiplexer`.
