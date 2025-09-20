
using System.Linq;
using QFramework;
using UnityEngine;

public class Player : Card
{
    public static Player instance;
    private GameObject hint1;
    private GameObject hint2;
    
    private Energy energyPrefab;
    
    [SerializeField]
    private int energyCountLimit = 5;

    private int energyCount = 5;
    public int EnergyCount
    {
        get { return energyCount; }
        set
        {
            energyCount = Mathf.Min(value, energyCountLimit);
            energyPrefab.UpdateEnergyIcon(energyCount);
            if (energyCount <= 0)
            {
                CoinManager.instance.Die();
            }
        }
    }
    private float energyTimer;
    public float energyCD = 7f;

    private bool hint1Used = false;
    public override void Awake()
    {
        base.Awake();
        instance = this;
        hint1 = GameObject.Find("Hint1");
        hint2 = GameObject.Find("Hint2");
    }

    public void Start()
    {
        energyPrefab = Instantiate((cardSO as PlayerConfig)?.energyPrefab, transform).GetComponent<Energy>();
        energyPrefab.UpdateEnergyIcon(energyCount);
    }

    public override void Update()
    {
        base.Update();
        
        energyTimer += Time.deltaTime;
        if (energyTimer >= energyCD)
        {
            energyTimer -= energyCD;
            EnergyCount++;
        }
    }

    public override void OnBeingConsumed()
    {
        EnergyCount--;
        if(!hint1Used)
            HideHints();
    }

    public override bool TryToEat(Card targetCard)
    {
        ConsumeMethod c = cardSO.consumeMethods.FirstOrDefault(x => x.inputCard == targetCard.cardSO);
        if (c != null)
        {
            if(c.ChooseOutput(out ConsumeMethod.Outcome output))
            {
                targetCard.OnBeingConsumed();
                if (targetCard.cardSO == (cardSO as PlayerConfig).grass)
                {
                    if (((Grass)targetCard).isRipe)
                    {
                        OnEating((cardSO as PlayerConfig).coin, Random.Range(4,9), output.time, true);
                    }
                    else
                        OnEating(output.outputCard, 2, output.time, true);
                }
                
                return true;
            }
            else
            {
                if (targetCard.cardSO == (cardSO as PlayerConfig).coffee)
                {
                    EnergyCount += 2;
                    OnEating(null, 0, c.time, false);
                    targetCard.OnBeingConsumed();
                }
                Debug.Log("No card generated");
            }
        }
        Debug.Log("Unable to eat");
        
        return false;
    }

    public override void ExecuteEffect(CardSO targetCardSo, int number)
    {
        CardManager.instance.SpawnCard(targetCardSo, transform.position, number);
    }
    
    
    public void HideHints()
    {
        Destroy(hint1);
        Destroy(hint2);
    }
}
