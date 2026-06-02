<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Bloktypen</div>
        <div class="content-count">{{ blockTypes.length }} bloktypen</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/block-types/new" class="btn btn-primary">+ Nieuw bloktype</NuxtLink>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <table v-if="blockTypes.length > 0" class="table">
      <thead>
        <tr>
          <th>Naam</th>
          <th>Slug</th>
          <th>Velden</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="bt in blockTypes" :key="bt.id">
          <td><div class="row-title">{{ bt.name }}</div></td>
          <td><div class="row-meta">{{ bt.slug }}</div></td>
          <td style="font-size:12px;color:#555">{{ bt.fields.length }}</td>
          <td>
            <NuxtLink :to="`/block-types/${bt.slug}`" class="table-action">Bewerken</NuxtLink>
            <button class="table-action" style="color:#dc2626;margin-left:8px" @click="confirmDelete(bt)">
              Verwijderen
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-else-if="!error" class="empty-state">
      Nog geen bloktypen aangemaakt.
    </div>
  </div>
</template>

<script setup lang="ts">
import type { BlockTypeDto } from '~/composables/useBlockTypes'

const { getAll, remove } = useBlockTypes()

const blockTypes = ref<BlockTypeDto[]>([])
const error = ref('')

onMounted(async () => {
  try {
    blockTypes.value = await getAll()
  } catch (e: unknown) {
    error.value = (e as Error).message
  }
})

async function confirmDelete(bt: BlockTypeDto) {
  if (!confirm(`Bloktype "${bt.name}" verwijderen?`)) return
  try {
    await remove(bt.slug)
    blockTypes.value = blockTypes.value.filter(b => b.id !== bt.id)
  } catch (e: unknown) {
    error.value = (e as Error).message
  }
}
</script>
