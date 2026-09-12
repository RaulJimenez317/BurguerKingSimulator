using UnityEngine;
using System.Collections.Generic;

public class DrinkDispenser : MonoBehaviour
{
    public GameObject drinkCupPrefab;
    public Transform spawnPoint;

    public int maxCups = 3;

    private List<GameObject> spawnedCups = new List<GameObject>();

    public void SpawnCup()
    {
        spawnedCups.RemoveAll(item => item == null);

        if (spawnedCups.Count >= maxCups)
        {
            return;
        }

        GameObject newCup = Instantiate(
            drinkCupPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        newCup.name = drinkCupPrefab.name;

        spawnedCups.Add(newCup);

    }
}