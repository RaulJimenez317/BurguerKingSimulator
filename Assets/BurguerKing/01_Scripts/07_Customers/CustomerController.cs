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

    [Header("SPAWNER")]
    public CustomerSpawner spawner;

    [Header("PEDIDOS")]
    public OrderManager orderManager;

    private bool arrived = false;
    private bool finishedLooking = false;
    private bool leaving = false;
    private bool finishedTurning = false;

    private bool restoredFromSave = false;

    public bool Arrived => arrived;
    public bool FinishedLooking => finishedLooking;
    public bool IsLeaving => leaving;
    public bool FinishedTurning => finishedTurning;


    private void Start()
    {
        if (!restoredFromSave)
        {
            if (orderPanel != null)
            {
                orderPanel.SetActive(false);
            }
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
            return;
        }

        if (!finishedLooking)
        {
            TurnToPlayer();
        }
    }


    private void MoveToWaitPoint()
    {
        if (waitPoint == null)
        {
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                waitPoint.position,
                Mathf.Max(0f, moveSpeed) *
                Time.deltaTime
            );

        if (Vector3.Distance(
            transform.position,
            waitPoint.position) < 0.05f)
        {
            transform.position =
                waitPoint.position;

            arrived =
                true;

            if (orderPanel != null)
            {
                orderPanel.SetActive(true);
            }
        }
    }


    private void TurnToPlayer()
    {
        if (lookPoint == null)
        {
            return;
        }

        Vector3 direction =
            lookPoint.position -
            transform.position;

        direction.y =
            0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            ) *
            Quaternion.Euler(
                0f,
                -90f,
                0f
            );

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                Mathf.Max(
                    0f,
                    rotationSpeed
                ) *
                Time.deltaTime
            );

        if (Quaternion.Angle(
            transform.rotation,
            targetRotation) < 1f)
        {
            transform.rotation =
                targetRotation;

            finishedLooking =
                true;

            if (orderManager != null &&
                !orderManager.OrderActive)
            {
                orderManager
                    .StartOrderTimer();
            }
        }
    }


    public void StartLeaving()
    {
        if (leaving)
        {
            return;
        }

        leaving =
            true;

        finishedTurning =
            false;

        if (orderPanel != null)
        {
            orderPanel.SetActive(false);
        }
    }


    private void LeaveCounter()
    {
        if (exitPoint == null)
        {
            return;
        }

        Vector3 direction =
            exitPoint.position -
            transform.position;

        direction.y =
            0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            FinishLeaving();
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            ) *
            Quaternion.Euler(
                0f,
                -90f,
                0f
            );

        if (!finishedTurning)
        {
            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    Mathf.Max(
                        0f,
                        rotationSpeed
                    ) *
                    Time.deltaTime
                );

            if (Quaternion.Angle(
                transform.rotation,
                targetRotation) < 1f)
            {
                transform.rotation =
                    targetRotation;

                finishedTurning =
                    true;
            }

            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                exitPoint.position,
                Mathf.Max(
                    0f,
                    moveSpeed
                ) *
                Time.deltaTime
            );

        if (Vector3.Distance(
            transform.position,
            exitPoint.position) < 0.1f)
        {
            transform.position =
                exitPoint.position;

            FinishLeaving();
        }
    }


    private void FinishLeaving()
    {
        if (spawner != null)
        {
            spawner.CustomerFinished();
        }

        Destroy(
            gameObject
        );
    }


    public SaveManager.CustomerSaveData
        CreateSaveData()
    {
        SaveManager.CustomerSaveData data =
            new SaveManager.CustomerSaveData();

        data.exists =
            true;

        data.position =
            new SaveManager.Vector3Data(
                transform.position
            );

        data.rotation =
            new SaveManager.QuaternionData(
                transform.rotation
            );

        data.arrived =
            arrived;

        data.finishedLooking =
            finishedLooking;

        data.leaving =
            leaving;

        data.finishedTurning =
            finishedTurning;

        data.orderPanelActive =
            orderPanel != null &&
            orderPanel.activeSelf;

        return data;
    }


    public bool RestoreFromSaveData(
        SaveManager.CustomerSaveData data)
    {
        if (data == null ||
            !data.exists)
        {
            return false;
        }

        if (data.position != null)
        {
            transform.position =
                data.position.ToVector3();
        }

        if (data.rotation != null)
        {
            transform.rotation =
                data.rotation.ToQuaternion();
        }

        arrived =
            data.arrived;

        finishedLooking =
            data.finishedLooking;

        leaving =
            data.leaving;

        finishedTurning =
            data.finishedTurning;

        restoredFromSave =
            true;

        if (orderPanel != null)
        {
            orderPanel.SetActive(
                data.orderPanelActive
            );
        }

        return true;
    }
}