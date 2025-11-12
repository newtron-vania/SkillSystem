using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Skill 클래스는 모든 스킬의 기본이 되는 추상 클래스입니다.
/// 스킬 이름, 코스트, 쿨타임, 스킬 타입, 수치 정보(values)와 함께
/// 이펙트 실행 순서와 상태를 관리합니다.
/// </summary>
public abstract class Skill : IDisposable
{
    public int SkillIndex { get; private set; }
    public string SkillName { get; private set; }
    public Define.SkillType SkillType { get; private set; }
    public int Cost { get; private set; }
    public int Cooltime { get; private set; }
    public List<float> Values { get; private set; }

    protected int _currentEffectIndex = 0;
    public bool IsRunning { get; protected set; } = true;

    // State management
    protected readonly SkillStateMachine _stateMachine = new SkillStateMachine();
    public SkillState CurrentState => _stateMachine.CurrentState;

    // Services for loose coupling
    protected readonly IInputService _inputService;
    protected readonly ITriggerService _triggerService;
    protected readonly ITargetingService _targetingService;
    protected readonly ISkillManagerService _skillManagerService;

    private readonly List<Effect> _effectList = new List<Effect>();
    private bool _disposed = false;

    public Skill(SkillData skillData,
                IInputService inputService = null,
                ITriggerService triggerService = null,
                ITargetingService targetingService = null,
                ISkillManagerService skillManagerService = null)
    {
        ValidateSkillData(skillData);

        SkillIndex = skillData.skillIndex;
        SkillName = skillData.skillName;
        SkillType = skillData.GetSkillType();
        Cost = skillData.cost;
        Cooltime = skillData.cooltime;
        Values = new List<float>(skillData.values); // Create defensive copy

        // Initialize services with defaults if not provided
        _inputService = inputService ?? new UnityInputService();
        _triggerService = triggerService ?? new UnityTriggerService();
        _targetingService = targetingService ?? new UnityTargetingService();
        _skillManagerService = skillManagerService ?? new UnitySkillManagerService();

        _stateMachine.OnStateChanged += OnStateChanged;
    }

    /// <summary>
    /// 스킬 데이터를 검증합니다.
    /// </summary>
    private void ValidateSkillData(SkillData skillData)
    {
        if (skillData == null)
            throw new ArgumentNullException(nameof(skillData), "SkillData cannot be null");

        if (string.IsNullOrEmpty(skillData.skillName))
            throw new ArgumentException("Skill name cannot be null or empty", nameof(skillData));

        if (skillData.values == null || skillData.values.Count == 0)
            throw new ArgumentException("Skill values cannot be null or empty", nameof(skillData));

        if (skillData.cost < 0)
            throw new ArgumentException("Skill cost cannot be negative", nameof(skillData));

        if (skillData.cooltime < 0)
            throw new ArgumentException("Skill cooltime cannot be negative", nameof(skillData));
    }

    /// <summary>
    /// 이펙트를 스킬에 등록합니다.
    /// </summary>
    protected SkillResult AddEffect(Effect effect)
    {
        if (effect == null)
            return SkillResult.Failure("Cannot add null effect");

        try
        {
            _effectList.Add(effect);
            return SkillResult.Success($"Effect added. Total effects: {_effectList.Count}");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("Failed to add effect", ex);
        }
    }

    /// <summary>
    /// 특정 순서의 이펙트를 반환합니다.
    /// </summary>
    protected SkillResult<Effect> GetEffect(int index)
    {
        if (index < 0 || index >= _effectList.Count)
            return SkillResult<Effect>.Failure($"Effect index {index} out of range [0, {_effectList.Count - 1}]");

        return SkillResult<Effect>.Success(_effectList[index]);
    }

    /// <summary>
    /// 현재 등록된 이펙트 개수를 반환합니다.
    /// </summary>
    protected int GetEffectCount() => _effectList.Count;

    /// <summary>
    /// 범위 값을 안전하게 가져옵니다.
    /// </summary>
    protected float GetRangeValue()
    {
        if (Values.Count <= SkillConstants.RANGE_VALUE_INDEX)
        {
            Debug.LogWarning($"Range value not found for skill {SkillName}, using default");
            return SkillConstants.MIN_RANGE;
        }

        var range = Values[SkillConstants.RANGE_VALUE_INDEX];
        return Mathf.Clamp(range, SkillConstants.MIN_RANGE, SkillConstants.MAX_RANGE);
    }

    /// <summary>
    /// 상태 변경 시 호출되는 가상 메서드입니다.
    /// </summary>
    protected virtual void OnStateChanged(SkillState oldState, SkillState newState)
    {
        Debug.Log($"Skill {SkillName} state changed: {oldState} -> {newState}");
    }

    /// <summary>
    /// 스킬의 효과들을 초기화합니다.
    /// </summary>
    public virtual SkillResult Initialize()
    {
        try
        {
            _currentEffectIndex = 0;
            _stateMachine.Reset();
            return SkillResult.Success("Skill initialized");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("Failed to initialize skill", ex);
        }
    }

    /// <summary>
    /// 스킬을 실행합니다.
    /// </summary>
    public abstract SkillResult ApplySkill(Actor source);

    /// <summary>
    /// 스킬을 정리하고 초기 상태로 되돌립니다.
    /// </summary>
    public virtual SkillResult Clear()
    {
        try
        {
            IsRunning = true;
            _currentEffectIndex = 0;
            _stateMachine.Reset();

            // Clear all effects
            foreach (var effect in _effectList)
            {
                effect?.Clear();
            }

            return SkillResult.Success("Skill cleared");
        }
        catch (Exception ex)
        {
            return SkillResult.Failure("Failed to clear skill", ex);
        }
    }

    /// <summary>
    /// 리소스를 해제합니다.
    /// </summary>
    public virtual void Dispose()
    {
        if (!_disposed)
        {
            Clear();
            _stateMachine.OnStateChanged -= OnStateChanged;
            _disposed = true;
        }
    }

    ~Skill()
    {
        Dispose();
    }
}

/// <summary>
/// 제네릭 스킬 결과 클래스입니다.
/// </summary>
public class SkillResult<T> : SkillResult
{
    public T Value { get; private set; }

    private SkillResult(bool success, T value, string message, Exception error = null)
        : base(success, message, error)
    {
        Value = value;
    }

    public static SkillResult<T> Success(T value, string message = "") => new SkillResult<T>(true, value, message);
    public static new SkillResult<T> Failure(string message) => new SkillResult<T>(false, default(T), message);
    public static new SkillResult<T> Failure(string message, Exception error) => new SkillResult<T>(false, default(T), message, error);

    protected SkillResult(bool success, string message, Exception error = null) : base(success, message, error) { }
}


/// <summary>
/// NoneTargetSkill은 비대상형 스킬을 위한 클래스입니다.
/// 사용 시 사거리 원을 먼저 표시하고, 마우스 클릭을 통해 대상 위치를 지정합니다.
/// 이후 지정된 위치로 투사체 발사 등의 스킬이 적용됩니다.
/// </summary>
public class NoneTargetSkill : Skill
{
    private Action _inputAction;
    protected Vector3 TargetPosition { get; private set; }

    public NoneTargetSkill(SkillData skillData,
                          IInputService inputService = null,
                          ITriggerService triggerService = null,
                          ITargetingService targetingService = null,
                          ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        var addResult = AddEffect(new NoneTargetRangeViewEffect());
        if (!addResult.IsSuccess)
        {
            Debug.LogError($"Failed to add range view effect: {addResult.Message}");
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        if (source == null)
            return SkillResult.Failure("Source actor cannot be null");

        // 이미 다른 스킬이 발동 중이라면 중단
        if (_triggerService.IsTriggerActive(GameTrigger.SkillActivated))
            return SkillResult.Failure("Another skill is already active");

        try
        {
            // 처음 사용할 경우: 사거리 표시 및 입력 바인딩
            if (_stateMachine.IsInState(SkillState.Inactive))
            {
                var transitionResult = _stateMachine.TransitionTo(SkillState.ShowingRange);
                if (!transitionResult.IsSuccess)
                    return transitionResult;

                var effectResult = GetEffect(SkillConstants.RANGE_VIEW_EFFECT_INDEX);
                if (!effectResult.IsSuccess)
                    return SkillResult.Failure(effectResult.Message);

                effectResult.Value.Initialize(GetRangeValue());

                _skillManagerService.RegisterSkill(this, source);
                _inputAction = () => HandleClickInput(source);
                _triggerService.ActivateTrigger(GameTrigger.SkillActivated);
                _inputService.AddMouseBinding(SkillConstants.LEFT_MOUSE_BUTTON, _inputAction);

                _stateMachine.TransitionTo(SkillState.WaitingForInput);
            }

            // 사거리 시각화 적용
            if (_stateMachine.IsInState(SkillState.WaitingForInput))
            {
                var effectResult = GetEffect(SkillConstants.RANGE_VIEW_EFFECT_INDEX);
                if (effectResult.IsSuccess)
                {
                    effectResult.Value.Apply(source, null);
                }
            }

            return SkillResult.Success("NoneTargetSkill applying");
        }
        catch (Exception ex)
        {
            _stateMachine.TransitionTo(SkillState.Failed);
            return SkillResult.Failure($"Error applying NoneTargetSkill: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 마우스 클릭 시 실행되는 콜백 함수입니다.
    /// </summary>
    private void HandleClickInput(Actor source)
    {
        try
        {
            var ray = _inputService.GetMouseRay();
            var targetResult = _targetingService.TryGetGroundPosition(ray, out Vector3 position);

            if (targetResult.IsSuccess)
            {
                TargetPosition = position;
                source.targetPosition = TargetPosition;

                // 사거리 이펙트 제거 및 상태 전환
                CleanupRangeView();
                _stateMachine.TransitionTo(SkillState.Executing);
                _currentEffectIndex = SkillConstants.FIRST_SKILL_EFFECT_INDEX;

                Debug.Log($"{source.championName} 사용: {SkillName} at {TargetPosition}");
            }
            else
            {
                Debug.LogWarning($"Failed to get target position: {targetResult.Message}");
                _stateMachine.TransitionTo(SkillState.Failed);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling click input: {ex.Message}");
            _stateMachine.TransitionTo(SkillState.Failed);
        }
    }

    /// <summary>
    /// 사거리 표시 이펙트를 정리합니다.
    /// </summary>
    private void CleanupRangeView()
    {
        var effectResult = GetEffect(SkillConstants.RANGE_VIEW_EFFECT_INDEX);
        if (effectResult.IsSuccess)
        {
            effectResult.Value.Clear();
        }

        if (_inputAction != null)
        {
            _inputService.RemoveMouseBinding(SkillConstants.LEFT_MOUSE_BUTTON, _inputAction);
            _inputAction = null;
        }
    }

    public override SkillResult Clear()
    {
        CleanupRangeView();
        return base.Clear();
    }

    public override void Dispose()
    {
        CleanupRangeView();
        base.Dispose();
    }
}

/// <summary>
/// InstantSkill은 즉시 발동되는 스킬을 위한 클래스입니다.
/// 입력 없이 즉시 효과를 적용하며, SkillManager에 등록 후 외부에서 이펙트를 순차 실행합니다.
/// </summary>
public class InstantSkill : Skill
{
    private bool _isRegistered = false;

    public InstantSkill(SkillData skillData,
                       IInputService inputService = null,
                       ITriggerService triggerService = null,
                       ITargetingService targetingService = null,
                       ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
    }

    public override SkillResult ApplySkill(Actor source)
    {
        if (source == null)
            return SkillResult.Failure("Source actor cannot be null");

        try
        {
            if (!_isRegistered)
            {
                var transitionResult = _stateMachine.TransitionTo(SkillState.Executing);
                if (!transitionResult.IsSuccess)
                    return transitionResult;

                _isRegistered = true;
                _skillManagerService.RegisterSkill(this, source);
                Debug.Log($"{source.championName} 사용: {SkillName}");
            }

            return SkillResult.Success("InstantSkill applied successfully");
        }
        catch (Exception ex)
        {
            _stateMachine.TransitionTo(SkillState.Failed);
            return SkillResult.Failure($"Error applying InstantSkill: {ex.Message}", ex);
        }
    }

    public override SkillResult Clear()
    {
        _isRegistered = false;
        return base.Clear();
    }
}

/// <summary>
/// TargetSkill은 대상 지정형 스킬을 위한 클래스입니다.
/// 클릭을 통해 적 Actor를 지정하고, 사거리 내일 경우 해당 대상을 타겟으로 지정합니다.
/// </summary>
public class TargetSkill : Skill
{
    private Actor _target;
    private Action _inputAction;

    public TargetSkill(SkillData skillData,
                      IInputService inputService = null,
                      ITriggerService triggerService = null,
                      ITargetingService targetingService = null,
                      ISkillManagerService skillManagerService = null)
        : base(skillData, inputService, triggerService, targetingService, skillManagerService)
    {
        var addResult = AddEffect(new TargetRangeViewEffect());
        if (!addResult.IsSuccess)
        {
            Debug.LogError($"Failed to add target range view effect: {addResult.Message}");
        }
    }

    public override SkillResult ApplySkill(Actor source)
    {
        if (source == null)
            return SkillResult.Failure("Source actor cannot be null");

        if (_triggerService.IsTriggerActive(GameTrigger.SkillActivated))
            return SkillResult.Failure("Another skill is already active");

        try
        {
            if (_stateMachine.IsInState(SkillState.Inactive))
            {
                var transitionResult = _stateMachine.TransitionTo(SkillState.ShowingRange);
                if (!transitionResult.IsSuccess)
                    return transitionResult;

                var effectResult = GetEffect(SkillConstants.RANGE_VIEW_EFFECT_INDEX);
                if (!effectResult.IsSuccess)
                    return SkillResult.Failure(effectResult.Message);

                effectResult.Value.Initialize(GetRangeValue());

                _skillManagerService.RegisterSkill(this, source);
                _inputAction = () => HandleClickInput(source);
                _inputService.AddMouseBinding(SkillConstants.LEFT_MOUSE_BUTTON, _inputAction);

                _stateMachine.TransitionTo(SkillState.WaitingForInput);
            }

            // 사거리 시각화 적용
            if (_stateMachine.IsInState(SkillState.WaitingForInput))
            {
                var effectResult = GetEffect(SkillConstants.RANGE_VIEW_EFFECT_INDEX);
                if (effectResult.IsSuccess)
                {
                    effectResult.Value.Apply(source, null);
                }
            }

            return SkillResult.Success("TargetSkill applying");
        }
        catch (Exception ex)
        {
            _stateMachine.TransitionTo(SkillState.Failed);
            return SkillResult.Failure($"Error applying TargetSkill: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 클릭 시 실행되는 입력 함수로, 지정된 Actor가 사거리 내에 있으면 타겟으로 설정합니다.
    /// </summary>
    private void HandleClickInput(Actor source)
    {
        try
        {
            var ray = _inputService.GetMouseRay();
            var targetResult = _targetingService.TryGetActorTarget(ray, source.transform.position, GetRangeValue(), out Actor target);

            if (targetResult.IsSuccess)
            {
                _target = target;
                source.target = _target;

                // 사거리 이펙트 제거 및 상태 전환
                CleanupRangeView();
                _stateMachine.TransitionTo(SkillState.Executing);
                _currentEffectIndex = SkillConstants.FIRST_SKILL_EFFECT_INDEX;
                _triggerService.ActivateTrigger(GameTrigger.SkillActivated);

                Debug.Log($"{source.championName} 사용: {SkillName} on {_target.championName}");
            }
            else
            {
                Debug.LogWarning($"Failed to get target: {targetResult.Message}");
                _stateMachine.TransitionTo(SkillState.Failed);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error handling click input: {ex.Message}");
            _stateMachine.TransitionTo(SkillState.Failed);
        }
    }

    /// <summary>
    /// 사거리 표시 이펙트를 정리합니다.
    /// </summary>
    private void CleanupRangeView()
    {
        var effectResult = GetEffect(SkillConstants.RANGE_VIEW_EFFECT_INDEX);
        if (effectResult.IsSuccess)
        {
            effectResult.Value.Clear();
        }

        if (_inputAction != null)
        {
            _inputService.RemoveMouseBinding(SkillConstants.LEFT_MOUSE_BUTTON, _inputAction);
            _inputAction = null;
        }
    }

    public override SkillResult Clear()
    {
        CleanupRangeView();
        _target = null;
        return base.Clear();
    }

    public override void Dispose()
    {
        CleanupRangeView();
        base.Dispose();
    }
}