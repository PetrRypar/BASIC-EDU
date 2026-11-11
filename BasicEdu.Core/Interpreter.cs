using System.Globalization;
using System.Text.RegularExpressions;

namespace BasicEdu.Core;

public interface IBasicConsole
{
    void Write(string text);
    void Clear();
    void Color(int foreground, int background);
    void Led(int indicator, int color);
    Task<string> ReadAsync(string prompt, CancellationToken cancellation);
}
public sealed class BasicException(int line, string message) : Exception($"Řádek {line}: {message}")
{ public int Line { get; } = line; }

public sealed class Interpreter(IBasicConsole console)
{
    readonly Dictionary<string, object> variables = new(StringComparer.OrdinalIgnoreCase);
    record Loop(string Name, double Limit, double Step, int Start, int End);
    readonly List<Loop> loops = [];
    static Match Match(string text, string pattern) => Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    static string Name(string text)
    {
        text = text.Trim().ToUpperInvariant();
        if (!Regex.IsMatch(text, @"^[A-Z][A-Z0-9_]*\$?$")) throw new Exception("Neplatný název proměnné.");
        return text;
    }
    static double Number(object value) => value is double n ? n : throw new Exception("Očekávám číslo.");
    static string Display(object value) => value is double n ? n.ToString("G", CultureInfo.InvariantCulture) : (string)value;
    object Eval(string text) => new Expression(text, variables).Evaluate();
    double Num(string text) => Number(Eval(text));
    void Assign(string name, object value)
    {
        name = Name(name);
        if (name.EndsWith('$') != (value is string)) throw new Exception("Nesouhlasí typ proměnné (text má příponu $).");
        variables[name] = value;
    }
    public IReadOnlyDictionary<string, object> GetVariables() =>
        new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new Dictionary<string, object>(variables, StringComparer.OrdinalIgnoreCase));

    public async Task RunAsync(string source, CancellationToken cancellation = default,
        Func<int, CancellationToken, Task>? beforeLine = null)
    {
        variables.Clear(); loops.Clear();
        for (int indicator = 1; indicator <= 3; indicator++) console.Led(indicator, 0);
        var lines = new SortedDictionary<int, string>();
        foreach (var raw in source.Replace("\r", "").Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var m = Match(raw, @"^\s*(\d+)\s+(.+?)\s*$");
            if (!m.Success || !int.TryParse(m.Groups[1].Value, out int n) || n is < 1 or > 65535)
                throw new BasicException(0, "Každý řádek musí začínat číslem 1 až 65535 a obsahovat příkaz.");
            if (!lines.TryAdd(n, m.Groups[2].Value)) throw new BasicException(n, "Číslo řádku je použito vícekrát.");
        }
        var program = lines.ToArray();
        var indexes = program.Select((p, i) => (p.Key, i)).ToDictionary(p => p.Key, p => p.i);
        int Jump(string target)
        {
            double n = Num(target);
            if (n != Math.Truncate(n) || n < 1 || n > 65535 || !indexes.TryGetValue((int)n, out int index)) throw new Exception("Cílový řádek neexistuje.");
            return index;
        }
        int FindNext(int start)
        {
            int depth = 0;
            for (int j = start + 1; j < program.Length; j++)
            {
                if (Match(program[j].Value, @"^FOR\b").Success) depth++;
                if (Match(program[j].Value, @"^NEXT\b").Success && depth-- == 0) return j;
            }
            throw new Exception("Příkazu FOR chybí NEXT.");
        }
        for (int pc = 0; pc < program.Length;)
        {
            cancellation.ThrowIfCancellationRequested();
            if (beforeLine != null) await beforeLine(program[pc].Key, cancellation);
            cancellation.ThrowIfCancellationRequested();
            int next = pc + 1;
            string command = program[pc].Value;
            try
            {
                // THEN can contain a single command, including a numeric jump target.
                if (Match(command, @"^IF\s+(.+?)\s+THEN\s+(.+)$") is { Success: true } condition)
                {
                    if (Num(condition.Groups[1].Value) == 0) { pc++; continue; }
                    command = condition.Groups[2].Value.Trim();
                    if (Regex.IsMatch(command, @"^\d+$")) command = "GOTO " + command;
                }
                var head = Match(command, @"^([A-Z]+)\b\s*(.*)$");
                var led = Match(command, @"^LED([0-9]+)\s*\((.*)\)$");
                string op = led.Success ? "LED" : head.Groups[1].Value.ToUpperInvariant(), arg = head.Groups[2].Value;
                switch (op)
                {
                    case "LED":
                        if (!int.TryParse(led.Groups[1].Value, out int indicator) || indicator is < 1 or > 3)
                            throw new Exception("Použij LED1(barva), LED2(barva) nebo LED3(barva).");
                        double ledColor = Num(led.Groups[2].Value);
                        if (ledColor != Math.Truncate(ledColor) || ledColor is < 0 or > 10)
                            throw new Exception("Barva LED musí být celé číslo 0 až 10 (0 = vypnuto).");
                        console.Led(indicator, (int)ledColor);
                        break;
                    case "REM": break;
                    case "END": case "STOP": return;
                    case "CLS": if (arg.Length > 0) throw new Exception("CLS nemá parametry."); console.Clear(); break;
                    case "COLOR":
                        var colors = arg.Split(',');
                        if (colors.Length is < 1 or > 2) throw new Exception("Použij COLOR popředí[, pozadí].");
                        int ColorValue(string s) { double v = Num(s); return v is >= 0 and <= 15 && v == Math.Truncate(v) ? (int)v : throw new Exception("Barva musí být celé číslo 0 až 15."); }
                        console.Color(ColorValue(colors[0]), colors.Length == 2 ? ColorValue(colors[1]) : 0); break;
                    case "PRINT":
                        var parts = SplitPrint(arg);
                        foreach (var (value, separator) in parts) { if (value.Trim().Length > 0) console.Write(Display(Eval(value))); if (separator == ',') console.Write("    "); }
                        if (arg.TrimEnd().Length == 0 || !";,".Contains(arg.TrimEnd()[^1])) console.Write("\n");
                        break;
                    case "INPUT":
                        var input = Match(arg, "^(?:\"([^\"]*)\"\\s*;\\s*)?([A-Z][A-Z0-9_]*\\$?)$");
                        if (!input.Success) throw new Exception("Použij INPUT [\"Otázka\";] proměnná.");
                        string name = Name(input.Groups[2].Value);
                        string answer = await console.ReadAsync(input.Groups[1].Success ? input.Groups[1].Value : "? ", cancellation);
                        if (name.EndsWith('$')) Assign(name, answer);
                        else if (double.TryParse(answer.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double number) && double.IsFinite(number)) Assign(name, number);
                        else throw new Exception("Zadaná hodnota není platné číslo.");
                        break;
                    case "GOTO": next = Jump(arg); break;
                    case "FOR":
                        var f = Match(arg, @"^([A-Z][A-Z0-9_]*)\s*=\s*(.+?)\s+TO\s+(.+?)(?:\s+STEP\s+(.+))?$");
                        if (!f.Success) throw new Exception("Použij FOR I = začátek TO konec [STEP krok].");
                        string variable = Name(f.Groups[1].Value);
                        double start = Num(f.Groups[2].Value), limit = Num(f.Groups[3].Value), step = f.Groups[4].Success ? Num(f.Groups[4].Value) : 1;
                        if (step == 0) throw new Exception("Krok cyklu nesmí být nula.");
                        int end = FindNext(pc);
                        Assign(variable, start);
                        int previous = loops.FindIndex(l => l.Name == variable);
                        if (previous >= 0) loops.RemoveRange(previous, loops.Count - previous);
                        if (step > 0 ? start > limit : start < limit) next = end + 1;
                        else loops.Add(new(variable, limit, step, pc + 1, end));
                        break;
                    case "NEXT":
                        if (loops.Count == 0) throw new Exception("NEXT nemá odpovídající FOR.");
                        var loop = loops[^1];
                        if (pc != loop.End || (arg.Length > 0 && Name(arg) != loop.Name)) throw new Exception("NEXT neodpovídá aktivnímu cyklu FOR.");
                        double current = Number(variables[loop.Name]) + loop.Step;
                        Assign(loop.Name, current);
                        if (loop.Step > 0 ? current <= loop.Limit : current >= loop.Limit) next = loop.Start;
                        else loops.RemoveAt(loops.Count - 1);
                        break;
                    default:
                        var assignment = Match(op == "LET" ? arg : command, @"^([A-Z][A-Z0-9_]*\$?)\s*=\s*(.+)$");
                        if (!assignment.Success) throw new Exception("Neznámý příkaz nebo chybný zápis.");
                        Assign(assignment.Groups[1].Value, Eval(assignment.Groups[2].Value)); break;
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex) { throw new BasicException(program[pc].Key, ex.Message); }
            pc = next;
            // Give cancellation and the UI a chance even in tight loops.
            if ((++ticks & 255) == 0) await Task.Delay(1, cancellation);
        }
    }
    int ticks;
    static List<(string, char)> SplitPrint(string text)
    {
        var result = new List<(string, char)>(); bool quoted = false; int start = 0;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '"') quoted = !quoted;
            if (!quoted && text[i] is ';' or ',') { result.Add((text[start..i], text[i])); start = i + 1; }
        }
        result.Add((text[start..], '\0')); return result;
    }
}
