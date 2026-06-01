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
.block-item {
  border: 1px solid rgba(255,255,255,.55);
  border-radius: 10px;
  margin-bottom: 6px;
  overflow: hidden;
  box-shadow: 0 1px 4px rgba(22,163,74,.06);
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
}
.block-item.is-open { border-color: rgba(22,163,74,.35); }
.block-header {
  display: flex;
  align-items: center;
  padding: 9px 12px;
  background: rgba(255,255,255,.72);
  cursor: pointer;
  gap: 8px;
  transition: background .12s;
}
.block-header:hover { background: rgba(255,255,255,.85); }
.block-item.is-open .block-header {
  background: linear-gradient(135deg, #16a34a, #15803d);
  color: #fff;
}
.block-toggle { font-size: 10px; width: 14px; }
.block-name { font-size: 13px; font-weight: 600; flex: 1; }
.block-preview { font-size: 12px; color: #94a3b8; flex: 2; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.block-item.is-open .block-preview { color: rgba(255,255,255,.7); }
.block-actions { display: flex; gap: 8px; align-items: center; }
.drag-handle { cursor: grab; opacity: .45; font-size: 16px; }
.btn-icon { background: none; border: none; cursor: pointer; font-size: 13px; opacity: .55; transition: opacity .1s; }
.btn-icon:hover { opacity: 1; }
.block-body { padding: 16px; background: rgba(255,255,255,.6); border-top: 1px solid rgba(255,255,255,.5); }
.field-group { margin-bottom: 14px; }
.field-label { display: block; font-size: 10px; font-weight: 600; text-transform: uppercase; letter-spacing: .06em; color: #64748b; margin-bottom: 5px; }
</style>
