<script setup lang="ts">
defineProps<{ title: string; count?: string }>()
</script>

<template>
  <div class="content-header">
    <div>
      <div class="content-title">{{ title }}</div>
      <div v-if="count" class="content-count">{{ count }}</div>
    </div>
    <div class="content-actions">
      <slot />
    </div>
  </div>
</template>
