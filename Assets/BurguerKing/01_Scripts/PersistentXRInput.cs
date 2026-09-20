using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

public class PersistentXRInput : MonoBehaviour
{
    private static PersistentXRInput instance;

    [Header("XR INPUT")]
    [SerializeField]
    private InputActionAsset inputActions;


    private void Awake()
    {
        if (instance != null &&
            instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded +=
            OnSceneLoaded;

        EnableInput();
    }


    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        // Primero intentamos reconectar inmediatamente.
        RebindXRInteractionSimulator();

        // Y repetimos al siguiente frame para asegurarnos
        // de que el nuevo XR Origin ya terminó de inicializar.
        StartCoroutine(
            RefreshXRAfterSceneLoad()
        );
    }


    private IEnumerator RefreshXRAfterSceneLoad()
    {
        yield return null;
        yield return null;

        EnableInput();

        RebindXRInteractionSimulator();
    }


    private void EnableInput()
    {
        if (inputActions == null)
        {
            Debug.LogWarning(
                "PersistentXRInput: " +
                "XRI Default Input Actions no está asignado."
            );

            return;
        }

        if (!inputActions.enabled)
        {
            inputActions.Enable();
        }
    }


    private void RebindXRInteractionSimulator()
    {
#if UNITY_EDITOR

        XRInteractionSimulator simulator =
            XRInteractionSimulator.instance;

        if (simulator == null)
        {
            simulator =
                FindFirstObjectByType<XRInteractionSimulator>();
        }

        if (simulator == null)
        {
            Debug.LogWarning(
                "⚠ No se encontró XR Interaction Simulator."
            );

            return;
        }


        XRInputModalityManager modalityManager =
            FindFirstObjectByType<XRInputModalityManager>();


        Camera currentCamera =
            Camera.main;

        if (currentCamera != null)
        {
            simulator.cameraTransform =
                currentCamera.transform;
        }


        if (modalityManager != null)
        {
            if (modalityManager.leftController != null)
            {
                simulator.leftControllerTransform =
                    modalityManager
                        .leftController
                        .transform;
            }

            if (modalityManager.rightController != null)
            {
                simulator.rightControllerTransform =
                    modalityManager
                        .rightController
                        .transform;
            }
        }


        // Reiniciamos solamente el componente simulador
        // para que utilice las nuevas referencias.
        if (simulator.enabled)
        {
            simulator.enabled = false;
            simulator.enabled = true;
        }


        Debug.Log(
            "✅ XR Interaction Simulator reconectado " +
            "al XR Origin de la escena: " +
            SceneManager.GetActiveScene().name
        );

#endif
    }


    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -=
                OnSceneLoaded;

            instance = null;
        }
    }
}