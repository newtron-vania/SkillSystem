using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Skill 클래스는 모든 스킬의 기본이 되는 추상 클래스입니다.
///     스킬 이름, 코스트, 쿨타임, 스킬 타입, 수치 정보(values)와 함께
///     이펙트 실행 순서와 상태를 관리합니다.
/// </summary>
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
        SkillIndex = skillData.skillIndex;
        SkillName = skillData.skillName;
        SkillType = skillData.GetSkillType(); // skillType을 Define.SkillType으로 설정
        Cost = skillData.cost;
        Cooltime = skillData.cooltime;
        Values = skillData.values;
    }

    private List<Effect> EffectList { get; } = new();

    /// <summary>
    ///     이펙트를 스킬에 등록합니다.
    /// </summary>
    protected void AddEffect(Effect effect)
    {
        EffectList.Add(effect);
    }

    /// <summary>
    ///     특정 순서의 이펙트를 반환합니다.
    /// </summary>
    protected Effect GetEffect(int index)
    {
        return EffectList[index];
    }

    /// <summary>
    ///     현재 등록된 이펙트 개수를 반환합니다.
    /// </summary>
    protected int GetEffectCount()
    {
        return EffectList.Count;
    }

    /// <summary>
    ///     스킬의 효과들을 초기화합니다. (오버라이딩용)
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    ///     스킬을 실행합니다. (하위 클래스에서 구현)
    /// </summary>
    public abstract bool ApplySkill(Actor source);
}


/// <summary>
///     NoneTargetSkill은 비대상형 스킬을 위한 클래스입니다.
///     사용 시 사거리 원을 먼저 표시하고, 마우스 클릭을 통해 대상 위치를 지정합니다.
///     이후 지정된 위치로 투사체 발사 등의 스킬이 적용됩니다.
/// </summary>
public class NoneTargetSkill : Skill
{
    private bool _isViewActive;
    private Action action;
    protected Vector3 targetPosition;

    public NoneTargetSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new NoneTargetRangeViewEffect()); // 사거리 시각화 효과 추가 (Effect[0])
    }

    public override bool ApplySkill(Actor source)
    {
        // 실행 중인지 여부 갱신
        IsRunning = _currentEffectIndex < GetEffectCount();

        // 이미 다른 스킬이 발동 중이라면 중단
        if (TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated)) return true;

        // 처음 사용할 경우: 사거리 표시 이펙트 실행 및 입력 바인딩
        if (!_isViewActive)
        {
            GetEffect(0).Initialize(Values[2]);
            _isViewActive = true;

            source.SkillManager.RegisterSkill(this, source);
            action = () => ClickSkillInput(source); // 클릭 이벤트 연결
            InputManager.Instance.AddMouseBinding(0, action);
        }

        // 사거리 시각화 적용
        GetEffect(0).Apply(source, null);
        return false;
    }

    /// <summary>
    ///     마우스 클릭 시 실행되는 콜백 함수입니다.
    ///     클릭한 위치를 targetPosition으로 저장하고 시각화 제거 및 실행 트리거를 켭니다.
    /// </summary>
    private void ClickSkillInput(Actor source)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        var groundLayer = LayerMask.GetMask("Ground");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            targetPosition = new Vector3(hit.point.x, 0, hit.point.z);
            source.targetPosition = targetPosition;
        }

        GetEffect(0).Clear(); // 사거리 이펙트 제거
        _isViewActive = false;
        InputManager.Instance.RemoveMouseBinding(0, action);
        TriggerManager.Instance.ActivateTrigger(GameTrigger.SkillActivated); // 스킬 시작 트리거
        Debug.Log($"{source.championName} 사용: {SkillName}");
    }
}

/// <summary>
///     InstantSkill은 즉시 발동되는 스킬을 위한 클래스입니다.
///     입력 없이 즉시 효과를 적용하며, SkillManager에 등록 후 외부에서 이펙트를 순차 실행합니다.
/// </summary>
public class InstantSkill : Skill
{
    private bool isRegistared = false;
    public InstantSkill(SkillData skillData) : base(skillData)
    {
    }

    public override bool ApplySkill(Actor source)
    {
        IsRunning = _currentEffectIndex < GetEffectCount();
        Debug.Log($"{source.championName} 사용: {SkillName}");

        if(!isRegistared) source.SkillManager.RegisterSkill(this, source); // 스킬 실행 등록
        
        return true; // 외부에서 이펙트 실행을 이어서 처리
    }
}

/// <summary>
///     TargetSkill은 대상 지정형 스킬을 위한 클래스입니다.
///     클릭을 통해 적 Actor를 지정하고, 사거리 내일 경우 해당 대상을 타겟으로 지정합니다.
/// </summary>
public class TargetSkill : Skill
{
    private Actor _target;
    private bool _isViewActive;
    private Action action;

    public TargetSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new TargetRangeViewEffect()); // 사거리 시각화 효과 (Effect[0])
    }

    public override bool ApplySkill(Actor source)
    {
        IsRunning = _currentEffectIndex < GetEffectCount();

        if (TriggerManager.Instance.IsTriggerActive(GameTrigger.SkillActivated)) return true;

        if (!_isViewActive)
        {
            GetEffect(0).Initialize(Values[2]);
            _isViewActive = true;

            source.SkillManager.RegisterSkill(this, source);
            action = () => ClickSkillInput(source);
            InputManager.Instance.AddMouseBinding(0, action);
        }

        GetEffect(0).Apply(source, null);
        return true;
    }

    /// <summary>
    ///     클릭 시 실행되는 입력 함수로, 지정된 Actor가 사거리 내에 있으면 타겟으로 설정합니다.
    /// </summary>
    private void ClickSkillInput(Actor source)
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        var actorLayer = LayerMask.GetMask("Actor");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, actorLayer))
        {
            if (Vector3.Distance(source.transform.position, hit.transform.position) <= Values[2])
            {
                _target = hit.transform.GetComponent<Actor>();
                source.target = _target;
            }

            GetEffect(0).Clear(); // 사거리 이펙트 제거
            _isViewActive = false;
            InputManager.Instance.RemoveMouseBinding(0, action);
            TriggerManager.Instance.ActivateTrigger(GameTrigger.SkillActivated); // 스킬 실행
            Debug.Log($"{source.championName} 사용: {SkillName}");
        }
    }
}