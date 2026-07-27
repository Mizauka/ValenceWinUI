using Microsoft.UI.Xaml;
using System;
using System.IO;
using ValenceWinUI.Helpers;

namespace ValenceWinUI;

public partial class App : Application
{
    private static readonly string _logPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ValenceWinUI", "crash.log");

    private Window? _window;

    public App()
    {
        InitializeComponent();

        this.UnhandledException += (_, e) =>
        {
            var msg = $"[FATAL] {DateTime.Now:HH:mm:ss} | UnhandledException: {e.Exception}";
            WriteLog(msg);
            e.Handled = true;
        };
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        try
        {
            WriteLog($"ValenceWinUI starting... Packaged={IsPackaged}, CWD={Environment.CurrentDirectory}");

            _window = new MainWindow();
            WriteLog("MainWindow created OK");

            WindowHelper.TrackWindow(_window);
            WriteLog("Window tracked OK");

            ThemeHelper.Initialize();
            WriteLog($"Theme initialized OK (theme={ThemeHelper.RootTheme})");

            _window.Activate();
            WriteLog("Window activated — GUI is now visible");
        }
        catch (Exception ex)
        {
            WriteLog($"");
            WriteLog($"╔══════════════════════════════════════════╗");
            WriteLog($"║         FATAL STARTUP ERROR             ║");
            WriteLog($"╠══════════════════════════════════════════╣");
            WriteLog($"║ Type: {ex.GetType().FullName}");
            WriteLog($"║ Message: {ex.Message}");
            WriteLog($"╠══════════════════════════════════════════╣");
            foreach (var line in (ex.StackTrace ?? "").Split('\n'))
                WriteLog($"║  {line.Trim()}");
            if (ex.InnerException != null)
            {
                WriteLog($"╠══════ Inner Exception ══════╣");
                WriteLog($"║ Type: {ex.InnerException.GetType().FullName}");
                WriteLog($"║ Message: {ex.InnerException.Message}");
                foreach (var line in (ex.InnerException.StackTrace ?? "").Split('\n'))
                    WriteLog($"║  {line.Trim()}");
            }
            WriteLog($"╚══════════════════════════════════════════╝");
            WriteLog($"");
            WriteLog("Press Enter to exit...");
            Console.ReadLine();
            throw;
        }
    }

    private static bool IsPackaged
    {
        get
        {
            try { return Windows.ApplicationModel.Package.Current != null; }
            catch { return false; }
        }
    }

    private static void WriteLog(string message)
    {
        // 控制台输出
        Console.WriteLine(message);

        // 文件输出
        try
        {
            var dir = Path.GetDirectoryName(_logPath)!;
            Directory.CreateDirectory(dir);
            File.AppendAllText(_logPath, $"{DateTime.Now:O} {message}{Environment.NewLine}");
        }
        catch { }
    }
}
