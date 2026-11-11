# Ověření prototypu

Funkční stav před přípravou zveřejnění, 11. 11. 2026:

- 79 konzolových testů úspěšných: interpret, syntaxe editoru, 31 ukázek,
  herní větve, LED a chybné parametry.
- WPF testy úspěšné: kroky, proměnné, INPUT, pokračování, chyby a cykly,
  zastavení rychlého PRINT/COLOR/LED i čekajícího INPUT, opětovné spuštění,
  skutečné barvy tří LED a reset.
- Ručně potvrzeno uživatelem: otevření aplikace, výpočty, IF/GOTO, FOR/NEXT,
  české chyby, UTF-8 soubory, ukázky, barevný editor, krokování a zastavení.
- Poslední LED sestavení na uživatelském počítači zablokovala ochrana Windows.

WPF test vyvolává události tlačítek v okně. Nenahrazuje ruční ověření klávesnice,
čtečky obrazovky a škálování DPI. Portable není ověřen na čistých Windows 11 a není podepsán.
GitHub workflow je připravený, ale na GitHubu ještě nebyl spuštěn.

Příprava veřejného vydání: místní sestavení s metadaty BASIC EDU / 1.0.0-prototype opět prošlo 79 testy i WPF testy. Portable byl vytvořen s licencí projektu, licencí AvalonEdit a licenčními soubory .NET, kontrolní součet SHA-256 je přiložen. Metadata BasicEdu.exe, BasicEdu.dll a BasicEdu.Core.dll byla zkontrolována. Tyto artefakty zůstávají nepodepsané; nejde o potvrzení, že je Smart App Control povolí. GitHub workflow nebyl spuštěn a SignPath žádost nebyla odeslána.
