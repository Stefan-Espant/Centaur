export default defineNuxtRouteMiddleware((to) => {
  if (import.meta.server) return

  const { isAuthenticated, isSuperAdmin } = useAuth()

  if (to.path === '/login') {
    if (isAuthenticated.value) {
      return navigateTo(isSuperAdmin.value ? '/tenants' : '/dashboard')
    }
    return
  }

  if (!isAuthenticated.value) {
    return navigateTo('/login')
  }
})
