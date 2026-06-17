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

## Swagger

- [ ] `GET /swagger` loads and displays all endpoints grouped by tag
- [ ] Clicking "Authorize" and pasting a valid Bearer token allows protected endpoints to succeed
