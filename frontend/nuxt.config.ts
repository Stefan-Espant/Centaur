export default defineNuxtConfig({
  ssr: false,
  devtools: { enabled: false },
  runtimeConfig: {
    public: {
      apiBase: process.env.NUXT_PUBLIC_API_BASE ?? 'http://localhost:5197'
    }
  },
  css: ['~/assets/css/main.css']
})
