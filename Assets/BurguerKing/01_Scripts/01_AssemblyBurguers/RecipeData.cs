using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipe", menuName = "BurgerRush/Recipe")]
public class RecipeData : ScriptableObject
{
    public string recipeName;

    public string[] ingredients;

    public float timeLimit;

    public int points;

    public int difficulty;

    public bool includesFries;
    public bool includesDrink;
}
