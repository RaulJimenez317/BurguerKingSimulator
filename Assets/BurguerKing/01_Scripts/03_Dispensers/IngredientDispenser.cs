using UnityEngine;
using System.Collections.Generic;

public class IngredientDispenser : MonoBehaviour
{
    public GameObject ingredientPrefab;
    public Transform spawnPoint;

    public int maxIngredients = 3;

    private List<GameObject> spawnedIngredients = new List<GameObject>();

    public void SpawnIngredient()
    {
        spawnedIngredients.RemoveAll(item => item == null);

        if (spawnedIngredients.Count >= maxIngredients)
        {
            return;
        }

        GameObject newIngredient = Instantiate(
            ingredientPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        newIngredient.name = ingredientPrefab.name;

        spawnedIngredients.Add(newIngredient);


    }
}