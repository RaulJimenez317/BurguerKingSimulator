using System.Text;
using UnityEngine;
using TMPro;

public class OrderManager : MonoBehaviour
{
    [Header("PEDIDOS")]
    public RecipeData[] recipes;
    public RecipeData currentRecipe;

    [Header("REFERENCIAS")]
    public BurgerAssembly burgerAssembly;
    public DeliveryManager deliveryManager;

    [Header("UI")]
    public TMP_Text titleText;
    public TMP_Text orderText;
    public TMP_Text ingredientsText;
    public TMP_Text statusText;
    public TMP_Text timerText;

    [Header("PENALIZACIÓN")]
    public int timeoutPenalty = 50;

    private float remainingTime;
    private bool orderActive = false;
    private int orderNumber = 0;

    public float RemainingTime => remainingTime;
    public bool OrderActive => orderActive;

    private void Start()
    {
        GenerateNewOrder();
    }

    private void Update()
    {
        if (!orderActive || currentRecipe == null)
        {
            return;
        }

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimerText();

            orderActive = false;

            if (statusText != null)
            {
                statusText.text = "TIEMPO AGOTADO";
            }

            Debug.Log("⏰ ¡Se acabó el tiempo!");

            if (deliveryManager != null)
            {
                deliveryManager.ApplyPenalty(timeoutPenalty);
            }
            else
            {
                Debug.LogWarning("No hay DeliveryManager asignado en OrderManager.");
            }

            if (burgerAssembly != null)
            {
                burgerAssembly.ResetAssembly();
            }

            GenerateNewOrder();

            return;
        }

        UpdateTimerText();
    }

    public void GenerateNewOrder()
    {
        if (recipes == null || recipes.Length == 0)
        {
            Debug.LogWarning("No hay recetas configuradas.");
            return;
        }

        int randomIndex = Random.Range(0, recipes.Length);

        currentRecipe = recipes[randomIndex];

        if (currentRecipe == null)
        {
            Debug.LogWarning("La receta seleccionada es nula.");
            return;
        }

        orderNumber++;

        if (burgerAssembly != null)
        {
            burgerAssembly.recipe = currentRecipe;
        }

        UpdateOrderUI();

        remainingTime = currentRecipe.timeLimit;
        orderActive = true;

        UpdateTimerText();

        Debug.Log("Nuevo pedido: " + currentRecipe.recipeName);
        Debug.Log("⏱ Tiempo disponible: " + currentRecipe.timeLimit);
    }

    private void UpdateOrderUI()
    {
        if (titleText != null)
        {
            titleText.text = "PEDIDO #" + orderNumber.ToString("00");
        }

        if (orderText != null)
        {
            orderText.text = GetDisplayRecipeName(currentRecipe.recipeName);
        }

        if (ingredientsText != null)
        {
            ingredientsText.text = BuildIngredientsText();
        }

        if (statusText != null)
        {
            statusText.text = "EN PREPARACIÓN";
        }
    }

    private string BuildIngredientsText()
    {
        if (currentRecipe == null)
        {
            return "";
        }

        StringBuilder builder = new StringBuilder();

        if (currentRecipe.ingredients != null)
        {
            for (int i = 0; i < currentRecipe.ingredients.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(" • ");
                }

                builder.Append(GetDisplayIngredientName(currentRecipe.ingredients[i]));
            }
        }

        if (currentRecipe.includesFries)
        {
            if (builder.Length > 0)
            {
                builder.Append(" • ");
            }

            builder.Append("Papas");
        }

        if (currentRecipe.includesDrink)
        {
            if (builder.Length > 0)
            {
                builder.Append(" • ");
            }

            builder.Append("Refresco");
        }

        return builder.ToString();
    }

    private string GetDisplayRecipeName(string recipeName)
    {
        if (string.IsNullOrWhiteSpace(recipeName))
        {
            return "HAMBURGUESA";
        }

        switch (recipeName)
        {
            case "SimpleBurguer":
            case "SimpleBurger":
                return "HAMBURGUESA SIMPLE";

            case "CheeseBurguer":
            case "CheeseBurger":
                return "HAMBURGUESA CON QUESO";

            case "CompleteBurguer":
            case "CompleteBurger":
                return "HAMBURGUESA COMPLETA";

            default:
                return recipeName.ToUpper();
        }
    }

    private string GetDisplayIngredientName(string ingredient)
    {
        switch (ingredient)
        {
            case "BreadBottom":
                return "Pan inferior";

            case "BreadTop":
                return "Pan superior";

            case "Meat":
                return "Carne";

            case "Cheese":
                return "Queso";

            case "Lettuce":
                return "Lechuga";

            case "Tomato":
                return "Tomate";

            default:
                return ingredient;
        }
    }

    private void UpdateTimerText()
    {
        if (timerText != null)
        {
            timerText.text = "TIEMPO: " + Mathf.CeilToInt(remainingTime);
        }
    }
}