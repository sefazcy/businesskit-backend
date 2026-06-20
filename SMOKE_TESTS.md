# Smoke Tests — BusinessKit Backend

Manual regression checklist. Run after any significant change or before
tagging a release. A fresh JWT is required for all admin checks — obtain one
via `POST /api/auth/login` with the default dev credentials below.

**Default dev credentials**
```
Email:    admin@businesskit.local
Password: Admin123!
```

---

## Configuration & Secrets

Notes moved here from `appsettings.json` (JSON does not support comments).

**JWT**
- `JwtSettings:SecretKey` in `appsettings.json` is a development-only placeholder.
- In production, override via environment variable: `JwtSettings__SecretKey=<real-secret>`
- Never commit a real secret key to source control.

**Email**
- `EmailSettings:Password` must be set via environment variable in production: `EmailSettings__Password=<smtp-password>`
- Keep `Enabled: false` in development — the app starts and works without any SMTP config.

**Iyzico sandbox credentials**
- Do not add real credentials to `appsettings.json` — this file is tracked in git.
- Supply `ApiKey` and `SecretKey` via one of:
  - `dotnet user-secrets set "Iyzico:ApiKey" "<your-key>"`
  - Environment variables: `Iyzico__ApiKey`, `Iyzico__SecretKey`
  - A local `appsettings.Local.json` (listed in `.gitignore`)

**Payment provider**
- `PaymentProvider:ActiveProvider` accepted values: `"Manual"`, `"Iyzico"`.
- An unsupported value causes checkout to fail clearly per-request — the app still starts.

---

## Health

- [ ] `GET /api/health` → 200, body contains `"status":"healthy"` and `"timestamp"`

---

## Auth

- [ ] `POST /api/auth/login` with valid credentials → 200, response contains `token`
- [ ] `POST /api/auth/login` with wrong password → 401
- [ ] `GET /api/auth/me` with valid Bearer token → 200, returns user + roles
- [ ] `GET /api/auth/me` without token → 401

---

## Business Settings

- [ ] `GET /api/business-settings` → 200 (or 404 if never set)
- [ ] `PUT /api/admin/business-settings` with admin token → 200, upserts settings
- [ ] `PUT /api/admin/business-settings` without token → 401

---

## Services

- [ ] `GET /api/services` → 200, returns only active services ordered by `displayOrder, id`
- [ ] `GET /api/services/{slug}` for a known active slug → 200
- [ ] `GET /api/services/{slug}` for an unknown or inactive slug → 404
- [ ] `GET /api/admin/services` with token → 200, returns all services including inactive
- [ ] `POST /api/admin/services` with valid body → 201
- [ ] `POST /api/admin/services` with a duplicate slug → 409
- [ ] `PUT /api/admin/services/{id}` with token → 200
- [ ] `PATCH /api/admin/services/{id}/toggle-active` with token → 200, flips `isActive`
- [ ] `GET /api/admin/services` without token → 401

---

## Contact Messages

- [ ] `POST /api/contact-messages` with valid body (no token needed) → 201
- [ ] `POST /api/contact-messages` with missing required field → 400
- [ ] `GET /api/admin/contact-messages` with token → 200, newest first
- [ ] `PATCH /api/admin/contact-messages/{id}/mark-read` with token → 200
- [ ] `PATCH /api/admin/contact-messages/{id}/mark-unread` with token → 200
- [ ] `PATCH /api/admin/contact-messages/{id}/mark-replied` with token → 200
- [ ] `PATCH /api/admin/contact-messages/{id}/archive` with token → 200
- [ ] `PATCH /api/admin/contact-messages/{id}/unarchive` with token → 200
- [ ] `GET /api/admin/contact-messages` without token → 401

---

## Gallery

- [ ] `GET /api/gallery` → 200, returns only active items
- [ ] `GET /api/gallery?category=X` → 200, filtered (case-insensitive)
- [ ] `GET /api/gallery/{id}` for a known active item → 200
- [ ] `GET /api/gallery/{id}` for an unknown id → 404
- [ ] `GET /api/admin/gallery` with token → 200, returns all items
- [ ] `POST /api/admin/gallery` with valid body → 201
- [ ] `PATCH /api/admin/gallery/{id}/toggle-active` with token → 200
- [ ] `GET /api/admin/gallery` without token → 401

---

## Blog

- [ ] `GET /api/blog` → 200, returns only published posts, ordered by `publishedAt desc, id desc`
- [ ] `GET /api/blog/{slug}` for a known published slug → 200
- [ ] `GET /api/blog/{slug}?language=en` resolves to the correct language variant
- [ ] `GET /api/blog/{slug}` for unknown slug → 404
- [ ] `GET /api/admin/blog` with token → 200, returns all posts
- [ ] `POST /api/admin/blog` with valid body → 201
- [ ] `POST /api/admin/blog` with same slug + same language → 409
- [ ] `POST /api/admin/blog` with same slug + different language → 201
- [ ] `PATCH /api/admin/blog/{id}/publish` with token → 200, sets `publishedAt` if null
- [ ] `PATCH /api/admin/blog/{id}/unpublish` with token → 200, does NOT clear `publishedAt`
- [ ] `GET /api/admin/blog` without token → 401

---

## Staff

- [ ] `GET /api/staff` → 200, returns only active staff ordered by `displayOrder, id`
- [ ] `GET /api/staff/{slug}` for a known active slug → 200
- [ ] `GET /api/staff/{slug}` for unknown slug → 404
- [ ] `GET /api/admin/staff` with token → 200, returns all staff
- [ ] `POST /api/admin/staff` with valid body → 201
- [ ] `POST /api/admin/staff` with a duplicate slug → 409
- [ ] `PUT /api/admin/staff/{id}` with token → 200
- [ ] `PATCH /api/admin/staff/{id}/toggle-active` with token → 200
- [ ] `GET /api/admin/staff` without token → 401

---

## Staff Working Hours

- [ ] `GET /api/admin/staff-working-hours` with token → 200
- [ ] `GET /api/admin/staff-working-hours?staffMemberId={id}` with token → 200, filtered
- [ ] `GET /api/admin/staff/{id}/working-hours` with token → 200
- [ ] `GET /api/admin/staff/{nonexistent}/working-hours` with token → 400
- [ ] `POST /api/admin/staff-working-hours` with valid body → 201
- [ ] `POST /api/admin/staff-working-hours` with duplicate staffMemberId+dayOfWeek → 409
- [ ] `PUT /api/admin/staff-working-hours/{id}` with token → 200
- [ ] `GET /api/admin/staff-working-hours` without token → 401

---

## Availability

- [ ] `GET /api/availability/slots?staffMemberId={id}&date={date}` → 200, slots respect 30-min default duration
- [ ] `GET /api/availability/slots?staffMemberId={id}&date={date}&businessServiceId={id}` → 200, slots respect service duration
- [ ] Last slot is only included if `slotStart + serviceDuration ≤ workEnd`
- [ ] Slots during break window are excluded (duration-based: `slotStart < breakEnd && slotEnd > breakStart`)
- [ ] Pending/Confirmed appointments block overlapping slots (duration-based overlap)
- [ ] Cancelled/Completed appointments do NOT block slots
- [ ] `GET /api/availability/slots?staffMemberId=9999&date={date}` → 400 (staff not found)
- [ ] `GET /api/availability/slots?staffMemberId={id}&date={date}&businessServiceId=9999` → 400 (service not found)

---

## Appointments (Public)

- [ ] `POST /api/appointments` with no `staffMemberId` → 201, bypasses availability check
- [ ] `POST /api/appointments` with valid `staffMemberId` and available slot → 201
- [ ] `POST /api/appointments` on a non-working day → 400
- [ ] `POST /api/appointments` outside working hours (duration end exceeds work end) → 400
- [ ] `POST /api/appointments` during break window (interval overlap) → 400
- [ ] `POST /api/appointments` at a time that overlaps a Pending/Confirmed appointment → 409
- [ ] `POST /api/appointments` with invalid `staffMemberId` → 400
- [ ] `POST /api/appointments` with invalid `businessServiceId` → 400
- [ ] Cancelled/Completed appointments do NOT block new bookings at the same time

---

## Appointments (Admin)

- [ ] `GET /api/admin/appointments` with token → 200
- [ ] `GET /api/admin/appointments?status=Pending` with token → 200, filtered
- [ ] `GET /api/admin/appointments?date={date}` with token → 200, filtered by single day
- [ ] `GET /api/admin/appointments?startDate={d1}&endDate={d2}` with token → 200, range
- [ ] `GET /api/admin/appointments?date={d}&startDate={d}` with token → 400 (ambiguous)
- [ ] `GET /api/admin/appointments/today` with token → 200
- [ ] `GET /api/admin/appointments/upcoming` with token → 200 (default 7 days)
- [ ] `GET /api/admin/appointments/upcoming?days=0` with token → 400
- [ ] `GET /api/admin/appointments/stats` with token → 200, totals by status
- [ ] `GET /api/admin/appointments/{id}` with token → 200 or 404
- [ ] `PATCH /api/admin/appointments/{id}/status` with valid status → 200
- [ ] `PATCH /api/admin/appointments/{id}/status` with invalid status → 400
- [ ] `PUT /api/admin/appointments/{id}` with valid body → 200
- [ ] `GET /api/admin/appointments` without token → 401

---

## Users & Roles (Admin)

- [ ] `GET /api/admin/users` with token → 200
- [ ] `GET /api/admin/users/{id}` with token → 200 or 404
- [ ] `POST /api/admin/users` with valid body → 201
- [ ] `POST /api/admin/users` with duplicate email → 409
- [ ] `POST /api/admin/users` with invalid role → 400
- [ ] `PUT /api/admin/users/{id}` with token → 200
- [ ] `PATCH /api/admin/users/{id}/toggle-active` with token → 200
- [ ] `GET /api/admin/roles` with token → 200
- [ ] `GET /api/admin/users` without token → 401

---

## Image Upload (Admin)

- [ ] `POST /api/admin/uploads/image` with a valid image file (JPG/PNG/WEBP ≤ 5 MB) → 200, returns URL
- [ ] `POST /api/admin/uploads/image` with no file → 400
- [ ] `POST /api/admin/uploads/image` with a file > 5 MB → 400
- [ ] `POST /api/admin/uploads/image` with a `.gif` or `.pdf` → 400
- [ ] `POST /api/admin/uploads/image` without token → 401

---

## Payments (v4.1 — Manual Admin Flow)

- [ ] `POST /api/admin/appointments/{id}/payments` with valid body (amount, currency) → 200, creates Pending payment
- [ ] `GET /api/admin/appointments/{id}/payments` with token → 200, returns list of payments for that appointment
- [ ] `GET /api/admin/payments` with token → 200, returns all payments newest-first
- [ ] `GET /api/admin/payments?status=Pending` with token → 200, filtered
- [ ] `GET /api/admin/payments?appointmentId={id}` with token → 200, filtered
- [ ] `GET /api/admin/payments/{id}` with token → 200 or 404
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` with token → 200, status becomes Paid, paidAt set
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` when already Paid → 400 (invalid transition)
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` with token → 200, status becomes Failed, failedAt set
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` when not Pending → 400 (invalid transition)
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` with token → 200, status becomes Refunded, refundedAt set
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` when not Paid → 400 (invalid transition)
- [ ] `GET /api/payments/{id}/status` (no token) → 200, returns only id / status / paidAt
- [ ] `GET /api/payments/{id}/status` (no token) — response does NOT include notes, failureReason, providerPaymentId, customerId
- [ ] `GET /api/admin/payments` without token → 401

---

## Payments (v4.5 — Public Checkout Prep)

### POST /api/payments/checkout — create checkout session

**Setup:** ensure appointment #N exists with a `BusinessServiceId` that has `Price > 0`.

- [ ] `POST /api/payments/checkout` with `{ "appointmentId": N }` → 200, response contains:
  - `paymentId` (integer)
  - `appointmentId` (integer, equals N)
  - `amount` (decimal, equals service price)
  - `currency` (string, matches BusinessSettings.Currency or "TRY" if none set)
  - `status` = `"Pending"`
  - `provider` = `"Manual"`
  - `checkoutUrl` = `"http://localhost:5174/payment-status/{paymentId}"`
  - `message` = `"Checkout session created. Awaiting payment."`

**Idempotency check:**

- [ ] Call `POST /api/payments/checkout` a second time with the same `appointmentId` → 200, same `paymentId` returned, `message` = `"Pending payment already exists for this appointment."`, no duplicate payment created
- [ ] Verify via `GET /api/admin/payments?appointmentId=N` that only one Pending payment record exists

**Error cases:**

- [ ] `POST /api/payments/checkout` with `{ "appointmentId": 0 }` → 400 (validation error — appointmentId must be ≥ 1)
- [ ] `POST /api/payments/checkout` with missing `appointmentId` field → 400 (ModelState validation)
- [ ] `POST /api/payments/checkout` with `{ "appointmentId": 99999 }` (non-existent appointment) → 404
- [ ] `POST /api/payments/checkout` for an appointment that has **no service** (BusinessServiceId is null) → 400 with message about missing service
- [ ] `POST /api/payments/checkout` for an appointment whose service has `Price = 0` → 400 with message about price

**Already-paid case:**

- [ ] Simulate the payment as Paid (via `PATCH /api/payments/{id}/simulate-paid` or admin mark-paid), then call `POST /api/payments/checkout` again for the same appointment → 400 with message `"Appointment already has a completed payment. No new checkout session can be created."`

---

### GET /api/payments/{id}/status — unchanged, verify still safe

- [ ] After creating a checkout session, `GET /api/payments/{paymentId}/status` → 200
- [ ] Response contains exactly: `id`, `status`, `paidAt` — **no** notes, failureReason, provider, customerId, checkoutUrl, or amount
- [ ] Unknown payment id → 404

---

### PATCH /api/payments/{id}/simulate-paid — dev simulation endpoint

- [ ] `PATCH /api/payments/{id}/simulate-paid` where payment is Pending → 200, returns full PaymentDto with status `"Paid"` and `paidAt` set
- [ ] Notification created: admin notifications list shows "Payment received" entry for this payment
- [ ] If appointment has a `CustomerEmail`, a payment confirmation email is attempted (check logs — non-critical if email not configured)
- [ ] `PATCH /api/payments/{id}/simulate-paid` where payment is already Paid → 400 with transition error message
- [ ] `PATCH /api/payments/{id}/simulate-paid` where payment id doesn't exist → 404

---

### Admin visibility

- [ ] After `POST /api/payments/checkout` creates a payment, `GET /api/admin/payments` (with token) shows the new Pending payment in the list
- [ ] `GET /api/admin/payments/{paymentId}` (with token) → 200, full PaymentDto including `providerCheckoutUrl` = `"http://localhost:5174/payment-status/{paymentId}"`
- [ ] Admin panel → Payments page shows the new payment with status badge "Pending"
- [ ] After `simulate-paid`, admin Payments page shows updated status badge "Paid" and paidAt timestamp

---

### Full end-to-end flow (Swagger walkthrough)

1. **Pick or create an appointment with a service:**
   - Use an existing appointment or `POST /api/appointments` with a valid `businessServiceId`
   - Note the `appointmentId`

2. **Create checkout session:**
   ```
   POST /api/payments/checkout
   Body: { "appointmentId": <id> }
   ```
   - Expect 200 with `paymentId`, `checkoutUrl`, `status: "Pending"`

3. **Verify idempotency:**
   ```
   POST /api/payments/checkout
   Body: { "appointmentId": <same id> }
   ```
   - Expect same `paymentId`, message confirms existing payment found

4. **Poll payment status (as PublicSite would):**
   ```
   GET /api/payments/{paymentId}/status
   ```
   - Expect `status: "Pending"`, `paidAt: null`

5. **Simulate payment success:**
   ```
   PATCH /api/payments/{paymentId}/simulate-paid
   ```
   - Expect 200 with `status: "Paid"`, `paidAt` set

6. **Poll status again:**
   ```
   GET /api/payments/{paymentId}/status
   ```
   - Expect `status: "Paid"`, `paidAt` is set

7. **Verify in admin:**
   ```
   GET /api/admin/payments?appointmentId=<id>   (with token)
   ```
   - Expect one payment record with status `Paid`

---

## Payments (v4.7 — simulate-paid environment gating)

### PATCH /api/payments/{id}/simulate-paid — Development only

**In Development environment (`ASPNETCORE_ENVIRONMENT=Development`):**

- [ ] `PATCH /api/payments/{id}/simulate-paid` for a Pending payment → 200, returns full PaymentDto with `status: "Paid"` and `paidAt` set
- [ ] `PATCH /api/payments/{id}/simulate-paid` for a Paid payment → 400 with transition error message (existing behavior)
- [ ] `PATCH /api/payments/{id}/simulate-paid` for a non-existent id → 404 with `{ message: "Payment with id X was not found." }`

**In non-Development environments (Staging, Production, etc.):**

- [ ] `PATCH /api/payments/{id}/simulate-paid` → 404 (plain, no body) — the endpoint appears not to exist
- [ ] The payment record is NOT modified (payment remains Pending) — verify via admin GET after the call
- [ ] No error details, internal state, or provider information are exposed

**Unchanged endpoints (must still work in all environments):**

- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200, checkout session created
- [ ] `GET /api/payments/{id}/status` → 200, returns `id`, `status`, `paidAt` only
- [ ] Both endpoints work with no `Authorization` header

---

## Business Settings (v4.9 — currency validation)

### PUT /api/admin/business-settings — currency field

**Valid currencies (case-insensitive input, stored as uppercase):**

- [ ] `PUT /api/admin/business-settings` with `"currency": "TRY"` (with full valid body) → 200, response `currency` = `"TRY"`
- [ ] `PUT /api/admin/business-settings` with `"currency": "USD"` → 200, response `currency` = `"USD"`
- [ ] `PUT /api/admin/business-settings` with `"currency": "EUR"` → 200, response `currency` = `"EUR"`
- [ ] `PUT /api/admin/business-settings` with `"currency": "GBP"` → 200, response `currency` = `"GBP"`
- [ ] `PUT /api/admin/business-settings` with `"currency": "try"` (lowercase) → 200, response `currency` = `"TRY"` (normalized)
- [ ] `PUT /api/admin/business-settings` with `"currency": "  usd  "` (with whitespace) → 200, response `currency` = `"USD"` (trimmed + normalized)

**Invalid currencies:**

- [ ] `PUT /api/admin/business-settings` with `"currency": "USDEWQ"` → 400, response body contains `"Currency must be one of: TRY, USD, EUR, GBP."`
- [ ] `PUT /api/admin/business-settings` with `"currency": "JPY"` → 400, same error message
- [ ] `PUT /api/admin/business-settings` with `"currency": ""` (empty string) → 400 (fails `[Required]` or `[AllowedCurrency]`)
- [ ] `PUT /api/admin/business-settings` without `currency` field → 400 (missing required field)
- [ ] No payment record is modified when settings update fails

**GET endpoint unchanged:**

- [ ] `GET /api/business-settings` → 200 (or 404 if never set), currency reflects the stored value
- [ ] `GET /api/admin/business-settings` with token → same

---

### Checkout — stale invalid currency in DB

If `BusinessSettings.Currency` contains an old invalid value (e.g., `"USDEWQ"` stored before v4.9):

- [ ] `POST /api/payments/checkout` with a valid `appointmentId` → 200, response `currency` = `"TRY"` (safe fallback applied)
- [ ] The checkout does NOT return an error due to the stale DB value
- [ ] After updating settings to `"EUR"` (valid), a new checkout for a new appointment returns `currency` = `"EUR"`

---

## Payments (v5.3 — Summary Statistics)

### Auth

- [ ] `GET /api/admin/payments/summary` without Bearer token → 401 Unauthorized
- [ ] `GET /api/admin/payments/summary` with valid admin Bearer token → 200 OK

### Response shape

- [ ] Response body contains top-level integer fields: `totalCount`, `pendingCount`, `paidCount`, `failedCount`, `refundedCount`
- [ ] Response body contains `totalsByCurrency` array
- [ ] Each entry in `totalsByCurrency` has: `currency`, `pendingAmount`, `paidAmount`, `failedAmount`, `refundedAmount`, `totalAmount`
- [ ] Counts add up correctly: `pendingCount + paidCount + failedCount + refundedCount ≤ totalCount` (remaining are Cancelled)
- [ ] `totalsByCurrency` is sorted alphabetically by currency

### Currency safety

- [ ] If any payment has a blank or invalid currency, the endpoint does not crash
- [ ] Such payments appear under currency `"UNKNOWN"` in `totalsByCurrency`

### Date filters (optional)

- [ ] `GET /api/admin/payments/summary?fromDate=2025-01-01` → returns only payments created on or after 2025-01-01
- [ ] `GET /api/admin/payments/summary?toDate=2025-12-31` → returns only payments created on or before 2025-12-31
- [ ] Both filters combined narrow the result correctly
- [ ] Omitting both filters returns stats for all payments

### No regressions

- [ ] `GET /api/admin/payments` still returns the full payment list
- [ ] `GET /api/admin/payments/{id}` still returns a single payment
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` still works
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` still works
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` still works
- [ ] `POST /api/payments/checkout` still creates a checkout session
- [ ] `GET /api/payments/{id}/status` still returns public payment status

---

---

## Payments (v5.6 — Provider Architecture Cleanup)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Provider identity

- [ ] `POST /api/payments/checkout` with a valid `appointmentId` → 200, response `provider` = `"Manual"`
- [ ] `GET /api/admin/payments/{paymentId}` → `provider` field = `"Manual"` (no other provider is active)
- [ ] No real API key, external credential, or third-party call is required

### Checkout flow — still works end-to-end

- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200, response contains `paymentId`, `checkoutUrl`, `status: "Pending"`
- [ ] `checkoutUrl` = `"http://localhost:5174/payment-status/{paymentId}"` (Manual provider sets this)
- [ ] Calling `POST /api/payments/checkout` again with the same `appointmentId` → 200, same `paymentId` returned, idempotency message present, no duplicate payment created
- [ ] `GET /api/payments/{paymentId}/status` → 200, returns `id`, `status: "Pending"`, `paidAt: null`

### simulate-paid still works

- [ ] `PATCH /api/payments/{paymentId}/simulate-paid` (Development only) → 200, `status: "Paid"`, `paidAt` set
- [ ] After simulate-paid: `GET /api/payments/{paymentId}/status` → `status: "Paid"`, `paidAt` set

### Admin payment actions still work

- [ ] `PATCH /api/admin/payments/{id}/mark-paid` with token → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` with token → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` with token → 200

### Payment summary still works

- [ ] `GET /api/admin/payments/summary` with token → 200, response contains `totalCount`, `pendingCount`, `paidCount`, `totalsByCurrency`

---

## Payments (v5.7 — Provider Config Infrastructure)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Default — ActiveProvider = "Manual"

Verify `appsettings.json` has `"PaymentProvider": { "ActiveProvider": "Manual" }` before running.

- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200, `provider: "Manual"`, `checkoutUrl` set
- [ ] Idempotency: second call with same `appointmentId` → 200, same `paymentId`, no duplicate
- [ ] `GET /api/payments/{id}/status` → 200, `status: "Pending"`
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `GET /api/admin/payments/summary` with token → 200, counts and `totalsByCurrency` present
- [ ] No external API call or credential required

### Unsupported provider — ActiveProvider = "Iyzico"

**Temporarily** edit `appsettings.json`: `"ActiveProvider": "Iyzico"`, restart the app, then:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Payment provider 'Iyzico' is not implemented yet."`
- [ ] `GET /api/payments/{id}/status` still returns 200 (no factory call for status reads)
- [ ] App starts and serves other endpoints normally (factory error is per-request, not startup)
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

### Unknown provider — ActiveProvider = "GhostPay"

**Temporarily** edit `appsettings.json`: `"ActiveProvider": "GhostPay"`, restart the app, then:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Payment provider 'GhostPay' is not supported."`
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

### Empty/null provider — falls back to Manual

**Temporarily** edit `appsettings.json`: `"ActiveProvider": ""`, restart, then:

- [ ] `POST /api/payments/checkout` → 200, `provider: "Manual"` (empty value defaults to Manual)
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

---

## Payments (v5.8 — Iyzico Sandbox Integration)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Secrets safety

- [ ] `appsettings.json` does NOT contain real Iyzico ApiKey or SecretKey — values are empty strings
- [ ] `appsettings.Local.json` is listed in `.gitignore` (can hold real sandbox creds locally without committing)
- [ ] `git status` shows no untracked secrets file

### ActiveProvider = "Manual" — unchanged behavior

- [ ] `POST /api/payments/checkout` → 200, `provider: "Manual"`, `checkoutUrl` set
- [ ] Idempotency: second call returns same `paymentId`
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `GET /api/admin/payments/summary` with token → 200
- [ ] Admin mark-paid, mark-failed, mark-refunded all return 200

### ActiveProvider = "Iyzico", no credentials (empty ApiKey/SecretKey)

**Temporarily** set `"ActiveProvider": "Iyzico"` in `appsettings.json`, leave `ApiKey` and `SecretKey` empty, restart, then:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Iyzico sandbox credentials are not configured."`
- [ ] App starts normally — other endpoints (`GET /api/health`, admin endpoints, etc.) still work
- [ ] `GET /api/payments/{id}/status` still returns 200 (no factory call for status reads)
- [ ] Provider does NOT fall back to Manual — checkout fails rather than silently using Manual
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

### Provider failure persistence and idempotency (v5.8 bug fix)

This test verifies that a failed provider attempt does not leave a broken Pending record that locks out future retries. Set `"ActiveProvider": "Iyzico"` with empty credentials, restart, then:

1. Use a fresh appointment that has no existing payment (or create a new booking).
2. `POST /api/payments/checkout` with that `appointmentId` → expect **400**, body `"Iyzico sandbox credentials are not configured."`
3. `GET /api/admin/payments?appointmentId={id}` with token → the payment record must have `status: "Failed"` and `failureReason: "Iyzico sandbox credentials are not configured."` — it must **not** have `status: "Pending"` or a non-null `checkoutUrl`
4. `POST /api/payments/checkout` again for the **same** `appointmentId` → expect **400** again with the same Iyzico credentials error — must **not** return `"Pending payment already exists for this appointment."`
5. `GET /api/admin/payments?appointmentId={id}` → only two `Failed` records (one per attempt) — no Pending record
6. **Restore** `"ActiveProvider": "Manual"` and restart
7. `POST /api/payments/checkout` for the same `appointmentId` → **200**, `provider: "Manual"`, `status: "Pending"`, `checkoutUrl` populated
8. Repeat checkout → **200** idempotency, same `paymentId` returned, `"Pending payment already exists"` message — confirms Manual idempotency is unaffected

### ActiveProvider = "Iyzico", credentials supplied

**Temporarily** supply sandbox credentials via user-secrets or `appsettings.Local.json`:
```
dotnet user-secrets set "Iyzico:ApiKey" "sandbox_key_here"
dotnet user-secrets set "Iyzico:SecretKey" "sandbox_secret_here"
```
Set `"ActiveProvider": "Iyzico"`, restart, then:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Iyzico sandbox checkout initialization is not yet implemented"` (real HTTP call is a v5.9/v6.0 TODO)
- [ ] No real payment is charged — this is expected skeleton behavior
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

### Unknown provider still fails clearly

- [ ] Temporarily set `"ActiveProvider": "GhostPay"` → `POST /api/payments/checkout` → 400, `"Payment provider 'GhostPay' is not supported."`
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

---

## Swagger

- [ ] `GET /swagger` loads and displays all endpoints grouped by tag
- [ ] Clicking "Authorize" and pasting a valid Bearer token allows protected endpoints to succeed
- [ ] `Payments (Public)` tag shows: `GET /api/payments/{id}/status`, `POST /api/payments/checkout`, `PATCH /api/payments/{id}/simulate-paid`
- [ ] Swagger summary for `simulate-paid` reads "[DEV ONLY — Development environment only]" and states it returns 404 in other environments
