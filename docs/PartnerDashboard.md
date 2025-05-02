# Partner Dashboard Guide

This dashboard is the primary interface for Distributors and Resellers. It summarizes the partner’s performance, compliance, and client activity using real-time KPIs, customizable widgets, and role-filtered data.

---

## Dashboard Purpose

The dashboard provides actionable insights into:
- License and tenant activity
- Partner compliance status (KYC, certifications)
- Revenue and tier progress
- Upcoming tasks (e.g., expiring KYC, inactive tenants)

All widgets are personalized by role:
- **Distributor**: full provisioning view, KYC compliance, reseller analytics
- **Reseller**: assigned clients, active licenses, branding preview

---

## KPIs & Metrics Displayed

| Widget                            | Data Source             | Notes                                 |
|-----------------------------------|--------------------------|----------------------------------------|
| Active Licenses                   | License table           | Filtered by PartnerId                  |
| Expired / Invalid KYC Profiles    | KycProfile table        | Color-coded status (Verified, Expired) |
| Certification Validity Tiles      | PartnerCertification    | Warnings before expiry                 |
| Revenue by Quarter                | License + Stripe        | Used to calculate tier level           |
| Current Tier & Thresholds         | Tier calculator         | Progress bar toward next level         |
| Partner Logo + Branding Preview   | PartnerBranding         | Visual theme preview                   |
| Alerts Panel                      | KYC, Cert, AuditLog     | Urgent action queue                    |

---

## Dashboard Widgets

Each widget is implemented using MudBlazor components:

- **MudCard** for KYC and Cert Status
- **MudChip** for Tier Level display
- **MudProgressLinear** for revenue progress
- **MudTable** for listing recent tenants, licenses, or audits
- **MudAlert** for warning banners

---

## Role-Specific Views

- **Distributor View**:
  - Shows all tenants they provisioned
  - Full access to KYC + License expiration reports
  - Can initiate partner impersonation for support

- **Reseller View**:
  - Sees only their clients
  - No access to create or revoke licenses
  - Branding preview is read-only

---

## Expandability

- Widgets are modular and rendered dynamically
- Feature flag `EnableDashboardKpis` controls visibility
- Future widgets may include:
  - AI usage forecast
  - Real-time license heartbeat
  - Multi-year trend chart

---

© 2025 P4 Software / Grupo Barrdega. All rights reserved.
