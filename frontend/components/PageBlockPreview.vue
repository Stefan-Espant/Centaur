<template>
  <section v-if="block._type === 'section'" class="demo-block demo-section">
    <div class="demo-eyebrow">Section</div>
    <div class="demo-section-grid">
      <div><strong>Naam:</strong> {{ readText('name', 'Sectie') }}</div>
      <div><strong>Anchor:</strong> {{ readText('anchor', '-') }}</div>
      <div><strong>Variant:</strong> {{ readText('variant', 'default') }}</div>
      <div><strong>Breedte:</strong> {{ readText('width', 'contained') }}</div>
    </div>
  </section>

  <section v-else-if="block._type === 'titel'" class="demo-block demo-title">
    <div class="demo-kicker">{{ readText('kicker', 'Kicker') }}</div>
    <h2>{{ readText('title', 'Titel blok') }}</h2>
  </section>

  <section v-else-if="block._type === 'paragraph'" class="demo-block demo-copy">
    <h3>{{ readText('title', 'Paragraaf') }}</h3>
    <p>{{ readText('content', 'Paragraaftekst') }}</p>
  </section>

  <section v-else-if="block._type === 'button'" class="demo-block demo-actions">
    <a href="/" class="demo-button" :class="`is-${readText('style', 'primary')}`">
      {{ readText('label', 'Knop') }}
    </a>
  </section>

  <figure v-else-if="block._type === 'medium'" class="demo-block demo-media">
    <img :src="readText('src', fallbackImage)" :alt="readText('alt', 'Media preview')" />
    <figcaption>{{ readText('caption', '') }}</figcaption>
  </figure>

  <section v-else-if="block._type === 'gallery'" class="demo-block demo-gallery">
    <h3>{{ readText('title', 'Galerij') }}</h3>
    <div class="demo-gallery-grid">
      <figure v-for="(item, index) in readArray('items')" :key="index">
        <img :src="readNestedText(item, 'image_url', fallbackImage)" :alt="readNestedText(item, 'alt', 'Galerij item')" />
        <figcaption>{{ readNestedText(item, 'caption', '') }}</figcaption>
      </figure>
    </div>
  </section>

  <section v-else-if="block._type === 'reviews_block'" class="demo-block demo-reviews">
    <h3>{{ readText('title', 'Reviews') }}</h3>
    <p>{{ readText('intro', '') }}</p>
    <div class="demo-review-list">
      <article v-for="(review, index) in readArray('reviews')" :key="index" class="demo-review-card">
        <div class="demo-stars">{{ '★'.repeat(Number(readNestedText(review, 'rating', '5'))) }}</div>
        <p>{{ readNestedText(review, 'quote', '') }}</p>
        <strong>{{ readNestedText(review, 'name', 'Naam') }}</strong>
        <span>{{ readNestedText(review, 'role', '') }}</span>
      </article>
    </div>
  </section>

  <section v-else-if="block._type === 'form'" class="demo-block demo-form">
    <h3>{{ readText('title', 'Formulier') }}</h3>
    <p>{{ readText('intro', '') }}</p>
    <div class="demo-form-fields">
      <div v-for="(field, index) in readArray('fields')" :key="index" class="demo-form-field">
        <label>{{ readNestedText(field, 'label', 'Veld') }}</label>
        <div class="demo-input">{{ readNestedText(field, 'placeholder', 'Placeholder') }}</div>
      </div>
    </div>
    <button type="button" class="demo-button is-primary">{{ readText('submit_label', 'Versturen') }}</button>
  </section>

  <section v-else class="demo-block demo-fallback">
    <div class="demo-eyebrow">{{ block._type }}</div>
    <p>Dit blok is toegevoegd aan de pagina en klaar om bewerkt te worden.</p>
  </section>
</template>

<script setup lang="ts">
import type { BlockInstance } from '~/composables/usePages'

const fallbackImage = 'https://images.unsplash.com/photo-1516321318423-f06f85e504b3?auto=format&fit=crop&w=1200&q=80'

const props = defineProps<{
  block: BlockInstance
}>()

function readText(key: string, fallback: string) {
  const value = props.block[key]
  return typeof value === 'string' && value.trim().length > 0 ? value : fallback
}

function readArray(key: string) {
  const value = props.block[key]
  return Array.isArray(value) ? value as Record<string, unknown>[] : []
}

function readNestedText(source: Record<string, unknown>, key: string, fallback: string) {
  const value = source[key]
  return typeof value === 'string' && value.trim().length > 0 ? value : fallback
}
</script>
