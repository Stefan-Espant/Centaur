<template>
  <tr class="repeater-row">
    <td class="drag-cell">
      <span class="drag-handle" title="Slepen">⠿</span>
    </td>
    <td v-for="subField in subFields" :key="subField.slug" class="value-cell">
      <FieldsFieldRenderer
        :field="subField"
        :model-value="row[subField.slug]"
        @update:model-value="updateField(subField.slug, $event)"
      />
    </td>
    <td class="remove-cell">
      <button type="button" class="btn-icon" @click="$emit('remove')">✕</button>
    </td>
  </tr>
</template>

<script setup lang="ts">
import type { FieldDefinitionDto } from '~/composables/useBlockTypes'

const props = defineProps<{
  row: Record<string, unknown>
  subFields: FieldDefinitionDto[]
}>()

const emit = defineEmits<{
  remove: []
  'update:row': [row: Record<string, unknown>]
}>()

function updateField(slug: string, value: unknown) {
  emit('update:row', { ...props.row, [slug]: value })
}
</script>

<style scoped>
.repeater-row td { padding: 6px 10px; vertical-align: top; border-bottom: 1px solid rgba(200,210,230,.3); }
.drag-cell { width: 24px; cursor: grab; color: #94a3b8; }
.remove-cell { width: 32px; }
.btn-icon { background: none; border: none; cursor: pointer; color: #94a3b8; font-size: 13px; border-radius: 5px; padding: 2px 4px; transition: color .1s, background .1s; }
.btn-icon:hover { color: #dc2626; background: rgba(220,38,38,.08); }
</style>
