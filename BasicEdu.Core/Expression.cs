using System.Globalization;
using System.Text.RegularExpressions;
namespace BasicEdu.Core;

internal sealed class Expression
{
    readonly List<string> tokens = [];
    readonly Dictionary<string, object> variables;
    int position;
    int depth;
    public Expression(string text, Dictionary<string, object> variables)
    {
        if (text.Length > 16384) throw new Exception("Výraz je příliš dlouhý (maximum 16384 znaků).");
        this.variables = variables;
        int offset = 0;
        while (offset < text.Length)
        {
            if (char.IsWhiteSpace(text[offset])) { offset++; continue; }
            var m = Regex.Match(text[offset..], "^(?:\"(?:[^\"]|\"\")*\"|(?:[0-9]+(?:\\.[0-9]*)?|\\.[0-9]+)(?:[Ee][+-]?[0-9]+)?|[A-Za-z][A-Za-z0-9_]*\\$?|<=|>=|<>|[+*/^()=<>-])");
            if (!m.Success) throw new Exception("Neplatný znak ve výrazu.");
            tokens.Add(m.Value); offset += m.Length;
        }
    }
    string Peek => position < tokens.Count ? tokens[position].ToUpperInvariant() : "";
    string Take() => position < tokens.Count ? tokens[position++] : throw new Exception("Neúplný výraz.");
    static double N(object v) => v is double d ? d : throw new Exception("Očekávám číselný výraz.");
    public object Evaluate() { var value = Parse(0); if (position != tokens.Count) throw new Exception("Neočekávaný zbytek výrazu."); return value; }
    static int Precedence(string op) => op switch { "OR" => 1, "AND" => 2, "=" or "<>" or "<" or ">" or "<=" or ">=" => 3, "+" or "-" => 4, "*" or "/" or "MOD" => 5, "^" => 7, _ => -1 };
    object Parse(int minimum)
    {
        if (++depth > 128) throw new Exception("Výraz je příliš vnořený.");
        string token = Take(); object left;
        if (token is "+" or "-" || token.Equals("NOT", StringComparison.OrdinalIgnoreCase))
        { var v = N(Parse(token.Equals("NOT", StringComparison.OrdinalIgnoreCase) ? 3 : 6)); left = token == "+" ? v : token == "-" ? -v : v == 0 ? -1d : 0d; }
        else if (token == "(") { left = Parse(0); if (Take() != ")") throw new Exception("Chybí uzavírací závorka."); }
        else if (token.StartsWith('"')) left = token[1..^1].Replace("\"\"", "\"");
        else if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out double number)) left = double.IsFinite(number) ? number : throw new Exception("Číslo je mimo rozsah.");
        else if (Regex.IsMatch(token, @"^[A-Za-z][A-Za-z0-9_]*\$?$")) { string name = token.ToUpperInvariant(); left = variables.GetValueOrDefault(name, name.EndsWith('$') ? (object)"" : 0d); }
        else throw new Exception("Očekávám číslo, text nebo proměnnou.");
        while (Precedence(Peek) >= minimum)
        {
            string op = Take().ToUpperInvariant(); int precedence = Precedence(op);
            object right = Parse(precedence + (op == "^" ? 0 : 1));
            if (op == "+" && left is string a && right is string b) left = a.Length + b.Length <= 1000000 ? a + b : throw new Exception("Text překročil limit milion znaků.");
            else if (op is "=" or "<>" or "<" or ">" or "<=" or ">=")
            {
                int cmp = left is string s && right is string t ? string.CompareOrdinal(s, t) : N(left).CompareTo(N(right));
                left = (op switch { "=" => cmp == 0, "<>" => cmp != 0, "<" => cmp < 0, ">" => cmp > 0, "<=" => cmp <= 0, _ => cmp >= 0 }) ? -1d : 0d;
            }
            else
            {
                double x = N(left), y = N(right);
                left = op switch { "+" => x + y, "-" => x - y, "*" => x * y, "/" => y != 0 ? x / y : throw new Exception("Dělení nulou."), "MOD" => y != 0 ? x % y : throw new Exception("Dělení nulou."), "^" => Math.Pow(x, y), "AND" => x != 0 && y != 0 ? -1d : 0d, "OR" => x != 0 || y != 0 ? -1d : 0d, _ => throw new Exception("Neplatný operátor.") };
            }
            if (left is double d && !double.IsFinite(d)) throw new Exception("Výsledek je mimo číselný rozsah.");
        }
        depth--; return left;
    }
}
