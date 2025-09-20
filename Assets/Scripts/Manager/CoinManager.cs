using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;
    private int count = 0;
    private int energy = 5;
    public int winAmount;

    public GameObject winScreen;
    public GameObject loseScreen;
    private void Awake()
    {
        instance = this;
    }

    public void AddCoin()
    {
        count++;
        CheckWin();
    }

    public void UpdateEnergy(int amount)
    {
        energy = amount;
    }
    
    private void Win()
    {
        winScreen.SetActive(true);
    }

    public void Die()
    {
        loseScreen.SetActive(true);
    }
    
    private void CheckWin()
    {
        if (count >= winAmount)
        {
            Win();
        }
    }

    private void CheckDead()
    {
        if (energy <= 0)
        {
            Die();
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
}
