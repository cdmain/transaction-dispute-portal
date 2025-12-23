# Docker Setup

## Prerequisites

- [Docker](https://www.docker.com/get-started) installed and running

## Quick Start

```bash
# Build and start all services
docker compose up -d --build

# View logs
docker compose logs -f

# Stop all services
docker compose down
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

Data is stored in Docker volumes:
- `auth-data` - User accounts
- `transaction-data` - Transactions
- `dispute-data` - Disputes

## Reset Data

```bash
# Stop services and remove volumes
docker compose down -v

# Rebuild from scratch
docker compose up -d --build
```

## Troubleshooting

**Port 5000 conflict (macOS)**  
The API Gateway uses port 5050 to avoid conflicts with macOS AirPlay.

**Container not starting**  
```bash
docker compose logs <service-name>
```

**Rebuild a specific service**  
```bash
docker compose up -d --build <service-name>
```
