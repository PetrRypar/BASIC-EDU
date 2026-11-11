using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using BasicEdu.App;

internal static class Program
{
    [STAThread]
    static int Main()
    {
        var app = new Application();
        int result = 1;
        app.Startup += async (_, _) =>
        {
            var window = new MainWindow();
            // Exercise the real window, its output and its Stop button handler.
            window.Show();
            try
            {
                var editor = (ICSharpCode.AvalonEdit.TextEditor)window.FindName("Editor");
                var run = (Button)window.FindName("RunButton");
                var stop = (Button)window.FindName("StopButton");
                var step = (Button)window.FindName("StepButton");
                var variables = (DataGrid)window.FindName("VariablesGrid");
                var status = (TextBlock)window.FindName("Status");
                async Task Until(Func<bool> condition)
                {
                    var timer = Stopwatch.StartNew();
                    while (!condition() && timer.Elapsed < TimeSpan.FromSeconds(5)) await Task.Delay(20);
                    if (!condition()) throw new Exception("Časový limit: " + status.Text);
                }
                string? Value(string name) => variables.Items.Cast<object>()
                    .Where(row => (string?)row.GetType().GetProperty("Name")!.GetValue(row) == name)
                    .Select(row => (string?)row.GetType().GetProperty("Value")!.GetValue(row)).SingleOrDefault();
                void Click(Button button) => button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                void AssertLed(int indicator, string expected)
                {
                    var lamp = (System.Windows.Shapes.Ellipse)window.FindName($"Led{indicator}Indicator");
                    var actual = ((System.Windows.Media.SolidColorBrush)lamp.Fill).Color;
                    if (actual != (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(expected))
                        throw new Exception($"LED{indicator}: {actual} místo {expected}");
                }
                editor.Text = "10 LED1(3)\n20 LED2(2)\n30 LED3(4)\n40 CLS\n50 END";
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 20"));
                AssertLed(1, "#FF3030"); AssertLed(2, "#000000"); AssertLed(3, "#000000");
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 30"));
                AssertLed(2, "#FFFF00");
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 40"));
                AssertLed(3, "#00D040");
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 50"));
                AssertLed(1, "#FF3030"); AssertLed(2, "#FFFF00"); AssertLed(3, "#00D040");
                Click(step);
                await Until(() => status.Text == "Program skončil.");
                AssertLed(3, "#00D040");
                editor.Text = "10 INPUT A";
                Click(run);
                await Until(() => ((TextBox)window.FindName("InputBox")).IsEnabled);
                for (int indicator = 1; indicator <= 3; indicator++) AssertLed(indicator, "#000000");
                Click(stop);
                await Until(() => status.Text == "Program byl zastaven.");
                editor.Text = "10 LED1(10)\n20 LED1(0)\n30 END";
                Click(run);
                await Until(() => status.Text == "Program skončil.");
                AssertLed(1, "#000000");
                Console.WriteLine("OK tři LED, krokování, CLS, konec, reset a vypnutí");
                editor.Text = "10 A=1\n20 A=A+2\n30 INPUT \"Jméno: \"; J$\n40 PRINT A; J$\n50 END";
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 20"));
                if (Value("A") != "1") throw new Exception("První krok má nesprávné proměnné.");
                await Task.Delay(100);
                if (Value("A") != "1") throw new Exception("Program pokračoval bez F8.");
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 30"));
                if (Value("A") != "3") throw new Exception("Druhý krok má nesprávné proměnné.");
                Click(step);
                var input = (TextBox)window.FindName("InputBox");
                await Until(() => input.IsEnabled);
                if (step.IsEnabled || run.IsEnabled) throw new Exception("Kroky nesmí přeskočit INPUT.");
                input.Text = "Žofie";
                Click((Button)window.FindName("InputButton"));
                await Until(() => status.Text.StartsWith("Další řádek: 40"));
                if (Value("J$") != "\"Žofie\"") throw new Exception("Chybí textová proměnná INPUT.");
                Click(run);
                await Until(() => status.Text == "Program skončil.");
                if (Value("A") != "3") throw new Exception("Pokračování ztratilo proměnné.");
                Console.WriteLine("OK krokování, proměnné, INPUT a pokračování F5");
                editor.Text = "10 A=7\n20 GOTO 10";
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 20"));
                Click(stop);
                await Until(() => status.Text == "Program byl zastaven.");
                if (Value("A") != "7" || editor.IsReadOnly) throw new Exception("Zastavení kroku neobnovilo editor.");
                Console.WriteLine("OK zastavení při čekání na krok");
                editor.Text = "10 A=1\n20 PRINT 1/0";
                Click(step);
                await Until(() => status.Text.StartsWith("Další řádek: 20"));
                Click(step);
                await Until(() => status.Text.Contains("Dělení nulou"));
                if (editor.IsReadOnly || Value("A") != "1") throw new Exception("Chyba při krokování neukončila běh.");
                Console.WriteLine("OK chyba během krokování");
                editor.Text = "10 FOR I=1 TO 3\n20 IF I=2 THEN 50\n30 NEXT I\n40 PRINT \"Nemá se vypsat\"\n50 END";
                foreach (var expected in new[] { (Line: 20, I: "1"), (Line: 30, I: "1"), (Line: 20, I: "2"), (Line: 50, I: "2") })
                {
                    Click(step);
                    await Until(() => status.Text.StartsWith($"Další řádek: {expected.Line}"));
                    if (Value("I") != expected.I) throw new Exception("Nesprávné krokování FOR/IF/NEXT.");
                }
                Click(step);
                await Until(() => status.Text == "Program skončil.");
                Console.WriteLine("OK krokování cyklu, nepravdivé podmínky a skoku");
                foreach (string source in new[] { "10 PRINT \"Běžím...\"\n20 GOTO 10", "10 COLOR 14,1\n20 PRINT \"Barvy\"\n30 CLS\n40 GOTO 10", "10 INPUT A", "10 LED1(3)\n20 LED2(4)\n30 GOTO 10" })
                {
                    editor.Text = source;
                    run.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    await Task.Delay(1500);
                    var watch = Stopwatch.StartNew();
                    await window.Dispatcher.InvokeAsync(() => stop.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)), DispatcherPriority.Input);
                    while (!run.IsEnabled && watch.Elapsed < TimeSpan.FromSeconds(3)) await Task.Delay(20);
                    if (!run.IsEnabled || status.Text != "Program byl zastaven.") throw new Exception("Zastavení selhalo: " + status.Text);
                    Console.WriteLine($"OK Stop po {watch.ElapsedMilliseconds} ms: {source.Split('\n')[0]}");
                }
                editor.Text = "10 PRINT \"Znovu spuštěno\"\n20 END";
                run.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var timeout = Stopwatch.StartNew();
                while (!run.IsEnabled && timeout.Elapsed < TimeSpan.FromSeconds(3)) await Task.Delay(20);
                if (!run.IsEnabled || status.Text != "Program skončil.") throw new Exception("Opětovné spuštění selhalo.");
                Console.WriteLine("OK opětovné spuštění");
                result = 0;
            }
            catch (Exception ex) { Console.Error.WriteLine(ex); }
            finally
            {
                // The generated test programs are disposable, not user documents.
                typeof(MainWindow).GetField("dirty", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(window, false);
                app.Shutdown();
            }
        };
        app.Run();
        return result;
    }
}
