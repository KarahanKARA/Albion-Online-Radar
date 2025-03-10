using AlbionRadar.Networking;
using AlbionRadar.Utils;
using SharpPcap;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace AlbionRadar;

public partial class MainForm : Form
{
    private static readonly object LogFileLock = new();

    private readonly PacketHandler _packetHandler = new();
    private readonly DarkModeCS DM = null;
    private static MainForm _mainForm;
    private static SettingForm _settingForm;

    public MainForm()
    {
        System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
        customCulture.NumberFormat.NumberDecimalSeparator = ".";

        Thread.CurrentThread.CurrentCulture = customCulture;

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            string logMessage = $"==================[{DateTime.Now}]==================" + Environment.NewLine +
                                args.ExceptionObject + Environment.NewLine +
                                "=========================================================";

            WriteLogToFile(logMessage);
        };

        InitializeComponent();
        _mainForm = this;

        DM = new DarkModeCS(this);

        Log("Starting Albion Radar...");
        CreateListener();

        new Thread(() =>
        {
            try
            {
                Log("Initializing overlay...");
                Overlay overlay = new();
                overlay.Initialize(IntPtr.Zero);
                overlay.Enable();

                while (true)
                {
                    overlay.Update();
                    Thread.Sleep(4); // prevent burn CPU/GPU
                }
            }
            catch (Exception ex)
            {
                Log($"Overlay error: {ex.Message}");
            }
        }).Start();

        GlobalKeyHandler.Start();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => GlobalKeyHandler.Stop();

        GlobalKeyHandler.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Home)
                ToggleSettingsWindow();
        };

        Log("Initialization complete");
    }

    private void CreateListener()
    {
        try
        {
            CaptureDeviceList allDevices = CaptureDeviceList.Instance;

            if (allDevices.Count == 0)
            {
                Log("No network interfaces found! Make sure WinPcap is installed.");
                return;
            }

            Log($"Found {allDevices.Count} network interfaces");
            foreach (var device in allDevices)
            {
                try
                {
                    device.OnPacketArrival += _packetHandler.HandlePacket;
                    device.Open(DeviceModes.Promiscuous, 5);
                    device.Filter = "udp and (dst port 5056 or src port 5056)";
                    device.StartCapture();
                    Log($"Started capturing on interface: {device.Description}");
                }
                catch (Exception ex)
                {
                    Log($"Failed to start capture on interface {device.Description}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Log($"Error creating network listener: {ex.Message}");
        }
    }

    public static void Log(string message)
    {
        try
        {
            void write()
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string formattedMessage = $"[{timestamp}] {message}";
                _mainForm.rbLog.AppendText(formattedMessage + Environment.NewLine);
                _mainForm.rbLog.ScrollToCaret();
            }

            if (_mainForm.rbLog.InvokeRequired)
                _mainForm.rbLog.Invoke(write);
            else
                write();
        }
        catch
        {
            // Fail silently if logging fails
        }
    }

    private static void WriteLogToFile(string message)
    {
        try
        {
            lock (LogFileLock)
            {
                if (message.Contains("Error") || message.Contains("Exception") ||
                    message.Contains("Starting") || message.Contains("Initialization"))
                {
                    string logFile = "radar_logs.txt";
                    using StreamWriter sw = File.AppendText(logFile);
                    sw.WriteLine(message);
                }
            }
        }
        catch
        {
            // Fail silently if file logging fails
        }
    }


    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        Environment.Exit(Environment.ExitCode);
    }

    public static bool CanShowOverlay()
    {
        return _settingForm is { Visible: true };
    }

    private void bSettings_Click(object sender, EventArgs e)
    {
        ToggleSettingsWindow();
    }

    private void ToggleSettingsWindow()
    {
        if (_settingForm == null)
        {
            _settingForm = new SettingForm();
            _settingForm.Show();

            _settingForm.Closing += (_, _) => { _settingForm = null; };
        }
        else
        {
            _settingForm.Close();
            _settingForm = null;
        }
    }
}
