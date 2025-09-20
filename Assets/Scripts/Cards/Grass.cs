using System.Threading.Tasks;
using UnityEngine;

public class Grass : Card
{
    public bool isRipe = false;
    public string newDescription = "Nice";
    

    protected override async Task OnEating(CardSO targetCardSO, int number, float time, bool shit)
    {
        
        base.OnEating(targetCardSO, number, time, false);
        isRipe = true;
        transform.Find("Content").GetComponent<SpriteRenderer>().sprite = (cardSO as GrassConfig)?.ripeSprite;
        description = newDescription;
    }

    public override bool TryToEat(Card targetCard)
    {
        if(isRipe)
            return false;
        else
            return base.TryToEat(targetCard);
    }
}
