using System.Collections.Generic;
using UnityEngine;

// 游戏结果定义
public enum GameResultDefine
{
    PlayerWin,
    AIWin,
    Draw
}

// 游戏颜色定义
public enum GameColor
{
    Gold,
    SkyBlue,
}

public static class GameColors
{
    private static readonly Dictionary<GameColor, Color32> colorMap = new()
    {
        { GameColor.Gold,      new Color32(244, 192, 106, 255) },
        { GameColor.SkyBlue,   new Color32(127, 178, 229, 255) },
    };

    public static Color32 Get(GameColor color)
    {
        return colorMap[color];
    }
}

// 游戏内用到的文本配置
public static class GameTexts
{
    public const string PlayerTurn = "Player's Turn";
    public const string AITurn = "AI's Turn";

    public const string TotalLabel = "Total";
    public const string WinLabel = "Wins";
    public const string LoseLabel = "Losses";
    public const string DrawLabel = "Draws";

    public const string PlayerTurnText = "Player's Turn";
    public const string AITurnText = "AI's Turn";

    public static readonly Dictionary<GameResultDefine, string> GameResultTexts = new()
    {
        { GameResultDefine.PlayerWin, "你赢了，但我不服！" },
        { GameResultDefine.AIWin,     "你输了，承让了哈！" },
        { GameResultDefine.Draw,      "平局，那再试试吧！" }
    };
}
