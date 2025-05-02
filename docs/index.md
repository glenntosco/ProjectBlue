# P4 Licensing Documentation

Welcome to the P4 Licensing & Partner Management Platform documentation site.

This documentation is designed to support developers, partners, and contributors building or integrating with the platform. It includes architectural guidance, integration logic, branding rules, and module-level feature references.

---

## 📘 Sections

- [Architecture Overview](Architecture.md)  
  System structure, tenant provisioning, feature flags, backup logic

- [Developer Guide (Copilot)](Copilot_Dev_Guide.md)  
  Folder layout, naming conventions, Copilot prompts, service interfaces

- [Branding & Theme Management](Branding.md)  
  Logo injection, accent color, light/dark theme logic, email branding

- [Partner Dashboard Guide](PartnerDashboard.md)  
  Role-based widgets, KPI tracking, reseller vs distributor views

---

## 🧠 Who Should Read This

- JetBrains  users building Razor + C# modules
- Internal P4 Software developers integrating new features
- Partners branding the UI or configuring language settings
- Contributors customizing Stripe tiers, provisioning flows, or audit tools

---

## 🛠️ System Notes

- All source code is Blazor Server (.NET 9, MudBlazor)
- Database is Azure SQL per tenant, with `ProjectBlueDev` for license registry development db, production db is ProjectBlue
- CI/CD via GitHub Actions to Azure Web App
- Cosmos DB used for low-cost micro-logging

---

© 2025 P4 Software / Grupo Barrdega. All rights reserved.
