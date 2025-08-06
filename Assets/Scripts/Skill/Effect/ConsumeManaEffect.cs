public class ConsumeManaEffect : Effect
{
    private float _cost = 0f;

    public ConsumeManaEffect(float cost) : base()
    {
        _cost = cost;
    }
    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float cost) _cost = cost;
    }

    public override bool Apply(Actor source, Actor target)
    {
        source.stat.mana = source.stat.mana - _cost;
        return true;
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}