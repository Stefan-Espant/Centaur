<script setup lang="ts">
const { isSuperAdmin } = useAuth()

await navigateTo(isSuperAdmin.value ? '/tenants' : '/dashboard')
</script>

<template>
  <div />
</template>
