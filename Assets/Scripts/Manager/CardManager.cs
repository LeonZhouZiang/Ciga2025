using System;
using QFramework;
using UnityEngine;
using UnityEngine.Serialization;

public class CardManager : MonoBehaviour
{
    public static CardManager instance;
    
    public PlayerConfig playerSO;
    public GameObject cardPrefab;

    public bool test = false;
    public CardSO testSO;
    
    private SpriteRenderer lastCardRenderer;
    
    private void Awake()
    {
        instance = this;
    }

    public void SpawnCard(CardSO cardSO, Vector3 position, int number = 1)
    {
        Type c = GetCard(cardSO.TypeName);
        for (int i = 0; i < number; i++)
        {
            GameObject o = Instantiate(cardPrefab, position, Quaternion.identity);
            if(c != null)
            {
                var addedComponent = o.AddComponent(c);
                Card card = addedComponent as Card;
                if (card != null)
                {
                    card.enabled = true;
                    card.cardSO = cardSO;
                    card.Initialize();
                    card.SpawnMove();
                }
                else
                    Debug.Log("Invalid Card Type");
            }
        }
    }

    public void HandleSortingLayer(SpriteRenderer spriteRenderer)
    {
        if (spriteRenderer != lastCardRenderer)
        {
            if(!lastCardRenderer)
                lastCardRenderer = spriteRenderer;
            lastCardRenderer.sortingOrder = 0;
            spriteRenderer.sortingOrder = 1;
            lastCardRenderer = spriteRenderer;
        }
    }
    
    public void Start()
    {
        if(test)
            SpawnCard(testSO, Vector3.zero, 20);
        
        SpawnCard(playerSO, transform.position);
        
        HandleSortingLayer(Player.instance.transform.GetChild(0).GetComponent<SpriteRenderer>());
        AudioKit.PlayMusic("resources://背景音乐", true, null,null,0.6f);
    }

    public Type GetCard(string TypeName)
    {
        Type componentType = Type.GetType(TypeName + ", Assembly-CSharp");
        
        if (componentType == null)
        {
            Debug.LogError($"找不到类型: {TypeName}");
            return null;
        }

        return componentType;
    }
    
}
