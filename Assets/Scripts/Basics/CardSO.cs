using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Card", menuName = "Card")]
public class CardSO : ScriptableObject
{
    public string cardName;
    public Sprite cardSprite;
    public string TypeName;
    public string description;
    public List<ConsumeMethod> consumeMethods;
    
}

[System.Serializable]
public class ConsumeMethod
{
    [System.Serializable]
    public struct Output
    {
        public CardSO outputCard;
        [Range(0.1f,1)]
        public float probability; 
        public int minInclusive;
        public int maxExclusive;
    }
    
    public CardSO inputCard;
    public List<Output> outputs;
    public float time;
    
    /// <summary>
    /// return false if do not produce card
    /// </summary>
    /// <param name="output"></param>
    /// <returns></returns>
    public bool ChooseOutput(out Outcome output)
    {
        float value = Random.Range(0f, 1f);
        for (int i = 0; i < outputs.Count; i++)
        {
            if (value < outputs[i].probability)
            {
                if (outputs[i].outputCard == null)
                {
                    //no output this time
                    output = default;
                    return false;
                }
                int number = Random.Range(outputs[i].minInclusive, outputs[i].maxExclusive);
                output = new Outcome(outputs[i].outputCard, number, time);
                return true;
            }
            else
            {
                value -= outputs[i].probability;
            }
        }
        //no output card at all
        output = default;
        output.time = time;
        return false;
    }

    public struct Outcome
    {
        public CardSO outputCard;
        public int number;
        public float time;
        public Outcome(CardSO outputCard, int number, float time)
        {
            this.outputCard = outputCard;
            this.number = number;
            this.time = time;
        }
    }
}