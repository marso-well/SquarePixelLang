namespace SqPiLang;

using System;
using System.Runtime.InteropServices;
using SDL2;

/*
 * What this does:
 *  - Opens ONE real window at SCREEN_W x SCREEN_H (your actual window size)
 *  - Maintains a tiny logical framebuffer at FB_W x FB_H (e.g. 64x64)
 *  - Each pixel in the framebuffer is just 0 or 1 (off/on - your 2 colors)
 *  - Every frame: clear -> draw into framebuffer -> upscale-blit to window
 *  - Fixed timestep game loop so movement speed is consistent
 *  - Arrow keys move a single pixel around, proving input -> update -> render
 */

class Program
{
    static byte[,] frameBuffer = new byte[Engine.FB_W, Engine.FB_H];

    static (byte r, byte g, byte b)[] paletteRgb = [];

    static UInt16 instructionsPerFrame = 200;

    static void Main(String[] args)
    {   
        InitPalette();
        Interpret.OpenFile(args);
        instructionsPerFrame = args.Length > 1 ? UInt16.Parse(args[1]) : (UInt16)200;

        if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine($"SDL_Init failed: {SDL.SDL_GetError()}");
            return;
        }

        IntPtr window = SDL.SDL_CreateWindow(
            Engine.Title,
            SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            Engine.SCREEN_W, Engine.SCREEN_H,
            SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN
        );

        IntPtr renderer = SDL.SDL_CreateRenderer(
            window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
            SDL.SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC
        );

        bool running = true;
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        double accumulator = 0.0;
        const double FIXED_STEP = 1.0 / 120.0;
        double lastTime = sw.Elapsed.TotalSeconds;
        while (running)
        {
            while (SDL.SDL_PollEvent(out SDL.SDL_Event e) != 0)
            {
                if (e.type == SDL.SDL_EventType.SDL_QUIT)
                    running = false;
            }

            double now = sw.Elapsed.TotalSeconds;
            double frameTime = now - lastTime;
            lastTime = now;
            accumulator += frameTime;

            Input.Update();

            while (accumulator >= FIXED_STEP)
            {   
                Update(FIXED_STEP);
                accumulator -= FIXED_STEP;
            }

            Render(renderer);
        }

        SDL.SDL_DestroyRenderer(renderer);
        SDL.SDL_DestroyWindow(window);
        SDL.SDL_Quit();
    }

    static void InitPalette()
    {
        paletteRgb = new (byte, byte, byte)[Palette.Colors.Count];
        for (int i = 0; i < Palette.Colors.Count; i++)
            paletteRgb[i] = Tools.HexToRgb(Palette.Colors[i]);
    }

    static void Update(double dt)
    {   
        if (Interpret.logBuffer.Length > 0)
        {
            Console.Write(Interpret.logBuffer.ToString());
            Interpret.logBuffer.Clear();
        }

        for (int i = 0; i < instructionsPerFrame; i++)
        {   
            Interpret.Step();
        }
    }

    public static void SetPixel(int x, int y, byte value)
    {
        if (x < 0 || x >= Engine.FB_W || y < 0 || y >= Engine.FB_H) return;
        frameBuffer[x, y] = value;
    }

    static void Render(IntPtr renderer)
    {
        (byte r, byte g, byte b) clear_rgb_code = paletteRgb[0];

        SDL.SDL_SetRenderDrawColor(renderer, clear_rgb_code.r, clear_rgb_code.g, clear_rgb_code.b, 255);
        SDL.SDL_RenderClear(renderer);

        for (int y = 0; y < Engine.FB_H; y++)
        {
            for (int x = 0; x < Engine.FB_W; x++)
            {
                (byte r, byte g, byte b) rgb = paletteRgb[frameBuffer[x, y]];
                SDL.SDL_SetRenderDrawColor(renderer, rgb.r, rgb.g, rgb.b, 255);

                var rect = new SDL.SDL_Rect
                    {
                        x = x * Engine.SCALE,
                        y = y * Engine.SCALE,
                        w = Engine.SCALE,
                        h = Engine.SCALE
                    };
                    SDL.SDL_RenderFillRect(renderer, ref rect);
            }
        }
        SDL.SDL_RenderPresent(renderer);
    }
}