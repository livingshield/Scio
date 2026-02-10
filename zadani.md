Vytvořte prototyp aplikace pro realtime sledování pokroku skupiny studentů
učitelem.
Funkcionalita:
Registrace / přihlášení Google účtem, RBAC.
Po přihlášení možnost založit novou skupinu a tabulkový přehled existujících skupin. Při zadání
skupiny vyplňuje uživatel pouze její název (například "A2 - kvadratické rovnice 1") a popis cíle
(například "vyřeší samostatně 3 různé kvadratické rovnice typu ax^2 + bx + c" pomocí
diskriminantu).
Pro skupinu je vygenerován QR kód, který může kdokoli použít pro vstup do skupiny, jedno
zařízení může do skupiny vstoupit pouze jednou (localstorage), po vstupu zadá uživatel svůj nick
(například "Honza Novák“).
Uživatel ve skupině pracuje formou textového chatu. Po vstupu je přivítán zprávou, která popisuje
cíl, který má splnit.
Na obrazovce uživatel vidí vždy svůj postup plnění cíle, případně cílů - cíle jsou typu "splněno/
nesplněno" (například "vysvětlí rozdíl mezi lineární a kvadratickou rovnicí"), nebo "splněno %"
(například "vyřeší 3 rovnice") - splněno 0%, 33%, 66%, 100%.
U cílů s procenty je progressbar, u cílů ano/ne checkmark.
Systém navádí studenta k řešení úkolů, pokud nepracuje, zobrazí nejprve varování (např. formou
indikátoru pod zprávou, která není relevantní k cílům), poté upozorní učitele (popsáno níže).
Zprávy, které řeší cíl, nebo zvyšují pokrok jsou v konverzaci zvýrazněny, například zeleným
rámečkem.
Učitel může v reálném čase sledovat pokrok žáků, vidí jejich pokrok v plnění cílů a pokud některý
ze žáků potřebuje pomoci, vidí u něj indikátor, který může označit jako vyřešený.
Technologie:
knihovny libovolně.
Blazor Server, .NET Core 8/9/10.- SQL Server. TS, Sass/Tailwind, UI komponenty libovolně. AI
Bonusové výzvy:
Učitel si u žáků může rozkliknout detail (inline na stránce), kde vidí klíčové zprávy, které vedou k
pokroku v zadaných cílech. Může být zobrazeno i jako pár zadání-řešení, případně agregace více
zpráv, které úkol řeší souhrnně.
Chat rozhraní pro žáky podporuje zobrazení matematických výrazů a ukázek kódu.
Žák může diktovat zadání hlasem.
Při tvorbě řešení můžete použít AI v libovolném rozsahu dle vlastního uvážení. Prosíme pouze o
vyjádření, které systémy (např. Cursor) byly použity a jaké části jsou takto řešeny (např. "celé to psalo
chatgpt"). Připojte prosím poznámku, kolik času jste strávil/a tvorbou řešení.
Odevzdání:
Preferujeme demo deployment na Vercel + GitHub repozitář, v případě soukromého repozitáře
přidejte prosím do spolupracovníků handle @lofcz. Pokud je to jen trochu možné, neodevzdávejte
prosím .zip soubor