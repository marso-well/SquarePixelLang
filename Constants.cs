namespace SqPiLang;

using System.Collections.Generic;
using System.Collections.ObjectModel;

public class Palette
{
    public static readonly int tableLen = 96;

    public static readonly ReadOnlyCollection<string> Colors = new List<string>
    {
            "#1F102A",
            "#390947",
            "#4E187C",
            "#4A3052",
            "#751756",
            "#611851",
            "#A32858",
            "#873555",
            "#7D2DA0",
            "#834DC4",
            "#CC425E",
            "#873E84",
            "#A6555F",
            "#7B5480",
            "#EA6262",
            "#5BA675",
            "#C97373",
            "#8465EC",
            "#6D80FA",
            "#D46EB3",
            "#A6859F",
            "#6BC96C",
            "#EE8FCB",
            "#8DB7FF",
            "#F2AE99",
            "#FFB879",
            "#D9BDC8",
            "#AEE2FF",
            "#ABDD64",
            "#FCEF8D",
            "#FFC3F2",
            "#FFFFFF",

            "#5BA675",
            "#6BC96C",
            "#ABDD64",
            "#FCEF8D",
            "#FFB879",
            "#EA6262",
            "#CC425E",
            "#A32858",
            "#751756",
            "#390947",
            "#611851",
            "#873555",
            "#A6555F",
            "#C97373",
            "#F2AE99",
            "#FFC3F2",
            "#EE8FCB",
            "#D46EB3",
            "#873E84",
            "#1F102A",
            "#4A3052",
            "#7B5480",
            "#A6859F",
            "#D9BDC8",
            "#FFFFFF",
            "#AEE2FF",
            "#8DB7FF",
            "#6D80FA",
            "#8465EC",
            "#834DC4",
            "#7D2DA0",
            "#4E187C",

            "#5BA675",
            "#6BC96C",
            "#ABDD64",
            "#FCEF8D",
            "#FFB879",
            "#EA6262",
            "#CC425E",
            "#A32858",
            "#751756",
            "#390947",
            "#611851",
            "#873555",
            "#A6555F",
            "#C97373",
            "#F2AE99",
            "#FFC3F2",
            "#EE8FCB",
            "#D46EB3",
            "#873E84",
            "#1F102A",
            "#4A3052",
            "#7B5480",
            "#A6859F",
            "#D9BDC8",
            "#FFFFFF",
            "#AEE2FF",
            "#8DB7FF",
            "#6D80FA",
            "#8465EC",
            "#834DC4",
            "#7D2DA0",
            "#4E187C"
    }.AsReadOnly();

    public static readonly ReadOnlyCollection<char> Characters = new List<char>
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
        'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',

        'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
        'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ',', '.',

        '[', ']', '{', '}', '<', '>', '!', '@', '#', '$', '%', '^', '&', '*', '(', ')',
        '-', '_', '+', '=', '/', '\\', '|', ':', ';', '\'', '"', '-', '~', '?', '\n', ' '
    }.AsReadOnly();

}

public class Engine
{
    public const String Title = "SqPiEngine";
    public const int FB_W = 16;
    public const int FB_H = 16;
    public const int SCALE = 32;
    public const int SCREEN_W = FB_W * SCALE;
    public const int SCREEN_H = FB_H * SCALE;

}