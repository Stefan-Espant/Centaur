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
      <div class="sidebar-logo-mark">C</div>
      <div class="sidebar-logo-name">Centaur</div>
    </div>

    <div class="sidebar-tenant">
      <div class="sidebar-tenant-dot"></div>
      {{ isSuperAdmin ? 'Systeem' : 'Mijn tenant' }}
    </div>

    <template v-if="isSuperAdmin">
      <div class="sidebar-section">Beheer</div>
      <NuxtLink to="/tenants" class="sidebar-item" :class="{ active: isActive('/tenants') }">
        <span class="sidebar-item-icon">◻</span> Tenants
      </NuxtLink>
    </template>

    <template v-else>
      <div class="sidebar-section">Beheer</div>
      <NuxtLink to="/dashboard" class="sidebar-item" :class="{ active: isActive('/dashboard') }">
        <span class="sidebar-item-icon">◻</span> Dashboard
      </NuxtLink>
      <NuxtLink to="/website" class="sidebar-item" :class="{ active: isActive('/website') }">
        <span class="sidebar-item-icon">◻</span> Website
      </NuxtLink>
      <NuxtLink to="/users" class="sidebar-item" :class="{ active: isActive('/users') }">
        <span class="sidebar-item-icon">◻</span> Gebruikers
      </NuxtLink>
      <NuxtLink to="/api-keys" class="sidebar-item" :class="{ active: isActive('/api-keys') }">
        <span class="sidebar-item-icon">◻</span> API-sleutels
      </NuxtLink>
    </template>

    <div class="sidebar-bottom">
      <div class="sidebar-avatar">{{ user ? initials(user.userId) : '?' }}</div>
      <span>{{ user?.role }}</span>
      <button class="table-action" style="margin-left:auto" @click="logout">Uitloggen</button>
    </div>
  </aside>
</template>
