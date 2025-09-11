using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("References")]
    public Image frontImage;      
    public GameObject frontFace;  
    public GameObject backFace;   
    public Button button;         
    [HideInInspector] public CardController cardController;

    [Header("State (runtime)")]
    public Sprite iconSprite;
    public bool isSelected = false;
    public bool isMatched = false;

    [Header("Flip settings")]
    public float flipDuration = 0.28f;

    private Coroutine flipRoutine = null;

    private void Awake()
    {
        try
        {
            if (frontImage == null && frontFace != null)
                frontImage = frontFace.GetComponentInChildren<Image>();

            if (button == null)
                button = GetComponent<Button>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClickSelf);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Card Awake: " + e.Message);
        }
    }

    public void OnClickSelf()
    {
        try
        {
            if (cardController != null)
                cardController.CardClicked(this);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in OnClickSelf: " + e.Message);
        }
    }

    public void SetIconSprite(Sprite s)
    {
        try
        {
            iconSprite = s;
            if (frontImage != null)
                frontImage.sprite = iconSprite;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in SetIconSprite: " + e.Message);
        }
    }

    public void Show()
    {
        try
        {
            if (isMatched || isSelected) return;
            isSelected = true;
            StartFlip(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Show: " + e.Message);
        }
    }

    public void Hide()
    {
        try
        {
            if (isMatched) return;
            isSelected = false;
            StartFlip(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in Hide: " + e.Message);
        }
    }

    public void MarkMatched()
    {
        try
        {
            isMatched = true;
            isSelected = true;
            if (button != null)
                button.interactable = false;

            StartCoroutine(MatchedPulse());
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in MarkMatched: " + e.Message);
        }
    }

    private IEnumerator MatchedPulse()
    {
        Vector3 start = transform.localScale;
        Vector3 target = start * 1.08f;
        float dur = 0.12f;

        // Scale up
        float t = 0f;
        while (t < dur)
        {
            try
            {
                transform.localScale = Vector3.Lerp(start, target, t / dur);
                t += Time.deltaTime;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during MatchedPulse scale up: " + e.Message);
            }
            yield return null;
        }

        // Scale down
        t = 0f;
        while (t < dur)
        {
            try
            {
                transform.localScale = Vector3.Lerp(target, start, t / dur);
                t += Time.deltaTime;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during MatchedPulse scale down: " + e.Message);
            }
            yield return null;
        }

        transform.localScale = start;
    }

    private void StartFlip(bool showFront)
    {
        try
        {
            if (flipRoutine != null)
                StopCoroutine(flipRoutine);
            flipRoutine = StartCoroutine(FlipRoutine(showFront));
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in StartFlip: " + e.Message);
        }
    }

    private IEnumerator FlipRoutine(bool showFront)
    {
        float half = flipDuration / 2f;
        float elapsed = 0f;

        Quaternion startRot = transform.localRotation;
        Quaternion midRot = startRot * Quaternion.Euler(0f, 90f, 0f);

        // First half rotation
        while (elapsed < half)
        {
            try
            {
                float t = elapsed / half;
                transform.localRotation = Quaternion.Slerp(startRot, midRot, t);
                elapsed += Time.deltaTime;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during first half of FlipRoutine: " + e.Message);
            }
            yield return null;
        }

        transform.localRotation = midRot;

        // Swap faces
        try
        {
            if (frontFace != null && backFace != null)
            {
                frontFace.SetActive(showFront);
                backFace.SetActive(!showFront);
            }
            else if (frontImage != null)
            {
                frontImage.enabled = showFront;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error swapping faces in FlipRoutine: " + e.Message);
        }

        // Second half rotation
        elapsed = 0f;
        Quaternion endRot = startRot;
        while (elapsed < half)
        {
            try
            {
                float t = elapsed / half;
                transform.localRotation = Quaternion.Slerp(midRot, endRot, t);
                elapsed += Time.deltaTime;
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error during second half of FlipRoutine: " + e.Message);
            }
            yield return null;
        }

        transform.localRotation = endRot;
        flipRoutine = null;
    }
}
