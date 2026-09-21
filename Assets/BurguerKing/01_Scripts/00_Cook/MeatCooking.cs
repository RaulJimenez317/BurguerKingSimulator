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

    private CookingState state =
        CookingState.Raw;

    private bool restoredFromSave = false;


    public CookingState CurrentState =>
        state;

    public bool IsReady =>
        state == CookingState.Ready;

    public bool IsRaw =>
        state == CookingState.Raw;

    public bool IsCooking =>
        state == CookingState.Cooking;

    public bool IsBurned =>
        state == CookingState.Burned;

    public bool IsOnGrill =>
        onGrill;

    public float CookingProgress =>
        timer;


    [Header("MODELOS")]
    public GameObject rawModel;
    public GameObject cookedModel;
    public GameObject burnedModel;


    private void Awake()
    {
        FindRenderer();
    }


    private void Start()
    {
        if (!restoredFromSave)
        {
            state =
                CookingState.Raw;

            timer =
                0f;

            onGrill =
                false;
        }

        ValidateCookingValues();

        UpdateMeatColor();
    }


    private void Update()
    {
        if (!onGrill)
        {
            return;
        }

        if (state ==
            CookingState.Burned)
        {
            return;
        }

        timer +=
            Time.deltaTime;

        CookingState previousState =
            state;

        UpdateCookingState();

        if (state != previousState)
        {
            UpdateMeatColor();

            if (state ==
                CookingState.Ready)
            {
                Debug.Log(
                    "✅ La carne está lista."
                );
            }
            else if (state ==
                     CookingState.Burned)
            {
                Debug.Log(
                    "🔥 La carne se quemó."
                );
            }
        }
    }


    private void OnTriggerEnter(
        Collider other)
    {
        if (other == null ||
            !other.CompareTag(
                "CookingZone"))
        {
            return;
        }

        onGrill =
            true;

        if (state ==
            CookingState.Raw)
        {
            state =
                CookingState.Cooking;

            UpdateMeatColor();
        }

        Debug.Log(
            "🥩 La carne está cocinándose."
        );
    }


    private void OnTriggerExit(
        Collider other)
    {
        if (other == null ||
            !other.CompareTag(
                "CookingZone"))
        {
            return;
        }

        onGrill =
            false;

        Debug.Log(
            "Estado de la carne: " +
            state
        );
    }


    private void UpdateCookingState()
    {
        ValidateCookingValues();

        if (timer >=
            burningTime)
        {
            state =
                CookingState.Burned;

            timer =
                Mathf.Max(
                    timer,
                    burningTime
                );

            return;
        }

        if (timer >=
            cookingTime)
        {
            state =
                CookingState.Ready;

            return;
        }

        if (timer > 0f ||
            onGrill)
        {
            state =
                CookingState.Cooking;

            return;
        }

        state =
            CookingState.Raw;
    }


    public SaveManager.MeatSaveData
        CreateSaveData()
    {
        SaveManager.MeatSaveData data =
            new SaveManager.MeatSaveData();

        data.hasMeatCooking =
            true;

        data.cookingState =
            (int)state;

        data.cookingProgress =
            Mathf.Max(
                0f,
                timer
            );

        data.onGrill =
            onGrill;

        return data;
    }


    public void RestoreFromSaveData(
        SaveManager.MeatSaveData data)
    {
        if (data == null ||
            !data.hasMeatCooking)
        {
            return;
        }

        FindRenderer();

        ValidateCookingValues();

        timer =
            Mathf.Max(
                0f,
                data.cookingProgress
            );

        onGrill =
            data.onGrill;

        if (System.Enum.IsDefined(
            typeof(CookingState),
            data.cookingState))
        {
            state =
                (CookingState)
                data.cookingState;
        }
        else
        {
            UpdateCookingState();
        }

        if (state ==
            CookingState.Raw)
        {
            timer =
                Mathf.Min(
                    timer,
                    Mathf.Max(
                        0f,
                        cookingTime
                    )
                );
        }

        if (state ==
            CookingState.Ready)
        {
            timer =
                Mathf.Clamp(
                    timer,
                    cookingTime,
                    burningTime
                );
        }

        if (state ==
            CookingState.Burned)
        {
            timer =
                Mathf.Max(
                    timer,
                    burningTime
                );
        }

        restoredFromSave =
            true;

        UpdateMeatColor();

        Debug.Log(
            "💾 Carne restaurada. Estado: " +
            state +
            " | Tiempo: " +
            timer
        );
    }


    private void ValidateCookingValues()
    {
        cookingTime =
            Mathf.Max(
                0.01f,
                cookingTime
            );

        burningTime =
            Mathf.Max(
                cookingTime,
                burningTime
            );
    }


    private void FindRenderer()
    {
        if (meatRenderer != null)
        {
            return;
        }

        meatRenderer =
            GetComponent<Renderer>();

        if (meatRenderer == null)
        {
            meatRenderer =
                GetComponentInChildren
                <Renderer>();
        }
    }


    private void UpdateMeatColor()
    {
        if (rawModel != null)
            rawModel.SetActive(false);

        if (cookedModel != null)
            cookedModel.SetActive(false);

        if (burnedModel != null)
            burnedModel.SetActive(false);

        switch (state)
        {
            case CookingState.Raw:

                if (rawModel != null)
                    rawModel.SetActive(true);

                break;

            case CookingState.Cooking:

                if (rawModel != null)
                    rawModel.SetActive(true);

                break;

            case CookingState.Ready:

                if (cookedModel != null)
                    cookedModel.SetActive(true);

                break;

            case CookingState.Burned:

                if (burnedModel != null)
                    burnedModel.SetActive(true);

                break;
        }
    }
}