<template>
  <div class="block-add-dropdown">
    <button type="button" class="btn-dashed" @click="open = !open">
      + Blok toevoegen ▼
    </button>
    <div v-if="open" class="dropdown-menu">
      <button
        v-for="bt in allowedBlockTypes"
        :key="bt.slug"
        type="button"
        class="dropdown-item"
        @click="select(bt)"
      >
        {{ bt.name }}
      </button>
      <div v-if="allowedBlockTypes.length === 0" class="dropdown-item" style="color:#888;cursor:default">
        Geen bloktypen beschikbaar
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { BlockTypeDto } from '~/composables/useBlockTypes'

const props = defineProps<{
  allowedBlockTypes: BlockTypeDto[]
}>()

const emit = defineEmits<{
  select: [blockType: BlockTypeDto]
}>()

const open = ref(false)

function select(bt: BlockTypeDto) {
  open.value = false
  emit('select', bt)
}
</script>

<style scoped>
.block-add-dropdown { position: relative; }
.btn-dashed {
  width: 100%;
  border: 1px dashed rgba(22,163,74,.45);
  border-radius: 9px;
  padding: 9px;
  background: var(--color-glass);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  cursor: pointer;
  color: #64748b;
  font-size: 13px;
  font-weight: 500;
  transition: border-color .12s, color .12s, background .12s;
}
.btn-dashed:hover {
  border-color: #16a34a;
  color: #15803d;
  background: rgba(22,163,74,.06);
}
.dropdown-menu {
  position: absolute; left: 0; right: 0; z-index: 10;
  background: var(--color-glass-strong);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  box-shadow: 0 8px 24px rgba(22,163,74,.12), 0 2px 6px rgba(0,0,0,.06);
  overflow: hidden;
  margin-top: 4px;
}
.dropdown-item {
  display: block; width: 100%; padding: 9px 14px; text-align: left;
  background: none; border: none; cursor: pointer; font-size: 13px;
  font-weight: 500; color: var(--color-text); transition: background .1s;
}
.dropdown-item:hover { background: rgba(22,163,74,.08); color: #15803d; }
</style>
