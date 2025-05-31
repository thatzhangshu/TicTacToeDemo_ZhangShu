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

    // �������UI
    public Image resultImage;
    public Sprite playerWinSprite;
    public Sprite aiWinSprite;
    public Sprite drawSprite;

    // ��Ч��BGM
    public AudioSource sfxSource;      // ��Ч������
    public AudioClip placeSoundClip;   // ������Ч

    // UI ��ť����
    public Button undoButton;

    private char[] grid;  // ���ڴ洢����״̬��ά���� ��'X', 'O', ��' '��
    private bool isPlayerTurn = true;  // ��ǰ�ִ�
    private bool isGameOver = false;

    // �Ʒְ�TMP
    public TMP_Text totalText;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text drawText;

    // ��������
    private int totalGames = 0;
    private int winCount = 0;
    private int loseCount = 0;
    private int drawCount = 0;

    // �����¼
    private Stack<(int index, char value)> moveHistory = new();

    // ���
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
            StartCoroutine(AIPlay()); // ����Ȩ���� AI
        }
    }

    IEnumerator ShowStatus(string text, Color color)
    {
        statusText.text = text;
        statusText.color = color;

        // �����Ŷ�Ч
        statusText.rectTransform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 4;
            statusText.rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
    }

    // �Ż�һ�����һ�����ӵĽ�� �������ִ��update�ᵼ��ui����Ⱦ֡�ų������һ������ ����ʱGameOver�Ѿ����� �����Э������delayʹ֮ƽ��
    IEnumerator ShowGameOverAfterDelay(GameResultDefine result)
    {
        yield return null; // �ȴ�һ֡��ȷ�� UpdateUI() ���ı�����ɫ��Ⱦ���
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
                text.text = "";  // �ո�Ͳ���ʾ�ַ�
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

        // ���ֽ��û���
        moveHistory.Clear();
        undoButton.interactable = false;
    }

    // ���幦�� ��ÿ��AI���Ӻ�ʹ�� �Ƶ�AI����һ������ҵ���һ��
    public void UndoLastMove()
    {

        if (moveHistory.Count < 2 || isGameOver) return;

        // ���� AI ��һ��
        var last = moveHistory.Pop();
        grid[last.index] = ' ';

        // ������ҵ���һ��
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


        // AI ���꣬�������
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
        // ʹ�ü򵥵�MinMax�㷨��ѡ��AI������ƶ�
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] == ' ')
            {
                grid[i] = 'O';
                if (CheckWin()) return i;
                grid[i] = ' ';
            }
        }

        // �����ԣ���ֹ��һ�ʤ
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] == ' ')
            {
                grid[i] = 'X';
                if (CheckWin()) return i;
                grid[i] = ' ';
            }
        }

        // ���ѡ��һ������ո�
        for (int i = 0; i < grid.Length; i++)
        {
            if (grid[i] == ' ') return i;
        }

        return -1;  // �޷�����ʱ����
    }
}
