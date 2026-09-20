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

    // Indica que este OrderManager recibió un estado
    // cargado antes de ejecutar Start().
    private bool restoredFromSave = false;


    // =========================================================
    // PROPIEDADES
    // =========================================================

    public float RemainingTime =>
        remainingTime;

    public bool OrderActive =>
        orderActive;

    public int CurrentDifficulty =>
        GetCurrentDifficultyLevel();

    public float CurrentOrderTimeLimit =>
        currentOrderTimeLimit;

    public int OrderNumber =>
        orderNumber;


    // =========================================================
    // INICIO
    // =========================================================

    private void Start()
    {
        // Si DeliveryManager restauró un pedido en Awake(),
        // NO debemos generar otro encima.
        if (restoredFromSave)
        {
            UpdateOrderUI();
            UpdateTimerText();

            Debug.Log(
                "💾 OrderManager inició usando el pedido restaurado."
            );

            return;
        }

        GenerateNewOrder();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!orderActive ||
            currentRecipe == null)
        {
            return;
        }


        remainingTime -=
            Time.deltaTime;


        if (remainingTime <= 0f)
        {
            remainingTime = 0f;

            UpdateTimerText();

            orderActive = false;


            if (statusText != null)
            {
                statusText.text =
                    "TIEMPO AGOTADO";
            }


            Debug.Log(
                "⏰ ¡Se acabó el tiempo!"
            );


            if (deliveryManager != null)
            {
                deliveryManager.HandleTimeout(
                    timeoutPenalty
                );
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
        if (recipes == null ||
            recipes.Length == 0)
        {
            Debug.LogWarning(
                "No hay recetas configuradas."
            );

            return;
        }


        // Ya no estamos trabajando con
        // un pedido restaurado.
        restoredFromSave = false;


        int difficultyLevel =
            GetCurrentDifficultyLevel();


        RecipeData previousRecipe =
            currentRecipe;


        currentRecipe =
            SelectRecipeForDifficulty(
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
            burgerAssembly.recipe =
                currentRecipe;
        }


        currentOrderTimeLimit =
            CalculateOrderTime(
                currentRecipe,
                difficultyLevel
            );


        remainingTime =
            currentOrderTimeLimit;


        // El cliente debe llegar al mostrador
        // antes de iniciar el contador.
        orderActive = false;


        UpdateOrderUI();
        UpdateTimerText();


        Debug.Log(
            "🍔 Nuevo pedido: " +
            currentRecipe.recipeName
        );


        Debug.Log(
            "📦 Pedido #" +
            orderNumber
        );


        Debug.Log(
            "📊 Dificultad actual: " +
            difficultyLevel
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
    // CREAR DATOS DE GUARDADO DEL PEDIDO
    // =========================================================

    public SaveManager.OrderSaveData CreateSaveData()
    {
        SaveManager.OrderSaveData data =
            new SaveManager.OrderSaveData();


        data.hasOrder =
            currentRecipe != null;


        if (currentRecipe != null)
        {
            data.recipeName =
                currentRecipe.recipeName;

            data.recipeIndex =
                GetRecipeIndex(
                    currentRecipe
                );
        }
        else
        {
            data.recipeName =
                "";

            data.recipeIndex =
                -1;
        }


        data.remainingTime =
            Mathf.Max(
                0f,
                remainingTime
            );


        data.currentOrderTimeLimit =
            Mathf.Max(
                0f,
                currentOrderTimeLimit
            );


        data.orderActive =
            orderActive;


        data.orderNumber =
            Mathf.Max(
                0,
                orderNumber
            );


        data.difficulty =
            GetCurrentDifficultyLevel();


        return data;
    }


    // =========================================================
    // RESTAURAR PEDIDO GUARDADO
    // =========================================================

    public bool RestoreFromSaveData(
        SaveManager.OrderSaveData data)
    {
        if (data == null)
        {
            Debug.LogWarning(
                "No existen datos del pedido para restaurar."
            );

            return false;
        }


        if (!data.hasOrder)
        {
            Debug.LogWarning(
                "El guardado no contiene un pedido activo."
            );

            return false;
        }


        RecipeData savedRecipe =
            FindSavedRecipe(
                data
            );


        if (savedRecipe == null)
        {
            Debug.LogWarning(
                "No se pudo encontrar la receta guardada: " +
                data.recipeName
            );

            return false;
        }


        currentRecipe =
            savedRecipe;


        // -----------------------------------------------------
        // RESTAURAR NÚMERO DE PEDIDO
        // -----------------------------------------------------

        orderNumber =
            Mathf.Max(
                1,
                data.orderNumber
            );


        // -----------------------------------------------------
        // RESTAURAR LÍMITE DE TIEMPO
        // -----------------------------------------------------

        if (data.currentOrderTimeLimit > 0f)
        {
            currentOrderTimeLimit =
                data.currentOrderTimeLimit;
        }
        else
        {
            int savedDifficulty =
                Mathf.Max(
                    1,
                    data.difficulty
                );


            currentOrderTimeLimit =
                CalculateOrderTime(
                    currentRecipe,
                    savedDifficulty
                );
        }


        // -----------------------------------------------------
        // RESTAURAR TIEMPO RESTANTE
        // -----------------------------------------------------

        remainingTime =
            Mathf.Clamp(
                data.remainingTime,
                0f,
                currentOrderTimeLimit
            );


        // Si por alguna razón un guardado antiguo
        // no tenía remainingTime correctamente,
        // usamos el tiempo completo.
        if (
            remainingTime <= 0f &&
            !data.orderActive
        )
        {
            remainingTime =
                currentOrderTimeLimit;
        }


        // -----------------------------------------------------
        // RESTAURAR ESTADO DEL CONTADOR
        // -----------------------------------------------------

        orderActive =
            data.orderActive &&
            remainingTime > 0f;


        // -----------------------------------------------------
        // RESTAURAR RECETA DEL ASSEMBLY
        // -----------------------------------------------------

        if (burgerAssembly != null)
        {
            burgerAssembly.recipe =
                currentRecipe;
        }


        restoredFromSave =
            true;


        UpdateOrderUI();
        UpdateTimerText();


        Debug.Log(
            "💾 Pedido restaurado correctamente."
        );


        Debug.Log(
            "🍔 Receta restaurada: " +
            currentRecipe.recipeName
        );


        Debug.Log(
            "📦 Pedido #" +
            orderNumber
        );


        Debug.Log(
            "⏱ Tiempo restante: " +
            remainingTime
        );


        Debug.Log(
            "▶ Temporizador activo: " +
            orderActive
        );


        return true;
    }


    // =========================================================
    // BUSCAR RECETA GUARDADA
    // =========================================================

    private RecipeData FindSavedRecipe(
        SaveManager.OrderSaveData data)
    {
        if (recipes == null ||
            recipes.Length == 0)
        {
            return null;
        }


        // Primero buscamos por índice.
        // Es la forma más directa si el array
        // de recetas no fue modificado.
        if (
            data.recipeIndex >= 0 &&
            data.recipeIndex < recipes.Length
        )
        {
            RecipeData indexedRecipe =
                recipes[data.recipeIndex];


            if (indexedRecipe != null)
            {
                // Además comprobamos el nombre cuando existe
                // para evitar cargar una receta equivocada
                // si el array cambió de orden.
                if (
                    string.IsNullOrEmpty(
                        data.recipeName
                    ) ||
                    indexedRecipe.recipeName ==
                    data.recipeName
                )
                {
                    return indexedRecipe;
                }
            }
        }


        // Si el índice cambió, buscamos por nombre.
        if (!string.IsNullOrEmpty(
            data.recipeName))
        {
            for (
                int i = 0;
                i < recipes.Length;
                i++
            )
            {
                RecipeData recipe =
                    recipes[i];


                if (recipe == null)
                {
                    continue;
                }


                if (
                    recipe.recipeName ==
                    data.recipeName
                )
                {
                    return recipe;
                }
            }
        }


        return null;
    }


    // =========================================================
    // OBTENER ÍNDICE DE RECETA
    // =========================================================

    private int GetRecipeIndex(
        RecipeData recipe)
    {
        if (recipe == null ||
            recipes == null)
        {
            return -1;
        }


        for (
            int i = 0;
            i < recipes.Length;
            i++
        )
        {
            if (recipes[i] == recipe)
            {
                return i;
            }
        }


        return -1;
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


        for (
            int i = 0;
            i < recipes.Length;
            i++
        )
        {
            RecipeData recipe =
                recipes[i];


            if (recipe == null)
            {
                continue;
            }


            int recipeDifficulty =
                Mathf.Max(
                    1,
                    recipe.difficulty
                );


            if (recipeDifficulty <=
                difficultyLevel)
            {
                availableRecipes.Add(
                    recipe
                );
            }


            if (recipeDifficulty ==
                difficultyLevel)
            {
                currentLevelRecipes.Add(
                    recipe
                );
            }
        }


        if (availableRecipes.Count == 0)
        {
            Debug.LogWarning(
                "No existen recetas disponibles para dificultad " +
                difficultyLevel
            );


            return
                GetAnyValidRecipe();
        }


        List<RecipeData> selectedPool;


        if (
            difficultyLevel > 1 &&
            currentLevelRecipes.Count > 0 &&
            Random.value <
            currentDifficultyRecipeChance
        )
        {
            selectedPool =
                currentLevelRecipes;
        }
        else
        {
            selectedPool =
                availableRecipes;
        }


        return
            GetRandomRecipeAvoidingRepeat(
                selectedPool,
                previousRecipe
            );
    }


    private RecipeData GetRandomRecipeAvoidingRepeat(
        List<RecipeData> recipeList,
        RecipeData previousRecipe)
    {
        if (
            recipeList == null ||
            recipeList.Count == 0
        )
        {
            return null;
        }


        if (recipeList.Count == 1)
        {
            return recipeList[0];
        }


        RecipeData selectedRecipe =
            null;


        int attempts = 0;


        while (
            attempts < 10 &&
            (
                selectedRecipe == null ||
                selectedRecipe ==
                previousRecipe
            )
        )
        {
            int randomIndex =
                Random.Range(
                    0,
                    recipeList.Count
                );


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


        for (
            int i = 0;
            i < recipes.Length;
            i++
        )
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
                Mathf.Max(
                    0,
                    deliveryManager.correctOrders
                );
        }


        // Protección adicional para evitar una
        // división entre cero.
        int safeOrdersPerLevel =
            Mathf.Max(
                1,
                correctOrdersPerLevel
            );


        int difficultyLevel =
            1 +
            (
                correctOrders /
                safeOrdersPerLevel
            );


        int maximumDifficulty =
            GetMaximumRecipeDifficulty();


        return
            Mathf.Clamp(
                difficultyLevel,
                1,
                maximumDifficulty
            );
    }


    private int GetMaximumRecipeDifficulty()
    {
        int maximumDifficulty =
            1;


        if (recipes == null)
        {
            return maximumDifficulty;
        }


        for (
            int i = 0;
            i < recipes.Length;
            i++
        )
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


            if (recipeDifficulty >
                maximumDifficulty)
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
            return
                Mathf.Max(
                    1f,
                    minimumOrderTime
                );
        }


        float multiplier =
            GetTimeMultiplier(
                difficultyLevel
            );


        float adjustedTime =
            recipe.timeLimit *
            multiplier;


        return
            Mathf.Max(
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

                return
                    level1TimeMultiplier;


            case 2:

                return
                    level2TimeMultiplier;


            case 3:

                return
                    level3TimeMultiplier;


            default:

                return
                    level3TimeMultiplier;
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


        // Evitamos reiniciar accidentalmente
        // un contador que ya está funcionando.
        if (orderActive)
        {
            return;
        }


        // Si este pedido vino de un guardado,
        // conservamos exactamente su tiempo restante.
        if (restoredFromSave)
        {
            if (remainingTime <= 0f)
            {
                remainingTime =
                    currentOrderTimeLimit;
            }


            restoredFromSave =
                false;
        }
        else
        {
            remainingTime =
                currentOrderTimeLimit;
        }


        orderActive =
            true;


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
            remainingTime +
            " segundos"
        );
    }


    // =========================================================
    // DETENER TEMPORIZADOR
    // =========================================================

    public void StopOrderTimer()
    {
        orderActive = false;

        UpdateTimerText();
    }


    // =========================================================
    // UI DEL PEDIDO
    // =========================================================

    private void UpdateOrderUI()
    {
        if (currentRecipe == null)
        {
            return;
        }


        if (titleText != null)
        {
            titleText.text =
                "PEDIDO #" +
                orderNumber.ToString(
                    "00"
                );
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
                i <
                currentRecipe.ingredients.Length;
                i++
            )
            {
                if (i > 0)
                {
                    builder.Append(
                        " • "
                    );
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
                builder.Append(
                    " • "
                );
            }


            builder.Append(
                "Papas"
            );
        }


        if (currentRecipe.includesDrink)
        {
            if (builder.Length > 0)
            {
                builder.Append(
                    " • "
                );
            }


            builder.Append(
                "Refresco"
            );
        }


        return
            builder.ToString();
    }


    // =========================================================
    // NOMBRES MOSTRADOS
    // =========================================================

    private string GetDisplayRecipeName(
        string recipeName)
    {
        if (string.IsNullOrWhiteSpace(
            recipeName))
        {
            return
                "HAMBURGUESA";
        }


        string cleanName =
            recipeName
                .Trim()
                .Trim(
                    '\'',
                    '"'
                );


        switch (cleanName)
        {
            case "SimpleBurguer":
            case "SimpleBurger":

                return
                    "HAMBURGUESA SIMPLE";


            case "CheeseBurguer":
            case "CheeseBurger":

                return
                    "HAMBURGUESA CON QUESO";


            case "CompleteBurguer":
            case "CompleteBurger":

                return
                    "HAMBURGUESA COMPLETA";


            default:

                return
                    cleanName.ToUpper();
        }
    }


    private string GetDisplayIngredientName(
        string ingredient)
    {
        switch (ingredient)
        {
            case "BreadBottom":

                return
                    "Pan inferior";


            case "BreadTop":

                return
                    "Pan superior";


            case "Meat":

                return
                    "Carne";


            case "Cheese":

                return
                    "Queso";


            case "Lettuce":

                return
                    "Lechuga";


            case "Tomato":

                return
                    "Tomate";


            default:

                return
                    ingredient;
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
            Mathf.CeilToInt(
                Mathf.Max(
                    0f,
                    remainingTime
                )
            );


        timerText.text =
            "TIEMPO: " +
            seconds;


        if (seconds <= 10)
        {
            timerText.text =
                "⚠️ TIEMPO: " +
                seconds;
        }
    }
}