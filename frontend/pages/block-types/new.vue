<template>
  <div style="max-width:640px">
    <div class="content-header">
      <div class="content-title">Nieuw bloktype</div>
      <NuxtLink to="/block-types" class="btn btn-ghost">Annuleren</NuxtLink>
    </div>

    <form style="background:#fff;border:1px solid #ddd;padding:1.5rem" @submit.prevent="submit">
      <div class="form-group">
        <label class="form-label">Naam</label>
        <input v-model="form.name" type="text" class="form-input" required placeholder="bijv. Hero" @input="autoSlug" />
      </div>

      <div class="form-group">
        <label class="form-label">Slug</label>
        <input v-model="form.slug" type="text" class="form-input" required pattern="[a-z0-9_]+"
          placeholder="bijv. hero" />
        <div style="font-size:11px;color:#888;margin-top:4px">
          Alleen lowercase, cijfers en underscores. Kan niet worden gewijzigd na aanmaken.
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">Velden</label>
        <div style="font-size:12px;color:#888;margin-bottom:8px">
          Velden worden toegevoegd via de bewerk-pagina na aanmaken.
        </div>
      </div>

      <div v-if="errors.length" class="alert alert-error">
        <div v-for="e in errors" :key="e">{{ e }}</div>
      </div>

      <div style="display:flex;gap:8px;margin-top:1.25rem">
        <button type="submit" class="btn btn-primary" :disabled="saving">
          {{ saving ? 'Opslaan...' : 'Bloktype aanmaken' }}
        </button>
      </div>
    </form>
  </div>
</template>

<script setup lang="ts">
import type { CreateBlockTypeRequest } from '~/composables/useBlockTypes'

const router = useRouter()
const { create } = useBlockTypes()

const form = reactive<CreateBlockTypeRequest>({ name: '', slug: '', fields: [] })
const errors = ref<string[]>([])
const saving = ref(false)

function autoSlug() {
  form.slug = form.name.toLowerCase().replace(/\s+/g, '_').replace(/[^a-z0-9_]/g, '')
}

async function submit() {
  saving.value = true
  errors.value = []
  try {
    const created = await create(form)
    await router.push(`/block-types/${created.slug}`)
  } catch (e: unknown) {
    const err = e as { data?: { errors?: string[] }; message?: string }
    errors.value = err?.data?.errors ?? [err?.message ?? 'Er is iets misgegaan.']
  } finally {
    saving.value = false
  }
}
</script>
