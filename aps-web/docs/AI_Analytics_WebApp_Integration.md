# AI Analytics — WebApp (Blazor) Integration Reference

This document describes how the AI Analytics feature works within the PlanetTogether WebApp (Blazor Server). It covers menu visibility, access control, and the embed token handshake used to authenticate users into the AI Analytics iframe app.

---

## 1. Menu Visibility

The "AI Analytics" menu item is added in `NavigationStateService.cs`. It appears in the left navigation menu **only** if the logged-in user has a role named `AI_Analytics` scoped to their company.

**Role check:**
```csharp
User.Roles.Any(r => r.Name == "AI_Analytics" && r.CompanyId == User.CompanyId)
```

- The role `AI_Analytics` must exist in the `dbo.Role` table with the user's `CompanyId`.
- The user must be assigned that role via the `dbo.UserRole` join table (columns: `UsersId`, `RoleId`).
- The menu item appears after the "KB AI Chatbot" item.
- Menu item properties: Text = "AI Analytics", URL = "ai_analytics", Icon = "fa fa-chart-line".

If the user does not have this role, the menu item is not rendered at all.

---

## 2. Page Access Control

The Blazor page is at route `/ai_analytics` (`AIAnalytics.razor`).

- The page has `[Authorize]`, so unauthenticated users are redirected to login.
- On load, it checks `BlockUsersByAuthStatus` (standard WebApp auth pattern).
- It then checks for the `AI_Analytics` role (same check as the menu).
- If the user lacks the role, the page shows an "Access Denied" message instead of the iframe.

Additionally, the page checks for the `Company Admin` role:
```csharp
var isCompanyAdmin = User.Roles.Any(r => r.Name == "Company Admin" && r.CompanyId == User.CompanyId);
```
This flag is passed to the iframe app via the embed token (see below). The iframe app uses it to control access to its admin features.

---

## 3. Embed Token (JWT)

The WebApp does **not** pass Auth0 tokens to the iframe. Instead, it generates a short-lived, purpose-built JWT called an "embed token" and sends it via `postMessage`.

### Token Generation (EmbedTokenService)

| Property | Value |
|----------|-------|
| Algorithm | HMAC-SHA256 (symmetric) |
| Signing Key | `EMBED_TOKEN_SECRET` environment variable (or Azure Key Vault secret `EMBED-TOKEN-SECRET`) |
| Issuer (`iss`) | `PlanetTogether.WebApp` |
| Audience (`aud`) | `PlanetTogether.EmbedApp` |
| Expiry (`exp`) | 5 minutes from creation |
| Not Before (`nbf`) | 30 seconds before creation (clock skew allowance) |
| JTI (`jti`) | Random GUID (prevents replay) |

### Claims

| Claim | Type | Description |
|-------|------|-------------|
| `email` | string | User's email from Auth0 claims |
| `companyId` | int (as string) | User's CompanyId from the WebApp database |
| `hasAIAnalyticsRole` | "true"/"false" | Whether the user has the AI_Analytics role |
| `isCompanyAdmin` | "true"/"false" | Whether the user has the Company Admin role |

### Secret Management

The `EMBED_TOKEN_SECRET` is a shared symmetric key. Both the WebApp (token creator) and the iframe app (token verifier) must have the same value.

- **Replit/Dev**: Stored as an environment variable `EMBED_TOKEN_SECRET`
- **Azure/Production**: Stored in Azure Key Vault as `EMBED-TOKEN-SECRET`. The WebApp reads it via `IConfiguration` (Key Vault is registered as a configuration source). Azure Key Vault does not allow underscores in secret names, so hyphens are used; ASP.NET Core's Key Vault provider maps them automatically.

The `EmbedTokenService` reads the secret with this fallback chain:
1. `IConfiguration["EMBED_TOKEN_SECRET"]` (covers appsettings.json, env vars, Key Vault)
2. `Environment.GetEnvironmentVariable("EMBED_TOKEN_SECRET")` (fallback)
3. Throws `InvalidOperationException` if neither is set

---

## 4. postMessage Handshake

The WebApp and iframe app communicate via the browser `postMessage` API. The handshake ensures the iframe is ready before credentials are sent.

### Sequence

```
Iframe App                          WebApp (parent)
    |                                     |
    |--- PT.EMBED.READY --------------->>|   (iframe signals it is loaded and ready)
    |                                     |
    |<<--- PT.EMBED.AUTH -----------------|   (parent sends embed token + theme)
    |                                     |
```

### Step 1: Iframe sends READY signal

The iframe app posts a message when it is ready to receive authentication:
```json
{ "type": "PT.EMBED.READY" }
```

### Step 2: WebApp sends AUTH payload

The WebApp listens for the READY signal, validates the message origin, then responds:
```json
{
  "type": "PT.EMBED.AUTH",
  "version": 1,
  "payload": {
    "embedToken": "<JWT string>",
    "ui": {
      "theme": "dark" | "light"
    }
  }
}
```

### Security

- The WebApp validates `event.origin` matches `https://query-insight-hvdvbpaugearfba2.westus2-01.azurewebsites.net` before processing any message.
- The WebApp sends the response with an explicit `targetOrigin` (not `"*"`).
- The listener is one-shot: it removes itself after sending the auth payload.
- The listener is cleaned up on page dispose (via `IAsyncDisposable`).
- The theme value (`"dark"` or `"light"`) comes from the WebApp's `IDarkModeService`.

---

## 5. Iframe App URL

The iframe app URL is configured via the `AIAnalyticsUrl` setting in `appsettings.json` (or as an Azure App Setting / environment variable). This allows the URL to differ between dev and production without code changes.

Current dev value:
```
https://aianalytics-ccbba9feguhgbxa2.westus2-01.azurewebsites.net
```

`AIAnalytics.razor` reads this at runtime and uses it as both `IframeUrl` (the iframe `src`) and `IframeOrigin` (for postMessage origin validation).

---

## 6. What the Iframe App Must Do

The iframe app (separate codebase) is responsible for:

1. Posting `PT.EMBED.READY` when loaded.
2. Listening for `PT.EMBED.AUTH` messages, validating the parent origin.
3. Sending the received `embedToken` to its own backend: `POST /api/session/from-embed`.
4. Backend validates the JWT signature, `exp`, `iss`, `aud`, and extracts claims.
5. Backend creates a server-side session (HttpOnly Secure cookie). All subsequent requests use this session.
6. Applying the `ui.theme` preference to its UI.

---

## 7. Database Roles (pt_webapp_dev)

### Relevant Tables

| Table | Purpose |
|-------|---------|
| `dbo.Role` | Role definitions (Id, Name, CompanyId) |
| `dbo.UserRole` | Join table (UsersId, RoleId) |
| `dbo.User` | User records (Id, Email, CompanyId) |

### Relevant Role Names

| Role Name | Purpose |
|-----------|---------|
| `AI_Analytics` | Gates access to the AI Analytics menu item and page |
| `Company Admin` | Grants admin privileges in the iframe app |

Both roles are company-scoped (each company has its own instance of these roles with a unique RoleId).

---

## 8. Files in this Repo

| File | Purpose |
|------|---------|
| `aps-web/WebApps/Pages/AIAnalytics.razor` | Blazor page: role check, iframe rendering, JS interop |
| `aps-web/WebApps/Services/EmbedTokenService.cs` | JWT generation (HMAC-SHA256, 5-min expiry) |
| `aps-web/WebApps/Services/IEmbedTokenService.cs` | Interface for EmbedTokenService |
| `aps-web/WebApps/wwwroot/js/aianalytics.js` | postMessage handshake (listen for READY, send AUTH) |
| `aps-web/ReportsWebApp.DB/Services/NavigationStateService.cs` | Menu item (AI_Analytics role gate) |
| `aps-web/WebApps/Program.cs` | DI registration (EmbedTokenService as Singleton) |
