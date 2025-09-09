using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardController : MonoBehaviour
{

    [SerializeField]  Card  cardPrefab;   
    [SerializeField]  Transform gridTransform;
    [SerializeField]  Sprite [] sprites;


    private List<Sprite> spritePairs;

    Card firstSelectedCard;
    Card secondSelectedCard;

 


   public void CardClicked(Card card)
   {
        Debug.Log("Card Clicked");
        if(card.isSelected)
        {
            return;
        }
        else
        { 
            card.Show();
            if(firstSelectedCard==null)
            {
                firstSelectedCard=card;
                return;
            }
            if(secondSelectedCard==null)
            {
                secondSelectedCard=card;
                StartCoroutine(CheckMatching(firstSelectedCard,secondSelectedCard));
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

   IEnumerator CheckMatching(Card first, Card second)
   {
        yield return new WaitForSeconds(1f);
        if(first.iconSprite==second.iconSprite)
            {
                Debug.Log("Match Found");
                firstSelectedCard=null;
                secondSelectedCard=null;
            }
            else
            {
                Debug.Log("No Match");
                first.hide();
                second.hide();
                firstSelectedCard=null;
                secondSelectedCard=null;
            }
   }
   private void CreateCards()
   {
        for(int i=0;i<spritePairs.Count;i++)
        {
         Card card = Instantiate(cardPrefab, gridTransform); // duplicate the same prefab
         card.SetIconSprite(spritePairs[i]);
         card.cardController=this;
        }
   }

    private void PrepareSprite()
    {
        spritePairs= new List<Sprite>();
        for(int i=0;i<sprites.Length;i++)
        {
            spritePairs.Add(sprites[i]);
            spritePairs.Add(sprites[i]);
        }
        spritePairs=ShuffleList(spritePairs);   

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


    // Start is called before the first frame update
    void Start()
    {

        PrepareSprite();
        CreateCards();        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
