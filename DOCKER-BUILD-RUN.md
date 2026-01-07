# Container Setup

> **Note:** This project uses **containerd** as the container runtime via [Rancher Desktop](https://rancherdesktop.io/).
> All commands use `nerdctl` (the containerd CLI), which is Docker-compatible.

## Prerequisites

- [Rancher Desktop](https://rancherdesktop.io/) installed with **containerd** runtime selected
- OR Docker Desktop with Docker daemon running (substitute `nerdctl` with `docker`)

## Quick Start

```bash
# Build and start all services
nerdctl compose up -d --build

# View logs
nerdctl compose logs -f

# Stop all services
nerdctl compose down
```

## Access

| Service | URL |
|---------|-----|
| Frontend | http://localhost:3000 |
| API Gateway | http://localhost:5050 |

## Demo Credentials

| Email | Password |
|-------|----------|
| `demo@example.com` | `Demo123!` |

## Services

| Service | Port | Description |
|---------|------|-------------|
| frontend | 3000 | Vue.js web app |
| api-gateway | 5050 | YARP reverse proxy |
| auth-service | 5003 | JWT authentication |
| transaction-service | 5001 | Transaction management |
| dispute-service | 5002 | Dispute management |
| redis | 6379 | Caching |

## Data Persistence

Data is stored in container volumes:
- `auth-data` - User accounts
- `transaction-data` - Transactions
- `dispute-data` - Disputes

## Reset Data

```bash
# Stop services and remove volumes
nerdctl compose down -v

# Rebuild from scratch
nerdctl compose up -d --build
```

## Troubleshooting

**Port 5000 conflict (macOS)**  
The API Gateway uses port 5050 to avoid conflicts with macOS AirPlay.

**Container not starting**  
```bash
nerdctl compose logs <service-name>
```

**Rebuild a specific service**  
```bash
nerdctl compose up -d --build <service-name>
```

**"Cannot connect to Docker daemon" error**  
You're using containerd, not Docker. Use `nerdctl` instead of `docker`:
```bash
nerdctl compose up -d --build
```
