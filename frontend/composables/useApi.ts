export const useApi = () => {
  const config = useRuntimeConfig()
  const { getToken, logout } = useAuth()

  async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const token = getToken()
    const response = await fetch(`${config.public.apiBase}${path}`, {
      ...options,
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
        ...options.headers
      }
    })

    if (response.status === 401) {
      logout()
      throw new Error('Sessie verlopen')
    }

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Onbekende fout' }))
      throw new Error(error.message ?? `Fout ${response.status}`)
    }

    if (response.status === 204) return undefined as T
    return response.json() as Promise<T>
  }

  return {
    get: <T>(path: string) => request<T>(path),
    post: <T>(path: string, body: unknown) => request<T>(path, { method: 'POST', body: JSON.stringify(body) }),
    put: <T>(path: string, body: unknown) => request<T>(path, { method: 'PUT', body: JSON.stringify(body) }),
    del: (path: string) => request<void>(path, { method: 'DELETE' })
  }
}
