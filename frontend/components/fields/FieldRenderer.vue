<template>
  <div class="field-renderer">
    <!-- Tekst -->
    <input
      v-if="field.type === 'text'"
      :value="modelValue as string ?? ''"
      type="text"
      class="form-input"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />

    <!-- Richtext (simpele textarea) -->
    <textarea
      v-else-if="field.type === 'richtext'"
      :value="modelValue as string ?? ''"
      class="form-input"
      rows="4"
      @input="$emit('update:modelValue', ($event.target as HTMLTextAreaElement).value)"
    />

    <!-- Getal -->
    <input
      v-else-if="field.type === 'number'"
      :value="modelValue as number ?? ''"
      type="number"
      class="form-input"
      @input="$emit('update:modelValue', Number(($event.target as HTMLInputElement).value))"
    />

    <!-- Boolean -->
    <div v-else-if="field.type === 'boolean'" style="display:flex;align-items:center;gap:8px">
      <input
        :checked="modelValue as boolean ?? false"
        type="checkbox"
        @change="$emit('update:modelValue', ($event.target as HTMLInputElement).checked)"
      />
      <span style="font-size:13px">{{ modelValue ? 'Ja' : 'Nee' }}</span>
    </div>

    <!-- Datum -->
    <input
      v-else-if="field.type === 'date' || field.type === 'datetime'"
      :value="modelValue as string ?? ''"
      :type="field.type === 'datetime' ? 'datetime-local' : 'date'"
      class="form-input"
      @input="$emit('update:modelValue', ($event.target as HTMLInputElement).value)"
    />

    <!-- Blocks -->
    <FieldsBlocksField
      v-else-if="field.type === 'blocks'"
      :model-value="(modelValue as BlockInstance[]) ?? []"
      :allowed-block-type-slugs="(field.config?.allowed_block_type_slugs as string[]) ?? []"
      :all-block-types="allBlockTypes"
      @update:model-value="$emit('update:modelValue', $event)"
    />

    <!-- Repeater -->
    <FieldsRepeaterField
      v-else-if="field.type === 'repeater'"
      :model-value="(modelValue as Record<string, unknown>[]) ?? []"
      :sub-fields="(field.config?.sub_fields as FieldDefinitionDto[]) ?? []"
      @update:model-value="$emit('update:modelValue', $event)"
    />

    <!-- Select -->
    <select
      v-else-if="field.type === 'select'"
      :value="modelValue as string ?? ''"
      class="form-input"
      @change="$emit('update:modelValue', ($event.target as HTMLSelectElement).value)"
    >
      <option value="">Kies een optie</option>
      <option v-for="option in selectOptions" :key="option" :value="option">
        {{ option }}
      </option>
    </select>

    <!-- Overige typen (media, select, relation) — placeholder -->
    <div v-else style="font-size:12px;color:#888;padding:6px;border:1px dashed #ddd">
      Veldtype '{{ field.type }}' (nog niet geïmplementeerd in editor)
    </div>
  </div>
</template>

<script setup lang="ts">
import type { BlockTypeDto, FieldDefinitionDto } from '~/composables/useBlockTypes'

interface BlockInstance extends Record<string, unknown> {
  _type: string
  _id: string
}

const props = defineProps<{
  field: FieldDefinitionDto
  modelValue: unknown
}>()

defineEmits<{
  'update:modelValue': [value: unknown]
}>()

const allBlockTypes = ref<BlockTypeDto[]>([])
const selectOptions = computed(() => {
  const options = props.field.config?.options
  return Array.isArray(options) ? options.filter((option): option is string => typeof option === 'string') : []
})

onMounted(async () => {
  if (props.field.type === 'blocks') {
    try {
      const { getAll } = useBlockTypes()
      allBlockTypes.value = await getAll()
    } catch {}
  }
})
</script>
