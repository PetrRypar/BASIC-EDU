using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BasicEdu.Core;
using Microsoft.Win32;

namespace BasicEdu.App;
public partial class MainWindow : Window, IBasicConsole
{
    string? path;
    bool dirty;
    CancellationTokenSource? cancellation;
    StepController? stepController;
    readonly ExecutionLineColorizer executionLine = new();
    TaskCompletionSource<string>? pendingInput;
    int foreground = 15, background;
    int outputLength;
    bool scrollPending;
    readonly DispatcherTimer outputScrollTimer;
    static readonly Brush[] LedBrushes = LedColors.All.Select(c =>
    {
        var brush = (Brush)new BrushConverter().ConvertFromString(c.Hex)!;
        brush.Freeze();
        return brush;
    }).ToArray();
    static readonly Brush[] Palette = new[] { "#000000", "#0000AA", "#00AA00", "#00AAAA", "#AA0000", "#AA00AA", "#AA5500", "#AAAAAA", "#555555", "#5555FF", "#55FF55", "#55FFFF", "#FF5555", "#FF55FF", "#FFFF55", "#FFFFFF" }.Select(s => (Brush)new BrushConverter().ConvertFromString(s)!).ToArray();
    public MainWindow()
    {
        InitializeComponent();
        LedLegend.Text = LedColors.Legend;
        Editor.TextArea.TextView.LineTransformers.Add(new BasicColorizer());
        Editor.TextArea.TextView.LineTransformers.Add(executionLine);
        ExamplesList.ItemsSource = ExamplePrograms.All;
        ExamplesList.SelectedIndex = 0;
        outputScrollTimer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        outputScrollTimer.Tick += (_, _) =>
        {
            if (!scrollPending) return;
            scrollPending = false;
            Output.ScrollToEnd();
        };
        outputScrollTimer.Start();
        Closed += (_, _) => outputScrollTimer.Stop();
        Editor.Text = "10 REM Vítej v BASIC EDU!\n20 CLS\n30 COLOR 14, 0\n40 PRINT \"Ahoj, světe!\"\n50 INPUT \"Jak se jmenuješ? \"; JMENO$\n60 FOR I = 1 TO 3\n70 PRINT I; \" – Vítej, \"; JMENO$\n80 NEXT I\n90 END";
        dirty = false;
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.F5) { Run(this, new RoutedEventArgs()); e.Handled = true; }
            if (e.Key == Key.F8) { Step(this, new RoutedEventArgs()); e.Handled = true; }
            if (e.Key == Key.Escape) { Stop(this, new RoutedEventArgs()); e.Handled = true; }
        };
    }
    void Edited(object? sender, EventArgs e)
    {
        dirty = true;
        if (cancellation == null && VariablesGrid != null) VariablesGrid.ItemsSource = null;
    }
    void ExampleSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ExamplesList.SelectedItem is not ExampleProgram example) return;
        ExampleDescription.Text = example.Description;
        ExampleTask.Text = "Úkol: " + example.Task;
    }
    void OpenExample(object sender, RoutedEventArgs e)
    {
        if (cancellation != null || ExamplesList.SelectedItem is not ExampleProgram example || !CanReplace()) return;
        Editor.Text = example.Source;
        path = null;
        dirty = true; // Saving an example always creates the learner's own .BAS file.
        Status.Text = "Otevřena ukázka: " + example.Title + " • Spusťte pomocí F5.";
        Editor.Focus();
        Editor.CaretOffset = 0;
        Editor.ScrollToHome();
    }
    bool CanReplace()
    {
        if (!dirty) return true;
        var result = MessageBox.Show(this, "Uložit změny v programu?", "BASIC EDU", MessageBoxButton.YesNoCancel);
        return result == MessageBoxResult.No || result == MessageBoxResult.Yes && Save(false);
    }
    void NewFile(object sender, RoutedEventArgs e) { if (cancellation != null || !CanReplace()) return; Editor.Clear(); path = null; dirty = false; Status.Text = "Nový program"; }
    void OpenFile(object sender, RoutedEventArgs e)
    {
        if (cancellation != null || !CanReplace()) return;
        var dialog = new OpenFileDialog { Filter = "BASIC program (*.bas)|*.bas|Všechny soubory|*.*" };
        if (dialog.ShowDialog(this) != true) return;
        try { var text = File.ReadAllText(dialog.FileName, new UTF8Encoding(false, true)); Editor.Text = text; path = dialog.FileName; dirty = false; Status.Text = path; }
        catch (Exception ex) { MessageBox.Show(this, "Soubor nelze otevřít: " + ex.Message); }
    }
    bool Save(bool saveAs)
    {
        string? target = path;
        if (saveAs || target == null)
        {
            var dialog = new SaveFileDialog { Filter = "BASIC program (*.bas)|*.bas", DefaultExt = ".bas", FileName = path == null ? "Program.bas" : Path.GetFileName(path) };
            if (dialog.ShowDialog(this) != true) return false;
            target = dialog.FileName;
        }
        try { File.WriteAllText(target, Editor.Text, new UTF8Encoding(false)); path = target; dirty = false; Status.Text = "Uloženo: " + path; return true; }
        catch (Exception ex) { MessageBox.Show(this, "Soubor nelze uložit: " + ex.Message); return false; }
    }
    void SaveFile(object sender, RoutedEventArgs e) => Save(false);
    void SaveAs(object sender, RoutedEventArgs e) => Save(true);
    void Run(object sender, RoutedEventArgs e)
    {
        if (cancellation != null)
        {
            if (!RunButton.IsEnabled) return;
            RunButton.IsEnabled = StepButton.IsEnabled = false;
            RunButton.Content = "▶ Spustit (F5)";
            executionLine.LineNumber = null;
            Editor.TextArea.TextView.Redraw();
            Status.Text = "Program běží…";
            stepController?.Continue();
            return;
        }
        StartProgram(false);
    }
    void Step(object sender, RoutedEventArgs e)
    {
        if (cancellation == null) { StartProgram(true); return; }
        if (!StepButton.IsEnabled) return;
        RunButton.IsEnabled = StepButton.IsEnabled = false;
        Status.Text = "Provádím krok…";
        stepController?.Step();
    }
    sealed record VariableRow(string Name, string Value);
    void ShowVariables(Interpreter interpreter)
    {
        VariablesGrid.ItemsSource = interpreter.GetVariables().OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .Select(p => new VariableRow(p.Key, p.Value is string text ? "\"" + text + "\"" : Convert.ToString(p.Value, System.Globalization.CultureInfo.InvariantCulture) ?? "")).ToArray();
    }
    async void StartProgram(bool stepping)
    {
        string source = Editor.Text;
        using var runCancellation = new CancellationTokenSource(); cancellation = runCancellation;
        var controller = new StepController(stepping);
        stepController = controller;
        var interpreter = new Interpreter(this);
        VariablesGrid.ItemsSource = null;
        RunButton.IsEnabled = StepButton.IsEnabled = false; StopButton.IsEnabled = true; Editor.IsReadOnly = true; ExamplesPanel.IsEnabled = false;
        foreground = 15; background = 0; Clear(); Status.Text = "Program běží…";
        try
        {
            await Task.Run(() => interpreter.RunAsync(source, runCancellation.Token,
                (line, token) => controller.BeforeLineAsync(line, async nextLine =>
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        if (runCancellation.IsCancellationRequested) return;
                        ShowVariables(interpreter);
                        MarkExecutionLine(nextLine);
                        RunButton.Content = "▶ Pokračovat (F5)";
                        RunButton.IsEnabled = StepButton.IsEnabled = true;
                        Status.Text = $"Další řádek: {nextLine} • F8 = jeden krok, F5 = pokračovat.";
                    }, DispatcherPriority.Background);
                }, token)));
            Status.Text = "Program skončil.";
        }
        catch (OperationCanceledException) { Status.Text = "Program byl zastaven."; }
        catch (BasicException ex) { Status.Text = ex.Message; Highlight(ex.Line); }
        catch (Exception ex) { Status.Text = "Program nelze spustit: " + ex.Message; }
        finally
        {
            ShowVariables(interpreter);
            executionLine.LineNumber = null;
            Editor.TextArea.TextView.Redraw();
            pendingInput = null; InputBox.IsEnabled = InputButton.IsEnabled = false;
            RunButton.Content = "▶ Spustit (F5)";
            RunButton.IsEnabled = StepButton.IsEnabled = true; StopButton.IsEnabled = false;
            Editor.IsReadOnly = false; ExamplesPanel.IsEnabled = true;
            stepController = null; cancellation = null;
        }
    }
    void MarkExecutionLine(int number)
    {
        foreach (var line in Editor.Document.Lines)
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(Editor.Document.GetText(line), @"^\s*" + number + @"\s")) continue;
            executionLine.LineNumber = line.LineNumber;
            Editor.ScrollToLine(line.LineNumber);
            Editor.TextArea.TextView.Redraw();
            break;
        }
    }
    void Highlight(int number)
    {
        int offset = 0;
        foreach (string line in Editor.Text.Split('\n')) { if (System.Text.RegularExpressions.Regex.IsMatch(line, @"^\s*" + number + @"\s")) { Editor.Focus(); Editor.Select(offset, line.Length); Editor.ScrollToLine(Editor.Document.GetLineByOffset(offset).LineNumber); break; } offset += line.Length + 1; }
    }
    void Stop(object sender, RoutedEventArgs e) => cancellation?.Cancel();
    void OnClosing(object? sender, CancelEventArgs e)
    {
        if (cancellation != null) { cancellation.Cancel(); e.Cancel = true; Status.Text = "Zastavuji program. Poté lze okno zavřít."; return; }
        e.Cancel = !CanReplace();
    }
    // Synchronous backpressure bounds pending output to one operation. Background
    // priority lets keyboard/mouse input (especially Stop) run before more output.
    public void Write(string text) => Dispatcher.Invoke(() =>
    {
        const int maximumOutput = 20_000;
        if (text.Length > maximumOutput) text = text[^maximumOutput..];
        if (outputLength + text.Length > maximumOutput) { Output.Document.Blocks.Clear(); outputLength = 0; }
        var paragraph = Output.Document.Blocks.LastBlock as Paragraph;
        if (paragraph == null) { paragraph = new Paragraph { Margin = new Thickness(0) }; Output.Document.Blocks.Add(paragraph); }
        // Merge adjacent text instead of creating two WPF elements per PRINT.
        if (paragraph.Inlines.LastInline is Run previous && previous.Text.Length + text.Length <= 2048
            && previous.Foreground == Palette[foreground] && previous.Background == Palette[background])
            previous.Text += text;
        else paragraph.Inlines.Add(new Run(text) { Foreground = Palette[foreground], Background = Palette[background] });
        outputLength += text.Length;
        scrollPending = true;
    }, DispatcherPriority.Background);
    public void Clear() => Dispatcher.Invoke(() => { Output.Document.Blocks.Clear(); Output.Background = Palette[background]; outputLength = 0; scrollPending = false; }, DispatcherPriority.Background);
    public void Color(int fg, int bg) => Dispatcher.Invoke(() => { foreground = fg; background = bg; Output.Background = Palette[bg]; }, DispatcherPriority.Background);
    public void Led(int indicator, int color) => Dispatcher.Invoke(() =>
    {
        var definition = LedColors.All[color];
        var lamp = indicator switch { 1 => Led1Indicator, 2 => Led2Indicator, _ => Led3Indicator };
        var label = indicator switch { 1 => Led1Label, 2 => Led2Label, _ => Led3Label };
        lamp.Fill = LedBrushes[color];
        label.Text = $"LED{indicator} • {definition.Name}";
        lamp.ToolTip = $"LED{indicator}({color}) • {definition.Name}";
        System.Windows.Automation.AutomationProperties.SetName(lamp, label.Text);
    }, DispatcherPriority.Background);
    public async Task<string> ReadAsync(string prompt, CancellationToken token)
    {
        Write(prompt);
        var completion = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = token.Register(() => completion.TrySetCanceled(token));
        await Dispatcher.InvokeAsync(() => { pendingInput = completion; InputBox.Clear(); InputBox.IsEnabled = InputButton.IsEnabled = true; InputBox.Focus(); Status.Text = "Čekám na vstup…"; });
        return await completion.Task;
    }
    void SubmitInput(object sender, RoutedEventArgs e)
    {
        if (pendingInput == null) return;
        string answer = InputBox.Text; var pending = pendingInput; pendingInput = null;
        InputBox.IsEnabled = InputButton.IsEnabled = false; Write(answer + "\n"); Status.Text = "Program běží…"; pending.TrySetResult(answer);
    }
    void InputKeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) { SubmitInput(sender, new RoutedEventArgs()); e.Handled = true; } }
}
