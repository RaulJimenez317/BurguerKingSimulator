using System.Collections;
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

    // Se mantiene para compatibilidad y como valor de respaldo.
    public int incorrectOrderPenalty = 50;

    private int highestScore = 0;

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


    private enum FinishType
    {
        Correct,
        Incorrect,
        Timeout
    }


    private void Start()
    {
        highestScore = score;
        UpdateScoreText();
    }


    // =========================================================
    // ENTREGAR PEDIDO
    // =========================================================

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


    // =========================================================
    // VALIDAR PAPAS
    // =========================================================

    private bool ValidateFries()
    {
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
                deliveryZone.CurrentFries.GetComponent<FriesBag>();


            if (friesBag == null)
            {
                return false;
            }


            return friesBag.isFilled;
        }


        // Si el pedido no incluye papas,
        // añadir papas cuenta como error.
        return !deliveryZone.HasFries;
    }


    // =========================================================
    // VALIDAR REFRESCO
    // =========================================================

    private bool ValidateDrink()
    {
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
                deliveryZone.CurrentDrink.GetComponent<DrinkCup>();


            if (drinkCup == null)
            {
                return false;
            }


            return drinkCup.isFilled;
        }


        // Si el pedido no incluye refresco,
        // añadir refresco cuenta como error.
        return !deliveryZone.HasDrink;
    }


    // =========================================================
    // PEDIDO CORRECTO
    // =========================================================

    private void DeliverCorrectOrder()
    {
        int earnedPoints = 0;


        if (assembly.recipe != null)
        {
            earnedPoints =
                assembly.recipe.points;
        }


        score += earnedPoints;

        ordersCompleted++;
        correctOrders++;


        if (score > highestScore)
        {
            highestScore = score;
        }


        UpdateScoreText();


        Debug.Log(
            "✅ Pedido entregado correctamente."
        );

        Debug.Log(
            "🥩 Carne correctamente cocinada."
        );

        Debug.Log(
            "⭐ +" + earnedPoints + " puntos"
        );

        Debug.Log(
            "🏆 Puntuación total: " + score
        );


        BeginOrderFinish(
            "PEDIDO CORRECTO\n\n+" +
            earnedPoints +
            " PUNTOS",
            FinishType.Correct,
            0
        );
    }


    // =========================================================
    // PEDIDO INCORRECTO
    // =========================================================

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


        // -----------------------------------------------------
        // HAMBURGUESA
        // -----------------------------------------------------

        if (!burgerCorrect)
        {
            if (!deliveryZone.HasBurger)
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


        // -----------------------------------------------------
        // CARNE
        // -----------------------------------------------------

        if (!meatCorrect &&
            deliveryZone.HasBurger &&
            assembly.HasMeat)
        {
            switch (assembly.MeatState)
            {
                case MeatCooking.CookingState.Raw:

                    Debug.Log(
                        "La carne está cruda."
                    );

                    feedbackMessage +=
                        "- La carne está cruda.\n";

                    break;


                case MeatCooking.CookingState.Cooking:

                    Debug.Log(
                        "La carne todavía está poco cocinada."
                    );

                    feedbackMessage +=
                        "- La carne está poco cocinada.\n";

                    break;


                case MeatCooking.CookingState.Burned:

                    Debug.Log(
                        "La carne está quemada."
                    );

                    feedbackMessage +=
                        "- La carne está quemada.\n";

                    break;


                case MeatCooking.CookingState.Ready:

                    Debug.Log(
                        "La carne está correctamente cocinada."
                    );

                    break;
            }
        }


        // -----------------------------------------------------
        // PAPAS
        // -----------------------------------------------------

        if (!friesCorrect)
        {
            if (assembly.recipe.includesFries)
            {
                if (!deliveryZone.HasFries)
                {
                    Debug.Log(
                        "Faltan las papas."
                    );

                    feedbackMessage +=
                        "- Faltan las papas.\n";
                }
                else
                {
                    Debug.Log(
                        "Las papas no están preparadas correctamente."
                    );

                    feedbackMessage +=
                        "- Las papas no están preparadas correctamente.\n";
                }
            }
            else
            {
                Debug.Log(
                    "El pedido no incluye papas."
                );

                feedbackMessage +=
                    "- El pedido no incluye papas.\n";
            }
        }


        // -----------------------------------------------------
        // REFRESCO
        // -----------------------------------------------------

        if (!drinkCorrect)
        {
            if (assembly.recipe.includesDrink)
            {
                if (!deliveryZone.HasDrink)
                {
                    Debug.Log(
                        "Falta el refresco."
                    );

                    feedbackMessage +=
                        "- Falta el refresco.\n";
                }
                else
                {
                    Debug.Log(
                        "El refresco no está preparado correctamente."
                    );

                    feedbackMessage +=
                        "- El refresco no está preparado correctamente.\n";
                }
            }
            else
            {
                Debug.Log(
                    "El pedido no incluye refresco."
                );

                feedbackMessage +=
                    "- El pedido no incluye refresco.\n";
            }
        }


        ordersCompleted++;
        incorrectOrders++;


        // Obtenemos la penalización correspondiente
        // a la dificultad actual.
        int penalty =
            GetPenaltyForCurrentDifficulty();


        ApplyPenalty(penalty);


        feedbackMessage +=
            "\n-" +
            penalty +
            " PUNTOS";


        Debug.Log(
            "📊 Dificultad actual: " +
            GetCurrentDifficulty()
        );

        Debug.Log(
            "⚠️ Penalización aplicada: -" +
            penalty +
            " puntos"
        );


        BeginOrderFinish(
            feedbackMessage,
            FinishType.Incorrect,
            penalty
        );
    }


    // =========================================================
    // PENALIZACIÓN SEGÚN DIFICULTAD
    // =========================================================

    private int GetPenaltyForCurrentDifficulty()
    {
        if (orderManager == null)
        {
            Debug.LogWarning(
                "No hay OrderManager asignado. " +
                "Se utilizará la penalización de respaldo."
            );

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

                // Si más adelante agregas dificultad 4 o superior,
                // se utilizará de momento la penalización máxima.
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


    // =========================================================
    // FINALIZAR PEDIDO
    // =========================================================

    private void BeginOrderFinish(
        string feedbackMessage,
        FinishType finishType,
        int appliedPenalty)
    {
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
                            assembly.recipe.points;
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


    // =========================================================
    // TIEMPO AGOTADO
    // =========================================================

    public void HandleTimeout(int penalty)
    {
        if (processingDelivery)
        {
            return;
        }


        Debug.Log(
            "⏰ Pedido perdido por falta de tiempo."
        );


        ordersCompleted++;
        incorrectOrders++;


        // Mantenemos el parámetro "penalty"
        // para que tu OrderManager actual siga funcionando.
        //
        // La penalización real ahora depende de la dificultad.
        int difficultyPenalty;


        if (orderManager != null)
        {
            difficultyPenalty =
                GetPenaltyForCurrentDifficulty();
        }
        else
        {
            // Si por algún motivo falta OrderManager,
            // se usa el valor enviado originalmente.
            difficultyPenalty =
                Mathf.Max(0, penalty);
        }


        ApplyPenalty(
            difficultyPenalty
        );


        Debug.Log(
            "📊 Dificultad actual: " +
            GetCurrentDifficulty()
        );

        Debug.Log(
            "⏰ Penalización por tiempo: -" +
            difficultyPenalty +
            " puntos"
        );


        BeginOrderFinish(
            "TIEMPO AGOTADO\n\n-" +
            difficultyPenalty +
            " PUNTOS",
            FinishType.Timeout,
            difficultyPenalty
        );
    }


    // =========================================================
    // ESPERA ENTRE PEDIDOS
    // =========================================================

    private IEnumerator FinishOrderAfterDelay()
    {
        yield return new WaitForSecondsRealtime(
            feedbackDuration
        );


        HideFeedback();


        RemoveDeliveredExtras();


        // Si se queda sin puntos,
        // termina la partida.
        if (score <= 0)
        {
            GameOver();

            yield break;
        }


        // El cliente actual comienza a marcharse.
        if (customerSpawner != null)
        {
            customerSpawner
                .StartCurrentCustomerLeaving();
        }


        // Limpiar estación de armado.
        if (assembly != null)
        {
            assembly.ResetAssembly();
        }


        // Limpiar zona de entrega.
        if (deliveryZone != null)
        {
            deliveryZone.ClearDeliveryZone();
        }


        // Preparar siguiente pedido.
        if (orderManager != null)
        {
            orderManager.enabled =
                orderManagerWasEnabled;

            orderManager.GenerateNewOrder();
        }
        else
        {
            Debug.LogWarning(
                "No hay OrderManager asignado en DeliveryManager."
            );
        }


        processingDelivery = false;
        finishCoroutine = null;
    }


    // =========================================================
    // GAME OVER
    // =========================================================

    private void GameOver()
    {
        processingDelivery = true;


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


    // =========================================================
    // ELIMINAR EXTRAS ENTREGADOS
    // =========================================================

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
            Destroy(fries);
        }


        if (drink != null)
        {
            Destroy(drink);
        }
    }


    // =========================================================
    // APLICAR PENALIZACIÓN
    // =========================================================

    public void ApplyPenalty(int amount)
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


    // =========================================================
    // ACTUALIZAR PUNTUACIÓN
    // =========================================================

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text =
                "PUNTOS: " +
                score;
        }
    }


    // =========================================================
    // FEEDBACK
    // =========================================================

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