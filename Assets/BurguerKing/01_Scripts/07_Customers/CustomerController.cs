
using UnityEngine;

public class CustomerController : MonoBehaviour
{
    [Header("PUNTOS")]
    public Transform waitPoint;
    public Transform lookPoint;
    public Transform exitPoint;

    [Header("MOVIMIENTO")]
    public float moveSpeed = 2f;

    [Header("GIRO")]
    public float rotationSpeed = 180f;

    [Header("UI DEL PEDIDO")]
    public GameObject orderPanel;

    private bool arrived = false;
    private bool finishedLooking = false;
    private bool leaving = false;
    private bool finishedTurning = false;

    [Header("SPAWNER")]
    public CustomerSpawner spawner;

    [Header("PEDIDOS")]
    public OrderManager orderManager;

    private void Start()
    {
        if (orderPanel != null)
        {
            orderPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (leaving)
        {
            LeaveCounter();
            return;
        }

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

            if (orderPanel != null)
            {
                orderPanel.SetActive(true);
            }

        }
    }

    private void TurnToPlayer()
    {
        if (lookPoint == null)
            return;

        Vector3 direction =
            lookPoint.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(0f, -90f, 0f);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(
            transform.rotation,
            targetRotation) < 1f)
        {
            transform.rotation = targetRotation;

            finishedLooking = true;

            if (orderManager != null)
            {
                orderManager.StartOrderTimer();
            }
        }
    }

    public void StartLeaving()
    {
        if (leaving)
            return;

        leaving = true;
        finishedTurning = false;

        if (orderPanel != null)
        {
            orderPanel.SetActive(false);
        }

    }

    private void LeaveCounter()
    {
        if (exitPoint == null)
            return;

        Vector3 direction =
            exitPoint.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            FinishLeaving();
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction) *
            Quaternion.Euler(0f, -90f, 0f);

        if (!finishedTurning)
        {
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );

            if (Quaternion.Angle(
                transform.rotation,
                targetRotation) < 1f)
            {
                transform.rotation = targetRotation;
                finishedTurning = true;
            }

            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            exitPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(
            transform.position,
            exitPoint.position) < 0.1f)
        {
            FinishLeaving();
        }
    }

    private void FinishLeaving()
    {
        if (spawner != null)
        {
            spawner.CustomerFinished();
        }

        Destroy(gameObject);
    }
}

