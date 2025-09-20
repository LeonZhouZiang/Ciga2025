using System.Collections.Generic;
using UnityEngine;

public class Energy : MonoBehaviour
{
    public List<GameObject> energies;

    public void UpdateEnergyIcon(int number)
    {
        for (int i = 0; i < energies.Count; i++)
        {
            if (number > 0)
            {
                number--;
                energies[i].SetActive(true);
            }
            else
            {
                energies[i].SetActive(false);
            }
        }
    }
}
