using UnityEngine;
using System.Collections.Generic;

public class IngredientDispenser : MonoBehaviour
{
    public GameObject ingredientPrefab;
    public Transform spawnPoint;

    [Min(1)]
    public int maxIngredients = 3;

    private readonly List<GameObject> spawnedIngredients =
        new List<GameObject>();

    public GameObject IngredientPrefab =>
        ingredientPrefab;

    public IReadOnlyList<GameObject> SpawnedIngredients =>
        spawnedIngredients;


    public void SpawnIngredient()
    {
        CleanupList();

        if (ingredientPrefab == null)
        {
            Debug.LogWarning(
                "No hay Ingredient Prefab asignado."
            );

            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "No hay Spawn Point asignado."
            );

            return;
        }

        if (spawnedIngredients.Count >=
            Mathf.Max(1, maxIngredients))
        {
            return;
        }

        GameObject newIngredient =
            Instantiate(
                ingredientPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        PrepareIngredient(
            newIngredient
        );

        spawnedIngredients.Add(
            newIngredient
        );
    }


    public GameObject SpawnRestoredIngredient(
        Vector3 position,
        Quaternion rotation)
    {
        if (ingredientPrefab == null)
        {
            Debug.LogWarning(
                "No hay Ingredient Prefab asignado."
            );

            return null;
        }

        CleanupList();

        GameObject newIngredient =
            Instantiate(
                ingredientPrefab,
                position,
                rotation
            );

        PrepareIngredient(
            newIngredient
        );

        RegisterIngredient(
            newIngredient
        );

        return newIngredient;
    }


    public void RegisterIngredient(
        GameObject ingredient)
    {
        if (ingredient == null)
        {
            return;
        }

        CleanupList();

        if (!spawnedIngredients.Contains(
            ingredient))
        {
            spawnedIngredients.Add(
                ingredient
            );
        }
    }


    public void UnregisterIngredient(
        GameObject ingredient)
    {
        if (ingredient == null)
        {
            CleanupList();
            return;
        }

        spawnedIngredients.Remove(
            ingredient
        );

        CleanupList();
    }


    public bool MatchesIngredient(
        string ingredientName)
    {
        if (ingredientPrefab == null ||
            string.IsNullOrEmpty(
                ingredientName))
        {
            return false;
        }

        string prefabName =
            GetCleanName(
                ingredientPrefab.name
            );

        string requestedName =
            GetCleanName(
                ingredientName
            );

        return prefabName ==
               requestedName;
    }


    public List<GameObject>
        GetSpawnedIngredients()
    {
        CleanupList();

        return new List<GameObject>(
            spawnedIngredients
        );
    }


    private void PrepareIngredient(
        GameObject ingredient)
    {
        if (ingredient == null ||
            ingredientPrefab == null)
        {
            return;
        }

        ingredient.name =
            ingredientPrefab.name;

        Rigidbody rb =
            ingredient.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }
    }


    private void CleanupList()
    {
        spawnedIngredients.RemoveAll(
            item =>
                item == null
        );
    }


    private string GetCleanName(
        string objectName)
    {
        if (string.IsNullOrEmpty(
            objectName))
        {
            return "";
        }

        return objectName
            .Replace(
                "(Clone)",
                ""
            )
            .Trim();
    }
}