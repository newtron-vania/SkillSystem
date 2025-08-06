
/// <summary>
/// StunnedEffect는 대상 Actor에게 일정 시간 동안 이동과 행동을 제한하는 스턴 상태를 부여하는 효과 클래스입니다.
/// 효과가 적용되면 대상의 Movement 컴포넌트가 비활성화되며, 지정된 시간 이후 자동으로 해제됩니다.
/// </summary>
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