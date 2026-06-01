<template>
  <div class="repeater-field">
    <table class="repeater-table">
      <thead>
        <tr>
          <th class="drag-header"></th>
          <th v-for="subField in subFields" :key="subField.slug" class="col-header">
            {{ subField.name.toUpperCase() }}
          </th>
          <th class="remove-header"></th>
        </tr>
      </thead>
      <tbody ref="tbodyEl">
        <FieldsRepeaterFieldRow
          v-for="(row, index) in modelValue"
          :key="index"
          :row="row"
          :sub-fields="subFields"
          @remove="removeRow(index)"
          @update:row="updateRow(index, $event)"
        />
      </tbody>
    </table>
    <button type="button" class="btn-add-row" @click="addRow">+ Rij toevoegen</button>
  </div>
</template>

<script setup lang="ts">
import Sortable from 'sortablejs'
import type { FieldDefinitionDto } from '~/composables/useBlockTypes'

const props = defineProps<{
  modelValue: Record<string, unknown>[]
  subFields: FieldDefinitionDto[]
}>()

const emit = defineEmits<{
  'update:modelValue': [rows: Record<string, unknown>[]]
}>()

const tbodyEl = ref<HTMLElement | null>(null)

function addRow() {
  const emptyRow: Record<string, unknown> = {}
  props.subFields.forEach(f => { emptyRow[f.slug] = null })
  emit('update:modelValue', [...props.modelValue, emptyRow])
}

function removeRow(index: number) {
  const updated = [...props.modelValue]
  updated.splice(index, 1)
  emit('update:modelValue', updated)
}

function updateRow(index: number, row: Record<string, unknown>) {
  const updated = [...props.modelValue]
  updated[index] = row
  emit('update:modelValue', updated)
}

onMounted(() => {
  if (!tbodyEl.value) return
  Sortable.create(tbodyEl.value, {
    handle: '.drag-handle',
    animation: 150,
    onEnd(evt) {
      if (evt.oldIndex === undefined || evt.newIndex === undefined) return
      const updated = [...props.modelValue]
      const [moved] = updated.splice(evt.oldIndex, 1)
      updated.splice(evt.newIndex, 0, moved)
      emit('update:modelValue', updated)
    }
  })
})
</script>

<style scoped>
.repeater-field { border: 1px solid #ddd; }
.repeater-table { width: 100%; border-collapse: collapse; }
.col-header {
  padding: 6px 8px; text-align: left; font-size: 10px;
  text-transform: uppercase; letter-spacing: 1px; color: #888;
  border-bottom: 1px solid #ddd; background: #f5f5f0;
}
.drag-header, .remove-header { width: 32px; background: #f5f5f0; border-bottom: 1px solid #ddd; }
.btn-add-row {
  width: 100%; padding: 8px; border: none; border-top: 1px solid #ddd;
  background: none; cursor: pointer; font-size: 13px; color: #888;
}
.btn-add-row:hover { background: #f5f5f0; color: #1a1a1a; }
</style>
