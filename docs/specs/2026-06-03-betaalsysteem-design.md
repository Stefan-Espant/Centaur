# Betaalsysteem Design

**Doel:** Tenants kunnen zich gratis registreren en upgraden naar een betaald Pro-plan via Stripe.

**Architectuur:** Stripe-native — subscription status wordt opgeslagen in onze eigen database en gesynchroniseerd via Stripe webhooks. Geen eigen Plan-tabel; plannen worden beheerd in het Stripe dashboard.

**Tech stack:** Stripe .NET SDK, Stripe Checkout, Stripe Customer Portal, Stripe webhooks

---

## Data model

De `Tenant` entiteit krijgt drie nieuwe kolommen:

| Kolom | Type | Default | Beschrijving |
|---|---|---|---|
| `stripe_customer_id` | `text` nullable | `null` | Aangemaakt zodra een tenant naar Stripe Checkout gaat |
| `stripe_subscription_id` | `text` nullable | `null` | Gevuld na succesvolle betaling via webhook |
| `subscription_status` | `text` | `"free"` | `"free"` / `"active"` / `"past_due"` / `"canceled"` |

Limieten zijn hardcoded in de applicatielaag:

```csharp
public static class PlanLimits
{
    public static int MaxApiKeys(string status) => status == "active" ? int.MaxValue : 1;
    public static int MaxUsers(string status)   => status == "active" ? int.MaxValue : 1;
}
```

Limieten worden **alleen in de backend** afgedwongen (in `ApiKeyService` en `UserService`), nooit uitsluitend in de frontend.

---

## Signup flow

### Gratis registratie

Nieuwe tenants registreren via de publieke pagina `/register` (buiten de beveiligde admin).

**Invoer:** tenantnaam, naam, e-mail, wachtwoord

**Backend (`POST /api/auth/register`):**
1. Valideer invoer (uniek e-mail, unieke tenant slug)
2. Maak `Tenant` aan met `subscription_status = "free"`
3. Maak `User` aan met rol `TenantAdmin`, gekoppeld aan de tenant
4. Retourneer JWT

### Upgraden naar Pro

Vanuit de admin (`/billing`):

1. Gebruiker klikt "Upgrade naar Pro"
2. Frontend roept `POST /api/billing/checkout` aan
3. Backend:
   - Maakt Stripe Customer aan (of hergebruikt bestaande `stripe_customer_id`)
   - Slaat `stripe_customer_id` op bij tenant
   - Maakt Stripe Checkout Session aan met:
     - `price`: de Pro Price ID uit `appsettings`
     - `customer`: de Stripe Customer ID
     - `metadata.tenant_id`: de tenant GUID
     - `success_url`: `/billing/success`
     - `cancel_url`: `/billing`
4. Backend retourneert de Checkout URL
5. Frontend redirect naar Stripe

---

## Stripe webhooks

Endpoint: `POST /api/billing/webhook` (niet achter JWT-authenticatie, wél handtekeningverificatie)

### Handtekeningverificatie

```csharp
var stripeEvent = EventUtility.ConstructEvent(
    await new StreamReader(Request.Body).ReadToEndAsync(),
    Request.Headers["Stripe-Signature"],
    _options.WebhookSecret
);
```

Bij verificatiefout: retourneer `400 Bad Request`.

### Verwerkte events

| Event | Actie |
|---|---|
| `checkout.session.completed` | Zoek tenant op via `metadata.tenant_id`, stel `stripe_subscription_id` en `subscription_status = "active"` in |
| `customer.subscription.updated` | Update `subscription_status` naar Stripe-status (`active` / `past_due` / etc.) |
| `customer.subscription.deleted` | Stel `subscription_status = "canceled"` in |

Niet-herkende events worden genegeerd (retourneer `200 OK`).

### Past due gedrag

Bij `past_due`: tenant behoudt Pro-toegang. Admin toont een banner. Stripe herprobeert automatisch. Als betaling slaagt gaat de status terug naar `active` via `customer.subscription.updated`.

Bij `canceled`: tenant valt terug op free-limieten. Bestaande data blijft intact.

---

## Billing UI (`/billing`)

Toegankelijk voor `TenantAdmin`-rol.

### Free plan

```
Plan: Gratis
──────────────────────────────
✓ 1 gebruiker
✓ 1 API-sleutel

[Upgrade naar Pro]
```

### Pro plan — actief

```
Plan: Pro  ● Actief
──────────────────────────────
Volgende factuur: {datum}

[Abonnement beheren]
```

"Abonnement beheren" opent de **Stripe Customer Portal** (kaartgegevens, facturen, opzeggen). Backend genereert een portal-sessie via `POST /api/billing/portal`.

`GET /api/billing/status` haalt de `current_period_end` op van de Stripe subscription (via Stripe API call) en retourneert die als ISO-datumstring. De frontend formatteert die als leesbare datum.

### Pro plan — opgezegd (canceled)

Wanneer `subscription_status = "canceled"` toont de UI dezelfde weergave als het free plan, aangevuld met een melding:

```
Plan: Gratis  (abonnement opgezegd)
──────────────────────────────
✓ 1 gebruiker
✓ 1 API-sleutel

[Opnieuw upgraden naar Pro]
```

### Pro plan — past due

```
⚠ Betaling mislukt — Stripe herprobeert automatisch.

[Betalingsgegevens bijwerken]
```

"Betalingsgegevens bijwerken" opent hetzelfde Stripe Customer Portal.

---

## Nieuwe API-endpoints

| Methode | Pad | Beschrijving |
|---|---|---|
| `POST` | `/api/auth/register` | Publiek — tenant + gebruiker aanmaken |
| `POST` | `/api/billing/checkout` | JWT — Stripe Checkout sessie aanmaken |
| `POST` | `/api/billing/portal` | JWT — Stripe Customer Portal sessie aanmaken |
| `POST` | `/api/billing/webhook` | Stripe — webhook events verwerken |
| `GET` | `/api/billing/status` | JWT — huidige subscription status ophalen |

---

## Configuratie

In `appsettings.json` (waarden via omgevingsvariabelen, nooit hardcoded):

```json
"Stripe": {
  "SecretKey": "",
  "WebhookSecret": "",
  "ProPriceId": ""
}
```

`SecretKey` en `WebhookSecret` zijn geheimen en mogen **nooit** worden gecommit naar git.

---

## Veiligheid

- Webhook-handtekening altijd verifiëren vóór verwerking
- Stripe Secret Key alleen in backend, nooit in frontend of gecommit
- Limieten afgedwongen in de applicatielaag (backend), niet uitsluitend in de UI
- `stripe_customer_id` en `stripe_subscription_id` zijn geen secrets — veilig om op te slaan
