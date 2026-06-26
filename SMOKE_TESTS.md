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

## Payments (v5.9 — Iyzico Callback Infrastructure)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### POST /api/payments/iyzico/callback — missing token

- [ ] `POST /api/payments/iyzico/callback` with body `{}` (no token field) → 400, body contains `"Token is required."`
- [ ] `POST /api/payments/iyzico/callback` with body `{ "token": "" }` (empty token) → 400, body contains `"Token is required."`
- [ ] `POST /api/payments/iyzico/callback` with body `{ "token": "   " }` (whitespace-only token) → 400, body contains `"Token is required."`
- [ ] No payment record is modified by any of the above calls

### POST /api/payments/iyzico/callback — sample token (controlled not-implemented response)

- [ ] `POST /api/payments/iyzico/callback` with body `{ "token": "sample-token" }` → 200, response body contains:
  - `isVerified: false`
  - `message: "Iyzico callback verification is not implemented yet."`
- [ ] `POST /api/payments/iyzico/callback` with body `{ "token": "sample-token", "paymentId": 123 }` → 200, same response shape
- [ ] No payment record is modified — verify via `GET /api/admin/payments` that no status changed to `"Paid"`
- [ ] No authorization header is required (endpoint is public, as Iyzico POSTs from its servers)

### Safety: callback does not mark payment Paid

- [ ] Create a Pending payment via `POST /api/payments/checkout` with a valid `appointmentId`
- [ ] Note the `paymentId` returned
- [ ] Call `POST /api/payments/iyzico/callback` with `{ "token": "any-token", "paymentId": <id> }` → 200, `isVerified: false`
- [ ] `GET /api/admin/payments/{paymentId}` with token → status is still `"Pending"`, NOT `"Paid"`
- [ ] `GET /api/payments/{paymentId}/status` → `status: "Pending"`, `paidAt: null`

### No regressions

- [ ] Manual checkout still works: `POST /api/payments/checkout` with valid `appointmentId` → 200, `provider: "Manual"`
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `GET /api/admin/payments/summary` with token → 200, counts and `totalsByCurrency` present
- [ ] `PATCH /api/admin/payments/{id}/mark-paid`, `mark-failed`, `mark-refunded` all still return 200

### Swagger

- [ ] `GET /swagger` → `Payments (Public)` tag shows `POST /api/payments/iyzico/callback`
- [ ] Swagger summary for the callback endpoint is visible and accurate
- [ ] Endpoint appears as public (no lock icon required)

---

## Payments (v6.0 — Iyzico Sandbox Checkout Initialization)

### Package

- [ ] `BusinessKit.Infrastructure.csproj` references `Iyzipay` version `2.1.78` (owner: iyzico)
- [ ] No other Iyzipay-related packages added

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Secrets safety

- [ ] `appsettings.json` still has empty `Iyzico:ApiKey`, `Iyzico:SecretKey`, `Iyzico:CallbackUrl`
- [ ] `appsettings.Local.json` is NOT committed (`git status` shows it as untracked if present locally, or absent)
- [ ] `appsettings.Local.json` appears in `.gitignore`
- [ ] `PaymentProvider:ActiveProvider` in committed `appsettings.json` is `"Manual"`

### Local secret setup (run once per developer machine)

Supply sandbox credentials using **one** of these methods:

**Option A — dotnet user-secrets (recommended):**
```
cd BusinessKit.Api
dotnet user-secrets set "Iyzico:ApiKey"      "<your-sandbox-key>"
dotnet user-secrets set "Iyzico:SecretKey"   "<your-sandbox-secret>"
dotnet user-secrets set "Iyzico:CallbackUrl" "http://localhost:5000/api/payments/iyzico/callback"
dotnet user-secrets set "PaymentProvider:ActiveProvider" "Iyzico"
```

**Option B — appsettings.Local.json (gitignored):**
Create `BusinessKit.Api/appsettings.Local.json`:
```json
{
  "PaymentProvider": { "ActiveProvider": "Iyzico" },
  "Iyzico": {
    "ApiKey":       "<your-sandbox-key>",
    "SecretKey":    "<your-sandbox-secret>",
    "CallbackUrl":  "http://localhost:5000/api/payments/iyzico/callback"
  }
}
```

**Option C — environment variables:**
```
Iyzico__ApiKey=<key>
Iyzico__SecretKey=<secret>
Iyzico__CallbackUrl=http://localhost:5000/api/payments/iyzico/callback
PaymentProvider__ActiveProvider=Iyzico
```

### ActiveProvider = "Manual" — no regressions

- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200, `provider: "Manual"`, `checkoutUrl` set
- [ ] Idempotency: second call returns same `paymentId`, message `"Pending payment already exists..."`
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `GET /api/admin/payments/summary` with token → 200
- [ ] Admin mark-paid, mark-failed, mark-refunded still return 200

### ActiveProvider = "Iyzico", no credentials

Set `"ActiveProvider": "Iyzico"`, leave `ApiKey`/`SecretKey`/`CallbackUrl` empty, restart:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Iyzico sandbox credentials are not configured."`
- [ ] No payment left in `Pending` status — `GET /api/admin/payments?appointmentId={id}` shows `"Failed"` record
- [ ] Other endpoints unaffected

### ActiveProvider = "Iyzico", credentials set, CallbackUrl missing

Set credentials but leave `CallbackUrl` empty:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Iyzico CallbackUrl is not configured"`
- [ ] No payment left in `Pending` status — record shows `"Failed"`

### ActiveProvider = "Iyzico", full credentials + CallbackUrl (real sandbox call)

Supply all three values and restart:

- [ ] `POST /api/payments/checkout` with valid `appointmentId` (service with `Price > 0`) → 200, response contains:
  - `provider: "Iyzico"`
  - `status: "Pending"`
  - `checkoutUrl` — non-empty Iyzico sandbox payment page URL (begins with `https://sandbox-api.iyzipay.com` or similar)
  - `paymentId` (integer)
- [ ] `GET /api/admin/payments/{paymentId}` with token → `providerPaymentId` is non-empty (the Iyzico checkout token), `providerCheckoutUrl` matches the `checkoutUrl` above
- [ ] Navigating to the `checkoutUrl` in a browser shows the Iyzico sandbox payment page
- [ ] **Restore** `"ActiveProvider": "Manual"` after this test

### Iyzico failure response (invalid credentials)

Set `ApiKey` / `SecretKey` to non-empty garbage values (`"bad"` / `"key"`), set a valid `CallbackUrl`:

- [ ] `POST /api/payments/checkout` → 400, body contains `"Iyzico checkout initialization failed:"` followed by the error message from Iyzico
- [ ] Payment record has `status: "Failed"` and a non-empty `failureReason`
- [ ] **Restore** correct credentials or revert to `"Manual"` after this test

### Callback endpoint — no regressions

- [ ] `POST /api/payments/iyzico/callback` with empty token → 400, `"Token is required."`
- [ ] `POST /api/payments/iyzico/callback` with sample token → 200, `isVerified: false`, `"not implemented yet"`
- [ ] No payment is marked `Paid` by the callback endpoint

### Provider failure persistence

- [ ] After a failed Iyzico attempt, the payment record has `status: "Failed"`, not `"Pending"`
- [ ] Retrying `POST /api/payments/checkout` for the same `appointmentId` with corrected credentials creates a new attempt — the previous `"Failed"` record does NOT block it

---

## Swagger

- [ ] `GET /swagger` loads and displays all endpoints grouped by tag
- [ ] Clicking "Authorize" and pasting a valid Bearer token allows protected endpoints to succeed
- [ ] `Payments (Public)` tag shows: `GET /api/payments/{id}/status`, `POST /api/payments/checkout`, `PATCH /api/payments/{id}/simulate-paid`, `POST /api/payments/iyzico/callback`
- [ ] Swagger summary for `simulate-paid` reads "[DEV ONLY — Development environment only]" and states it returns 404 in other environments

---

## Payments (v6.1 — Iyzico Callback Verification and Status Update)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Secrets and config safety

- [ ] `appsettings.json` still has empty `Iyzico:ApiKey`, `Iyzico:SecretKey`, `Iyzico:CallbackUrl`
- [ ] `PaymentProvider:ActiveProvider` in committed `appsettings.json` is `"Manual"`
- [ ] `appsettings.Local.json` is NOT committed

### POST /api/payments/iyzico/callback — token validation

- [ ] `POST /api/payments/iyzico/callback` with JSON body `{}` → 400, `"Token is required."`
- [ ] `POST /api/payments/iyzico/callback` with form body (no token field) → 400, `"Token is required."`
- [ ] `POST /api/payments/iyzico/callback` with JSON `{ "token": "  " }` (whitespace) → 400, `"Token is required."`

### POST /api/payments/iyzico/callback — unknown token

- [ ] `POST /api/payments/iyzico/callback` with JSON `{ "token": "unknown-token-abc" }` → 200, response:
  ```json
  { "isVerified": false, "message": "No payment found for the provided token.", "paymentId": null, "status": null }
  ```
- [ ] No payment record is modified

### POST /api/payments/iyzico/callback — form-encoded (Iyzico's real format)

- [ ] `POST /api/payments/iyzico/callback` with `Content-Type: application/x-www-form-urlencoded`, body `token=unknown-token` → 200, `isVerified: false`, `"No payment found..."`
- [ ] Use curl or Postman to test form encoding (Swagger sends JSON)
  ```
  curl -X POST http://localhost:5000/api/payments/iyzico/callback \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "token=unknown-token"
  ```

### POST /api/payments/iyzico/callback — known pending token, unverified with Iyzico

With `ActiveProvider=Iyzico` and credentials set:

1. Create a booking and call `POST /api/payments/checkout` → get `paymentId` and `checkoutUrl`
2. Note the Iyzico checkout `token` stored as `providerPaymentId` (`GET /api/admin/payments/{paymentId}`)
3. Call `POST /api/payments/iyzico/callback` with `{ "token": "<that-token>" }` **without completing payment in browser**
4. Expected: 200, `isVerified: false` — Iyzico will return PaymentStatus other than SUCCESS since no payment was made
5. `GET /api/payments/{paymentId}/status` → status still `"Pending"` (not changed)

### POST /api/payments/iyzico/callback — successful sandbox payment (full ngrok flow)

**Prerequisites:** sandbox credentials set in user-secrets, ngrok installed

**Setup steps:**

```
# Terminal 1 — start backend
cd BusinessKit.Api
dotnet run

# Terminal 2 — expose localhost via ngrok (default port 5299 or check launchSettings.json)
ngrok http 5299
```

Copy the ngrok HTTPS URL (e.g. `https://abc123.ngrok-free.app`), then:

```
dotnet user-secrets set "Iyzico:CallbackUrl" "https://abc123.ngrok-free.app/api/payments/iyzico/callback"
dotnet user-secrets set "PaymentProvider:ActiveProvider" "Iyzico"
```

Restart the backend (credentials are re-read on startup).

**End-to-end flow:**

1. Create appointment and call `POST /api/payments/checkout` → get `checkoutUrl`
2. Note `paymentId` from response
3. Open `checkoutUrl` in browser — Iyzico sandbox payment page loads
4. Pay with Iyzico sandbox test card:
   - Card no: `5528790000000008`
   - Expiry: `12/30`
   - CVV: `123`
5. After payment, Iyzico POSTs `token=xxx` (form-encoded) to your ngrok URL
6. Backend verifies with `CheckoutForm.Retrieve` → if `PaymentStatus == "SUCCESS"` → marks payment `Paid`
7. Check results:
   - `GET /api/payments/{paymentId}/status` → `status: "Paid"`, `paidAt` set
   - `GET /api/admin/payments/{paymentId}` with token → full record shows `Paid`
   - Admin notifications list shows "Payment received" entry
   - If `CustomerEmail` was set on the appointment, check logs for email send attempt

**Restore after test:**
```
dotnet user-secrets set "PaymentProvider:ActiveProvider" "Manual"
```

### Safety: callback never marks Paid without provider verification

- [ ] Callback with unknown token → 200, `isVerified: false`, no DB change
- [ ] Callback with valid token for already-Paid payment → 200, `isVerified: true`, message `"already ... Paid"`, status unchanged
- [ ] Callback with valid token for already-Failed payment → 200, `isVerified: false`, message contains `"terminal status"`, status unchanged
- [ ] No endpoint accepts a client-supplied `paymentId` to mark a payment Paid

### No regressions

- [ ] `ActiveProvider=Manual` → `POST /api/payments/checkout` → 200, `provider: "Manual"`, `checkoutUrl` set
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `GET /api/admin/payments/summary` with token → 200, counts and `totalsByCurrency` present
- [ ] Admin mark-paid, mark-failed, mark-refunded still return 200 for Manual payments
- [ ] `GET /api/payments/{id}/status` still returns only `id`, `status`, `paidAt`

---

## Payments (v6.2 — Admin Safety and UX Cleanup)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Provider-aware backend guards — Manual payments (must still work)

- [ ] Create a Manual Pending payment (via admin create or `POST /api/payments/checkout` with Manual active)
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` with token → 200, `status: "Paid"`
- [ ] Create another Manual Pending payment
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` with token → 200, `status: "Failed"`
- [ ] Take any Manual Paid payment
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` with token → 200, `status: "Refunded"`

### Provider-aware backend guards — Iyzico payments (must be blocked)

**Setup:** set `ActiveProvider = "Iyzico"` with credentials, create a checkout (payment will be Iyzico + Pending).

- [ ] `PATCH /api/admin/payments/{id}/mark-paid` for the Iyzico Pending payment → **400**, body contains `"Iyzico payments can only be marked Paid after provider callback verification."`
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` for the Iyzico Pending payment → **400**, body contains `"Iyzico payments cannot be manually marked as Failed."`
- [ ] Simulate a Paid Iyzico payment via the callback flow (or use `simulate-paid` in dev)
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` for the Iyzico Paid payment → **400**, body contains `"Iyzico refund is not implemented yet."`
- [ ] Iyzico callback verification still marks Pending → Paid correctly (unaffected by guards)

### Regression checks

- [ ] `GET /api/admin/payments/summary` with token → 200
- [ ] `GET /api/admin/payments` with token → 200
- [ ] `POST /api/payments/checkout` (Manual) → 200, `provider: "Manual"`
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `POST /api/payments/iyzico/callback` with unknown token → 200, `isVerified: false`

---

### v6.2 Regression Report — Iyzico admin guard not in committed code

**Date caught:** 2026-06-22
**Payment ID tested:** 17 (Iyzico, status was Paid before test)

**Request:**
```
PATCH /api/admin/payments/17/mark-refunded
Body: { "notes": "string" }
```

**Expected:** 400 Bad Request
```json
{ "message": "Iyzico refund is not implemented yet. Process refunds through the Iyzico merchant dashboard." }
```

**Actual:** 200 OK — payment status changed to `"Refunded"`, `refundedAt` set.

**Root cause:** The provider guards in `MarkPaidAsync`, `MarkFailedAsync`, and `MarkRefundedAsync`
(in `PaymentService.cs`) were added as local working-tree changes but **never committed** in v6.1.
The running backend binary had no provider checks at all — every payment regardless of provider
could be manually transitioned to any status.

**Fix (v6.2):** All three guards committed. Backend now blocks Iyzico payments in all three
admin mark-* endpoints before any status transition is attempted.

**Guard messages (exact):**
- `mark-paid` Iyzico → `"Iyzico payments can only be marked Paid after provider callback verification."`
- `mark-failed` Iyzico → `"Iyzico payments cannot be manually marked as Failed."`
- `mark-refunded` Iyzico → `"Iyzico refund is not implemented yet. Process refunds through the Iyzico merchant dashboard."`

**Note for re-testing:** Payment #17 is now in terminal `Refunded` state. Use a new Iyzico
Pending payment (create checkout with `ActiveProvider=Iyzico`) or an existing Iyzico Paid
payment to verify the guards. The `simulate-paid` endpoint (Development) can fast-path an
Iyzico Pending payment to Paid for the `mark-refunded` guard test.

---

## Payments (v6.5 — Production Readiness and Refund Strategy)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Secrets and config safety (no regression)

- [ ] `appsettings.json` still has empty `Iyzico:ApiKey`, `Iyzico:SecretKey`, `Iyzico:CallbackUrl`
- [ ] `PaymentProvider:ActiveProvider` in committed `appsettings.json` is `"Manual"`
- [ ] `appsettings.Local.json` is NOT committed (`git status` shows it absent or untracked)
- [ ] `PAYMENT_PRODUCTION_READINESS.md` exists in the repository root

### ActiveProvider = "Manual" — no regression

- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200, `provider: "Manual"`, `checkoutUrl` set
- [ ] `PATCH /api/payments/{id}/simulate-paid` (Development) → 200, `status: "Paid"`
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` Manual Pending → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` Manual Pending → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` Manual Paid → 200

### ActiveProvider = "Iyzico", missing ApiKey/SecretKey

Set `"ActiveProvider": "Iyzico"`, leave `ApiKey` and `SecretKey` empty, restart:

- [ ] `POST /api/payments/checkout` → 400, body: `"Iyzico sandbox credentials are not configured."`
- [ ] App starts normally — `GET /api/health` still returns 200
- [ ] Restore `"ActiveProvider": "Manual"` after this test

### ActiveProvider = "Iyzico", localhost CallbackUrl, Production/Staging environment

This test requires running the app with `ASPNETCORE_ENVIRONMENT=Production` (or Staging).

Set credentials and a localhost CallbackUrl, set environment to Production, restart:

- [ ] `POST /api/payments/checkout` → 400, body: `"Iyzico CallbackUrl cannot use localhost in non-Development environments. Set a public HTTPS callback URL."`
- [ ] Restore environment and config after this test

### ActiveProvider = "Iyzico", HTTP CallbackUrl, Production/Staging environment

Set credentials and an `http://` (non-HTTPS) CallbackUrl, environment to Production, restart:

- [ ] `POST /api/payments/checkout` → 400, body: `"Iyzico CallbackUrl must use HTTPS in non-Development environments."`
- [ ] Restore environment and config after this test

### ActiveProvider = "Iyzico", localhost CallbackUrl, Development environment (must be allowed)

With `ASPNETCORE_ENVIRONMENT=Development` (the default), credentials set, localhost CallbackUrl:

- [ ] `POST /api/payments/checkout` does NOT fail with the localhost error — proceeds to Iyzico API call
- [ ] Restore config after this test

### Iyzico admin guard — mark-refunded blocked

With an Iyzico Paid payment (via real callback or `simulate-paid`):

- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` → 400, body: `"Iyzico refund is not implemented yet. Process refunds through the Iyzico merchant dashboard."`
- [ ] Payment status remains Paid — `GET /api/admin/payments/{id}` confirms no status change

### Iyzico admin guard — mark-paid and mark-failed blocked

- [ ] `PATCH /api/admin/payments/{id}/mark-paid` for an Iyzico Pending payment → 400, body: `"Iyzico payments can only be marked Paid after provider callback verification."`
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` for an Iyzico Pending payment → 400, body: `"Iyzico payments cannot be manually marked as Failed."`

### Iyzico checkout still works in Development with ngrok callback

Full end-to-end (requires sandbox credentials and ngrok — optional, run when ngrok available):

- [ ] Set credentials + ngrok CallbackUrl, `ActiveProvider=Iyzico`
- [ ] `POST /api/payments/checkout` → 200, `provider: "Iyzico"`, `checkoutUrl` is a real Iyzico sandbox URL
- [ ] Iyzico callback verification still marks payment Paid correctly
- [ ] Restore `"ActiveProvider": "Manual"` after this test

---

## Payments (v6.4 — Admin payment detail and audit visibility)

### Backend DTO — no changes required

`GET /api/admin/payments/{id}` (`PaymentDto`) already exposes all fields needed by the detail
page: `id`, `appointmentId`, `customerId`, `amount`, `currency`, `status`, `provider`,
`providerPaymentId`, `providerCheckoutUrl`, `paidAt`, `failedAt`, `refundedAt`,
`failureReason`, `notes`, `createdAt`, `updatedAt`. No backend code was changed in this sprint.

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### GET /api/admin/payments/{id} — field completeness

- [ ] Response includes `providerPaymentId` (Iyzico token, or null for Manual)
- [ ] Response includes `providerCheckoutUrl` (checkout URL, or null)
- [ ] Response includes `failedAt` and `refundedAt` (null unless payment reached that state)
- [ ] Response includes `createdAt` and `updatedAt`
- [ ] Response does **not** include API secrets or Iyzico credentials

### Existing endpoints — no regressions

- [ ] `GET /api/admin/payments` → 200, list unaffected
- [ ] `GET /api/admin/payments/{id}` → 200 for known id, 404 for unknown

---

## Products (v7.0 — Product / Inventory Foundation)

### Build

- [ ] `dotnet build --configuration Release` completes with 0 errors and 0 warnings

### Auth guard

- [ ] `GET /api/admin/products` without token → 401
- [ ] `POST /api/admin/products` without token → 401

### GET /api/admin/products — list

- [ ] `GET /api/admin/products` with token → 200, returns array (empty if none created yet)
- [ ] `GET /api/admin/products?isActive=true` → 200, returns only active products
- [ ] `GET /api/admin/products?isActive=false` → 200, returns only inactive products
- [ ] `GET /api/admin/products?search=coffee` → 200, returns products matching name/sku/category
- [ ] `GET /api/admin/products?category=Drinks` → 200, filtered by category substring
- [ ] `GET /api/admin/products?lowStockOnly=true` → 200, returns only products where `minStock > 0 && currentStock <= minStock`
- [ ] `GET /api/admin/products?take=5` → 200, returns at most 5 products
- [ ] Response items include: `id`, `name`, `sku`, `category`, `unit`, `currentStock`, `minStock`, `costPrice`, `salePrice`, `isActive`, `isLowStock`, `notes`, `createdAt`, `updatedAt`

### POST /api/admin/products — create

- [ ] `POST /api/admin/products` with valid minimal body `{ "name": "Espresso", "unit": "cup" }` → 201, response contains product with `id`, `isActive: true`, `currentStock: 0`
- [ ] `POST /api/admin/products` with full body (name, sku, category, unit, currentStock, minStock, costPrice, salePrice, isActive, notes) → 201, all fields reflected in response
- [ ] `POST /api/admin/products` with `name: ""` (empty) → 400
- [ ] `POST /api/admin/products` with missing `name` field → 400
- [ ] `POST /api/admin/products` with missing `unit` field → 400
- [ ] `POST /api/admin/products` with `currentStock: -1` → 400, error mentions "CurrentStock cannot be negative"
- [ ] `POST /api/admin/products` with `minStock: -5` → 400, error mentions "MinStock cannot be negative"
- [ ] `POST /api/admin/products` with `costPrice: -10` → 400, error mentions "CostPrice cannot be negative"
- [ ] `POST /api/admin/products` with `salePrice: -0.01` → 400, error mentions "SalePrice cannot be negative"
- [ ] `POST /api/admin/products` with a duplicate SKU → 409, body contains `"already exists"`

### GET /api/admin/products/{id}

- [ ] `GET /api/admin/products/{id}` for a created product → 200, full ProductDto
- [ ] `GET /api/admin/products/99999` → 404, body contains `"was not found"`

### PUT /api/admin/products/{id} — update

- [ ] `PUT /api/admin/products/{id}` with updated name, category, prices → 200, response reflects changes
- [ ] `PUT /api/admin/products/{id}` with empty name → 400
- [ ] `PUT /api/admin/products/{id}` with negative stock → 400
- [ ] `PUT /api/admin/products/99999` → 404
- [ ] `PUT /api/admin/products/{id}` changing SKU to one already used by another product → 409
- [ ] `UpdatedAt` is newer than `CreatedAt` after a successful update; `CreatedAt` is unchanged

### PATCH /api/admin/products/{id}/toggle-active

- [ ] `PATCH /api/admin/products/{id}/toggle-active` on an active product → 200, `isActive: false`
- [ ] `PATCH /api/admin/products/{id}/toggle-active` again → 200, `isActive: true`
- [ ] `PATCH /api/admin/products/99999/toggle-active` → 404

### GET /api/admin/products/categories

- [ ] `GET /api/admin/products/categories` with token → 200, returns sorted array of distinct non-null category strings
- [ ] After creating products with categories "Drinks", "Food", "Drinks" → returns `["Drinks", "Food"]` (deduplicated, sorted)
- [ ] If no products have a category set → returns empty array

### isLowStock computed field

- [ ] Create product with `currentStock: 5`, `minStock: 10` → response has `isLowStock: true`
- [ ] Create product with `currentStock: 10`, `minStock: 10` → response has `isLowStock: true` (equal is low)
- [ ] Create product with `currentStock: 11`, `minStock: 10` → response has `isLowStock: false`
- [ ] Create product with `currentStock: 0`, `minStock: 0` → response has `isLowStock: false` (minStock = 0 means threshold not set)

### No regressions

- [ ] `GET /api/admin/appointments` with token → 200 (appointments unaffected)
- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200, `provider: "Manual"` (payments unaffected)
- [ ] `GET /api/admin/customers` with token → 200 (customers unaffected)

- [ ] `GET /api/admin/payments/summary` → 200, stats unaffected
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` Manual Pending → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-paid` Iyzico → 400 with guard message
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` Manual Pending → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-failed` Iyzico → 400 with guard message
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` Manual Paid → 200
- [ ] `PATCH /api/admin/payments/{id}/mark-refunded` Iyzico → 400 with guard message
- [ ] `POST /api/payments/iyzico/callback` with unknown token → 200, `isVerified: false`

---

## Stock Movements (v7.2)

### Auth guard

- [ ] `GET /api/admin/stock-movements` without token → 401
- [ ] `POST /api/admin/stock-movements` without token → 401
- [ ] `GET /api/admin/products/{id}/stock-movements` without token → 401
- [ ] `GET /api/admin/products/{id}/stock-summary` without token → 401

### POST /api/admin/stock-movements — In movement

- [ ] `POST /api/admin/stock-movements` with `{ "productId": {id}, "type": "In", "quantity": 10 }` → 200, response has `type: "In"`, `quantity: 10`, `previousStock: <old>`, `newStock: previousStock + 10`
- [ ] Product `currentStock` in `GET /api/admin/products/{id}` reflects the new stock after the movement
- [ ] `POST` In with `quantity: 0` → 400, error mentions "greater than 0"
- [ ] `POST` In with `quantity: -5` → 400 (DTO Range validation)

### POST /api/admin/stock-movements — Out movement

- [ ] `POST /api/admin/stock-movements` with `{ "productId": {id}, "type": "Out", "quantity": 5 }` when currentStock >= 5 → 200, `newStock = previousStock - 5`
- [ ] `POST` Out where `quantity` would make stock negative → 400, error mentions "Insufficient stock"
- [ ] `POST` Out with `quantity: 0` → 400, error mentions "greater than 0"

### POST /api/admin/stock-movements — Adjustment movement

- [ ] `POST /api/admin/stock-movements` with `{ "productId": {id}, "type": "Adjustment", "quantity": 25 }` → 200, `newStock: 25` regardless of previous stock
- [ ] Product `currentStock` is now 25 after adjustment
- [ ] `POST` Adjustment with `quantity: 0` → 200, stock is set to 0 (zero adjustment is valid)
- [ ] `POST` Adjustment with `quantity: -1` → 400, error mentions "cannot be negative"

### POST /api/admin/stock-movements — validation

- [ ] `POST` with missing `productId` → 400
- [ ] `POST` with `productId: 99999` (non-existent product) → 404, body contains "was not found"
- [ ] `POST` with missing `type` → 400
- [ ] `POST` with `type: "Invalid"` → 400, error mentions "not a valid stock movement type"
- [ ] `POST` with optional `reason` (max 150 chars) and `notes` (max 1000 chars) → 200, fields reflected in response
- [ ] `POST` with `reason` exceeding 150 chars → 400
- [ ] `POST` with `notes` exceeding 1000 chars → 400

### POST atomicity

- [ ] After a successful movement, both `StockMovement` record exists and `Product.currentStock` is updated in the same request — verify by checking `GET /api/admin/products/{id}` immediately after `POST /api/admin/stock-movements`
- [ ] `Product.updatedAt` is newer than before the movement

### GET /api/admin/stock-movements

- [ ] `GET /api/admin/stock-movements` with token → 200, returns array, latest movement first
- [ ] `GET /api/admin/stock-movements?productId={id}` → 200, only movements for that product
- [ ] `GET /api/admin/stock-movements?type=In` → 200, only In movements
- [ ] `GET /api/admin/stock-movements?type=Out` → 200, only Out movements
- [ ] `GET /api/admin/stock-movements?type=Adjustment` → 200, only Adjustment movements
- [ ] `GET /api/admin/stock-movements?dateFrom=2026-01-01&dateTo=2026-12-31` → 200, movements within range
- [ ] `GET /api/admin/stock-movements?take=2` → 200, at most 2 results
- [ ] Each movement DTO contains `id`, `productId`, `productName`, `productSku`, `type`, `quantity`, `previousStock`, `newStock`, `reason`, `notes`, `createdAt`

### GET /api/admin/products/{productId}/stock-movements

- [ ] `GET /api/admin/products/{productId}/stock-movements` → 200, only that product's movements, latest first
- [ ] For a product with no movements → 200, empty array

### GET /api/admin/products/{id}/stock-summary

- [ ] `GET /api/admin/products/{id}/stock-summary` → 200, contains `productId`, `productName`, `currentStock`, `minStock`, `isLowStock`, `totalIn`, `totalOut`, `adjustmentCount`, `lastMovementAt`
- [ ] `totalIn` equals sum of all In movement quantities for that product
- [ ] `totalOut` equals sum of all Out movement quantities for that product
- [ ] `adjustmentCount` equals number of Adjustment movements
- [ ] `isLowStock` matches `minStock > 0 && currentStock <= minStock`
- [ ] `GET /api/admin/products/99999/stock-summary` → 404

### isLowStock reflects updated stock

- [ ] Create product with `currentStock: 15`, `minStock: 10` → `isLowStock: false`
- [ ] `POST` Out movement of 6 → `currentStock` becomes 9, `GET /api/admin/products/{id}` shows `isLowStock: true`
- [ ] `POST` Adjustment to 20 → `currentStock` becomes 20, `isLowStock: false`

### No regressions

- [ ] `GET /api/admin/products` with token → 200 (existing product endpoints unaffected)
- [ ] `POST /api/admin/products` → 201 (product creation unaffected)
- [ ] `PUT /api/admin/products/{id}` → 200 (direct stock update via product update still works)
- [ ] `GET /api/admin/appointments` with token → 200 (appointments unaffected)
- [ ] `POST /api/payments/checkout` with valid `appointmentId` → 200 (payments unaffected)
- [ ] `GET /api/admin/customers` with token → 200 (customers unaffected)

---

## v7.5 — Inventory Final Polish and Readiness

> Final quality, safety, and readiness pass for the complete Product / Inventory module.
> Run after v7.5 tag against a live backend at `http://localhost:5299`.

### Build check

- [ ] `dotnet build --configuration Release` → Build succeeded, 0 Warning(s), 0 Error(s)

### Product management smoke checks

- [ ] `GET /api/admin/products` with valid token → 200, returns product array
- [ ] `POST /api/admin/products` with valid body → 201, returns created product with `id`, `isLowStock`, `createdAt`, `updatedAt`
- [ ] `PUT /api/admin/products/{id}` with valid body → 200, returns updated product
- [ ] `PATCH /api/admin/products/{id}/toggle-active` → 200, `isActive` flips
- [ ] `GET /api/admin/products/categories` → 200, returns sorted string array
- [ ] `GET /api/admin/products/{id}` for unknown id → 404 with `{ message: "..." }`
- [ ] `POST /api/admin/products` with duplicate SKU → 409 with `{ message: "A product with SKU '...' already exists." }`
- [ ] `POST /api/admin/products` without `name` → 400 (model validation)
- [ ] `POST /api/admin/products` without token → 401
- [ ] `POST /api/admin/products` with non-admin token → 403

### Stock movement smoke checks

- [ ] `POST /api/admin/stock-movements` (In, qty 10) → **201** (was 200 before v7.5), returns movement with `previousStock`, `newStock`
- [ ] `POST /api/admin/stock-movements` (Out, qty within stock) → 201, `newStock = previousStock - qty`
- [ ] `POST /api/admin/stock-movements` (Out, qty exceeds stock) → 400 with `"Insufficient stock"` message
- [ ] `POST /api/admin/stock-movements` (Adjustment, qty 0) → 201, `newStock = 0`
- [ ] `POST /api/admin/stock-movements` (Adjustment, qty 50) → 201, `newStock = 50`
- [ ] `POST /api/admin/stock-movements` with invalid type `"Remove"` → 400 with valid type list in message
- [ ] `POST /api/admin/stock-movements` with In/Out and `quantity: 0` → 400 ("must be greater than 0")
- [ ] `POST /api/admin/stock-movements` with negative `quantity` → 400 (model validation)
- [ ] `POST /api/admin/stock-movements` for unknown productId → 404
- [ ] `GET /api/admin/products/{id}` after stock movement → `currentStock` and `updatedAt` reflect the change
- [ ] `GET /api/admin/products/{id}/stock-movements` → 200, array, latest first
- [ ] `GET /api/admin/products/{id}/stock-summary` → 200, contains `totalIn`, `totalOut`, `adjustmentCount`, `lastMovementAt`
- [ ] `GET /api/admin/stock-movements?take=5` → 200, at most 5 results
- [ ] `GET /api/admin/stock-movements?productId={id}` → filtered to that product only
- [ ] `GET /api/admin/stock-movements` without token → 401

### Safety invariants

- [ ] Product `currentStock` cannot go negative via Out movement (400 returned, stock unchanged)
- [ ] Adjustment to 0 is accepted and sets `currentStock` to exactly 0
- [ ] Stock movement records are never deleted when their product still exists (FK Restrict)
- [ ] `product.updatedAt` changes after every stock movement that modifies stock

### No regressions

- [ ] `GET /api/admin/appointments` with token → 200
- [ ] `GET /api/admin/customers` with token → 200
- [ ] `POST /api/payments/checkout` flow unaffected
- [ ] `GET /api/admin/staff` with token → 200
- [ ] `GET /api/admin/blog/posts` with token → 200

### Known limitations (not bugs)

- No inventory export / reporting endpoint yet
- No automatic stock deduction from appointments or payments yet
- Product images not supported yet
- No supplier / vendor module yet
- No barcode / SKU scanning yet
- No multi-warehouse / location support yet
- `PUT /api/admin/products/{id}` allows direct `currentStock` override (no movement record created); intended for initial setup and bulk corrections
- `GET /api/admin/products/{id}/stock-movements` is capped at 500 rows server-side; products with more history should use the global `/api/admin/stock-movements?productId={id}` endpoint with explicit `take` and date filters
