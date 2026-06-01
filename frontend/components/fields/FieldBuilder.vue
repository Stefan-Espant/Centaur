<template>
  <div class="field-builder">
    <table v-if="modelValue.length > 0" class="table" style="margin-bottom:8px">
      <thead>
        <tr>
          <th>Naam</th>
          <th>Slug</th>
          <th>Type</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="(field, index) in modelValue" :key="field.id">
          <td>
            <input v-model="field.name" class="form-input" style="padding:4px 6px;font-size:12px"
              placeholder="Naam" @input="autoSlug(index)" />
          </td>
          <td>
            <input v-model="field.slug" class="form-input" style="padding:4px 6px;font-size:12px"
              placeholder="slug" pattern="[a-z0-9_]+" />
          </td>
          <td>
            <select v-model="field.type" class="form-input" style="padding:4px 6px;font-size:12px">
              <option v-for="t in availableTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
            </select>
          </td>
          <td>
            <button type="button" class="table-action" style="color:#dc2626" @click="removeField(index)">✕</button>
          </td>
        </tr>
      </tbody>
    </table>

    <button type="button" class="btn btn-ghost" style="font-size:12px" @click="addField">
      + Veld toevoegen
    </button>
  </div>
</template>

<script setup lang="ts">
import type { FieldDefinitionDto } from '~/composables/useBlockTypes'

const props = defineProps<{
  modelValue: FieldDefinitionDto[]
  excludeTypes?: string[]
}>()

const emit = defineEmits<{
  'update:modelValue': [fields: FieldDefinitionDto[]]
}>()

const allTypes = [
  { value: 'text', label: 'Tekst' },
  { value: 'richtext', label: 'Rich text' },
  { value: 'number', label: 'Getal' },
  { value: 'boolean', label: 'Ja/Nee' },
  { value: 'date', label: 'Datum' },
  { value: 'datetime', label: 'Datum + tijd' },
  { value: 'media', label: 'Media' },
  { value: 'select', label: 'Selectie' },
  { value: 'blocks', label: 'Blokken' },
  { value: 'repeater', label: 'Repeater' },
]

const availableTypes = computed(() =>
  allTypes.filter(t => !props.excludeTypes?.includes(t.value))
)

function addField() {
  emit('update:modelValue', [
    ...props.modelValue,
    { id: crypto.randomUUID(), name: '', slug: '', type: 'text', config: {} }
  ])
}

function removeField(index: number) {
  const updated = [...props.modelValue]
  updated.splice(index, 1)
  emit('update:modelValue', updated)
}

function autoSlug(index: number) {
  const field = props.modelValue[index]
  const updated = [...props.modelValue]
  updated[index] = {
    ...field,
    slug: field.name.toLowerCase().replace(/\s+/g, '_').replace(/[^a-z0-9_]/g, '')
  }
  emit('update:modelValue', updated)
}
</script>
