using System.Windows.Media;
using BasicEdu.Core;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;

namespace BasicEdu.App;

public sealed class BasicColorizer : DocumentColorizingTransformer
{
    protected override void ColorizeLine(DocumentLine line)
    {
        string text = CurrentContext.Document.GetText(line);
        foreach (var token in BasicSyntax.TokenizeLine(text))
        {
            Brush brush = token.Kind switch
            {
                SyntaxKind.Keyword => Brushes.Blue,
                SyntaxKind.String => Brushes.SaddleBrown,
                SyntaxKind.Comment => Brushes.ForestGreen,
                SyntaxKind.LineNumber => Brushes.SlateGray,
                _ => Brushes.Teal
            };
            ChangeLinePart(line.Offset + token.Start, line.Offset + token.Start + token.Length,
                element => element.TextRunProperties.SetForegroundBrush(brush));
        }
    }
}
