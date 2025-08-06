
/// <summary>
/// CooldownEffect는 지정된 스킬 인덱스에 대해 쿨타임을 설정하는 효과를 담당하는 클래스입니다.
/// 스킬 사용 후 해당 스킬의 쿨다운을 source Actor의 skillCooldowns에 적용합니다.
/// </summary>
public class CooldownEffect : Effect
{
    private float _maxCooltime = 0f;
    private int _skillindex;

    public CooldownEffect(float maxCooltime, int skillindex) : base()
    {
        _maxCooltime = maxCooltime;
        _skillindex = skillindex;
    }
    public override void Initialize(params object[] objects)
    {
        if(objects[0] is float maxCooltime) _maxCooltime = maxCooltime;
        if(objects[1] is int skillIndex) _skillindex = skillIndex;
    }

    public override bool Apply(Actor source, Actor target)
    {
        source.skillCooldowns[_skillindex] = _maxCooltime;
        return true;
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}