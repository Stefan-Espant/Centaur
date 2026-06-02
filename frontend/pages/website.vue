<script setup lang="ts">
const { get, put } = useApi()

interface Settings {
  siteName: string
  tagline: string
  contactEmail: string
  phone: string
  metaDescription: string
  titleSuffix: string
  primaryColor: string
  secondaryColor: string
  instagram: string
  linkedIn: string
  facebook: string
  twitter: string
  analyticsId: string
  cookieBannerEnabled: boolean
  cookieBannerText: string
  maintenanceMode: boolean
  maintenanceMessage: string
  schemaType: string
}

const settings = reactive<Settings>({
  siteName: '',
  tagline: '',
  contactEmail: '',
  phone: '',
  metaDescription: '',
  titleSuffix: '',
  primaryColor: '#16a34a',
  secondaryColor: '',
  instagram: '',
  linkedIn: '',
  facebook: '',
  twitter: '',
  analyticsId: '',
  cookieBannerEnabled: false,
  cookieBannerText: '',
  maintenanceMode: false,
  maintenanceMessage: '',
  schemaType: 'Organization',
})

const error = ref('')
const saved = ref(false)
const loading = ref(false)

async function load() {
  try {
    Object.assign(settings, await get<Settings>('/api/website'))
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Instellingen laden mislukt'
  }
}

async function save() {
  error.value = ''
  saved.value = false
  loading.value = true
  try {
    Object.assign(settings, await put<Settings>('/api/website', settings))
    saved.value = true
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Instellingen opslaan mislukt'
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
        <div class="content-title">Instellingen</div>
        <div class="content-count">Sitebreed geconfigureerde waarden</div>
      </div>
      <div class="content-actions">
        <button class="btn btn-primary" :disabled="loading" @click="save">
          {{ loading ? 'Opslaan...' : 'Wijzigingen opslaan' }}
        </button>
      </div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>
    <div v-if="saved" class="alert alert-success">Instellingen opgeslagen.</div>

    <div class="settings-grid">

      <!-- Algemeen -->
      <div class="panel stack">
        <div class="settings-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <rect x="1" y="5" width="14" height="10" rx="1.5"/><path d="M5 5V3a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/>
          </svg>
          Algemeen
        </div>
        <div class="form-group">
          <label class="form-label">Websitenaam</label>
          <input v-model="settings.siteName" type="text" class="form-input" />
        </div>
        <div class="form-group">
          <label class="form-label">Tagline / slogan</label>
          <input v-model="settings.tagline" type="text" class="form-input" placeholder="Kort en krachtig" />
        </div>
        <div class="form-grid">
          <div class="form-group">
            <label class="form-label">Contactmail</label>
            <input v-model="settings.contactEmail" type="email" class="form-input" />
          </div>
          <div class="form-group">
            <label class="form-label">Telefoonnummer</label>
            <input v-model="settings.phone" type="tel" class="form-input" />
          </div>
        </div>
      </div>

      <!-- SEO -->
      <div class="panel stack">
        <div class="settings-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="6" cy="6" r="4.5"/><line x1="9.5" y1="9.5" x2="14.5" y2="14.5"/>
          </svg>
          SEO
        </div>
        <div class="form-group">
          <label class="form-label">Standaard meta omschrijving</label>
          <textarea v-model="settings.metaDescription" class="form-input textarea" rows="3" maxlength="160" />
          <div class="field-hint">{{ settings.metaDescription.length }}/160 — fallback voor pagina's zonder eigen omschrijving</div>
        </div>
        <div class="form-group">
          <label class="form-label">Paginatitel suffix</label>
          <input v-model="settings.titleSuffix" type="text" class="form-input" placeholder="— Bedrijfsnaam" />
          <div class="field-hint">Wordt achter elke paginatitel geplaatst</div>
        </div>
        <div class="form-group">
          <label class="form-label">Schema.org type</label>
          <select v-model="settings.schemaType" class="form-input">
            <option value="Organization">Organization — Algemene organisatie</option>
            <option value="LocalBusiness">LocalBusiness — Lokaal bedrijf</option>
            <option value="Store">Store — Winkel</option>
            <option value="Restaurant">Restaurant — Horeca</option>
            <option value="EducationalOrganization">EducationalOrganization — Onderwijs</option>
            <option value="MedicalOrganization">MedicalOrganization — Zorg</option>
            <option value="NGO">NGO — Non-profit</option>
            <option value="SportsOrganization">SportsOrganization — Sport</option>
          </select>
          <div class="field-hint">Gebruikt voor gestructureerde data (SEO rich results)</div>
        </div>
      </div>

      <!-- Huisstijl -->
      <div class="panel stack">
        <div class="settings-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="8" cy="8" r="6.5"/><circle cx="8" cy="8" r="2.5" fill="currentColor" stroke="none"/>
          </svg>
          Huisstijl
        </div>
        <div class="form-grid">
          <div class="form-group">
            <label class="form-label">Primaire kleur</label>
            <div class="color-control">
              <input v-model="settings.primaryColor" type="color" class="color-input" />
              <input v-model="settings.primaryColor" type="text" class="form-input" pattern="#[0-9a-fA-F]{6}" />
            </div>
          </div>
          <div class="form-group">
            <label class="form-label">Secundaire kleur</label>
            <div class="color-control">
              <input v-model="settings.secondaryColor" type="color" class="color-input" />
              <input v-model="settings.secondaryColor" type="text" class="form-input" pattern="#[0-9a-fA-F]{6}" />
            </div>
          </div>
        </div>
      </div>

      <!-- Social media -->
      <div class="panel stack">
        <div class="settings-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="4" r="2"/><circle cx="4" cy="8" r="2"/><circle cx="12" cy="12" r="2"/>
            <line x1="6" y1="7" x2="10" y2="5"/><line x1="6" y1="9" x2="10" y2="11"/>
          </svg>
          Social media
        </div>
        <div class="form-grid">
          <div class="form-group">
            <label class="form-label">Instagram</label>
            <input v-model="settings.instagram" type="url" class="form-input" placeholder="https://instagram.com/..." />
          </div>
          <div class="form-group">
            <label class="form-label">LinkedIn</label>
            <input v-model="settings.linkedIn" type="url" class="form-input" placeholder="https://linkedin.com/in/..." />
          </div>
          <div class="form-group">
            <label class="form-label">Facebook</label>
            <input v-model="settings.facebook" type="url" class="form-input" placeholder="https://facebook.com/..." />
          </div>
          <div class="form-group">
            <label class="form-label">X / Twitter</label>
            <input v-model="settings.twitter" type="url" class="form-input" placeholder="https://x.com/..." />
          </div>
        </div>
      </div>

      <!-- Technisch -->
      <div class="panel stack" style="grid-column: 1 / -1;">
        <div class="settings-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="1 4 5 8 1 12"/><line x1="7" y1="12" x2="15" y2="12"/>
          </svg>
          Technisch
        </div>
        <div class="form-group">
          <label class="form-label">Google Analytics ID</label>
          <input v-model="settings.analyticsId" type="text" class="form-input" placeholder="G-XXXXXXXXXX" />
        </div>
        <div class="form-group">
          <label class="form-label">Cookie banner</label>
          <div class="toggle-row">
            <label class="toggle">
              <input v-model="settings.cookieBannerEnabled" type="checkbox" />
              <span class="toggle-track">
                <span class="toggle-thumb" />
              </span>
            </label>
            <span class="toggle-label">{{ settings.cookieBannerEnabled ? 'Ingeschakeld' : 'Uitgeschakeld' }}</span>
          </div>
        </div>
        <div v-if="settings.cookieBannerEnabled" class="form-group">
          <label class="form-label">Cookie banner tekst</label>
          <textarea v-model="settings.cookieBannerText" class="form-input textarea" rows="2" />
        </div>

        <div class="form-group">
          <label class="form-label">Onderhoudsmodus</label>
          <div class="toggle-row">
            <label class="toggle">
              <input v-model="settings.maintenanceMode" type="checkbox" />
              <span class="toggle-track">
                <span class="toggle-thumb" />
              </span>
            </label>
            <span class="toggle-label" :style="settings.maintenanceMode ? 'color:#dc2626;font-weight:600' : ''">
              {{ settings.maintenanceMode ? '⚠ Website offline voor bezoekers' : 'Uitgeschakeld' }}
            </span>
          </div>
        </div>
        <div v-if="settings.maintenanceMode" class="form-group">
          <label class="form-label">Boodschap voor bezoekers</label>
          <textarea v-model="settings.maintenanceMessage" class="form-input textarea" rows="2" />
        </div>
      </div>

    </div>
  </div>
</template>

<style scoped>
.settings-grid {
  display: grid;
  grid-template-columns: repeat(2, 1fr);
  gap: 14px;
  align-items: stretch;
}

.settings-section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text);
  letter-spacing: -.01em;
  padding-bottom: .75rem;
  border-bottom: 1px solid var(--color-border-subtle);
  margin-bottom: .25rem;
}
.settings-section-title svg { width: 14px; height: 14px; color: var(--color-primary); }

.toggle-row { display: flex; align-items: center; gap: 10px; }
.toggle { position: relative; display: inline-block; width: 36px; height: 20px; flex-shrink: 0; }
.toggle input { opacity: 0; width: 0; height: 0; }
.toggle-track {
  position: absolute; inset: 0;
  background: var(--color-border-subtle);
  border-radius: 999px;
  cursor: pointer;
  transition: background .2s;
  border: 1px solid var(--color-border);
}
.toggle input:checked + .toggle-track { background: var(--color-primary-grad); border-color: transparent; }
.toggle-thumb {
  position: absolute;
  top: 2px; left: 2px;
  width: 14px; height: 14px;
  background: #fff;
  border-radius: 50%;
  transition: transform .2s;
  box-shadow: 0 1px 3px rgba(0,0,0,.2);
}
.toggle input:checked + .toggle-track .toggle-thumb { transform: translateX(16px); }
.toggle-label { font-size: 13px; color: var(--color-muted); }

@media (max-width: 900px) {
  .settings-grid { grid-template-columns: 1fr; }
}
</style>
