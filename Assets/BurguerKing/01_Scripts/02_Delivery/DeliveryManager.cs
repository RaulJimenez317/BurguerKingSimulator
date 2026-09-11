using UnityEngine;

public class DeliveryManager : MonoBehaviour
{
    public BurgerAssembly assembly;
    public int points = 100;

    public void DeliverOrder()
    {
        if (assembly.IsRecipeCorrect())
        {
            Debug.Log("🍔 ¡Pedido entregado correctamente!");
            Debug.Log("⭐ +" + points + " puntos");
        }
        else
        {
            Debug.Log("❌ No puedes entregar una hamburguesa incorrecta.");
        }
    }
}