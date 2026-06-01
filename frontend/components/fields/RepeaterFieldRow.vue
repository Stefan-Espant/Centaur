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
.repeater-row td { padding: 6px 8px; vertical-align: top; }
.drag-cell { width: 24px; cursor: grab; color: #888; }
.remove-cell { width: 32px; }
.btn-icon { background: none; border: none; cursor: pointer; color: #888; }
.btn-icon:hover { color: #1a1a1a; }
</style>
