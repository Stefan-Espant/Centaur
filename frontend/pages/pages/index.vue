<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Pagina's</div>
        <div class="content-count">{{ pages.length }} pagina's</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/pages/new" class="btn btn-primary">+ Nieuwe pagina</NuxtLink>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <table v-if="pages.length > 0" class="table">
      <thead>
        <tr>
          <th>Titel</th>
          <th>Slug</th>
          <th>Blokken</th>
          <th>Bijgewerkt</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="page in pages" :key="page.id">
          <td>
            <div class="row-title">{{ page.title }}</div>
          </td>
          <td>
            <div class="row-meta">/{{ page.slug }}</div>
          </td>
          <td style="font-size:12px;color:#555">{{ page.body.length }}</td>
          <td style="font-size:12px;color:#555">{{ formatDate(page.updatedAt) }}</td>
          <td>
            <NuxtLink :to="`/pages/${page.id}`" class="table-action">Bewerken</NuxtLink>
            <button class="table-action" style="color:#dc2626;margin-left:8px" @click="confirmDelete(page)">
              Verwijderen
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-else-if="!error" class="empty-state">
      Nog geen pagina's aangemaakt.
    </div>
  </div>
</template>

<script setup lang="ts">
import type { PageDto } from '~/composables/usePages'

const { getAll, remove } = usePages()

const pages = ref<PageDto[]>([])
const error = ref('')

onMounted(load)

async function load() {
  try {
    pages.value = await getAll()
  } catch (e: unknown) {
    error.value = (e as Error).message
  }
}

async function confirmDelete(page: PageDto) {
  if (!confirm(`Pagina "${page.title}" verwijderen?`)) return

  try {
    await remove(page.id)
    pages.value = pages.value.filter(item => item.id !== page.id)
  } catch (e: unknown) {
    error.value = (e as Error).message
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString('nl-NL', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  })
}
</script>
