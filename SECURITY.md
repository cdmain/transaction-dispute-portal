# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 1.x     | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability, please report it privately:

1. **Do not** create a public GitHub issue
2. Email the maintainers directly or use GitHub's private vulnerability reporting
3. Include detailed steps to reproduce the issue
4. Allow up to 48 hours for an initial response

## Security Measures

### Authentication
- JWT tokens with secure secret (minimum 32 characters)
- BCrypt password hashing with secure defaults
- Token refresh mechanism with revocation support
- Rate limiting on authentication endpoints

### Data Protection
- HTTPS enforcement (HSTS)
- Input validation and sanitization
- SQL injection prevention via Entity Framework parameterized queries
- XSS protection with Content Security Policy

### Security Headers (OWASP 2025)
- `Strict-Transport-Security`
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `Content-Security-Policy`
- `Referrer-Policy: strict-origin-when-cross-origin`
- `Permissions-Policy`

### API Security
- Authorization on all endpoints
- Resource ownership verification
- CORS configuration
- Rate limiting (200 requests/minute per client)

## Configuration Requirements

### Production Deployment

1. **Set a secure JWT secret:**
   ```bash
   export JWT_SECRET=$(openssl rand -base64 32)
   ```

2. **Use HTTPS in production**

3. **Configure allowed CORS origins**

4. **Review and update default rate limits as needed**
