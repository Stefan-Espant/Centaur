<script setup lang="ts">
const { get, del } = useApi()

interface Tenant {
  id: string
  name: string
  slug: string
  createdAt: string
}

const tenants = ref<Tenant[]>([])
const error = ref('')
const deleting = ref<string | null>(null)
const showConfirm = ref<Tenant | null>(null)

async function load() {
  try {
    tenants.value = await get<Tenant[]>('/api/tenants')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Tenants laden mislukt'
  }
}

async function confirmDelete() {
  if (!showConfirm.value) return

  deleting.value = showConfirm.value.id
  try {
    await del(`/api/tenants/${showConfirm.value.id}`)
    tenants.value = tenants.value.filter((tenant) => tenant.id !== showConfirm.value?.id)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Verwijderen mislukt'
  } finally {
    deleting.value = null
    showConfirm.value = null
  }
}

onMounted(load)
</script>

<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Tenants</div>
        <div class="content-count">{{ tenants.length }} klanten</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/tenants/new" class="btn btn-primary">+ Nieuwe tenant</NuxtLink>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <UiDataTable>
      <thead>
        <tr>
          <th>Naam</th>
          <th>Slug</th>
          <th>Aangemaakt</th>
          <th>Gebruikers</th>
          <th>API-sleutels</th>
          <th></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="tenant in tenants" :key="tenant.id">
          <td><div class="row-title">{{ tenant.name }}</div></td>
          <td><div class="row-meta">{{ tenant.slug }}</div></td>
          <td style="color:#888;font-size:12px">{{ new Date(tenant.createdAt).toLocaleDateString('nl-NL') }}</td>
          <td><NuxtLink :to="`/tenants/${tenant.id}/users`" class="table-action">Gebruikers</NuxtLink></td>
          <td><NuxtLink :to="`/tenants/${tenant.id}/api-keys`" class="table-action">API-sleutels</NuxtLink></td>
          <td><button class="table-action" style="color:#dc2626" @click="showConfirm = tenant">Verwijderen</button></td>
        </tr>
        <tr v-if="tenants.length === 0">
          <td colspan="6" style="text-align:center;color:#888;font-size:13px;padding:2rem">Nog geen tenants</td>
        </tr>
      </tbody>
    </UiDataTable>

    <UiConfirmModal
      v-if="showConfirm"
      :loading="!!deleting"
      title="Tenant verwijderen?"
      :message="`Weet je zeker dat je ${showConfirm.name} wilt verwijderen? Dit kan niet ongedaan worden gemaakt.`"
      @close="showConfirm = null"
      @confirm="confirmDelete"
    />
  </div>
</template>
