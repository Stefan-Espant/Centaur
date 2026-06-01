<script setup lang="ts">
defineProps<{
  title: string
  message: string
  loading?: boolean
}>()

const emit = defineEmits<{
  close: []
  confirm: []
}>()
</script>

<template>
  <div class="modal-backdrop" @click.self="emit('close')">
    <div class="modal">
      <div class="modal-title">{{ title }}</div>
      <p style="font-size:13px;color:#555">{{ message }}</p>
      <div class="modal-actions">
        <button class="btn btn-ghost" @click="emit('close')">Annuleren</button>
        <button class="btn btn-danger" :disabled="loading" @click="emit('confirm')">Verwijderen</button>
      </div>
    </div>
  </div>
</template>
