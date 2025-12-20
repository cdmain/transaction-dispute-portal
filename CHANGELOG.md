# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-20

### Added

- **Multi-Environment Deployment**
  - DEV, INT, QA, PROD environments with GitHub Pages
  - Branch-based deployment (dev → DEV/INT, release → QA/PROD)
  - Environment-specific banners and versioning

- **CI/CD Pipeline**
  - Continuous Integration with backend and frontend builds
  - Automated testing with 42 unit tests
  - Security audit for .NET dependencies
  - Multi-environment deployment with approval gates

- **Versioning System**
  - Semantic versioning with automatic version bumps
  - Conventional commits support
  - Automatic changelog generation

- **Contributing Guidelines**
  - Branch strategy documentation
  - Commit message conventions
  - Pull request templates
  - Code standards for .NET and Vue.js

- **Core Features**
  - Microservice architecture (Auth, Transaction, Dispute services)
  - YARP API Gateway with routing
  - Vue 3 frontend with TypeScript
  - TanStack Query for server state management
  - Zod schema validation
  - JWT authentication with refresh tokens
  - Redis caching
  - Demo mode with mock data

- **Infrastructure**
  - Docker Compose setup
  - Kubernetes manifests
  - GitHub Actions workflows

### Security

- JWT token authentication
- Password hashing with BCrypt
- Secure token refresh mechanism

---

## [Unreleased]

### Planned

- Integration tests
- E2E tests with Playwright
- OpenAPI documentation
- Rate limiting
- Audit logging
