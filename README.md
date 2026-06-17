# BusinessKit Backend

## Purpose

BusinessKit Backend is a reusable ASP.NET Core Web API backend for small and
medium business websites — cafes, barbers, clinics, consultants, gyms,
courses, agencies, and similar businesses. It provides the common building
blocks these sites need (authentication, business settings, a service
catalog, contact form handling, a photo gallery, a blog, staff profiles, and
appointment request management) behind a clean, versioned REST API that any
frontend can consume.

**Current release: v2.0 MVP.** This is the first complete, usable backend
release. It is suitable for local development, demos, and small deployments
where the limitations noted under [Post-v2.0 Roadmap](#post-v20-roadmap) are
acceptable. It is not a production SaaS product.

## Tech Stack

- **.NET 8** / ASP.NET Core Web API
- **Entity Framework Core 8** with **SQLite** (local/dev database)
- **JWT Bearer authentication**
- **BCrypt.Net-Next** for password hashing
- **Swashbuckle (Swagger / OpenAPI)** for interactive API docs

## Architecture Overview

The solution follows a layered architecture:

- **BusinessKit.Domain** — entities only (`User`, `Role`, `UserRole`,
  `BusinessSettings`, `BusinessService`, `ContactMessage`, `GalleryItem`,
  `BlogPost`, `StaffMember`, `Appointment`, `StaffWorkingHour`). No framework
  or persistence concerns. The Availability module has no entity of its own —
  it is a pure query service over existing data.
- **BusinessKit.Application** — DTOs and service interfaces per module
  (e.g. `IBlogService`, `IGalleryService`). Defines contracts; has no EF
  Core dependency.
- **BusinessKit.Infrastructure** — service implementations using
  `AppDbContext` directly, EF Core entity configurations, and migrations.
  No repository or unit-of-work abstraction is used — each service talks to
  `AppDbContext` directly, which keeps the codebase simple and easy to follow.
- **BusinessKit.Api** — ASP.NET Core controllers. Controllers inject only
  Application-layer interfaces, never Infrastructure implementations or
  Domain entities directly.
- **BusinessKit.Shared** — cross-cutting constants (e.g. `Roles`).

Each business module follows the same pattern: a Domain entity, an EF
configuration, a pair of public/admin DTOs, a service interface +
implementation, and a pair of public/admin controllers (where applicable).

## Modules

| Module | Public endpoints | Admin endpoints |
|---|---|---|
| Health | `GET /api/health` | — |
| Auth | `POST /api/auth/login`, `GET /api/auth/me` | — |
| Admin Users/Roles | — | `/api/admin/users`, `/api/admin/roles` |
| Business Settings | `GET /api/business-settings` | `PUT /api/admin/business-settings` |
| Services | `/api/services` | `/api/admin/services` |
| Contact Messages | `POST /api/contact-messages` | `/api/admin/contact-messages` |
| Gallery | `/api/gallery` | `/api/admin/gallery` |
| Blog | `/api/blog` | `/api/admin/blog` |
| Staff | `/api/staff` | `/api/admin/staff` |
| Image Upload | — | `POST /api/admin/uploads/image` |
| Appointments | `POST /api/appointments` | `/api/admin/appointments` |
| Staff Working Hours | — | `/api/admin/staff-working-hours`, `/api/admin/staff/{id}/working-hours` |
| Availability | `GET /api/availability/slots` | — |

## How to Run Locally

```bash
cd BusinessKit.Api
dotnet run
```

The API starts on the URL printed in the console (e.g.
`http://localhost:5299` if you pass `--urls`). On startup, the app seeds
default roles (`Admin`, `Manager`, `Viewer`) and a default development admin
user if they don't already exist.

## How to Apply Migrations

Migrations live in `BusinessKit.Infrastructure/Data/Migrations`. To create
the local SQLite database (or bring it up to date after pulling new
migrations), run from the repository root:

```bash
dotnet ef database update --project BusinessKit.Infrastructure --startup-project BusinessKit.Api
```

To add a new migration after changing an entity or EF configuration:

```bash
dotnet ef migrations add <MigrationName> --project BusinessKit.Infrastructure --startup-project BusinessKit.Api
```

## Default Development Admin Credentials

| Field | Value |
|---|---|
| Email | `admin@businesskit.local` |
| Password | `Admin123!` |

These are seeded automatically by `DataSeeder` for local development only.
**Do not rely on this account in any non-development environment.**

## Main Endpoints

**Public**
- `GET /api/health`
- `POST /api/auth/login`
- `GET /api/business-settings`
- `GET /api/services`, `GET /api/services/{slug}`
- `POST /api/contact-messages`
- `GET /api/gallery`, `GET /api/gallery/{id}`
- `GET /api/blog`, `GET /api/blog/{slug}`
- `GET /api/staff`, `GET /api/staff/{slug}`
- `POST /api/appointments`
- `GET /api/availability/slots?staffMemberId=&date=&businessServiceId=` (optional businessServiceId)

**Authenticated**
- `GET /api/auth/me`

**Admin only** (`Authorize(Roles = "Admin")`)
- `/api/admin/users`, `/api/admin/roles`
- `PUT /api/admin/business-settings`
- `/api/admin/services` (list, get, create, update, toggle-active)
- `/api/admin/contact-messages` (list, get, mark-read/unread, mark-replied, archive/unarchive)
- `/api/admin/gallery` (list, get, create, update, toggle-active)
- `/api/admin/blog` (list, get, create, update, publish/unpublish)
- `/api/admin/staff` (list, get, create, update, toggle-active)
- `POST /api/admin/uploads/image`
- `GET /api/admin/appointments` (filters: status, staffMemberId, businessServiceId, date; or startDate/endDate range — combining date with startDate/endDate returns 400)
- `GET /api/admin/appointments/today` (optional filters: status, staffMemberId, businessServiceId)
- `GET /api/admin/appointments/upcoming` (optional filters: status, staffMemberId, businessServiceId, days — defaults to 7)
- `GET /api/admin/appointments/stats` (optional filters: staffMemberId, businessServiceId, startDate, endDate)
- `GET /api/admin/appointments/{id}`
- `PATCH /api/admin/appointments/{id}/status`
- `PUT /api/admin/appointments/{id}`
- `GET /api/admin/staff-working-hours` (optional filter: `?staffMemberId=`)
- `GET /api/admin/staff-working-hours/{id}`
- `GET /api/admin/staff/{staffMemberId}/working-hours`
- `POST /api/admin/staff-working-hours`
- `PUT /api/admin/staff-working-hours/{id}`

Open `/swagger` while the API is running for the full interactive list,
including request/response shapes.

## Version History

| Version | Summary |
|---|---|
| v0.1 | Scaffolded layered backend solution, Swagger, `GET /api/health` |
| v0.2 | Domain entities, EF Core SQLite, `AppDbContext`, `InitialCreate` migration |
| v0.3 | JWT authentication: BCrypt hashing, default admin, roles, login/me, Swagger Bearer auth |
| v0.4 | Admin user and role management (CRUD + toggle-active, admin-only authorization) |
| v0.5 | Business Settings module (public GET, admin upsert) |
| v0.6 | Service Catalog module (public/admin, slug uniqueness, active/inactive) |
| v0.7 | Contact Messages module (public submit, admin read/unread/replied/archive workflow) |
| v0.8 | Gallery module (public/admin, category filter, active/inactive, `ImageUrl` string only) |
| v0.9 | Blog module (public/admin, slug+language uniqueness, publish/unpublish workflow) |
| v1.0 | MVP stabilization & API polish: route consistency, minimal global error handling, Swagger tags, documentation — no new business modules |
| v1.1 | Local image upload system (`POST /api/admin/uploads/image`, served from `wwwroot/uploads/images`) |
| v1.2 | Staff management module (public profile listing by slug, admin CRUD + toggle-active, photo URL, social links) |
| v1.3 | Appointment Foundation: public appointment request creation; admin list/detail/status-update/full-update; optional StaffMember and BusinessService references; status workflow (Pending → Confirmed / Cancelled / Completed); status validation returns 400 for unknown values — **not a full scheduling engine**: no slot calculation, no working hours, no calendar sync, no notifications, no payments |
| v1.4 | Staff Working Hours Foundation: admin can define weekly working hours per staff member (day, start/end time, optional break window, IsWorkingDay flag); unique index on StaffMemberId + DayOfWeek enforces one row per day; 409 on duplicate, 400 on invalid staff or day — **does not calculate available appointment slots**: no slot engine, no conflict detection, no booking rules, no calendar sync, no notifications |
| v1.5 | Available Slot Calculation Foundation: public `GET /api/availability/slots` returns available time slots for a staff member on a given date; respects staff working hours (start/end time, IsWorkingDay, break window); excludes slots blocked by Pending or Confirmed appointments; slot duration defaults to 30 min or uses `BusinessService.DurationMinutes` when `businessServiceId` is supplied — **simple foundation only**: no holidays/exceptions, no multi-slot conflict duration, no staff-service assignment, no calendar sync, no time zones |
| v1.6 | Appointment Creation Availability Validation: `POST /api/appointments` now validates the requested time against staff working hours when `staffMemberId` is provided — checks the staff working hour record for that day, blocks creation on non-working days (400), outside working hours (400), during the break window (400), and at a time already held by a Pending or Confirmed appointment (409); Cancelled and Completed appointments do not block the slot; requests with no `staffMemberId` bypass availability checks and are still accepted as generic appointment requests — exact-time conflict only, no duration-based overlap engine yet |
| v1.7 | Appointment Admin Polish: `GET /api/admin/appointments` gains `startDate`/`endDate` range filters; combining `date` with `startDate`/`endDate` returns 400 to avoid ambiguity; new `GET /appointments/today` returns today's appointments with optional status/staff/service filters; new `GET /appointments/upcoming` returns appointments from today onward with optional `days` parameter (default 7, value ≤ 0 returns 400); new `GET /appointments/stats` returns totals by status plus today-count and upcoming-7-day count with optional staffMember/service/date-range filters; all existing detail/status/update endpoints unchanged — no duration-based conflict logic, no notifications, no calendar sync, no payments, no multi-tenancy |
| v1.8 | Duration-Based Appointment Conflict Logic: availability slots now respect the full service duration — the last slot is only included if `slotStart + duration ≤ workEnd`; break-time exclusion is now interval-based (`slotStart < breakEnd && slotEnd > breakStart`) instead of point-in-time; existing appointments block a range equal to their own service duration (defaulting to 30 min), so a 60-minute appointment at 10:00 blocks 10:00–11:00 and prevents new appointments at 10:30; `POST /api/appointments` applies the same duration-aware overlap check when `staffMemberId` is provided — outside-hours (duration end exceeds work end) and break-overlap return 400, scheduling conflict returns 409; Cancelled and Completed appointments still do not block; requests without `staffMemberId` still bypass availability checks — no new tables, no migrations, no holidays/exceptions, no calendar sync, no notifications, no payments, no multi-tenancy |
| v1.9 | Final MVP Cleanup: route consistency audit across all 21 controllers (all public routes under `api/…`, all admin routes under `api/admin/…`, `{id:int}` constraints in place, fixed sub-routes `today`/`upcoming`/`stats` ordered before `{id:int}` to prevent shadowing); Swagger tag review — all controllers confirmed tagged correctly with Public/Admin suffixes; error-response consistency check — 400/404/409 patterns uniform across all modules; README updated to reflect full v1.0–v1.9 history and v2.0 roadmap; `SMOKE_TESTS.md` added as a manual regression checklist; no schema changes, no migrations, no new features |
| **v2.0** | **First MVP release**: all v1.x modules stable and verified; `RELEASE_NOTES.md` added; README and `SMOKE_TESTS.md` updated to reflect current release state; full regression pass confirmed clean; build 0 errors / 0 warnings; last schema migration `AddStaffWorkingHours` — no schema changes since v1.4; tagged as first usable backend release for demos, local development, and small deployments |

## Notes on Secrets

The `JwtSettings:SecretKey` value in `appsettings.json` is a **development-only
placeholder**. It must be overridden via environment variables or user
secrets before deploying to any shared or production environment. Never
commit a real secret key to source control.

## Notes on the Database

This project uses **SQLite** (`businesskit.db`) as a simple, file-based
database suited for local development and demos. It is not intended for
production deployments with concurrent write load — a production deployment
should migrate to a server-based database (e.g. PostgreSQL or SQL Server).
The `.db` file itself is git-ignored and must never be committed.

## Post-v2.0 Roadmap

v2.0 is the first MVP release. The following features are intentionally out
of scope for v2.0 and are planned for future versions:

- **Holidays and date exceptions** — staff-specific or business-wide non-working dates
- **Staff-service assignment** — restrict which staff members can perform which services
- **Booking rules** — min/max advance booking windows, slot padding between appointments
- **Calendar sync** — Google Calendar or iCal integration
- **Notifications** — email/SMS confirmation and reminder workflows
- **Payments** — online payment capture at booking time
- **Multi-tenancy** — multiple businesses per deployment with isolated data
- **Frontend / admin panel** — browser-based management UI
- **Automated tests** — unit and integration test suites
- **Docker / deployment** — containerisation, CI/CD pipeline, production database migration guidance
- **QR-code menus** — for F&B businesses
- **CRM / customer management** — customer profiles, visit history, loyalty

## Manual Regression Checklist

Use this checklist after any change to verify the MVP still works end to end.
A fresh JWT (`POST /api/auth/login` with the default admin credentials above)
is required for all admin-only checks.

- [ ] `GET /api/health` → `200`, returns `{ status: "healthy", timestamp }`
- [ ] `POST /api/auth/login` with valid credentials → `200`, returns a JWT
- [ ] `POST /api/auth/login` with wrong password → `401`
- [ ] `GET /api/auth/me` with valid token → `200`, returns current user + roles
- [ ] `GET /api/auth/me` without token → `401`
- [ ] `GET /api/admin/users` with admin token → `200`
- [ ] `GET /api/admin/users` without token → `401`
- [ ] `POST /api/admin/users` with a duplicate email → `409`
- [ ] `POST /api/admin/users` with an invalid role name → `400`
- [ ] `GET /api/admin/roles` with admin token → `200`
- [ ] `GET /api/business-settings` (public) → `200` (or `404` if never configured)
- [ ] `PUT /api/admin/business-settings` with admin token → `200`, upserts
- [ ] `GET /api/services` (public) → `200`, only active services, ordered by `DisplayOrder, Id`
- [ ] `GET /api/services/{slug}` for an inactive/unknown slug → `404`
- [ ] `POST /api/admin/services` with a duplicate slug → `409`
- [ ] `PATCH /api/admin/services/{id}/toggle-active` → flips `IsActive`
- [ ] `POST /api/contact-messages` (public, no token) with valid body → `201`
- [ ] `POST /api/contact-messages` missing a required field → `400`
- [ ] `GET /api/admin/contact-messages` with admin token → `200`, newest first
- [ ] `PATCH .../mark-read`, `.../mark-unread`, `.../mark-replied`, `.../archive`, `.../unarchive` all return `200`
- [ ] `GET /api/gallery` (public) → `200`, only active items, ordered by `DisplayOrder, Id`
- [ ] `GET /api/gallery?category=X` filters correctly (case-insensitive)
- [ ] `PATCH /api/admin/gallery/{id}/toggle-active` → flips `IsActive`
- [ ] `GET /api/blog` (public) → `200`, only published posts, ordered by `PublishedAt desc, Id desc`
- [ ] `GET /api/blog/{slug}?language=en` and `?language=tr` resolve to the correct post when both exist
- [ ] `POST /api/admin/blog` with the same slug + same language → `409`
- [ ] `POST /api/admin/blog` with the same slug + different language → `201`
- [ ] `PATCH /api/admin/blog/{id}/publish` auto-sets `PublishedAt` if it was null
- [ ] `PATCH /api/admin/blog/{id}/unpublish` does **not** clear `PublishedAt`
- [ ] Any admin endpoint called without a token → `401`
- [ ] Swagger UI (`/swagger`) loads, "Authorize" button accepts a Bearer token, and protected endpoints succeed once authorized
