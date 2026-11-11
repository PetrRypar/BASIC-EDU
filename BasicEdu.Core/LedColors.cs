namespace BasicEdu.Core;

public sealed record LedColor(string Name, string Hex);
public static class LedColors
{
    // This educational extension deliberately has its own palette, independent of COLOR.
    public static IReadOnlyList<LedColor> All { get; } = Array.AsReadOnly(new[]
    {
        new LedColor("vypnuto", "#000000"), new LedColor("bílá", "#FFFFFF"),
        new LedColor("žlutá", "#FFFF00"), new LedColor("červená", "#FF3030"),
        new LedColor("zelená", "#00D040"), new LedColor("modrá", "#2864FF"),
        new LedColor("oranžová", "#FF9000"), new LedColor("fialová", "#A040FF"),
        new LedColor("azurová", "#00E5FF"), new LedColor("růžová", "#FF70C0"),
        new LedColor("šedá", "#999999")
    });
    public static string Legend => string.Join(" • ", All.Select((c, i) => $"{i} = {c.Name}"));
}
