<template>
  <div class="block-item" :class="{ 'is-open': isOpen }">
    <div class="block-header" @click="isOpen = !isOpen">
      <span class="block-toggle">{{ isOpen ? '▼' : '▶' }}</span>
      <span class="block-name">{{ blockType?.name ?? block._type }}</span>
      <span v-if="!isOpen && previewText" class="block-preview">{{ previewText }}</span>
      <div class="block-actions" @click.stop>
        <span class="drag-handle" title="Slepen">⠿</span>
        <button type="button" class="btn-icon" @click="$emit('remove')">✕</button>
      </div>
    </div>

    <div v-if="isOpen" class="block-body">
      <template v-if="blockType">
        <div v-for="field in blockType.fields" :key="field.slug" class="field-group">
          <label class="field-label">{{ field.name.toUpperCase() }}</label>
          <FieldsFieldRenderer
            :field="field"
            :model-value="block[field.slug]"
            @update:model-value="updateField(field.slug, $event)"
          />
        </div>
        <div v-if="blockType.fields.length === 0" style="font-size:12px;color:#888">
          Dit bloktype heeft geen velden.
        </div>
      </template>
      <p v-else style="font-size:12px;color:#dc2626">Bloktype '{{ block._type }}' niet gevonden.</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { BlockTypeDto } from '~/composables/useBlockTypes'

interface BlockInstance extends Record<string, unknown> {
  _type: string
  _id: string
}

const props = defineProps<{
  block: BlockInstance
  blockType: BlockTypeDto | undefined
}>()

const emit = defineEmits<{
  remove: []
  'update:block': [block: BlockInstance]
}>()

const isOpen = ref(false)

const previewText = computed(() => {
  const firstTextField = props.blockType?.fields.find(f => f.type === 'text')
  if (!firstTextField) return ''
  const val = props.block[firstTextField.slug]
  return typeof val === 'string' ? val.slice(0, 50) : ''
})

function updateField(slug: string, value: unknown) {
  emit('update:block', { ...props.block, [slug]: value })
}
</script>

<style scoped>
.block-item { border: 1px solid #ddd; margin-bottom: 6px; }
.block-item.is-open { border-color: #1a1a1a; }
.block-header {
  display: flex; align-items: center; padding: 8px 12px;
  background: #f5f5f5; cursor: pointer; gap: 8px;
}
.block-item.is-open .block-header { background: #1a1a1a; color: #fff; }
.block-toggle { font-size: 10px; width: 14px; }
.block-name { font-size: 13px; font-weight: 600; flex: 1; }
.block-preview { font-size: 12px; color: #888; flex: 2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.block-actions { display: flex; gap: 8px; align-items: center; }
.drag-handle { cursor: grab; opacity: 0.5; font-size: 16px; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 14px; opacity: 0.6; }
.btn-icon:hover { opacity: 1; }
.block-body { padding: 16px; background: #fff; }
.field-group { margin-bottom: 14px; }
.field-label { display: block; font-size: 10px; text-transform: uppercase; letter-spacing: 1px; color: #888; margin-bottom: 4px; }
</style>
