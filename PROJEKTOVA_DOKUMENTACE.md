# Projektová Dokumentace: Scio

**Inteligentní systém pro realtime sledování vzdělávacího pokroku**

---

## 1. Představení projektu a našeho řešení

Aplikace **Scio** byla vyvinuta jako moderní nástroj pro zefektivnění interakce mezi učitelem a studentem během samostatné práce. Naše řešení se zaměřuje na odstranění komunikační bariéry v digitální i prezenční výuce.

### Hlavní přínos řešení

* **Transparentnost procesu:** Učitel okamžitě vidí, kdo na úkolu pracuje a kdo stagnuje, aniž by musel obcházet každého studenta.
* **AI asistence:** Analýza pokroku neběží až po odevzdání, ale *kontinuálně* během práce, což umožňuje včasnou intervenci.
* **Jednoduchost nasazení:** Student nepotřebuje žádný účet, stačí mu naskenovat QR kód nebo kliknout na odkaz z emailu.

**Ostrá verze aplikace je dostupná na adrese:** [https://www.ekobio.org/scio/](https://www.ekobio.org/scio/)

---

## 2. Klíčové funkcionality

### 👨‍🏫 Pro Učitele

* **Správa výukových skupin:** Vytváření lekcí se specifickými cíli (checkmark i procentuální).
* **Realtime Dashboard (`GroupDetail.razor`):** Přehled aktivních studentů, jejich aktuálního stavu a procentuálního pokroku. Taktický monitoring s živými kartami žáků.
* **Emailové pozvánky:** Hromadné odeslání pozvánek přímo z dashboardu (SMTP/TLS přes MailKit).
* **QR kódy:** Automaticky generované pro každou skupinu.
* **Schvalování pokroku:** Učitel může schválit zprávy studentů, které AI označilo jako příspěvek k cíli (`ApproveMessage` v `ScioHub`).
* **Interaktivní monitoring:** Odpovídání na dotazy studentů přímo v dashboardu, označování "potřebuje pomoc" jako vyřešené.

### 🎓 Pro Studenty

* **Okamžitý vstup:** Přes QR kód nebo unikátní URL (`/vstup/{inviteCode}`).
* **Interaktivní Chat (`Chat.razor`):** Bublinový design s micro-animacemi, vizuální zvýraznění zpráv relevantních k cíli (zelený rámeček).
* **AI zpětná vazba:** Každá zpráva je v reálném čase analyzována AI (Gemini). Student obdrží povzbudivou zpětnou vazbu a varování při nerelevantních zprávách.
* **Vizualizace pokroku:** Progressbar pro procentuální cíle, checkmark pro boolean cíle.
* **Hlasové vstupy:** Speech-to-Text pro diktování odpovědí (Web Speech API).
* **Žádost o pomoc:** Tlačítko pro diskrétní signalizaci učiteli (`SignalNeedHelp` v `ScioHub`).

### 🤖 AI Integrace

* **Motor:** Google Gemini 1.5 Flash (`GeminiAIService.cs`)
* **Vstup:** Zpráva studenta + kontext cíle skupiny + aktuální pokrok
* **Výstup (JSON):** `isProgress`, `isRelevant`, `newProgressValue`, `feedback` (pro učitele), `studentFeedback` (pro studenta)
* **Systémový prompt:** Empatický asistent rozpoznávající snahu studenta, tolerantní k formátu (matematika i kreativní úkoly).

---

## 3. Architektura aplikace

### Struktura projektu

```
ScioApp/
├── Components/
│   ├── Pages/
│   │   ├── Home.razor          # Dashboard učitele (seznam skupin) [Authorize]
│   │   ├── GroupDetail.razor    # Realtime monitoring skupiny [Authorize]
│   │   ├── Chat.razor           # Studentský chat [Authorize]
│   │   ├── Entrance.razor       # Vstup studenta přes QR kód
│   │   ├── Login.razor          # Přihlášení (InteractiveServer)
│   │   ├── Register.razor       # Registrace učitele
│   │   └── Error.razor          # Chybová stránka
│   ├── Layout/
│   │   ├── MainLayout.razor     # Hlavní layout s navigací
│   │   └── NavMenu.razor        # Navigační menu
│   ├── App.razor                # Root komponent (HTML, meta, fonty)
│   └── Routes.razor             # Routování
├── Controllers/
│   └── AuthController.cs        # Login, Google OAuth, Logout (MVC)
├── Data/
│   └── ScioDbContext.cs          # EF Core kontext (5 tabulek s prefixem Scio_)
├── Hubs/
│   └── ScioHub.cs                # SignalR Hub (chat, AI analýza, pokrok)
├── Models/
│   ├── User.cs                  # Učitelský účet
│   ├── Group.cs                 # Skupina s cílem (Boolean/Percentage)
│   ├── Student.cs               # Student (session-based, DeviceId)
│   ├── Message.cs               # Chatová zpráva
│   ├── ProgressLog.cs           # Pokrok studenta
│   └── AuthRequests.cs          # DTO pro autentifikaci
├── Services/
│   ├── AIService.cs             # Gemini AI integrace
│   ├── AuthService.cs           # Login, registrace, Google merge
│   ├── EmailService.cs          # SMTP pozvánky (MailKit)
│   ├── GroupService.cs          # CRUD skupin
│   └── StudentService.cs       # Join flow, validace
├── Migrations/                  # 3 EF Core migrace
├── wwwroot/
│   └── app.css                  # Design systém (CSS proměnné, glassmorphism)
├── Program.cs                   # Hlavní konfigurace aplikace
└── ScioApp.csproj               # Projekt (.NET 8, OutOfProcess)
```

### Datový model (5 tabulek)

| Tabulka | Popis | Klíčové atributy |
|---|---|---|
| `Scio_Users` | Učitelské účty | Login (unique), Email, PasswordHash (BCrypt), GoogleId (nullable), Role |
| `Scio_Groups` | Skupiny s cíli | TeacherId (FK), Name, GoalDescription, GoalType (Boolean/Percentage), InviteCode (unique) |
| `Scio_Students` | Studenti per session | GroupId (FK), Nickname, DeviceId, Status (Active/NeedHelp/Inactive/Completed) |
| `Scio_Messages` | Chat zprávy | StudentId (FK), GroupId (FK), Content, IsFromTeacher, IsProgressContribution |
| `Scio_ProgressLogs` | Pokrok studentů | StudentId (FK, 1:1), CurrentValue, TargetValue, IsCompleted |

### Realtime komunikace (SignalR Hub)

`ScioHub.cs` poskytuje 6 metod:

| Metoda | Popis |
|---|---|
| `JoinGroup` | Připojení do SignalR skupiny |
| `SendMessage` | Odeslání zprávy + AI analýza |
| `SendTeacherMessage` | Přímá zpráva od učitele studentovi |
| `ApproveMessage` | Schválení zprávy jako příspěvku k pokroku |
| `UpdateProgress` | Manuální update progresu |
| `SignalNeedHelp` | Přepnutí stavu "potřebuje pomoc" |

---

## 4. Technologie a závislosti

| Technologie | Verze | Účel |
|---|---|---|
| **Blazor Server** | .NET 8 | Frontend + Backend (SSR + InteractiveServer) |
| **Entity Framework Core** | 8.0 | ORM, migrace, SQL Server |
| **SignalR** | 8.0 | Realtime komunikace (WebSocket) |
| **Google OAuth 2.0** | 8.0 | Přihlášení učitelů přes Google |
| **BCrypt.Net-Next** | 4.0.3 | Hashování hesel |
| **MailKit** | 4.14.1 | SMTP odesílání emailů (TLS) |
| **Gemini AI** | 1.5 Flash | Sémantická analýza zpráv studentů |
| **Bootstrap** | 5.x | CSS framework (grid, utility classes) |
| **Bootstrap Icons** | 1.11 | Ikony |
| **MathJax** | 3.x | Rendering matematických výrazů |
| **Web Speech API** | - | Speech-to-Text (diktování) |

### Design systém

* **Styl:** Dark Premium / Glassmorphism
* **Fonty:** Outfit + Inter (Google Fonts)
* **Barvy:** Neon Cyan (`#00d2ff`), Electric Purple (`#a855f7`), Deep Navy (`#03050a`)
* **Efekty:** Radiální gradienty na pozadí, průhledné skleněné karty, micro-animace

---

## 5. Autentifikace a autorizace

### Dvě metody přihlášení

1. **Klasické heslo:** Login + Password → BCrypt verifikace → Cookie
2. **Google OAuth 2.0:** Google Login → Callback → Merge s existujícím účtem (pokud existuje email) nebo vytvoření nového

### Konfigurace bezpečnosti (`Program.cs`)

* **Cookie:** `Scio_Auth`, HttpOnly, Secure, SameSite=Lax, Path=`/scio`, Expirace 7 dní
* **Antiforgery:** Separátní cookie `Scio_Antiforgery`
* **DataProtection:** Klíče uloženy v `App_Data/Keys/`
* **ForwardedHeaders:** Trust pro lokální IIS reverse proxy (vyčištěné KnownNetworks)
* **Autorizace:** `[Authorize]` atribut na `Home.razor`, `GroupDetail.razor`, `Chat.razor`

---

## 6. Infrastruktura a Hosting

| Parametr | Hodnota |
|---|---|
| **Server** | windows11.aspone.cz |
| **Web Server** | IIS (Internet Information Services) |
| **Hosting Mode** | OutOfProcess (sdílený App Pool s KikiAI) |
| **PathBase** | `/scio` |
| **Databáze** | MS SQL Server na sql8.aspone.cz |
| **SSL** | HTTPS (vynuceno) |
| **Deploy** | `dotnet publish` + FTP skript (`full_deploy.py`) |

### Důležité provozní poznámky

* Před nahráním DLL/EXE je nutné soubory přejmenovat na `.bak` (IIS lock).
* Po změnách v `Program.cs` nebo `web.config` je nutné v panelu Aspone **recyklovat App Pool**.
* `web.config` musí mít `hostingModel="OutOfProcess"`.

---

## 7. Splnění požadavků ze zadání

### Základní funkcionalita ✅

* [x] Registrace / přihlášení Google účtem, RBAC
* [x] Založení nové skupiny (název, popis cíle) a tabulkový přehled
* [x] QR kód pro vstup, omezení jednoho zařízení (LocalStorage)
* [x] Zadání nicku po vstupu
* [x] Textový chat, uvítací zpráva s cílem
* [x] Cíle typu "splněno/nesplněno" (checkmark) i "splněno %" (progressbar)
* [x] Systém navádí studenta, varování při nerelevantních zprávách
* [x] Upozornění učitele na neaktivitu
* [x] Zvýraznění zpráv řešících cíl (zelený rámeček)
* [x] Realtime sledování pokroku s indikátorem "potřebuje pomoc"

### Bonusové výzvy ✅

* [x] Detail studenta s klíčovými zprávami
* [x] Podpora matematických výrazů (LaTeX/MathJax)
* [x] Podpora ukázek kódu v chatu
* [x] Hlasové diktování (Speech-to-Text)

### Navíc (nad rámec) ✅

* [x] Hromadné emailové pozvánky s přímým odkazem
* [x] Cyber-Edu futuristický design (Glassmorphism + Neon)
* [x] Schvalování zpráv učitelem (manuelní kontrola pokroku)
* [x] Zprávy od učitele přímo studentovi v chatu

---

## 8. Použité nástroje

Při tvorbě řešení byly použity následující AI nástroje:

* **Google Gemini (Antigravity / Jules):** Plánování architektury, generování komponent, debugging nasazení, CSS design systém, dokumentace.
* **Cursor / Copilot:** Asistence při psaní kódu a autokompletace.

> Všechny vygenerované části byly revidovány, testovány a přizpůsobeny konkrétním potřebám projektu.

---

## 9. Zdrojové kódy

Kompletní zdrojové kódy projektu jsou dostupné na GitHubu:

* **URL:** [https://github.com/lordkikin/ScioApp](https://github.com/lordkikin/ScioApp)

---

**Datum odevzdání:** 11. 2. 2026
**Zpracoval:** Tým Scio (EkoBio integrace)
