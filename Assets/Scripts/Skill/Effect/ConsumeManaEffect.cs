

/// <summary>
/// ConsumeManaEffect는 스킬 사용 시 지정된 마나를 소모하는 효과를 정의하는 클래스입니다.
/// 적용 대상은 source(시전자)이며, 스킬 실행과 동시에 마나를 차감합니다.
/// </summary>
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