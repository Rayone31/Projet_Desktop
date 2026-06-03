using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DMsound.UI.Wpf.Infrastructure;

/// <summary>
/// Service de hotkeys globales via SetWindowsHookEx.
/// Les touches restent disponibles partout — DMsound les intercepte sans les bloquer.
/// </summary>
public sealed class GlobalHotkeyService : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeydown = 0x0100;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct KbdllHookStruct
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private readonly HashSet<string> _registeredKeys = new();
    private readonly LowLevelKeyboardProc _hookProc;
    private IntPtr _hookId = IntPtr.Zero;
    private bool _captureMode;

    public event Action<string>? HotkeyPressed;
    public event Action<string>? KeyCaptured;

    public GlobalHotkeyService()
    {
        _hookProc = HookCallback;
    }

    public void Attach(System.Windows.Window window)
    {
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule!;
        _hookId = SetWindowsHookEx(WhKeyboardLl, _hookProc, GetModuleHandle(module.ModuleName!), 0);
    }

    public bool Register(string keyText)
    {
        _registeredKeys.Add(keyText.ToUpperInvariant());
        return true;
    }

    public void Unregister(string keyText)
    {
        _registeredKeys.Remove(keyText.ToUpperInvariant());
    }

    public void UnregisterAll()
    {
        _registeredKeys.Clear();
    }

    public void Dispose()
    {
        UnregisterAll();

        if (_hookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookId);
            _hookId = IntPtr.Zero;
        }
    }

    public void StartCapture()
    {
        _captureMode = true;
    }

    public void StopCapture()
    {
        _captureMode = false;
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == WmKeydown)
        {
            var kbd = Marshal.PtrToStructure<KbdllHookStruct>(lParam);
            var keyText = VirtualKeyToKeyText(kbd.vkCode);

            if (keyText is not null)
            {
                if (_captureMode)
                {
                    _captureMode = false;
                    KeyCaptured?.Invoke(keyText);
                }
                else if (_registeredKeys.Contains(keyText))
                {
                    HotkeyPressed?.Invoke(keyText);
                }
            }
        }

        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    private static string? VirtualKeyToKeyText(uint vk)
    {
        return vk switch
        {
            // Touches de fonction
            0x70 => "F1",
            0x71 => "F2",
            0x72 => "F3",
            0x73 => "F4",
            0x74 => "F5",
            0x75 => "F6",
            0x76 => "F7",
            0x77 => "F8",
            0x78 => "F9",
            0x79 => "F10",
            0x7A => "F11",
            0x7B => "F12",

            // Lettres
            0x41 => "A",
            0x42 => "B",
            0x43 => "C",
            0x44 => "D",
            0x45 => "E",
            0x46 => "F",
            0x47 => "G",
            0x48 => "H",
            0x49 => "I",
            0x4A => "J",
            0x4B => "K",
            0x4C => "L",
            0x4D => "M",
            0x4E => "N",
            0x4F => "O",
            0x50 => "P",
            0x51 => "Q",
            0x52 => "R",
            0x53 => "S",
            0x54 => "T",
            0x55 => "U",
            0x56 => "V",
            0x57 => "W",
            0x58 => "X",
            0x59 => "Y",
            0x5A => "Z",

            // Chiffres rangée du haut
            0x30 => "D0",
            0x31 => "D1",
            0x32 => "D2",
            0x33 => "D3",
            0x34 => "D4",
            0x35 => "D5",
            0x36 => "D6",
            0x37 => "D7",
            0x38 => "D8",
            0x39 => "D9",

            // Pavé numérique
            0x60 => "Num0",
            0x61 => "Num1",
            0x62 => "Num2",
            0x63 => "Num3",
            0x64 => "Num4",
            0x65 => "Num5",
            0x66 => "Num6",
            0x67 => "Num7",
            0x68 => "Num8",
            0x69 => "Num9",
            0x6A => "Multiply",
            0x6B => "Add",
            0x6D => "Subtract",
            0x6E => "Decimal",
            0x6F => "Divide",

            // Navigation
            0x21 => "PageUp",
            0x22 => "PageDown",
            0x23 => "End",
            0x24 => "Home",
            0x25 => "Left",
            0x26 => "Up",
            0x27 => "Right",
            0x28 => "Down",
            0x2D => "Insert",
            0x2E => "Delete",

            // Touches spéciales
            0x20 => "Space",
            0x09 => "Tab",
            0x0D => "Enter",
            0x1B => "Escape",
            0x08 => "Back",

            // Ponctuation / symboles
            0xBA => "OemSemicolon",
            0xBB => "OemPlus",
            0xBC => "OemComma",
            0xBD => "OemMinus",
            0xBE => "OemPeriod",
            0xBF => "OemQuestion",
            0xC0 => "OemTilde",
            0xDB => "OemOpenBrackets",
            0xDC => "OemPipe",
            0xDD => "OemCloseBrackets",
            0xDE => "OemQuotes",

            _ => null,
        };
    }
}
