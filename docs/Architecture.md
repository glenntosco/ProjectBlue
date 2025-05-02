# Architecture Overview

## Platform Purpose

The P4 Licensing Platform provides a secure, multi-tenant, API-first architecture for distributing and managing encrypted licenses for SaaS products. It supports partner branding, regional compliance (KYC), Stripe-integrated billing, and full tenant isolation.

## Multi-Tenant Model

Each tenant (client) is provisioned with:
- A dedicated database (or schema) for full data isolation
- A license signed with Ed25519 and encrypted using AES-256-GCM
- A DNS-routed subdomain like `clientname.p4books.cloud`

Tenant records are managed globally in the `ProjectBlueDev` database, including:
- Tenant metadata
- Associated subscription and license data
- Partner affiliations and role permissions

## License Generation & Validation

Licenses are JSON payloads that include:
- `TenantId`, `ProductCode`, `MaxUsers`, `ExpiryDate`
- FeatureFlags such as `EnableReports`, `EnableAPI`, `IsSandbox`
- Signed using Ed25519 (Curve25519)
- Encrypted using AES-256-GCM per tenant key

Licenses are issued upon provisioning and validated on startup within each SaaS app.

## Provisioning Workflow

1. Admin or Distributor creates a new tenant
2. Tenant database is provisioned (Azure SQL)
3. License is generated and signed
4. A subdomain is created and pointed to the tenant environment
5. Webhook handshake delivers license to app
6. Audit log captures all provisioning events

## Feature Flags

Licenses contain embedded feature flags that control access to modules:

- `EnableReports`
- `EnableAPI`
- `AllowBrandingOverrides`
- `MaxUsers`
- `IsSandbox`

These flags are validated both in the backend and Blazor frontend using claim-based role checks.

## Authentication & Authorization

The platform uses JWT tokens that include:
- `TenantId`
- `Role` (Admin, Distributor, Reseller, TenantUser)
- `Features`

JWT tokens are signed with RS256 and validated server-side. IP filtering and session limits are enforced for partners.

## Database Layer

- **P4L_Master**: Central admin DB for licenses, partners, roles
- **Product Master DBs**: e.g. `P4Books_Master`, `P4Warehouse_Master`
- **Tenant Databases**: Named by GUID or customer alias

## CI/CD and Deployment

- Hosted on Azure Web Apps
- Deployment via GitHub Actions with separate environments
- Each tenant shares the codebase, isolated by DB and license

## Logging & Monitoring

- All licensing and provisioning actions are audit-logged
- UI edits and metadata changes (like language overrides) are stored in Cosmos DB
- Admin portal includes audit viewer and backup access logs

## Backup Strategy

- BACPAC of `P4L_Master` saved to Azure Blob Storage (cold tier) every 6 hours
- Admins can download latest backup from dashboard
- Access is permission-logged and restricted to Admins only

## Scalability

- Multi-region deployment planned with Azure Traffic Manager
- Load-balanced app instances
- Webhook retry queues and asynchronous provisioning flows

---

© 2025 P4 Software / Grupo Barrdega. All rights reserved.
