using UnityEngine;

public class FriesFillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FriesBag bag = other.GetComponent<FriesBag>();

        if (bag != null)
        {
            bag.FillBag();
        }
    }
}