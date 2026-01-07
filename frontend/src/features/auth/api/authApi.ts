import { z } from 'zod'
import { post } from '@/shared/api/client'
import { isDemoMode, mockAuthApi } from '@/services/mockApi'

// Schemas
export const LoginRequestSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1),
})

export const RegisterRequestSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8),
  firstName: z.string().min(1).max(50),
  lastName: z.string().min(1).max(50),
})

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

// Types
export type LoginRequest = z.infer<typeof LoginRequestSchema>
export type RegisterRequest = z.infer<typeof RegisterRequestSchema>
export type User = z.infer<typeof UserSchema>
export type AuthResponse = z.infer<typeof AuthResponseSchema>

// API
export async function login(data: LoginRequest): Promise<AuthResponse> {
  const validated = LoginRequestSchema.parse(data)
  if (isDemoMode()) {
    return AuthResponseSchema.parse(await mockAuthApi.login(validated.email, validated.password))
  }
  return AuthResponseSchema.parse(await post<AuthResponse>('/auth/login', validated))
}

export async function register(data: RegisterRequest): Promise<AuthResponse> {
  const validated = RegisterRequestSchema.parse(data)
  if (isDemoMode()) {
    return AuthResponseSchema.parse(await mockAuthApi.register(validated.email, validated.password, validated.firstName, validated.lastName))
  }
  return AuthResponseSchema.parse(await post<AuthResponse>('/auth/register', validated))
}

export async function refreshToken(token: string): Promise<AuthResponse> {
  if (isDemoMode()) {
    return AuthResponseSchema.parse(await mockAuthApi.refreshToken())
  }
  return AuthResponseSchema.parse(await post<AuthResponse>('/auth/refresh', { refreshToken: token }))
}

export async function logout(token: string): Promise<void> {
  if (isDemoMode()) {
    await mockAuthApi.logout()
    return
  }
  await post<void>('/auth/logout', { refreshToken: token })
}
