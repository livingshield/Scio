# Scio - Realtime Student Progress Monitoring

Aplikace pro učitele umožňující sledovat pokrok studentů v reálném čase pomocí AI analýzy chatu.

## 🚀 Rychlý přístup

* **Produkční URL:** `https://www.ekobio.org/scio/`
* **Přihlášení učitele:** Google Auth nebo klasický login/heslo.
* **Studenti:** Připojují se přes `/scio/join/{inviteCode}` (nebo naskenováním QR kódu z dashboardu učitele).

## 🛠 Technologie

* **Framework:** Blazor Server (.NET 8)
* **Komunikace:** SignalR (realtime aktualizace)
* **Databáze:** MS SQL Server (Azure/Aspone)
* **AI:** OpenAI GPT-4o-mini (vyhodnocování zpráv studentů)
* **Hosting:** Aspone (IIS), režim OutOfProcess v podadresáři `/scio`.

## 📂 Důležité soubory pro vývojáře

* `Program.cs`: Hlavní konfigurace (HTTPS, Auth, SignalR, PathBase).
* `Hubs/ScioHub.cs`: Logika realtime chatu a spouštění AI analýzy.
* `Services/AIService.cs`: Implementace propojení s OpenAI.
* **Důležité:** Pro lokální spuštění si vytvořte `appsettings.json` podle šablony `appsettings.Template.json`.
* **Nasazení:** Použijte `full_deploy.Template.py` jako základ pro svůj nasazovací skript (lokální soubory s hesly jsou ignorovány v `.gitignore`).

## ⚠️ Známá specifika hostingu (Aspone)

* Aplikace běží ve sdíleném App Poolu s hlavní aplikací KikiAI.
* **VŽDY** musí být v `web.config` nastaven `hostingModel="OutOfProcess"`.
* Při nahrávání DLL/EXE je nutné soubory na serveru nejprve přejmenovat (např. na `.bak`), jinak je IIS nepustí přepsat (řešeno v `full_deploy.py`).
* Po každé změně v `Program.cs` nebo `web.config` je nutné v panelu Aspone kliknout na **"Recyklovat Pool"**.

## 📝 Historie změn

Všechny kroky implementace jsou detailně popsány v `implementation_plan.md`.
