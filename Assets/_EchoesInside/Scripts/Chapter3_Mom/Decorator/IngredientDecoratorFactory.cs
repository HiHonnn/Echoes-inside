/// <summary>
/// Tập trung ánh xạ Ingredient ID sang Concrete Decorator tương ứng.
/// CookingStation chỉ làm việc với IMeal và không cần biết class cụ thể.
/// </summary>
public static class IngredientDecoratorFactory
{
    public static bool IsSupported(string ingredientId)
    {
        return ingredientId == "rice" ||
               ingredientId == "egg" ||
               ingredientId == "veggie" ||
               ingredientId == "meat" ||
               ingredientId == "mushroom" ||
               ingredientId == "spice" ||
               ingredientId == "carrot" ||
               ingredientId == "noodle" ||
               ingredientId == "onion";
    }

    public static bool TryCreate(
        string ingredientId,
        IMeal meal,
        float contributionRate,
        out IMeal decoratedMeal)
    {
        decoratedMeal = meal;

        if (meal == null || contributionRate <= 0f)
            return false;

        decoratedMeal = ingredientId switch
        {
            "rice"     => new RiceDecorator(meal, contributionRate),
            "egg"      => new EggDecorator(meal, contributionRate),
            "veggie"   => new VeggieDecorator(meal, contributionRate),
            "meat"     => new MeatDecorator(meal, contributionRate),
            "mushroom" => new MushroomDecorator(meal, contributionRate),
            "spice"    => new SpiceDecorator(meal, contributionRate),
            "carrot"   => new CarrotDecorator(meal, contributionRate),
            "noodle"   => new NoodleDecorator(meal, contributionRate),
            "onion"    => new OnionDecorator(meal, contributionRate),
            _           => meal
        };

        return IsSupported(ingredientId);
    }
}
