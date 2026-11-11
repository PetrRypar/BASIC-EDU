namespace BasicEdu.Core;

public enum SyntaxKind { Keyword, String, Number, LineNumber, Comment }
public readonly record struct SyntaxToken(int Start, int Length, SyntaxKind Kind);

/// <summary>Lexical colors only: never changes source or rejects unfinished code.</summary>
public static class BasicSyntax
{
    static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "PRINT", "INPUT", "LET", "IF", "THEN", "GOTO", "FOR", "TO", "STEP",
        "NEXT", "COLOR", "CLS", "END", "STOP", "REM", "AND", "OR", "NOT", "MOD",
        "LED1", "LED2", "LED3"
    };
    public static IReadOnlyList<SyntaxToken> TokenizeLine(string text)
    {
        var result = new List<SyntaxToken>();
        bool firstToken = true, commandStart = true;
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            int start = i;
            if (c == '"')
            {
                i++;
                while (i < text.Length)
                {
                    if (text[i++] != '"') continue;
                    if (i < text.Length && text[i] == '"') { i++; continue; }
                    break;
                }
                result.Add(new(start, i - start, SyntaxKind.String));
            }
            else if (char.IsAsciiDigit(c) || c == '.' && i + 1 < text.Length && char.IsAsciiDigit(text[i + 1]))
            {
                while (i < text.Length && char.IsAsciiDigit(text[i])) i++;
                if (i < text.Length && text[i] == '.') { i++; while (i < text.Length && char.IsAsciiDigit(text[i])) i++; }
                if (i < text.Length && text[i] is 'e' or 'E')
                {
                    int exponent = i++;
                    if (i < text.Length && text[i] is '+' or '-') i++;
                    int digits = i;
                    while (i < text.Length && char.IsAsciiDigit(text[i])) i++;
                    if (i == digits) i = exponent;
                }
                bool label = firstToken && text[start..i].All(char.IsAsciiDigit)
                    && i < text.Length && char.IsWhiteSpace(text[i]);
                result.Add(new(start, i - start, label ? SyntaxKind.LineNumber : SyntaxKind.Number));
                firstToken = false;
                if (label) continue;
            }
            else if (char.IsAsciiLetter(c))
            {
                i++;
                while (i < text.Length && (char.IsAsciiLetterOrDigit(text[i]) || text[i] == '_')) i++;
                if (i < text.Length && text[i] == '$') i++;
                string word = text[start..i];
                if (commandStart && word.Equals("REM", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new(start, text.Length - start, SyntaxKind.Comment));
                    break;
                }
                if (Keywords.Contains(word)) result.Add(new(start, i - start, SyntaxKind.Keyword));
                commandStart = word.Equals("THEN", StringComparison.OrdinalIgnoreCase);
                firstToken = false;
                continue;
            }
            else i++;
            firstToken = false;
            commandStart = false;
        }
        return result;
    }
}
