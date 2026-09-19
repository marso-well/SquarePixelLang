namespace SqPiLang;

using System.Numerics;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using SDL2;

public class Tools
{
    public static (byte r, byte g, byte b) HexToRgb(string hex)
    {
        hex = hex.TrimStart('#');
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return (r, g, b);
    }

    public static bool IsHexDigits(string s)
    {
        return Regex.IsMatch(s, @"^[0-9A-Fa-f]+$");
    }

    public static byte HexToByte(String hex)
    {
        return Convert.ToByte(hex, 16);
    }

    public static String ByteToHex(Byte b)
    {
        return Convert.ToHexString([b]);
    }

    public static byte IntToByte(int i)
    {
        if (i < 0 || i > 255)
            throw new ArgumentOutOfRangeException("Value must be between 0 and 255.");
        return (byte)i;
    }
    public static byte StringToByte(String s)
    {
        if (byte.TryParse(s, out byte result))
        {
            return result;
        }
        else
        {
            return 0;
        }
    }

    public static (byte x, byte y) ByteToPos(byte b)
    {
        
        byte x = (byte)((int)b % Engine.FB_W);
        byte y = (byte)((int)b / Engine.FB_W);

        return (x, y);
    }

    public static SDL.SDL_Scancode GetScancode(char name)
    {
        string fullName = "SDL_SCANCODE_" + name.ToString().ToUpper();

        if (Enum.TryParse(fullName, out SDL.SDL_Scancode result))
            return result;

        return SDL.SDL_Scancode.SDL_SCANCODE_UNKNOWN;
    }
}

public static class Input
{
    static HashSet<SDL.SDL_Scancode> prevKeys = new();
    static HashSet<SDL.SDL_Scancode> currKeys = new();
    static HashSet<SDL.SDL_Scancode> consumed = new();

    public static void Update()
    {
        prevKeys = currKeys;
        currKeys = new HashSet<SDL.SDL_Scancode>();
        consumed.Clear();

        IntPtr keys = SDL.SDL_GetKeyboardState(out int numKeys);
        for (int i = 0; i < numKeys; i++)
        {
            if (Marshal.ReadByte(keys, i) == 1)
                currKeys.Add((SDL.SDL_Scancode)i);
        }
    }

    public static bool IsKeyDown(SDL.SDL_Scancode scancode) => currKeys.Contains(scancode);

    public static bool IsKeyPressed(SDL.SDL_Scancode scancode) =>
        currKeys.Contains(scancode) && !prevKeys.Contains(scancode);

    public static bool ConsumeKeyPressed(SDL.SDL_Scancode scancode)
    {
        if (IsKeyPressed(scancode) && !consumed.Contains(scancode))
        {
            consumed.Add(scancode);
            return true;
        }
        return false;
    }

    // Scans Palette.Characters for any key that was just pressed, consuming it.
    public static bool TryGetAnyKeyPressed(out char pressedChar)
    {
        foreach (char c in Palette.Characters)
        {
            SDL.SDL_Scancode sc = Tools.GetScancode(c);
            if (ConsumeKeyPressed(sc))
            {
                pressedChar = c;
                return true;
            }
        }
        pressedChar = '\0';
        return false;
    }
}