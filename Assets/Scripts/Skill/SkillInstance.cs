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