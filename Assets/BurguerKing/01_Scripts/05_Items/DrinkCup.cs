using UnityEngine;

public class DrinkCup : MonoBehaviour
{
    public bool isFilled = false;

    private Renderer cupRenderer;

    private void Start()
    {
        cupRenderer = GetComponent<Renderer>();
    }

    public void FillCup()
    {
        if (isFilled)
            return;

        isFilled = true;


        if (cupRenderer != null)
        {
            cupRenderer.material.color = Color.blue;
        }
    }
}
