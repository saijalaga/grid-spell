using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    [Header("References")]
    public Image frontImage;      // front icon (assign in prefab)
    public GameObject frontFace;  // container for front (optional)
    public GameObject backFace;   // container for back side
    public Button button;         // optional button component
    [HideInInspector] public CardController cardController;

    [Header("State (runtime)")]
    public Sprite iconSprite;
    public bool isSelected = false; // currently flipped face-up
    public bool isMatched = false;  // permanently matched

    // Flip animation parameters
    [Header("Flip settings")]
    public float flipDuration = 0.28f; // total flip time

    private Coroutine flipRoutine = null;

    private void Awake()
    {
        // Ensure UI references are set
        if (frontImage == null && frontFace != null)
        {
            frontImage = frontFace.GetComponentInChildren<Image>();
        }
        if (button == null)
        {
            button = GetComponent<Button>();
        }
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickSelf);
        }
    }

    public void OnClickSelf()
    {
        if (cardController != null)
            cardController.CardClicked(this);
    }

    public void SetIconSprite(Sprite s)
    {
        iconSprite = s;
        if (frontImage != null) frontImage.sprite = iconSprite;
    }

    public void Show()
    {
        if (isMatched || isSelected) return;
        isSelected = true;
        StartFlip(true);
    }

    public void Hide()
    {
        if (isMatched) return;
        isSelected = false;
        StartFlip(false);
    }

    public void MarkMatched()
    {
        isMatched = true;
        isSelected = true;
        // disable interactions
        if (button != null) button.interactable = false;
        // optional matched animation (scale/pulse)
        StartCoroutine(MatchedPulse());
    }

    private IEnumerator MatchedPulse()
    {
        // simple scale pulse
        Vector3 start = transform.localScale;
        Vector3 target = start * 1.08f;
        float t = 0f;
        float dur = 0.12f;
        while (t < dur)
        {
            transform.localScale = Vector3.Lerp(start, target, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
        while (t < dur)
        {
            transform.localScale = Vector3.Lerp(target, start, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = start;
    }

    private void StartFlip(bool showFront)
    {
        if (flipRoutine != null) StopCoroutine(flipRoutine);
        flipRoutine = StartCoroutine(FlipRoutine(showFront));
    }

    private IEnumerator FlipRoutine(bool showFront)
    {
        float half = flipDuration / 2f;
        float elapsed = 0f;

        // Rotate Y from 0 to 90 (hide), swap, then 90 -> 0
        Quaternion startRot = transform.localRotation;
        Quaternion midRot = startRot * Quaternion.Euler(0f, 90f, 0f);

        while (elapsed < half)
        {
            float t = elapsed / half;
            transform.localRotation = Quaternion.Slerp(startRot, midRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = midRot;

        // Swap face visibility at middle
        if (frontFace != null && backFace != null)
        {
            frontFace.SetActive(showFront);
            backFace.SetActive(!showFront);
        }
        else
        {
            // ensure frontImage only visible when showFront
            if (frontImage != null) frontImage.enabled = showFront;
        }

        elapsed = 0f;
        Quaternion endRot = startRot; // rotate back to original orientation
        while (elapsed < half)
        {
            float t = elapsed / half;
            transform.localRotation = Quaternion.Slerp(midRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localRotation = endRot;

        flipRoutine = null;
    }
}
