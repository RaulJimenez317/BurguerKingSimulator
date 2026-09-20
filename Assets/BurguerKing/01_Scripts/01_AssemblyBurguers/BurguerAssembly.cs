using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections.Generic;

public class BurgerAssembly : MonoBehaviour
{
    [Header("PEDIDO ACTUAL")]
    public RecipeData recipe;

    [Header("HAMBURGUESA")]
    public GameObject burgerStack;
    public GameObject finishedBurger;

    private readonly List<string> placedIngredients =
        new List<string>();

    private readonly List<GameObject> placedIngredientObjects =
        new List<GameObject>();

    private bool burgerCompleted = false;

    private bool hasMeat = false;
    private bool meatCookedCorrectly = false;

    private MeatCooking.CookingState meatState =
        MeatCooking.CookingState.Raw;


    public bool BurgerCompleted =>
        burgerCompleted;

    public bool HasMeat =>
        hasMeat;

    public bool MeatCookedCorrectly =>
        meatCookedCorrectly;

    public MeatCooking.CookingState MeatState =>
        meatState;

    public int PlacedIngredientCount =>
        placedIngredients.Count;


    public bool ContainsPlacedIngredient(
        GameObject ingredient)
    {
        if (ingredient == null)
        {
            return false;
        }

        return placedIngredientObjects.Contains(
            ingredient
        );
    }


    public bool IsRecipeCorrect()
    {
        if (!burgerCompleted)
        {
            return false;
        }

        if (recipe == null ||
            recipe.ingredients == null)
        {
            return false;
        }

        if (placedIngredients.Count !=
            recipe.ingredients.Length)
        {
            return false;
        }

        for (int i = 0;
             i < recipe.ingredients.Length;
             i++)
        {
            if (placedIngredients[i] !=
                recipe.ingredients[i])
            {
                return false;
            }
        }

        return true;
    }


    public bool IsMeatCookingCorrect()
    {
        if (recipe == null ||
            recipe.ingredients == null)
        {
            return false;
        }

        bool recipeRequiresMeat =
            false;

        for (int i = 0;
             i < recipe.ingredients.Length;
             i++)
        {
            if (recipe.ingredients[i] ==
                "Meat")
            {
                recipeRequiresMeat =
                    true;

                break;
            }
        }

        if (!recipeRequiresMeat)
        {
            return true;
        }

        return
            hasMeat &&
            meatCookedCorrectly;
    }


    private void OnTriggerStay(
        Collider other)
    {
        if (burgerCompleted)
        {
            return;
        }

        XRGrabInteractable grabInteractable =
            other.GetComponentInParent
                <XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject ingredientObject =
            grabInteractable.gameObject;

        if (ingredientObject == null)
        {
            return;
        }

        if (!ingredientObject.CompareTag(
            "Ingredient"))
        {
            return;
        }

        if (grabInteractable.isSelected)
        {
            return;
        }

        if (placedIngredientObjects.Contains(
            ingredientObject))
        {
            return;
        }

        string ingredientName =
            GetCleanIngredientName(
                ingredientObject.name
            );

        placedIngredientObjects.Add(
            ingredientObject
        );

        placedIngredients.Add(
            ingredientName
        );

        Debug.Log(
            "🥬 Ingrediente colocado: " +
            ingredientName
        );

        PrintCurrentBurger();

        if (ingredientName == "BreadTop")
        {
            CompleteBurger();
        }
    }


    private void OnTriggerExit(
        Collider other)
    {
        if (burgerCompleted)
        {
            return;
        }

        XRGrabInteractable grabInteractable =
            other.GetComponentInParent
                <XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject ingredientObject =
            grabInteractable.gameObject;

        int index =
            placedIngredientObjects.IndexOf(
                ingredientObject
            );

        if (index < 0 ||
            index >= placedIngredients.Count)
        {
            return;
        }

        string ingredientName =
            placedIngredients[index];

        placedIngredientObjects.RemoveAt(
            index
        );

        placedIngredients.RemoveAt(
            index
        );

        Debug.Log(
            "↩ Ingrediente retirado: " +
            ingredientName
        );

        PrintCurrentBurger();
    }


    private void CompleteBurger()
    {
        if (burgerCompleted)
        {
            return;
        }

        CaptureMeatState();

        burgerCompleted =
            true;

        Debug.Log(
            "🍔 Hamburguesa terminada."
        );

        if (IsRecipeCorrect())
        {
            Debug.Log(
                "✅ La hamburguesa coincide con el pedido."
            );
        }
        else
        {
            Debug.Log(
                "❌ La hamburguesa NO coincide con el pedido."
            );
        }

        if (hasMeat)
        {
            Debug.Log(
                "🥩 Estado de la carne guardado: " +
                meatState
            );

            if (meatCookedCorrectly)
            {
                Debug.Log(
                    "✅ La carne está correctamente cocinada."
                );
            }
            else
            {
                Debug.Log(
                    "❌ La carne no está correctamente cocinada."
                );
            }
        }

        Vector3 burgerPosition =
            transform.position;

        Vector3 positionSum =
            Vector3.zero;

        int validIngredients =
            0;

        for (int i = 0;
             i < placedIngredientObjects.Count;
             i++)
        {
            GameObject ingredient =
                placedIngredientObjects[i];

            if (ingredient == null)
            {
                continue;
            }

            positionSum +=
                ingredient.transform.position;

            validIngredients++;
        }

        if (validIngredients > 0)
        {
            burgerPosition =
                positionSum /
                validIngredients;
        }

        if (finishedBurger != null)
        {
            finishedBurger.transform.position =
                burgerPosition +
                Vector3.up * 0.08f;

            finishedBurger.transform.rotation =
                Quaternion.identity;

            finishedBurger.SetActive(
                true
            );

            Rigidbody rb =
                finishedBurger.GetComponent
                    <Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity =
                    Vector3.zero;

                rb.angularVelocity =
                    Vector3.zero;
            }
        }
        else
        {
            Debug.LogWarning(
                "No hay Finished Burger asignado en BurgerAssembly."
            );
        }

        for (int i = 0;
             i < placedIngredientObjects.Count;
             i++)
        {
            GameObject ingredient =
                placedIngredientObjects[i];

            if (ingredient != null)
            {
                Destroy(
                    ingredient
                );
            }
        }

        Debug.Log(
            "🍔 Ingredientes convertidos en hamburguesa terminada."
        );
    }


    private void CaptureMeatState()
    {
        hasMeat =
            false;

        meatCookedCorrectly =
            true;

        meatState =
            MeatCooking.CookingState.Raw;

        bool problematicStateFound =
            false;

        for (int i = 0;
             i < placedIngredientObjects.Count;
             i++)
        {
            if (i >= placedIngredients.Count)
            {
                continue;
            }

            if (placedIngredients[i] !=
                "Meat")
            {
                continue;
            }

            GameObject meatObject =
                placedIngredientObjects[i];

            if (meatObject == null)
            {
                continue;
            }

            hasMeat =
                true;

            MeatCooking meatCooking =
                meatObject.GetComponent
                    <MeatCooking>();

            if (meatCooking == null)
            {
                meatCooking =
                    meatObject
                        .GetComponentInChildren
                        <MeatCooking>();
            }

            if (meatCooking == null)
            {
                meatCookedCorrectly =
                    false;

                if (!problematicStateFound)
                {
                    meatState =
                        MeatCooking
                            .CookingState
                            .Raw;

                    problematicStateFound =
                        true;
                }

                continue;
            }

            MeatCooking.CookingState currentState =
                meatCooking.CurrentState;

            if (currentState !=
                MeatCooking.CookingState.Ready)
            {
                meatCookedCorrectly =
                    false;

                if (!problematicStateFound ||
                    currentState ==
                    MeatCooking.CookingState.Burned)
                {
                    meatState =
                        currentState;

                    problematicStateFound =
                        true;
                }
            }
            else if (!problematicStateFound)
            {
                meatState =
                    currentState;
            }
        }

        if (!hasMeat)
        {
            meatCookedCorrectly =
                false;

            meatState =
                MeatCooking.CookingState.Raw;
        }
    }


    public SaveManager.AssemblySaveData
        CreateSaveData()
    {
        SaveManager.AssemblySaveData data =
            new SaveManager.AssemblySaveData();

        data.hasAssemblyData =
            true;

        data.burgerCompleted =
            burgerCompleted;

        data.hasMeat =
            hasMeat;

        data.meatCookedCorrectly =
            meatCookedCorrectly;

        data.meatState =
            (int)meatState;

        if (finishedBurger != null)
        {
            data.finishedBurgerActive =
                finishedBurger.activeSelf;

            data.finishedBurgerPosition =
                new SaveManager.Vector3Data(
                    finishedBurger
                        .transform
                        .position
                );

            data.finishedBurgerRotation =
                new SaveManager.QuaternionData(
                    finishedBurger
                        .transform
                        .rotation
                );
        }

        data.placedIngredients =
            new List
                <SaveManager.AssemblyIngredientSaveData>();

        for (int i = 0;
             i < placedIngredients.Count;
             i++)
        {
            SaveManager.AssemblyIngredientSaveData
                ingredientData =
                    new SaveManager
                        .AssemblyIngredientSaveData();

            ingredientData.ingredientName =
                placedIngredients[i];

            GameObject ingredientObject =
                null;

            if (i <
                placedIngredientObjects.Count)
            {
                ingredientObject =
                    placedIngredientObjects[i];
            }

            if (ingredientObject != null)
            {
                ingredientData.objectName =
                    GetCleanIngredientName(
                        ingredientObject.name
                    );

                ingredientData.position =
                    new SaveManager.Vector3Data(
                        ingredientObject
                            .transform
                            .position
                    );

                ingredientData.rotation =
                    new SaveManager.QuaternionData(
                        ingredientObject
                            .transform
                            .rotation
                    );

                ingredientData.scale =
                    new SaveManager.Vector3Data(
                        ingredientObject
                            .transform
                            .localScale
                    );

                MeatCooking meatCooking =
                    ingredientObject
                        .GetComponent
                        <MeatCooking>();

                if (meatCooking == null)
                {
                    meatCooking =
                        ingredientObject
                            .GetComponentInChildren
                            <MeatCooking>();
                }

                if (meatCooking != null)
                {
                    ingredientData.meatState =
                        meatCooking.CreateSaveData();
                }
            }
            else
            {
                ingredientData.objectName =
                    placedIngredients[i];
            }

            data.placedIngredients.Add(
                ingredientData
            );
        }

        return data;
    }


    public bool RestoreFromSaveData(
        SaveManager.AssemblySaveData data)
    {
        if (data == null ||
            !data.hasAssemblyData)
        {
            return false;
        }

        ClearAssemblyForRestore();

        burgerCompleted =
            data.burgerCompleted;

        hasMeat =
            data.hasMeat;

        meatCookedCorrectly =
            data.meatCookedCorrectly;

        if (System.Enum.IsDefined(
            typeof(MeatCooking.CookingState),
            data.meatState))
        {
            meatState =
                (MeatCooking.CookingState)
                    data.meatState;
        }
        else
        {
            meatState =
                MeatCooking.CookingState.Raw;
        }

        if (data.placedIngredients != null)
        {
            for (int i = 0;
                 i < data.placedIngredients.Count;
                 i++)
            {
                SaveManager.AssemblyIngredientSaveData
                    ingredientData =
                        data.placedIngredients[i];

                if (ingredientData == null)
                {
                    continue;
                }

                string ingredientName =
                    ingredientData
                        .ingredientName;

                if (string.IsNullOrEmpty(
                    ingredientName))
                {
                    ingredientName =
                        GetCleanIngredientName(
                            ingredientData.objectName
                        );
                }

                placedIngredients.Add(
                    ingredientName
                );

                if (burgerCompleted)
                {
                    placedIngredientObjects.Add(
                        null
                    );

                    continue;
                }

                IngredientDispenser dispenser =
                    FindIngredientDispenser(
                        ingredientData
                    );

                if (dispenser == null ||
                    dispenser.IngredientPrefab == null)
                {
                    placedIngredientObjects.Add(
                        null
                    );

                    Debug.LogWarning(
                        "No se encontró el dispensador del ingrediente: " +
                        ingredientName
                    );

                    continue;
                }

                Vector3 position =
                    transform.position;

                Quaternion rotation =
                    Quaternion.identity;

                Vector3 scale =
                    dispenser
                        .IngredientPrefab
                        .transform
                        .localScale;

                if (ingredientData.position != null)
                {
                    position =
                        ingredientData
                            .position
                            .ToVector3();
                }

                if (ingredientData.rotation != null)
                {
                    rotation =
                        ingredientData
                            .rotation
                            .ToQuaternion();
                }

                if (ingredientData.scale != null)
                {
                    scale =
                        ingredientData
                            .scale
                            .ToVector3();
                }

                GameObject restoredIngredient =
                    dispenser.SpawnRestoredIngredient(
                        position,
                        rotation
                    );

                if (restoredIngredient == null)
                {
                    placedIngredientObjects.Add(
                        null
                    );

                    continue;
                }

                if (!string.IsNullOrEmpty(
                    ingredientData.objectName))
                {
                    restoredIngredient.name =
                        GetCleanIngredientName(
                            ingredientData.objectName
                        );
                }

                restoredIngredient
                    .transform
                    .localScale =
                        scale;

                Rigidbody rb =
                    restoredIngredient
                        .GetComponent
                        <Rigidbody>();

                if (rb != null)
                {
                    rb.linearVelocity =
                        Vector3.zero;

                    rb.angularVelocity =
                        Vector3.zero;
                }

                placedIngredientObjects.Add(
                    restoredIngredient
                );

                if (ingredientData.meatState != null &&
                    ingredientData
                        .meatState
                        .hasMeatCooking)
                {
                    MeatCooking meatCooking =
                        restoredIngredient
                            .GetComponent
                            <MeatCooking>();

                    if (meatCooking == null)
                    {
                        meatCooking =
                            restoredIngredient
                                .GetComponentInChildren
                                <MeatCooking>();
                    }

                    if (meatCooking != null)
                    {
                        meatCooking.RestoreFromSaveData(
                            ingredientData.meatState
                        );
                    }
                }
            }
        }

        if (finishedBurger != null)
        {
            bool showFinishedBurger =
                data.finishedBurgerActive ||
                burgerCompleted;

            if (data.finishedBurgerPosition != null)
            {
                finishedBurger.transform.position =
                    data
                        .finishedBurgerPosition
                        .ToVector3();
            }

            if (data.finishedBurgerRotation != null)
            {
                finishedBurger.transform.rotation =
                    data
                        .finishedBurgerRotation
                        .ToQuaternion();
            }

            finishedBurger.SetActive(
                showFinishedBurger
            );

            Rigidbody finishedRb =
                finishedBurger
                    .GetComponent
                    <Rigidbody>();

            if (finishedRb != null)
            {
                finishedRb.linearVelocity =
                    Vector3.zero;

                finishedRb.angularVelocity =
                    Vector3.zero;
            }
        }

        PrintCurrentBurger();

        Debug.Log(
            "💾 Estado de la hamburguesa restaurado."
        );

        return true;
    }


    private IngredientDispenser
        FindIngredientDispenser(
            SaveManager.AssemblyIngredientSaveData
                ingredientData)
    {
        IngredientDispenser[] dispensers =
            Object.FindObjectsByType
                <IngredientDispenser>(
                    FindObjectsSortMode.None
                );

        string ingredientName =
            GetCleanIngredientName(
                ingredientData.ingredientName
            );

        string objectName =
            GetCleanIngredientName(
                ingredientData.objectName
            );

        for (int i = 0;
             i < dispensers.Length;
             i++)
        {
            IngredientDispenser dispenser =
                dispensers[i];

            if (dispenser == null ||
                dispenser.IngredientPrefab == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(
                    objectName) &&
                dispenser.MatchesIngredient(
                    objectName))
            {
                return dispenser;
            }

            if (!string.IsNullOrEmpty(
                    ingredientName) &&
                dispenser.MatchesIngredient(
                    ingredientName))
            {
                return dispenser;
            }
        }

        return null;
    }


    private void ClearAssemblyForRestore()
    {
        for (int i = 0;
             i < placedIngredientObjects.Count;
             i++)
        {
            GameObject ingredient =
                placedIngredientObjects[i];

            if (ingredient != null)
            {
                Destroy(
                    ingredient
                );
            }
        }

        placedIngredients.Clear();
        placedIngredientObjects.Clear();

        burgerCompleted =
            false;

        hasMeat =
            false;

        meatCookedCorrectly =
            false;

        meatState =
            MeatCooking.CookingState.Raw;

        if (finishedBurger != null)
        {
            finishedBurger.SetActive(
                false
            );
        }
    }


    public void ResetAssembly()
    {
        for (int i = 0;
             i < placedIngredientObjects.Count;
             i++)
        {
            GameObject ingredient =
                placedIngredientObjects[i];

            if (ingredient != null)
            {
                Destroy(
                    ingredient
                );
            }
        }

        placedIngredients.Clear();
        placedIngredientObjects.Clear();

        burgerCompleted =
            false;

        hasMeat =
            false;

        meatCookedCorrectly =
            false;

        meatState =
            MeatCooking.CookingState.Raw;

        if (finishedBurger != null)
        {
            finishedBurger.SetActive(
                false
            );
        }

        Debug.Log(
            "🔄 Zona de armado reiniciada."
        );
    }


    private string GetCleanIngredientName(
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


    private void PrintCurrentBurger()
    {
        if (placedIngredients.Count == 0)
        {
            Debug.Log(
                "Hamburguesa actual: vacía."
            );

            return;
        }

        Debug.Log(
            "Hamburguesa actual: " +
            string.Join(
                " → ",
                placedIngredients
            )
        );
    }
}