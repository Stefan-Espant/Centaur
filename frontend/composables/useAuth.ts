interface AuthUser {
  userId: string
  role: string
  tenantId: string | null
}

const TOKEN_KEY = 'centaur_token'

function parseToken(token: string): AuthUser | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1] ?? ''))
    if ((payload.exp ?? 0) * 1000 < Date.now()) return null

    return {
      userId: payload.user_id,
      role: payload.role,
      tenantId: payload.tenant_id || null
    }
  } catch {
    return null
  }
}

export const useAuth = () => {
  const user = useState<AuthUser | null>('auth_user', () => {
    if (!import.meta.client) return null

    const token = localStorage.getItem(TOKEN_KEY)
    return token ? parseToken(token) : null
  })

  const isAuthenticated = computed(() => user.value !== null)
  const isSuperAdmin = computed(() => user.value?.role === 'SuperAdmin')

  async function login(email: string, password: string): Promise<void> {
    const config = useRuntimeConfig()
    const data = await $fetch<{ accessToken: string }>(`${config.public.apiBase}/api/auth/login`, {
      method: 'POST',
      body: { email, password }
    })

    localStorage.setItem(TOKEN_KEY, data.accessToken)
    user.value = parseToken(data.accessToken)
  }

  function logout(): void {
    if (import.meta.client) {
      localStorage.removeItem(TOKEN_KEY)
    }

    user.value = null
    navigateTo('/login')
  }

  function getToken(): string | null {
    return import.meta.client ? localStorage.getItem(TOKEN_KEY) : null
  }

  return { user, isAuthenticated, isSuperAdmin, login, logout, getToken }
}
