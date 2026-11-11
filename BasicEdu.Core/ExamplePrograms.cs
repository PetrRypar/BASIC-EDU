namespace BasicEdu.Core;

public sealed record ExampleProgram(string Title, string Description, string Task, string Source)
{
    public override string ToString() => Title;
}

public static class ExamplePrograms
{
    public static IReadOnlyList<ExampleProgram> All { get; } = Array.AsReadOnly(new[]
    {
        new ExampleProgram("01 • Ahoj, světe!", "První výpis a české znaky. Příkaz PRINT vypíše text mezi uvozovkami.",
            "Změň pozdrav a přidej další řádek PRINT před END.", """
            10 REM První program
            20 PRINT "Ahoj, světe!"
            30 PRINT "Příliš žluťoučký kůň"
            40 END
            """),
        new ExampleProgram("02 • Pozdrav se jménem", "INPUT načte jméno do textové proměnné JMENO$. Středník spojuje položky výpisu.",
            "Uprav program, aby se zeptal také na oblíbenou barvu.", """
            10 INPUT "Jak se jmenuješ? "; JMENO$
            20 PRINT "Ahoj, "; JMENO$; "!"
            30 END
            """),
        new ExampleProgram("03 • Malá kalkulačka", "Proměnné A a B obsahují čísla. Vyzkoušej součet, rozdíl, součin a závorky.",
            "Změň A a B. Přidej výpočet A / B; B nesmí být nula.", """
            10 LET A = 8
            20 B = 3
            30 PRINT "Součet: "; A + B
            40 PRINT "Rozdíl: "; A - B
            50 PRINT "Součin: "; A * B
            60 PRINT "Závorky: "; (A + B) * 2
            70 END
            """),
        new ExampleProgram("04 • Obsah obdélníku", "Dva číselné vstupy a výpočet obsahu S = A * B. Při vstupu lze použít desetinnou čárku.",
            "Doplň také obvod: 2 * (A + B).", """
            10 INPUT "Šířka: "; A
            20 INPUT "Výška: "; B
            30 S = A * B
            40 PRINT "Obsah: "; S
            50 END
            """),
        new ExampleProgram("05 • Kladné, záporné, nula", "IF vykoná příkaz za THEN jen tehdy, když podmínka platí.",
            "Spusť třikrát s hodnotami -5, 0 a 7.", """
            10 INPUT "Zadej číslo: "; A
            20 IF A < 0 THEN PRINT "Záporné"
            30 IF A = 0 THEN PRINT "Nula"
            40 IF A > 0 THEN PRINT "Kladné"
            50 END
            """),
        new ExampleProgram("06 • Otázka z násobilky", "IF a GOTO opakují otázku, dokud nezadáš správnou odpověď 42.",
            "Změň otázku i správnou odpověď. Program lze kdykoli zastavit klávesou Esc.", """
            10 INPUT "Kolik je 6 krát 7? "; A
            20 IF A = 42 THEN 60
            30 PRINT "Nesprávně, zkus to znovu."
            40 GOTO 10
            60 PRINT "Správně!"
            70 END
            """),
        new ExampleProgram("07 • Odpočítávání", "FOR/NEXT opakuje příkaz. Záporný STEP postupně snižuje hodnotu I.",
            "Změň začátek na 10 a krok na -2. Odpočítávání zatím nemá časovou prodlevu.", """
            10 FOR I = 5 TO 1 STEP -1
            20 PRINT I
            30 NEXT I
            40 PRINT "Hotovo!"
            50 END
            """),
        new ExampleProgram("08 • Tabulka násobků", "Dva vnořené cykly vytvoří tabulku 5 × 5. Čárka v PRINT vloží čtyři mezery.",
            "Zvětši oba cykly na 10. Samostatný PRINT ukončuje řádek tabulky.", """
            10 FOR I = 1 TO 5
            20 FOR J = 1 TO 5
            30 PRINT I * J,
            40 NEXT J
            50 PRINT
            60 NEXT I
            70 END
            """),
        new ExampleProgram("09 • Šestnáct barev", "COLOR mění barvu nových výpisů. Čísla barev jsou 0 až 15; černý text dostane bílé pozadí.",
            "Vyzkoušej COLOR 14, 1: žlutý text na modrém pozadí.", """
            10 CLS
            20 FOR C = 0 TO 15
            30 COLOR C, 15
            40 PRINT "Barva číslo "; C
            50 NEXT C
            60 COLOR 0, 15
            70 END
            """),
        new ExampleProgram("10 • Součet čísel", "Proměnná S postupně shromažďuje součet čísel od 1 do 10. Výsledek je 55.",
            "Změň horní mez cyklu na 100. Výsledek má být 5050.", """
            10 S = 0
            20 FOR I = 1 TO 10
            30 S = S + I
            40 NEXT I
            50 PRINT "Součet: "; S
            60 END
            """),
        new ExampleProgram("11 • Převod teploty", "Výraz převede stupně Celsia na stupně Fahrenheita.", "Vyzkoušej 0 a 100 °C; očekávej 32 a 212 °F.", """
            10 INPUT "Teplota ve °C: "; C
            20 F = C * 9 / 5 + 32
            30 PRINT "Teplota ve °F: "; F
            40 END
            """),
        new ExampleProgram("12 • Cena po slevě", "Procenta lze zapsat jako podíl ze sta. Sleva 20 % ponechá 80 % ceny.", "Zadej cenu 500 a slevu 20. Doplň výpis ušetřené částky.", """
            10 INPUT "Původní cena: "; C
            20 INPUT "Sleva v procentech: "; S
            30 PRINT "Nová cena: "; C * (1 - S / 100)
            40 END
            """),
        new ExampleProgram("13 • Průměr tří čísel", "Závorky zajistí sečtení všech hodnot před dělením třemi.", "Přidej čtvrté číslo a uprav dělitele.", """
            10 INPUT "První číslo: "; A
            20 INPUT "Druhé číslo: "; B
            30 INPUT "Třetí číslo: "; C
            40 PRINT "Průměr: "; (A + B + C) / 3
            50 END
            """),
        new ExampleProgram("14 • Větší ze dvou", "Tři podmínky rozliší větší číslo i shodu.", "Otestuj obě pořadí čísel a také dvě stejná čísla.", """
            10 INPUT "První číslo: "; A
            20 INPUT "Druhé číslo: "; B
            30 IF A > B THEN PRINT "Větší: "; A
            40 IF B > A THEN PRINT "Větší: "; B
            50 IF A = B THEN PRINT "Čísla jsou stejná."
            60 END
            """),
        new ExampleProgram("15 • Sudé nebo liché", "MOD vrací zbytek po dělení. Celé číslo je sudé, pokud má po dělení dvěma zbytek nula.", "Vyzkoušej 0, 7 a -4. Zadávej pouze celá čísla.", """
            10 INPUT "Celé číslo: "; A
            20 IF A MOD 2 = 0 THEN PRINT "Sudé"
            30 IF A MOD 2 <> 0 THEN PRINT "Liché"
            40 END
            """),
        new ExampleProgram("16 • Číslo v intervalu", "AND vyžaduje splnění obou podmínek; OR stačí jedna. Meze 1 a 10 jsou zahrnuté.", "Zkus 0, 1, 10 a 11. Potom změň interval.", """
            10 INPUT "Zadej číslo: "; A
            20 IF A >= 1 AND A <= 10 THEN PRINT "Uvnitř 1 až 10"
            30 IF A < 1 OR A > 10 THEN PRINT "Mimo interval"
            40 END
            """),
        new ExampleProgram("17 • Tajné slovo", "Porovnání textů rozlišuje velikost písmen. Toto je výuková hádanka, nikoli zabezpečení heslem.", "Tajné slovo je drak. Vyzkoušej také Drak a změň odpověď.", """
            10 INPUT "Tajné slovo: "; S$
            20 IF S$ = "drak" THEN PRINT "Správné slovo!"
            30 IF S$ <> "drak" THEN PRINT "Zkus příště jiné slovo."
            40 END
            """),
        new ExampleProgram("18 • Skládání věty", "Operátor + spojuje textové proměnné. Mezery je třeba zapsat do textu.", "Přidej třetí otázku a rozšiř větu.", """
            10 INPUT "Jméno: "; J$
            20 INPUT "Oblíbené zvíře: "; Z$
            30 V$ = J$ + " má rád zvíře: " + Z$ + "."
            40 PRINT V$
            50 END
            """),
        new ExampleProgram("19 • Druhé mocniny", "Operátor ^ umocňuje. Cyklus vypíše druhé mocniny čísel 1 až 5.", "Změň exponent na 3 a sleduj hodnoty při krokování.", """
            10 FOR I = 1 TO 5
            20 PRINT I; " na druhou = "; I ^ 2
            30 NEXT I
            40 END
            """),
        new ExampleProgram("20 • Faktoriál", "Součin čísel 1 až 5 je 5! = 120. Součin začíná hodnotou 1, nikoli nulou.", "Změň mez na 0: prázdný cyklus ponechá výsledek 1, tedy 0!.", """
            10 F = 1
            20 FOR I = 1 TO 5
            30 F = F * I
            40 NEXT I
            50 PRINT "Faktoriál: "; F
            60 END
            """),
        new ExampleProgram("21 • Fibonacciho čísla", "Každé další číslo je součtem předchozích dvou. Pomocná proměnná C zachová mezivýsledek.", "Krokuj F8 a sleduj A, B a C. Přidej další členy cyklu.", """
            10 A = 0
            20 B = 1
            30 FOR I = 1 TO 8
            40 PRINT A
            50 C = A + B
            60 A = B
            70 B = C
            80 NEXT I
            90 END
            """),
        new ExampleProgram("22 • Násobilka na přání", "INPUT nastaví číslo, cyklus vypíše jeho deset násobků.", "Zkus také 0 nebo záporné číslo. Uprav rozsah na 12 násobků.", """
            10 INPUT "Kterou násobilku? "; N
            20 FOR I = 1 TO 10
            30 PRINT N; " * "; I; " = "; N * I
            40 NEXT I
            50 END
            """),
        new ExampleProgram("23 • Obdélník z hvězdiček", "Vnitřní cyklus kreslí řádek, vnější cyklus řádky opakuje. Středník potlačuje odřádkování.", "Změň šířku 6 a výšku 3; zkus místo hvězdičky jiný znak.", """
            10 FOR R = 1 TO 3
            20 FOR S = 1 TO 6
            30 PRINT "*";
            40 NEXT S
            50 PRINT
            60 NEXT R
            70 END
            """),
        new ExampleProgram("24 • Rostoucí trojúhelník", "Horní mez vnitřního cyklu závisí na čísle aktuálního řádku R.", "Změň výšku na 8. Potom zkus obrátit pořadí řádků pomocí STEP -1.", """
            10 FOR R = 1 TO 4
            20 FOR S = 1 TO R
            30 PRINT "*";
            40 NEXT S
            50 PRINT
            60 NEXT R
            70 END
            """),
        new ExampleProgram("25 • Cyklus pomocí GOTO", "Opakování lze vytvořit i bez FOR. Podmínka musí zajistit ukončení.", "Sleduj N při krokování. Co by se stalo bez řádku 30? Esc program zastaví.", """
            10 N = 3
            20 PRINT N
            30 N = N - 1
            40 IF N > 0 THEN 20
            50 PRINT "Start!"
            60 END
            """),
        new ExampleProgram("26 • Bezpečné dělení", "Před dělením se kontroluje nulový dělitel a otázka se případně zopakuje.", "Zadej dělitele nejprve 0 a potom 2. Ověř, že nevznikne chyba dělení nulou.", """
            10 INPUT "Dělenec: "; A
            20 INPUT "Dělitel: "; B
            30 IF B <> 0 THEN 60
            40 PRINT "Nulou dělit nelze."
            50 GOTO 20
            60 PRINT "Výsledek: "; A / B
            70 END
            """),
        new ExampleProgram("27 • Kvíz se skóre", "Dvě otázky přičítají body do proměnné S. Každá správná odpověď má jeden bod.", "Přidej třetí otázku a uprav maximální skóre ve výpisu.", """
            10 S = 0
            20 INPUT "Kolik je 3 + 4? "; A
            30 IF A = 7 THEN S = S + 1
            40 INPUT "Kolik je 5 * 5? "; A
            50 IF A = 25 THEN S = S + 1
            60 PRINT "Skóre: "; S; " / 2"
            70 END
            """),
        new ExampleProgram("28 • Hádej číslo", "Hádané číslo je pevně 17. Program radí větší nebo menší a počítá pokusy; náhoda zatím není použita.", "Nech spoluhráče změnit tajné číslo N. Potom hádej bez nahlížení do editoru.", """
            10 N = 17
            20 P = 0
            30 INPUT "Tvůj tip: "; A
            40 P = P + 1
            50 IF A = N THEN 100
            60 IF A < N THEN PRINT "Hádej větší číslo."
            70 IF A > N THEN PRINT "Hádej menší číslo."
            80 GOTO 30
            100 PRINT "Uhodnuto! Počet pokusů: "; P
            110 END
            """),
        new ExampleProgram("29 • Dračí jeskyně", "Textová minihra větví příběh podle číselné volby 1 nebo 2. Jinou volbu odmítne.", "Přidej třetí cestu a vlastní zakončení příběhu.", """
            10 PRINT "Před tebou je dračí jeskyně."
            20 INPUT "1 = vstoupit, 2 = vrátit se: "; V
            30 IF V = 1 THEN 80
            40 IF V = 2 THEN 110
            50 PRINT "Vyber 1 nebo 2."
            60 GOTO 20
            80 PRINT "Drak spí. Našel jsi poklad!"
            90 END
            110 PRINT "Vrátil ses bezpečně domů."
            120 END
            """),
        new ExampleProgram("30 • Poslední sirka", "Začínáš se 7 sirkami. Odeber 1 až 3, počítač pak odebere jednu. Kdo vezme poslední, vyhrává.", "Vyzkoušej výhru i prohru. Počítač zatím vždy bere jednu; navrhni mu lepší strategii.", """
            10 N = 7
            20 PRINT "Zbývá sirek: "; N
            30 INPUT "Odeber 1 až 3: "; A
            40 IF A <> 1 AND A <> 2 AND A <> 3 THEN 150
            50 IF A > N THEN 150
            60 N = N - A
            70 IF N = 0 THEN 170
            80 PRINT "Počítač bere 1."
            90 N = N - 1
            100 IF N = 0 THEN 190
            110 GOTO 20
            150 PRINT "Neplatný tah."
            160 GOTO 30
            170 PRINT "Vyhrál jsi!"
            180 END
            190 PRINT "Vyhrál počítač."
            200 END
            """),
        new ExampleProgram("31 • LED semafor", "Tři LED nad výstupem ovládají příkazy LED1(barva) až LED3(barva). 0 vypíná, 2 je žlutá, 3 červená a 4 zelená. Enter přepíná fáze.", "Spusť také pomocí F8. Změň barvy podle legendy LED nad výstupem; tato paleta je jiná než COLOR.", """
            10 LED1(3)
            20 INPUT "Červená. Pro žlutou stiskni Enter: "; A$
            30 LED1(0)
            40 LED2(2)
            50 INPUT "Žlutá. Pro zelenou stiskni Enter: "; A$
            60 LED2(0)
            70 LED3(4)
            80 PRINT "Zelená!"
            90 END
            """)
    });
}
