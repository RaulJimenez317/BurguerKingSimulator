using UnityEngine;
using System.Collections.Generic;

public class BurgerAssembly : MonoBehaviour
{
    public RecipeData recipe;
    public GameObject burgerStack;
    public GameObject finishedBurger;

    private List<string> placedIngredients = new List<string>();

    public bool IsRecipeCorrect()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        string ingredientName = other.gameObject.name;

        if (!placedIngredients.Contains(ingredientName))
        {
            placedIngredients.Add(ingredientName);

            Debug.Log("Ingrediente colocado: " + ingredientName);

            if (IsRecipeCorrect())
            {
                Debug.Log("✅ ¡Hamburguesa correcta!");

                if (finishedBurger != null)
                {
                    finishedBurger.SetActive(true);
                }
            }
            else
            {
                Debug.Log("❌ Hamburguesa incompleta o incorrecta");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        string ingredientName = other.gameObject.name;

        if (placedIngredients.Contains(ingredientName))
        {
            placedIngredients.Remove(ingredientName);

            Debug.Log("Ingrediente retirado: " + ingredientName);
        }
    }
}