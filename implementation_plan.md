# Implementační plán projektu Scio

Tento dokument slouží jako detailní plán a záznam o implementaci aplikace pro realtime sledování pokroku studentů.
**STAV PROJEKTU: DOKONČENO (PRODUKCE)**

## 1. Architektura a Technologie

* **Backend & Frontend:** Blazor Server (.NET 8) - Hostováno na Microsoft IIS (OutOfProcess).
  * *Důvod:* Požadavek na realtime funkcionalitu (SignalR je integrovaný), C# na obou koncích zjednodušuje vývoj.
* **Databáze:** Microsoft SQL Server (Entity Framework Core 8.0)
  * *Strategie:* Prefix `Scio_` pro všechny tabulky (vyhnutí se konfliktům s existujícími tabulkami na sdíleném serveru)
* **Styling:** Vanilla CSS — Dark Premium Theme (glassmorphism, fonty Outfit + Inter, neonové akcenty)
* **AI Integrace:** Google Gemini 1.5 Flash pro sémantickou analýzu textu a vyhodnocování cílů
* **Autentifikace:**
  * Klasická autentifikace (Login/Heslo + BCrypt hashování)
  * Google OAuth 2.0 (pro učitelské účty)
  * RBAC (Role-Based Access Control) na úrovni `[Authorize]` atributů
* **Realtime:** SignalR Hub (`/sciohub`) pro obousměrnou komunikaci
* **Deployment:** Subdirectory `/www/scio/` na sdíleném IIS serveru (windows11.aspone.cz)

## 2. Datový Model (5 tabulek)

> **Poznámka:** Všechny tabulky mají prefix `Scio_` pro oddělení od existujících databázových tabulek.

### Scio_Users (Učitelské účty)

* `Id` (PK, int, auto-increment)
* `Login` (Unique, max 100) - email nebo uživatelské jméno
* `Email` (max 255)
* `Name` (max 100)
* `PasswordHash` (BCrypt, max 500) - "GOOGLE_AUTH" pro Google-only účty
* `GoogleId` (Unique, Nullable, max 255) - partial index (IS NOT NULL)
* `Role` (max 50) - "Teacher" nebo "Admin"
* `CreatedAt` (default: GETUTCDATE())

### Scio_Groups

* `Id` (PK, int, auto-increment)
* `TeacherId` (FK → Scio_Users, Cascade Delete)
* `Name` (max 200, např. "A2 - kvadratické rovnice 1")
* `GoalDescription` (max 2000, textový popis cíle)
* `GoalType` (enum uložený jako string: "Boolean" / "Percentage")
* `TargetValue` (int, např. 3 pro 3 rovnice)
* `CreatedAt` (default: GETUTCDATE())
* `InviteCode` (Unique, max 100 - prvních 8 znaků GUID, uppercase)
* `IsActive` (bool, default: true)

### Scio_Students (Session-based)

* `Id` (PK, int, auto-increment)
* `GroupId` (FK → Scio_Groups, Cascade Delete)
* `Nickname` (max 100)
* `DeviceId` (max 255, uloženo v LocalStorage)
* `JoinedAt` (default: GETUTCDATE())
* `LastActivityAt` (default: GETUTCDATE())
* `Status` (enum jako string: "Active" / "NeedHelp" / "Inactive" / "Completed")
* **Unique index:** (GroupId, DeviceId, Nickname)

### Scio_Messages

* `Id` (PK, int, auto-increment)
* `StudentId` (FK → Scio_Students, Restrict Delete)
* `GroupId` (FK → Scio_Groups, Cascade Delete)
* `Content` (max 5000)
* `Timestamp` (default: GETUTCDATE())
* `IsSystemMessage` (bool, default: false)
* `IsFromTeacher` (bool, default: false)
* `IsProgressContribution` (bool, default: false)
* `AIConfidence` (float?, nullable)
* **Index:** (GroupId, Timestamp) pro rychlé načítání

### Scio_ProgressLogs

* `Id` (PK, int, auto-increment)
* `StudentId` (FK → Scio_Students, 1:1, Cascade Delete, Unique index)
* `CurrentValue` (int, default: 0)
* `TargetValue` (int)
* `Percentage` (computed: CurrentValue / TargetValue * 100)
* `IsCompleted` (bool, default: false)
* `LastUpdatedAt` (default: GETUTCDATE())
* `CompletedAt` (DateTime?, nullable)

### Migrace

1. `20260209123849_InitialCreate` — Všechny tabulky, indexy, vztahy
2. `20260210214005_AddIsFromTeacher` — Přidání `IsFromTeacher` na Message
3. `20260210221427_AllowMultipleStudentsPerDevice` — Úprava unique indexu Students

## 3. Klíčové Funkcionality

### A. Autentifikace (`AuthController.cs` + `AuthService.cs`)

* **Klasický login:** POST `/auth/login` → BCrypt.Verify → Cookie sign-in
* **Google OAuth:** GET `/auth/google-login` → Challenge → Google callback → `/auth/google-response`
  * Automatický merge účtu, pokud existuje email
  * Vytvoření nového účtu, pokud neexistuje
* **Logout:** GET `/auth/logout` → SignOut → Redirect na login
* **Cookies:** HttpOnly, Secure, SameSite=Lax, Path=/scio, 7 dní

### B. Učitelský Dashboard

* **Home.razor (`/`):** Mřížka skupin s gradientními kartami, modal pro vytvoření nové skupiny, QR kódy
* **GroupDetail.razor (`/group/{id}`):** Realtime monitoring — plovoucí karty žáků s progress bary, historie zpráv, odesílání zpráv učitelem, emailové pozvánky

### C. Studentské Rozhraní

* **Entrance.razor (`/vstup/{inviteCode}`):** Vstupní stránka — validace InviteCode, DeviceId z LocalStorage, formulář pro Nickname
* **Chat.razor (`/chat/{groupId}`):** Bublinový chat, AI zpětná vazba pod každou zprávou, progressbar/checkmark nahoře, tlačítko "potřebuji pomoc", hlasové diktování

### D. SignalR Hub (`ScioHub.cs`)

Obousměrná komunikace v reálném čase:

* **Klient → Server:** `SendMessage`, `SendTeacherMessage`, `ApproveMessage`, `UpdateProgress`, `SignalNeedHelp`
* **Server → Klient:** `ReceiveMessage`, `ProgressUpdated`, `StatusChanged`, `MessageApproved`, `UserJoined`

### E. AI Integrace (`GeminiAIService.cs`)

* **API:** Google Gemini 1.5 Flash (REST, JSON response)
* **Timeout:** 15 sekund
* **Prompt:** Systémová instrukce + kontext úkolu + zpráva studenta
* **Výstup:** JSON s `isProgress`, `isRelevant`, `newProgressValue`, `feedback`, `studentFeedback`
* **Fallback:** Pokud AI selže, zpráva je označena jako relevantní a pokrok se nemění

### F. Email Service (`EmailService.cs`)

* **SMTP:** MailKit s TLS (StartTls)
* **Funkce:** `SendEmailAsync` (individuální), `SendBulkInviteAsync` (hromadné)
* **Podpora:** Vícenásobní příjemci (oddělovač `;`), HTML šablona s přímým odkazem

## 4. NuGet Balíčky

| Balíček | Verze | Účel |
|---|---|---|
| BCrypt.Net-Next | 4.0.3 | Hashování hesel |
| MailKit | 4.14.1 | SMTP emailing |
| Microsoft.AspNetCore.Authentication.Google | 8.0.0 | Google OAuth |
| Microsoft.AspNetCore.SignalR.Client | 8.0.0 | SignalR klient (pro Blazor komponenty) |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.0 | SQL Server provider |
| Microsoft.EntityFrameworkCore.Design | 8.0.0 | Migrace (dev-only) |
| Microsoft.EntityFrameworkCore.Tools | 8.0.0 | CLI nástroje (dev-only) |

## 5. Middleware Pipeline (`Program.cs`)

```
ForwardedHeaders (X-Forwarded-For, X-Forwarded-Proto)
  ↓
PathBase("/scio")
  ↓
DeveloperExceptionPage
  ↓
HSTS (production only)
  ↓
HttpsRedirection
  ↓
StaticFiles
  ↓
Routing
  ↓
Authentication
  ↓
Authorization
  ↓
Antiforgery
  ↓
MapControllers + MapHub + MapRazorComponents
```

## 6. Aktuální Konfigurace

* **URL:** `https://www.ekobio.org/scio/`
* **Hosting:** Aspone (Windows IIS, OutOfProcess)
* **DB Server:** sql8.aspone.cz
* **Autentifikace:** Google OAuth 2.0 + klasické heslo (BCrypt)
* **AI:** Google Gemini 1.5 Flash (API klíč v `appsettings.json`)
* **Realtime:** SignalR Hub (`/sciohub`)
* **HTTPS:** Vynuceno (`app.UseHttpsRedirection()`)

## 7. Bonusy (implementováno)

* [x] Podpora Markdown/LaTeX v chatu (MathJax)
* [x] Speech-to-Text (Web Speech API) pro diktování
* [x] Emailové pozvánky s přímým odkazem
* [x] Detail studenta s klíčovými zprávami
* [x] Schvalování pokroku učitelem
* [x] Cy Ber-Edu futuristický design (Glassmorphism + Neon)
