public class RiceDecorator : IngredientDecorator
{
    public RiceDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Gạo";
}

public class EggDecorator : IngredientDecorator
{
    public EggDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Trứng";
}

public class VeggieDecorator : IngredientDecorator
{
    public VeggieDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Rau";
}

public class MeatDecorator : IngredientDecorator
{
    public MeatDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Thịt";
}

public class MushroomDecorator : IngredientDecorator
{
    public MushroomDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Nấm";
}

public class SpiceDecorator : IngredientDecorator
{
    public SpiceDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Gia vị";
}

public class CarrotDecorator : IngredientDecorator
{
    public CarrotDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Cà rốt";
}

public class NoodleDecorator : IngredientDecorator
{
    public NoodleDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Mì";
}

public class OnionDecorator : IngredientDecorator
{
    public OnionDecorator(IMeal meal, float rate) : base(meal, rate) { }
    public override string GetDescription() => _meal.GetDescription() + " + Hành";
}
