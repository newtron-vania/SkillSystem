
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Skill
{
    public int SkillIndex;
    public string SkillName;
    public Define.SkillType SkillType;
    public int Cost;
    public int Cooltime;
    public List<float> Values;

    protected int _currentEffectIndex = 1; // 0은 RangeViewEffect, 1부터 실행
    public bool IsRunning { get; protected set; } = true;

    public Skill(SkillData skillData)
    {
        this.SkillIndex = skillData.skillIndex;
        this.SkillName = skillData.skillName;
        this.SkillType = skillData.GetSkillType();  // skillType을 Define.SkillType으로 설정
        this.Cost = skillData.cost;
        this.Cooltime = skillData.cooltime;
        this.Values = skillData.values;
    }
    
    private List<Effect> EffectList {get;} = new();
    
    
    protected void AddEffect(Effect effect)
    {
        EffectList.Add(effect);
    }

    protected Effect GetEffect(int index)
    {
        return EffectList[index];
    }

    protected int GetEffectCount()
    {
        return EffectList.Count;
    }

    public virtual void Initialize()
    {
        
    }
    
    public abstract bool ApplySkill(Actor source);
    
}

public class NoneTargetSkill : Skill
{
    private bool _isViewActive = false;
    private Action action;
    protected Vector3 targetPosition;
    public NoneTargetSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new NoneTargetRangeViewEffect());
    }

    public override bool ApplySkill(Actor source)
    {
        IsRunning = _currentEffectIndex < GetEffectCount();
        if (TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated)) return true;
        
        if(!_isViewActive)
        {
            GetEffect(0).Initialize(Values[2]);
            _isViewActive = true;
            
            var skill = this as Skill;  // 현재 인스턴스가 실제 하위 클래스인 Skill 객체
            source.SkillManager.RegisterSkill(skill, source);
            action = () => ClickSkillInput(source);
            InputManager.Instance.AddMouseBinding(0, action);
        }
        
        GetEffect(0).Apply(source, null);
        return false;
    }

    private void ClickSkillInput(Actor source)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // "Ground" 레이어만 레이캐스트가 충돌하도록 LayerMask 사용
        int groundLayer = LayerMask.GetMask("Ground");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            targetPosition = new Vector3(hit.point.x, 0, hit.point.z);
            source.targetPosition = targetPosition;
        }
                
        GetEffect(0).Clear();
        _isViewActive = false;
        InputManager.Instance.RemoveMouseBinding(0, action);
        TriggerManager.Instance.ActivateTrigger(GameTrigger.SkillActivated);
        Debug.Log($"{source.championName} 사용: {this.SkillName}");
    }
}

// 예시로 "OverDrive" 스킬 (Instant)
public class InstantSkill : Skill
{
    public InstantSkill(SkillData skillData) : base(skillData) { }


    public override bool ApplySkill(Actor source)
    {
        IsRunning = _currentEffectIndex < GetEffectCount();
        Debug.Log($"{source.championName} 사용: {this.SkillName}");
        var skill = this as Skill;  // 현재 인스턴스가 실제 하위 클래스인 Skill 객체
        source.SkillManager.RegisterSkill(skill, source);
        return true;
    }
}

// 예시로 "Power Fist" 스킬 (Target)
public class TargetSkill : Skill
{
    private Actor _target;
    private bool _isViewActive = false;
    private Action action;
    public TargetSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new TargetRangeViewEffect());
    }
    
    public override bool ApplySkill(Actor source)
    {
        IsRunning = _currentEffectIndex < GetEffectCount();
        if (TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated)) return true;
        
        if(!_isViewActive)
        {
            GetEffect(0).Initialize(Values[2]);
            _isViewActive = true;
            var skill = this as Skill;  // 현재 인스턴스가 실제 하위 클래스인 Skill 객체
            source.SkillManager.RegisterSkill(skill, source);
            action = () => ClickSkillInput(source);
            InputManager.Instance.AddMouseBinding(0, action);
        }
        
        GetEffect(0).Apply(source, null);
        
        return true;
    }
    private void ClickSkillInput(Actor source)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // "Ground" 레이어만 레이캐스트가 충돌하도록 LayerMask 사용
        int actorLayer = LayerMask.GetMask("Actor");
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, actorLayer))
        {
            if (Vector3.Distance(source.transform.position, hit.transform.position) <= Values[2])
            {
                _target = hit.transform.GetComponent<Actor>();
                source.target = _target;
            }
            GetEffect(0).Clear();
                    
            TriggerManager.Instance.ActivateTrigger(GameTrigger.SkillActivated);
            Debug.Log($"{source.championName} 사용: {this.SkillName}");
        }
        _isViewActive = false;
        InputManager.Instance.RemoveMouseBinding(0, action);
        
        TriggerManager.Instance.ActivateTrigger(GameTrigger.SkillActivated);
        Debug.Log($"{source.championName} 사용: {this.SkillName}");
    }
}