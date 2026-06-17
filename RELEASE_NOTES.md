# BusinessKit Backend — v2.0 MVP Release Notes

## Overview

BusinessKit Backend is a modular ASP.NET Core Web API backend for small and
medium businesses (cafes, barbers, clinics, gyms, consultants, agencies, and
similar). It provides a clean, versioned REST API covering the core needs of
a business website — authentication, settings, a service catalog, a blog, a
gallery, staff profiles, contact handling, and a basic appointment scheduling
engine — behind a consistent `/api/…` and `/api/admin/…` route structure.

**v2.0 is the first complete, usable MVP release.** It is suitable for local
development, demos, and small deployments. It is not yet a production-grade
SaaS platform — see [Not Included in v2.0](#not-included-in-v20) below.

---

## What Is Included in v2.0

### Core Infrastructure

- **.NET 8** / ASP.NET Core Web API, layered architecture (Domain →
  Application → Infrastructure → Api → Shared)
- **SQLite** via Entity Framework Core 8 (8 migrations, schema stable since
  v1.4)
- **JWT Bearer authentication** with BCrypt password hashing
- **Swagger / OpenAPI** UI with Bearer auth support, endpoints grouped by tag
- Global error handling — consistent `{ "message": "…" }` responses for
  400 / 404 / 409 errors

### Modules

| Module | Public API | Admin API |
|---|---|---|
| Health | `GET /api/health` | — |
| Auth | `POST /api/auth/login`, `GET /api/auth/me` | — |
| Users & Roles | — | `/api/admin/users`, `/api/admin/roles` |
| Business Settings | `GET /api/business-settings` | `PUT /api/admin/business-settings` |
| Service Catalog | `GET /api/services`, `GET /api/services/{slug}` | `/api/admin/services` (CRUD + toggle-active) |
| Contact Messages | `POST /api/contact-messages` | `/api/admin/contact-messages` (workflow: read/replied/archive) |
| Gallery | `GET /api/gallery` | `/api/admin/gallery` (CRUD + toggle-active) |
| Blog | `GET /api/blog`, `GET /api/blog/{slug}` | `/api/admin/blog` (CRUD + publish/unpublish, multi-language) |
| Staff | `GET /api/staff`, `GET /api/staff/{slug}` | `/api/admin/staff` (CRUD + toggle-active) |
| Image Upload | — | `POST /api/admin/uploads/image` (JPG/PNG/WEBP ≤ 5 MB, served from `wwwroot`) |
| Appointments | `POST /api/appointments` | `/api/admin/appointments` (list/filter/update/stats/today/upcoming) |
| Staff Working Hours | — | `/api/admin/staff-working-hours`, `/api/admin/staff/{id}/working-hours` |
| Availability Slots | `GET /api/availability/slots` | — |

### Appointment Scheduling Engine (v2.0 capability)

- Staff working hours per day of week (start/end time, break window,
  `IsWorkingDay` flag)
- Public availability slot query: returns time slots for a staff member on a
  given date, respecting working hours, break window, and existing bookings
- **Duration-based conflict logic** (added in v1.8):
  - Slot generation: a slot is only included if `slotStart + serviceDuration ≤ workEnd`
  - Break exclusion: `slotStart < breakEnd && slotEnd > breakStart`
  - Booking conflict: `newStart < existingEnd && newEnd > existingStart`
  - Existing appointments use their own service's duration (default 30 min)
  - Pending and Confirmed appointments block; Cancelled and Completed do not
- Appointment creation (`POST /api/appointments`) validates the requested
  interval against working hours, break window, and existing bookings when
  `staffMemberId` is provided — returns 400 for invalid times, 409 for conflicts
- Appointments without `staffMemberId` bypass availability checks and are
  accepted as generic requests
- Admin can list, filter, update, and change appointment status; appointment
  stats endpoint provides totals by status plus today/upcoming-7-day counts

---

## Not Included in v2.0

The following are intentionally deferred to post-v2.0:

| Area | Notes |
|---|---|
| Holidays / date exceptions | No mechanism to mark specific dates as non-working |
| Staff-service assignment | Any staff member can be booked for any service |
| Booking rules | No min/max advance window, no inter-appointment padding |
| Calendar sync | No Google Calendar or iCal integration |
| Notifications | No email or SMS confirmation/reminder |
| Payments | No online payment capture |
| Multi-tenancy | Single-business deployment only |
| Frontend / admin panel | API only — no browser UI |
| Automated tests | No unit or integration test suite yet |
| Docker / deployment | No Dockerfile or CI/CD pipeline |
| Time zones | All times are stored as strings; no timezone conversion |

---

## Suggested Post-v2.0 Roadmap

1. **v2.1** — Holidays and date exceptions (staff-specific non-working dates)
2. **v2.2** — Staff-service assignment (restrict which staff perform which services)
3. **v2.3** — Booking rules (advance window, slot padding, max daily bookings)
4. **v2.4** — Email notifications (appointment confirmation and reminder)
5. **v2.5** — Automated test suite (unit + integration)
6. **v2.6** — Docker + deployment guide (PostgreSQL, environment config, CI/CD)
7. **v3.0** — Multi-tenancy, payments, calendar sync, admin panel

---

## Technical Notes

### Default Development Credentials

| Field | Value |
|---|---|
| Email | `admin@businesskit.local` |
| Password | `Admin123!` |

Seeded automatically on first run. **Do not use in any shared or production environment.**

### JWT Secret

The `JwtSettings:SecretKey` in `appsettings.json` is a development placeholder.
Override it via environment variables or user secrets before any non-local deployment.

### Database

SQLite (`businesskit.db`) is used for development and demos. For production
workloads, migrate to PostgreSQL or SQL Server. The `.db` file is git-ignored.

### Schema

8 EF Core migrations, last applied: `AddStaffWorkingHours` (v1.4).
No schema changes were made in v1.5 through v2.0.

---

## Manual Verification Summary (v2.0)

Full checklist: see `SMOKE_TESTS.md`.

Key checks confirmed clean before v2.0 tag:

- Build: 0 errors, 0 warnings
- `GET /api/health` → 200
- `POST /api/auth/login` → 200, JWT returned
- All public endpoints return 200 with expected data
- All admin endpoints return 200 with valid token, 401 without
- Appointment creation: null staff → 201; valid staff + slot → 201;
  overlap with Pending/Confirmed → 409; cancelled appointment does not block
- Availability slots: last slot respects full service duration;
  break window excluded duration-based; overlapping bookings remove the slot
- `dotnet ef migrations list` confirms no unexpected migrations
- `git status` clean before tag
