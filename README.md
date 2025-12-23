# Transaction Dispute Portal

A microservice application for managing financial transaction disputes.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue-3.4-4FC08D?logo=vue.js&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![containerd](https://img.shields.io/badge/containerd-Ready-575757?logo=containerd&logoColor=white)](https://containerd.io/)

---

## Quick Start

> Uses **containerd** via [Rancher Desktop](https://rancherdesktop.io/). If using Docker, replace `nerdctl` with `docker`.

```bash
nerdctl compose up -d --build
```

Open http://localhost:3000

**Demo:** `demo@example.com` / `Demo123!`

See [DOCKER-BUILD-RUN.md](DOCKER-BUILD-RUN.md) for full setup instructions.

---

## Live Demo

| Environment | URL |
|-------------|-----|
| DEV | [cdmain.github.io/transaction-dispute-portal/dev/](https://cdmain.github.io/transaction-dispute-portal/dev/) |
| PROD | [cdmain.github.io/transaction-dispute-portal/prod/](https://cdmain.github.io/transaction-dispute-portal/prod/) |

---

## Documentation

| Document | Description |
|----------|-------------|
| [DOCKER-BUILD-RUN.md](DOCKER-BUILD-RUN.md) | Docker build and run instructions |
| [ARCHITECTURE.md](ARCHITECTURE.md) | System design and security |
| [TESTING.md](TESTING.md) | Testing guide |
| [CONTRIBUTING.md](CONTRIBUTING.md) | Contribution guidelines |
| [CHANGELOG.md](CHANGELOG.md) | Version history |

---

## Architecture

```
Frontend (Vue 3)  →  API Gateway (YARP)  →  Microservices
    :3000                :5050               :5001-5003
                                                 │
                                              Redis
                                               :6379
```

**Services:**
- **Auth Service** - JWT authentication
- **Transaction Service** - Transaction management
- **Dispute Service** - Dispute lifecycle

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Vue 3, TypeScript, TanStack Query, Tailwind CSS |
| Backend | ASP.NET Core 8, Entity Framework Core, SQLite |
| Infrastructure | Docker, Redis, YARP |

---

## API Endpoints

### Auth
- `POST /api/auth/register` - Create account
- `POST /api/auth/login` - Login
- `POST /api/auth/refresh` - Refresh token

### Transactions
- `GET /api/transactions` - List transactions
- `POST /api/transactions/seed` - Generate demo data

### Disputes
- `GET /api/disputes` - List disputes
- `POST /api/disputes` - Create dispute
- `PUT /api/disputes/{id}/status` - Update status
- `POST /api/disputes/{id}/cancel` - Cancel dispute

---

## Development

```bash
# Run tests
cd backend && dotnet test

# Local development
./start-local.sh
```

---

## License

MIT
