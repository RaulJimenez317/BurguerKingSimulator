using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("REFERENCIA")]
    public BurgerAssembly burgerAssembly;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Burger"))
        {

            if (burgerAssembly != null)
            {
                burgerAssembly.ResetAssembly();
            }

            return;
        }

        if (other.CompareTag("Ingredient") ||
            other.GetComponent<FriesBag>() != null ||
            other.GetComponent<DrinkCup>() != null)
        {

            Destroy(other.gameObject);
        }
    }
}