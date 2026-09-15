using UnityEngine;

public class MeatCooking : MonoBehaviour
{
    public enum CookingState
    {
        Raw,
        Cooking,
        Ready,
        Burned
    }

    [Header("TIEMPOS")]
    public float cookingTime = 5f;
    public float burningTime = 8f;

    private float timer = 0f;
    private bool onGrill = false;

    private Renderer meatRenderer;
    private CookingState state = CookingState.Raw;

    public CookingState CurrentState => state;
    public bool IsReady => state == CookingState.Ready;
    public bool IsRaw => state == CookingState.Raw;
    public bool IsCooking => state == CookingState.Cooking;
    public bool IsBurned => state == CookingState.Burned;
    public float CookingProgress => timer;

    private void Start()
    {
        meatRenderer = GetComponent<Renderer>();

        if (meatRenderer == null)
        {
            meatRenderer = GetComponentInChildren<Renderer>();
        }

        state = CookingState.Raw;
        timer = 0f;

        UpdateMeatColor();
    }

    private void Update()
    {
        if (!onGrill)
        {
            return;
        }

        timer += Time.deltaTime;

        CookingState previousState = state;

        if (timer >= burningTime)
        {
            state = CookingState.Burned;
        }
        else if (timer >= cookingTime)
        {
            state = CookingState.Ready;
        }
        else
        {
            state = CookingState.Cooking;
        }

        if (state != previousState)
        {
            UpdateMeatColor();

            if (state == CookingState.Ready)
            {
                Debug.Log("✅ La carne está lista.");
            }
            else if (state == CookingState.Burned)
            {
                Debug.Log("🔥 La carne se quemó.");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("CookingZone"))
        {
            return;
        }

        onGrill = true;

        if (state == CookingState.Raw)
        {
            state = CookingState.Cooking;
            UpdateMeatColor();
        }

        Debug.Log("🥩 La carne está cocinándose.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("CookingZone"))
        {
            return;
        }

        onGrill = false;

        Debug.Log("Estado de la carne: " + state);
    }

    private void UpdateMeatColor()
    {
        if (meatRenderer == null)
        {
            return;
        }

        switch (state)
        {
            case CookingState.Raw:
                meatRenderer.material.color = Color.red;
                break;

            case CookingState.Cooking:
                meatRenderer.material.color = Color.gray;
                break;

            case CookingState.Ready:
                meatRenderer.material.color = Color.yellow;
                break;

            case CookingState.Burned:
                meatRenderer.material.color = Color.black;
                break;
        }
    }
}