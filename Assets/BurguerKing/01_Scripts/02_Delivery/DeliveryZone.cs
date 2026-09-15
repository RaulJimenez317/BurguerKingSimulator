using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DeliveryZone : MonoBehaviour
{
    public DeliveryManager deliveryManager;

    private GameObject currentBurger;
    private GameObject currentFries;
    private GameObject currentDrink;

    private readonly Dictionary<GameObject, int> objectsInside = new Dictionary<GameObject, int>();

    public GameObject CurrentBurger => currentBurger;
    public GameObject CurrentFries => currentFries;
    public GameObject CurrentDrink => currentDrink;

    public bool HasBurger => currentBurger != null;
    public bool HasFries => currentFries != null;
    public bool HasDrink => currentDrink != null;

    private void OnTriggerEnter(Collider other)
    {
        XRGrabInteractable grabInteractable =
            other.GetComponentInParent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject item = grabInteractable.gameObject;

        if (objectsInside.ContainsKey(item))
        {
            objectsInside[item]++;
        }
        else
        {
            objectsInside.Add(item, 1);
        }

        RegisterItem(item);
    }

    private void OnTriggerStay(Collider other)
    {
        XRGrabInteractable grabInteractable =
            other.GetComponentInParent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject item = grabInteractable.gameObject;

        if (!objectsInside.ContainsKey(item))
        {
            objectsInside.Add(item, 1);
        }

        RegisterItem(item);
    }

    private void OnTriggerExit(Collider other)
    {
        XRGrabInteractable grabInteractable =
            other.GetComponentInParent<XRGrabInteractable>();

        if (grabInteractable == null)
        {
            return;
        }

        GameObject item = grabInteractable.gameObject;

        if (!objectsInside.ContainsKey(item))
        {
            RemoveItemReference(item);
            return;
        }

        objectsInside[item]--;

        if (objectsInside[item] <= 0)
        {
            objectsInside.Remove(item);
            RemoveItemReference(item);
        }
    }

    private void RegisterItem(GameObject item)
    {
        if (item == null)
        {
            return;
        }

        if (item.CompareTag("Burger"))
        {
            currentBurger = item;
            return;
        }

        if (item.GetComponent<FriesBag>() != null)
        {
            currentFries = item;
            return;
        }

        if (item.GetComponent<DrinkCup>() != null)
        {
            currentDrink = item;
        }
    }

    private void RemoveItemReference(GameObject item)
    {
        if (item == currentBurger)
        {
            currentBurger = null;
        }

        if (item == currentFries)
        {
            currentFries = null;
        }

        if (item == currentDrink)
        {
            currentDrink = null;
        }
    }

    public void DeliverCurrentOrder()
    {
        CleanupReferences();

        if (deliveryManager == null)
        {
            Debug.LogWarning("No hay DeliveryManager asignado en DeliveryZone.");
            return;
        }

        if (currentBurger == null &&
            currentFries == null &&
            currentDrink == null)
        {
            Debug.Log("No hay productos en la zona de entrega.");
            return;
        }

        deliveryManager.DeliverOrder();
    }

    public void ClearDeliveryZone()
    {
        currentBurger = null;
        currentFries = null;
        currentDrink = null;
        objectsInside.Clear();
    }

    private void Update()
    {
        CleanupReferences();
    }

    private void CleanupReferences()
    {
        if (currentBurger != null && !currentBurger.activeInHierarchy)
        {
            objectsInside.Remove(currentBurger);
            currentBurger = null;
        }

        if (currentFries != null && !currentFries.activeInHierarchy)
        {
            objectsInside.Remove(currentFries);
            currentFries = null;
        }

        if (currentDrink != null && !currentDrink.activeInHierarchy)
        {
            objectsInside.Remove(currentDrink);
            currentDrink = null;
        }

        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (GameObject item in objectsInside.Keys)
        {
            if (item == null || !item.activeInHierarchy)
            {
                objectsToRemove.Add(item);
            }
        }

        foreach (GameObject item in objectsToRemove)
        {
            objectsInside.Remove(item);
        }
    }
}