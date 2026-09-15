
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    [Header("PUNTOS")]
    public Transform waitPoint;
    public Transform lookPoint;

    [Header("MOVIMIENTO")]
    public float moveSpeed = 2f;

    [Header("GIRO")]
    public float rotationSpeed = 180f;

    private bool arrived = false;
    private bool finishedLooking = false;

    private void Update()
    {
        if (!arrived)
        {
            MoveToWaitPoint();
        }
        else if (!finishedLooking)
        {
            TurnToPlayer();
        }
    }

    private void MoveToWaitPoint()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            waitPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, waitPoint.position) < 0.05f)
        {
            transform.position = waitPoint.position;

            arrived = true;

            Debug.Log("🧍 Cliente llegó al mostrador.");
        }
    }

    private void TurnToPlayer()
    {
        if (lookPoint == null)
            return;

        Vector3 direction = lookPoint.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // Como el frente visual del modelo está en X+
        // compensamos los 90 grados respecto al Z+ de Unity.
        Quaternion targetRotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(0f, -90f, 0f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
        {
            transform.rotation = targetRotation;

            finishedLooking = true;

            Debug.Log("👀 Cliente está mirando al jugador.");
        }
    }
}

