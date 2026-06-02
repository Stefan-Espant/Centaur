const STORAGE_KEY = 'centaur-theme'

export type Theme = 'light' | 'dark'

export function useTheme() {
  const theme = useState<Theme>('theme', () => 'light')

  function apply(t: Theme) {
    theme.value = t
    if (import.meta.client) {
      document.documentElement.classList.toggle('dark', t === 'dark')
      localStorage.setItem(STORAGE_KEY, t)
    }
  }

  function toggle() {
    apply(theme.value === 'dark' ? 'light' : 'dark')
  }

  function init() {
    if (!import.meta.client) return
    const stored = localStorage.getItem(STORAGE_KEY) as Theme | null
    const preferred = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
    apply(stored ?? preferred)
  }

  return { theme, toggle, init, apply }
}
