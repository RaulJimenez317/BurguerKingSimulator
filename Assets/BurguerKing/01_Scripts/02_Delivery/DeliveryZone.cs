using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    public DeliveryManager deliveryManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Burger"))
        {
            Debug.Log("🍔 Hamburguesa llegó a la zona de entrega");

            deliveryManager.DeliverOrder();
        }
    }
}