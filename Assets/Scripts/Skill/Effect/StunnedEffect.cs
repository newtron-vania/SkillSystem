public class StunnedEffect : Effect
{
    private float _duration;  // 스턴 지속 시간

    public StunnedEffect(Actor owner, float maxDistance, float duration) : base()
    {
        _duration = duration;
    }

    public override void Initialize(params object[] objects)
    {
        if (objects[0] is float duration) _duration = duration;
    }

    public override bool Apply(Actor source, Actor target)
    {
        // Stunned 상태 적용
        target.ApplyStunnedEffect(_duration);
        return true;
    }

    public override void Clear()
    {
        throw new System.NotImplementedException();
    }
}