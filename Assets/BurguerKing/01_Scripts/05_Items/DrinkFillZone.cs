using UnityEngine;

public class DrinkFillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        DrinkCup cup = other.GetComponent<DrinkCup>();

        if (cup != null)
        {
            cup.FillCup();
        }
    }
}