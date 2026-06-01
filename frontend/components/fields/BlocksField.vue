<template>
  <div class="blocks-field">
    <div ref="listEl" class="blocks-list">
      <FieldsBlocksFieldItem
        v-for="(block, index) in modelValue"
        :key="block._id"
        :block="block"
        :block-type="getBlockType(block._type)"
        @remove="removeBlock(index)"
        @update:block="updateBlock(index, $event)"
      />
    </div>
    <FieldsBlockAddDropdown
      :allowed-block-types="allowedBlockTypes"
      @select="addBlock"
    />
  </div>
</template>

<script setup lang="ts">
import Sortable from 'sortablejs'
import type { BlockTypeDto } from '~/composables/useBlockTypes'

interface BlockInstance extends Record<string, unknown> {
  _type: string
  _id: string
}

const props = defineProps<{
  modelValue: BlockInstance[]
  allowedBlockTypeSlugs: string[]
  allBlockTypes: BlockTypeDto[]
}>()

const emit = defineEmits<{
  'update:modelValue': [blocks: BlockInstance[]]
}>()

const listEl = ref<HTMLElement | null>(null)

const allowedBlockTypes = computed(() =>
  props.allBlockTypes.filter(bt => props.allowedBlockTypeSlugs.includes(bt.slug))
)

function getBlockType(slug: string) {
  return props.allBlockTypes.find(bt => bt.slug === slug)
}

function addBlock(bt: BlockTypeDto) {
  emit('update:modelValue', [
    ...props.modelValue,
    { _type: bt.slug, _id: crypto.randomUUID() }
  ])
}

function removeBlock(index: number) {
  const updated = [...props.modelValue]
  updated.splice(index, 1)
  emit('update:modelValue', updated)
}

function updateBlock(index: number, block: BlockInstance) {
  const updated = [...props.modelValue]
  updated[index] = block
  emit('update:modelValue', updated)
}

onMounted(() => {
  if (!listEl.value) return
  Sortable.create(listEl.value, {
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
.blocks-field { display: flex; flex-direction: column; }
.blocks-list { margin-bottom: 8px; }
</style>
