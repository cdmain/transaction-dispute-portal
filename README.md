# Transaction Dispute Portal

A production-ready microservice application for managing financial transaction disputes.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Vue 3](https://img.shields.io/badge/Vue-3.4-4FC08D?logo=vue.js&logoColor=white)](https://vuejs.org/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5.0-3178C6?logo=typescript&logoColor=white)](https://www.typescriptlang.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)

---

## Quick Start

### Demo Credentials

| Email | Password |
|-------|----------|
| `demo@example.com` | `Demo123!` |

### Docker (Recommended)

```bash
./build.sh
docker compose up -d
```

Open http://localhost:3000

### Stop

```bash
docker compose down
```

---

## Documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](ARCHITECTURE.md) | System design, error handling, validation, security |
| [TESTING.md](TESTING.md) | Unit tests, integration tests, manual testing |

---

## Architecture

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

## Project Structure

```
├── backend/
│   ├── ApiGateway/              # YARP reverse proxy
│   ├── AuthService/             # JWT authentication
│   ├── TransactionService/      # Transaction CRUD
│   ├── DisputeService/          # Dispute management
│   └── *Service.Tests/          # Unit tests
├── frontend/
│   ├── src/
│   │   ├── views/               # Page components
│   │   ├── composables/         # TanStack Query hooks
│   │   ├── schemas/             # Zod validation
│   │   └── api/                 # Axios client
├── k8s/                         # Kubernetes manifests
└── docker-compose.yml
```

---

## API Endpoints

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
| GET | `/api/transactions` | List transactions |
| GET | `/api/transactions/{id}` | Get transaction |

### Disputes

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/disputes` | List disputes |
| POST | `/api/disputes` | Create dispute |
| PUT | `/api/disputes/{id}/status` | Update status |
| POST | `/api/disputes/{id}/cancel` | Cancel dispute |

---

## Development

### Prerequisites

- .NET 8 SDK
- Node.js 18+ or Bun
- Docker (optional)

### Local Development

**Start all services:**
```bash
./start-local.sh
```

**Stop all services:**
```bash
./stop-local.sh
```

**Or run individually:**

```bash
# Backend (separate terminals)
cd backend/AuthService && dotnet run
cd backend/TransactionService && dotnet run
cd backend/DisputeService && dotnet run
cd backend/ApiGateway && dotnet run

# Frontend
cd frontend && bun install && bun dev
```

### Run Tests

```bash
cd backend && dotnet test
```

---

## Kubernetes

```bash
# Deploy
kubectl apply -f k8s/

# Remove
kubectl delete -f k8s/
```

---

## Tech Stack

| Layer | Technology |
|-------|------------|
| Frontend | Vue 3, TypeScript, TanStack Query, Zod, Tailwind CSS |
| API Gateway | ASP.NET Core 8, YARP |
| Services | ASP.NET Core 8, Entity Framework Core, SQLite |
| Auth | JWT, BCrypt |
| Cache | Redis |
| Testing | xUnit, Moq, FluentAssertions |

---

## License

MIT
