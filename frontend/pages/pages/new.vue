<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Nieuwe pagina</div>
        <div class="content-count">Maak een pagina met een eigen pagebuilder</div>
      </div>
      <div class="content-actions">
        <NuxtLink to="/pages" class="btn btn-ghost">Annuleren</NuxtLink>
        <button class="btn btn-primary" :disabled="saving" @click="save">
          {{ saving ? 'Opslaan...' : 'Pagina opslaan' }}
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <section class="panel stack">
      <div class="form-group">
        <label class="form-label">Titel</label>
        <input v-model="form.title" type="text" class="form-input" @input="syncSlug" />
      </div>

      <div class="form-group">
        <label class="form-label">Slug</label>
        <input v-model="form.slug" type="text" class="form-input" @input="slugTouched = true" />
        <div class="field-hint">Wordt gebruikt als URL: `/{{ form.slug || 'voorbeeld-pagina' }}`</div>
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

const router = useRouter()
const { create } = usePages()
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
const saving = ref(false)
const slugTouched = ref(false)

const allowedBlockTypeSlugs = computed(() => blockTypes.value.map(blockType => blockType.slug))

onMounted(async () => {
  try {
    blockTypes.value = await getAll()
  } catch (e: unknown) {
    error.value = (e as Error).message
  }
})

function syncSlug() {
  if (slugTouched.value) return
  form.slug = slugify(form.title)
}

function slugify(value: string) {
  return value
    .toLowerCase()
    .trim()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

async function save() {
  error.value = ''
  saving.value = true

  try {
    const created = await create(form)
    await router.push(`/pages/${created.id}`)
  } catch (e: unknown) {
    error.value = (e as Error).message
  } finally {
    saving.value = false
  }
}

</script>
