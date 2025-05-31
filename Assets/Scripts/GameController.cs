using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text statusText;
    public Button[] gridButtons;
    public GameObject gameOverPanel;
    public Text gameOverText;

    // 结算界面UI
    public Image resultImage;
    public Sprite playerWinSprite;
    public Sprite aiWinSprite;
    public Sprite drawSprite;

    // 音效和BGM
    public AudioSource sfxSource;      // 音效播放器
    public AudioClip placeSoundClip;   // 落子音效

    // UI 按钮引用
    public Button undoButton;

    private char[] grid;  // 用于存储棋盘状态二维数组 （'X', 'O', 或' '）
    private bool isPlayerTurn = true;  // 当前轮次
    private bool isGameOver = false;

    // 计分板TMP
    public TMP_Text totalText;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text drawText;

    // 积分数据
    private int totalGames = 0;
    private int winCount = 0;
    private int loseCount = 0;
    private int drawCount = 0;

    // 悔棋记录
    private Stack<(int index, char value)> moveHistory = new();

    // 入口
    void Start()
    {
        undoButton.interactable = false;
        grid = new char[9]; 
        for (int i = 0; i < grid.Length; i++)
        {
            grid[i] = ' ';
        }
        LoadScore();
        UpdateUI();
        UpdateScoreUI();
    }

    public void OnCellClick(int index)
    {
        if (isGameOver || !isPlayerTurn || grid[index] != ' ')
            return;

        grid[index] = 'X';
        moveHistory.Push((index, 'X'));
        PlayPlaceSound();  
        UpdateUI();

        if (CheckWin())
        {
            StartCoroutine(ShowGameOverAfterDelay(isPlayerTurn ? GameResultDefine.PlayerWin : GameResultDefine.AIWin));
    
        }
        else if (CheckDraw())
        {
            StartCoroutine(ShowGameOverAfterDelay(GameResultDefine.Draw));
        }
        else
        {
            isPlayerTurn = false;
            StartCoroutine(AIPlay()); // 控制权交给 AI
        }
    }

    IEnumerator ShowStatus(string text, Color color)
    {
        statusText.text = text;
        statusText.color = color;

        // 简单缩放动效
        statusText.rectTransform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 4;
            statusText.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
    }

    // 优化一下最后一个棋子的结果 如果立即执行update会导致ui在渲染帧才出现最后一个棋子 但此时GameOver已经弹出 因此用协程做个delay使之平滑
    IEnumerator ShowGameOverAfterDelay(GameResultDefine result)
    {
        yield return null; // 等待一帧，确保 UpdateUI() 的文本和颜色渲染完毕
        yield return new WaitForSeconds(0.3f); 

        GameOver(result);
    }


    void UpdateUI()
    {
        for (int i = 0; i < gridButtons.Length; i++)
        {
            //gridButtons[i].GetComponentInChildren<TMP_Text>().text = grid[i].ToString();
            TMP_Text text = gridButtons[i].GetComponentInChildren<TMP_Text>();

            char c = grid[i];
            text.text = c.ToString();

            if (c == 'X')
            {
                text.color = GameColors.Get(GameColor.Gold); 
            }
            else if (c == 'O')
            {
                text.color = GameColors.Get(GameColor.SkyBlue);  
            }
            else
            {
                text.text = "";  // 空格就不显示字符
            }
        }
  
        if (isPlayerTurn)
            StartCoroutine(ShowStatus(GameTexts.PlayerTurnText, GameColors.Get(GameColor.Gold))); 
        else
            StartCoroutine(ShowStatus(GameTexts.AITurnText, GameColors.Get(GameColor.SkyBlue))); 

    }

    bool CheckWin()
    {
        int[][] winPatterns = new int[][]
        {
            new int[] {0, 1, 2},
            new int[] {3, 4, 5},
            new int[] {6, 7, 8},
            new int[] {0, 3, 6},
            new int[] {1, 4, 7},
            new int[] {2, 5, 8},
            new int[] {0, 4, 8},
            new int[] {2, 4, 6}
        };

        foreach (var pattern in winPatterns)
        {
            if (grid[pattern[0]] != ' ' && grid[pattern[0]] == grid[pattern[1]] && grid[pattern[1]] == grid[pattern[2]])
            {
                return true;
            }
        }
        return false;
    }

    bool CheckDraw()
    {
        foreach (var cell in grid)
        {
            if (cell == ' ')
                return false;
        }
        return true;
    }

    void GameOver(GameResultDefine result)
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        resultImage.sprite = result switch
        {
            GameResultDefine.PlayerWin => playerWinSprite,
            GameResultDefine.AIWin => aiWinSprite,
            _ => drawSprite
        };

        gameOverText.text = GameTexts.GameResultTexts[result];

        totalGames++;
        switch (result)
        {
            case GameResultDefine.PlayerWin: winCount++; break;
            case GameResultDefine.AIWin: loseCount++; break;
            case GameResultDefine.Draw: drawCount++; break;
        }
        SaveScore();
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (totalText != null) totalText.text = $"{GameTexts.TotalLabel}: {totalGames}";
        if (winText != null) winText.text = $"{GameTexts.WinLabel}: {winCount}";
        if (loseText != null) loseText.text = $"{GameTexts.LoseLabel}: {loseCount}";
        if (drawText != null) drawText.text = $"{GameTexts.DrawLabel}: {drawCount}";
    }

    public void RestartGame()
    {
        grid = new char[9];
        for (int i = 0; i < grid.Length; i++) grid[i] = ' ';
        isPlayerTurn = true;
        isGameOver = false;
        moveHistory.Clear();
        gameOverPanel.SetActive(false);
        UpdateUI();

        // 开局禁用悔棋
        moveHistory.Clear();
        undoButton.interactable = false;
    }

    // 悔棋功能 在每次AI落子后使用 推掉AI的这一步和玩家的上一步
    public void UndoLastMove()
    {

        if (moveHistory.Count < 2 || isGameOver) return;

        // 回退 AI 的一步
        var last = moveHistory.Pop();
        grid[last.index] = ' ';

        // 回退玩家的上一步
        var secondLast = moveHistory.Pop();
        grid[secondLast.index] = ' ';

        isGameOver = false;
        gameOverPanel.SetActive(false);
        isPlayerTurn = true;

        undoButton.interactable = false; 
        UpdateUI();
    }

    void SaveScore()
    {
        PlayerPrefs.SetInt("TotalGames", totalGames);
        PlayerPrefs.SetInt("Wins", winCount);
        PlayerPrefs.SetInt("Losses", loseCount);
        PlayerPrefs.SetInt("Draws", drawCount);
        PlayerPrefs.Save();
    }

    void LoadScore()
    {
        totalGames = PlayerPrefs.GetInt("TotalGames", 0);
        winCount = PlayerPrefs.GetInt("Wins", 0);
        loseCount = PlayerPrefs.GetInt("Losses", 0);
        drawCount = PlayerPrefs.GetInt("Draws", 0);
    }

    IEnumerator AIPlay()
    {
        yield return new WaitForSeconds(1);
        int move = GetBestMove();
        grid[move] = 'O';
        moveHistory.Push((move, 'O'));
        PlayPlaceSound();
        UpdateUI();

        if (CheckWin())
            StartCoroutine(ShowGameOverAfterDelay(GameResultDefine.AIWin));
        else if (CheckDraw())
            StartCoroutine(ShowGameOverAfterDelay(GameResultDefine.Draw));
        else
            isPlayerTurn = true;


        // AI 走完，允许悔棋
        undoButton.interactable = true;
    }

    void PlayPlaceSound()
    {
        if (sfxSource != null && placeSoundClip != null)
        {
            sfxSource.PlayOneShot(placeSoundClip);
        }
    }

    int GetBestMove()
    {
        // 使用简单的MinMax算法来选择AI的最佳移动
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] == ' ')
            {
                grid[i] = 'O';
                if (CheckWin()) return i;
                grid[i] = ' ';
            }
        }

        // 防御性：阻止玩家获胜
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] == ' ')
            {
                grid[i] = 'X';
                if (CheckWin()) return i;
                grid[i] = ' ';
            }
        }

        // 最后选择一个随机空格
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] == ' ') return i;
        }

        return -1;  // 无法走棋时返回
    }
}
