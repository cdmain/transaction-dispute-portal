/**
 * Auth Endpoints
 * 
 * All authentication-related API calls with Zod validation.
 * Each endpoint validates request/response data at runtime.
 */
import { z } from 'zod'
import { post } from './client'
import { isDemoMode, mockAuthApi } from '@/services/mockApi'

// ─────────────────────────────────────────────────────────────────────────────
// Request Schemas
// ─────────────────────────────────────────────────────────────────────────────

export const LoginRequestSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z.string().min(1, 'Password is required'),
})

export const RegisterRequestSchema = z.object({
  email: z.string().email('Invalid email address'),
  password: z
    .string()
    .min(8, 'Password must be at least 8 characters')
    .regex(/[A-Z]/, 'Password must contain uppercase letter')
    .regex(/[a-z]/, 'Password must contain lowercase letter')
    .regex(/[0-9]/, 'Password must contain a number')
    .regex(/[^A-Za-z0-9]/, 'Password must contain special character'),
  firstName: z.string().min(1, 'First name is required').max(50),
  lastName: z.string().min(1, 'Last name is required').max(50),
})

export const RefreshTokenRequestSchema = z.object({
  refreshToken: z.string().min(1, 'Refresh token is required'),
})

// ─────────────────────────────────────────────────────────────────────────────
// Response Schemas
// ─────────────────────────────────────────────────────────────────────────────

export const UserSchema = z.object({
  id: z.string(),
  email: z.string().email(),
  firstName: z.string(),
  lastName: z.string(),
  customerId: z.string(),
})

export const AuthResponseSchema = z.object({
  token: z.string(),
  refreshToken: z.string(),
  expiresAt: z.string(),
  user: UserSchema,
})

// ─────────────────────────────────────────────────────────────────────────────
// Types (inferred from schemas)
// ─────────────────────────────────────────────────────────────────────────────

export type LoginRequest = z.infer<typeof LoginRequestSchema>
export type RegisterRequest = z.infer<typeof RegisterRequestSchema>
export type RefreshTokenRequest = z.infer<typeof RefreshTokenRequestSchema>
export type User = z.infer<typeof UserSchema>
export type AuthResponse = z.infer<typeof AuthResponseSchema>

// ─────────────────────────────────────────────────────────────────────────────
// API Functions
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Login with email and password
 */
export async function login(data: LoginRequest): Promise<AuthResponse> {
  // Validate request data
  const validated = LoginRequestSchema.parse(data)
  
  if (isDemoMode()) {
    const response = await mockAuthApi.login(validated.email, validated.password)
    return AuthResponseSchema.parse(response)
  }
  
  const response = await post<AuthResponse>('/auth/login', validated)
  return AuthResponseSchema.parse(response)
}

/**
 * Register new account
 */
export async function register(data: RegisterRequest): Promise<AuthResponse> {
  // Validate request data
  const validated = RegisterRequestSchema.parse(data)
  
  if (isDemoMode()) {
    const response = await mockAuthApi.register(
      validated.email,
      validated.password,
      validated.firstName,
      validated.lastName
    )
    return AuthResponseSchema.parse(response)
  }
  
  const response = await post<AuthResponse>('/auth/register', validated)
  return AuthResponseSchema.parse(response)
}

/**
 * Refresh access token
 */
export async function refreshToken(token: string): Promise<AuthResponse> {
  const validated = RefreshTokenRequestSchema.parse({ refreshToken: token })
  
  if (isDemoMode()) {
    const response = await mockAuthApi.refreshToken()
    return AuthResponseSchema.parse(response)
  }
  
  const response = await post<AuthResponse>('/auth/refresh', validated)
  return AuthResponseSchema.parse(response)
}

/**
 * Logout (invalidate refresh token)
 */
export async function logout(token: string): Promise<void> {
  if (isDemoMode()) {
    await mockAuthApi.logout()
    return
  }
  
  await post<void>('/auth/logout', { refreshToken: token })
}
