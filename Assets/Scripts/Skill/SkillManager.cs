using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillManager는 게임 내 모든 Actor들의 스킬을 관리하는 클래스입니다.
/// 각 Actor의 보유 스킬 목록을 저장하고, 스킬 사용 시 활성화 리스트에 등록하여 매 프레임마다 Tick을 통해 효과를 실행합니다.
/// </summary>
public class SkillManager : MonoBehaviour
{
    private Dictionary<Actor, List<Skill>> actorSkills = new(); // Actor별 보유 스킬 목록
    private List<SkillInstance> activeSkills = new();           // 현재 실행 중인 스킬 인스턴스 목록

    /// <summary>
    /// 매 프레임마다 모든 활성 스킬을 실행(Tick)하고,
    /// 완료된 스킬은 리스트에서 제거합니다.
    /// </summary>
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
                activeSkills[i].Clear();
                activeSkills.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 스킬을 Actor에게 등록하고 즉시 실행 큐(activeSkills)에 추가합니다.
    /// </summary>
    public void RegisterSkill(Skill skill, Actor actor)
    {
        if (!actorSkills.ContainsKey(actor))
            actorSkills[actor] = new List<Skill>();

        actorSkills[actor].Add(skill);
        activeSkills.Add(new SkillInstance(skill, actor)); // 스킬 인스턴스 생성 및 활성화

        TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated); // 트리거 상태 확인용 호출
    }

    /// <summary>
    /// 주어진 Actor에게 모든 스킬 데이터베이스 기반 스킬을 초기화 및 할당합니다.
    /// </summary>
    public void InitializeActorSkills(Actor actor, SkillDatabase skillDatabase)
    {
        var skills = new List<Skill>();

        foreach (var skillData in skillDatabase.skills)
        {
            var skill = CreateSkill(skillData);
            skills.Add(skill);
        }

        if (!actorSkills.ContainsKey(actor)) 
            actorSkills.Add(actor, skills);
    }

    /// <summary>
    /// 특정 Actor가 가진 모든 스킬을 반환합니다.
    /// </summary>
    public List<Skill> GetSkillsForActor(Actor actor)
    {
        if (actorSkills.ContainsKey(actor)) 
            return actorSkills[actor];

        Debug.LogError("No skills found for actor: " + actor.name);
        return null;
    }

    /// <summary>
    /// 스킬 데이터를 기반으로 실제 스킬 객체를 생성합니다.
    /// 스킬 인덱스를 기준으로 적절한 스킬 클래스 인스턴스를 반환합니다.
    /// </summary>
    private Skill CreateSkill(SkillData skillData)
    {
        switch (skillData.skillIndex)
        {
            case 101: return new RocketGrabSkill(skillData);
            case 102: return new OverDriveSkill(skillData);
            case 103: return new PowerFistSkill(skillData);
            default:
                Debug.LogError("Unknown skillIndex: " + skillData.skillIndex);
                return null;
        }
    }

    /// <summary>
    /// 특정 Actor가 가진 스킬 중 특정 인덱스의 스킬을 반환합니다.
    /// </summary>
    public Skill GetSkillForActorByIndex(Actor actor, int skillIndex)
    {
        var skills = GetSkillsForActor(actor);
        if (skills != null)
        {
            foreach (var skill in skills)
                if (skill.SkillIndex == skillIndex)
                    return skill;
        }

        Debug.LogError("Skill not found for actor and index: " + skillIndex);
        return null;
    }
}
