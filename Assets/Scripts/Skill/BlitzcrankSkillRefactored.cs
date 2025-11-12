using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// RocketGrabSkill은 NoneTargetSkill을 상속한 블리츠크랭크의 대표 스킬인 '로켓 손'을 구현한 클래스입니다.
/// 이 스킬은 투사체를 발사하여 적중 시 대상을 끌어오고, 피해를 입힌 후 Grab 효과를 적용합니다.
/// 구성된 Effect들은 순차적으로 실행되며, 맞추기 전까지 대기 상태를 유지합니다.
/// </summary>
public class RocketGrabSkill : NoneTargetSkill
{
    private readonly ToBoolCheck _isCatch = new ToBoolCheck();

    public RocketGrabSkill(SkillData skillData,
                          IInputService inputService = null,
                          ITriggerService triggerService = null,
                          ITargetingService targetingService = null,
                          ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        // Effects를 생성하고 나중에 Initialize에서 설정
        AddEffect(new ConsumeManaEffect());
        AddEffect(new CooldownEffect());
        AddEffect(new ShootProjectileEffect());
        AddEffect(new WaitForCheckEffect());
        AddEffect(new DeActivateTriggerEffect());
        AddEffect(new GrabEffect());
    }

    public override SkillResult Initialize()
    {
        var baseResult = base.Initialize();
        if (!baseResult.IsSuccess) return baseResult;

        try
        {
            _isCatch._ischeck = false;

            // Effect 1: ConsumeManaEffect
            var consumeManaResult = GetEffect(1);
            if (consumeManaResult.IsSuccess)
            {
                var initResult = consumeManaResult.Value.Initialize(Cost);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 2: CooldownEffect
            var cooldownResult = GetEffect(2);
            if (cooldownResult.IsSuccess)
            {
                var initResult = cooldownResult.Value.Initialize(Cooltime, SkillIndex);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 3: ShootProjectileEffect
            var projectileResult = GetEffect(3);
            if (projectileResult.IsSuccess)
            {
                var onHitAction = new Action<Actor, Actor>((source, target) =>
                {
                    // 즉시 데미지 적용
                    var damageEffect = new DamageEffect();
                    damageEffect.Initialize(Values[0], Values[1], 0f);
                    damageEffect.Apply(source, target);

                    // Grab 효과 초기화
                    var grabResult = GetEffect(5);
                    if (grabResult.IsSuccess)
                    {
                        grabResult.Value.Initialize(Values[3] * 2);
                    }

                    _isCatch._ischeck = true;
                    source.target = target;
                });

                var initResult = projectileResult.Value.Initialize(
                    Values[3], // speed
                    "Projectile/DefaultProjectile", // prefab path
                    Values[2], // range
                    onHitAction // on hit callback
                );
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 4: WaitForCheckEffect
            var waitResult = GetEffect(4);
            if (waitResult.IsSuccess)
            {
                var initResult = waitResult.Value.Initialize(_isCatch);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 5: DeActivateTriggerEffect
            var deactivateResult = GetEffect(5);
            if (deactivateResult.IsSuccess)
            {
                var triggers = new List<Define.GameTrigger> { Define.GameTrigger.SkillActivated };
                var initResult = deactivateResult.Value.Initialize(triggers);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 6: GrabEffect는 기본 초기화만
            var grabEffectResult = GetEffect(6);
            if (grabEffectResult.IsSuccess)
            {
                var initResult = grabEffectResult.Value.Initialize();
                if (!initResult.IsSuccess) return initResult;
            }

            return SkillResult.Success("RocketGrabSkill initialized successfully");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error initializing RocketGrabSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var baseResult = base.ApplySkill(source);
        if (!baseResult.IsSuccess)
            return baseResult;

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            _stateMachine.TransitionTo(SkillState.Completed);
            return SkillResult.Success("RocketGrabSkill completed");
        }

        try
        {
            var effectResult = GetEffect(_currentEffectIndex);
            if (!effectResult.IsSuccess)
            {
                return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
            }

            var success = effectResult.Value.Apply(source, source.target);

            if (success)
            {
                Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
                _currentEffectIndex++;
            }

            return SkillResult.Success("RocketGrabSkill effect applied");
        }
        catch (Exception ex)
        {
            _stateMachine.TransitionTo(SkillState.Failed);
            return SkillResult.Failure($"Error applying RocketGrabSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult Clear()
    {
        try
        {
            var baseResult = base.Clear();
            _currentEffectIndex = 1; // Skip range view effect
            _isCatch._ischeck = false;
            return baseResult;
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error clearing RocketGrabSkill: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// OverDriveSkill은 InstantSkill을 상속한 블리츠크랭크의 자체 강화 스킬 '폭주'를 구현한 클래스입니다.
/// 사용 시 일정 시간 동안 공격 속도 및 이동 속도에 영향을 주는 버프 효과를 부여합니다.
/// 두 개의 MultiplyStatEffect로 각각의 스탯을 배수만큼 강화합니다.
/// </summary>
public class OverDriveSkill : InstantSkill
{
    private readonly List<float> _multiplyValues1 = new List<float> { 1, 1, 1, 1, 1, 1 };
    private readonly List<float> _multiplyValues2 = new List<float> { 1, 1, 1, 1, 1, 1 };

    public OverDriveSkill(SkillData skillData,
                         IInputService inputService = null,
                         ITriggerService triggerService = null,
                         ITargetingService targetingService = null,
                         ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        AddEffect(new WaitForTimeEffect());
        AddEffect(new ConsumeManaEffect());
        AddEffect(new CooldownEffect());
        AddEffect(new MultiplyStatEffect());
        AddEffect(new MultiplyStatEffect());
    }

    public override SkillResult Initialize()
    {
        var baseResult = base.Initialize();
        if (!baseResult.IsSuccess) return baseResult;

        try
        {
            // Effect 0: WaitForTimeEffect (0.15초 대기)
            var waitResult = GetEffect(0);
            if (waitResult.IsSuccess)
            {
                var initResult = waitResult.Value.Initialize(0.15f);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 1: ConsumeManaEffect
            var consumeManaResult = GetEffect(1);
            if (consumeManaResult.IsSuccess)
            {
                var initResult = consumeManaResult.Value.Initialize(Cost);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 2: CooldownEffect
            var cooldownResult = GetEffect(2);
            if (cooldownResult.IsSuccess)
            {
                var initResult = cooldownResult.Value.Initialize(Cooltime, SkillIndex);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 3: MultiplyStatEffect (첫 번째 스탯 강화)
            var stat1Result = GetEffect(3);
            if (stat1Result.IsSuccess)
            {
                _multiplyValues1[5] = Values[0]; // 공격속도 강화
                var playerStat = GameManager.Instance?.Player?.stat;
                if (playerStat != null)
                {
                    var initResult = stat1Result.Value.Initialize(playerStat, _multiplyValues1, Values[1]);
                    if (!initResult.IsSuccess) return initResult;
                }
                else
                {
                    return SkillResult.Failure("Player stat not found for OverDriveSkill");
                }
            }

            // Effect 4: MultiplyStatEffect (두 번째 스탯 강화)
            var stat2Result = GetEffect(4);
            if (stat2Result.IsSuccess)
            {
                _multiplyValues2[5] = Values[2]; // 이동속도 강화
                var playerStat = GameManager.Instance?.Player?.stat;
                if (playerStat != null)
                {
                    var initResult = stat2Result.Value.Initialize(playerStat, _multiplyValues2, Values[3]);
                    if (!initResult.IsSuccess) return initResult;
                }
                else
                {
                    return SkillResult.Failure("Player stat not found for OverDriveSkill");
                }
            }

            return SkillResult.Success("OverDriveSkill initialized successfully");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error initializing OverDriveSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var baseResult = base.ApplySkill(source);
        if (!baseResult.IsSuccess)
            return baseResult;

        // 자동으로 초기화
        if (!IsInitialized)
        {
            var initResult = Initialize();
            if (!initResult.IsSuccess)
                return initResult;
        }

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            _stateMachine.TransitionTo(SkillState.Completed);
            return SkillResult.Success("OverDriveSkill completed");
        }

        try
        {
            var effectResult = GetEffect(_currentEffectIndex);
            if (!effectResult.IsSuccess)
            {
                return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
            }

            var success = effectResult.Value.Apply(source, null);

            if (success)
            {
                Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
                _currentEffectIndex++;
            }

            return SkillResult.Success("OverDriveSkill effect applied");
        }
        catch (Exception ex)
        {
            _stateMachine.TransitionTo(SkillState.Failed);
            return SkillResult.Failure($"Error applying OverDriveSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult Clear()
    {
        try
        {
            var baseResult = base.Clear();
            _currentEffectIndex = 0;
            return baseResult;
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error clearing OverDriveSkill: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// PowerFistSkill은 TargetSkill을 상속한 블리츠크랭크의 강화 공격 스킬 '강철 주먹'을 구현한 클래스입니다.
/// 대상에게 강화된 데미지를 주고 에어본 효과를 적용합니다.
/// </summary>
public class PowerFistSkill : TargetSkill
{
    public PowerFistSkill(SkillData skillData,
                         IInputService inputService = null,
                         ITriggerService triggerService = null,
                         ITargetingService targetingService = null,
                         ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        AddEffect(new ConsumeManaEffect());
        AddEffect(new CooldownEffect());
        AddEffect(new DamageEffect());
        AddEffect(new AirBonrneEffect());
    }

    public override SkillResult Initialize()
    {
        var baseResult = base.Initialize();
        if (!baseResult.IsSuccess) return baseResult;

        try
        {
            // Effect 1: ConsumeManaEffect
            var consumeManaResult = GetEffect(1);
            if (consumeManaResult.IsSuccess)
            {
                var initResult = consumeManaResult.Value.Initialize(Cost);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 2: CooldownEffect
            var cooldownResult = GetEffect(2);
            if (cooldownResult.IsSuccess)
            {
                var initResult = cooldownResult.Value.Initialize(Cooltime, SkillIndex);
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 3: DamageEffect
            var damageResult = GetEffect(3);
            if (damageResult.IsSuccess)
            {
                var initResult = damageResult.Value.Initialize(
                    Values[0], // base damage multiplier
                    Values[1], // damage multiplier
                    0f        // bonus damage
                );
                if (!initResult.IsSuccess) return initResult;
            }

            // Effect 4: AirBonrneEffect
            var airborneResult = GetEffect(4);
            if (airborneResult.IsSuccess)
            {
                var initResult = airborneResult.Value.Initialize(
                    Values[2] // airborne duration
                );
                if (!initResult.IsSuccess) return initResult;
            }

            return SkillResult.Success("PowerFistSkill initialized successfully");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error initializing PowerFistSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        var baseResult = base.ApplySkill(source);
        if (!baseResult.IsSuccess)
            return baseResult;

        if (_currentEffectIndex >= GetEffectCount())
        {
            IsRunning = false;
            _stateMachine.TransitionTo(SkillState.Completed);
            return SkillResult.Success("PowerFistSkill completed");
        }

        try
        {
            var effectResult = GetEffect(_currentEffectIndex);
            if (!effectResult.IsSuccess)
            {
                return SkillResult.Failure($"Failed to get effect at index {_currentEffectIndex}: {effectResult.Message}");
            }

            var success = effectResult.Value.Apply(source, source.target);

            if (success)
            {
                Debug.Log($"{_currentEffectIndex}th Effect successfully applied");
                _currentEffectIndex++;
            }

            return SkillResult.Success("PowerFistSkill effect applied");
        }
        catch (Exception ex)
        {
            _stateMachine.TransitionTo(SkillState.Failed);
            return SkillResult.Failure($"Error applying PowerFistSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult Clear()
    {
        try
        {
            var baseResult = base.Clear();
            _triggerService.DeactivateTrigger(Define.GameTrigger.SkillActivated);
            return baseResult;
        }
        catch (Exception ex)
        {
            return SkillResult.Failure($"Error clearing PowerFistSkill: {ex.Message}", ex);
        }
    }
}