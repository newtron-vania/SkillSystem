using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    private Dictionary<Actor, List<Skill>> actorSkills = new();

    private List<SkillInstance> activeSkills = new();

    private void Update()
    {
        for (int i = 0; i < activeSkills.Count; i++)
        {
            activeSkills[i].Tick();
        }

        for (int i = activeSkills.Count - 1; i >= 0; i--)
        {
            if (activeSkills[i].IsComplete)
            {
                activeSkills.RemoveAt(i);
            }
        }
    }
    
    public void RegisterSkill(Skill skill, Actor actor)
    {
        if (!actorSkills.ContainsKey(actor))
            actorSkills[actor] = new List<Skill>();

        actorSkills[actor].Add(skill);
        activeSkills.Add(new SkillInstance(skill, actor)); // actor와 함께 등록
        TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated);
    }

    public void InitializeActorSkills(Actor actor, SkillDatabase skillDatabase)
    {
        var skills = new List<Skill>();

        foreach (var skillData in skillDatabase.skills)
        {
            var skill = CreateSkill(skillData);
            skills.Add(skill);
        }

        if (!actorSkills.ContainsKey(actor)) actorSkills.Add(actor, skills);
    }

    public List<Skill> GetSkillsForActor(Actor actor)
    {
        if (actorSkills.ContainsKey(actor)) return actorSkills[actor];

        Debug.LogError("No skills found for actor: " + actor.name);
        return null;
    }

    private Skill CreateSkill(SkillData skillData)
    {
        switch (skillData.skillIndex)
        {
            case 101: // RocketGrabSkill
                return new RocketGrabSkill(skillData);

            case 102: // OverDriveSkill
                return new OverDriveSkill(skillData);

            case 103: // PowerFistSkill
                return new PowerFistSkill(skillData);

            default:
                Debug.LogError("Unknown skillIndex: " + skillData.skillIndex);
                return null;
        }
    }

    public Skill GetSkillForActorByIndex(Actor actor, int skillIndex)
    {
        var skills = GetSkillsForActor(actor);
        if (skills != null)
            foreach (var skill in skills)
                if (skill.SkillIndex == skillIndex)
                    return skill;

        Debug.LogError("Skill not found for actor and index: " + skillIndex);
        return null;
    }
}