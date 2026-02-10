---
description: Konfigurace a správa Google přihlášení
---

Google přihlášení je v aplikaci integrováno a vyžaduje správné nastavení v `appsettings.json` a v Google Cloud Console.

### Kde najít údaje

* **Kláče v aplikaci:** Soubor `appsettings.json` -> sekce `Authentication:Google`.
* **Google Cloud Console Projekt:** `gen-lang-client-0373211036` (nebo pod e-mailem <dancakytyrova@gmail.com>).

### Důležitá pravidla pro Google Console

Pokud se změní doména nebo přidá nová, musí být v **Authorized redirect URIs** přidány obě verze (s www i bez):

1. `https://ekobio.org/scio/signin-google`
2. `https://www.ekobio.org/scio/signin-google`

### Přidání nového učitele

Pokud se učitel přihlásí přes Google poprvé a jeho e-mail ještě není v databázi, systém mu **automaticky vytvoří účet** s rolí `Teacher`.
Pokud učitel už má klasický účet (login/heslo) se stejným e-mailem jako na Google, systém tyto dva účty **automaticky propojí** při prvním úspěšném Google přihlášení.
