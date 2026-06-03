
<p align="center">
  <img src="docs/logo.svg" alt="Centaur CMS" height="60" />
</p>

Door: Stefan van der Kort

---

## Features

- **Multi-tenancy** — elke tenant krijgt een eigen geïsoleerd PostgreSQL-schema
- **Pagebuilder** — pagina's opbouwen met herbruikbare blokken en drag-and-drop
- **Bloktypen** — zelf blokken definiëren met eigen veldstructuur
- **Publicatiestatus** — concept, gepubliceerd en gearchiveerd per pagina
- **Instellingen** — SEO, huisstijl, social media en onderhoudsmodus per tenant
- **API-sleutels** — externe toegang voor frontend-applicaties
- **Dark mode** — volledig themabaar admin UI

---

## Tech stack

| Laag | Technologie |
|------|-------------|
| Backend | .NET 8, Clean Architecture |
| Database | PostgreSQL, Entity Framework Core |
| Frontend | Nuxt 3, Vue 3, TypeScript |
| Auth | JWT (BCrypt) |
| Tests | xUnit, Moq |

---

## Aan de slag

### Vereisten

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org)
- [PostgreSQL 15+](https://www.postgresql.org)

### 1. Omgevingsvariabelen instellen

Maak een `.env` bestand aan in de root, of stel de variabelen in via je omgeving:

```env
ConnectionStrings__Default=Host=localhost;Database=centaur;Username=postgres;Password=jouwwachtwoord
Jwt__Secret=minimaal-32-tekens-lang-geheim
SuperAdmin__Email=admin@example.com
SuperAdmin__Password=JouwWachtwoord123!
```

> Nooit wachtwoorden of secrets committen naar git.

### 2. Backend starten

```bash
dotnet run --project src/Centaur.Api
```

De API draait op `http://localhost:5000`. Bij de eerste opstart worden de database-migraties automatisch uitgevoerd en een SuperAdmin-account aangemaakt.

### 3. Frontend starten

```bash
cd frontend
npm install
npm run dev
```

De admin UI is beschikbaar op `http://localhost:3000`.

---

## Projectstructuur

```
src/
  Centaur.Domain/          # Entiteiten, enums, value objects
  Centaur.Application/     # Services, interfaces, DTOs
  Centaur.Infrastructure/  # Repositories, database, EF Core
  Centaur.Api/             # Controllers, middleware, Program.cs

frontend/
  pages/                   # Nuxt pagina's (dashboard, pagina's, bloktypen, ...)
  components/              # Vue componenten (sidebar, FieldBuilder, ...)
  composables/             # API-wrappers en state (usePages, useBlockTypes, ...)
  assets/css/              # Globale stijlen en thema-variabelen

tests/
  Centaur.Application.Tests/  # Unit tests (xUnit + Moq)
```

---

## Tests uitvoeren

```bash
dotnet test tests/Centaur.Application.Tests/
```

---

## API

De REST API is beschikbaar op `/api/`. Authenticatie via JWT Bearer token.

| Endpoint | Beschrijving |
|----------|-------------|
| `POST /api/auth/login` | Inloggen, ontvangt JWT |
| `GET /api/pages` | Alle pagina's van de tenant |
| `POST /api/pages` | Nieuwe pagina aanmaken |
| `PUT /api/pages/{id}` | Pagina bijwerken (inclusief status) |
| `GET /api/block-types` | Alle bloktypen |
| `GET /api/website` | Website-instellingen |

---

## Roadmap

Zie [docs/roadmap.md](docs/roadmap.md) voor gepland werk.

---

## Licentie

Privé project — [Semantique Agency](https://semantique.nl)
