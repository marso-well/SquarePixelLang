namespace SqPiLang;

using System;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SDL2;

public class Interpret
{   
    static byte storeValue = 0;
    static byte pointer = 0;
    static uint lineNumber = 1;
    static bool? previousDetect = null;
    static string currentScriptPath = "";
    
    static byte[,] pixelValues = new byte[Engine.FB_W, Engine.FB_H];

    static String[] lines = [];

    static String[][] tokenizedLines = [];

    public static StringBuilder logBuffer = new();


    class ScriptFrame
    {
        public String[] Lines;
        public String[][] TokenizedLines;
        public uint LineNumber;
        public string ScriptPath;
    }

    static Stack<ScriptFrame> callStack = new();



    public static void OpenFile(String[] args)
    {
        if (args.Length == 0) HandleException("No script file was provided");

        currentScriptPath = Path.GetFullPath(args[0]);
        LoadScript(currentScriptPath);
    }

    static void LoadScript(string path)
    {
        try
        {
            lines = File.ReadAllLines(path);
            tokenizedLines = new String[lines.Length][];
            for (int i = 0; i < lines.Length; i++)
            {
                tokenizedLines[i] = lines[i].Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
        }
        catch (IOException)
        {
            HandleException($"File does not exist: {path}");
        }
    }

    
    public static void Step()
    {
        if (lineNumber >= lines.Length)
        {
            if (callStack.Count > 0)
            {
                var frame = callStack.Pop();
                lines = frame.Lines;
                tokenizedLines = frame.TokenizedLines;
                lineNumber = frame.LineNumber;
                currentScriptPath = frame.ScriptPath;
            }
            else
            {
                lineNumber = 0;
            }
            return;
        }

        if (lineNumber == 0)
        {
            if (tokenizedLines[0][0] == "scriptstart" && tokenizedLines[0].Length == 1) { lineNumber++; return; }
            else { HandleException($"First line must be \"scriptstart\". Line: {lineNumber}"); return; }
        }

        String[] tokenLine = tokenizedLines[lineNumber];
        lineNumber++;

        if (tokenLine.Length == 0) return;
        if (tokenLine[0].StartsWith("#") || tokenLine[0].Trim() == "") return;
        Execute(tokenLine);
    }

    static void Execute(String[] tokens)
    {
        switch (tokens[0])
            {
                case "pixel": Pixel(tokens[1..]); break;
                
                case "pixelvalue": PixelValue(tokens[1..]); break;

                case "pixeldraw": if (tokens.Length != 1) HandleException($"\"pixeldraw\" needs no arguments. Line: {lineNumber}"); PixelDraw(); break;

                case "log": if (tokens.Length != 1) HandleException($"\"log\" needs no arguments. Line: {lineNumber}"); Log(); break;

                case "logchar": if (tokens.Length != 1) HandleException($"\"logchar\" needs no arguments. Line: {lineNumber}"); LogChar(); break;

                case "store": if (tokens.Length != 1) HandleException($"\"store\" needs no arguments. Line: {lineNumber}"); Store(); break;

                case "restore": if (tokens.Length != 1) HandleException($"\"restore\" needs no arguments. Line: {lineNumber}"); Restore(); break;

                case "reset": if (tokens.Length != 1) HandleException($"\"reset\" needs no arguments. Line: {lineNumber}"); Reset(); break;

                case "clear": Clear(tokens[1..]); break;

                case "detectinput": DetectInput(tokens[1..]); break;

                case "return": Return(); break;

                case "detectpixel": DetectPixel(tokens[1..]); break;

                case "detectpixelnum": DetectPixelNum(tokens[1..]); break;

                case "waitinput": WaitInput(tokens[1..]); break;

                case "resetdetect": if (tokens.Length != 1) HandleException($"\"resetdetect\" needs no arguments. Line: {lineNumber}"); ResetDetect(); break;

                case "jump": Jump(tokens[1..]); break;

                case "runscript": RunScript(tokens[1..]); break;

                case "_": if (previousDetect == true) Execute(tokens[1..]); break;

                case "!_": if (previousDetect == false) Execute(tokens[1..]); break;

                case "quit": Environment.Exit(0); break;

                case "#": break;

                default: if (tokens[0] != "") HandleException($"Unknown instruction. Line: {lineNumber}"); break;
            }
    }

    static void Pixel(String[] tokens)
    {   
        if ((tokens.Length != 1) && (tokens.Length != 2))
        {
            HandleException($"\"pixel\" needs 1 or 2 arguments. Line: {lineNumber}");
            return;
        }

        bool notByte = false;

        if (tokens[0] == ">") {
            if (pointer != 255) {pointer += 1;}
            else {pointer = 0;}
            notByte = true;
            }
        else if (tokens[0] == "<") {
            if (pointer != 0) {pointer -= 1;}
            else {pointer = 255;}
            notByte = true;
            }
        else if (tokens[0] == "^") {if (pointer > 15) {pointer -= 16;}; notByte = true;}
        else if (tokens[0] == "v") {if (pointer < 240) {pointer += 16;}; notByte = true;}
        
        if (!notByte && !byte.TryParse(tokens[0], out pointer))
        {
            HandleException($"\"pixel\" argument must be 0-255, got \"{tokens[0]}\". Line: {lineNumber}");
            return;
        }

        if ((tokens.Length == 2)) {PixelValue(tokens[1..]); return;}
    }

    static void PixelValue(String[] tokens)
    {  
        if (tokens.Length != 1)
        {
            HandleException($"\"pixelvalue\" needs exactly 1 argument. Line: {lineNumber}");
            return;
        }

        (byte x, byte y) pp = Tools.ByteToPos(pointer);

        if (tokens[0] == "++") {
            if (pixelValues[pp.x, pp.y] < Palette.tableLen-1)
            {
                pixelValues[pp.x, pp.y]++;
            }
            else {pixelValues[pp.x, pp.y] = 0;}
            return;
            }
        else if (tokens[0] == "--") {
            if (pixelValues[pp.x, pp.y] > 0)
            {
                pixelValues[pp.x, pp.y]--;
            }
            else {pixelValues[pp.x, pp.y] = Tools.IntToByte(Palette.tableLen - 1);}
            return;
            }
        else if (tokens[0] == "INPUT")
        {
            if (!Input.TryGetAnyKeyPressed(out char pressed))
            {
                lineNumber -= 1; // no key yet — stay on this line, try again next Step()
                return;
            }

            byte b = (byte)Palette.Characters.IndexOf(pressed);
            pixelValues[pp.x, pp.y] = b;
            return;
        }

        if (Tools.IsHexDigits(tokens[0]))
        {
            byte b = Tools.HexToByte(tokens[0]);

            if (b >= Palette.tableLen)
            {
                HandleException($"Value out of range. Line: {lineNumber}");
                return;
            }

            pixelValues[pp.x, pp.y] = b;
        }
        else
        {
            HandleException($"Argument is not valid. Line: {lineNumber}");
            return;
        }
    }

    static void PixelDraw()
    {
        for (int x = 0; x < 16; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                Program.SetPixel(x, y, pixelValues[x, y]);
            }
        }

    }

    static void Log()
    {   
        (byte x, byte y) pp = Tools.ByteToPos(pointer);
        logBuffer.Append(Tools.ByteToHex(pixelValues[pp.x, pp.y]));
    }

    static void LogChar()
    {   
        (byte x, byte y) pp = Tools.ByteToPos(pointer);
        logBuffer.Append(Palette.Characters[pixelValues[pp.x, pp.y]].ToString());
    }

    static void Store()
    {   
        (byte x, byte y) pp = Tools.ByteToPos(pointer);
        storeValue = pixelValues[pp.x, pp.y];
    }

    static void Restore()
    {
        (byte x, byte y) pp = Tools.ByteToPos(pointer);
        pixelValues[pp.x, pp.y] = storeValue;
    }

    static void Reset()
    {
        (byte x, byte y) pp = Tools.ByteToPos(pointer);
        pixelValues[pp.x, pp.y] = 0;
    }

    static void Clear(String[] tokens)
    {   
        if (!(tokens.Length == 1)) HandleException($"Clear needs exactly one argument. Line: {lineNumber}");

        if (Tools.IsHexDigits(tokens[0]))
        {
            byte b = Tools.HexToByte(tokens[0]);

            if (b >= Palette.tableLen)
            {
                HandleException($"Value out of range. Line: {lineNumber}");
                return;
            }

            for (int y = 0; y < Engine.FB_H; y++)
            {
                for (int x = 0; x < Engine.FB_W; x++)
                {
                    pixelValues[x, y] = b;
                } 
            }
        }
        else
        {
            HandleException($"Argument is not valid. Line: {lineNumber}");
            return;
        }
    }

    static void DetectInput(String[] tokens)
    {   
        bool not = false;

        if (!(tokens.Length > 2)) HandleException($"Not enough arguments. Line: {lineNumber}");

        if (!(tokens[1] == "-->"))
        {
            HandleException($"No Arrow in \"detectinput\". Line: {lineNumber}");
            return;
        }

        char key = Palette.Characters[Tools.HexToByte(tokens[0].TrimStart('!'))];

        
        if (tokens[0].StartsWith("!")) not = true;

        if (Input.IsKeyDown(Tools.GetScancode(key)) == !not)
        {   
            previousDetect = true;
            Execute(tokens[2..]);
            return;
        }
        previousDetect = false;
        return;
    }

    static void DetectPixel(String[] tokens)
    {   
        bool not = false;
        
        if (!(tokens.Length > 2)) HandleException($"Not enough arguments. Line: {lineNumber}");

        if (!(tokens[1] == "-->"))
        {
            HandleException($"No Arrow in \"detectpixel\". Line: {lineNumber}");
            return;
        }

        if (tokens[0].StartsWith("!")) not = true;

        (byte x, byte y) pp = Tools.ByteToPos(pointer);
        if (pixelValues[pp.x, pp.y] == Tools.HexToByte(tokens[0].TrimStart('!')) == !not)
        {
            previousDetect = true;
            Execute(tokens[2..]);
            return;
        }

        previousDetect = false;
        return;
    }

    static void DetectPixelNum(String[] tokens)
    {   
        bool not = false;
        
        if (!(tokens.Length > 2)) HandleException($"Not enough arguments. Line: {lineNumber}");

        if (!(tokens[1] == "-->"))
        {
            HandleException($"No Arrow in \"detectpixelnum\". Line: {lineNumber}");
            return;
        }

        if (tokens[0].StartsWith("!")) not = true;

        if (pointer == Tools.StringToByte(tokens[0].TrimStart('!')) == !not)
        {
            previousDetect = true;
            Execute(tokens[2..]);
            return;
        }

        previousDetect = false;
        return;
    }

    static void ResetDetect()
    {
        previousDetect = null;
    }

    static void Jump(String[] tokens)
    {
        if (uint.TryParse(tokens[0], out lineNumber) && lineNumber < lines.Length){lineNumber--; return;}
        else {HandleException($"Invalid line number. Line: {lineNumber}");}
    }

    static void RunScript(String[] tokens)
    {
        if (tokens.Length != 1)
        {
            HandleException($"\"runscript\" needs exactly one argument. Line: {lineNumber}");
            return;
        }

        string scriptPath = tokens[0];
        if (!Path.IsPathRooted(scriptPath))
            scriptPath = Path.Combine(Path.GetDirectoryName(currentScriptPath) ?? Directory.GetCurrentDirectory(), scriptPath);

        if (!scriptPath.EndsWith(".sqpi", StringComparison.OrdinalIgnoreCase))
        {
            HandleException($"\"runscript\" path must point to a .sqpi file. Line: {lineNumber}");
            return;
        }

        scriptPath = Path.GetFullPath(scriptPath);

        callStack.Push(new ScriptFrame
        {
            Lines = lines,
            TokenizedLines = tokenizedLines,
            LineNumber = lineNumber,
            ScriptPath = currentScriptPath
        });

        currentScriptPath = scriptPath;
        LoadScript(currentScriptPath);
        lineNumber = 0;
    }
    
    static void Return()
    {
        if (callStack.Count > 0)
        {
            var frame = callStack.Pop();
            lines = frame.Lines;
            tokenizedLines = frame.TokenizedLines;
            lineNumber = frame.LineNumber;
            currentScriptPath = frame.ScriptPath;
        }
        else
        {
            HandleException($"\"return\" used outside of a runscript call. Line: {lineNumber}");
        }
    }

    static void WaitInput(String[] tokens)
    {
        char key = Palette.Characters[Tools.HexToByte(tokens[0].TrimStart('!'))];

        bool not = false;

        if (tokens[0].StartsWith("!")) not = true;

        if (Input.IsKeyDown(Tools.GetScancode(key)) == not)
        {
            lineNumber -= 1;
        }
    }
    
    static void HandleException(String ExcArg)
    {
        throw new ArgumentException(ExcArg);
    }
}