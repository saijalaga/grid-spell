using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Card cardPrefab;
    [SerializeField] public Transform gridTransform;
    [SerializeField] public CardGridLayout Panel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private UIManager UIController;

    [Header("Sprites (Assign in Inspector)")]
    [SerializeField] private Sprite[] sprites;  // assign card face sprites in inspector
    private List<Sprite> allSprites;
    private List<Sprite> spritePairs;

    [Header("Game Settings")]
    public int matchPoints = 10;
    public int mismatchPenalty = 2;

    [Header("Audio")]
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    public AudioSource audioSource;

    public int score = 0;
    private int comboCount = 0;

    public int gameRows = 0;
    public int gameCols = 0;

    private Queue<Card> flipQueue = new Queue<Card>();
    private bool processing = false;

    private const string HIGH_SCORE_KEY = "highscore";
    private const string LAST_ROWS_KEY = "lastRows";
    private const string LAST_COLS_KEY = "lastCols";

    void Awake()
    {
        try
        {
            spritePairs = new List<Sprite>();
            allSprites = new List<Sprite>();

            if (sprites != null && sprites.Length > 0)
                allSprites.AddRange(sprites);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Awake failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    void Start()
    {
        try
        {
            LoadProgress();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Start failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void SetData(int rows, int cols)
    {
        try
        {
            if (rows <= 0 || cols <= 0)
            {
                Debug.LogError("SetData called with invalid rows/cols.");
                return;
            }

            gameRows = rows;
            gameCols = cols;

            PrepareSprite(rows, cols);
            CreateCards(rows * cols);
            UpdateScoreUI();

            if (Panel != null)
            {
                Panel.rows = rows;
                Panel.columns = cols;
                Panel.CalculateLayoutInputVertical();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SetData failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void CardClicked(Card card)
    {
        try
        {
            if (card == null || card.isSelected || card.isMatched) return;

            PlaySound(flipSound);
            card.Show();
            flipQueue.Enqueue(card);

            if (!processing)
                StartCoroutine(ProcessFlipQueue());
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CardClicked failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private IEnumerator ProcessFlipQueue()
    {
        processing = true;

        while (flipQueue.Count > 0)
        {
            yield return new WaitForSeconds(0.12f);

            if (flipQueue.Count >= 2)
            {
                Card first = flipQueue.Dequeue();
                Card second = flipQueue.Dequeue();

                if (first == null || second == null || first == second)
                    continue;

                yield return new WaitForSeconds(0.25f);

                bool isMatch = false;
                try
                {
                    if (first.iconSprite == second.iconSprite)
                    {
                        first.MarkMatched();
                        second.MarkMatched();
                        comboCount++;
                        score += matchPoints + comboCount * 2;
                        PlaySound(matchSound);
                        isMatch = true;
                    }
                    else
                    {
                        comboCount = 0;
                        score -= mismatchPenalty;
                        PlaySound(mismatchSound);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"ProcessFlipQueue failed: {ex.Message}\n{ex.StackTrace}");
                }

                if (!isMatch)
                {
                    yield return new WaitForSeconds(0.45f);
                    if (first != null) first.Hide();
                    if (second != null) second.Hide();
                }

                UpdateScoreUI();
                CheckGameOver();
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        processing = false;
    }

    private void CreateCards(int totalCards)
    {
        try
        {
            if (gridTransform == null)
            {
                Debug.LogError("GridTransform is not assigned.");
                return;
            }

            if (spritePairs == null || spritePairs.Count < totalCards)
            {
                Debug.LogError($"Not enough spritePairs ({spritePairs?.Count ?? 0}) for {totalCards} cards.");
                return;
            }

            ClearGrid();

            for (int i = 0; i < totalCards; i++)
            {
                Card card = Instantiate(cardPrefab, gridTransform);
                card.SetIconSprite(spritePairs[i]);
                card.cardController = this;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CreateCards failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void PrepareSprite(int rows, int cols)
    {
        try
        {
            spritePairs = new List<Sprite>();
            allSprites = new List<Sprite>();

            if (sprites != null && sprites.Length > 0)
                allSprites.AddRange(sprites);

            int totalCards = rows * cols;
            int totalPairs = totalCards / 2;

            if (allSprites.Count < totalPairs)
            {
                Debug.LogError($"Not enough sprites! Need {totalPairs}, but only {allSprites.Count} provided.");
                return;
            }

            List<Sprite> chosen = new List<Sprite>();
            System.Random rand = new System.Random();
            while (chosen.Count < totalPairs)
            {
                int idx = rand.Next(0, allSprites.Count);
                Sprite s = allSprites[idx];
                if (!chosen.Contains(s)) chosen.Add(s);
            }

            foreach (var s in chosen)
            {
                spritePairs.Add(s);
                spritePairs.Add(s);
            }

            spritePairs = ShuffleList(spritePairs);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PrepareSprite failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private List<Sprite> ShuffleList(List<Sprite> originalList)
    {
        try
        {
            List<Sprite> randomList = new List<Sprite>();
            System.Random r = new System.Random();
            while (originalList.Count > 0)
            {
                int randomIndex = r.Next(0, originalList.Count);
                randomList.Add(originalList[randomIndex]);
                originalList.RemoveAt(randomIndex);
            }
            return randomList;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ShuffleList failed: {ex.Message}\n{ex.StackTrace}");
            return new List<Sprite>();
        }
    }

    private void UpdateScoreUI()
    {
        try
        {
            if (scoreText != null)
                scoreText.text = "Score: " + Mathf.Max(score, 0);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"UpdateScoreUI failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void SaveProgress()
    {
        try
        {
            int prev = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, Mathf.Max(prev, score));
            PlayerPrefs.SetInt(LAST_ROWS_KEY, gameRows);
            PlayerPrefs.SetInt(LAST_COLS_KEY, gameCols);
            PlayerPrefs.Save();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveProgress failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void LoadProgress()
    {
        try
        {
            int r = PlayerPrefs.GetInt(LAST_ROWS_KEY, 0);
            int c = PlayerPrefs.GetInt(LAST_COLS_KEY, 0);
            int hs = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);

            if (hs > 0 && scoreText != null) scoreText.text = $"High: {hs}";
            if (r > 0 && c > 0) SetData(r, c);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LoadProgress failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void PlaySound(AudioClip clip)
    {
        try
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"PlaySound failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void StartNewGame()
    {
        try
        {
            ClearGrid();
            score = 0;
            comboCount = 0;
            UpdateScoreUI();
            SetData(gameRows, gameCols);
            if (UIController != null && UIController.gameOverPanel != null)
                UIController.gameOverPanel.SetActive(false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"StartNewGame failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void OnClickHomeButton()
    {
        try
        {
            score = 0;
            comboCount = 0;
            UpdateScoreUI();
            ClearGrid();
            if (UIController != null) UIController.BackToHome();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"OnClickHomeButton failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    public void ClearGrid()
    {
        try
        {
            if (gridTransform == null) return;
            for (int i = gridTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(gridTransform.GetChild(i).gameObject);
            }
            flipQueue.Clear();
            processing = false;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ClearGrid failed: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void CheckGameOver()
    {
        try
        {
            if (gridTransform == null) return;

            foreach (Transform t in gridTransform)
            {
                Card c = t.GetComponent<Card>();
                if (c != null && !c.isMatched) return;
            }

            Debug.Log("Game Over!");
            PlaySound(gameOverSound);
            SaveProgress();
            if (UIController != null) UIController.ShowGameOverPanel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CheckGameOver failed: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
