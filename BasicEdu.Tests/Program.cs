using BasicEdu.Core;
using System.Text;

int passed = 0;
async Task Check(string name, string source, string expected, params string[] inputs)
{
    var console = new TestConsole(inputs);
    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    await new Interpreter(console).RunAsync(source, timeout.Token);
    if (console.Text.ToString() != expected) throw new Exception(name + ": " + console.Text);
    Console.WriteLine("OK " + name); passed++;
}
async Task Error(string name, string source, int line, params string[] inputs)
{
    try { await new Interpreter(new TestConsole(inputs)).RunAsync(source); }
    catch (BasicException ex) when (ex.Line == line) { Console.WriteLine("OK " + name); passed++; return; }
    throw new Exception("Očekávána chyba: " + name);
}
await Check("pořadí, čeština, aritmetika", "30 PRINT A\n10 LET A = (2+3)*4\n20 PRINT \"Příliš žluťoučký kůň\"", "Příliš žluťoučký kůň\n20\n");
await Check("INPUT text a číslo", "10 INPUT \"Jméno: \"; J$\n20 INPUT A\n30 PRINT J$; A*2", "Jméno: ? Žofie5\n", "Žofie", "2,5");
await Check("IF GOTO END", "10 A=A+1\n20 IF A<3 THEN 10\n30 IF A=3 THEN PRINT \"ano\"\n40 END\n50 PRINT 99", "ano\n");
await Check("vnořené cykly", "10 FOR I=1 TO 2\n20 FOR J=3 TO 1 STEP -1\n30 PRINT I*10+J\n40 NEXT J\n50 NEXT I", "13\n12\n11\n23\n22\n21\n");
await Check("prázdný cyklus", "10 FOR I=2 TO 1\n20 PRINT 99\n30 NEXT I\n40 PRINT 1", "1\n");
await Check("výrazy", "10 PRINT -2^2;\"/\";2^3^2;\"/\";(3>2 AND NOT 0)\n20 PRINT \"a\"+\"b\";\"a\"<>\"b\"", "-4/512/-1\nab-1\n");
await Check("uvozovky a oddělovače", "10 PRINT \"A;B,\";\"Řekl \"\"ahoj\"\"\";\n20 PRINT \"!\"", "A;B,Řekl \"ahoj\"!\n");
await Check("COLOR CLS", "10 PRINT 1\n20 COLOR 14,1\n30 CLS\n40 PRINT 2", "2\n");
await Error("dělení nulou", "10 PRINT 1/0", 10);
await Error("neexistující cíl", "10 GOTO 99", 10);
await Error("duplicitní číslo", "10 END\n10 END", 10);
await Error("chybějící číslo", "PRINT 1", 0);
await Error("nulový krok", "10 FOR I=1 TO 2 STEP 0\n20 NEXT I", 10);
await Error("NEXT bez FOR", "10 NEXT I", 10);
await Error("chybějící NEXT", "10 FOR I=1 TO 2", 10);
await Error("typ proměnné", "10 A=\"text\"", 10);
await Error("barva mimo rozsah", "10 COLOR 16", 10);
await Error("syntaxe výrazu", "10 PRINT 1+", 10);
await Error("neplatný vstup", "10 INPUT A", 10, "nečíslo");
foreach (bool waiting in new[] { false, true })
{
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
    var console = new TestConsole([]) { Wait = waiting };
    try { await new Interpreter(console).RunAsync(waiting ? "10 INPUT A" : "10 GOTO 10", cts.Token); throw new Exception("Nezastaveno"); }
    catch (OperationCanceledException) { Console.WriteLine("OK zastavení " + (waiting ? "INPUT" : "cyklu")); passed++; }
}
var exampleCases = new (string[] Inputs, string Output)[]
{
    ([], "Ahoj, světe!\nPříliš žluťoučký kůň\n"),
    (["Žofie"], "Jak se jmenuješ? Ahoj, Žofie!\n"),
    ([], "Součet: 11\nRozdíl: 5\nSoučin: 24\nZávorky: 22\n"),
    (["2,5", "4"], "Šířka: Výška: Obsah: 10\n"),
    (["-5"], "Zadej číslo: Záporné\n"),
    (["10", "42"], "Kolik je 6 krát 7? Nesprávně, zkus to znovu.\nKolik je 6 krát 7? Správně!\n"),
    ([], "5\n4\n3\n2\n1\nHotovo!\n"),
    ([], string.Concat(Enumerable.Range(1, 5).Select(i => string.Concat(Enumerable.Range(1, 5).Select(j => $"{i * j}    ")) + "\n"))),
    ([], string.Concat(Enumerable.Range(0, 16).Select(i => $"Barva číslo {i}\n"))),
    ([], "Součet: 55\n"),
    (["100"], "Teplota ve °C: Teplota ve °F: 212\n"),
    (["500", "20"], "Původní cena: Sleva v procentech: Nová cena: 400\n"),
    (["3", "6", "9"], "První číslo: Druhé číslo: Třetí číslo: Průměr: 6\n"),
    (["8", "3"], "První číslo: Druhé číslo: Větší: 8\n"),
    (["7"], "Celé číslo: Liché\n"),
    (["10"], "Zadej číslo: Uvnitř 1 až 10\n"),
    (["drak"], "Tajné slovo: Správné slovo!\n"),
    (["Petr", "kůň"], "Jméno: Oblíbené zvíře: Petr má rád zvíře: kůň.\n"),
    ([], "1 na druhou = 1\n2 na druhou = 4\n3 na druhou = 9\n4 na druhou = 16\n5 na druhou = 25\n"),
    ([], "Faktoriál: 120\n"),
    ([], "0\n1\n1\n2\n3\n5\n8\n13\n"),
    (["3"], string.Concat(Enumerable.Range(1, 10).Select(i => $"3 * {i} = {3 * i}\n")).Insert(0, "Kterou násobilku? ")),
    ([], "******\n******\n******\n"),
    ([], "*\n**\n***\n****\n"),
    ([], "3\n2\n1\nStart!\n"),
    (["10", "0", "2"], "Dělenec: Dělitel: Nulou dělit nelze.\nDělitel: Výsledek: 5\n"),
    (["7", "25"], "Kolik je 3 + 4? Kolik je 5 * 5? Skóre: 2 / 2\n"),
    (["10", "20", "17"], "Tvůj tip: Hádej větší číslo.\nTvůj tip: Hádej menší číslo.\nTvůj tip: Uhodnuto! Počet pokusů: 3\n"),
    (["0", "1"], "Před tebou je dračí jeskyně.\n1 = vstoupit, 2 = vrátit se: Vyber 1 nebo 2.\n1 = vstoupit, 2 = vrátit se: Drak spí. Našel jsi poklad!\n"),
    (["3", "3"], "Zbývá sirek: 7\nOdeber 1 až 3: Počítač bere 1.\nZbývá sirek: 3\nOdeber 1 až 3: Vyhrál jsi!\n"),
    (["", ""], "Červená. Pro žlutou stiskni Enter: Žlutá. Pro zelenou stiskni Enter: Zelená!\n")
};
if (ExamplePrograms.All.Count != exampleCases.Length) throw new Exception("Chybí test ukázky.");
for (int i = 0; i < exampleCases.Length; i++)
{
    var example = ExamplePrograms.All[i];
    if (string.IsNullOrWhiteSpace(example.Description) || string.IsNullOrWhiteSpace(example.Task)) throw new Exception("Chybí popis ukázky.");
    await Check(example.Title, example.Source, exampleCases[i].Output, exampleCases[i].Inputs);
}
await Check("Větší druhé číslo", ExamplePrograms.All[13].Source, "První číslo: Druhé číslo: Větší: 9\n", "1", "9");
await Check("Shodná čísla", ExamplePrograms.All[13].Source, "První číslo: Druhé číslo: Čísla jsou stejná.\n", "5", "5");
await Check("Sudé záporné číslo", ExamplePrograms.All[14].Source, "Celé číslo: Sudé\n", "-4");
await Check("Mimo interval", ExamplePrograms.All[15].Source, "Zadej číslo: Mimo interval\n", "0");
await Check("Rozlišení velikosti textu", ExamplePrograms.All[16].Source, "Tajné slovo: Zkus příště jiné slovo.\n", "Drak");
await Check("Kvíz bez bodů", ExamplePrograms.All[26].Source, "Kolik je 3 + 4? Kolik je 5 * 5? Skóre: 0 / 2\n", "0", "0");
await Check("Návrat z jeskyně", ExamplePrograms.All[28].Source, "Před tebou je dračí jeskyně.\n1 = vstoupit, 2 = vrátit se: Vrátil ses bezpečně domů.\n", "2");
await Check("Sirky neplatné tahy a prohra", ExamplePrograms.All[29].Source,
    "Zbývá sirek: 7\nOdeber 1 až 3: Neplatný tah.\nOdeber 1 až 3: Neplatný tah.\nOdeber 1 až 3: Počítač bere 1.\nZbývá sirek: 3\nOdeber 1 až 3: Počítač bere 1.\nVyhrál počítač.\n", "0", "1,5", "3", "2");
void Syntax(string source, params (string Text, SyntaxKind Kind)[] expected)
{
    var actual = BasicSyntax.TokenizeLine(source).Select(t => (source.Substring(t.Start, t.Length), t.Kind));
    if (!actual.SequenceEqual(expected)) throw new Exception("Chybné zvýraznění: " + source);
    passed++;
    Console.WriteLine("OK zvýraznění " + source);
}
Syntax("10 print \"REM 42\"", ("10", SyntaxKind.LineNumber), ("print", SyntaxKind.Keyword), ("\"REM 42\"", SyntaxKind.String));
Syntax("20 REM Český PRINT 123", ("20", SyntaxKind.LineNumber), ("REM Český PRINT 123", SyntaxKind.Comment));
Syntax("30 PRINT \"Řekl \"\"ahoj\"\"\"", ("30", SyntaxKind.LineNumber), ("PRINT", SyntaxKind.Keyword), ("\"Řekl \"\"ahoj\"\"\"", SyntaxKind.String));
Syntax("40 PRINT \"Rozepsáno", ("40", SyntaxKind.LineNumber), ("PRINT", SyntaxKind.Keyword), ("\"Rozepsáno", SyntaxKind.String));
Syntax("50 IF A>0 THEN REM Poznámka", ("50", SyntaxKind.LineNumber), ("IF", SyntaxKind.Keyword), ("0", SyntaxKind.Number), ("THEN", SyntaxKind.Keyword), ("REM Poznámka", SyntaxKind.Comment));
Syntax("60 REMARK$ = \"test\"", ("60", SyntaxKind.LineNumber), ("\"test\"", SyntaxKind.String));
Syntax("70 A1=1.5E-2+.5", ("70", SyntaxKind.LineNumber), ("1.5E-2", SyntaxKind.Number), (".5", SyntaxKind.Number));
Syntax("80 PRINT REM$", ("80", SyntaxKind.LineNumber), ("PRINT", SyntaxKind.Keyword));
Syntax("10 led1(3)", ("10", SyntaxKind.LineNumber), ("led1", SyntaxKind.Keyword), ("3", SyntaxKind.Number));
var ledConsole = new TestConsole([]);
var ledInterpreter = new Interpreter(ledConsole);
await ledInterpreter.RunAsync("10 led1 (3)\n20 C=2\n30 LED2(C)\n40 IF C=2 THEN LED3((C+2))\n50 CLS\n60 END");
if (!ledConsole.Leds.SequenceEqual(new[] { (1,0), (2,0), (3,0), (1,3), (2,2), (3,4) })) throw new Exception("Nesprávná posloupnost LED.");
passed++; Console.WriteLine("OK LED výrazy, IF a nezávislost CLS");
ledConsole.Leds.Clear();
await ledInterpreter.RunAsync("10 END");
if (!ledConsole.Leds.SequenceEqual(new[] { (1,0), (2,0), (3,0) })) throw new Exception("LED se při novém běhu nevypnuly.");
passed++; Console.WriteLine("OK reset LED při novém běhu");
ledConsole.Leds.Clear();
await ledInterpreter.RunAsync("10 FOR I=0 TO 10\n20 LED1(I)\n30 NEXT I");
if (!ledConsole.Leds.Skip(3).SequenceEqual(Enumerable.Range(0,11).Select(i => (1,i)))) throw new Exception("Chybí barva LED.");
passed++; Console.WriteLine("OK všechny LED barvy 0 až 10");
await Error("LED záporná barva", "10 LED1(-1)", 10);
await Error("LED velká barva", "10 LED2(11)", 10);
await Error("LED desetinná barva", "10 LED3(1.5)", 10);
await Error("LED textová barva", "10 LED1(\"červená\")", 10);
await Error("LED neexistující indikátor", "10 LED4(3)", 10);
await Error("LED chybějící závorka", "10 LED1(3", 10);
await Error("LED prázdný parametr", "10 LED1()", 10);
Console.WriteLine($"Prošlo {passed} testů.");

sealed class TestConsole(string[] inputs) : IBasicConsole
{
    public List<(int Indicator, int Color)> Leds { get; } = [];
    public void Led(int indicator, int color) => Leds.Add((indicator, color));
    public StringBuilder Text { get; } = new();
    readonly Queue<string> answers = new(inputs);
    public bool Wait { get; init; }
    public void Write(string text) => Text.Append(text);
    public void Clear() => Text.Clear();
    public void Color(int f, int b) { if (f is < 0 or > 15 || b is < 0 or > 15) throw new Exception("Nesprávná barva"); }
    public async Task<string> ReadAsync(string prompt, CancellationToken token)
    {
        Write(prompt);
        if (Wait) await Task.Delay(Timeout.Infinite, token);
        return answers.Count > 0 ? answers.Dequeue() : throw new Exception("Testu chybí vstup.");
    }
}
