<script setup lang="ts">
import type { BlockTypeDto } from '~/composables/useBlockTypes'

interface PageDto {
  id: string
  title: string
  slug: string
  updatedAt: string
  createdAt: string
}

interface ApiKey { id: string }
interface User { id: string }

const { get } = useApi()

const loading = ref(true)
const pages = ref<PageDto[]>([])
const blockTypes = ref<BlockTypeDto[]>([])
const users = ref<User[]>([])
const apiKeys = ref<ApiKey[]>([])

onMounted(async () => {
  try {
    ;[pages.value, blockTypes.value, users.value, apiKeys.value] = await Promise.all([
      get<PageDto[]>('/api/pages'),
      get<BlockTypeDto[]>('/api/admin/block-types'),
      get<User[]>('/api/users'),
      get<ApiKey[]>('/api/api-keys'),
    ])
  } finally {
    loading.value = false
  }
})

const recentPages = computed(() =>
  [...pages.value]
    .sort((a, b) => new Date(b.updatedAt).getTime() - new Date(a.updatedAt).getTime())
    .slice(0, 5)
)

// ── Activity chart ────────────────────────────────────────────────────────────
const CHART_DAYS = 30
const W = 600
const H = 72
const PAD_X = 0

const chartDays = computed(() => {
  const today = new Date()
  return Array.from({ length: CHART_DAYS }, (_, i) => {
    const d = new Date(today)
    d.setDate(d.getDate() - (CHART_DAYS - 1 - i))
    return d.toISOString().slice(0, 10)
  })
})

const chartCounts = computed(() => {
  const counts = Object.fromEntries(chartDays.value.map(d => [d, 0]))
  for (const page of pages.value) {
    const c = page.createdAt?.slice(0, 10)
    const u = page.updatedAt?.slice(0, 10)
    if (c && counts[c] !== undefined) counts[c]++
    if (u && u !== c && counts[u] !== undefined) counts[u]++
  }
  return chartDays.value.map(d => counts[d])
})

const chartPoints = computed(() => {
  const max = Math.max(...chartCounts.value, 1)
  const step = (W - PAD_X * 2) / (CHART_DAYS - 1)
  return chartCounts.value.map((count, i) => ({
    x: PAD_X + i * step,
    y: H - (count / max) * (H - 8) - 4,
  }))
})

function smoothPath(pts: { x: number; y: number }[]) {
  if (pts.length < 2) return ''
  let d = `M ${pts[0].x} ${pts[0].y}`
  for (let i = 0; i < pts.length - 1; i++) {
    const cp1x = pts[i].x + (pts[i + 1].x - (pts[i - 1]?.x ?? pts[i].x)) / 5
    const cp1y = pts[i].y + (pts[i + 1].y - (pts[i - 1]?.y ?? pts[i].y)) / 5
    const cp2x = pts[i + 1].x - ((pts[i + 2]?.x ?? pts[i + 1].x) - pts[i].x) / 5
    const cp2y = pts[i + 1].y - ((pts[i + 2]?.y ?? pts[i + 1].y) - pts[i].y) / 5
    d += ` C ${cp1x} ${cp1y} ${cp2x} ${cp2y} ${pts[i + 1].x} ${pts[i + 1].y}`
  }
  return d
}

const linePath = computed(() => smoothPath(chartPoints.value))

const areaPath = computed(() => {
  const pts = chartPoints.value
  if (!pts.length) return ''
  return `${linePath.value} L ${pts[pts.length - 1].x} ${H} L ${pts[0].x} ${H} Z`
})

const xLabels = computed(() =>
  chartDays.value
    .map((d, i) => ({ i, label: new Date(d).toLocaleDateString('nl-NL', { day: 'numeric', month: 'short' }) }))
    .filter((_, i) => i % 7 === 0 || i === CHART_DAYS - 1)
)

function timeAgo(dateStr: string) {
  const diff = Date.now() - new Date(dateStr).getTime()
  const m = Math.floor(diff / 60000)
  if (m < 1) return 'zojuist'
  if (m < 60) return `${m}m geleden`
  const h = Math.floor(m / 60)
  if (h < 24) return `${h}u geleden`
  const d = Math.floor(h / 24)
  if (d < 30) return `${d}d geleden`
  return new Date(dateStr).toLocaleDateString('nl-NL', { day: 'numeric', month: 'short' })
}

const stats = computed(() => [
  {
    label: "Pagina's",
    value: pages.value.length,
    icon: '<path d="M9 1H3a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V6z"/><polyline points="9 1 9 6 14 6"/><line x1="5" y1="9" x2="11" y2="9"/><line x1="5" y1="12" x2="9" y2="12"/>',
    to: '/pages',
    color: '#16a34a',
  },
  {
    label: 'Bloktypen',
    icon: '<path d="M8 1 1 4.5v7L8 15l7-3.5v-7z"/><line x1="8" y1="1" x2="8" y2="15"/><path d="M1 4.5 8 8l7-3.5"/>',
    value: blockTypes.value.length,
    to: '/block-types',
    color: '#0d9488',
  },
  {
    label: 'Gebruikers',
    icon: '<circle cx="6" cy="5" r="3"/><path d="M1 14a5 5 0 0 1 10 0"/><path d="M11 3a3 3 0 0 1 0 6"/><path d="M15 14a5 5 0 0 0-4-4.9"/>',
    value: users.value.length,
    to: '/users',
    color: '#2563eb',
  },
  {
    label: 'API-sleutels',
    icon: '<circle cx="5.5" cy="5.5" r="4"/><path d="M8.5 8.5 15 15"/><path d="M12 13l2-2"/>',
    value: apiKeys.value.length,
    to: '/api-keys',
    color: '#7c3aed',
  },
])
</script>

<template>
  <div>
    <div class="content-header">
      <div>
        <div class="content-title">Dashboard</div>
        <div class="content-count">Overzicht van je website</div>
      </div>
    </div>

    <!-- Stat cards -->
    <div class="dash-stats">
      <NuxtLink
        v-for="stat in stats"
        :key="stat.label"
        :to="stat.to"
        class="stat-card"
      >
        <div class="stat-icon-wrap" :style="{ background: stat.color + '18', color: stat.color }">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" v-html="stat.icon" />
        </div>
        <div>
          <div class="stat-value">
            <span v-if="loading" class="stat-skeleton" />
            <span v-else>{{ stat.value }}</span>
          </div>
          <div class="stat-label">{{ stat.label }}</div>
        </div>
      </NuxtLink>
    </div>

    <!-- Activiteitsgrafiek -->
    <div class="panel chart-panel">
      <div class="chart-header">
        <div class="dash-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="1 12 5 7 8 9 11 5 15 8"/><line x1="1" y1="15" x2="15" y2="15"/>
          </svg>
          Paginawijzigingen — laatste 30 dagen
        </div>
      </div>

      <div class="chart-wrap">
        <svg :viewBox="`0 0 ${W} ${H + 20}`" preserveAspectRatio="none" class="chart-svg">
          <defs>
            <linearGradient id="chart-fill" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" stop-color="#16a34a" stop-opacity="0.25"/>
              <stop offset="100%" stop-color="#16a34a" stop-opacity="0.02"/>
            </linearGradient>
          </defs>

          <!-- Area fill -->
          <path :d="areaPath" fill="url(#chart-fill)" />

          <!-- Line -->
          <path :d="linePath" fill="none" stroke="#16a34a" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />

          <!-- Data dots -->
          <circle
            v-for="pt in chartPoints"
            :key="pt.x"
            :cx="pt.x"
            :cy="pt.y"
            r="2.5"
            fill="#16a34a"
            opacity="0.7"
          />

          <!-- X-axis labels -->
          <text
            v-for="lbl in xLabels"
            :key="lbl.i"
            :x="chartPoints[lbl.i]?.x ?? 0"
            :y="H + 16"
            text-anchor="middle"
            font-size="8"
            fill="#94a3b8"
          >{{ lbl.label }}</text>
        </svg>
      </div>
    </div>

    <!-- Main grid -->
    <div class="dash-grid">

      <!-- Recente pagina's -->
      <div class="panel stack dash-recent">
        <div class="dash-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <path d="M9 1H3a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V6z"/><polyline points="9 1 9 6 14 6"/>
          </svg>
          Recente pagina's
        </div>

        <div v-if="loading" class="dash-loading">Laden…</div>

        <div v-else-if="recentPages.length === 0" class="dash-empty">
          Nog geen pagina's aangemaakt.
        </div>

        <div v-else class="recent-list">
          <NuxtLink
            v-for="page in recentPages"
            :key="page.id"
            :to="`/pages/${page.id}`"
            class="recent-item"
          >
            <div>
              <div class="recent-title">{{ page.title || 'Naamloos' }}</div>
              <div class="recent-slug">/{{ page.slug }}</div>
            </div>
            <div class="recent-time">{{ timeAgo(page.updatedAt) }}</div>
          </NuxtLink>
        </div>

        <NuxtLink to="/pages" class="dash-view-all">Alle pagina's bekijken →</NuxtLink>
      </div>

      <!-- Snelkoppelingen -->
      <div class="dash-actions">
        <div class="dash-section-title">
          <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <line x1="8" y1="2" x2="8" y2="14"/><line x1="2" y1="8" x2="14" y2="8"/>
          </svg>
          Snel starten
        </div>

        <NuxtLink to="/pages/new" class="action-card">
          <div class="action-icon" style="background: rgba(22,163,74,.12); color: #16a34a;">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M9 1H3a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h10a1 1 0 0 0 1-1V6z"/><polyline points="9 1 9 6 14 6"/><line x1="8" y1="9" x2="8" y2="13"/><line x1="6" y1="11" x2="10" y2="11"/>
            </svg>
          </div>
          <span class="action-title">Nieuwe pagina</span>
        </NuxtLink>

        <NuxtLink to="/block-types/new" class="action-card">
          <div class="action-icon" style="background: rgba(13,148,136,.12); color: #0d9488;">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <path d="M8 1 1 4.5v7L8 15l7-3.5v-7z"/><line x1="8" y1="1" x2="8" y2="15"/><path d="M1 4.5 8 8l7-3.5"/>
            </svg>
          </div>
          <span class="action-title">Nieuw bloktype</span>
        </NuxtLink>

        <NuxtLink to="/website" class="action-card">
          <div class="action-icon" style="background: rgba(37,99,235,.12); color: #2563eb;">
            <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="8" cy="8" r="7"/><path d="M8 1c-2 2-3 4-3 7s1 5 3 7"/><path d="M8 1c2 2 3 4 3 7s-1 5-3 7"/><line x1="1" y1="8" x2="15" y2="8"/>
            </svg>
          </div>
          <span class="action-title">Website-instellingen</span>
        </NuxtLink>
      </div>

    </div>
  </div>
</template>

<style scoped>
.dash-stats {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 12px;
  margin-bottom: 16px;
}

.stat-card {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 18px 20px;
  background: var(--color-glass);
  backdrop-filter: var(--blur);
  -webkit-backdrop-filter: var(--blur);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  box-shadow: var(--shadow-sm);
  text-decoration: none;
  transition: box-shadow .15s, transform .15s;
}
.stat-card:hover { box-shadow: var(--shadow-md); transform: translateY(-1px); }

.stat-icon-wrap {
  width: 42px; height: 42px;
  border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
  flex-shrink: 0;
}
.stat-icon-wrap svg { width: 18px; height: 18px; }

.stat-value {
  font-size: 28px;
  font-weight: 800;
  letter-spacing: -.05em;
  color: var(--color-text);
  line-height: 1;
  margin-bottom: 2px;
}
.stat-label { font-size: 12px; color: var(--color-muted); font-weight: 500; }
.stat-skeleton { display: inline-block; width: 32px; height: 28px; background: rgba(200,210,230,.4); border-radius: 6px; animation: pulse 1.2s ease-in-out infinite; }

@keyframes pulse { 0%,100% { opacity: 1 } 50% { opacity: .4 } }

.dash-grid {
  display: grid;
  grid-template-columns: 1fr 280px;
  gap: 12px;
}

.dash-section-title {
  display: flex;
  align-items: center;
  gap: 7px;
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text);
  letter-spacing: -.01em;
}
.dash-section-title svg { width: 14px; height: 14px; color: var(--color-primary); }

.dash-loading { font-size: 13px; color: var(--color-muted); }
.dash-empty { font-size: 13px; color: var(--color-muted); padding: 8px 0; }

.recent-list { display: flex; flex-direction: column; }
.recent-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 0;
  border-bottom: 1px solid var(--color-border-subtle);
  text-decoration: none;
  transition: color .12s;
}
.recent-item:last-child { border-bottom: none; }
.recent-item:hover .recent-title { color: var(--color-primary); }
.recent-title { font-size: 13px; font-weight: 600; color: var(--color-text); }
.recent-slug { font-size: 11px; color: var(--color-muted); margin-top: 1px; }
.recent-time { font-size: 11px; color: var(--color-muted); white-space: nowrap; }

.dash-view-all { margin-top: auto; font-size: 12px; color: var(--color-primary); font-weight: 600; text-decoration: none; }
.dash-view-all:hover { text-decoration: underline; }

.dash-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 18px;
  background: var(--color-glass);
  backdrop-filter: var(--blur);
  -webkit-backdrop-filter: var(--blur);
  border: 1px solid var(--color-border);
  border-radius: 12px;
  box-shadow: var(--shadow-sm);
  align-self: stretch;
}

.action-card {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: rgba(255,255,255,.5);
  border: 1px solid var(--color-border);
  border-radius: 10px;
  text-decoration: none;
  transition: background .12s, box-shadow .12s;
}
.action-card:hover { background: rgba(255,255,255,.85); box-shadow: var(--shadow-sm); }

.action-icon {
  width: 38px; height: 38px; border-radius: 9px;
  display: flex; align-items: center; justify-content: center; flex-shrink: 0;
}
.action-icon svg { width: 16px; height: 16px; }

.action-title { font-size: 13px; font-weight: 600; color: var(--color-text); }

@media (max-width: 900px) {
  .dash-stats { grid-template-columns: repeat(2, 1fr); }
  .dash-grid { grid-template-columns: 1fr; }
}
</style>
