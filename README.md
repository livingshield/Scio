# Scio - Realtime Student Progress Monitoring

Aplikace pro učitele umožňující sledovat pokrok studentů v reálném čase pomocí AI analýzy chatu.

## 🚀 Živá aplikace

**URL:** [https://www.ekobio.org/scio/](https://www.ekobio.org/scio/)

## 📖 O projektu

Scio je webová aplikace pro realtime sledování vzdělávacího pokroku. Učitel vytvoří skupinu s cílem, studenti se připojí přes QR kód a komunikují prostřednictvím chatu. Každá zpráva je analyzována AI (Google Gemini), které vyhodnocuje relevanci a pokrok studenta k cíli.

### Klíčové funkce

* **Učitel:** Dashboard skupin, realtime monitoring žáků, schvalování pokroku, emailové pozvánky, QR kódy
* **Student:** Chat s AI zpětnou vazbou, progressbar/checkmark cílů, diktování hlasem, žádost o pomoc
* **AI:** Sémantická analýza zpráv, detekce pokroku, povzbudivá zpětná vazba

## 🛠 Technologie

| Technologie | Účel |
|---|---|
| Blazor Server (.NET 8) | Frontend + Backend |
| Entity Framework Core 8.0 | ORM + SQL Server |
| SignalR | Realtime (WebSocket) |
| Google OAuth 2.0 | Přihlášení učitelů |
| BCrypt.Net | Hashování hesel |
| MailKit | SMTP pozvánky (TLS) |
| Google Gemini 1.5 Flash | AI analýza zpráv |
| MathJax | Matematické výrazy |
| Web Speech API | Speech-to-Text |

## 📂 Struktura projektu

```
ScioApp/
├── Components/Pages/     # Blazor stránky (Home, GroupDetail, Chat, Login, Register, Entrance)
├── Components/Layout/    # MainLayout + NavMenu
├── Controllers/          # AuthController (Login, Google OAuth, Logout)
├── Data/                 # ScioDbContext (5 tabulek s prefixem Scio_)
├── Hubs/                 # ScioHub (SignalR - chat, AI, pokrok)
├── Models/               # User, Group, Student, Message, ProgressLog
├── Services/             # AIService, AuthService, EmailService, GroupService, StudentService
├── Migrations/           # 3 EF Core migrace
├── wwwroot/              # app.css (dark theme, glassmorphism), favicon
├── Program.cs            # Hlavní konfigurace (Auth, DB, SignalR, Middleware)
└── ScioApp.csproj        # .NET 8, OutOfProcess
```

## 🏃 Lokální spuštění

1. Naklonujte repozitář
2. Vytvořte `appsettings.json` podle šablony `appsettings.Template.json`
3. Vyplňte connection string, Google OAuth credentials a Gemini API klíč
4. Spusťte:

   ```bash
   dotnet restore
   dotnet run
   ```

## 🚀 Nasazení na server

1. Vytvořte `full_deploy.py` podle šablony `full_deploy.Template.py` (FTP údaje)
2. Spusťte:

   ```bash
   dotnet publish -c Release -o publish
   python full_deploy.py
   ```

## ⚠️ Specifika hostingu (Aspone/IIS)

* Sdílený App Pool s jinou aplikací → používáme `OutOfProcess` hosting model
* Aplikace běží v podsložce `/scio` → `PathBase("/scio")` v `Program.cs`
* DLL soubory je nutné před přepsáním přejmenovat na `.bak` (IIS lock)
* Po změnách v `Program.cs` / `web.config` → **recyklovat App Pool** v panelu Aspone
* Cookies mají explicitní `Path="/scio"` pro izolaci od root aplikace
* ForwardedHeaders s vyčištěnými KnownNetworks (důvěra lokální IIS proxy)

## 📝 Dokumentace

* `PROJEKTOVA_DOKUMENTACE.md` — Kompletní projektová dokumentace
* `implementation_plan.md` — Implementační plán a datový model
* `zadani.md` — Originální zadání
