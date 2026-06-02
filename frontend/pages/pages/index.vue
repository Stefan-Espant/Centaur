<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Pagina's</div>
        <div class="content-count">{{ filteredPages.length }} pagina's</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/pages/new" class="btn btn-primary">+ Nieuwe pagina</NuxtLink>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <div class="status-tabs">
      <button
        v-for="tab in tabs"
        :key="tab.value"
        class="status-tab"
        :class="{ active: activeTab === tab.value }"
        @click="activeTab = tab.value"
      >
        {{ tab.label }}
        <span class="status-tab-count">{{ countByStatus(tab.value) }}</span>
      </button>
    </div>

    <table v-if="filteredPages.length > 0" class="table">
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
        <tr v-for="page in filteredPages" :key="page.id">
          <td>
            <div class="row-title-wrap">
              <div class="row-title">{{ page.title }}</div>
              <span class="status-badge" :class="statusClass(page.status)">
                {{ statusLabel(page.status) }}
              </span>
            </div>
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
      Geen pagina's gevonden.
    </div>
  </div>
</template>

<script setup lang="ts">
import type { PageDto } from '~/composables/usePages'

const { getAll, remove } = usePages()

const pages = ref<PageDto[]>([])
const error = ref('')
const activeTab = ref<string>('all')

const tabs = [
  { value: 'all', label: 'Alle' },
  { value: 'draft', label: 'Concept' },
  { value: 'published', label: 'Gepubliceerd' },
  { value: 'archived', label: 'Gearchiveerd' },
]

const filteredPages = computed(() => {
  if (activeTab.value === 'all') return pages.value
  return pages.value.filter(p => effectiveStatus(p.status) === activeTab.value)
})

function effectiveStatus(status: string | null): string {
  return status === 'draft' || status === 'archived' ? status : 'published'
}

function countByStatus(tab: string): number {
  if (tab === 'all') return pages.value.length
  return pages.value.filter(p => effectiveStatus(p.status) === tab).length
}

function statusLabel(status: string | null): string {
  if (status === 'draft') return 'Concept'
  if (status === 'archived') return 'Gearchiveerd'
  return 'Gepubliceerd'
}

function statusClass(status: string | null): string {
  if (status === 'draft') return 'status-draft'
  if (status === 'archived') return 'status-archived'
  return 'status-published'
}

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
    day: '2-digit', month: '2-digit', year: 'numeric'
  })
}
</script>

<style scoped>
.status-tabs {
  display: flex;
  gap: 4px;
  margin-bottom: 16px;
}

.status-tab {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border-radius: 8px;
  border: 1px solid var(--color-border);
  background: var(--color-glass);
  color: var(--color-muted);
  font-size: 13px;
  cursor: pointer;
  transition: all .15s;
}
.status-tab.active {
  background: var(--color-primary-grad);
  color: #fff;
  border-color: transparent;
}
.status-tab-count {
  background: rgba(0,0,0,.12);
  border-radius: 999px;
  padding: 1px 7px;
  font-size: 11px;
}

.row-title-wrap {
  display: flex;
  align-items: center;
  gap: 8px;
}

.status-badge {
  display: inline-block;
  padding: 2px 8px;
  border-radius: 999px;
  font-size: 11px;
  font-weight: 600;
  letter-spacing: .02em;
  white-space: nowrap;
}
.status-published { background: #dcfce7; color: #15803d; }
.status-draft     { background: #f1f5f9; color: #64748b; }
.status-archived  { background: #fef9c3; color: #a16207; }

html.dark .status-published { background: rgba(21,128,61,.25); color: #86efac; }
html.dark .status-draft     { background: rgba(100,116,139,.2); color: #94a3b8; }
html.dark .status-archived  { background: rgba(161,98,7,.2); color: #fcd34d; }
</style>
