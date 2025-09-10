using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class Card : MonoBehaviour
{
 
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite;      // Image component showing front (unique per card)
    public Sprite iconSprite; 

    public bool isSelected;

    public CardController cardController;


   public void OnCardClicked()
    {
        cardController.CardClicked(this);
    }

    public void SetIconSprite(Sprite sp)
    {
        iconSprite=sp;
    } 

    public void Show()
    {
            cardController.PlaySound(cardController.flipSound);
            Tween.LocalRotation(transform, new Vector3(0, 180, 0), 0.2f)
            .OnComplete(() => {
                iconImage.sprite = iconSprite;
                isSelected = true;
            });
      
    }

    public void hide()
    {
        
            Tween.LocalRotation(transform, new Vector3(0, 0, 0), 0.2f)
            .OnComplete(() => {
                iconImage.sprite = hiddenIconSprite;        
                isSelected = false;
            });

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
