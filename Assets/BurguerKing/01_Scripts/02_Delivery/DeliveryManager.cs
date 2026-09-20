using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeliveryManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public BurgerAssembly assembly;
    public OrderManager orderManager;
    public DeliveryZone deliveryZone;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text statusText;

    [Header("FEEDBACK UI")]
    public GameObject feedbackPanel;
    public TMP_Text feedbackText;

    [Header("GAME OVER UI")]
    public GameObject gameOverPanel;
    public TMP_Text gameOverText;

    [Header("PUNTUACIÓN")]
    public int score = 20;
    public int incorrectOrderPenalty = 50;

    private int highestScore = 0;

    public int HighestScore
    {
        get
        {
            return highestScore;
        }
    }

    public bool IsProcessingDelivery
    {
        get
        {
            return processingDelivery;
        }
    }

    [Header("PENALIZACIÓN POR DIFICULTAD")]
    [Tooltip("Penalización en dificultad 1.")]
    [Min(0)]
    public int penaltyDifficulty1 = 50;

    [Tooltip("Penalización en dificultad 2.")]
    [Min(0)]
    public int penaltyDifficulty2 = 120;

    [Tooltip("Penalización en dificultad 3.")]
    [Min(0)]
    public int penaltyDifficulty3 = 180;

    [Header("ESTADISTICAS")]
    public int ordersCompleted = 0;
    public int correctOrders = 0;
    public int incorrectOrders = 0;

    [Header("FEEDBACK")]
    public float feedbackDuration = 3.5f;

    [Header("CLIENTE")]
    public CustomerSpawner customerSpawner;

    private bool processingDelivery = false;
    private bool orderManagerWasEnabled = true;
    private Coroutine finishCoroutine;
    private bool gameResultSaved = false;

    private enum FinishType
    {
        Correct,
        Incorrect,
        Timeout
    }

    private void Awake()
    {
        highestScore =
            Mathf.Max(
                0,
                score
            );

        LoadSavedGameIfRequested();
    }

    private void Start()
    {
        UpdateScoreText();
    }

    private void LoadSavedGameIfRequested()
    {
        if (!SaveManager.ConsumeLoadRequest())
        {
            return;
        }

        SaveManager.GameSaveData savedGame =
            SaveManager.LoadCurrentGame();

        if (savedGame == null)
        {
            Debug.LogWarning(
                "No se pudo cargar la partida guardada."
            );

            return;
        }

        score =
            Mathf.Max(
                0,
                savedGame.score
            );

        highestScore =
            Mathf.Max(
                savedGame.highestScore,
                score
            );

        ordersCompleted =
            Mathf.Max(
                0,
                savedGame.ordersCompleted
            );

        correctOrders =
            Mathf.Max(
                0,
                savedGame.correctOrders
            );

        incorrectOrders =
            Mathf.Max(
                0,
                savedGame.incorrectOrders
            );

        processingDelivery = false;
        gameResultSaved = false;

        bool orderRestored = false;
        bool assemblyRestored = false;
        bool customerRestored = false;

        if (orderManager != null &&
            savedGame.order != null &&
            savedGame.order.hasOrder)
        {
            orderRestored =
                orderManager.RestoreFromSaveData(
                    savedGame.order
                );
        }

        if (orderRestored &&
            assembly != null &&
            savedGame.assembly != null &&
            savedGame.assembly.hasAssemblyData)
        {
            assemblyRestored =
                assembly.RestoreFromSaveData(
                    savedGame.assembly
                );
        }

        if (orderRestored &&
            customerSpawner != null &&
            savedGame.customer != null &&
            savedGame.customer.exists)
        {
            customerRestored =
                customerSpawner.RestoreFromSaveData(
                    savedGame.customer
                );
        }

        RestoreWorldItems(
            savedGame
        );

        Debug.Log(
            "💾 Partida cargada correctamente."
        );

        Debug.Log(
            "🏆 Puntuación actual: " +
            score
        );

        Debug.Log(
            "⭐ Puntuación máxima: " +
            highestScore
        );

        Debug.Log(
            "📦 Pedidos atendidos: " +
            ordersCompleted
        );

        Debug.Log(
            "✅ Pedidos correctos: " +
            correctOrders
        );

        Debug.Log(
            "❌ Pedidos incorrectos: " +
            incorrectOrders
        );

        if (savedGame.order != null &&
            savedGame.order.hasOrder)
        {
            if (orderRestored)
            {
                Debug.Log(
                    "🍔 Pedido guardado restaurado correctamente."
                );
            }
            else
            {
                Debug.LogWarning(
                    "No se pudo restaurar el pedido guardado."
                );
            }
        }

        if (savedGame.assembly != null &&
            savedGame.assembly.hasAssemblyData)
        {
            if (assemblyRestored)
            {
                Debug.Log(
                    "🥬 Hamburguesa restaurada correctamente."
                );
            }
            else
            {
                Debug.LogWarning(
                    "No se pudo restaurar el estado de la hamburguesa."
                );
            }
        }

        if (savedGame.customer != null &&
            savedGame.customer.exists)
        {
            if (customerRestored)
            {
                Debug.Log(
                    "👤 Cliente restaurado correctamente."
                );
            }
            else
            {
                Debug.LogWarning(
                    "No se pudo restaurar el cliente guardado."
                );
            }
        }
    }

    public SaveManager.GameSaveData CreateSaveData()
    {
        SaveManager.GameSaveData gameData =
            new SaveManager.GameSaveData(
                Mathf.Max(
                    0,
                    score
                ),
                Mathf.Max(
                    highestScore,
                    score
                ),
                Mathf.Max(
                    0,
                    ordersCompleted
                ),
                Mathf.Max(
                    0,
                    correctOrders
                ),
                Mathf.Max(
                    0,
                    incorrectOrders
                )
            );

        if (processingDelivery)
        {
            gameData.hasFullGameState =
                false;

            return gameData;
        }

        bool orderSaved = false;
        bool assemblySaved = false;
        bool customerSaved = false;

        if (orderManager != null)
        {
            SaveManager.OrderSaveData orderData =
                orderManager.CreateSaveData();

            if (orderData != null)
            {
                gameData.order =
                    orderData;

                orderSaved =
                    orderData.hasOrder;
            }
        }

        if (assembly != null)
        {
            SaveManager.AssemblySaveData assemblyData =
                assembly.CreateSaveData();

            if (assemblyData != null)
            {
                gameData.assembly =
                    assemblyData;

                assemblySaved =
                    assemblyData.hasAssemblyData;
            }
        }

        if (customerSpawner != null)
        {
            SaveManager.CustomerSaveData customerData =
                customerSpawner.CreateSaveData();

            if (customerData != null)
            {
                gameData.customer =
                    customerData;

                customerSaved =
                    customerData.exists;
            }
        }

        SaveWorldItems(
            gameData
        );

        gameData.hasFullGameState =
            orderSaved &&
            assemblySaved &&
            customerSaved;

        return gameData;
    }

    public bool SaveCurrentGame()
    {
        if (processingDelivery)
        {
            Debug.LogWarning(
                "No se puede guardar mientras se procesa un pedido."
            );

            return false;
        }

        if (score <= 0)
        {
            Debug.LogWarning(
                "No se puede guardar una partida terminada."
            );

            return false;
        }

        SaveManager.GameSaveData gameData =
            CreateSaveData();

        if (gameData == null)
        {
            Debug.LogWarning(
                "No se pudieron crear los datos de guardado."
            );

            return false;
        }

        SaveManager.SaveCurrentGame(
            gameData
        );

        return true;
    }

    private void SaveWorldItems(
        SaveManager.GameSaveData gameData)
    {
        if (gameData == null)
        {
            return;
        }

        if (gameData.worldItems == null)
        {
            gameData.worldItems =
                new List<SaveManager.WorldItemSaveData>();
        }
        else
        {
            gameData.worldItems.Clear();
        }

        DrinkCup[] drinks =
            Object.FindObjectsByType<DrinkCup>(
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < drinks.Length;
             i++)
        {
            DrinkCup drink =
                drinks[i];

            if (drink == null ||
                !drink.gameObject.scene.IsValid())
            {
                continue;
            }

            SaveManager.WorldItemSaveData itemData =
                drink.CreateSaveData();

            if (itemData != null)
            {
                gameData.worldItems.Add(
                    itemData
                );
            }
        }

        FriesBag[] fries =
            Object.FindObjectsByType<FriesBag>(
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < fries.Length;
             i++)
        {
            FriesBag friesBag =
                fries[i];

            if (friesBag == null ||
                !friesBag.gameObject.scene.IsValid())
            {
                continue;
            }

            SaveManager.WorldItemSaveData itemData =
                friesBag.CreateSaveData();

            if (itemData != null)
            {
                gameData.worldItems.Add(
                    itemData
                );
            }
        }

        IngredientDispenser[] dispensers =
            Object.FindObjectsByType<IngredientDispenser>(
                FindObjectsSortMode.None
            );

        HashSet<GameObject> savedIngredients =
            new HashSet<GameObject>();

        for (int i = 0;
             i < dispensers.Length;
             i++)
        {
            IngredientDispenser dispenser =
                dispensers[i];

            if (dispenser == null)
            {
                continue;
            }

            List<GameObject> ingredients =
                dispenser.GetSpawnedIngredients();

            for (int j = 0;
                 j < ingredients.Count;
                 j++)
            {
                GameObject ingredient =
                    ingredients[j];

                if (ingredient == null ||
                    !ingredient.scene.IsValid())
                {
                    continue;
                }

                if (savedIngredients.Contains(
                    ingredient))
                {
                    continue;
                }

                if (assembly != null &&
                    assembly.ContainsPlacedIngredient(
                        ingredient))
                {
                    continue;
                }

                SaveManager.WorldItemSaveData itemData =
                    CreateIngredientSaveData(
                        ingredient,
                        dispenser
                    );

                if (itemData == null)
                {
                    continue;
                }

                gameData.worldItems.Add(
                    itemData
                );

                savedIngredients.Add(
                    ingredient
                );
            }
        }
    }

    private SaveManager.WorldItemSaveData
        CreateIngredientSaveData(
            GameObject ingredient,
            IngredientDispenser dispenser)
    {
        if (ingredient == null)
        {
            return null;
        }

        SaveManager.WorldItemSaveData data =
            new SaveManager.WorldItemSaveData();

        data.itemType =
            "Ingredient";

        data.objectName =
            GetCleanObjectName(
                ingredient.name
            );

        if (dispenser != null &&
            dispenser.IngredientPrefab != null)
        {
            data.prefabName =
                GetCleanObjectName(
                    dispenser.IngredientPrefab.name
                );
        }
        else
        {
            data.prefabName =
                data.objectName;
        }

        data.active =
            ingredient.activeSelf;

        data.position =
            new SaveManager.Vector3Data(
                ingredient.transform.position
            );

        data.rotation =
            new SaveManager.QuaternionData(
                ingredient.transform.rotation
            );

        data.scale =
            new SaveManager.Vector3Data(
                ingredient.transform.localScale
            );

        MeatCooking meatCooking =
            ingredient.GetComponent<MeatCooking>();

        if (meatCooking == null)
        {
            meatCooking =
                ingredient.GetComponentInChildren
                    <MeatCooking>();
        }

        if (meatCooking != null)
        {
            data.hasMeatCooking =
                true;

            data.cookingState =
                (int)meatCooking.CurrentState;

            data.cookingProgress =
                meatCooking.CookingProgress;

            data.onGrill =
                meatCooking.IsOnGrill;
        }

        return data;
    }

    private void RestoreWorldItems(
        SaveManager.GameSaveData gameData)
    {
        if (gameData == null ||
            gameData.worldItems == null)
        {
            return;
        }

        if (deliveryZone != null)
        {
            deliveryZone.ClearDeliveryZone();
        }

        ClearExistingDrinks();
        ClearExistingFries();
        ClearExistingLooseIngredients();

        DrinkDispenser drinkDispenser =
            Object.FindFirstObjectByType
                <DrinkDispenser>();

        FriesDispenser friesDispenser =
            Object.FindFirstObjectByType
                <FriesDispenser>();

        for (int i = 0;
             i < gameData.worldItems.Count;
             i++)
        {
            SaveManager.WorldItemSaveData itemData =
                gameData.worldItems[i];

            if (itemData == null)
            {
                continue;
            }

            if (itemData.itemType == "Drink")
            {
                RestoreDrink(
                    itemData,
                    drinkDispenser
                );
            }
            else if (itemData.itemType == "Fries")
            {
                RestoreFries(
                    itemData,
                    friesDispenser
                );
            }
            else if (itemData.itemType == "Ingredient")
            {
                RestoreIngredient(
                    itemData
                );
            }
        }
    }

    private void ClearExistingDrinks()
    {
        DrinkCup[] existingDrinks =
            Object.FindObjectsByType<DrinkCup>(
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < existingDrinks.Length;
             i++)
        {
            if (existingDrinks[i] == null)
            {
                continue;
            }

            existingDrinks[i]
                .gameObject
                .SetActive(false);

            Destroy(
                existingDrinks[i].gameObject
            );
        }
    }

    private void ClearExistingFries()
    {
        FriesBag[] existingFries =
            Object.FindObjectsByType<FriesBag>(
                FindObjectsSortMode.None
            );

        for (int i = 0;
             i < existingFries.Length;
             i++)
        {
            if (existingFries[i] == null)
            {
                continue;
            }

            existingFries[i]
                .gameObject
                .SetActive(false);

            Destroy(
                existingFries[i].gameObject
            );
        }
    }

    private void ClearExistingLooseIngredients()
    {
        IngredientDispenser[] dispensers =
            Object.FindObjectsByType<IngredientDispenser>(
                FindObjectsSortMode.None
            );

        HashSet<GameObject> processed =
            new HashSet<GameObject>();

        for (int i = 0;
             i < dispensers.Length;
             i++)
        {
            IngredientDispenser dispenser =
                dispensers[i];

            if (dispenser == null)
            {
                continue;
            }

            List<GameObject> ingredients =
                dispenser.GetSpawnedIngredients();

            for (int j = 0;
                 j < ingredients.Count;
                 j++)
            {
                GameObject ingredient =
                    ingredients[j];

                if (ingredient == null ||
                    processed.Contains(
                        ingredient))
                {
                    continue;
                }

                processed.Add(
                    ingredient
                );

                if (assembly != null &&
                    assembly.ContainsPlacedIngredient(
                        ingredient))
                {
                    continue;
                }

                dispenser.UnregisterIngredient(
                    ingredient
                );

                ingredient.SetActive(
                    false
                );

                Destroy(
                    ingredient
                );
            }
        }
    }

    private void RestoreDrink(
        SaveManager.WorldItemSaveData itemData,
        DrinkDispenser dispenser)
    {
        if (itemData == null)
        {
            return;
        }

        if (dispenser == null ||
            dispenser.drinkCupPrefab == null)
        {
            Debug.LogWarning(
                "No se pudo restaurar un refresco porque no se encontró su prefab."
            );

            return;
        }

        Vector3 position =
            dispenser.transform.position;

        Quaternion rotation =
            dispenser.transform.rotation;

        if (itemData.position != null)
        {
            position =
                itemData.position.ToVector3();
        }

        if (itemData.rotation != null)
        {
            rotation =
                itemData.rotation.ToQuaternion();
        }

        GameObject newDrink =
            Instantiate(
                dispenser.drinkCupPrefab,
                position,
                rotation
            );

        if (!string.IsNullOrEmpty(
            itemData.objectName))
        {
            newDrink.name =
                itemData.objectName;
        }

        DrinkCup drinkCup =
            newDrink.GetComponent<DrinkCup>();

        if (drinkCup == null)
        {
            drinkCup =
                newDrink.GetComponentInChildren
                    <DrinkCup>();
        }

        if (drinkCup == null)
        {
            Debug.LogWarning(
                "El prefab del refresco no tiene DrinkCup."
            );

            Destroy(
                newDrink
            );

            return;
        }

        drinkCup.RestoreFromSaveData(
            itemData
        );
    }

    private void RestoreFries(
        SaveManager.WorldItemSaveData itemData,
        FriesDispenser dispenser)
    {
        if (itemData == null)
        {
            return;
        }

        if (dispenser == null ||
            dispenser.friesBagPrefab == null)
        {
            Debug.LogWarning(
                "No se pudieron restaurar las papas porque no se encontró su prefab."
            );

            return;
        }

        Vector3 position =
            dispenser.transform.position;

        Quaternion rotation =
            dispenser.transform.rotation;

        if (itemData.position != null)
        {
            position =
                itemData.position.ToVector3();
        }

        if (itemData.rotation != null)
        {
            rotation =
                itemData.rotation.ToQuaternion();
        }

        GameObject newFries =
            Instantiate(
                dispenser.friesBagPrefab,
                position,
                rotation
            );

        if (!string.IsNullOrEmpty(
            itemData.objectName))
        {
            newFries.name =
                itemData.objectName;
        }

        FriesBag friesBag =
            newFries.GetComponent<FriesBag>();

        if (friesBag == null)
        {
            friesBag =
                newFries.GetComponentInChildren
                    <FriesBag>();
        }

        if (friesBag == null)
        {
            Debug.LogWarning(
                "El prefab de papas no tiene FriesBag."
            );

            Destroy(
                newFries
            );

            return;
        }

        friesBag.RestoreFromSaveData(
            itemData
        );
    }

    private void RestoreIngredient(
        SaveManager.WorldItemSaveData itemData)
    {
        if (itemData == null)
        {
            return;
        }

        IngredientDispenser dispenser =
            FindIngredientDispenser(
                itemData
            );

        if (dispenser == null ||
            dispenser.IngredientPrefab == null)
        {
            Debug.LogWarning(
                "No se pudo restaurar el ingrediente: " +
                itemData.objectName
            );

            return;
        }

        Vector3 position =
            dispenser.transform.position;

        Quaternion rotation =
            dispenser.transform.rotation;

        if (itemData.position != null)
        {
            position =
                itemData.position.ToVector3();
        }

        if (itemData.rotation != null)
        {
            rotation =
                itemData.rotation.ToQuaternion();
        }

        GameObject restoredIngredient =
            dispenser.SpawnRestoredIngredient(
                position,
                rotation
            );

        if (restoredIngredient == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(
            itemData.objectName))
        {
            restoredIngredient.name =
                GetCleanObjectName(
                    itemData.objectName
                );
        }

        if (itemData.scale != null)
        {
            restoredIngredient
                .transform
                .localScale =
                    itemData.scale.ToVector3();
        }

        Rigidbody rb =
            restoredIngredient.GetComponent
                <Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        if (itemData.hasMeatCooking)
        {
            MeatCooking meatCooking =
                restoredIngredient.GetComponent
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
                SaveManager.MeatSaveData meatData =
                    new SaveManager.MeatSaveData();

                meatData.hasMeatCooking =
                    true;

                meatData.cookingState =
                    itemData.cookingState;

                meatData.cookingProgress =
                    itemData.cookingProgress;

                meatData.onGrill =
                    itemData.onGrill;

                meatCooking.RestoreFromSaveData(
                    meatData
                );
            }
        }

        restoredIngredient.SetActive(
            itemData.active
        );
    }

    private IngredientDispenser
        FindIngredientDispenser(
            SaveManager.WorldItemSaveData itemData)
    {
        IngredientDispenser[] dispensers =
            Object.FindObjectsByType<IngredientDispenser>(
                FindObjectsSortMode.None
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
                    itemData.prefabName) &&
                dispenser.MatchesIngredient(
                    itemData.prefabName))
            {
                return dispenser;
            }

            if (!string.IsNullOrEmpty(
                    itemData.objectName) &&
                dispenser.MatchesIngredient(
                    itemData.objectName))
            {
                return dispenser;
            }
        }

        return null;
    }

    private string GetCleanObjectName(
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

    public void DeliverOrder()
    {
        if (processingDelivery)
        {
            return;
        }

        if (assembly == null)
        {
            Debug.LogWarning(
                "No hay BurgerAssembly asignado en DeliveryManager."
            );

            return;
        }

        if (deliveryZone == null)
        {
            Debug.LogWarning(
                "No hay DeliveryZone asignado en DeliveryManager."
            );

            return;
        }

        if (assembly.recipe == null)
        {
            Debug.LogWarning(
                "No hay una receta activa."
            );

            return;
        }

        bool burgerCorrect =
            deliveryZone.HasBurger &&
            assembly.IsRecipeCorrect();

        bool meatCorrect =
            deliveryZone.HasBurger &&
            assembly.IsMeatCookingCorrect();

        bool friesCorrect =
            ValidateFries();

        bool drinkCorrect =
            ValidateDrink();

        bool orderCorrect =
            burgerCorrect &&
            meatCorrect &&
            friesCorrect &&
            drinkCorrect;

        if (orderCorrect)
        {
            DeliverCorrectOrder();
        }
        else
        {
            DeliverIncorrectOrder(
                burgerCorrect,
                meatCorrect,
                friesCorrect,
                drinkCorrect
            );
        }
    }

    private bool ValidateFries()
    {
        if (assembly == null ||
            assembly.recipe == null ||
            deliveryZone == null)
        {
            return false;
        }

        bool friesRequired =
            assembly.recipe.includesFries;

        if (friesRequired)
        {
            if (!deliveryZone.HasFries)
            {
                return false;
            }

            if (deliveryZone.CurrentFries == null)
            {
                return false;
            }

            FriesBag friesBag =
                deliveryZone.CurrentFries
                    .GetComponent<FriesBag>();

            if (friesBag == null)
            {
                return false;
            }

            return friesBag.isFilled;
        }

        return !deliveryZone.HasFries;
    }

    private bool ValidateDrink()
    {
        if (assembly == null ||
            assembly.recipe == null ||
            deliveryZone == null)
        {
            return false;
        }

        bool drinkRequired =
            assembly.recipe.includesDrink;

        if (drinkRequired)
        {
            if (!deliveryZone.HasDrink)
            {
                return false;
            }

            if (deliveryZone.CurrentDrink == null)
            {
                return false;
            }

            DrinkCup drinkCup =
                deliveryZone.CurrentDrink
                    .GetComponent<DrinkCup>();

            if (drinkCup == null)
            {
                return false;
            }

            return drinkCup.isFilled;
        }

        return !deliveryZone.HasDrink;
    }

    private void DeliverCorrectOrder()
    {
        int earnedPoints = 0;

        if (assembly != null &&
            assembly.recipe != null)
        {
            earnedPoints =
                Mathf.Max(
                    0,
                    assembly.recipe.points
                );
        }

        score += earnedPoints;

        ordersCompleted++;
        correctOrders++;

        if (score > highestScore)
        {
            highestScore =
                score;
        }

        UpdateScoreText();

        Debug.Log(
            "✅ Pedido entregado correctamente."
        );

        Debug.Log(
            "🥩 Carne correctamente cocinada."
        );

        Debug.Log(
            "⭐ +" +
            earnedPoints +
            " puntos"
        );

        Debug.Log(
            "🏆 Puntuación total: " +
            score
        );

        BeginOrderFinish(
            "PEDIDO CORRECTO\n\n+" +
            earnedPoints +
            " PUNTOS",
            FinishType.Correct,
            0
        );
    }

    private void DeliverIncorrectOrder(
        bool burgerCorrect,
        bool meatCorrect,
        bool friesCorrect,
        bool drinkCorrect)
    {
        Debug.Log(
            "❌ Pedido entregado incorrectamente."
        );

        string feedbackMessage =
            "PEDIDO INCORRECTO\n\n";

        if (!burgerCorrect)
        {
            if (deliveryZone == null ||
                !deliveryZone.HasBurger)
            {
                Debug.Log(
                    "Falta la hamburguesa."
                );

                feedbackMessage +=
                    "- Falta la hamburguesa.\n";
            }
            else
            {
                Debug.Log(
                    "Hamburguesa incorrecta."
                );

                feedbackMessage +=
                    "- Hamburguesa incorrecta.\n";
            }
        }

        if (!meatCorrect &&
            deliveryZone != null &&
            deliveryZone.HasBurger &&
            assembly != null &&
            assembly.HasMeat)
        {
            switch (assembly.MeatState)
            {
                case MeatCooking.CookingState.Raw:

                    feedbackMessage +=
                        "- La carne está cruda.\n";

                    break;

                case MeatCooking.CookingState.Cooking:

                    feedbackMessage +=
                        "- La carne está poco cocinada.\n";

                    break;

                case MeatCooking.CookingState.Burned:

                    feedbackMessage +=
                        "- La carne está quemada.\n";

                    break;

                case MeatCooking.CookingState.Ready:

                    break;
            }
        }

        if (!friesCorrect &&
            assembly != null &&
            assembly.recipe != null)
        {
            if (assembly.recipe.includesFries)
            {
                if (deliveryZone == null ||
                    !deliveryZone.HasFries)
                {
                    feedbackMessage +=
                        "- Faltan las papas.\n";
                }
                else
                {
                    feedbackMessage +=
                        "- Las papas no están preparadas correctamente.\n";
                }
            }
            else
            {
                feedbackMessage +=
                    "- El pedido no incluye papas.\n";
            }
        }

        if (!drinkCorrect &&
            assembly != null &&
            assembly.recipe != null)
        {
            if (assembly.recipe.includesDrink)
            {
                if (deliveryZone == null ||
                    !deliveryZone.HasDrink)
                {
                    feedbackMessage +=
                        "- Falta el refresco.\n";
                }
                else
                {
                    feedbackMessage +=
                        "- El refresco no está preparado correctamente.\n";
                }
            }
            else
            {
                feedbackMessage +=
                    "- El pedido no incluye refresco.\n";
            }
        }

        ordersCompleted++;
        incorrectOrders++;

        int penalty =
            GetPenaltyForCurrentDifficulty();

        ApplyPenalty(
            penalty
        );

        feedbackMessage +=
            "\n-" +
            penalty +
            " PUNTOS";

        BeginOrderFinish(
            feedbackMessage,
            FinishType.Incorrect,
            penalty
        );
    }

    private int GetPenaltyForCurrentDifficulty()
    {
        if (orderManager == null)
        {
            return Mathf.Max(
                0,
                incorrectOrderPenalty
            );
        }

        int difficulty =
            orderManager.CurrentDifficulty;

        switch (difficulty)
        {
            case 1:

                return Mathf.Max(
                    0,
                    penaltyDifficulty1
                );

            case 2:

                return Mathf.Max(
                    0,
                    penaltyDifficulty2
                );

            case 3:

                return Mathf.Max(
                    0,
                    penaltyDifficulty3
                );

            default:

                return Mathf.Max(
                    0,
                    penaltyDifficulty3
                );
        }
    }

    private int GetCurrentDifficulty()
    {
        if (orderManager == null)
        {
            return 1;
        }

        return orderManager.CurrentDifficulty;
    }

    private void BeginOrderFinish(
        string feedbackMessage,
        FinishType finishType,
        int appliedPenalty)
    {
        if (processingDelivery)
        {
            return;
        }

        processingDelivery = true;

        ShowFeedback(
            feedbackMessage
        );

        if (statusText != null)
        {
            switch (finishType)
            {
                case FinishType.Correct:

                    int earnedPoints = 0;

                    if (assembly != null &&
                        assembly.recipe != null)
                    {
                        earnedPoints =
                            Mathf.Max(
                                0,
                                assembly.recipe.points
                            );
                    }

                    statusText.text =
                        "CORRECTO +" +
                        earnedPoints;

                    break;

                case FinishType.Incorrect:

                    statusText.text =
                        "INCORRECTO -" +
                        appliedPenalty;

                    break;

                case FinishType.Timeout:

                    statusText.text =
                        "TIEMPO AGOTADO -" +
                        appliedPenalty;

                    break;
            }
        }

        if (orderManager != null)
        {
            orderManagerWasEnabled =
                orderManager.enabled;

            orderManager.enabled =
                false;
        }

        if (finishCoroutine != null)
        {
            StopCoroutine(
                finishCoroutine
            );
        }

        finishCoroutine =
            StartCoroutine(
                FinishOrderAfterDelay()
            );
    }

    public void HandleTimeout(
        int penalty)
    {
        if (processingDelivery)
        {
            return;
        }

        ordersCompleted++;
        incorrectOrders++;

        int difficultyPenalty;

        if (orderManager != null)
        {
            difficultyPenalty =
                GetPenaltyForCurrentDifficulty();
        }
        else
        {
            difficultyPenalty =
                Mathf.Max(
                    0,
                    penalty
                );
        }

        ApplyPenalty(
            difficultyPenalty
        );

        BeginOrderFinish(
            "TIEMPO AGOTADO\n\n-" +
            difficultyPenalty +
            " PUNTOS",
            FinishType.Timeout,
            difficultyPenalty
        );
    }

    private IEnumerator FinishOrderAfterDelay()
    {
        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                feedbackDuration
            )
        );

        HideFeedback();

        RemoveDeliveredExtras();

        if (score <= 0)
        {
            GameOver();

            yield break;
        }

        if (customerSpawner != null)
        {
            customerSpawner
                .StartCurrentCustomerLeaving();
        }

        if (assembly != null)
        {
            assembly.ResetAssembly();
        }

        if (deliveryZone != null)
        {
            deliveryZone.ClearDeliveryZone();
        }

        if (orderManager != null)
        {
            orderManager.enabled =
                orderManagerWasEnabled;

            orderManager.GenerateNewOrder();
        }

        processingDelivery = false;
        finishCoroutine = null;
    }

    private void GameOver()
    {
        processingDelivery = true;
        finishCoroutine = null;

        if (!gameResultSaved)
        {
            SaveManager.SaveGameResult(
                highestScore,
                correctOrders,
                incorrectOrders
            );

            gameResultSaved = true;

            RankingManager rankingManager =
                Object.FindAnyObjectByType
                    <RankingManager>();

            if (rankingManager != null)
            {
                rankingManager.RefreshRanking();
            }
        }

        if (orderManager != null)
        {
            orderManager.enabled =
                false;
        }

        if (customerSpawner != null)
        {
            customerSpawner.enabled =
                false;
        }

        if (gameOverText != null)
        {
            gameOverText.text =
                "FIN DEL TURNO\n\n" +
                "Tu puntuacion llego a 0.\n\n" +
                "Puntuacion maxima: " +
                highestScore +
                "\n\nPedidos atendidos: " +
                ordersCompleted +
                "\nCorrectos: " +
                correctOrders +
                "\nIncorrectos: " +
                incorrectOrders +
                "\n\nGAME OVER";
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(
                true
            );
        }

        Debug.Log(
            "💀 GAME OVER"
        );
    }

    private void RemoveDeliveredExtras()
    {
        if (deliveryZone == null)
        {
            return;
        }

        GameObject fries =
            deliveryZone.CurrentFries;

        GameObject drink =
            deliveryZone.CurrentDrink;

        if (fries != null)
        {
            Destroy(
                fries
            );
        }

        if (drink != null)
        {
            Destroy(
                drink
            );
        }
    }

    public void ApplyPenalty(
        int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        score -= amount;

        if (score < 0)
        {
            score = 0;
        }

        UpdateScoreText();

        Debug.Log(
            "⚠️ -" +
            amount +
            " puntos"
        );

        Debug.Log(
            "🏆 Puntuación total: " +
            score
        );
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "PUNTOS: " +
                score;
        }
    }

    private void ShowFeedback(
        string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text =
                message;

            if (message.StartsWith(
                "PEDIDO CORRECTO"))
            {
                feedbackText.color =
                    Color.green;
            }
            else
            {
                feedbackText.color =
                    Color.red;
            }
        }

        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(
                true
            );
        }
    }

    private void HideFeedback()
    {
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(
                false
            );
        }
    }
}