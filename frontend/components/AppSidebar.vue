<script setup lang="ts">
const { user, isSuperAdmin, logout } = useAuth()
const route = useRoute()

function isActive(path: string) {
  return route.path === path || route.path.startsWith(`${path}/`)
}

function initials(value: string) {
  return value.charAt(0).toUpperCase()
}
</script>

<template>
  <aside class="sidebar">
    <div class="sidebar-logo">
      <AppLogo height="60px" />
    </div>

    <div class="sidebar-tenant">
      <div class="sidebar-tenant-dot"></div>
      {{ isSuperAdmin ? 'Systeem' : 'Mijn tenant' }}
    </div>

    <template v-if="isSuperAdmin">
      <div class="sidebar-section">Beheer</div>
      <NuxtLink to="/tenants" class="sidebar-item" :class="{ active: isActive('/tenants') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <rect x="1" y="5" width="14" height="10" rx="1"/><path d="M5 5V3a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/><line x1="8" y1="9" x2="8" y2="11"/>
        </svg>
        Tenants
      </NuxtLink>
    </template>

    <template v-else>
      <div class="sidebar-section">Beheer</div>
      <NuxtLink to="/dashboard" class="sidebar-item" :class="{ active: isActive('/dashboard') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <rect x="1" y="1" width="6" height="6" rx="1"/><rect x="9" y="1" width="6" height="6" rx="1"/><rect x="1" y="9" width="6" height="6" rx="1"/><rect x="9" y="9" width="6" height="6" rx="1"/>
        </svg>
        Dashboard
      </NuxtLink>
      <NuxtLink to="/website" class="sidebar-item" :class="{ active: isActive('/website') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="8" cy="8" r="7"/><path d="M8 1c-2 2-3 4-3 7s1 5 3 7"/><path d="M8 1c2 2 3 4 3 7s-1 5-3 7"/><line x1="1" y1="8" x2="15" y2="8"/>
        </svg>
        Website
      </NuxtLink>
      <NuxtLink to="/users" class="sidebar-item" :class="{ active: isActive('/users') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="6" cy="5" r="3"/><path d="M1 14a5 5 0 0 1 10 0"/><path d="M11 3a3 3 0 0 1 0 6"/><path d="M15 14a5 5 0 0 0-4-4.9"/>
        </svg>
        Gebruikers
      </NuxtLink>
      <NuxtLink to="/api-keys" class="sidebar-item" :class="{ active: isActive('/api-keys') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="5.5" cy="5.5" r="4"/><path d="M8.5 8.5 15 15"/><path d="M12 13l2-2"/>
        </svg>
        API-sleutels
      </NuxtLink>
      <div class="sidebar-section">Content</div>
      <NuxtLink to="/pages" class="sidebar-item" :class="{ active: isActive('/pages') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <path d="M9 1H3a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V6z"/><polyline points="9 1 9 6 14 6"/><line x1="5" y1="9" x2="11" y2="9"/><line x1="5" y1="12" x2="9" y2="12"/>
        </svg>
        Pagina's
      </NuxtLink>
      <NuxtLink to="/block-types" class="sidebar-item" :class="{ active: isActive('/block-types') }">
        <svg class="sidebar-item-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
          <path d="M8 1 1 4.5v7L8 15l7-3.5v-7z"/><line x1="8" y1="1" x2="8" y2="15"/><path d="M1 4.5 8 8l7-3.5"/>
        </svg>
        Bloktypen
      </NuxtLink>
    </template>

    <div class="sidebar-bottom">
      <div class="sidebar-avatar">{{ user ? initials(user.userId) : '?' }}</div>
      <span>{{ user?.role }}</span>
      <button class="table-action" style="margin-left:auto" @click="logout">Uitloggen</button>
    </div>
  </aside>
</template>
