using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public GameObject pauseCanvas;
    public DeliveryManager deliveryManager;
    public Transform playerCamera;

    [Header("INPUT")]
    public InputActionReference pauseAction;

    [Header("POSICION DEL MENU")]
    [Min(0.5f)]
    public float menuDistance = 1.5f;

    private bool isPaused = false;
    private bool isChangingScene = false;

    private float lastToggleTime = -1f;

    private const float ToggleCooldown = 0.15f;
    private const string MainMenuSceneName = "MainMenu";


    private void Start()
    {
        // La escena siempre debe comenzar funcionando
        // a velocidad normal.
        Time.timeScale = 1f;

        isPaused = false;
        isChangingScene = false;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }

        if (deliveryManager == null)
        {
            deliveryManager =
                FindAnyObjectByType<DeliveryManager>();
        }

        if (playerCamera == null &&
            Camera.main != null)
        {
            playerCamera =
                Camera.main.transform;
        }

        ConfigureCanvasCamera();
    }


    private void Update()
    {
        // Permite probar el menú desde PC con Escape.
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }


    private void OnEnable()
    {
        if (pauseAction == null ||
            pauseAction.action == null)
        {
            return;
        }

        // Evita registrar el evento más de una vez.
        pauseAction.action.performed -=
            OnPauseAction;

        pauseAction.action.performed +=
            OnPauseAction;
    }


    private void OnDisable()
    {
        if (pauseAction == null ||
            pauseAction.action == null)
        {
            return;
        }

        pauseAction.action.performed -=
            OnPauseAction;
    }


    private void OnDestroy()
    {
        // Evita que otra escena quede congelada
        // si salimos mientras el juego estaba pausado.
        Time.timeScale = 1f;
    }


    private void OnPauseAction(
        InputAction.CallbackContext context)
    {
        if (isChangingScene)
        {
            return;
        }

        TogglePause();
    }


    public void TogglePause()
    {
        if (isChangingScene)
        {
            return;
        }

        // Evita dobles pulsaciones muy rápidas.
        if (Time.unscaledTime -
            lastToggleTime <
            ToggleCooldown)
        {
            return;
        }

        lastToggleTime =
            Time.unscaledTime;

        if (isPaused)
        {
            ContinueGame();
        }
        else
        {
            OpenPauseMenu();
        }
    }


    public void OpenPauseMenu()
    {
        if (isPaused ||
            isChangingScene)
        {
            return;
        }

        if (playerCamera == null &&
            Camera.main != null)
        {
            playerCamera =
                Camera.main.transform;
        }

        ConfigureCanvasCamera();
        PositionPauseMenu();

        isPaused = true;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(true);
        }

        Time.timeScale = 0f;
    }


    public void ContinueGame()
    {
        if (!isPaused ||
            isChangingScene)
        {
            return;
        }

        isPaused = false;

        Time.timeScale = 1f;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }
    }


    private void PositionPauseMenu()
    {
        if (pauseCanvas == null ||
            playerCamera == null)
        {
            return;
        }

        Vector3 forward =
            playerCamera.forward;

        // Mantiene el menú recto aunque el jugador
        // esté mirando hacia arriba o abajo.
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            forward =
                Vector3.forward;
        }

        forward.Normalize();

        Vector3 targetPosition =
            playerCamera.position +
            forward * menuDistance;

        targetPosition.y =
            playerCamera.position.y;

        pauseCanvas.transform.position =
            targetPosition;

        pauseCanvas.transform.rotation =
            Quaternion.LookRotation(
                forward,
                Vector3.up
            );
    }


    private void ConfigureCanvasCamera()
    {
        if (pauseCanvas == null ||
            playerCamera == null)
        {
            return;
        }

        Canvas canvas =
            pauseCanvas.GetComponent<Canvas>();

        if (canvas == null)
        {
            return;
        }

        Camera cameraComponent =
            playerCamera.GetComponent<Camera>();

        if (cameraComponent != null)
        {
            canvas.worldCamera =
                cameraComponent;
        }
    }


    public void SaveGame()
    {
        if (isChangingScene)
        {
            return;
        }

        if (deliveryManager == null)
        {
            deliveryManager =
                FindAnyObjectByType<DeliveryManager>();
        }

        if (deliveryManager == null)
        {
            Debug.LogWarning(
                "No hay DeliveryManager disponible para guardar."
            );

            return;
        }

        bool saved =
            deliveryManager.SaveCurrentGame();

        if (saved)
        {
            Debug.Log(
                "💾 Partida guardada correctamente."
            );
        }
        else
        {
            Debug.LogWarning(
                "No se pudo guardar la partida."
            );
        }
    }


    public void ReturnToMainMenu()
    {
        if (isChangingScene)
        {
            return;
        }

        isChangingScene = true;

        // Importantísimo:
        // nunca cargar otra escena con TimeScale = 0.
        Time.timeScale = 1f;

        isPaused = false;

        if (pauseCanvas != null)
        {
            pauseCanvas.SetActive(false);
        }

        // Dejamos limpio el indicador de carga.
        // Esto NO borra la partida guardada.
        SaveManager.ClearLoadRequest();

        // NO desactivamos el Input System.
        // NO tocamos InputActionManager.
        // NO apagamos XRI Default Input Actions.

        SceneManager.LoadScene(
            MainMenuSceneName
        );
    }


    public bool IsPaused()
    {
        return isPaused;
    }
}