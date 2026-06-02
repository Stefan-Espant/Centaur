<template>
  <div class="field-builder">
    <div v-for="(field, index) in modelValue" :key="field.id" class="field-card">
      <input
        v-model="field.name"
        class="form-input field-input"
        placeholder="Veldnaam"
        @input="autoSlug(index)"
      />
      <input
        v-model="field.slug"
        class="form-input field-input field-input-slug"
        placeholder="slug"
        pattern="[a-z0-9_]+"
      />
      <button type="button" class="type-current" @click="togglePicker(index)">
        <svg class="type-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" v-html="getType(field.type).icon" />
        <span>{{ getType(field.type).label }}</span>
        <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" class="type-chevron" :class="{ open: openPicker === index }">
          <polyline points="4,6 8,10 12,6"/>
        </svg>
      </button>
      <button type="button" class="field-remove" @click="removeField(index)">
        <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round">
          <line x1="3" y1="3" x2="13" y2="13"/><line x1="13" y1="3" x2="3" y2="13"/>
        </svg>
      </button>

      <div v-if="openPicker === index" class="type-grid">
        <button
          v-for="t in availableTypes"
          :key="t.value"
          type="button"
          class="type-btn"
          :class="{ active: field.type === t.value }"
          :title="t.label"
          @click="setType(index, t.value)"
        >
          <svg class="type-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" v-html="t.icon" />
          <span>{{ t.label }}</span>
        </button>
      </div>
    </div>

    <button type="button" class="btn btn-ghost field-add-btn" @click="addField">
      <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.75" stroke-linecap="round" style="width:14px;height:14px;flex-shrink:0">
        <line x1="8" y1="2" x2="8" y2="14"/><line x1="2" y1="8" x2="14" y2="8"/>
      </svg>
      Veld toevoegen
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
  {
    value: 'text', label: 'Tekst',
    icon: '<text x="2" y="12" font-size="11" font-weight="700" fill="currentColor" stroke="none" font-family="serif">Aa</text>'
  },
  {
    value: 'richtext', label: 'Rich text',
    icon: '<line x1="2" y1="4" x2="14" y2="4"/><line x1="2" y1="8" x2="11" y2="8"/><line x1="2" y1="12" x2="13" y2="12"/>'
  },
  {
    value: 'number', label: 'Getal',
    icon: '<text x="1" y="12" font-size="12" font-weight="700" fill="currentColor" stroke="none" font-family="monospace">#</text>'
  },
  {
    value: 'boolean', label: 'Ja/Nee',
    icon: '<rect x="1" y="5" width="14" height="6" rx="3"/><circle cx="11" cy="8" r="2.5" fill="currentColor" stroke="none"/>'
  },
  {
    value: 'date', label: 'Datum',
    icon: '<rect x="2" y="3" width="12" height="11" rx="1.5"/><line x1="5" y1="1" x2="5" y2="5"/><line x1="11" y1="1" x2="11" y2="5"/><line x1="2" y1="7" x2="14" y2="7"/>'
  },
  {
    value: 'datetime', label: 'Datum+tijd',
    icon: '<rect x="1" y="3" width="10" height="9" rx="1"/><line x1="4" y1="1" x2="4" y2="5"/><line x1="8" y1="1" x2="8" y2="5"/><line x1="1" y1="7" x2="11" y2="7"/><circle cx="13" cy="11" r="3"/><line x1="13" y1="9.5" x2="13" y2="11"/><line x1="13" y1="11" x2="14.2" y2="11"/>'
  },
  {
    value: 'media', label: 'Media',
    icon: '<rect x="1" y="3" width="14" height="10" rx="1.5"/><circle cx="5.5" cy="7" r="1.5"/><path d="M1 11l4-3 3 2.5 2-1.5 5 4"/>'
  },
  {
    value: 'select', label: 'Selectie',
    icon: '<line x1="2" y1="4" x2="10" y2="4"/><line x1="2" y1="8" x2="10" y2="8"/><line x1="2" y1="12" x2="10" y2="12"/><polyline points="12,6 14,8 12,10"/>'
  },
  {
    value: 'blocks', label: 'Blokken',
    icon: '<path d="M8 1 1 4.5v7L8 15l7-3.5v-7z"/><line x1="8" y1="1" x2="8" y2="15"/><path d="M1 4.5 8 8l7-3.5"/>'
  },
  {
    value: 'repeater', label: 'Repeater',
    icon: '<rect x="1" y="2" width="14" height="3" rx="1"/><rect x="1" y="7" width="14" height="3" rx="1"/><rect x="1" y="12" width="14" height="3" rx="1"/>'
  },
]

const availableTypes = computed(() =>
  allTypes.filter(t => !props.excludeTypes?.includes(t.value))
)

const openPicker = ref<number | null>(null)

function getType(value: string) {
  return allTypes.find(t => t.value === value) ?? allTypes[0]
}

function togglePicker(index: number) {
  openPicker.value = openPicker.value === index ? null : index
}

function setType(index: number, type: string) {
  const updated = [...props.modelValue]
  updated[index] = { ...updated[index], type }
  emit('update:modelValue', updated)
  openPicker.value = null
}

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

<style scoped>
.field-builder {
  display: grid;
  grid-template-columns: 1fr 160px auto 28px;
  gap: 6px;
}

.field-card {
  grid-column: 1 / -1;
  display: grid;
  grid-template-columns: subgrid;
  align-items: center;
  gap: 8px;
  padding: 8px 10px;
  background: rgba(255,255,255,.65);
  border: 1px solid rgba(255,255,255,.6);
  border-radius: 10px;
}

.type-grid {
  grid-column: 1 / -1;
}

.field-input { flex: 1; padding: 5px 8px !important; font-size: 12px !important; }
.field-input-slug { max-width: 160px; color: #64748b; }

.field-remove {
  width: 26px; height: 26px; flex-shrink: 0;
  background: none; border: none; cursor: pointer;
  color: #94a3b8; border-radius: 6px;
  display: flex; align-items: center; justify-content: center;
  transition: color .1s, background .1s;
}
.field-remove svg { width: 12px; height: 12px; }
.field-remove:hover { color: #dc2626; background: rgba(220,38,38,.08); }

.type-grid {
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
  padding: 8px 10px 10px;
}

.type-btn {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: 5px;
  padding: 6px 10px;
  border: 1px solid rgba(200,210,230,.45);
  border-radius: 8px;
  background: rgba(255,255,255,.55);
  cursor: pointer;
  transition: background .12s, border-color .12s, color .12s, box-shadow .12s;
  color: #64748b;
}
.type-btn span { font-size: 11px; font-weight: 600; white-space: nowrap; }
.type-btn:hover { background: rgba(255,255,255,.9); color: #1e293b; border-color: rgba(22,163,74,.3); }
.type-btn.active {
  background: linear-gradient(135deg, #16a34a, #15803d);
  border-color: transparent;
  color: #fff;
  box-shadow: 0 2px 8px rgba(22,163,74,.3);
}

.type-icon { width: 14px; height: 14px; flex-shrink: 0; }

.field-add-btn { grid-column: 1 / -1; font-size: 12px; display: flex; align-items: center; gap: 6px; align-self: start; }

.type-current {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 5px 10px 5px 8px;
  background: rgba(255,255,255,.7);
  border: 1px solid rgba(200,210,230,.5);
  border-radius: 8px;
  cursor: pointer;
  font-size: 12px;
  font-weight: 600;
  color: #1e293b;
  transition: background .12s, border-color .12s;
  justify-content: center;
}
.type-current:hover { background: rgba(255,255,255,.95); border-color: rgba(22,163,74,.35); }
.type-current .type-icon { width: 14px; height: 14px; color: #16a34a; }
.type-chevron { width: 10px; height: 10px; margin-left: 2px; opacity: .5; transition: transform .15s; }
.type-chevron.open { transform: rotate(180deg); }

.type-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 4px;
  padding: 8px 10px 10px;
  border-top: 1px solid rgba(200,210,230,.3);
}
</style>
