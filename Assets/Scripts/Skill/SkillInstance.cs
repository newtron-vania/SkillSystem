/// <summary>
///     SkillInstance는 실행 중인 스킬을 나타내는 클래스입니다.
///     어떤 Actor가 어떤 Skill을 사용하고 있는지를 나타내며,
///     매 프레임마다 Tick()을 호출해 스킬 효과를 순차적으로 처리합니다.
/// </summary>
public class SkillInstance
{
    public Skill Skill { get; }
    public Actor Source { get; }

    public SkillInstance(Skill skill, Actor source)
    {
        Skill = skill;
        Source = source;
    }

    public void Tick()
    {
        Skill.ApplySkill(Source);
    }

    public bool IsComplete => !Skill.IsRunning;
}