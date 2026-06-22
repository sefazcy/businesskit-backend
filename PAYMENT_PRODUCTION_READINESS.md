# Payment Production Readiness

This document describes what is required before enabling payment processing in
production, covers the current refund strategy, and provides a sandbox-to-live
checklist.

---

## Current Payment Providers

| Provider | Status |
|---|---|
| Manual | Fully implemented. Admin marks payments paid / failed / refunded manually. |
| Iyzico | Sandbox integration implemented and smoke-tested. Live mode is NOT enabled by default. |

`PaymentProvider:ActiveProvider` in `appsettings.json` controls which provider is
active. Accepted values: `"Manual"`, `"Iyzico"`. The committed default is `"Manual"`.

---

## Production Prerequisites

Before switching `ActiveProvider` to `"Iyzico"` in any production or staging
environment, all of the following must be in place.

### 1. HTTPS backend URL
The backend must be served over HTTPS.
Iyzico does not accept plain-HTTP callback URLs in production.

### 2. Stable public callback URL
The callback must be reachable from the internet without a proxy or tunnel.
Ngrok is acceptable in local development **only** — do not use it in production.

Required format:
```
https://your-domain.com/api/payments/iyzico/callback
```

### 3. Real domain (no ngrok)
Replace `your-domain.com` with the actual production domain. The domain must be
stable — changing the callback URL after checkout sessions are created breaks
in-flight payments.

### 4. Iyzico live merchant account
- Register and complete KYC verification with Iyzico for the live environment.
- Obtain live `ApiKey` and `SecretKey` from the Iyzico merchant dashboard.
- Update `Iyzico:BaseUrl` to the live API URL (`https://api.iyzipay.com`).

### 5. Credentials in environment variables or secure secrets only
Never commit real credentials to source control. Supply them via:

**Option A — environment variables (recommended for production):**
```
Iyzico__ApiKey=<live-key>
Iyzico__SecretKey=<live-secret>
Iyzico__CallbackUrl=https://your-domain.com/api/payments/iyzico/callback
PaymentProvider__ActiveProvider=Iyzico
JwtSettings__SecretKey=<strong-random-jwt-secret>
```

**Option B — dotnet user-secrets (local development only):**
```
cd BusinessKit.Api
dotnet user-secrets set "Iyzico:ApiKey"      "<sandbox-key>"
dotnet user-secrets set "Iyzico:SecretKey"   "<sandbox-secret>"
dotnet user-secrets set "Iyzico:CallbackUrl" "https://<ngrok>.ngrok-free.app/api/payments/iyzico/callback"
dotnet user-secrets set "PaymentProvider:ActiveProvider" "Iyzico"
```

**Option C — appsettings.Local.json (gitignored, development only):**
```json
{
  "PaymentProvider": { "ActiveProvider": "Iyzico" },
  "Iyzico": {
    "ApiKey":       "<sandbox-key>",
    "SecretKey":    "<sandbox-secret>",
    "CallbackUrl":  "https://<ngrok>.ngrok-free.app/api/payments/iyzico/callback"
  }
}
```

---

## What Must NEVER Be Committed

- `Iyzico:ApiKey`
- `Iyzico:SecretKey`
- `JwtSettings:SecretKey` (the value in `appsettings.json` is a dev-only placeholder)
- `appsettings.Local.json`
- Any file containing real API credentials or production secrets

---

## Backend Config Safety Guards

Guards are enforced at request-time (per checkout attempt), not at startup. The
app always starts — misconfig surfaces clearly on the first checkout.

| Scenario | Environment | Behavior |
|---|---|---|
| `ActiveProvider=Iyzico`, missing `ApiKey`/`SecretKey` | any | Checkout fails: `"Iyzico sandbox credentials are not configured."` |
| `ActiveProvider=Iyzico`, missing `CallbackUrl` | any | Checkout fails: `"Iyzico CallbackUrl is not configured."` |
| `ActiveProvider=Iyzico`, `CallbackUrl` contains `localhost` | non-Development | Checkout fails: `"Iyzico CallbackUrl cannot use localhost in non-Development environments."` |
| `ActiveProvider=Iyzico`, `CallbackUrl` is HTTP (not HTTPS) | non-Development | Checkout fails: `"Iyzico CallbackUrl must use HTTPS in non-Development environments."` |
| `ActiveProvider=Iyzico`, all valid | any | Checkout initializes normally |
| `ActiveProvider=Manual` | any | Manual flow always works — no external calls |

In **Development** environment (`ASPNETCORE_ENVIRONMENT=Development`), `localhost`
and ngrok callback URLs are allowed without restriction.

---

## Sandbox vs Live Checklist

### Sandbox (current state — verified)
- [x] `Iyzico:BaseUrl` = `https://sandbox-api.iyzipay.com`
- [x] `appsettings.json` has empty `ApiKey`, `SecretKey`, `CallbackUrl`
- [x] Sandbox end-to-end flow verified with ngrok callback
- [x] `PaymentProvider:ActiveProvider` defaults to `"Manual"` in committed config
- [x] `appsettings.Local.json` is gitignored

### Before switching to live
- [ ] Iyzico live merchant account approved and KYC complete
- [ ] Update `Iyzico:BaseUrl` to `https://api.iyzipay.com`
- [ ] Set live credentials via environment variables only (never committed)
- [ ] Backend deployed with HTTPS
- [ ] Public callback URL confirmed: `https://your-domain.com/api/payments/iyzico/callback`
- [ ] Callback URL registered in Iyzico merchant dashboard (if required)
- [ ] Run full manual smoke test with a real low-value transaction (see below)
- [ ] Confirm payment callback reaches the backend (check application logs)
- [ ] Confirm payment status changes Pending → Paid
- [ ] Confirm admin panel shows Iyzico Paid with correct provider reference ID
- [ ] Confirm public payment status page shows "Payment completed"
- [ ] Confirm admin cannot manually mark-paid or mark-failed for Iyzico payments

---

## Manual Smoke Test Checklist (Before Going Live)

1. Deploy backend to staging with HTTPS and real public callback URL
2. Set `ActiveProvider=Iyzico` with live credentials via environment variables
3. Create a test booking through the public site
4. Verify `POST /api/payments/checkout` returns a live Iyzico payment page URL
5. Complete payment on the Iyzico payment page using a small test amount
6. Verify the Iyzico callback reaches the backend (check application logs)
7. Verify payment status changes from Pending to Paid
8. Verify admin panel shows payment as Paid with `providerPaymentId` (Iyzico token)
9. Verify public payment status page shows "Payment completed"
10. Attempt `PATCH /api/admin/payments/{id}/mark-paid` for the Iyzico payment → expect 400
11. Attempt `PATCH /api/admin/payments/{id}/mark-refunded` → expect 400 with refund note
12. Verify admin detail page shows the refund strategy message for Iyzico Paid payments

---

## Refund Strategy

### Manual provider
- Admin can mark a Manual Paid payment as Refunded via the admin panel.
- This records the refund in the local database only.
- No external payment processor is contacted.

### Iyzico provider — real refund API not implemented
- `PATCH /api/admin/payments/{id}/mark-refunded` is **blocked** for Iyzico payments.
- The exact error returned: `"Iyzico refund is not implemented yet. Process refunds through the Iyzico merchant dashboard."`
- The admin payment detail page shows this note for all Iyzico Paid payments.
- **To refund an Iyzico payment:** log in to the Iyzico merchant dashboard and issue the refund there manually.

### Future Iyzico refund implementation
When implementing the real Iyzico refund API:
1. Use the Iyzipay SDK `CreateRefundRequest` with the payment's `providerPaymentId`
2. Record the Iyzico refund response (refund transaction ID, amount, status)
3. Mark the payment as Refunded only after the provider confirms success
4. Never allow manual admin status changes for provider-managed refunds

---

## Rollback Plan

If Iyzico payments fail or the integration must be disabled:

1. Set environment variable: `PaymentProvider__ActiveProvider=Manual`
2. Restart the backend
3. Manual provider takes effect immediately for all new checkouts
4. Existing Iyzico Pending payments remain Pending — check the Iyzico merchant
   dashboard for their payment state and contact affected customers manually
5. No database migration required

---

## Known Limitations (MVP)

- SQLite is used as the database. Migrate to PostgreSQL or SQL Server before
  production deployments with concurrent write load.
- No automatic retry for failed Iyzico callbacks.
- Iyzico callback is verified by re-querying the Iyzico API, not by HMAC signature
  on the incoming POST body.
- Iyzico refund API is not implemented.
- `simulate-paid` endpoint is Development-only and returns 404 in all other environments.
