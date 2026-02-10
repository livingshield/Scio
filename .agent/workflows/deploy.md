---
description: Jak nasadit aplikaci na produkční server (FTP)
---

Tento projekt vyžaduje specifický postup nahrávání kvůli zámkům souborů na IIS a sdílenému hostingu.

// turbo

1. Spusťte build (publish) aplikace:
   `dotnet publish -c Release -o ./publish -f net8.0 -r win-x64 --self-contained true /p:PublishSingleFile=false`

2. (Volitelné) Pokud jste změnili `web.config`, ujistěte se, že `hostingModel` je nastaven na `OutOfProcess`.

3. Spusťte nasazovací skript, který vyřeší zámky souborů přejmenováním:
   `python full_deploy.py`

4. Po dokončení nahrávání **BĚŽTE DO ADMINISTRACE ASPONE** a u domény `ekobio.org` klikněte na tlačítko **Recyklovat Pool**. Bez tohoto kroku se nové změny (zejména v DLL) nemusí projevit.
