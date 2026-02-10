# Implementační plán projektu Scio

Tento dokument slouží jako detailní plán a záznam o implementaci aplikace pro realtime sledování pokroku studentů.
**STAV PROJEKTU: DOKONČENO (PRODUKCE)**

## 1. Architektura a Technologie

* **Backend & Frontend:** Blazor Server (.NET 8/9) - Hostováno na Microsoft Server (IIS).
  * *Důvod:* Požadavek na realtime funkcionalitu (SignalR je integrovaný), C# na obou koncích zjednodušuje vývoj.
* **Databáze:** Microsoft SQL Server (Entity Framework Core)
  * *Strategie:* Prefix `Scio_` pro všechny tabulky (vyhnutí se konfliktům s existujícími tabulkami)
* **Styling:** Tailwind CSS (pro rychlý a moderní design) + Sass (pro specifické úpravy)
* **AI Integrace:** OpenAI API / Azure OpenAI (nebo lokální model, pokud bude k dispozici) pro analýzu textu a vyhodnocování cílů.
* **Autentifikace:**
  * *Fáze 1:* Klasická autentifikace (Login/Heslo + BCrypt)
  * *Fáze 2:* Google Auth (OAuth 2.0) - přidá se po zprovoznění HTTPS
  * RBAC (Role-Based Access Control) na úrovni aplikace
* **Deployment:** Subdirectory `/www/scio/` na sdíleném IIS serveru
  * *Důvod:* Server již hostuje jinou aplikaci (KikiAI) v `/www/`

## 2. Datový Model (ER Diagram náčrt)

> **Poznámka:** Všechny tabulky mají prefix `Scio_` pro oddělení od existujících databázových tabulek.

### Scio_Users (Učitelské účty)

* `Id` (PK)
* `Login` (Unique) - email nebo uživatelské jméno
* `Email`
* `Name`
* `PasswordHash` (BCrypt)
* `GoogleId` (Unique, Nullable) - pro budoucí OAuth integraci
* `Role` (enum: Teacher / Admin)
* `CreatedAt`

### Scio_Groups

* `Id` (PK)
* `TeacherId` (FK -> Scio_Users)
* `Name` (např. "A2 - kvadratické rovnice 1")
* `GoalDescription` (např. "vyřeši samostatně 3 různé kvadratické rovnice...")
* `GoalType` (enum: Boolean / Percentage)
* `TargetValue` (např. 3 pro 3 rovnice, nebo 100 pro procenta)
* `CreatedAt`
* `InviteCode` (GUID pro QR kód)
* `IsActive` (bool - aktivní/archivovaná skupina)

### Scio_Students (Session-based)

* `Id` (PK)
* `GroupId` (FK -> Scio_Groups)
* `Nickname`
* `DeviceId` (Unique per group - uloženo v LocalStorage a ověřováno)
* `JoinedAt`
* `LastActivityAt` (pro detekci neaktivity)
* `Status` (enum: Active / NeedHelp / Inactive / Completed)

### Scio_Messages

* `Id` (PK)
* `StudentId` (FK -> Scio_Students)
* `GroupId` (FK -> Scio_Groups)
* `Content` (text zprávy)
* `Timestamp`
* `IsSystemMessage` (bool - oznámení, varování)
* `IsProgressContribution` (bool - AI vyhodnotilo jako příspěvek k cíli -> zvýraznění)
* `AIConfidence` (float 0-1, confidence skóre AI analýzy)

### Scio_ProgressLogs

* `Id` (PK)
* `StudentId` (FK -> Scio_Students)
* `CurrentValue` (int - aktuální hodnota progresu)
* `TargetValue` (int - cílová hodnota z Groups)
* `Percentage` (computed - CurrentValue / TargetValue * 100)
* `IsCompleted` (bool)
* `LastUpdatedAt`
* `CompletedAt` (nullable - čas dokončení)

## 3. Klíčové Funkcionality a Implementace

### A. Autentifikace a RBAC

**Fáze 1 (bez HTTPS):**

* Klasická autentifikace: Login + Password (BCrypt hashing)
* Cookie-based sessions
* Registrační formulář pro učitele
* Login stránka s validací

**Fáze 2 (po zprovoznění HTTPS):**

* Implementace Google OAuth 2.0
* Propojení existujících účtů s Google ID
* Dual authentication (Google OR klasický login)

**RBAC:**

* Vytvoření `BaseComponent` nebo Services pro kontrolu rolí
* Authorization policies pro Teacher/Admin role

### B. Učitelský Dashboard (Teacher View)

* **Seznam skupin:** Tabulka s přehledem, tlačítko "Vytvořit skupinu".
* **Vytvoření skupiny:** Modální okno / stránka s formulářem (Název, Cíl, Typ cíle).
* **Detail skupiny (Realtime):**
  * Využití Blazor `HubConnection` (SignalR).
  * Grid zobrazení studentů.
  * Každá karta studenta ukazuje: Nick, Progress Bar, Status indikátor.
  * Možnost označit "NeedHelp" jako vyřešené.
  * (Bonus) Rozklik detailu studenta -> načtení historie zpráv.

### C. Studentské Rozhraní (Student View)

* **Vstup:** Scan QR kódu -> URL `/join/{inviteCode}`.
* **Validace:** Kontrola LocalStorage. Pokud existuje token pro tuto skupinu, rovnou přihlásí. Pokud ne, formulář pro Nickname -> uložení tokenu.
* **Chat:**
  * Jednoduché chatovací rozhraní (příchozí/odchozí bubliny).
  * Nahoře fixní lišta s popisem cíle a aktuálním progresem.
* **Logika:**
  * Odeslání zprávy -> SignalR event na server.
  * Server uloží zprávu.
  * **AI Background Task:**
    * Server pošle zprávu (a kontext) do AI služby.
    * Prompt: "Analyzuj, zda tato zpráva posouvá studenta k cíli: {GoalDescription}. Odpověz JSON: { isProgress: bool, newProgressValue: int, feedback: string }."
    * Aktualizace `ProgressLogs` a `Messages` (IsProgressContribution).
    * Odeslání aktualizace klientům (Student i Učitel) přes SignalR.
  * **System Watchdog:**
    * Timer na serveru/klientovi sleduje neaktivitu.
    * Pokud student nepíše X minut -> Varování do chatu.
    * Pokud stále nic -> Změna statusu na `Inactive` -> notifikace učiteli.

### D. AI Integrace

* Vytvoření `IAIService` rozhraní.
* Implementace `OpenAIService` (nebo jiné).
* System prompt inženýrství pro správné vyhodnocování pokroku dle volného popisu cíle.

## 4. Postup Implementace (Kroky)

### Fáze 1: Základní funkcionalita (bez HTTPS)

1. **Setup:**
   * Založení Blazor Server projektu
   * Konfigurace EF Core s `Scio_` table prefix
   * Vytvoření všech entity tříd a DbContext
   * První migrace a aplikace na produkční DB

2. **Auth:**
   * Implementace klasické autentifikace (Login/Password)
   * Registrace učitelů
   * Cookie-based authentication
   * Authorization policies

3. **Teacher Core:**
   * CRUD pro Skupiny (Create, Read, Update, Delete)
   * Generování QR kódů s InviteCode
   * Seznam skupin s přehledem

4. **Student Core:**
   * Join flow přes QR kód/InviteCode
   * LocalStorage handling (DeviceId)
   * Nickname validace a ukládání
   * Základní Chat UI

5. **Realtime:**
   * SignalR Hub pro chat
   * Broadcast zpráv ve skupině
   * Online status tracking

6. **AI Logic:**
   * Implementace AI služby (OpenAI API)
   * Prompt engineering pro vyhodnocování zpráv
   * Update progresu na základě AI analýzy
   * Zvýraznění zpráv přispívajících k cíli

7. **Dashboard:**
   * Realtime zobrazení studentů
   * Progress bars/checkmarks
   * Status indikátory (Active/NeedHelp/Inactive)
   * Notifikace pro učitele

8. **Polish:**
   * Tailwind CSS styling
   * Responsivní design
   * Loading states a error handling
   * Watchdog timer pro neaktivitu

9. **Deployment:**
   * Konfigurace pro subdirectory `/www/scio/`
   * FTP upload
   * IIS sub-application setup (web.config)
   * Testování SignalR konektivity

### Fáze 2: Google OAuth (DOKONČENO)

1. **Google Auth:**
    * Konfigurace Google Cloud Console (Aktivní)
    * Implementace OAuth 2.0 flow (Dokončeno)
    * Propojení s existujícími účty (Dokončeno)
    * Dual authentication support (Dokončeno)

## 6. Aktuální Konfigurace a Provoz

* **URL:** `https://www.ekobio.org/scio/`
* **Hosting:** Aspone (Windows IIS)
  * Režim: `OutOfProcess` (sdílený App Pool s KikiAI)
  * Podsložka: `/www/scio/`
* **Autentifikace:**
  * Google OAuth 2.0 (ClientId: 335921-...)
  * Klasické heslo (BCrypt)
* **AI:** OpenAI GPT-4o-mini (vyžaduje API klíč v `appsettings.json`)
* **Realtime:** SignalR Hub (`/sciohub`)
* **HTTPS:** Vynuceno (`app.UseHttpsRedirection()`)

## 5. Bonusy (pokud zbude čas)

* Podpora Markdown/LaTeX v chatu.
* Speech-to-Text (Web Speech API) pro diktování.
* Export dat pro učitele.
