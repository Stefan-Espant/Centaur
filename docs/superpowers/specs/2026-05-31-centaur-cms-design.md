# Centaur CMS — Design Document

**Datum:** 2026-05-31  
**Status:** Goedgekeurd  

---

## 1. Doel

Centaur is een headless CMS gebouwd met ASP.NET Core, NuxtJS en eigen CSS, zelfgehost op Europese bodem (Hetzner). Het biedt twee lagen: een gebruiksvriendelijk bewerkscherm voor redacteuren en volledige API-toegang voor ontwikkelaars — vergelijkbaar met DatoCMS maar self-hosted.

---

## 2. Doelgroep

- **Redacteuren** — beheren content via het admin-dashboard zonder technische kennis
- **Ontwikkelaars** — halen content op via REST of GraphQL en bouwen eigen frontends
- **SuperAdmin (Stefan)** — beheert tenants, gebruikers en de hele installatie

---

## 3. Architectuur

### Overzicht

```
Hetzner VPS
├── ASP.NET Core API (Clean Architecture)
│   ├── Domain Layer          — entiteiten, business rules
│   ├── Application Layer     — use cases, services
│   ├── Infrastructure Layer  — database, opslag, e-mail
│   ├── REST API              — admin-dashboard (authenticatie vereist)
│   └── GraphQL API           — content delivery (API-key, read-only)
├── NuxtJS Admin Dashboard    — SPA, communiceert via REST API
└── PostgreSQL                — schema-per-tenant

Hetzner Object Storage        — media (S3-compatibel, los van de server)
```

### Twee API-surfaces

| API | Doel | Authenticatie | Rechten |
|---|---|---|---|
| **REST** | Admin-dashboard | JWT-token | Lezen + schrijven |
| **GraphQL** | Content delivery voor developers | API-key per tenant | Alleen lezen |

---

## 4. Multi-tenancy

Elke klant (tenant) heeft een volledig afgeschermde omgeving:

- **PostgreSQL schema-per-tenant** — bijv. `bakkerij_de_molen.*`, `garage_pietersen.*`
- Geen enkele tenant kan data van een andere tenant zien
- De `public` schema bevat alleen gedeelde tabellen (tenants, users, api_keys)

### Rollen

| Rol | Bevoegdheden |
|---|---|
| **SuperAdmin** | Alle tenants beheren, nieuwe tenants aanmaken |
| **Admin** | Alles binnen eigen tenant |
| **Redacteur** | Content aanmaken en bewerken, niet publiceren |
| **Viewer** | Alleen lezen in het dashboard |

---

## 5. Datamodel

### Publiek schema (gedeeld)

```sql
public.tenants      — id, slug, name, plan, created_at
public.users        — id, email, password_hash, role, tenant_id (null voor SuperAdmin)
public.api_keys     — id, key_hash, label, tenant_id, expires_at
```

### Tenant schema (per klant)

```sql
tenant.content_types — id, name, slug, fields (JSONB)
tenant.entries       — id, content_type_id, data (JSONB), status, published_at, created_by, updated_at
tenant.media         — id, filename, url, mime_type, size, alt, created_by
tenant.settings      — key, value (JSONB)
```

### Content-types & velden

Content-types worden opgeslagen als schemadefinities in JSONB. Tenants definiëren zelf velden zonder database-migraties. Veldtypes:

- `text`, `richtext`, `number`, `boolean`
- `date`, `datetime`
- `media` (verwijzing naar `tenant.media`)
- `relation` (verwijzing naar ander content-type)
- `select` (vaste opties)

### Standaard content-types (bij aanmaken tenant)

- Pages, Posts, Products, Categories, Authors, Global Settings

---

## 6. Authenticatie

- **JWT-tokens** — bevatten `tenant_id`, `user_id`, `role`
- **Verlooptijd** — access token: 15 minuten, refresh token: 7 dagen
- **API-keys** — voor GraphQL delivery, per tenant, kunnen worden ingetrokken
- **Tenant-isolatie** — elke API-aanroep selecteert het juiste PostgreSQL-schema op basis van `tenant_id` uit het token

---

## 7. Media-opslag

- **Hetzner Object Storage** — S3-compatibel, Europese servers
- Bestanden worden geüpload via de REST API, opgeslagen in Object Storage
- Metadata (bestandsnaam, URL, MIME-type, grootte, alt-tekst) opgeslagen in `tenant.media`
- Elke tenant krijgt een eigen map/prefix in de bucket: `/{tenant_slug}/`

---

## 8. Admin UI

### Tech

- **NuxtJS** (SPA-modus) — communiceert via REST API
- **Eigen CSS** — geen UI-bibliotheek, volledig op maat

### Visuele stijl

- **Richting:** Clean White × Swiss Editorial
- **Primaire kleur:** Charcoal `#1a1a1a`
- **Achtergrond:** `#f5f5f0`
- **Wit vlak:** `#ffffff`
- **Randen:** `#dddddd`
- **Subtitels:** `#888888`
- Geen afgeronde hoeken, geen schaduwen
- Uppercase labels met letterspatiëring
- Sterke typografische hiërarchie

### Navigatiestructuur (sidebar)

De Content-sectie toont **dynamisch** de content-types van de actieve tenant. De Beheer-sectie is altijd vast.

```
Centaur
└── [Tenant naam]
    ├── Content
    │   └── [content-types van tenant, bijv. Pagina's, Posts, Producten]
    └── Beheer
        ├── Media
        ├── Gebruikers
        ├── API-sleutels
        └── Instellingen
```

---

## 9. Deployment

- **Hetzner VPS** — single server om te starten
- **ASP.NET Core API** — draait als service (systemd of Docker)
- **NuxtJS Admin** — gebuild als statische SPA, geserveerd via Nginx
- **PostgreSQL** — op dezelfde server of Hetzner Managed Database
- **Hetzner Object Storage** — extern, S3-compatibel

---

## 10. Buiten scope (v1)

- E-mail notificaties
- Webhooks bij publiceren
- Versiebeheer / content geschiedenis
- Volledige i18n / meertalige content
- Betaalsysteem / licentiemodel
- Automatische tenant-provisioning (eerst handmatig via SuperAdmin)
