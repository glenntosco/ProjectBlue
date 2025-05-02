# System Architecture – P4 Licensing & Partner Management Platform

This document provides a detailed breakdown of the architecture powering the P4 Licensing Platform. It covers the core components, flows, multi-tenant model, licensing logic, partner branding layers, and SaaS provisioning infrastructure. Designed for long-term maintainability and partner extensibility, this architecture enforces high security, auditability, and multi-organization governance.

---

## 🧱 High-Level Overview

- **Platform Type**: Multi-tenant cloud software with per-product licensing.
- **Primary Interface**: Admin + Partner Portal (Blazor Server).
- **Persistence**: Azure SQL Server (`P4L_Master` for core, 1 DB per tenant).
- **Deployment Model**: Azure Web App (separate apps per product).
- **Authentication**: JWT signed using Ed25519, encrypted with AES-256-GCM.
- **Billing Logic**: Stripe webhooks triggering license state transitions.

---

## 🧩 Core Components

| Component                  | Description                                                                 |
|----------------------------|-----------------------------------------------------------------------------|
| `P4L_Master` DB            | Central license registry, partner metadata, user accounts, logs            |
| `Per-Tenant Databases`     | Provisioned by the licensing platform; used by product SaaS apps           |
| `Blazor Portal`            | Unified UI for Admins, Distributors, Resellers, and Tenants                |
| `License Service`          | Core logic for issuing, signing, and encrypting license packages           |
| `Partner Config Engine`    | Handles branding (logo, theme, footer) per partner                         |
| `Stripe Webhook Listener`  | Receives billing events to update license state                            |
| `Stimulsoft Report Engine` | Embedded reporting in portal and partner dashboards                        |
| `Language Manager`         | JSON-based i18n system with admin-editable text                            |
| `Azure App Services`       | Hosting for Licensing Portal and each product (P4W, P4B, P4C)               |
| `GitHub Actions`           | CI/CD deployment pipeline with staging/production environments             |

---

## 🧮 Database Structure

### `P4L_Master` – Central License Registry

This database contains:

- `Licenses` – License records (per tenant, per product)
- `Partners` – Distributor and reseller records
- `Tenants` – SaaS customer metadata
- `Users` – Portal login users (role-based)
- `LoginLogs` – IP logs, country flags, timestamps
- `MicroLogs` – Cosmos DB-linked minor changes (e.g., theme edits)
- `StripeSubscriptions` – Stripe subscription metadata
- `Events` – Internal platform events (e.g., license expiry, webhooks)

> 🔐 Tenant database DSNs are encrypted when stored and signed before delivery.

---

## 👥 User Roles and UI Modes

| Role           | Description                                                                       |
|----------------|-----------------------------------------------------------------------------------|
| `Admin`        | Full platform access; can impersonate any partner, control all tenants/licenses  |
| `Distributor`  | Can create resellers, assign license quotas, view white-labeled portal           |
| `Reseller`     | Can provision tenants from assigned quota; no access to pricing controls         |
| `TenantUser`   | End-user login for accessing software; typically not exposed to this platform    |

> Role-based menus, dashboard widgets, and branding are dynamically rendered via injected claims.

---

## 🔄 License Generation Flow

1. **Admin or Partner provisions a new tenant**
2. Platform generates a **license JSON**:
   - Product Code (P4W, P4B, P4C)
   - Expiry, Seats, Language, Region
   - Tenant Metadata (ID, Alias, PartnerID)
3. License is **digitally signed** (Ed25519) and **encrypted** (AES-256-GCM)
4. JSON is stored in `Licenses`, a **PDF license summary** is emailed
5. Platform **pings the SaaS app endpoint** (e.g., `https://client.p4books.cloud/handshake`)
6. SaaS app responds to validate the license and provision DB

---

## 🧬 Tenant Provisioning Flow

1. Reseller creates new **Tenant**
2. System creates:
   - Tenant DB (`Project_XXXX`)
   - Two users (readonly + readwrite)
   - Entry in `Tenants` table
3. Writes DB DSN and metadata to the target product’s **master DB**
4. Calls SaaS app `/handshake` endpoint
5. SaaS app reads license, validates, stores, and completes onboarding

> ⚙️ Product-specific master DBs are `P4Books_Master`, `P4Warehouse_Master`, etc.

---

## 💸 Stripe Billing Integration

- Each Partner or Tenant is linked to a `StripeCustomerId`
- Subscription changes are triggered via:
  - Stripe dashboard (manually)
  - Automated webhook events (`invoice.paid`, `customer.subscription.deleted`)
- Webhook listener updates:
  - License status (`Active`, `Expired`, `Suspended`)
  - Quota enforcement (e.g., seats exceeded)
- Admin can override billing enforcement for testing or delayed payments

---

## 🌐 Internationalization (i18n)

- JSON files per language (English, Spanish, French, etc.)
- Admins can:
  - Add new languages
  - Edit phrases via Language Manager UI
  - Set default language per tenant
- Emails (system-generated) use:
  - Preferred language only
  - OR bilingual mode (English + Spanish)
  - Configurable footer per language
- UI respects per-user language preferences

---

## 🎨 Partner Branding Layer

Each partner (distributor/reseller) may configure:

- Company Logo
- Theme Accent Color
- Email Footer (per language)
- Custom Stimulsoft Dashboard Widgets
- Documentation Links (e.g., PDF Guides)

Branding is enforced across:

- Tenant Portal
- License PDFs
- Email Templates
- UI Header/Footer

> All customization is stored per `PartnerId` and injected into the UI via middleware.

---

## 📊 Reporting & Dashboards

- Embedded **Stimulsoft Reports** (.mrt files)
- Each Partner dashboard includes:
  - Tenants by Status
  - Active Licenses
  - Expiring Soon
  - Usage Heatmaps
- Admin dashboards include:
  - All-partner aggregation
  - Billing status vs. license usage
  - Failed webhook alerts

---

## 🔐 Security Architecture

| Mechanism          | Description                                           |
|--------------------|-------------------------------------------------------|
| `JWT Tokens`       | Used across API calls; signed with Ed25519            |
| `AES-256-GCM`      | License payloads encrypted with symmetric key         |
| `Two-Factor Auth`  | Planned enhancement for Admin + Partner accounts      |
| `Geo-IP Logging`   | Login IP stored with reverse-lookup country info      |
| `Audit Trail`      | CosmosDB event capture (e.g., language edits)         |
| `Role Claims`      | Roles resolved via identity server and token claims   |

---

## ☁️ CI/CD and Environments

- **CI/CD Pipeline**: GitHub Actions → Azure Web App
- Environments:
  - `Development` – Local, Rider or VS2022
  - `Staging` – Internal testing (manual swap)
  - `Production` – Public SaaS + partner access
- Backups:
  - Full P4L_Master BACPAC every 6 hours → Azure Blob Cold
  - Tenant DB backups handled per-app basis

---

## 🧠 Future Enhancements

- QR code scan-to-validate (rejected, archived design)
- HardwareKey license locks (planned for OEM devices)
- AI-powered license usage scoring (planned)

---

© 2025 P4 Software / Grupo Barrdega. Internal use only. Do not redistribute.
