using System.Collections.Generic;
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

    [Header("DIFICULTAD PROGRESIVA")]

    [Tooltip("Cantidad de pedidos correctos necesarios para subir un nivel.")]
    [Min(1)]
    public int correctOrdersPerLevel = 3;

    [Tooltip("Multiplicador de tiempo para dificultad 1.")]
    [Range(0.1f, 1f)]
    public float level1TimeMultiplier = 1f;

    [Tooltip("Multiplicador de tiempo para dificultad 2.")]
    [Range(0.1f, 1f)]
    public float level2TimeMultiplier = 0.85f;

    [Tooltip("Multiplicador de tiempo para dificultad 3.")]
    [Range(0.1f, 1f)]
    public float level3TimeMultiplier = 0.70f;

    [Tooltip("Probabilidad de elegir una receta del nivel de dificultad actual.")]
    [Range(0f, 1f)]
    public float currentDifficultyRecipeChance = 0.70f;

    [Tooltip("Tiempo mínimo que puede tener cualquier pedido.")]
    [Min(1f)]
    public float minimumOrderTime = 30f;


    private float remainingTime;
    private float currentOrderTimeLimit;

    private bool orderActive = false;
    private int orderNumber = 0;


    public float RemainingTime => remainingTime;
    public bool OrderActive => orderActive;
    public int CurrentDifficulty => GetCurrentDifficultyLevel();
    public float CurrentOrderTimeLimit => currentOrderTimeLimit;


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
                deliveryManager.HandleTimeout(timeoutPenalty);
            }
            else
            {
                Debug.LogWarning(
                    "No hay DeliveryManager asignado en OrderManager."
                );
            }

            return;
        }

        UpdateTimerText();
    }


    // =========================================================
    // GENERAR PEDIDO
    // =========================================================

    public void GenerateNewOrder()
    {
        if (recipes == null || recipes.Length == 0)
        {
            Debug.LogWarning("No hay recetas configuradas.");
            return;
        }

        int difficultyLevel = GetCurrentDifficultyLevel();

        RecipeData previousRecipe = currentRecipe;

        currentRecipe = SelectRecipeForDifficulty(
            difficultyLevel,
            previousRecipe
        );

        if (currentRecipe == null)
        {
            Debug.LogWarning(
                "No se pudo encontrar una receta válida."
            );

            return;
        }

        orderNumber++;

        if (burgerAssembly != null)
        {
            burgerAssembly.recipe = currentRecipe;
        }

        // Calculamos el tiempo según la dificultad actual.
        currentOrderTimeLimit =
            CalculateOrderTime(currentRecipe, difficultyLevel);

        remainingTime = currentOrderTimeLimit;

        // IMPORTANTE:
        // El pedido existe, pero el tiempo todavía NO empieza.
        // CustomerController llamará a StartOrderTimer()
        // cuando el cliente llegue al mostrador.
        orderActive = false;

        UpdateOrderUI();
        UpdateTimerText();

        Debug.Log(
            "🍔 Nuevo pedido: " + currentRecipe.recipeName
        );

        Debug.Log(
            "📊 Dificultad actual: " + difficultyLevel
        );

        Debug.Log(
            "⏱ Tiempo original: " +
            currentRecipe.timeLimit +
            " segundos"
        );

        Debug.Log(
            "⏱ Tiempo ajustado: " +
            currentOrderTimeLimit +
            " segundos"
        );
    }


    // =========================================================
    // SELECCIÓN DE RECETA SEGÚN DIFICULTAD
    // =========================================================

    private RecipeData SelectRecipeForDifficulty(
        int difficultyLevel,
        RecipeData previousRecipe)
    {
        List<RecipeData> availableRecipes =
            new List<RecipeData>();

        List<RecipeData> currentLevelRecipes =
            new List<RecipeData>();


        for (int i = 0; i < recipes.Length; i++)
        {
            RecipeData recipe = recipes[i];

            if (recipe == null)
            {
                continue;
            }

            int recipeDifficulty =
                Mathf.Max(1, recipe.difficulty);


            // Se pueden usar recetas del nivel actual
            // o de niveles anteriores.
            if (recipeDifficulty <= difficultyLevel)
            {
                availableRecipes.Add(recipe);
            }


            // Recetas exactamente del nivel actual.
            if (recipeDifficulty == difficultyLevel)
            {
                currentLevelRecipes.Add(recipe);
            }
        }


        if (availableRecipes.Count == 0)
        {
            Debug.LogWarning(
                "No existen recetas disponibles para dificultad " +
                difficultyLevel
            );

            return GetAnyValidRecipe();
        }


        List<RecipeData> selectedPool;


        // Desde dificultad 2 hay mayor probabilidad
        // de recibir pedidos propios de ese nivel.
        if (
            difficultyLevel > 1 &&
            currentLevelRecipes.Count > 0 &&
            Random.value < currentDifficultyRecipeChance
        )
        {
            selectedPool = currentLevelRecipes;
        }
        else
        {
            selectedPool = availableRecipes;
        }


        return GetRandomRecipeAvoidingRepeat(
            selectedPool,
            previousRecipe
        );
    }


    private RecipeData GetRandomRecipeAvoidingRepeat(
        List<RecipeData> recipeList,
        RecipeData previousRecipe)
    {
        if (recipeList == null || recipeList.Count == 0)
        {
            return null;
        }


        // Si solo existe una receta disponible,
        // no podemos evitar repetirla.
        if (recipeList.Count == 1)
        {
            return recipeList[0];
        }


        RecipeData selectedRecipe = null;

        int attempts = 0;


        // Intentamos varias veces evitar que salga
        // exactamente el mismo pedido dos veces seguidas.
        while (
            attempts < 10 &&
            (selectedRecipe == null ||
             selectedRecipe == previousRecipe)
        )
        {
            int randomIndex =
                Random.Range(0, recipeList.Count);

            selectedRecipe =
                recipeList[randomIndex];

            attempts++;
        }


        return selectedRecipe;
    }


    private RecipeData GetAnyValidRecipe()
    {
        if (recipes == null)
        {
            return null;
        }

        for (int i = 0; i < recipes.Length; i++)
        {
            if (recipes[i] != null)
            {
                return recipes[i];
            }
        }

        return null;
    }


    // =========================================================
    // DIFICULTAD
    // =========================================================

    private int GetCurrentDifficultyLevel()
    {
        int correctOrders = 0;


        if (deliveryManager != null)
        {
            correctOrders =
                deliveryManager.correctOrders;
        }


        int difficultyLevel =
            1 + (correctOrders / correctOrdersPerLevel);


        int maximumDifficulty =
            GetMaximumRecipeDifficulty();


        return Mathf.Clamp(
            difficultyLevel,
            1,
            maximumDifficulty
        );
    }


    private int GetMaximumRecipeDifficulty()
    {
        int maximumDifficulty = 1;

        if (recipes == null)
        {
            return maximumDifficulty;
        }


        for (int i = 0; i < recipes.Length; i++)
        {
            if (recipes[i] == null)
            {
                continue;
            }

            int recipeDifficulty =
                Mathf.Max(
                    1,
                    recipes[i].difficulty
                );

            if (recipeDifficulty > maximumDifficulty)
            {
                maximumDifficulty =
                    recipeDifficulty;
            }
        }


        return maximumDifficulty;
    }


    // =========================================================
    // TIEMPO SEGÚN DIFICULTAD
    // =========================================================

    private float CalculateOrderTime(
        RecipeData recipe,
        int difficultyLevel)
    {
        if (recipe == null)
        {
            return minimumOrderTime;
        }


        float multiplier =
            GetTimeMultiplier(difficultyLevel);


        float adjustedTime =
            recipe.timeLimit * multiplier;


        return Mathf.Max(
            minimumOrderTime,
            adjustedTime
        );
    }


    private float GetTimeMultiplier(
        int difficultyLevel)
    {
        switch (difficultyLevel)
        {
            case 1:
                return level1TimeMultiplier;

            case 2:
                return level2TimeMultiplier;

            case 3:
                return level3TimeMultiplier;

            default:
                // Si en el futuro agregas dificultad 4, 5, etc.,
                // seguirá utilizando el multiplicador del nivel 3.
                return level3TimeMultiplier;
        }
    }


    // =========================================================
    // INICIO DEL TEMPORIZADOR
    // =========================================================

    public void StartOrderTimer()
    {
        if (currentRecipe == null)
        {
            return;
        }


        // IMPORTANTE:
        // Ya NO usamos directamente currentRecipe.timeLimit.
        // Usamos el tiempo reducido según dificultad.
        remainingTime = currentOrderTimeLimit;

        orderActive = true;

        UpdateTimerText();


        Debug.Log(
            "⏱️ ¡Comenzó el tiempo del pedido!"
        );

        Debug.Log(
            "📊 Dificultad: " +
            GetCurrentDifficultyLevel()
        );

        Debug.Log(
            "⏱️ Tiempo disponible: " +
            currentOrderTimeLimit +
            " segundos"
        );
    }


    // =========================================================
    // UI DEL PEDIDO
    // =========================================================

    private void UpdateOrderUI()
    {
        if (titleText != null)
        {
            titleText.text =
                "PEDIDO #" +
                orderNumber.ToString("00");
        }


        if (orderText != null)
        {
            orderText.text =
                GetDisplayRecipeName(
                    currentRecipe.recipeName
                );
        }


        if (ingredientsText != null)
        {
            ingredientsText.text =
                BuildIngredientsText();
        }


        if (statusText != null)
        {
            statusText.text =
                "EN PREPARACIÓN";
        }
    }


    private string BuildIngredientsText()
    {
        if (currentRecipe == null)
        {
            return "";
        }


        StringBuilder builder =
            new StringBuilder();


        if (currentRecipe.ingredients != null)
        {
            for (
                int i = 0;
                i < currentRecipe.ingredients.Length;
                i++
            )
            {
                if (i > 0)
                {
                    builder.Append(" • ");
                }


                builder.Append(
                    GetDisplayIngredientName(
                        currentRecipe.ingredients[i]
                    )
                );
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


    // =========================================================
    // NOMBRES MOSTRADOS
    // =========================================================

    private string GetDisplayRecipeName(
        string recipeName)
    {
        if (string.IsNullOrWhiteSpace(recipeName))
        {
            return "HAMBURGUESA";
        }


        string cleanName =
            recipeName.Trim().Trim('\'', '"');


        switch (cleanName)
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
                return cleanName.ToUpper();
        }
    }


    private string GetDisplayIngredientName(
        string ingredient)
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


    // =========================================================
    // UI TEMPORIZADOR
    // =========================================================

    private void UpdateTimerText()
    {
        if (timerText == null)
        {
            return;
        }


        int seconds =
            Mathf.CeilToInt(remainingTime);


        timerText.text =
            "TIEMPO: " + seconds;


        if (seconds <= 10)
        {
            timerText.text =
                "⚠️ TIEMPO: " + seconds;
        }
    }
}