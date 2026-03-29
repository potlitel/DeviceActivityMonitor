import axios, { AxiosError, AxiosResponse } from 'axios'
import { useAuthStore } from '@/stores/auth-store'

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api'

export const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
})

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Extraer automáticamente la propiedad "data" de las respuestas
api.interceptors.response.use(
  (response: AxiosResponse) => {
    // Si la respuesta tiene la estructura { success, data, ... }, extraer data
    if (response.data && typeof response.data === 'object' && 'data' in response.data) {
      return { ...response, data: response.data.data }
    }
    return response
  },
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      useAuthStore.getState().reset()
      window.location.href = '/sign-in'
    }
    return Promise.reject(error)
  }
)

// ============ AUTH ============
export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  token: string
  userEmail: string
  expiresAt: string
}

export const authApi = {
  login: async (data: LoginRequest): Promise<LoginResponse> => {
    const response = await api.post<LoginResponse>('/auth/login', data)
    return response.data
  },
}

// ============ PROFILE ============
export interface ProfileResponse {
  id: string
  username: string
  email: string
  role: string
  isTwoFactorEnabled: boolean
}

export const profileApi = {
  getProfile: async (): Promise<ProfileResponse> => {
    const response = await api.get<ProfileResponse>('/identity/profile')
    return response.data
  },
}

// ============ PAGINATION ============
export interface PaginatedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface PaginationParams {
  pageNumber?: number
  pageSize?: number
  serialNumber?: string
  status?: string
}

// ============ DEVICE ACTIVITIES ============
export type ActivityStatus = 'Active' | 'Completed'

export interface DeviceActivity {
  id: number
  serialNumber: string
  model: string
  totalCapacityMB: number
  insertedAt: string
  extractedAt: string | null
  initialAvailableMB: number
  finalAvailableMB: number
  megabytesCopied: number
  megabytesDeleted: number
  status: ActivityStatus
  specialEvent: string
  filesCopied: string[]
  filesDeleted: string[]
  timeInserted: string | null
}

export const activitiesApi = {
  getAll: async (params: PaginationParams = {}): Promise<PaginatedResponse<DeviceActivity>> => {
    const response = await api.get<PaginatedResponse<DeviceActivity>>('/activities', { params })
    return response.data
  },
  getById: async (id: number): Promise<DeviceActivity> => {
    const response = await api.get<DeviceActivity>(`/activities/${id}`)
    return response.data
  },
}

// ============ DEVICE PRESENCE ============
export interface DevicePresence {
  id: number
  serialNumber: string
  timestamp: string
  deviceActivityId: number
}

export const presenceApi = {
  getAll: async (params: PaginationParams = {}): Promise<PaginatedResponse<DevicePresence>> => {
    const response = await api.get<PaginatedResponse<DevicePresence>>('/presence', { params })
    return response.data
  },
  getById: async (id: number): Promise<DevicePresence> => {
    const response = await api.get<DevicePresence>(`/presence/${id}`)
    return response.data
  },
}

// ============ INVOICES ============
export interface Invoice {
  id: number
  serialNumber: string
  timestamp: string
  totalAmount: number
  description: string
  deviceActivityId: number
}

export const invoicesApi = {
  getAll: async (params: PaginationParams = {}): Promise<PaginatedResponse<Invoice>> => {
    const response = await api.get<PaginatedResponse<Invoice>>('/invoices', { params })
    return response.data
  },
  getById: async (id: number): Promise<Invoice> => {
    const response = await api.get<Invoice>(`/invoices/${id}`)
    return response.data
  },
}

// ============ SYSTEM EVENTS ============
export type EventLevel = 'Information' | 'Warning' | 'Error'

export interface ServiceEvent {
  id: number
  message: string
  level: EventLevel
  source: string
  timestamp: string
  details?: string
}

export const eventsApi = {
  getAll: async (params: PaginationParams & { level?: string; source?: string } = {}): Promise<PaginatedResponse<ServiceEvent>> => {
    const response = await api.get<PaginatedResponse<ServiceEvent>>('/system/events', { params })
    return response.data
  },
  getById: async (id: number): Promise<ServiceEvent> => {
    const response = await api.get<ServiceEvent>(`/system/events/${id}`)
    return response.data
  },
}

// ============ AUDIT LOGS ============
export interface AuditLog {
  id: string
  userId: string
  username?: string
  action: string
  resource: string
  httpMethod: string
  ipAddress?: string
  timestampUtc: string
}

export const auditApi = {
  getAll: async (params: PaginationParams = {}): Promise<PaginatedResponse<AuditLog>> => {
    const response = await api.get<PaginatedResponse<AuditLog>>('/audit/logs', { params })
    return response.data
  },
  getById: async (id: string): Promise<AuditLog> => {
    const response = await api.get<AuditLog>(`/audit/${id}`)
    return response.data
  },
}
