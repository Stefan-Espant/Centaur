<script setup lang="ts">
definePageMeta({ layout: 'auth' })

const { login, isSuperAdmin } = useAuth()
const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true

  try {
    await login(email.value, password.value)
    await navigateTo(isSuperAdmin.value ? '/tenants' : '/dashboard')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Inloggen mislukt'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-box">
    <div class="auth-logo">
      <AppLogo height="44px" />
    </div>
    <div class="auth-title">Inloggen</div>
    <form @submit.prevent="submit">
      <div class="form-group">
        <label class="form-label">E-mailadres</label>
        <input v-model="email" type="email" class="form-input" autocomplete="email" required />
      </div>
      <div class="form-group">
        <label class="form-label">Wachtwoord</label>
        <input v-model="password" type="password" class="form-input" autocomplete="current-password" required />
      </div>
      <div v-if="error" class="form-error">{{ error }}</div>
      <button type="submit" class="btn btn-primary" style="width:100%;margin-top:1.25rem" :disabled="loading">
        {{ loading ? 'Bezig...' : 'Inloggen' }}
      </button>
    </form>
  </div>
</template>
