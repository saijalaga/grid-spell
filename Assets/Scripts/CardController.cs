using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class CardController : MonoBehaviour
{

    [SerializeField] Card cardPrefab;
    [SerializeField] Transform gridTransform;
    [SerializeField] Sprite[] sprites;
    [SerializeField] private DefaultAsset spriteFolder; // drag folder here
    private List<Sprite> allSprites;

    [SerializeField] CardGridLayout Panel;


    [Header("Scoring")]
    public int score = 0;
    public int matchPoints = 10;
    public int mismatchPenalty = 2;
    private int comboCount = 0;
    [Header("Audio")]
    public AudioClip flipSound;        // was private → make public
    public AudioClip matchSound;
    public AudioClip mismatchSound;
    public AudioClip gameOverSound;
    public AudioSource audioSource;
    private List<Sprite> spritePairs;

    Card firstSelectedCard;
    Card secondSelectedCard;

    [SerializeField] private TMP_Text scoreText;

    public int gameRows = 0;
    public int gameCols = 0;

    [SerializeField] UIManager UIController;




    public void CardClicked(Card card)
    {
        Debug.Log("Card Clicked");
        if (card.isSelected)
        {
            return;
        }
        else
        {
            card.Show();
            if (firstSelectedCard == null)
            {
                firstSelectedCard = card;
                return;
            }
            if (secondSelectedCard == null)
            {
                secondSelectedCard = card;
                StartCoroutine(CheckMatching(firstSelectedCard, secondSelectedCard));
            }

            // else
            // {
            //     secondSelectedCard=card;
            //     if(firstSelectedCard.iconSprite==secondSelectedCard.iconSprite)
            //     {
            //         Debug.Log("Match Found");
            //         firstSelectedCard=null;
            //         secondSelectedCard=null;
            //     }
            //     else
            //     {
            //         Debug.Log("No Match");
            //         StartCoroutine(HideCards());
            //     }
            // }
        }

    }


    public void SetData(int rows, int cols)
    {

        PrepareSprite(rows, cols);
        CreateCards(rows * cols);
        UpdateScoreUI();
        Panel.rows = rows;
        Panel.columns = cols;
        Panel.CalculateLayoutInputVertical();
        gameRows = rows;
        gameCols = cols;
    }

    // ---------------- GRID SETUP ----------------

    //    IEnumerator CheckMatching(Card first, Card second)
    //    {
    //         yield return new WaitForSeconds(1f);
    //         if(first.iconSprite==second.iconSprite)
    //             {
    //                 Debug.Log("Match Found");
    //                 firstSelectedCard=null;
    //                 secondSelectedCard=null;
    //             }
    //             else
    //             {
    //                 Debug.Log("No Match");
    //                 first.hide();
    //                 second.hide();
    //                 firstSelectedCard=null;
    //                 secondSelectedCard=null;
    //             }
    //    }


    IEnumerator CheckMatching(Card first, Card second)
    {
        yield return new WaitForSeconds(0.5f);

        if (first.iconSprite == second.iconSprite)
        {
            Debug.Log("Match Found");
            comboCount++;
            score += matchPoints + comboCount * 2; // optional combo bonus
            PlaySound(matchSound); // We'll add sounds next
        }
        else
        {
            Debug.Log("No Match");
            first.hide();
            second.hide();
            comboCount = 0;
            score -= mismatchPenalty;
            PlaySound(mismatchSound);
        }

        firstSelectedCard = null;
        secondSelectedCard = null;

        UpdateScoreUI();
        CheckGameOver();
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
            Debug.LogError("Error updating score UI: " + ex.Message);
        }
    }

    private void CheckGameOver()
    {
        try
        {
            // If all cards are matched
            foreach (Transform t in gridTransform)
            {
                Card c = t.GetComponent<Card>();
                if (!c.isSelected) return; // still cards left
            }

            Debug.Log("Game Over!");
            PlaySound(gameOverSound);
            // Could show Game Over UI here
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error checking game over: " + ex.Message);
        }

    }
    private void CreateCards(int totalCards)
    {
        for (int i = 0; i < totalCards; i++)
        {
            Card card = Instantiate(cardPrefab, gridTransform); // duplicate the same prefab
            card.SetIconSprite(spritePairs[i]);
            card.cardController = this;
        }
    }

    private void PrepareSprites()
    {
        spritePairs = new List<Sprite>();
        for (int i = 0; i < sprites.Length; i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        spritePairs = ShuffleList(spritePairs);

    }

    private List<Sprite> ShuffleList(List<Sprite> originalList)
    {
        List<Sprite> randomList = new List<Sprite>();

        System.Random r = new System.Random();
        int randomIndex = 0;
        while (originalList.Count > 0)
        {
            randomIndex = r.Next(0, originalList.Count); //Choose a random object in the list
            randomList.Add(originalList[randomIndex]); //add it to the new, random list
            originalList.RemoveAt(randomIndex); //remove to avoid duplicates
        }
        return randomList; //return the new random list
    }

    public void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
    // Start is called before the first frame update
    void Start()
    {

        // PrepareSprite();
        // CreateCards();
        // UpdateScoreUI();
        // SetupGrid();     
    }

    // Update is called once per frame
    void Update()
    {

    }



private void PrepareSprite(int rows, int cols)
{
    // Always reset before building
    spritePairs = new List<Sprite>();
    allSprites = new List<Sprite>();

#if UNITY_EDITOR
    if (spriteFolder != null)
    {
        string folderPath = UnityEditor.AssetDatabase.GetAssetPath(spriteFolder);
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) allSprites.Add(sprite);
        }
    }
#endif

    int totalCards = rows * cols;
    int totalPairs = totalCards / 2;

    if (allSprites.Count < totalPairs)
    {
        Debug.LogError($"Not enough sprites in folder! Required: {totalPairs}, Found: {allSprites.Count}");
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

    Debug.Log($"Prepared {spritePairs.Count} sprites for {rows}x{cols} grid.");
}


    public void StartNewGame()
    {
        // Clear old cards if any
        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }

        // Reset score & combo
        score = 0;
        comboCount = 0;
        UpdateScoreUI();

        // Setup new grid
        SetData(gameRows, gameCols);
    }


    public void OnClickHomeButton()
    {

        // Optional: stop game, reset cards, reset score
        score = 0;
        comboCount = 0;
        UpdateScoreUI();

        foreach (Transform child in gridTransform)
        {
            Destroy(child.gameObject);
        }
        UIController.BackToHome();

    }
   
public void ClearGrid()
{
    foreach (Transform child in gridTransform)
    {
        Destroy(child.gameObject);
    }
}


}
