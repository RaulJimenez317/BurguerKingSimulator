using UnityEngine;

public class TrashBin : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {

            Destroy(other.gameObject);
        }
    }
}