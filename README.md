# BASIC EDU 1.0

České výukové prostředí pro číslovaný BASIC. Windows 11 x64, C# / .NET 10 / WPF.
Zdarma a open source pod [licencí MIT](LICENSE), která dovoluje také komerční použití.
Jde o funkční prototyp, nikoli dokončený produkt.

## Funkce

- Barevný editor, čeština a otevření/uložení `.BAS` v UTF-8.
- PRINT, INPUT, LET, IF THEN, GOTO, FOR TO STEP / NEXT, COLOR, CLS, END, REM a STOP.
- Číselné a textové proměnné s `$`, výrazy, porovnání a logické operátory.
- Krokování F8, panel proměnných, zvýraznění dalšího řádku, pokračování F5.
- Bezpečné zastavení tlačítkem či Esc, včetně nekonečného výpisu a INPUT.
- 31 vestavěných ukázek s vysvětlením a úkoly, od pozdravu po malé hry.
- Tři virtuální LED ovládané rozšířením LED1(barva) až LED3(barva).

```basic
10 INPUT "Jak se jmenuješ? "; JMENO$
20 LED1(4)
30 FOR I = 1 TO 3
40 PRINT I; " – Ahoj, "; JMENO$
50 NEXT I
60 END
```

## Sestavení a spuštění

Otevřete `BasicEdu.sln` ve Visual Studiu 2026 s úlohou „Vývoj desktopových aplikací pomocí .NET“
a SDK .NET 10. Jako spouštěný projekt vyberte `BasicEdu.App` a stiskněte F5.
Při prvním sestavení se z NuGet stáhne knihovna editoru AvalonEdit.

Nebo na Windows s SDK .NET 10:

```powershell
./build.ps1 -Portable -WpfTests
```

Skript sestaví řešení, spustí konzolové testy, volitelně WPF testy a vytvoří
`dist/BasicEdu-1.0-win-x64.zip` a kontrolní součet SHA-256. WPF testy otevírají testovací okno.
Portable obsahuje .NET: celý ZIP rozbalte a spusťte `BasicEdu.exe`.
Odinstalace spočívá v odstranění rozbalené složky; vlastní `.BAS` soubory si ponechte.

**Sestavení zatím nejsou digitálně podepsána.** Smart App Control může blokovat EXE i DLL,
včetně sestavení ve Visual Studiu. Úspěšné sestavení nezaručuje spuštění na každém počítači.
Nevypínejte kvůli prototypu ochranu Windows. Viz [Code signing policy](CODE_SIGNING.md).
Veřejná adresa ke stažení a schválená podepisovací služba zatím nejsou nastaveny.

## Ovládání

V aplikaci F5 spouští program; během krokování pokračuje v běhu. F8 provede jeden BASIC řádek.
Žlutě je další řádek k provedení. Proměnné se obnovují mezi kroky a při skončení.
INPUT používá pole pod výstupem, potvrzuje se Enterem. Esc či Zastavit ukončí program.
Ukázku vyberte v nabídce a klikněte na „Otevřít ukázku v editoru“.
Před nahrazením neuložené práce se zobrazí dotaz na uložení.

Každý BASIC řádek má unikátní číslo 1–65535. Program běží podle pořadí čísel.
Jeden příkaz na řádek, bez oddělovače `:`. Příkazy a proměnné nerozlišují velikost písmen,
textová porovnání ji rozlišují. Konstanty používají desetinnou tečku; INPUT přijímá také čárku.
PRINT odděluje položky středníkem bez mezery nebo čárkou se čtyřmi mezerami.
Koncový oddělovač potlačuje nový řádek.

Paleta LED: 0 vypnuto/černá, 1 bílá, 2 žlutá, 3 červená, 4 zelená, 5 modrá,
6 oranžová, 7 fialová, 8 azurová, 9 růžová, 10 šedá. Parametr může být výraz
s celočíselným výsledkem 0–10. Nový běh LED vypne; CLS maže pouze text;
ukončení ponechá poslední stav LED. Tato paleta je odlišná od příkazu COLOR.

## Stav a omezení

Ověřeno 79 konzolovými testy a integračními WPF testy: interpret, všechny ukázky,
krokování, proměnné, INPUT, zastavení a LED. Ruční testy potvrdily editor, soubory UTF-8,
základní příkazy a krokování. Poslední LED verzi na uživatelském počítači zablokovala
zásada Windows. Podrobnosti v [docs/TESTING.md](docs/TESTING.md).
Připravený GitHub workflow na GitHubu zatím nebyl spuštěn.

Chybí ELSE, GOSUB/RETURN, pole, DATA/READ, LOCATE, matematické a řetězcové funkce,
našeptávání a časová prodleva. Výstup je posuvný text, nikoli přesná obrazovka 80 × 25.
Uchovává přibližně 20 000 znaků. Chyby se hlásí při dosažení příkazu.
BASIC program nemá podporu volání systému, práce se soubory ani sítě.

## Soukromí, licence a příspěvky

Aplikace nemá účty, reklamy, analytiku, automatické aktualizace ani vlastní síťovou komunikaci.
Viz [Soukromí](PRIVACY.md), [cizí součásti](THIRD_PARTY_NOTICES.md),
[přispívání](CONTRIBUTING.md) a [Code signing policy](CODE_SIGNING.md).
Podklady pro zveřejnění a žádost SignPath: [docs/PUBLISHING.md](docs/PUBLISHING.md).
