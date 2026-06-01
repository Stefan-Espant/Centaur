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
  width: 100%; border: 1px dashed #888; padding: 8px;
  background: none; cursor: pointer; color: #888; font-size: 13px;
}
.btn-dashed:hover { border-color: #1a1a1a; color: #1a1a1a; }
.dropdown-menu {
  position: absolute; left: 0; right: 0; z-index: 10;
  background: #fff; border: 1px solid #ddd; box-shadow: 0 4px 12px rgba(0,0,0,0.1);
}
.dropdown-item {
  display: block; width: 100%; padding: 10px 14px; text-align: left;
  background: none; border: none; cursor: pointer; font-size: 13px;
}
.dropdown-item:hover { background: #f5f5f0; }
</style>
