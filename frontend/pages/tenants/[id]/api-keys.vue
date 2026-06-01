<script setup lang="ts">
const route = useRoute()
const tenantId = route.params.id as string
const { get, post, del } = useApi()

interface ApiKey {
  id: string
  label: string
  expiresAt: string | null
  createdAt: string
}

interface CreatedKey extends ApiKey {
  key: string
}

const keys = ref<ApiKey[]>([])
const newKey = ref<CreatedKey | null>(null)
const error = ref('')
const showForm = ref(false)
const newLabel = ref('')
const newExpiry = ref('')

async function load() {
  try {
    keys.value = await get<ApiKey[]>(`/api/tenants/${tenantId}/api-keys`)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'API-sleutels laden mislukt'
  }
}

async function createKey() {
  try {
    const created = await post<CreatedKey>(`/api/tenants/${tenantId}/api-keys`, {
      label: newLabel.value,
      expiresAt: newExpiry.value || null
    })

    newKey.value = created
    keys.value.push(created)
    keys.value.sort((a, b) => a.label.localeCompare(b.label))
    showForm.value = false
    newLabel.value = ''
    newExpiry.value = ''
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'API-sleutel aanmaken mislukt'
  }
}

async function deleteKey(id: string) {
  if (!confirm('API-sleutel verwijderen?')) return

  try {
    await del(`/api/tenants/${tenantId}/api-keys/${id}`)
    keys.value = keys.value.filter((key) => key.id !== id)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Verwijderen mislukt'
  }
}

onMounted(load)
</script>

<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">API-sleutels</div>
        <div class="content-count">{{ keys.length }} sleutels</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/tenants" class="btn btn-ghost">← Terug</NuxtLink>
        <button class="btn btn-primary" @click="showForm = !showForm">+ Sleutel aanmaken</button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <div v-if="newKey" class="alert alert-success">
      <strong>Sla deze sleutel nu op, hij wordt nooit meer getoond:</strong><br>
      <code style="font-size:12px;word-break:break-all">{{ newKey.key }}</code>
      <button class="table-action" style="margin-left:.5rem" @click="newKey = null">Sluiten</button>
    </div>

    <div v-if="showForm" class="panel" style="margin-bottom:1rem">
      <form class="inline-form" style="grid-template-columns:1fr 1fr auto;align-items:end" @submit.prevent="createKey">
        <div class="form-group" style="margin:0">
          <label class="form-label">Label</label>
          <input v-model="newLabel" type="text" class="form-input" required placeholder="Frontend productie" />
        </div>
        <div class="form-group" style="margin:0">
          <label class="form-label">Vervaldatum (optioneel)</label>
          <input v-model="newExpiry" type="date" class="form-input" />
        </div>
        <button type="submit" class="btn btn-primary">Aanmaken</button>
      </form>
    </div>

    <UiDataTable>
      <thead>
        <tr><th>Label</th><th>Vervalt</th><th>Aangemaakt</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="key in keys" :key="key.id">
          <td><div class="row-title">{{ key.label }}</div></td>
          <td style="font-size:12px;color:#555">{{ key.expiresAt ? new Date(key.expiresAt).toLocaleDateString('nl-NL') : 'Nooit' }}</td>
          <td style="color:#888;font-size:12px">{{ new Date(key.createdAt).toLocaleDateString('nl-NL') }}</td>
          <td><button class="table-action" style="color:#dc2626" @click="deleteKey(key.id)">Verwijderen</button></td>
        </tr>
        <tr v-if="keys.length === 0">
          <td colspan="4" style="text-align:center;color:#888;padding:2rem">Geen API-sleutels</td>
        </tr>
      </tbody>
    </UiDataTable>
  </div>
</template>
