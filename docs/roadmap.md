# Centaur CMS — Roadmap

Overzicht van wat er nog toegevoegd kan worden, gerangschikt op impact en complexiteit.

---

## ✅ Gebouwd

- Multi-tenancy (tenants, gebruikers, API-sleutels)
- Authenticatie (JWT)
- Pagebuilder met bloktypen en velden
- Dashboard met statistieken en activiteitsgrafiek
- Instellingenpagina (SEO, huisstijl, social, technisch)
- Dark mode (Midnight Forest)

---

## 🔴 Hoog — Essentieel voor echt gebruik

### Mediabeheer
Afbeeldingen en bestanden uploaden, opslaan en koppelen aan content.
- Upload-endpoint (multipart/form-data)
- Opslag: lokaal (filesystem) of S3-compatibel
- Media library UI (rasterweergave, zoeken)
- Media-veld in de pagebuilder werkt nog niet
- Favicon en Open Graph afbeelding in Instellingen

### Publicatiestatus
Pagina's kunnen nu niet als concept worden opgeslagen.
- Status: `draft` / `published` / `archived`
- Badge op de paginalijst
- Toggle op de bewerkpagina
- API retourneert alleen gepubliceerde pagina's bij publieke aanvragen
- Geplande publicatie (datum + tijd instellen)

---

## 🟠 Gemiddeld — Maakt het professioneel

### Navigatiebeheer
Sitenavigatie samenstellen als boomstructuur.
- Menu-items aanmaken (label, link, volgorde)
- Koppelen aan bestaande pagina's of externe URLs
- Meerdere menus per site (hoofd, footer)
- Exposed via API

### Collecties / Contenttypen
Gestructureerde content naast pagina's (blogposts, cases, producten).
- Contenttype definiëren met eigen velden (zoals bloktypen)
- Entries aanmaken, bewerken, verwijderen
- Lijst- en detailweergave in CMS
- Exposed via API per type

### Contentversies
Vorige versies van een pagina bekijken en herstellen.
- Versie opslaan bij elke opslag
- Versiegeschiedenis weergeven (datum, gebruiker)
- Rollback naar eerdere versie

### Webhooks
Externe systemen notificeren bij content-wijzigingen.
- Webhook URL instellen per event (pagina gepubliceerd, bijgewerkt, verwijderd)
- Retry-mechanisme bij mislukte aanroepen
- Log van recente webhook-aanroepen

---

## 🟡 Lager — Handige uitbreidingen

### Formulieren + inzendingen
Contactformulieren aanmaken en inzendingen opslaan.
- Formuliervelden definiëren (naam, email, textarea, etc.)
- Inzendingen opslaan in de database
- Overzicht van inzendingen in het CMS
- E-mailnotificatie bij nieuwe inzending

### Meertaligheid
Vertalingen per veld instellen.
- Talen configureren per tenant
- Per veld een vertaling opgeven
- API retourneert content op basis van `Accept-Language` header

### Audit log
Bijhouden wie wat wanneer heeft gewijzigd.
- Logboek per tenant: gebruiker, actie, tijdstip, object
- Overzicht in het CMS

### Geavanceerde rollen
Meer dan alleen Admin en SuperAdmin.
- Rollen definiëren: Editor, Auteur, Lezer
- Rechten per rol instellen (lezen/schrijven/publiceren)
- Gebruikerstoegang per collectie of sectie beperken

### Zoeken
Volledige zoekopdrachten door content.
- Zoekbalk in CMS (pagina's, entries)
- API-endpoint voor frontend zoekfunctionaliteit

---

## 🔵 Later — Geavanceerd

### Aangepaste domeinen
Elke tenant koppelen aan een eigen domein.

### Analyticsintegratie
Paginaweergaven direct in het CMS tonen (via GA4 Data API).

### AI-assistentie
Tekst genereren of verbeteren op basis van een veldwaarde (via Claude API).

### Exporteren / importeren
Content exporteren als JSON of CSV, en importeren vanuit andere systemen.

---

## Aanbevolen volgorde

1. **Publicatiestatus** — klein, directe waarde, één sessie
2. **Mediabeheer** — essentieel, groter werk
3. **Navigatiebeheer** — klein, hoge dagelijkse waarde
4. **Collecties** — groot, opent veel mogelijkheden
5. **Webhooks** — nodig voor SSG-frontends (Next.js, Nuxt static)
