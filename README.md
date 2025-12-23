# Transaction Dispute Portal

A production-ready microservice application for managing financial transaction disputes.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue-3.4-4FC08D?logo=vue.js&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![CI](https://github.com/cdmain/transaction-dispute-portal/actions/workflows/ci.yml/badge.svg)](https://github.com/cdmain/transaction-dispute-portal/actions/workflows/ci.yml)
[![Deploy](https://github.com/cdmain/transaction-dispute-portal/actions/workflows/deploy.yml/badge.svg)](https://github.com/cdmain/transaction-dispute-portal/actions/workflows/deploy.yml)
[![Version](https://img.shields.io/github/v/tag/cdmain/transaction-dispute-portal?label=version)](https://github.com/cdmain/transaction-dispute-portal/tags)

---

## 🌍 Live Environments

| Environment | URL | Branch | Status |
|-------------|-----|--------|--------|
| **DEV** | [🔗 /dev/](https://cdmain.github.io/transaction-dispute-portal/dev/) | `dev` | Latest features |
| **INT** | [🔗 /int/](https://cdmain.github.io/transaction-dispute-portal/int/) | `dev` | Integration testing |
| **QA** | [🔗 /qa/](https://cdmain.github.io/transaction-dispute-portal/qa/) | `main` | Pre-release testing |
| **PROD** | [🔗 /prod/](https://cdmain.github.io/transaction-dispute-portal/prod/) | `main` | Stable release |

> **Demo Mode:** All environments use mock data. Use `demo@example.com` / `Demo123!` to sign in.

---

## 🚀 Quick Start

### Demo Credentials

| Email | Password |
|-------|----------|
| `demo@example.com` | `Demo123!` |

### Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/cdmain/transaction-dispute-portal.git
cd transaction-dispute-portal

# Copy environment file
cp .env.example .env

# Start all services
docker compose up -d --build
```

Open http://localhost:3000

### Stop

```bash
docker compose down
```

---

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System design, error handling, validation, security |
| [TESTING.md](TESTING.md) | Unit tests, integration tests, manual testing |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution guidelines, branch strategy, code standards |
| [CHANGELOG.md](CHANGELOG.md) | Version history and release notes |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    Frontend (Vue 3)                      │
│                     Port: 3000                           │
└─────────────────────────────────────────────────────────┘
                          │
                          ▼
┌─────────────────────────────────────────────────────────┐
│                 API Gateway (YARP)                       │
│                     Port: 5000                           │
└─────────────────────────────────────────────────────────┘
         │                │                │
         ▼                ▼                ▼
┌─────────────┐  ┌─────────────┐  ┌─────────────┐
│    Auth     │  │ Transaction │  │   Dispute   │
│   Service   │  │   Service   │  │   Service   │
│   :5003     │  │   :5001     │  │   :5002     │
└─────────────┘  └─────────────┘  └─────────────┘
                          │
                          ▼
                 ┌─────────────┐
                 │    Redis    │
                 │    :6379    │
                 └─────────────┘
```

---

## 📁 Project Structure

```
├── .github/
│   └── workflows/           # CI/CD pipelines
│       ├── ci.yml           # Continuous Integration
│       ├── deploy.yml       # Multi-environment deployment
│       └── version.yml      # Semantic versioning
├── backend/
│   ├── ApiGateway/          # YARP reverse proxy
│   ├── AuthService/         # JWT authentication
│   ├── TransactionService/  # Transaction CRUD
│   ├── DisputeService/      # Dispute management
│   └── *Service.Tests/      # Unit tests (42 total)
├── frontend/
│   ├── src/
│   │   ├── views/           # Page components
│   │   ├── composables/     # TanStack Query hooks
│   │   ├── schemas/         # Zod validation
│   │   └── services/        # API client + mock data
├── k8s/                     # Kubernetes manifests
├── VERSION                  # Semantic version file
└── docker-compose.yml
```

---

## 🔄 Deployment Pipeline

### Branch Strategy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                             │
│  feature/* ──┬──▶ dev ──────────▶ DEV/INT (development testing)            │
│  bugfix/*  ──┘        (push)          │                                     │
│                                       │                                     │
│                                       ▼ (PR to main)                        │
│                                                                             │
│                     main ──────────▶ QA/PROD (production tracking)         │
│                           (push)                                            │
│                                                                             │
│  🔄 Rollback: main branch tracks all production changes                    │
│               git revert + push to main redeploys                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Environment Promotion

| Branch | Deploys To | Purpose |
|--------|------------|----------|
| `dev` | DEV + INT | Development & integration testing |
| `main` | QA + PROD | Production releases (rollback tracking) |

---

## 🤝 Contributing

We welcome contributions! Please read our [CONTRIBUTING.md](CONTRIBUTING.md) for details on:

- 🌿 **Branch Strategy** - Feature branches, naming conventions
- 📝 **Commit Guidelines** - Conventional commits format
- 🔀 **Pull Request Process** - Review requirements, CI checks
- 🎨 **Code Standards** - .NET and Vue.js style guides
- 🧪 **Testing Requirements** - Unit test coverage

### Quick Contribution Steps

```bash
# 1. Fork and clone
git clone https://github.com/YOUR_USERNAME/transaction-dispute-portal.git

# 2. Create feature branch from dev
git checkout -b feature/your-feature dev

# 3. Make changes and commit
git commit -m "feat(scope): add new feature"

# 4. Push and create PR to dev branch
git push origin feature/your-feature
```

---

## 🔌 API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Create account |
| POST | `/api/auth/login` | Login |
| POST | `/api/auth/refresh` | Refresh token |
| POST | `/api/auth/logout` | Logout |

### Transactions

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/transactions` | List transactions (paginated) |
| GET | `/api/transactions/{id}` | Get transaction by ID |

### Disputes

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/disputes` | List disputes (paginated) |
| POST | `/api/disputes` | Create dispute |
| PUT | `/api/disputes/{id}/status` | Update status |
| POST | `/api/disputes/{id}/cancel` | Cancel dispute |
| GET | `/api/disputes/statistics` | Get dispute statistics |

---

## 🛠️ Development

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Bun](https://bun.sh/) (or Node.js 18+)
- [Docker](https://www.docker.com/) (optional)

### Local Development

```bash
# Start all services
./start-local.sh

# Stop all services
./stop-local.sh
```

### Run Tests

```bash
# Backend (42 unit tests)
cd backend && dotnet test

# Frontend type check
cd frontend && bun run type-check
```

---

## 🏷️ Versioning

This project follows [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backwards compatible)
- **PATCH**: Bug fixes (backwards compatible)

Version is automatically bumped when merging to `release` branch based on commit messages:
- `feat!:` or `BREAKING CHANGE` → Major
- `feat:` → Minor
- `fix:`, `docs:`, etc. → Patch

---

## 🧰 Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Vue 3, TypeScript, TanStack Query, Zod, Tailwind CSS |
| API Gateway | ASP.NET Core 8, YARP |
| Services | ASP.NET Core 8, Entity Framework Core, SQLite |
| Auth | JWT, BCrypt |
| Cache | Redis |
| Testing | xUnit, Moq, FluentAssertions |
| CI/CD | GitHub Actions |
| Hosting | GitHub Pages (demo) |

---

## 📜 License

MIT License - see [LICENSE](LICENSE) for details.

---

## 🙏 Acknowledgments

- Built with [Vue.js](https://vuejs.org/)
- Backend powered by [.NET 8](https://dotnet.microsoft.com/)
- Icons from [Heroicons](https://heroicons.com/)
- UI components from [Headless UI](https://headlessui.com/)
