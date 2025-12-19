# Testing Documentation

This document provides comprehensive testing guidelines for the Transaction Dispute Portal.

---

## Table of Contents

- [Overview](#overview)
- [Backend Testing](#backend-testing)
  - [Test Framework](#test-framework)
  - [Running Tests](#running-tests)
  - [Test Structure](#test-structure)
  - [Writing Tests](#writing-tests)
- [Frontend Testing](#frontend-testing)
- [Integration Testing](#integration-testing)
- [Manual Testing](#manual-testing)

---

## Overview

### Testing Pyramid

```
        ┌───────────────┐
        │   E2E Tests   │  ← Manual/Playwright
        ├───────────────┤
        │  Integration  │  ← API Testing
        ├───────────────┤
        │  Unit Tests   │  ← xUnit/Vitest
        └───────────────┘
```

### Test Coverage Goals

| Layer | Target Coverage |
|-------|-----------------|
| Services (Business Logic) | 80%+ |
| Controllers (API) | 70%+ |
| Utilities | 90%+ |

---

## Backend Testing

### Test Framework

| Package | Purpose |
|---------|---------|
| **xUnit** | Test framework |
| **Moq** | Mocking dependencies |
| **FluentAssertions** | Readable assertions |
| **Microsoft.EntityFrameworkCore.InMemory** | In-memory database |

### Running Tests

**All Tests:**
```bash
cd backend
dotnet test
```

**Specific Project:**
```bash
cd backend/AuthService.Tests
dotnet test
```

**With Verbosity:**
```bash
dotnet test --verbosity normal
```

**With Coverage Report:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Filter by Test Name:**
```bash
dotnet test --filter "FullyQualifiedName~Login"
```

### Test Structure

```
backend/
├── AuthService.Tests/
│   ├── AuthService.Tests.csproj
│   └── AuthServiceTests.cs
├── TransactionService.Tests/
│   ├── TransactionService.Tests.csproj
│   └── TransactionsControllerTests.cs
└── DisputeService.Tests/
    ├── DisputeService.Tests.csproj
    └── DisputesControllerTests.cs
```

### Test Categories

#### AuthService Tests

| Test | Description |
|------|-------------|
| `Login_WithValidCredentials_ReturnsToken` | Successful login returns JWT |
| `Login_WithInvalidPassword_ReturnsUnauthorized` | Wrong password rejected |
| `Login_WithNonexistentUser_ReturnsUnauthorized` | Unknown email rejected |
| `Register_WithValidData_CreatesUser` | New user registration |
| `Register_WithExistingEmail_ReturnsBadRequest` | Duplicate email prevented |
| `RefreshToken_WithValidToken_ReturnsNewToken` | Token refresh works |
| `RefreshToken_WithExpiredToken_ReturnsUnauthorized` | Expired refresh rejected |

#### TransactionService Tests

| Test | Description |
|------|-------------|
| `GetTransactions_ReturnsPagedResult` | Pagination works correctly |
| `GetTransactions_WithFilters_ReturnsFiltered` | Filtering by status/type |
| `GetTransactionById_ExistingId_ReturnsTransaction` | Single transaction retrieval |
| `GetTransactionById_NonexistentId_ReturnsNotFound` | 404 for missing ID |
| `GetTransactionsByCustomer_ReturnsOnlyCustomerTransactions` | Customer isolation |

#### DisputeService Tests

| Test | Description |
|------|-------------|
| `CreateDispute_WithValidData_ReturnsCreated` | Dispute creation |
| `CreateDispute_DuplicateActive_ReturnsBadRequest` | Prevents duplicate disputes |
| `UpdateStatus_ValidTransition_ReturnsUpdated` | Status update workflow |
| `UpdateStatus_InvalidTransition_ReturnsBadRequest` | Invalid status rejected |
| `CancelDispute_PendingDispute_ReturnsCancelled` | Cancellation works |
| `CancelDispute_ResolvedDispute_ReturnsBadRequest` | Can't cancel resolved |

### Writing Tests

#### Test Template

```csharp
using FluentAssertions;
using Moq;
using Xunit;

public class MyServiceTests
{
    private readonly Mock<IMyDependency> _mockDependency;
    private readonly MyService _sut; // System Under Test

    public MyServiceTests()
    {
        _mockDependency = new Mock<IMyDependency>();
        _sut = new MyService(_mockDependency.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var input = new InputDto { /* ... */ };
        _mockDependency
            .Setup(x => x.SomeMethod(It.IsAny<string>()))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _sut.MethodUnderTest(input);

        // Assert
        result.Should().NotBeNull();
        result.Property.Should().Be(expectedValue);
        _mockDependency.Verify(x => x.SomeMethod(input.Id), Times.Once);
    }

    [Theory]
    [InlineData("valid@email.com", true)]
    [InlineData("invalid-email", false)]
    public void ValidateEmail_VariousInputs_ReturnsExpected(string email, bool expected)
    {
        // Arrange & Act
        var result = _sut.ValidateEmail(email);

        // Assert
        result.Should().Be(expected);
    }
}
```

#### In-Memory Database Testing

```csharp
public class DisputeServiceTests : IDisposable
{
    private readonly DisputeDbContext _context;
    private readonly DisputeService _sut;

    public DisputeServiceTests()
    {
        var options = new DbContextOptionsBuilder<DisputeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new DisputeDbContext(options);
        _sut = new DisputeService(_context);

        // Seed test data
        _context.Disputes.Add(new Dispute { /* ... */ });
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task GetDispute_ExistingId_ReturnsDispute()
    {
        // Act
        var result = await _sut.GetByIdAsync("test-id");

        // Assert
        result.Should().NotBeNull();
    }
}
```

#### Mocking HttpContext for Auth

```csharp
private static ClaimsPrincipal CreateUserPrincipal(string customerId)
{
    var claims = new List<Claim>
    {
        new Claim("customer_id", customerId),
        new Claim(ClaimTypes.NameIdentifier, "user-id"),
        new Claim(ClaimTypes.Email, "test@example.com")
    };
    return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
}

[Fact]
public async Task GetTransactions_AuthenticatedUser_ReturnsUserTransactions()
{
    // Arrange
    var controller = new TransactionsController(_service, _cache);
    controller.ControllerContext = new ControllerContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = CreateUserPrincipal("CUST001")
        }
    };

    // Act
    var result = await controller.GetTransactions();

    // Assert
    result.Should().BeOfType<OkObjectResult>();
}
```

---

## Frontend Testing

### Framework

| Package | Purpose |
|---------|---------|
| Vitest | Test runner |
| Vue Test Utils | Component testing |
| MSW | API mocking |

### Running Tests

```bash
cd frontend
bun run test
```

### Test Examples

#### Component Test

```typescript
import { mount } from '@vue/test-utils'
import { describe, it, expect } from 'vitest'
import LoginView from '@/views/LoginView.vue'

describe('LoginView', () => {
  it('renders login form', () => {
    const wrapper = mount(LoginView)
    expect(wrapper.find('input[type="email"]').exists()).toBe(true)
    expect(wrapper.find('input[type="password"]').exists()).toBe(true)
  })

  it('shows validation error for invalid email', async () => {
    const wrapper = mount(LoginView)
    await wrapper.find('input[type="email"]').setValue('invalid')
    await wrapper.find('form').trigger('submit')
    expect(wrapper.text()).toContain('valid email')
  })
})
```

#### Zod Schema Test

```typescript
import { describe, it, expect } from 'vitest'
import { CreateDisputeSchema } from '@/schemas'

describe('CreateDisputeSchema', () => {
  it('validates correct data', () => {
    const validData = {
      transactionId: '123e4567-e89b-12d3-a456-426614174000',
      reason: 'Unauthorized',
      description: 'I did not authorize this transaction',
      disputedAmount: 100.50
    }
    
    const result = CreateDisputeSchema.safeParse(validData)
    expect(result.success).toBe(true)
  })

  it('rejects invalid reason', () => {
    const invalidData = {
      transactionId: '123e4567-e89b-12d3-a456-426614174000',
      reason: 'InvalidReason',
      description: 'Test description',
      disputedAmount: 100
    }
    
    const result = CreateDisputeSchema.safeParse(invalidData)
    expect(result.success).toBe(false)
  })
})
```

---

## Integration Testing

### API Testing with curl

```bash
# 1. Login
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"demo@example.com","password":"Demo123!"}' | jq -r '.token')

# 2. Get Transactions
curl -s http://localhost:5000/api/transactions \
  -H "Authorization: Bearer $TOKEN" | jq

# 3. Create Dispute
curl -s -X POST http://localhost:5000/api/disputes \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "transactionId": "TRANSACTION_ID",
    "reason": 1,
    "description": "Unauthorized charge",
    "disputedAmount": 50.00
  }' | jq

# 4. Check Dispute Status
curl -s http://localhost:5000/api/disputes \
  -H "Authorization: Bearer $TOKEN" | jq
```

### Health Check Script

```bash
#!/bin/bash
# health-check.sh

services=("5000:API Gateway" "5001:Transaction" "5002:Dispute" "5003:Auth")

for service in "${services[@]}"; do
  port="${service%%:*}"
  name="${service##*:}"
  
  if curl -s "http://localhost:$port/health" > /dev/null; then
    echo "✓ $name Service (port $port): Healthy"
  else
    echo "✗ $name Service (port $port): Down"
  fi
done
```

---

## Manual Testing

### Test Scenarios

#### Authentication Flow

1. [ ] Navigate to login page
2. [ ] Enter invalid credentials → Error message displayed
3. [ ] Enter valid credentials → Redirect to dashboard
4. [ ] Refresh page → Stay logged in
5. [ ] Click logout → Redirect to login

#### Transaction Viewing

1. [ ] View transaction list
2. [ ] Filter by status
3. [ ] Filter by date range
4. [ ] Search by description
5. [ ] Pagination works correctly

#### Dispute Creation

1. [ ] Select transaction
2. [ ] Click "Dispute" button
3. [ ] Fill dispute form
4. [ ] Submit with invalid data → Validation errors
5. [ ] Submit with valid data → Success message
6. [ ] View dispute in list

#### Dispute Management

1. [ ] View dispute details
2. [ ] Update dispute status (admin)
3. [ ] Cancel pending dispute
4. [ ] Try to cancel resolved dispute → Error

### Browser Testing Checklist

| Browser | Status |
|---------|--------|
| Chrome (latest) | ☐ |
| Firefox (latest) | ☐ |
| Safari (latest) | ☐ |
| Edge (latest) | ☐ |
| Mobile Chrome | ☐ |
| Mobile Safari | ☐ |

---

## CI/CD Integration

GitHub Actions workflows are located in `.github/workflows/`:

| Workflow | File | Purpose |
|----------|------|---------|
| **CI** | `ci.yml` | Build & test on every push |
| **CD** | `cd.yml` | Deploy to dev/int/prod |

### Pipeline Overview

```
┌──────────────────────────────────────────────────────────────┐
│                    CI Pipeline (ci.yml)                       │
│         Runs on: All branches, All PRs                        │
├─────────────────────────┬────────────────────────────────────┤
│      Backend Tests      │       Frontend Build               │
│  • Restore & Build      │  • Install dependencies            │
│  • Run unit tests       │  • Type check                      │
│                         │  • Build                           │
└─────────────────────────┴────────────────────────────────────┘
                              │
                              ▼
┌──────────────────────────────────────────────────────────────┐
│                    CD Pipeline (cd.yml)                       │
│         Runs on: develop (→DEV), main (→INT→PROD)             │
├──────────────────────────────────────────────────────────────┤
│                      Build Images                             │
│  • API Gateway, Auth, Transaction, Dispute, Frontend          │
│  • Push to GitHub Container Registry                          │
└──────────────────────────────────────────────────────────────┘
          │                    │                    │
          ▼                    ▼                    ▼
    ┌──────────┐        ┌──────────┐        ┌──────────┐
    │   DEV    │        │   INT    │        │   PROD   │
    │          │        │ (Staging)│   ──►  │          │
    │ develop  │        │   main   │        │  Manual  │
    │  branch  │        │  branch  │        │ Approval │
    └──────────┘        └──────────┘        └──────────┘
```

### Environment Strategy

| Environment | Branch | Trigger | Approval |
|-------------|--------|---------|----------|
| **DEV** | `develop` | Automatic on push | None |
| **INT** (Staging) | `main` | Automatic on push | None |
| **PROD** | `main` | After INT succeeds | **Required** |

### Branching Strategy (GitFlow)

```
main ─────●─────────●─────────●───────── (production-ready)
          │         ↑         ↑
          │         │         │
develop ──●────●────●────●────●───────── (integration)
               │         │
               │         │
feature/* ─────●─────────●─────────────── (feature work)
```

### Required GitHub Setup

1. **Create Environments** (Settings → Environments):
   - `dev` - No protection rules
   - `int` - No protection rules  
   - `prod` - Add **required reviewers**

2. **Add Secrets** (Settings → Secrets):

| Secret | Environment | Purpose |
|--------|-------------|---------|
| `KUBE_CONFIG_DEV` | dev | Kubernetes access |
| `KUBE_CONFIG_INT` | int | Kubernetes access |
| `KUBE_CONFIG_PROD` | prod | Kubernetes access |

### Manual Deployment

Trigger deployment manually via **Actions → CD → Run workflow**:

```
Select environment: [dev | int | prod]
```
| `main` | ✅ | ✅ | ✅ | ✅ (manual approval) |
| `develop` | ✅ | ❌ | ❌ | ❌ |
| PR to `main` | ✅ | ❌ | ❌ | ❌ |

---

## Troubleshooting Tests

### Common Issues

**Tests not finding dependencies:**
```bash
dotnet restore
```

**In-memory database conflicts:**
- Use unique database name per test class
- Implement `IDisposable` to clean up

**Flaky async tests:**
- Always `await` async operations
- Use proper timeouts
- Don't rely on timing

**Mock verification failing:**
- Check parameter matchers (`It.IsAny<T>()`)
- Verify method signatures match exactly
