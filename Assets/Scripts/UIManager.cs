using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private CardController cardController;

    public void OnStartGameClicked()
    {
        homePanel.SetActive(false);
        difficultyPanel.SetActive(true);
    }


    public void OnClickLevelOne()
    {
     
        cardController.ClearGrid();
        difficultyPanel.SetActive(false);
        cardController.gameObject.SetActive(true);
        cardController.SetData(2, 2);

    }

    public void OnClickLevelTwo()
    {

        cardController.ClearGrid();
        difficultyPanel.SetActive(false);
        cardController.gameObject.SetActive(true);
        cardController.SetData(3, 4);

    }

    public void OnClickLevelThree()
    {

       cardController.ClearGrid();
        difficultyPanel.SetActive(false);
        cardController.gameObject.SetActive(true);
        cardController.SetData(4, 5);

    }

    public void BackToHome()
    {
        // Hide game panel
        if (cardController != null) cardController.gameObject.SetActive(false);

        // Show home panel
        if (homePanel != null) homePanel.SetActive(true);

}

}

