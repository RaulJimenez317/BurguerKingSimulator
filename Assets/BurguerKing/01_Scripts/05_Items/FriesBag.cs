using UnityEngine;

public class FriesBag : MonoBehaviour
{
    public bool isFilled = false;

    private Renderer bagRenderer;

    private void Start()
    {
        bagRenderer = GetComponent<Renderer>();
    }

    public void FillBag()
    {
        if (isFilled)
            return;

        isFilled = true;

        Debug.Log("¡Bolsa de papas llena!");

        if (bagRenderer != null)
        {
            bagRenderer.material.color = Color.yellow;
        }
    }
}