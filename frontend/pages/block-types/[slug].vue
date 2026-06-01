<template>
  <div style="max-width:640px">
    <div class="content-header">
      <div>
        <div class="content-title">{{ form.name || 'Bloktype bewerken' }}</div>
        <div class="content-count" style="font-family:monospace">{{ $route.params.slug }}</div>
      </div>
      <NuxtLink to="/block-types" class="btn btn-ghost">← Terug</NuxtLink>
    </div>

    <div v-if="loading" style="color:#888;padding:2rem">Laden...</div>

    <form v-else style="background:#fff;border:1px solid #ddd;padding:1.5rem" @submit.prevent="submit">
      <div class="form-group">
        <label class="form-label">Naam</label>
        <input v-model="form.name" type="text" class="form-input" required />
      </div>

      <div class="form-group">
        <label class="form-label">Velden</label>
        <FieldsFieldBuilder v-model="form.fields" :exclude-types="['blocks', 'repeater']" />
      </div>

      <div v-if="errors.length" class="alert alert-error">
        <div v-for="e in errors" :key="e">{{ e }}</div>
      </div>

      <div style="display:flex;gap:8px;margin-top:1.25rem">
        <button type="submit" class="btn btn-primary" :disabled="saving">
          {{ saving ? 'Opslaan...' : 'Wijzigingen opslaan' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import type { UpdateBlockTypeRequest } from '~/composables/useBlockTypes'

const route = useRoute()
const router = useRouter()
const { getBySlug, update } = useBlockTypes()

const slug = computed(() => route.params.slug as string)
const form = reactive<UpdateBlockTypeRequest>({ name: '', fields: [] })
const loading = ref(true)
const errors = ref<string[]>([])
const saving = ref(false)

onMounted(async () => {
  try {
    const bt = await getBySlug(slug.value)
    form.name = bt.name
    form.fields = bt.fields
  } catch (e: unknown) {
    errors.value = [(e as Error).message]
  } finally {
    loading.value = false
  }
})

async function submit() {
  saving.value = true
  errors.value = []
  try {
    await update(slug.value, form)
    await router.push('/block-types')
  } catch (e: unknown) {
    const err = e as { data?: { errors?: string[] }; message?: string }
    errors.value = err?.data?.errors ?? [err?.message ?? 'Er is iets misgegaan.']
  } finally {
    saving.value = false
  }
}
</script>
