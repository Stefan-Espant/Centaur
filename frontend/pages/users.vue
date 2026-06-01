<script setup lang="ts">
const { get, post, del } = useApi()

interface User {
  id: string
  email: string
  role: string
  createdAt: string
}

const users = ref<User[]>([])
const error = ref('')
const showForm = ref(false)
const newEmail = ref('')
const newPassword = ref('')
const newRole = ref('Editor')

async function load() {
  try {
    users.value = await get<User[]>('/api/users')
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Gebruikers laden mislukt'
  }
}

async function createUser() {
  try {
    const user = await post<User>('/api/users', {
      email: newEmail.value,
      password: newPassword.value,
      role: newRole.value
    })

    users.value.push(user)
    users.value.sort((a, b) => a.email.localeCompare(b.email))
    showForm.value = false
    newEmail.value = ''
    newPassword.value = ''
    newRole.value = 'Editor'
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Gebruiker aanmaken mislukt'
  }
}

async function deleteUser(id: string) {
  if (!confirm('Gebruiker verwijderen?')) return

  try {
    await del(`/api/users/${id}`)
    users.value = users.value.filter((user) => user.id !== id)
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
        <div class="content-title">Gebruikers</div>
        <div class="content-count">{{ users.length }} gebruikers</div>
      </div>
      <div class="content-actions">
        <button class="btn btn-primary" @click="showForm = !showForm">+ Gebruiker toevoegen</button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <div v-if="showForm" class="panel" style="margin-bottom:1rem">
      <form class="inline-form" style="grid-template-columns:1fr 1fr auto auto;align-items:end" @submit.prevent="createUser">
        <div class="form-group" style="margin:0">
          <label class="form-label">E-mail</label>
          <input v-model="newEmail" type="email" class="form-input" required />
        </div>
        <div class="form-group" style="margin:0">
          <label class="form-label">Wachtwoord</label>
          <input v-model="newPassword" type="password" class="form-input" required minlength="8" />
        </div>
        <div class="form-group" style="margin:0">
          <label class="form-label">Rol</label>
          <select v-model="newRole" class="form-input">
            <option>Admin</option>
            <option>Editor</option>
            <option>Viewer</option>
          </select>
        </div>
        <button type="submit" class="btn btn-primary">Toevoegen</button>
      </form>
    </div>

    <UiDataTable>
      <thead>
        <tr><th>E-mailadres</th><th>Rol</th><th>Aangemaakt</th><th></th></tr>
      </thead>
      <tbody>
        <tr v-for="user in users" :key="user.id">
          <td><div class="row-title">{{ user.email }}</div></td>
          <td style="font-size:12px;color:#555">{{ user.role }}</td>
          <td style="color:#888;font-size:12px">{{ new Date(user.createdAt).toLocaleDateString('nl-NL') }}</td>
          <td><button class="table-action" style="color:#dc2626" @click="deleteUser(user.id)">Verwijderen</button></td>
        </tr>
        <tr v-if="users.length === 0">
          <td colspan="4" style="text-align:center;color:#888;padding:2rem">Geen gebruikers</td>
        </tr>
      </tbody>
    </UiDataTable>
  </div>
</template>
