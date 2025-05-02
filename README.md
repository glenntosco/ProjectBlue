# P4 Licensing & Partner Management Platform

This repository contains the complete source code and documentation for the P4 Licensing Platform. This system allows P4 Software and its partners to manage tenant licensing, distributor/reseller control, KYC compliance, and partner branding across a secure multi-tenant SaaS infrastructure.

---

## 🔧 Technology Stack

- **Frontend**: Blazor Server (C# 13, .NET 9)
- **UI**: MudBlazor components
- **Backend**: API-first using JWT with Ed25519 + AES-256-GCM for license encryption
- **Database**: Azure SQL per tenant, `P4L_Master` for license registry
- **Logging**: SQL (critical) + Cosmos DB (micro-logs)
- **CI/CD**: GitHub Actions → Azure App Services
- **Billing**: Stripe subscriptions + webhooks

---

## 📁 Folder Structure

```
/Pages            - All main Razor pages (Login, Dashboard, Licenses, etc.)
/Components       - Shared UI widgets and controls
/Services         - Interfaces + implementations
/Controllers      - API logic split by role
/Models           - Domain entities (License, Partner, Certification, etc.)
/Reports          - .mrt Stimulsoft reports
/docs             - Markdown documentation
```

---

## 📚 Documentation Index

| File                       | Description                                |
|----------------------------|--------------------------------------------|
| `docs/Architecture.md`     | Full system design and provisioning flow   |
| `docs/Copilot_Dev_Guide.md`| Guide for Copilot + modular file creation  |
| `docs/Branding.md`         | Partner customization and UI branding      |
| `docs/PartnerDashboard.md` | Metrics + widgets shown to each partner    |

For docs index page: [`docs/index.md`](docs/index.md)

---

## 🚀 Getting Started (Local Dev)

1. Clone this repository
2. Configure `appsettings.json`:
   - `ConnectionStrings:DefaultConnection`
   - JWT Key / Signing Key
3. Launch from Visual Studio 2022+
4. Default Admin user seeded with:
   - **Username**: `admin@p4.software`
   - **Password**: `admin!2025`

---

## 🧠 GitHub Copilot Developers

If you are using Copilot:
- Review [`Copilot_Dev_Guide.md`](docs/Copilot_Dev_Guide.md)
- All services should follow async interface + logging pattern
- Scaffold Razor pages using the file structure shown above

---

## 📦 Deployment & Backups

- CI/CD via `main → GitHub Actions → Azure App`
- Automatic backups: 6-hour BACPAC to Azure Blob Cold Storage
- License verification: offline-capable with embedded Ed25519 signature

---

© 2025 P4 Software / Grupo Barrdega. All rights reserved.
