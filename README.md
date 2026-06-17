# BusinessKit Backend

## Purpose

BusinessKit Backend is a reusable ASP.NET Core Web API backend for small and
medium business websites — cafes, barbers, clinics, consultants, gyms,
courses, agencies, and similar businesses. It provides the common building
blocks these sites need (authentication, business settings, a service
catalog, contact form handling, a photo gallery, a blog, staff profiles, and
appointment request management) behind a clean, versioned REST API that any
frontend can consume.

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
  `BlogPost`, `StaffMember`, `Appointment`). No framework or persistence
  concerns.
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
- `GET /api/admin/appointments` (list with optional filters: status, staffMemberId, businessServiceId, date)
- `GET /api/admin/appointments/{id}`
- `PATCH /api/admin/appointments/{id}/status`
- `PUT /api/admin/appointments/{id}`

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

## Future Modules (Not Yet Implemented)

The following are intentionally out of scope for the current MVP and are
expected to be addressed in future versions:

- Full appointment scheduling engine: available slot calculation, staff
  working hours, calendar sync (e.g. Google Calendar) — v1.3 provides the
  appointment request foundation only
- QR-code menus
- Orders and payments
- CRM / customer management
- Multi-tenancy (multiple businesses per deployment)
- Notifications (email/SMS)

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
