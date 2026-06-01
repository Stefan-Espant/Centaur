<script setup lang="ts">
const { get, put } = useApi()

interface WebsiteSettings {
  siteName: string
  metaDescription: string
  heroTitle: string
  heroSubtitle: string
  introText: string
  contactEmail: string
  primaryColor: string
}

const settings = reactive<WebsiteSettings>({
  siteName: '',
  metaDescription: '',
  heroTitle: '',
  heroSubtitle: '',
  introText: '',
  contactEmail: '',
  primaryColor: '#1a1a1a'
})

const error = ref('')
const saved = ref(false)
const loading = ref(false)

async function load() {
  error.value = ''

  try {
    Object.assign(settings, await get<WebsiteSettings>('/api/website'))
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Website-instellingen laden mislukt'
  }
}

async function save() {
  error.value = ''
  saved.value = false
  loading.value = true

  try {
    Object.assign(settings, await put<WebsiteSettings>('/api/website', settings))
    saved.value = true
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Website-instellingen opslaan mislukt'
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Website</div>
        <div class="content-count">Basiscontent en uitstraling</div>
      </div>
      <div class="content-actions">
        <button class="btn btn-primary" :disabled="loading" @click="save">
          {{ loading ? 'Opslaan...' : 'Wijzigingen opslaan' }}
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>
    <div v-if="saved" class="alert alert-success">Website-instellingen opgeslagen.</div>

    <div class="website-editor">
      <form class="panel stack" @submit.prevent="save">
        <div class="form-group">
          <label class="form-label">Websitenaam</label>
          <input v-model="settings.siteName" type="text" class="form-input" required />
        </div>

        <div class="form-group">
          <label class="form-label">SEO omschrijving</label>
          <textarea v-model="settings.metaDescription" class="form-input textarea" rows="3" maxlength="180" />
          <div class="field-hint">{{ settings.metaDescription.length }}/180 tekens</div>
        </div>

        <div class="form-group">
          <label class="form-label">Hero titel</label>
          <input v-model="settings.heroTitle" type="text" class="form-input" required />
        </div>

        <div class="form-group">
          <label class="form-label">Hero subtitel</label>
          <textarea v-model="settings.heroSubtitle" class="form-input textarea" rows="3" />
        </div>

        <div class="form-group">
          <label class="form-label">Intro tekst</label>
          <textarea v-model="settings.introText" class="form-input textarea" rows="5" />
        </div>

        <div class="form-grid">
          <div class="form-group">
            <label class="form-label">Contact e-mailadres</label>
            <input v-model="settings.contactEmail" type="email" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Primaire kleur</label>
            <div class="color-control">
              <input v-model="settings.primaryColor" type="color" class="color-input" aria-label="Primaire kleur" />
              <input v-model="settings.primaryColor" type="text" class="form-input" pattern="#[0-9a-fA-F]{6}" />
            </div>
          </div>
        </div>
      </form>

      <section class="website-preview" :style="{ '--preview-color': settings.primaryColor }">
        <div class="preview-topbar">
          <div class="preview-brand">{{ settings.siteName || 'Mijn website' }}</div>
          <div class="preview-pill">Live preview</div>
        </div>
        <div class="preview-hero">
          <div class="preview-kicker">Website</div>
          <h1>{{ settings.heroTitle || 'Welkom op onze website' }}</h1>
          <p>{{ settings.heroSubtitle || 'Vertel hier kort wat bezoekers direct moeten weten.' }}</p>
        </div>
        <div class="preview-body">
          <p>{{ settings.introText || 'Hier verschijnt de introductietekst van de website.' }}</p>
          <a v-if="settings.contactEmail" :href="`mailto:${settings.contactEmail}`">Contact opnemen</a>
        </div>
      </section>
    </div>
  </div>
</template>
