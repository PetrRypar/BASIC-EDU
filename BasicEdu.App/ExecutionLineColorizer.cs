using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
namespace BasicEdu.App;

public sealed class ExecutionLineColorizer : DocumentColorizingTransformer
{
    public int? LineNumber { get; set; }
    protected override void ColorizeLine(DocumentLine line)
    {
        if (line.LineNumber == LineNumber && line.Length > 0)
            ChangeLinePart(line.Offset, line.EndOffset, e => e.TextRunProperties.SetBackgroundBrush(Brushes.LemonChiffon));
    }
}
