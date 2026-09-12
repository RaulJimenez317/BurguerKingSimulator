using UnityEngine;
using System.Collections.Generic;

public class FriesDispenser : MonoBehaviour
{
    public GameObject friesBagPrefab;
    public Transform spawnPoint;

    public int maxBags = 3;

    private List<GameObject> spawnedBags = new List<GameObject>();

    public void SpawnBag()
    {
        spawnedBags.RemoveAll(item => item == null);

        if (spawnedBags.Count >= maxBags)
        {
            return;
        }

        GameObject newBag = Instantiate(
            friesBagPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        newBag.name = friesBagPrefab.name;

        spawnedBags.Add(newBag);

    }
}