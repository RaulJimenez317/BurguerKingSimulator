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

    private readonly List<string> placedIngredients = new List<string>();
    private readonly List<GameObject> placedIngredientObjects = new List<GameObject>();

    private bool burgerCompleted = false;

    private bool hasMeat = false;
    private bool meatCookedCorrectly = false;
    private MeatCooking.CookingState meatState = MeatCooking.CookingState.Raw;

    public bool BurgerCompleted => burgerCompleted;
    public bool HasMeat => hasMeat;
    public bool MeatCookedCorrectly => meatCookedCorrectly;
    public MeatCooking.CookingState MeatState => meatState;

    public bool IsRecipeCorrect()
    {
        if (!burgerCompleted)
        {
            return false;
        }

        if (recipe == null || recipe.ingredients == null)
        {
            return false;
        }

        if (placedIngredients.Count != recipe.ingredients.Length)
        {
            return false;
        }

        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            if (placedIngredients[i] != recipe.ingredients[i])
            {
                return false;
            }
        }

        return true;
    }

    public bool IsMeatCookingCorrect()
    {
        if (recipe == null || recipe.ingredients == null)
        {
            return false;
        }

        bool recipeRequiresMeat = false;

        foreach (string ingredient in recipe.ingredients)
        {
            if (ingredient == "Meat")
            {
                recipeRequiresMeat = true;
                break;
            }
        }

        if (!recipeRequiresMeat)
        {
            return true;
        }

        return hasMeat && meatCookedCorrectly;
    }

    private void OnTriggerStay(Collider other)
    {
        if (burgerCompleted)
        {
            return;
        }

        XRGrabInteractable grabInteractable =
            other.GetComponentInParent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject ingredientObject = grabInteractable.gameObject;

        if (!ingredientObject.CompareTag("Ingredient"))
        {
            return;
        }

        if (grabInteractable.isSelected)
        {
            return;
        }

        if (placedIngredientObjects.Contains(ingredientObject))
        {
            return;
        }

        string ingredientName =
            GetCleanIngredientName(ingredientObject.name);

        placedIngredientObjects.Add(ingredientObject);
        placedIngredients.Add(ingredientName);

        Debug.Log("🥬 Ingrediente colocado: " + ingredientName);

        PrintCurrentBurger();

        if (ingredientName == "BreadTop")
        {
            CompleteBurger();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (burgerCompleted)
        {
            return;
        }

        XRGrabInteractable grabInteractable =
            other.GetComponentInParent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject ingredientObject = grabInteractable.gameObject;

        int index =
            placedIngredientObjects.IndexOf(ingredientObject);

        if (index < 0)
        {
            return;
        }

        string ingredientName = placedIngredients[index];

        placedIngredientObjects.RemoveAt(index);
        placedIngredients.RemoveAt(index);

        Debug.Log("↩ Ingrediente retirado: " + ingredientName);

        PrintCurrentBurger();
    }

    private void CompleteBurger()
    {
        if (burgerCompleted)
        {
            return;
        }

        CaptureMeatState();

        burgerCompleted = true;

        Debug.Log("🍔 Hamburguesa terminada.");

        if (IsRecipeCorrect())
        {
            Debug.Log("✅ La hamburguesa coincide con el pedido.");
        }
        else
        {
            Debug.Log("❌ La hamburguesa NO coincide con el pedido.");
        }

        if (hasMeat)
        {
            Debug.Log("🥩 Estado de la carne guardado: " + meatState);

            if (meatCookedCorrectly)
            {
                Debug.Log("✅ La carne está correctamente cocinada.");
            }
            else
            {
                Debug.Log("❌ La carne no está correctamente cocinada.");
            }
        }

        Vector3 burgerPosition = transform.position;

        Vector3 positionSum = Vector3.zero;
        int validIngredients = 0;

        foreach (GameObject ingredient in placedIngredientObjects)
        {
            if (ingredient != null)
            {
                positionSum += ingredient.transform.position;
                validIngredients++;
            }
        }

        if (validIngredients > 0)
        {
            burgerPosition =
                positionSum / validIngredients;
        }

        if (finishedBurger != null)
        {
            finishedBurger.transform.position =
                burgerPosition + Vector3.up * 0.08f;

            finishedBurger.transform.rotation =
                Quaternion.identity;

            finishedBurger.SetActive(true);

            Rigidbody rb =
                finishedBurger.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            Debug.LogWarning(
                "No hay Finished Burger asignado en BurgerAssembly."
            );
        }

        foreach (GameObject ingredient in placedIngredientObjects)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        Debug.Log(
            "🍔 Ingredientes convertidos en hamburguesa terminada."
        );
    }

    private void CaptureMeatState()
    {
        hasMeat = false;
        meatCookedCorrectly = true;
        meatState = MeatCooking.CookingState.Raw;

        bool problematicStateFound = false;

        for (int i = 0; i < placedIngredientObjects.Count; i++)
        {
            if (i >= placedIngredients.Count)
            {
                continue;
            }

            if (placedIngredients[i] != "Meat")
            {
                continue;
            }

            GameObject meatObject =
                placedIngredientObjects[i];

            if (meatObject == null)
            {
                continue;
            }

            hasMeat = true;

            MeatCooking meatCooking =
                meatObject.GetComponent<MeatCooking>();

            if (meatCooking == null)
            {
                meatCooking =
                    meatObject.GetComponentInChildren<MeatCooking>();
            }

            if (meatCooking == null)
            {
                meatCookedCorrectly = false;

                if (!problematicStateFound)
                {
                    meatState = MeatCooking.CookingState.Raw;
                    problematicStateFound = true;
                }

                continue;
            }

            MeatCooking.CookingState currentState =
                meatCooking.CurrentState;

            if (currentState != MeatCooking.CookingState.Ready)
            {
                meatCookedCorrectly = false;

                if (!problematicStateFound ||
                    currentState == MeatCooking.CookingState.Burned)
                {
                    meatState = currentState;
                    problematicStateFound = true;
                }
            }
            else if (!problematicStateFound)
            {
                meatState = currentState;
            }
        }

        if (!hasMeat)
        {
            meatCookedCorrectly = false;
            meatState = MeatCooking.CookingState.Raw;
        }
    }

    public void ResetAssembly()
    {
        foreach (GameObject ingredient in placedIngredientObjects)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        placedIngredients.Clear();
        placedIngredientObjects.Clear();

        burgerCompleted = false;

        hasMeat = false;
        meatCookedCorrectly = false;
        meatState = MeatCooking.CookingState.Raw;

        if (finishedBurger != null)
        {
            finishedBurger.SetActive(false);
        }

        Debug.Log("🔄 Zona de armado reiniciada.");
    }

    private string GetCleanIngredientName(string objectName)
    {
        return objectName
            .Replace("(Clone)", "")
            .Trim();
    }

    private void PrintCurrentBurger()
    {
        if (placedIngredients.Count == 0)
        {
            Debug.Log("Hamburguesa actual: vacía.");
            return;
        }

        Debug.Log(
            "Hamburguesa actual: " +
            string.Join(" → ", placedIngredients)
        );
    }
}