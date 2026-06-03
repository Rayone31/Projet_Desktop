using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DMsound.UI.Wpf.Infrastructure;

/// <summary>
/// Service de hotkeys globales via Win32 RegisterHotKey.
/// Fonctionne même quand la fenêtre DMsound est en arrière-plan.
/// </summary>
public sealed class GlobalHotkeyService : IDisposable
{
    private const int WmHotkey = 0x0312;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private readonly Dictionary<int, string> _registeredKeys = new();
    private HwndSource? _hwndSource;
    private int _nextId = 9000;

    public event Action<string>? HotkeyPressed;

    public void Attach(Window window)
    {
        var helper = new WindowInteropHelper(window);
        _hwndSource = HwndSource.FromHwnd(helper.Handle);
        _hwndSource?.AddHook(WndProc);
    }

    public bool Register(string keyText)
    {
        if (_hwndSource is null)
        {
            return false;
        }

        if (_registeredKeys.ContainsValue(keyText.ToUpperInvariant()))
        {
            return true;
        }

        var vk = KeyTextToVirtualKey(keyText);

        if (vk == 0)
        {
            return false;
        }

        var id = _nextId++;
        var success = RegisterHotKey(_hwndSource.Handle, id, 0, vk);

        if (success)
        {
            _registeredKeys[id] = keyText.ToUpperInvariant();
        }

        return success;
    }

    public void Unregister(string keyText)
    {
        if (_hwndSource is null)
        {
            return;
        }

        var entry = _registeredKeys.FirstOrDefault(kv => kv.Value == keyText.ToUpperInvariant());

        if (entry.Value is null)
        {
            return;
        }

        UnregisterHotKey(_hwndSource.Handle, entry.Key);
        _registeredKeys.Remove(entry.Key);
    }

    public void UnregisterAll()
    {
        if (_hwndSource is null)
        {
            return;
        }

        foreach (var id in _registeredKeys.Keys)
        {
            UnregisterHotKey(_hwndSource.Handle, id);
        }

        _registeredKeys.Clear();
    }

    public void Dispose()
    {
        UnregisterAll();
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WmHotkey && _registeredKeys.TryGetValue(wParam.ToInt32(), out var keyText))
        {
            HotkeyPressed?.Invoke(keyText);
            handled = true;
        }

        return IntPtr.Zero;
    }

    private static uint KeyTextToVirtualKey(string keyText)
    {
        return keyText.ToUpperInvariant() switch
        {
            "F1"  => 0x70,
            "F2"  => 0x71,
            "F3"  => 0x72,
            "F4"  => 0x73,
            "F5"  => 0x74,
            "F6"  => 0x75,
            "F7"  => 0x76,
            "F8"  => 0x77,
            "F9"  => 0x78,
            "F10" => 0x79,
            "F11" => 0x7A,
            "F12" => 0x7B,
            "A"   => 0x41,
            "B"   => 0x42,
            "C"   => 0x43,
            "D"   => 0x44,
            "E"   => 0x45,
            "F"   => 0x46,
            "G"   => 0x47,
            "H"   => 0x48,
            "I"   => 0x49,
            "J"   => 0x4A,
            "K"   => 0x4B,
            "L"   => 0x4C,
            "M"   => 0x4D,
            "N"   => 0x4E,
            "O"   => 0x4F,
            "P"   => 0x50,
            "Q"   => 0x51,
            "R"   => 0x52,
            "S"   => 0x53,
            "T"   => 0x54,
            "U"   => 0x55,
            "V"   => 0x56,
            "W"   => 0x57,
            "X"   => 0x58,
            "Y"   => 0x59,
            "Z"   => 0x5A,
            "D0"  => 0x30,
            "D1"  => 0x31,
            "D2"  => 0x32,
            "D3"  => 0x33,
            "D4"  => 0x34,
            "D5"  => 0x35,
            "D6"  => 0x36,
            "D7"  => 0x37,
            "D8"  => 0x38,
            "D9"  => 0x39,
            _     => 0,
        };
    }
}
