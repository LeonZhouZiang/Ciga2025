using System.Threading.Tasks;
using UnityEngine;

public class PublicOpinion : Card
{
    protected override async Task OnEating(CardSO targetCardSO, int number, float time, bool shit)
    {
        await base.OnEating(targetCardSO, number, time, shit);
        Destroy(gameObject);
    }
}
