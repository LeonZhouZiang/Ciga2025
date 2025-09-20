using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDeck : Card
{
    public int price;
    private int currentMoney;
    public TextMeshProUGUI text;
    
    public override void Awake()
    {
        currentMoney = price;
        Initialize();
        //bug, 以后研究
        dragScale = 1.2f;
        Debug.Log(dragScale);
    }

    public void Start()
    {
        
    }

    public override void Update()
    {
        
    }

    public override void Initialize()
    {
        UpdateUI();
        description = cardSO.description;
        originalScale = transform.localScale.x;
    }

    private void UpdateUI()
    {
        text.text = currentMoney.ToString();
    }

    public override bool TryToEat(Card targetCard)
    {
        ConsumeMethod c = cardSO.consumeMethods.FirstOrDefault(x => x.inputCard == targetCard.cardSO);
        if (c != null)
        {
            if(c.ChooseOutput(out ConsumeMethod.Outcome output))
            {
                targetCard.OnBeingConsumed();
                PlayEatSound();
                if (ShouldPop())
                {
                    OnEating(output.outputCard, output.number);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        Debug.Log("Unable to eat");
        
        return false;
    }

    private bool ShouldPop()
    {
        currentMoney -= 1;
        if (currentMoney <= 0)
        {
            currentMoney = price;
            UpdateUI();
            return true;
        }
        else
        {
            UpdateUI();
            return false;
        }

    }

    private void OnEating(CardSO targetCardSO, int number)
    {
        StartCoroutine(OnEatingCoroutine(targetCardSO, number));
    }

    private IEnumerator OnEatingCoroutine(CardSO targetCardSO, int number)
    {
        yield return new WaitForSeconds(0.5f); // 等待0.5秒
        PlayShitSound();
        CardManager.instance.SpawnCard(targetCardSO, transform.position, number);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        
    }

}
