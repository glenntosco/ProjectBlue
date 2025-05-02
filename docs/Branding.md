# Branding & Theme Guide

This document defines how partner branding is implemented across the licensing platform. Branding allows every distributor and reseller to maintain their identity while operating within the unified P4 Software environment.

---

## Branding Options per Partner

Each partner has the ability to customize:

- **Logo (SVG or PNG)**  
  Used on login screen, dashboard header, and all email templates

- **Accent Color and Primary Color**  
  Used for theming MudBlazor components dynamically

- **Theme Mode**  
  Light or dark, selected by the partner and saved per session or user

- **Email Footer (HTML)**  
  Customizable per language (e.g., English, Spanish)

- **Favicon (PNG)**  
  Displayed in browser tab and PWA manifest

---

## Where Branding is Applied

| Area                        | Description                             |
|-----------------------------|-----------------------------------------|
| Login Page                 | Partner logo, background color, and theme mode used |
| Admin Dashboard            | Logo in header, accent colors in side nav, charts, cards |
| Email Notifications        | Logo, footer, button styles match partner theme |
| Report Templates           | Reports rendered with selected branding settings |
| Mobile App (Future)        | Color theme and partner icon preloaded for Flutter |

---

## Branding Storage

Branding preferences are stored in the `PartnerBranding` entity:

```csharp
public class PartnerBranding {
  public Guid PartnerId { get; set; }
  public string LogoUrl { get; set; }
  public string PrimaryColor { get; set; }
  public string AccentColor { get; set; }
  public string EmailFooterHtml_EN { get; set; }
  public string EmailFooterHtml_ES { get; set; }
  public string FaviconUrl { get; set; }
  public string DefaultTheme { get; set; } // "light" or "dark"
}
```

---

## Branding UI Editor

Partners use the `/BrandingEditor.razor` page to:
- Upload logo and favicon
- Preview theme in real-time
- Save primary and accent color via color pickers
- Set HTML footers per language
- Test email templates with new branding

---

## Design Rules

- Logos are shown at a max height of 60px
- Color values must pass contrast validation (accessibility)
- Only verified partners may publish branding

---

## Developer Integration Notes

- Theming is injected via `MudTheme` with partner-specific override
- Email HTML is merged using Razor or string templates
- Live previews are supported with temporary override states

---

© 2025 P4 Software / Grupo Barrdega. All rights reserved.
