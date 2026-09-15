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

    private GameObject currentCustomer;

    private void Start()
    {
        SpawnCustomer();
    }

    public void SpawnCustomer()
    {
        if (currentCustomer != null)
            return;

        if (customerPrefab == null)
        {
            return;
        }

        if (spawnPoint == null)
        {
            
            return;
        }

        currentCustomer = Instantiate(
            customerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerController customer =
            currentCustomer.GetComponent<CustomerController>();

        if (customer != null)
        {
            customer.waitPoint = waitPoint;
            customer.lookPoint = lookPoint;
            customer.exitPoint = exitPoint;
            customer.orderPanel = orderPanel;
            customer.spawner = this;
        }

    }

    public void StartCurrentCustomerLeaving()
    {
        if (currentCustomer == null)
        {
            return;
        }

        CustomerController customer =
            currentCustomer.GetComponent<CustomerController>();

        if (customer != null)
        {
            customer.StartLeaving();
        }
    }

    public void CustomerFinished()
    {
        currentCustomer = null;


        SpawnCustomer();
    }
}