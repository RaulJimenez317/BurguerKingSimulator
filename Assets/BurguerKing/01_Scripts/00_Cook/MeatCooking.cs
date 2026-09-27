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


    [Header("MODELOS")]
    public GameObject rawModel;
    public GameObject cookedModel;
    public GameObject burnedModel;


    [Header("SONIDOS")]
    [SerializeField]
    private AudioClip putOnGrillSound;

    [SerializeField]
    private AudioClip cookingSound;

    [SerializeField]
    private AudioClip readySound;

    [SerializeField]
    private AudioClip burnedSound;


    [Header("AUDIO SOURCES")]
    [SerializeField]
    private AudioSource effectsAudioSource;

    [SerializeField]
    private AudioSource cookingAudioSource;


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


    private void Awake()
    {
        FindRenderer();

        SetupAudioSources();
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

        UpdateCookingAudio();
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
            StopCookingSound();

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
                PlayEffect(
                    readySound
                );

                Debug.Log(
                    "✅ La carne está lista."
                );
            }
            else if (state ==
                     CookingState.Burned)
            {
                StopCookingSound();

                PlayEffect(
                    burnedSound
                );

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


        // Sonido de colocar la carne.
        PlayEffect(
            putOnGrillSound
        );


        if (state ==
            CookingState.Raw)
        {
            state =
                CookingState.Cooking;

            UpdateMeatColor();
        }


        // Empieza el chisporroteo.
        if (state !=
            CookingState.Burned)
        {
            StartCookingSound();
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


        // Deja de chisporrotear
        // cuando se saca de la parrilla.
        StopCookingSound();


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

        SetupAudioSources();

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

        // Si la partida se guardó con la
        // carne sobre la parrilla,
        // continúa el sonido de cocción.
        UpdateCookingAudio();


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


    private void SetupAudioSources()
    {
        // AudioSource para efectos:
        // poner carne, lista y quemada.
        if (effectsAudioSource == null)
        {
            effectsAudioSource =
                gameObject.AddComponent
                <AudioSource>();
        }


        // AudioSource independiente
        // para el sonido continuo de cocción.
        if (cookingAudioSource == null)
        {
            cookingAudioSource =
                gameObject.AddComponent
                <AudioSource>();
        }


        ConfigureAudioSource(
            effectsAudioSource
        );

        ConfigureAudioSource(
            cookingAudioSource
        );


        cookingAudioSource.loop =
            true;
    }


    private void ConfigureAudioSource(
        AudioSource source)
    {
        if (source == null)
        {
            return;
        }


        source.playOnAwake =
            false;

        // Sonido 3D para VR.
        source.spatialBlend =
            1f;
    }


    private void PlayEffect(
        AudioClip clip)
    {
        if (clip == null ||
            effectsAudioSource == null)
        {
            return;
        }


        effectsAudioSource.PlayOneShot(
            clip
        );
    }


    private void StartCookingSound()
    {
        if (cookingAudioSource == null ||
            cookingSound == null)
        {
            return;
        }


        if (cookingAudioSource.isPlaying &&
            cookingAudioSource.clip ==
            cookingSound)
        {
            return;
        }


        cookingAudioSource.Stop();

        cookingAudioSource.clip =
            cookingSound;

        cookingAudioSource.loop =
            true;

        cookingAudioSource.Play();
    }


    private void StopCookingSound()
    {
        if (cookingAudioSource == null)
        {
            return;
        }


        if (cookingAudioSource.isPlaying)
        {
            cookingAudioSource.Stop();
        }
    }


    private void UpdateCookingAudio()
    {
        if (onGrill &&
            state != CookingState.Burned)
        {
            StartCookingSound();
        }
        else
        {
            StopCookingSound();
        }
    }


    private void OnDisable()
    {
        StopCookingSound();
    }


    private void UpdateMeatColor()
    {
        if (rawModel != null)
        {
            rawModel.SetActive(
                false
            );
        }


        if (cookedModel != null)
        {
            cookedModel.SetActive(
                false
            );
        }


        if (burnedModel != null)
        {
            burnedModel.SetActive(
                false
            );
        }


        switch (state)
        {
            case CookingState.Raw:

                if (rawModel != null)
                {
                    rawModel.SetActive(
                        true
                    );
                }

                break;


            case CookingState.Cooking:

                if (rawModel != null)
                {
                    rawModel.SetActive(
                        true
                    );
                }

                break;


            case CookingState.Ready:

                if (cookedModel != null)
                {
                    cookedModel.SetActive(
                        true
                    );
                }

                break;


            case CookingState.Burned:

                if (burnedModel != null)
                {
                    burnedModel.SetActive(
                        true
                    );
                }

                break;
        }
    }
}