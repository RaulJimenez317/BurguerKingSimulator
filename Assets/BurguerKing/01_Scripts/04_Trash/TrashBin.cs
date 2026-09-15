using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("REFERENCIA")]
    public BurgerAssembly burgerAssembly;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient") ||
            other.CompareTag("Burger") ||
            other.GetComponent<FriesBag>() != null ||
            other.GetComponent<DrinkCup>() != null)
        {

            bool isBurger = other.CompareTag("Burger");

            Destroy(other.gameObject);

            if (isBurger && burgerAssembly != null)
            {
                burgerAssembly.ResetAssembly();
            }
        }
    }
}