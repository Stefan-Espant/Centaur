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
.repeater-field {
  border: 1px solid rgba(255,255,255,.55);
  border-radius: 10px;
  overflow: hidden;
  backdrop-filter: blur(20px);
  -webkit-backdrop-filter: blur(20px);
  box-shadow: 0 1px 4px rgba(22,163,74,.06);
}
.repeater-table { width: 100%; border-collapse: collapse; }
.col-header {
  padding: 7px 10px; text-align: left; font-size: 10px; font-weight: 600;
  text-transform: uppercase; letter-spacing: .06em; color: #64748b;
  border-bottom: 1px solid rgba(200,210,230,.4);
  background: rgba(255,255,255,.55);
}
.drag-header, .remove-header {
  width: 32px;
  background: rgba(255,255,255,.55);
  border-bottom: 1px solid rgba(200,210,230,.4);
}
.btn-add-row {
  width: 100%; padding: 9px; border: none;
  border-top: 1px solid rgba(200,210,230,.4);
  background: rgba(255,255,255,.4);
  cursor: pointer; font-size: 13px; font-weight: 500; color: #64748b;
  transition: background .12s, color .12s;
}
.btn-add-row:hover { background: rgba(22,163,74,.07); color: #15803d; }
</style>
