<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">{{ form.title || 'Pagina bewerken' }}</div>
        <div class="content-count">Werk de pagina-opbouw en URL bij</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/pages" class="btn btn-ghost">Terug</NuxtLink>
        <button
          v-if="form.status === 'draft' || form.status === null"
          class="btn btn-primary"
          :disabled="saving || loading"
          @click="changeStatus('published')"
        >
          Publiceren
        </button>
        <button
          v-else-if="form.status === 'published'"
          class="btn btn-ghost"
          :disabled="saving || loading"
          @click="changeStatus('draft')"
        >
          Depubliceren
        </button>
        <button
          v-else-if="form.status === 'archived'"
          class="btn btn-ghost"
          :disabled="saving || loading"
          @click="changeStatus('draft')"
        >
          Herstellen
        </button>
        <button class="btn btn-primary" :disabled="saving || loading" @click="save">
          {{ saving ? 'Opslaan...' : 'Wijzigingen opslaan' }}
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>
    <div v-if="saved" class="alert alert-success">Wijzigingen opgeslagen.</div>

    <div v-if="loading" class="panel" style="color:#555;font-size:14px">
      Pagina laden...
    </div>

    <section v-else class="panel stack">
      <div class="form-group">
        <label class="form-label">Titel</label>
        <input v-model="form.title" type="text" class="form-input" />
      </div>

      <div class="form-group">
        <label class="form-label">Slug</label>
        <input v-model="form.slug" type="text" class="form-input" />
        <div class="field-hint">URL: `/{{ form.slug || 'voorbeeld-pagina' }}`</div>
      </div>

      <div class="form-group">
        <label class="form-label">Meta omschrijving</label>
        <textarea v-model="form.metaDescription" class="form-input textarea" rows="3" maxlength="180" />
      </div>

      <div class="form-group">
        <label class="form-label">Pagina-opbouw</label>
        <FieldsBlocksField
          :model-value="form.body"
          :allowed-block-type-slugs="allowedBlockTypeSlugs"
          :all-block-types="blockTypes"
          @update:model-value="form.body = $event"
        />
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import type { BlockTypeDto } from '~/composables/useBlockTypes'
import type { SavePageRequest } from '~/composables/usePages'

const route = useRoute()
const { getById, update } = usePages()
const { getAll } = useBlockTypes()

const form = reactive<SavePageRequest>({
  title: '',
  slug: '',
  metaDescription: '',
  body: [],
  status: 'draft'
})

const blockTypes = ref<BlockTypeDto[]>([])
const error = ref('')
const saved = ref(false)
const loading = ref(true)
const saving = ref(false)

const allowedBlockTypeSlugs = computed(() => blockTypes.value.map(blockType => blockType.slug))

onMounted(load)

async function load() {
  error.value = ''
  loading.value = true

  try {
    const [page, availableBlockTypes] = await Promise.all([
      getById(String(route.params.id)),
      getAll()
    ])

    form.title = page.title
    form.slug = page.slug
    form.metaDescription = page.metaDescription
    form.body = page.body ?? []
    form.status = page.status
    blockTypes.value = availableBlockTypes
  } catch (e: unknown) {
    error.value = (e as Error).message
  } finally {
    loading.value = false
  }
}

async function save() {
  error.value = ''
  saved.value = false
  saving.value = true

  try {
    const result = await update(String(route.params.id), form)
    form.status = result.status
    saved.value = true
  } catch (e: unknown) {
    error.value = (e as Error).message
  } finally {
    saving.value = false
  }
}

async function changeStatus(newStatus: string) {
  form.status = newStatus
  await save()
}
</script>
