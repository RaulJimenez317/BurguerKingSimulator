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

    [Header("PUNTUACIÓN")]
    public int score = 0;
    public int incorrectOrderPenalty = 50;

    [Header("FEEDBACK")]
    public float feedbackDuration = 1.5f;

    private bool processingDelivery = false;
    private bool orderManagerWasEnabled = true;
    private Coroutine finishCoroutine;


    [Header("CLIENTE")]
    public CustomerSpawner customerSpawner;

    private void Start()
    {
        UpdateScoreText();
    }

    public void DeliverOrder()
    {
        if (processingDelivery)
        {
            return;
        }

        if (assembly == null)
        {
            Debug.LogWarning("No hay BurgerAssembly asignado en DeliveryManager.");
            return;
        }

        if (deliveryZone == null)
        {
            Debug.LogWarning("No hay DeliveryZone asignado en DeliveryManager.");
            return;
        }

        if (assembly.recipe == null)
        {
            Debug.LogWarning("No hay una receta activa.");
            return;
        }

        bool burgerCorrect =
            deliveryZone.HasBurger &&
            assembly.IsRecipeCorrect();

        bool meatCorrect =
            deliveryZone.HasBurger &&
            assembly.IsMeatCookingCorrect();

        bool friesCorrect = ValidateFries();
        bool drinkCorrect = ValidateDrink();

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
        bool friesRequired = assembly.recipe.includesFries;

        if (friesRequired)
        {
            if (!deliveryZone.HasFries)
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

        return !deliveryZone.HasFries;
    }

    private bool ValidateDrink()
    {
        bool drinkRequired = assembly.recipe.includesDrink;

        if (drinkRequired)
        {
            if (!deliveryZone.HasDrink)
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

        return !deliveryZone.HasDrink;
    }

    private void DeliverCorrectOrder()
    {
        int earnedPoints = 0;

        if (assembly.recipe != null)
        {
            earnedPoints = assembly.recipe.points;
        }

        score += earnedPoints;

        UpdateScoreText();

        Debug.Log("✅ Pedido entregado correctamente.");
        Debug.Log("🥩 Carne correctamente cocinada.");
        Debug.Log("⭐ +" + earnedPoints + " puntos");
        Debug.Log("🏆 Puntuación total: " + score);

        BeginOrderFinish("CORRECTO +" + earnedPoints);
    }

    private void DeliverIncorrectOrder(
        bool burgerCorrect,
        bool meatCorrect,
        bool friesCorrect,
        bool drinkCorrect)
    {
        Debug.Log("❌ Pedido entregado incorrectamente.");

        if (!burgerCorrect)
        {
            if (!deliveryZone.HasBurger)
            {
                Debug.Log("🍔 Falta la hamburguesa.");
            }
            else
            {
                Debug.Log("🍔 Hamburguesa incorrecta.");
            }
        }

        if (!meatCorrect &&
            deliveryZone.HasBurger &&
            assembly.HasMeat)
        {
            switch (assembly.MeatState)
            {
                case MeatCooking.CookingState.Raw:
                    Debug.Log("🥩 La carne está cruda.");
                    break;

                case MeatCooking.CookingState.Cooking:
                    Debug.Log("🥩 La carne todavía está poco cocinada.");
                    break;

                case MeatCooking.CookingState.Burned:
                    Debug.Log("🔥 La carne está quemada.");
                    break;

                case MeatCooking.CookingState.Ready:
                    Debug.Log("🥩 La carne está correctamente cocinada.");
                    break;
            }
        }

        if (!friesCorrect)
        {
            if (assembly.recipe.includesFries)
            {
                if (!deliveryZone.HasFries)
                {
                    Debug.Log("🍟 Faltan las papas.");
                }
                else
                {
                    Debug.Log("🍟 Las papas no están preparadas correctamente.");
                }
            }
            else
            {
                Debug.Log("🍟 El pedido no incluye papas.");
            }
        }

        if (!drinkCorrect)
        {
            if (assembly.recipe.includesDrink)
            {
                if (!deliveryZone.HasDrink)
                {
                    Debug.Log("🥤 Falta el refresco.");
                }
                else
                {
                    Debug.Log("🥤 El refresco no está preparado correctamente.");
                }
            }
            else
            {
                Debug.Log("🥤 El pedido no incluye refresco.");
            }
        }

        ApplyPenalty(incorrectOrderPenalty);

        BeginOrderFinish("INCORRECTO -" + incorrectOrderPenalty);
    }

    private void BeginOrderFinish(string feedbackMessage)
    {
        processingDelivery = true;

        if (statusText != null)
        {
            statusText.text = feedbackMessage;
        }

        if (orderManager != null)
        {
            orderManagerWasEnabled = orderManager.enabled;
            orderManager.enabled = false;
        }

        if (finishCoroutine != null)
        {
            StopCoroutine(finishCoroutine);
        }

        finishCoroutine =
            StartCoroutine(FinishOrderAfterDelay());
    }

    private IEnumerator FinishOrderAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);

        RemoveDeliveredExtras();

        if (customerSpawner != null)
        {
            customerSpawner.StartCurrentCustomerLeaving();
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
            orderManager.enabled = orderManagerWasEnabled;
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

    private void RemoveDeliveredExtras()
    {
        if (deliveryZone == null)
        {
            return;
        }

        GameObject fries = deliveryZone.CurrentFries;
        GameObject drink = deliveryZone.CurrentDrink;

        if (fries != null)
        {
            Destroy(fries);
        }

        if (drink != null)
        {
            Destroy(drink);
        }
    }

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

        Debug.Log("⚠️ -" + amount + " puntos");
        Debug.Log("🏆 Puntuación total: " + score);
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "PUNTOS: " + score;
        }
    }
}