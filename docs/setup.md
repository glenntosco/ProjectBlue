# 🔧 Full Setup Instructions – P4 Licensing & Partner Management Platform

This document details the full local development setup for the P4 Licensing Platform, designed for high-fidelity execution by AI and human developers. The setup uses a modular architecture, secure cryptographic licensing, and multi-tenant SaaS provisioning.

---

## 🧱 Platform Overview

* **Frontend**: Blazor Server (`P4LicensePortal.Web`)
* **Licensing Logic**: Encrypted and signed JSON with Ed25519 + AES-256-GCM
* **Database**:

  * Central: `ProjectBlueDev` (Azure SQL)
  * One per provisioned tenant
* **Authentication**: Role-based (Admin, Distributor, Reseller, TenantUser)
* **Hosting**: Azure App Services
* **Billing**: Stripe Subscriptions via webhook

---

## 📂 Project Structure

```
P4LicensePortal/
├── P4LicensePortal.Api // Web API endpoints (REST)
├── P4LicensePortal.Core // Interfaces, Enums, DTOs
├── P4LicensePortal.Data // EF Core models and DbContext
├── P4LicensePortal.Services // Service implementations
└── P4LicensePortal.Web // Blazor Server UI (Razor Pages, Components)
/docs
```

---

## 🛠️ Requirements

| Tool            | Version / Notes                                |
| --------------- | ---------------------------------------------- |
| .NET SDK        | 9.0 or later                                   |
| JetBrains Rider | Preferred IDE with ASP.NET Core + Razor plugin |
| SQL Server      | Azure SQL (uses `ProjectBlueDev`)              |
| Git             | GitHub CLI or Git Desktop                      |
| Azure CLI       | For remote deploy and Blob Storage (optional)  |
| Node.js         | Optional (for future tooling or preprocessors) |

---

## 🔧 Setup Instructions

### 1. Clone the Repository



### 2. Create the Central Database (if needed)

```sql
CREATE DATABASE ProjectBlueDev;
```

Or confirm it exists in Azure SQL. Ensure your IP is whitelisted.

---

### 3. Create `appsettings.Development.json`

Create `src/ProjectBlue.Server/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=fixedassets-server.database.windows.net;Database=ProjectBlueDev;User Id=fixedassets-server-admin;Password=Ns$N$y6FNihf4xw1;TrustServerCertificate=True;Connection Timeout=30;"
  },
  "Jwt": {
    "Issuer": "p4.software",
    "Audience": "p4.software",
    "SigningKey": "<your-ed25519-private-key>",
    "EncryptionKey": "<your-aes-256-gcm-key>"
  },
  "LicenseSettings": {
    "PingTargetUrl": "https://*.p4books.cloud/handshake",
    "DefaultLicenseDays": 30,
    "DefaultSeats": 3
  },
  "Logging": {
    "MicroLogs": {
      "UseCosmosDb": true,
      "EndpointUri": "https://your-cosmos.documents.azure.com:443/",
      "PrimaryKey": "<cosmos-primary-key>",
      "DatabaseName": "P4Logs",
      "ContainerName": "MicroLogs"
    }
  }
}
```

> Do not commit this file. Use environment variables or GitHub secrets in production.

---

### 4. Run Entity Framework Migrations

```bash
cd src/ProjectBlue.Infrastructure
dotnet ef database update --startup-project ../ProjectBlue.Server
```

To add a migration:

```bash
dotnet ef migrations add InitSchema --context ApplicationDbContext --startup-project ../ProjectBlue.Server
```

---

### 5. Launch the Server

#### Option A: Rider

* Open `LicensingPlatform.sln`
* Set `ProjectBlue.Server` as startup project
* Click **Run** or use Kestrel configuration

#### Option B: Command Line

```bash
dotnet run --project src/ProjectBlue.Server
```

The system will be live at:

```
https://localhost:5001/
```

---

## 🔑 Default Credentials

* **Email**: `admin@p4.software`
* **Password**: `admin!2025`

Use this account to access the Admin Portal.

---

## 🔄 License Provisioning Test (Manual)

1. Log in as Admin
2. Create Distributor → Reseller → Tenant
3. Issue License (e.g., P4B, 30 days, 3 seats)
4. System will:

   * Generate license JSON
   * Sign with Ed25519
   * Encrypt with AES-256-GCM
   * Email license summary (PDF)
   * Ping product domain (e.g., `https://client.p4books.cloud/handshake`)
   * Product creates and links tenant DB

---

## 🚀 CI/CD Deployment

* GitHub Actions triggered on `main` branch push
* Deploys to Azure App Services
* Optional auto-swap staging → production

Backups:

* `ProjectBlueDev` is backed up via BACPAC every 6 hours
* Stored in Azure Blob Cold Tier

---

## 🚨 Troubleshooting

| Problem                     | Fix                                                              |
| --------------------------- | ---------------------------------------------------------------- |
| SQL login fails             | Whitelist your IP in Azure SQL firewall                          |
| EF migration error          | Add `--startup-project ../ProjectBlue.Server` to command         |
| Ping fails on license issue | Confirm `PingTargetUrl` is correct and target SaaS app is online |
| Login fails                 | Check `Users` table for `admin@p4.software` record               |

---

## 🧠 AI Guidelines

When developing features or tests:

* Follow async service patterns using DI and `ILogger<T>`
* Use Ed25519 to sign JSON license payloads
* Encrypt using AES-256-GCM with base64 keys
* Never hardcode credentials in `.cs` files
* Use Radzen components unless otherwise stated
* Localize using JSON-based editable language packs
* Log UI micro-events to Cosmos DB
* Honor role-based rendering for Admin, Distributor, Reseller, and TenantUser

---

© 2025 P4 Software / Grupo Barrdega. For internal development use only.
