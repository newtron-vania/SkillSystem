using System;
using UnityEngine;

/// <summary>
/// 스킬의 상태를 정의하는 열거형입니다.
/// </summary>
public enum SkillState
{
    Inactive,       // 스킬이 비활성화 상태
    ShowingRange,   // 사거리 표시 중
    WaitingForInput, // 입력 대기 중
    Executing,      // 스킬 실행 중
    Completed,      // 스킬 완료
    Failed          // 스킬 실패
}

/// <summary>
/// 스킬 결과를 나타내는 클래스입니다.
/// 성공/실패와 함께 메시지를 포함합니다.
/// </summary>
public class SkillResult
{
    public bool IsSuccess { get; private set; }
    public string Message { get; private set; }
    public Exception Error { get; private set; }

    protected SkillResult(bool success, string message, Exception error = null)
    {
        IsSuccess = success;
        Message = message;
        Error = error;
    }

    public static SkillResult Success(string message = "") => new SkillResult(true, message);
    public static SkillResult Failure(string message) => new SkillResult(false, message);
    public static SkillResult Failure(string message, Exception error) => new SkillResult(false, message, error);
}

/// <summary>
/// 스킬의 상태 관리를 담당하는 상태 머신 클래스입니다.
/// </summary>
public class SkillStateMachine
{
    public SkillState CurrentState { get; private set; } = SkillState.Inactive;

    public event Action<SkillState, SkillState> OnStateChanged;

    /// <summary>
    /// 상태를 전환합니다.
    /// </summary>
    public SkillResult TransitionTo(SkillState newState)
    {
        if (!IsValidTransition(CurrentState, newState))
        {
            return SkillResult.Failure($"Invalid state transition from {CurrentState} to {newState}");
        }

        var oldState = CurrentState;
        CurrentState = newState;
        OnStateChanged?.Invoke(oldState, newState);

        return SkillResult.Success($"State changed from {oldState} to {newState}");
    }

    /// <summary>
    /// 상태 전환이 유효한지 검사합니다.
    /// </summary>
    private bool IsValidTransition(SkillState from, SkillState to)
    {
        return (from, to) switch
        {
            (SkillState.Inactive, SkillState.ShowingRange) => true,
            (SkillState.Inactive, SkillState.Executing) => true, // InstantSkill
            (SkillState.ShowingRange, SkillState.WaitingForInput) => true,
            (SkillState.ShowingRange, SkillState.Failed) => true,
            (SkillState.WaitingForInput, SkillState.Executing) => true,
            (SkillState.WaitingForInput, SkillState.Failed) => true,
            (SkillState.Executing, SkillState.Completed) => true,
            (SkillState.Executing, SkillState.Failed) => true,
            (SkillState.Completed, SkillState.Inactive) => true,
            (SkillState.Failed, SkillState.Inactive) => true,
            _ => false
        };
    }

    /// <summary>
    /// 상태 머신을 초기 상태로 재설정합니다.
    /// </summary>
    public void Reset()
    {
        var oldState = CurrentState;
        CurrentState = SkillState.Inactive;
        OnStateChanged?.Invoke(oldState, SkillState.Inactive);
    }

    /// <summary>
    /// 현재 상태가 지정된 상태인지 확인합니다.
    /// </summary>
    public bool IsInState(SkillState state) => CurrentState == state;

    /// <summary>
    /// 스킬이 활성화 상태인지 확인합니다.
    /// </summary>
    public bool IsActive => CurrentState != SkillState.Inactive && CurrentState != SkillState.Completed && CurrentState != SkillState.Failed;
}