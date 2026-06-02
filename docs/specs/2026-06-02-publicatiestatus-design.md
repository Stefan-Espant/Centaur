# Publicatiestatus — Design Spec

**Datum:** 2026-06-02  
**Status:** Goedgekeurd

---

## Doel

Pagina's kunnen nu als concept worden opgeslagen voordat ze gepubliceerd worden. Redacteurs moeten bewust kiezen om een pagina te publiceren.

---

## Statussen

| Waarde | Betekenis |
|--------|-----------|
| `draft` | Concept — standaard bij aanmaken, niet zichtbaar via publieke API |
| `published` | Gepubliceerd — live |
| `archived` | Gearchiveerd — inactief, maar bewaard |

---

## Backend

### DTOs

`PageDto` krijgt een extra veld:

```csharp
public record PageDto(
    Guid Id,
    string Title,
    string Slug,
    string MetaDescription,
    JsonElement Body,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string Status  // "draft" | "published" | "archived"
);
```

`CreatePageRequest` en `UpdatePageRequest` krijgen beide ook een `string Status` parameter.

### Backward compatibility

Bestaande pagina's in de database hebben geen `status` veld in hun JSON. Bij deserialisatie via `System.Text.Json` krijgt `Status` de default waarde `null`. De `PageService` behandelt `null` als `"published"` — deze pagina's waren al live.

In `PageService`:
- `GetAllAsync` / `GetByIdAsync`: return pagina's as-is; consumers mogen null als published behandelen
- `CreateAsync`: forceert `Status = "draft"` ongeacht wat de request meegeeft
- `UpdateAsync`: neemt `Status` over uit de request; als request.Status null/leeg is, behoudt de bestaande pagina's status; als de bestaande status ook null is, wordt `"published"` gebruikt (backward compat)

### Normalisatie hulpfunctie

```csharp
private static string NormalizeStatus(string? requested, string? existing) =>
    requested is "draft" or "published" or "archived" ? requested
    : existing is "draft" or "published" or "archived" ? existing
    : "published";  // legacy pagina's zonder status zijn al live
```

`CreateAsync` gebruikt altijd `"draft"` direct, zonder deze functie.

### Geen DB-migratie nodig

Pagina's worden als JSON opgeslagen in de `settings` tabel. Het toevoegen van een veld aan het record is volledig backward compatible.

---

## Frontend

### 1. Paginalijst (`pages/index.vue`)

- Filterbalk bovenaan: **Alle** / **Concept** / **Gepubliceerd** / **Gearchiveerd**
- Elke rij toont een gekleurde badge naast de paginatitel:
  - `draft` → grijs badge ("Concept")
  - `published` → groen badge ("Gepubliceerd")
  - `archived` → rood/oranje badge ("Gearchiveerd")
- Filter werkt client-side (alle pagina's worden al opgehaald)

### 2. Nieuwe pagina (`pages/new.vue`)

- Status hoeft niet zichtbaar te zijn in het formulier
- Backend forceert `draft`; frontend stuurt altijd `status: "draft"` mee in de request

### 3. Pagina bewerken (`pages/[id].vue`)

Statusknop in de header naast "Wijzigingen opslaan":

| Huidige status | Knop | Actie |
|----------------|------|-------|
| `draft` | Groen primaire knop: **Publiceren** | Status → `published`, direct opslaan |
| `published` | Ghost knop: **Depubliceren** | Status → `draft`, direct opslaan |
| `archived` | Ghost knop: **Herstellen** | Status → `draft`, direct opslaan |

"Direct opslaan" betekent: `updatePage()` aanroepen met de nieuwe status (geen aparte endpoint nodig).

---

## API

Geen nieuwe endpoints nodig. Status is een gewoon veld in de bestaande `PUT /api/pages/{id}` payload.

De bestaande `GET /api/pages` retourneert alle pagina's inclusief status — filtering op status is verantwoordelijkheid van de frontend-consumer van de tenant.

---

## Wat dit NIET doet (bewust uitgesloten)

- Geplande publicatie (datum + tijd) — apart feature
- Statusfilter op de publieke API — dat is de verantwoordelijkheid van de Nuxt/Next frontend die de API consumeert
- Goedkeuringsworkflow (editor → publisher) — apart feature

---

## Betrokken bestanden

**Backend:**
- `src/Centaur.Application/DTOs/PageDto.cs`
- `src/Centaur.Application/DTOs/CreatePageRequest.cs`
- `src/Centaur.Application/DTOs/UpdatePageRequest.cs`
- `src/Centaur.Application/Services/PageService.cs`
- `tests/Centaur.Application.Tests/Services/PageServiceTests.cs`
- `tests/Centaur.Application.Tests/Helpers/MockPageRepository.cs`

**Frontend:**
- `frontend/composables/usePages.ts`
- `frontend/pages/pages/index.vue`
- `frontend/pages/pages/new.vue`
- `frontend/pages/pages/[id].vue`
