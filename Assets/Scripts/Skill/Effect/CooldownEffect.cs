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