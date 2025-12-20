# Contributing to Transaction Dispute Portal

Thank you for your interest in contributing to the Transaction Dispute Portal! This document provides guidelines and instructions for contributing.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Branch Strategy](#branch-strategy)
- [Commit Guidelines](#commit-guidelines)
- [Pull Request Process](#pull-request-process)
- [Code Standards](#code-standards)
- [Testing](#testing)
- [Environment Deployments](#environment-deployments)

---

## 📜 Code of Conduct

By participating in this project, you agree to maintain a respectful and inclusive environment. We expect all contributors to:

- Be respectful of differing viewpoints and experiences
- Accept constructive criticism gracefully
- Focus on what is best for the community
- Show empathy towards other community members

---

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Bun](https://bun.sh/) (v1.0+)
- [Docker](https://www.docker.com/) (optional, for containerized development)
- [Git](https://git-scm.com/)

### Local Setup

1. **Fork the repository**
   ```bash
   # Click "Fork" on GitHub, then clone your fork
   git clone https://github.com/YOUR_USERNAME/transaction-dispute-portal.git
   cd transaction-dispute-portal
   ```

2. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/cdmain/transaction-dispute-portal.git
   ```

3. **Install dependencies**
   ```bash
   # Backend
   cd backend
   dotnet restore

   # Frontend
   cd ../frontend
   bun install
   ```

4. **Start development servers**
   ```bash
   # Using the convenience script
   ./start-local.sh

   # Or manually
   # Terminal 1: Backend services
   cd backend && dotnet run --project ApiGateway

   # Terminal 2: Frontend
   cd frontend && bun dev
   ```

5. **Access the application**
   - Frontend: http://localhost:3000
   - API Gateway: http://localhost:5000

---

## 🔄 Development Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│                    Development Workflow                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   1. Create feature branch from 'dev'                           │
│      └── git checkout -b feature/your-feature dev               │
│                                                                  │
│   2. Make changes and commit                                    │
│      └── git commit -m "feat: add new feature"                  │
│                                                                  │
│   3. Push and create PR to 'dev'                                │
│      └── git push origin feature/your-feature                   │
│                                                                  │
│   4. After review, merge to 'dev'                               │
│      └── Deploys to DEV + INT                                   │
│                                                                  │
│   5. Create PR from 'dev' to 'main'                             │
│      └── Deploys to QA + PROD                                   │
│                                                                  │
│   6. Rollback (if needed)                                       │
│      └── git revert + push to main                              │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🌿 Branch Strategy

| Branch | Purpose | Deploys To | Protection |
|--------|---------|------------|------------|
| `main` | Production releases | QA, PROD | Requires PR + approval |
| `dev` | Active development | DEV, INT | Requires PR |
| `feature/*` | New features | - | - |
| `bugfix/*` | Bug fixes | - | - |
| `hotfix/*` | Production fixes | - | - |

> 💡 **Rollback Strategy**: The `main` branch tracks all production changes. To rollback, simply `git revert` the problematic commit and push to `main`.

### Branch Naming Conventions

```
feature/short-description    # New features
bugfix/issue-number-desc     # Bug fixes
hotfix/critical-fix          # Production hotfixes
docs/update-readme           # Documentation changes
refactor/component-name      # Code refactoring
test/add-unit-tests          # Test additions
```

---

## 📝 Commit Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/) specification.

### Commit Format

```
<type>(<scope>): <description>

[optional body]

[optional footer(s)]
```

### Commit Types

| Type | Description | Example |
|------|-------------|---------|
| `feat` | New feature | `feat(disputes): add dispute filtering` |
| `fix` | Bug fix | `fix(auth): resolve token refresh issue` |
| `docs` | Documentation | `docs: update API documentation` |
| `style` | Formatting | `style: fix indentation in LoginView` |
| `refactor` | Code restructuring | `refactor(api): simplify error handling` |
| `test` | Adding tests | `test(disputes): add unit tests` |
| `chore` | Maintenance | `chore(deps): update dependencies` |
| `perf` | Performance | `perf(queries): optimize transaction queries` |
| `ci` | CI/CD changes | `ci: add deployment workflow` |

### Examples

```bash
# Feature with scope
git commit -m "feat(transactions): add date range filter"

# Bug fix with issue reference
git commit -m "fix(auth): prevent token expiration race condition

Closes #123"

# Breaking change
git commit -m "feat(api)!: change pagination response format

BREAKING CHANGE: PagedResult now includes hasNextPage instead of totalPages"
```

---

## 🔀 Pull Request Process

### Before Submitting

1. **Sync with upstream**
   ```bash
   git fetch upstream
   git rebase upstream/dev
   ```

2. **Run tests locally**
   ```bash
   # Backend tests
   cd backend && dotnet test

   # Frontend type check
   cd frontend && bun run type-check
   ```

3. **Build successfully**
   ```bash
   cd frontend && bun run build
   ```

### PR Requirements

- [ ] Descriptive title following commit conventions
- [ ] Description of changes with context
- [ ] Link to related issue(s)
- [ ] Tests added/updated (if applicable)
- [ ] Documentation updated (if applicable)
- [ ] No merge conflicts
- [ ] CI pipeline passing

### PR Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix (non-breaking change fixing an issue)
- [ ] New feature (non-breaking change adding functionality)
- [ ] Breaking change (fix or feature causing existing functionality to change)
- [ ] Documentation update

## How Has This Been Tested?
Describe the tests you ran

## Checklist
- [ ] My code follows the project's style guidelines
- [ ] I have performed a self-review
- [ ] I have added tests that prove my fix/feature works
- [ ] New and existing unit tests pass locally
- [ ] I have updated the documentation accordingly
```

---

## 🎨 Code Standards

### Backend (.NET)

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation for public APIs
- Keep methods focused and under 30 lines
- Use async/await for I/O operations

```csharp
/// <summary>
/// Retrieves a dispute by its unique identifier.
/// </summary>
/// <param name="id">The dispute ID</param>
/// <returns>The dispute if found; otherwise, null</returns>
public async Task<Dispute?> GetByIdAsync(string id)
{
    return await _context.Disputes
        .Include(d => d.Transaction)
        .FirstOrDefaultAsync(d => d.Id == id);
}
```

### Frontend (Vue + TypeScript)

- Use Composition API with `<script setup>`
- Follow Vue.js Style Guide (Priority A & B rules)
- Use TypeScript strict mode
- Prefer Zod for runtime validation
- Use TanStack Query for server state

```vue
<script setup lang="ts">
import { computed } from 'vue'
import { useQuery } from '@tanstack/vue-query'
import type { Transaction } from '@/types'

const props = defineProps<{
  transactionId: string
}>()

const { data, isLoading } = useQuery({
  queryKey: ['transaction', props.transactionId],
  queryFn: () => fetchTransaction(props.transactionId)
})
</script>
```

---

## 🧪 Testing

### Backend Testing

```bash
# Run all tests
cd backend && dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test TransactionService.Tests
```

### Frontend Testing

```bash
# Type checking
cd frontend && bun run type-check

# Build verification
bun run build
```

### Test Naming Convention

```csharp
// Method_Scenario_ExpectedResult
[Fact]
public async Task GetById_WithValidId_ReturnsDispute()
{
    // Arrange
    // Act
    // Assert
}
```

---

## 🌍 Environment Deployments

### Deployment Pipeline

```
┌──────────┐     ┌──────────┐     ┌──────────┐     ┌──────────┐
│   DEV    │────▶│   INT    │     │    QA    │────▶│   PROD   │
│  (auto)  │     │(approval)│     │  (auto)  │     │(approval)│
└──────────┘     └──────────┘     └──────────┘     └──────────┘
      │                │                │                │
      └────── dev branch ───────┘      └───── release branch ────┘
```

### Environment URLs

| Environment | URL | Purpose |
|-------------|-----|---------|
| DEV | `/dev/` | Latest development builds |
| INT | `/int/` | Integration testing |
| QA | `/qa/` | Quality assurance |
| PROD | `/` | Production release |

### Manual Deployment

You can trigger manual deployments from the Actions tab using the workflow dispatch feature.

---

## ❓ Questions?

If you have questions, please:

1. Check existing [Issues](https://github.com/cdmain/transaction-dispute-portal/issues)
2. Search [Discussions](https://github.com/cdmain/transaction-dispute-portal/discussions)
3. Open a new issue with the `question` label

---

## 📄 License

By contributing, you agree that your contributions will be licensed under the same license as the project.

---

Thank you for contributing! 🎉
