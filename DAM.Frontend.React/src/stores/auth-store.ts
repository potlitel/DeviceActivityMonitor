import { create } from 'zustand'
import { getCookie, setCookie, removeCookie } from '@/lib/cookies'

const ACCESS_TOKEN = 'dam_access_token'

export interface AuthUser {
  id: string
  username: string
  email: string
  role: string
}

interface AuthState {
  user: AuthUser | null
  isAuthenticated: boolean
  accessToken: string
  setUser: (user: AuthUser | null) => void
  setAccessToken: (token: string) => void
  reset: () => void
}

const getStoredToken = (): string => {
  const cookieState = getCookie(ACCESS_TOKEN)
  if (cookieState) {
    try {
      const parsed = JSON.parse(cookieState)
      if (parsed.exp && Date.now() > parsed.exp) {
        removeCookie(ACCESS_TOKEN)
        return ''
      }
      return parsed.token || ''
    } catch {
      return ''
    }
  }
  return ''
}

const decodeJwtPayload = (token: string): Record<string, unknown> | null => {
  try {
    const parts = token.split('.')
    if (parts.length !== 3) return null
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
    const decoded = atob(base64)
    return JSON.parse(decoded)
  } catch {
    return null
  }
}

export const useAuthStore = create<AuthState>((set) => {
  const storedToken = getStoredToken()

  return {
    user: null,
    isAuthenticated: !!storedToken,
    accessToken: storedToken,
    
    setUser: (user) => {
      set({ user, isAuthenticated: !!user })
    },
    
    setAccessToken: (token) => {
      if (token) {
        try {
          const payload = decodeJwtPayload(token)
          const exp = payload?.exp 
            ? (payload.exp as number) * 1000 
            : Date.now() + 60 * 60 * 1000
          setCookie(ACCESS_TOKEN, JSON.stringify({ token, exp }))
        } catch {
          // continue without storing
        }
      }
      set({ accessToken: token, isAuthenticated: !!token })
    },
    
    reset: () => {
      removeCookie(ACCESS_TOKEN)
      set({ user: null, isAuthenticated: false, accessToken: '' })
    }
  }
})
