using UnityEngine;

public class MeatCooking : MonoBehaviour
{
    public float cookingTime = 5f;
    public float burningTime = 8f;

    private float timer = 0f;
    private bool onGrill = false;

    private Renderer meatRenderer;

    private enum CookingState
    {
        Raw,
        Cooking,
        Ready,
        Burned
    }

    private CookingState state = CookingState.Raw;

    private void Start()
    {
        meatRenderer = GetComponent<Renderer>();
        meatRenderer.material.color = Color.red;
    }

    private void Update()
    {
        if (!onGrill)
            return;

        timer += Time.deltaTime;

        if (timer >= burningTime)
        {
            state = CookingState.Burned;
            meatRenderer.material.color = Color.black;
        }
        else if (timer >= cookingTime)
        {
            state = CookingState.Ready;
            meatRenderer.material.color = Color.yellow;
        }
        else
        {
            state = CookingState.Cooking;
            meatRenderer.material.color = Color.gray;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CookingZone"))
        {
            onGrill = true;
            Debug.Log("La carne está cocinándose");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CookingZone"))
        {
            onGrill = false;

            Debug.Log("Estado de la carne: " + state);
        }
    }
}