# Copilot Developer Guide

This document is written specifically to guide GitHub Copilot in generating the core services, Razor pages, API endpoints, and UI components for the P4 Licensing Platform.

---

## Folder Structure (Required)

Copilot should assume the following directory layout:

```
/Pages
  /Login
  /Dashboard
  /Partners
  /Licenses
  /KYC
  /Reports
/Components
  /Shared
  /UI
/Controllers
  /Admin
  /Partner
  /Distributor
/Services
  /Interfaces
  /Implementations
/Models
  /License.cs
  /KycProfile.cs
  /Partner.cs
  /Certification.cs
  /Tenant.cs
/Reports
  /Stimulsoft
```

---

## Required Services to Scaffold

Copilot must generate the following services using C# interfaces + Blazor DI pattern:

- `ILicenseService` + `LicenseService`
- `IPartnerService` + `PartnerService`
- `IKycService` + `KycService`
- `ICertificationService`
- `IAuditLogService`
- `IBackupService` (for 6-hour BACPAC backup tasks)
- `IEmailService` (with HTML theming and language override support)
- `JwtService` (JWT creation/validation helper)
- `TranslationService` (loads per-language UI strings from database)

Each service must follow async/await patterns and log user actions via `AuditLogService`.

---

## Role and Token Design

- Copilot should implement JWTs that include:
  - `TenantId`
  - `Role` (Admin, Distributor, Reseller, TenantUser)
  - `FeatureFlags`

- All controllers should validate token claims and enforce role-based access

- Example roles:
  - `Admin`: full access
  - `Distributor`: provision tenants, manage KYC
  - `Reseller`: view data, limited provisioning
  - `TenantUser`: read-only license/usage dashboard

---

## Frontend Patterns

- Use **MudBlazor** for all components
- UI should reflect license feature flags
- All forms require validation
- Razor pages should reflect Role and KYC status

---

## Sample Copilot Prompt Examples

```csharp
// Create a Razor page to list all licenses and allow admin to revoke them
// Scaffold API controller for POST /api/license/create
// Add token generation method that includes tenant ID and user role
// Create service to upload logo and save branding settings per partner
// Scaffold backup service that writes a BACPAC every 6 hours to Azure Blob
```

---

## Reporting & Localization

- Reports should use `.mrt` files loaded into `/Reports/Stimulsoft/`
- Language files should be stored in `LanguageEntry` table
- Copilot should support string overrides per language using `TranslationService`

---

© 2025 P4 Software / Grupo Barrdega. All rights reserved.
