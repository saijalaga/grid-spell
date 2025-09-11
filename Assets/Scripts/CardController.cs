using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CardController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Card cardPrefab;
    [SerializeField] public Transform gridTransform;
    [SerializeField] public CardGridLayout Panel; // your layout script
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private UIManager UIController;

    [Header("Sprites (Editor)")]
    [SerializeField] private Sprite[] sprites; // optional static list
    [SerializeField] private DefaultAsset spriteFolder; // optional: editor folder for sprites
    private List<Sprite> allSprites;

    [Header("Game Settings")]
    public int matchPoints = 10;
    public int mismatchPenalty = 2;

    [Header("Audio")]
    public AudioClip flipSound;
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    public AudioSource audioSource;

    // Runtime state
    private List<Sprite> spritePairs;
    public int score = 0;
    private int comboCount = 0;

    public int gameRows = 0;
    public int gameCols = 0;

    // Queue-based continuous flipping
    private Queue<Card> flipQueue = new Queue<Card>();
    private bool processing = false;

    // For saving
    private const string HIGH_SCORE_KEY = "highscore";
    private const string LAST_ROWS_KEY = "lastRows";
    private const string LAST_COLS_KEY = "lastCols";

    void Awake()
    {
        // Initialize lists
        spritePairs = new List<Sprite>();
        allSprites = new List<Sprite>();

        // If user provided static sprites array, add them to allSprites as fallback
        if (sprites != null && sprites.Length > 0)
        {
            allSprites.AddRange(sprites);
        }
    }

    void Start()
    {
        // Optionally load last session layout
        LoadProgress();
    }

    // Public API: called by UI
    public void SetData(int rows, int cols)
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

    // ----------- Card click / queue processing --------------
    public void CardClicked(Card card)
    {
        if (card == null) return;
        // ignore clicks on already selected or matched cards
        if (card.isSelected || card.isMatched) return;

        // Play flip sound
        PlaySound(flipSound);

        // Show the card (starts flip animation)
        card.Show();

        // enqueue for comparison processing
        flipQueue.Enqueue(card);

        if (!processing)
            StartCoroutine(ProcessFlipQueue());
    }

    private IEnumerator ProcessFlipQueue()
    {
        processing = true;
        while (flipQueue.Count > 0)
        {
            // small delay to allow animation / player perception
            yield return new WaitForSeconds(0.12f);

            // process in pairs; if only one available, wait shortly to let player add more
            if (flipQueue.Count >= 2)
            {
                Card first = flipQueue.Dequeue();
                Card second = flipQueue.Dequeue();

                // guard nulls
                if (first == null || second == null) continue;
                if (first == second)
                {
                    // If same card clicked twice quickly, ignore and continue.
                    continue;
                }

                // allow the flip animation to reach visible state
                yield return new WaitForSeconds(0.25f);

                if (first.iconSprite == second.iconSprite)
                {
                    // match found
                    first.MarkMatched();
                    second.MarkMatched();

                    comboCount++;
                    score += matchPoints + comboCount * 2;
                    PlaySound(matchSound);

                    // optional small matched animation delay
                    yield return new WaitForSeconds(0.12f);
                }
                else
                {
                    // mismatch
                    comboCount = 0;
                    score -= mismatchPenalty;
                    PlaySound(mismatchSound);

                    // Show mismatch for a bit, then hide
                    yield return new WaitForSeconds(0.45f);
                    first.Hide();
                    second.Hide();
                }

                UpdateScoreUI();
                CheckGameOver();
            }
            else
            {
                // only one card in queue: give a short window for more flips
                yield return new WaitForSeconds(0.2f);
            }
        }
        processing = false;
    }

    // ----------- Create / Prepare sprites ---------------
    private void CreateCards(int totalCards)
    {
        if (gridTransform == null)
        {
            Debug.LogError("GridTransform is not assigned.");
            return;
        }

        if (spritePairs == null || spritePairs.Count < totalCards)
        {
            Debug.LogError($"Not enough spritePairs ({spritePairs?.Count ?? 0}) for {totalCards} cards. Aborting CreateCards.");
            return;
        }

        // Clear any existing children just in case
        ClearGrid();

        for (int i = 0; i < totalCards; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform);
            if (card == null)
            {
                Debug.LogError("Failed to instantiate cardPrefab");
                continue;
            }
            card.SetIconSprite(spritePairs[i]);
            card.cardController = this;
        }
    }

    private void PrepareSprite(int rows, int cols)
    {
        // reset
        spritePairs = new List<Sprite>();
        if (allSprites == null) allSprites = new List<Sprite>();

#if UNITY_EDITOR
        // If a folder is assigned in editor, try to load sprites from it
        if (spriteFolder != null)
        {
            allSprites.Clear();
            string folderPath = AssetDatabase.GetAssetPath(spriteFolder);
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite != null) allSprites.Add(sprite);
            }
        }
#endif

        // Fallback: if editor methods didn't populate sprites, try to use the serialized sprites array
        if ((allSprites == null || allSprites.Count == 0) && sprites != null && sprites.Length > 0)
        {
            allSprites = new List<Sprite>(sprites);
        }

        // Runtime fallback: Resources folder. Place sprites in Assets/Resources/Sprites/<folderName>
        if (allSprites == null || allSprites.Count == 0)
        {
            LoadSpritesFromResources("Sprites"); // default folder under Resources
        }

        int totalCards = rows * cols;

        if (totalCards % 2 != 0)
        {
            Debug.LogWarning("Requested total cards is odd; last card will not have a pair. Consider using even total.");
        }

        int totalPairs = totalCards / 2;

        if (allSprites.Count < totalPairs)
        {
            Debug.LogError($"Not enough sprites available. Required pairs: {totalPairs}, Found sprites: {allSprites.Count}");
            return;
        }

        // choose random unique sprites for pairs
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
            spritePairs.Add(s); // pair
        }

        spritePairs = ShuffleList(spritePairs);
        Debug.Log($"Prepared {spritePairs.Count} sprites for {rows}x{cols} grid.");
    }

    private List<Sprite> ShuffleList(List<Sprite> originalList)
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

    private void LoadSpritesFromResources(string resourcesFolder)
    {
        Sprite[] loaded = Resources.LoadAll<Sprite>(resourcesFolder);
        if (loaded != null && loaded.Length > 0)
        {
            allSprites = new List<Sprite>(loaded);
            Debug.Log($"Loaded {loaded.Length} sprites from Resources/{resourcesFolder}");
        }
        else
        {
            allSprites = new List<Sprite>();
            Debug.LogWarning($"No sprites found in Resources/{resourcesFolder}. Place sprites there or assign via editor.");
        }
    }

    // ----------- Score, Game Over & UI ---------------
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + Mathf.Max(score, 0);
    }



    // ----------- Save / Load ---------------
    public void SaveProgress()
    {
        int prev = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, Mathf.Max(prev, score));
        PlayerPrefs.SetInt(LAST_ROWS_KEY, gameRows);
        PlayerPrefs.SetInt(LAST_COLS_KEY, gameCols);
        PlayerPrefs.Save();
        Debug.Log("Progress saved.");
    }

    public void LoadProgress()
    {
        int r = PlayerPrefs.GetInt(LAST_ROWS_KEY, 0);
        int c = PlayerPrefs.GetInt(LAST_COLS_KEY, 0);
        int hs = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        if (hs > 0 && scoreText != null)
        {
            // optionally display high score somewhere - here we append
            scoreText.text = $"High: {hs}";
        }

        if (r > 0 && c > 0)
        {
            // automatically start last grid (optional)
            SetData(r, c);
        }
    }

    // ----------- Utility ---------------
    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    public void StartNewGame()
    {
        ClearGrid();
        score = 0;
        comboCount = 0;
        UpdateScoreUI();
        SetData(gameRows, gameCols);
        if (UIController.gameOverPanel != null) UIController.gameOverPanel.SetActive(false);
    }

    public void OnClickHomeButton()
    {
        score = 0;
        comboCount = 0;
        UpdateScoreUI();
        ClearGrid();
        if (UIController != null) UIController.BackToHome();
    }

    public void ClearGrid()
    {
        if (gridTransform == null) return;
        for (int i = gridTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = gridTransform.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
#if UNITY_EDITOR
            else UnityEditor.EditorApplication.delayCall += () => { if (child != null) DestroyImmediate(child.gameObject); };
#endif
        }
        flipQueue.Clear();
        processing = false;
    }


    private void CheckGameOver()
{
    try
    {
        if (gridTransform == null) return;

        foreach (Transform t in gridTransform)
        {
            if (t == null) continue;
            Card c = t.GetComponent<Card>();
            if (c == null) continue;
            if (!c.isMatched) return; // still unmatched cards remain
        }

        Debug.Log("Game Over!");
        PlaySound(gameOverSound);
        SaveProgress(); // save highscore and last layout

        // Show Game Over UI
        if (UIController != null)
            UIController.ShowGameOverPanel();
    }
    catch (System.Exception ex)
    {
        Debug.LogError("Error checking game over: " + ex.Message);
    }
}

}
