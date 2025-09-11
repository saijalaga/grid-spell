using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private CardController cardController;

    [SerializeField] public GameObject gameOverPanel; 
    public void OnStartGameClicked()
    {
        if (homePanel != null) homePanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(true);
    }

    public void OnClickLevelOne()
    {
        if (cardController == null) return;
        cardController.ClearGrid();
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        cardController.gameObject.SetActive(true);
        cardController.SetData(2, 2);
    }

    public void OnClickLevelTwo()
    {
        if (cardController == null) return;
        cardController.ClearGrid();
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        cardController.gameObject.SetActive(true);
        cardController.SetData(3, 4);
    }

    public void OnClickLevelThree()
    {
        if (cardController == null) return;
        cardController.ClearGrid();
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        cardController.gameObject.SetActive(true);
        cardController.SetData(4, 5);
    }

    public void BackToHome()
    {
        // Hide game panel
        if (cardController != null) cardController.gameObject.SetActive(false);

        // Show home panel
        if (homePanel != null) homePanel.SetActive(true);

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
    }

      public void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
    }
}
