using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("CLIENTE")]
    public GameObject customerPrefab;

    [Header("PUNTOS")]
    public Transform spawnPoint;
    public Transform waitPoint;
    public Transform lookPoint;
    public Transform exitPoint;

    [Header("UI")]
    public GameObject orderPanel;

    [Header("PEDIDOS")]
    public OrderManager orderManager;

    private GameObject currentCustomer;
    private bool restoredFromSave = false;

    public GameObject CurrentCustomer =>
        currentCustomer;

    public bool HasCurrentCustomer =>
        currentCustomer != null;


    private void Start()
    {
        if (restoredFromSave)
        {
            return;
        }

        SpawnCustomer();
    }


    public void SpawnCustomer()
    {
        if (currentCustomer != null)
        {
            return;
        }

        if (customerPrefab == null)
        {
            Debug.LogWarning(
                "No hay Customer Prefab asignado."
            );

            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning(
                "No hay Spawn Point asignado."
            );

            return;
        }

        currentCustomer =
            Instantiate(
                customerPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

        ConfigureCustomer(
            currentCustomer
        );
    }


    private CustomerController ConfigureCustomer(
        GameObject customerObject)
    {
        if (customerObject == null)
        {
            return null;
        }

        CustomerController customer =
            customerObject.GetComponent
                <CustomerController>();

        if (customer == null)
        {
            Debug.LogWarning(
                "El prefab del cliente no tiene CustomerController."
            );

            return null;
        }

        customer.waitPoint =
            waitPoint;

        customer.lookPoint =
            lookPoint;

        customer.exitPoint =
            exitPoint;

        customer.orderPanel =
            orderPanel;

        customer.spawner =
            this;

        customer.orderManager =
            orderManager;

        return customer;
    }


    public void StartCurrentCustomerLeaving()
    {
        if (currentCustomer == null)
        {
            return;
        }

        CustomerController customer =
            currentCustomer.GetComponent
                <CustomerController>();

        if (customer != null)
        {
            customer.StartLeaving();
        }
    }


    public void CustomerFinished()
    {
        currentCustomer =
            null;

        SpawnCustomer();
    }


    public SaveManager.CustomerSaveData
        CreateSaveData()
    {
        if (currentCustomer == null)
        {
            SaveManager.CustomerSaveData emptyData =
                new SaveManager.CustomerSaveData();

            emptyData.exists =
                false;

            return emptyData;
        }

        CustomerController customer =
            currentCustomer.GetComponent
                <CustomerController>();

        if (customer != null)
        {
            return
                customer.CreateSaveData();
        }

        SaveManager.CustomerSaveData data =
            new SaveManager.CustomerSaveData();

        data.exists =
            true;

        data.position =
            new SaveManager.Vector3Data(
                currentCustomer
                    .transform
                    .position
            );

        data.rotation =
            new SaveManager.QuaternionData(
                currentCustomer
                    .transform
                    .rotation
            );

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
            restoredFromSave =
                false;

            return false;
        }

        if (customerPrefab == null)
        {
            Debug.LogWarning(
                "No se puede restaurar el cliente porque Customer Prefab no está asignado."
            );

            return false;
        }

        if (currentCustomer != null)
        {
            currentCustomer.SetActive(
                false
            );

            Destroy(
                currentCustomer
            );

            currentCustomer =
                null;
        }

        Vector3 position =
            spawnPoint != null
                ? spawnPoint.position
                : transform.position;

        Quaternion rotation =
            spawnPoint != null
                ? spawnPoint.rotation
                : transform.rotation;

        if (data.position != null)
        {
            position =
                data.position.ToVector3();
        }

        if (data.rotation != null)
        {
            rotation =
                data.rotation.ToQuaternion();
        }

        currentCustomer =
            Instantiate(
                customerPrefab,
                position,
                rotation
            );

        CustomerController customer =
            ConfigureCustomer(
                currentCustomer
            );

        if (customer == null)
        {
            Destroy(
                currentCustomer
            );

            currentCustomer =
                null;

            return false;
        }

        bool restored =
            customer.RestoreFromSaveData(
                data
            );

        if (!restored)
        {
            Destroy(
                currentCustomer
            );

            currentCustomer =
                null;

            return false;
        }

        restoredFromSave =
            true;

        Debug.Log(
            "💾 Cliente restaurado correctamente."
        );

        return true;
    }
}