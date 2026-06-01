<script setup lang="ts">
const { post } = useApi()

const name = ref('')
const slug = ref('')
const adminEmail = ref('')
const adminPassword = ref('')
const error = ref('')
const loading = ref(false)

watch(name, (value) => {
  slug.value = value.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '')
})

async function submit() {
  error.value = ''
  loading.value = true

  try {
    await post('/api/tenants', {
      name: name.value,
      slug: slug.value,
      adminEmail: adminEmail.value,
      adminPassword: adminPassword.value
    })

    await navigateTo('/tenants')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Tenant aanmaken mislukt'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div style="max-width:480px">
    <div class="content-header">
      <div class="content-title">Nieuwe tenant</div>
      <NuxtLink to="/tenants" class="btn btn-ghost">Annuleren</NuxtLink>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <form class="panel" @submit.prevent="submit">
      <div class="form-group">
        <label class="form-label">Bedrijfsnaam</label>
        <input v-model="name" type="text" class="form-input" required placeholder="Bakkerij De Molen" />
      </div>
      <div class="form-group">
        <label class="form-label">Slug</label>
        <input v-model="slug" type="text" class="form-input" required pattern="[a-z0-9\\-]+" />
        <div style="font-size:11px;color:#888;margin-top:.25rem">Automatisch gegenereerd en aanpasbaar.</div>
      </div>
      <div class="form-group">
        <label class="form-label">Admin e-mailadres</label>
        <input v-model="adminEmail" type="email" class="form-input" required />
      </div>
      <div class="form-group">
        <label class="form-label">Admin wachtwoord</label>
        <input v-model="adminPassword" type="password" class="form-input" required minlength="8" />
      </div>
      <div style="margin-top:1.25rem">
        <button type="submit" class="btn btn-primary" :disabled="loading">
          {{ loading ? 'Bezig...' : 'Tenant aanmaken' }}
        </button>
      </div>
    </form>
  </div>
</template>
