using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     RocketGrabSkill은 NoneTargetSkill을 상속한 블리츠크랭크의 대표 스킬인 '로켓 손'을 구현한 클래스입니다.
///     이 스킬은 투사체를 발사하여 적중 시 대상을 끌어오고, 피해를 입힌 후 Grab 효과를 적용합니다.
///     구성된 Effect들은 순차적으로 실행되며, 맞추기 전까지 대기 상태를 유지합니다.
/// </summary>
public class RocketGrabSkill : NoneTargetSkill
{
    private readonly ToBoolCheck isCatch = new();

    public RocketGrabSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new ConsumeManaEffect(Cost));
        AddEffect(new CooldownEffect(Cooltime, SkillIndex));
        AddEffect(new ActivateTriggerEffect(new List<GameTrigger> { GameTrigger.SkillActivated }));
        AddEffect(new ShootProjectileEffect(Values[3], "Projectile/DefaultProjectile", Values[2], (source, target) =>
        {
            var damageEffect = new DamageEffect(Values[0], Values[1], 0);
            damageEffect.Apply(source, target); // 즉시 데미지 적용
            GetEffect(5).Initialize(Values[3] * 2);
            isCatch._ischeck = true;
            source.target = target;
        }));
        AddEffect(new WaitForCheckEffect(isCatch));
        AddEffect(new DeActivateTriggerEffect(new List<GameTrigger> { GameTrigger.SkillActivated }));
        AddEffect(new GrabEffect());
    }

    public override void Initialize()
    {
        isCatch._ischeck = false;
        GetEffect(1).Initialize(Cost); // ConsumeManaEffect
        GetEffect(2).Initialize(Cooltime, SkillIndex); // CooldownEffect
        GetEffect(3).Initialize(Values[3], "Projectile/DefaultProjectile", Values[2],
            (Action<Actor, Actor>)((source, target) =>
            {
                var damageEffect = new DamageEffect(Values[0], Values[1], 0);
                damageEffect.Apply(source, target); // 즉시 데미지 적용
                GetEffect(5).Initialize(Values[3] * 2);
                isCatch._ischeck = true;
                source.target = target;
            })
        ); // ShootProjectileEffect
        GetEffect(4).Initialize(isCatch); // WaitForTimeEffect
    }

    public override bool ApplySkill(Actor source)
    {
        if (!base.ApplySkill(source))
            return false;

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            return true;
        }

        var effect = GetEffect(_currentEffectIndex);
        var success = effect.Apply(source, source.target ?? null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++; // 다음 Effect로
        }

        return false; // 아직 스킬 진행 중
    }

    public override void Clear()
    {
        base.Clear();
        _currentEffectIndex = 1; // 다시 초기화 가능하도록
        isCatch._ischeck = false;
    }
}

/// <summary>
///     OverDriveSkill은 InstantSkill을 상속한 블리츠크랭크의 자체 강화 스킬 '폭주'를 구현한 클래스입니다.
///     사용 시 일정 시간 동안 공격 속도 및 이동 속도에 영향을 주는 버프 효과를 부여합니다.
///     두 개의 MultiplyStatEffect로 각각의 스탯을 배수만큼 강화합니다.
///     현재 오류로 인해 미완성 상태입니다.
/// </summary>
public class OverDriveSkill : InstantSkill
{
    private bool _isStart;
    private readonly List<float> _multiplyValues1 = new() { 1, 1, 1, 1, 1, 1 };
    private readonly List<float> _multiplyValues2 = new() { 1, 1, 1, 1, 1, 1 };
    public OverDriveSkill(SkillData skillData) : base(skillData)
    {
        AddEffect(new WaitForTimeEffect(0.15f));
        AddEffect(new ConsumeManaEffect(Cost));
        AddEffect(new CooldownEffect(Cooltime, SkillIndex));
        _multiplyValues1[5] = Values[0];
        AddEffect(new MultiplyStatEffect(GameManager.Instance.Player.stat, _multiplyValues1, Values[1]));
        _multiplyValues2[5] = Values[2];
        AddEffect(new MultiplyStatEffect(GameManager.Instance.Player.stat, _multiplyValues2, Values[3]));
    }

    public override void Initialize()
    {
        GetEffect(1).Initialize(Cost); // ConsumeManaEffect
        GetEffect(2).Initialize(Cooltime, SkillIndex); // CooldownEffect
        _multiplyValues1[5] = Values[0];
        GetEffect(3).Initialize(GameManager.Instance.Player.stat, _multiplyValues1, Values[1]);
        _multiplyValues2[5] = Values[2];
        GetEffect(4).Initialize(GameManager.Instance.Player.stat, _multiplyValues2, Values[3]);
    }

    public override bool ApplySkill(Actor source)
    {
        if (!base.ApplySkill(source))
            return false;

        if (!_isStart)
        {
            _isStart = true;
            Initialize();
        }
        
        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            return true;
        }

        var effect = GetEffect(_currentEffectIndex);
        var success = effect.Apply(source, null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++; // 다음 Effect로
        }

        return false;
    }

    public override void Clear()
    {
        base.Clear();
        _currentEffectIndex = 0; // 다시 초기화 가능하도록
        _isStart = false;
        isRegistered = false;
    }
}


/// <summary>
///     PowerFistSkill은 TargetSkill을 상속한 블리츠크랭크의 강화 공격 스킬 '강철 주먹'을 구현한 클래스입니다.
///     현재는 스킬 로직이 미완성 상태이며, 추후 대상에게 강화된 기본 공격 또는 에어본 등의 효과를 추가할 예정입니다.
/// </summary>
public class PowerFistSkill : TargetSkill
{
    public PowerFistSkill(SkillData skillData) : base(skillData)
    {
    }

    public override bool ApplySkill(Actor source)
    {
        if (!base.ApplySkill(source))
            return false;

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            return true;
        }

        var effect = GetEffect(_currentEffectIndex);
        var success = effect.Apply(source, null);

        if (success)
        {
            Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
            _currentEffectIndex++; // 다음 Effect로
        }
        
        return false;
    }


    public override void Clear()
    {
        base.Clear();
        TriggerManager.Instance.DeactivateTrigger(GameTrigger.SkillActivated);
    }
}