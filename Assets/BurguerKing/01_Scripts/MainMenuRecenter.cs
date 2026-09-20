using System.Collections;
using UnityEngine;

public class MainMenuRecenter : MonoBehaviour
{
    [Header("REFERENCIAS")]
    public Transform xrOrigin;
    public Transform playerCamera;
    public Transform spawnPoint;

    [Header("CONFIGURACION")]
    [Min(1)]
    public int framesToWait = 2;


    private IEnumerator Start()
    {
        // Esperamos unos frames para que el XR Interaction Simulator
        // termine de reconectarse al XR Origin del MainMenu.
        for (int i = 0; i < framesToWait; i++)
        {
            yield return null;
        }

        RecenterPlayer();
    }


    public void RecenterPlayer()
    {
        // Intentamos recuperar la cámara automáticamente
        // si no fue asignada.
        if (playerCamera == null &&
            Camera.main != null)
        {
            playerCamera =
                Camera.main.transform;
        }

        if (xrOrigin == null ||
            playerCamera == null ||
            spawnPoint == null)
        {
            Debug.LogWarning(
                "MainMenuRecenter: faltan referencias."
            );

            return;
        }


        // ==========================================
        // 1. CORREGIR ROTACION HORIZONTAL
        // ==========================================

        float currentYaw =
            playerCamera.eulerAngles.y;

        float targetYaw =
            spawnPoint.eulerAngles.y;

        float yawDifference =
            Mathf.DeltaAngle(
                currentYaw,
                targetYaw
            );

        // Giramos alrededor de la propia cámara.
        // Así evitamos desplazar al jugador
        // accidentalmente durante la rotación.
        xrOrigin.RotateAround(
            playerCamera.position,
            Vector3.up,
            yawDifference
        );


        // ==========================================
        // 2. CORREGIR POSICION X / Z
        // ==========================================

        Vector3 cameraPosition =
            playerCamera.position;

        Vector3 originPosition =
            xrOrigin.position;

        // Movemos únicamente horizontalmente.
        // NO tocamos Y para conservar correctamente
        // la altura de la cabeza/cámara XR.
        originPosition.x +=
            spawnPoint.position.x -
            cameraPosition.x;

        originPosition.z +=
            spawnPoint.position.z -
            cameraPosition.z;

        xrOrigin.position =
            originPosition;


        Debug.Log(
            "✅ Jugador recentrado correctamente en MainMenu."
        );
    }
}